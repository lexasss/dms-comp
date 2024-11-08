using DmsComparison.Common;

namespace DmsComparison.Algorithms;

public record Size(int Width, int Height);

public class Options(bool useRectification, Normalization.Type normalizationType, bool cropArea)
{
    public bool UseRectification => useRectification;
    public Normalization.Type NormalizationType => normalizationType;
    public bool Crop => cropArea;
    public override string ToString()
    {
        string rect = UseRectification ? "R" : "-";
        string norm = NormalizationType == Normalization.Type.None ? "-" : NormalizationType.ToString()[0].ToString();
        string crop = Crop ? "C" : "-";
        return $"{rect},{norm},{crop}";
    }
}

public abstract class Algorithm
{
    public abstract string Name { get; }

    public virtual bool IsVisible => true;

    /// <summary>
    /// Returns types of all algorithms derived from "Algorithm"
    /// </summary>
    /// <returns>List of types</returns>
    public static Type[] GetDescendantTypes() => System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
        .Where(type => type.IsSubclassOf(typeof(Algorithm))).ToArray();

    public double ComputeDistance(float[] data1, float[] data2, Size size, Options options)
    {
        data1 = (float[])data1.Clone();
        data2 = (float[])data2.Clone();

        if (options.Crop)
        {
            data1 = Crop(data1, size);
            data2 = Crop(data2, size);
        }

        if (options.UseRectification)
        {
            data1 = Rectify(data1);
            data2 = Rectify(data2);
            _isDataRectified = options.UseRectification;
        }

        Normalization.Process(data1, options.NormalizationType);
        Normalization.Process(data2, options.NormalizationType);

        return Math.Abs(ComputeDistance(data1, data2));
    }

    // Internal

    protected const float RectifiedMedianFactor = 3f;
    protected const float CropThreshold = 1.5f;

    protected bool _isDataRectified { get; private set; } = false;

    protected abstract double ComputeDistance(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2);

    private static float[] Rectify(float[] data)
    {
        var median = data.Median();
        //System.Diagnostics.Debug.WriteLine($"[DIST] Median: {median:F6}");

        // Remove median
        float[] result = (float[])data.Clone();
        for (int i = 0; i < result.Length; i++)
        {
            result[i] -= median;
        }

        // Find absolute median deviation
        float[] t = (float[])result.Clone();
        for (int i = 0; i < t.Length; i++)
        {
            t[i] = Math.Abs(t[i]);
        }
        var medianDeviation = RectifiedMedianFactor * t.Median();
        //System.Diagnostics.Debug.WriteLine($"[DIST] Median deviation: {medianDeviation:F6}");

        // Subtract median deviation: anything within the median deviation range becomes 0
        for (int i = 0; i < result.Length; i++)
        {
            if (result[i] > medianDeviation)
                result[i] -= medianDeviation;
            else if (result[i] < -medianDeviation)
                result[i] += medianDeviation;
            else
                result[i] = 0;
        }

        return result;
    }

    private static float[] Crop(float[] data, Size size)
    {
        /*
        var median = GetMedian(data);

        int[] rowRange = [0, size.Height - 1];
        int rowRangeIndex = 0;
        bool[] colHasAboveThreshold = new bool[size.Width];

        for (int row = 0; row < size.Height; row++)
        {
            bool hasLargeOffsets = false;
            for (int col = 0; col < size.Width; col++)
            {
                var isAboveThreshold = Math.Abs(data[row * size.Width + col] - median) > CropThreshold;

                if (isAboveThreshold)
                {
                    colHasAboveThreshold[col] = true;
                    hasLargeOffsets = true;

                    if (rowRangeIndex == 0)
                    {
                        rowRange[0] = row;
                        rowRangeIndex = 1;
                        break;
                    }
                }
            }

            if (rowRangeIndex == 1 && !hasLargeOffsets)
            {
                rowRange[1] = row - 1;
            }
        }

        int[] colRange = [0, size.Width - 1];
        for (int i = 0; i < size.Width - 1; i++)
        {
            if (!colHasAboveThreshold[i] && colHasAboveThreshold[i + 1])
                colRange[0] = i + 1;
            if (colHasAboveThreshold[i] && !colHasAboveThreshold[i + 1])
                colRange[1] = i;
        }
        */

        int[] rowRange = [0, Math.Min(59, size.Height - 1)];
        int[] colRange = [
            Math.Min(10, size.Width - 2),
            Math.Min(79, size.Width - 1)
        ];

        int width = colRange[1] + 1 - colRange[0];
        int height = rowRange[1] + 1 - rowRange[0];
        var result = new float[width * height];

        for (int i = 0, row = rowRange[0]; row <= rowRange[1]; row++, i++)
            for (int j = 0, col = colRange[0]; col <= colRange[1]; col++, j++)
                result[i * width + j] = data[row * size.Width + col];

        return result;
    }
}
