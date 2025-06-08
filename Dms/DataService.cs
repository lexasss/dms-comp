namespace DmsComparison;

public enum DataSource
{
    Positive,
    Negative,
}

public enum DataType
{
    Raw,
    Gradient
}

public record class DataArray(int Rows, int Columns, float[] Values);

public class DataService
{
    public static bool IsSameShape(Dms dms1, Dms dms2) => dms1.Height == dms2.Height && dms1.Width == dms2.Width;
    
    public static DataArray GetRaw(Dms dms, DataSource source = DataSource.Positive)
    {
        var raw = source switch
        {
            DataSource.Positive => dms.Scan.MeasurementData.IntensityTop,
            DataSource.Negative => dms.Scan.MeasurementData.IntensityBottom.Select(v => -v).ToArray(),
            _ => throw new NotSupportedException($"""Data source "{source}" is not supported.""")
        };
        return new DataArray(dms.Height, dms.Width, raw);
    }

    public static DataArray? GetDifference(Dms? dms1, Dms? dms2, DataType type, DataSource source = DataSource.Positive)
    {
        if (dms1 == null || dms2 == null || !IsSameShape(dms1, dms2))
            return null;

        var m1 = dms1.Scan.MeasurementData;
        var m2 = dms2.Scan.MeasurementData;
        
        var (raw1, raw2) = source switch
        {
            DataSource.Positive => (m1.IntensityTop, m2.IntensityTop),
            DataSource.Negative => (m1.IntensityBottom, m2.IntensityBottom),
            _ => throw new NotSupportedException($"""Data source "{source}" is not supported.""")
        };

        return type switch
        {
            DataType.Raw => GetDifference(raw1, raw2, dms1.Height, dms1.Width),
            DataType.Gradient => GetDifference(
                GetGradient(raw1, dms1.Height, dms1.Width),
                GetGradient(raw2, dms2.Height, dms2.Width)
            ),
            _ => throw new NotSupportedException($"""Data type "{type}" is not supported.""")
        };
    }

    // Internal

    record class DataGradient(int Rows, int Columns, System.Windows.Vector[] Values);

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
}
