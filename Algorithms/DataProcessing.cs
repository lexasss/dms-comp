using DmsComparison.Common;

namespace DmsComparison.Algorithms;

public enum NormalizationType
{
    None,
    Linear,
    MinMax,
    RawWise,
}

internal static class DataProcessing
{
    public static float RectificationMedianFactor = 3f;
    public static float CropThreshold = 1.5f;

    public static void Normalize(float[] data, Size size, NormalizationType type)
    {
        if (type == NormalizationType.None)
        {
            // do nothing
        }
        else if (type == NormalizationType.Linear)
        {
            var max = data.Max();
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = data[i] / max;
            }
        }
        else if (type == NormalizationType.MinMax)
        {
            var min = data.Min();
            var max = data.Max();
            var range = max - min;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (data[i] - min) / range;
            }
        }
        else if (type == NormalizationType.RawWise)
        {
            for (int row = 0; row < size.Height; row++)
            {
                var rowData = data.AsSpan(row * size.Width, size.Width);
                var (mean, std) = rowData.MeanAndStandardDeviation();

                for (int i = 0; i < size.Width; i++)
                {
                    rowData[i] = (rowData[i] - mean) / std;
                }
            }
        }
        else
        {
            throw new NotImplementedException($"Normalization of type '{type}' is not implemented");
        }
    }

    public static float[] Rectify(float[] data)
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
        var medianDeviation = RectificationMedianFactor * t.Median();
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

    public static float[] Crop(float[] data, Size size)
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
