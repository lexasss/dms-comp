namespace DmsComparison;

internal record class DataGradient(int Rows, int Columns, System.Windows.Vector[] Values);

public record class DataArray(int Rows, int Columns, float[] Values)
{
    internal static DataArray FromGradient(DataGradient gradient)
    {
        int size = gradient.Columns * gradient.Rows;
        var result = new float[size];
        for (int i = 0; i < size; i++)
        {
            var v = gradient.Values[i];
            result[i] = (float)(Math.Abs(v.X) + Math.Abs(v.Y)) / 2;
        }
        return new DataArray(gradient.Rows, gradient.Columns, result);
    }
}

public static class DataService
{
    public static bool IsSameShape(Dms dms1, Dms dms2) => dms1.Height == dms2.Height && dms1.Width == dms2.Width;
    
    public static DataArray GetRaw(Dms dms,
        Data.Type type = Data.Type.Raw,
        Data.Filter filter = Data.Filter.Unfiltered,
        Data.Source source = Data.Source.Positive)
    {
        var raw = source switch
        {
            Data.Source.Positive => dms.Scan.MeasurementData.IntensityTop,
            Data.Source.Negative => dms.Scan.MeasurementData.IntensityBottom.Select(v => -v).ToArray(),
            _ => throw new NotSupportedException($"""Data source "{source}" is not supported.""")
        };

        var filtered = ApplyFilter(raw, filter);

        return type switch
        {
            Data.Type.Raw => new DataArray(dms.Height, dms.Width, filtered),
            Data.Type.Gradient => DataArray.FromGradient(GetGradient(filtered, dms.Height, dms.Width)),
            _ => throw new NotSupportedException($"""Data type "{type}" is not supported.""")
        };
    }

    public static DataArray? GetDifference(Dms? dms1, Dms? dms2,
        Data.Type type,
        Data.Filter filter = Data.Filter.Unfiltered,
        Data.Source source = Data.Source.Positive)
    {
        if (dms1 == null || dms2 == null || !IsSameShape(dms1, dms2))
            return null;

        var m1 = dms1.Scan.MeasurementData;
        var m2 = dms2.Scan.MeasurementData;
        
        var (raw1, raw2) = source switch
        {
            Data.Source.Positive => (m1.IntensityTop, m2.IntensityTop),
            Data.Source.Negative => (m1.IntensityBottom, m2.IntensityBottom),
            _ => throw new NotSupportedException($"""Data source "{source}" is not supported.""")
        };

        var filtered1 = ApplyFilter(raw1, filter);
        var filtered2 = ApplyFilter(raw2, filter);

        return type switch
        {
            Data.Type.Raw => GetDifference(filtered1, filtered2, dms1.Height, dms1.Width),
            Data.Type.Gradient => GetDifference(
                GetGradient(filtered1, dms1.Height, dms1.Width),
                GetGradient(filtered2, dms2.Height, dms2.Width)
            ),
            _ => throw new NotSupportedException($"""Data type "{type}" is not supported.""")
        };
    }

    // Internal

    private static DataArray GetDifference(float[] array1, float[] array2, int rows, int columns)
    {
        var array = new float[array1.Length];
        for (int i = 0; i < array1.Length; i++)
        {
            array[i] = array2[i] - array1[i];
        }
        return new DataArray(rows, columns, array);
    }

    private static DataArray GetDifference(DataGradient gradient1, DataGradient gradient2)
    {
        var size = gradient1.Values.Length;
        var array = new float[size];
        for (int i = 0; i < size; i++)
        {
            var v1 = gradient1.Values[i];
            var v2 = gradient2.Values[i];
            var dx = v1.X - v2.X;
            var dy = v1.Y - v2.Y;
            array[i] = (float)Math.Sqrt(dx * dx + dy * dy);
        }

        return new DataArray(gradient1.Rows, gradient1.Columns, array);
    }

    private static DataGradient GetGradient(float[] dms, int rows, int columns)
    {
        int width = columns - 1;
        int height = rows - 1;
        var array = new System.Windows.Vector[width * height];

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                array[r * width + c] = new System.Windows.Vector(
                    dms[(r + 1) * columns + c + 1] - dms[r * columns + c],
                    dms[r * columns + c + 1] - dms[(r + 1) * columns + c]
                );
            }
        }

        return new DataGradient(height, width, array);
    }

    private static float[] ApplyFilter(float[] data, Data.Filter filter)
    {
        if (filter == Data.Filter.Unfiltered)
            return data;

        var result = new float[data.Length];

        if (filter == Data.Filter.LowValues)
        {
            float min = data.Min();
            float low = min + 1.0f;
            float high = min + 10.0f;

            for (int i = 0; i < data.Length; i++)
            {
                var value = data[i];
                if (value < low)
                    result[i] = low;
                else if (value > high)
                    result[i] = high;
                else 
                    result[i] = value;
            }
        }

        return result;
    }
}
