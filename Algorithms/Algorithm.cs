namespace DmsComparison.Algorithms;

public abstract class Algorithm
{
    public abstract string Name { get; }

    /// <summary>
    /// Returns types of all algorithms derived from "Algorithm"
    /// </summary>
    /// <returns>List of types</returns>
    public static Type[] GetDescendantTypes() => System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
        .Where(type => type.IsSubclassOf(typeof(Algorithm))).ToArray();

    public double ComputeDistance(float[] data1, float[] data2, bool shouldRectify,
        Normalization.Type normalizationType = Normalization.Type.None)
    {
        data1 = (float[])data1.Clone();
        data2 = (float[])data2.Clone();

        _isDataRectified = shouldRectify;

        if (shouldRectify)
        {
            data1 = Rectify(data1);
            data2 = Rectify(data2);
        }

        Normalization.Process(data1, normalizationType);
        Normalization.Process(data2, normalizationType);

        return Math.Abs(ComputeDistance(data1, data2));
    }

    // Internal

    protected const float RectifiedMedianFactor = 3f;

    protected bool _isDataRectified { get; private set; } = false;

    protected abstract double ComputeDistance(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2);

    private static float[] Rectify(float[] data)
    {
        var median = GetMedian(data);
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
        var medianDeviation = RectifiedMedianFactor * GetMedian(t);
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

    private static float GetMedian(float[] data)
    {
        if (data == null || data.Length == 0)
            return 0;

        float[] sorted = (float[])data.Clone();
        Array.Sort(sorted);

        int size = sorted.Length;
        int mid = size / 2;
        float median = (size % 2 != 0) ? sorted[mid] : (sorted[mid] + sorted[mid - 1]) / 2;
        return median;
    }
}
