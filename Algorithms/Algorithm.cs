using MathNet.Numerics.Statistics;

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

    public double ComputeDistance(float[] data1, float[] data2, bool shouldRectify)
    {
        data1 = (float[])data1.Clone();
        data2 = (float[])data2.Clone();

        _isDataRectified = shouldRectify;

        if (shouldRectify)
        {
            data1 = Rectify(data1);
            data2 = Rectify(data2);
        }

        return ComputeDistance(data1, data2);
    }

    // Internal

    protected const float RectifiedMedianFactor = 3f;

    protected bool _isDataRectified { get; private set; } = false;

    protected abstract double ComputeDistance(float[] data1, float[] data2);

    private static float[] Rectify(float[] data)
    {
        var median = data.Median();
        System.Diagnostics.Debug.WriteLine($"[DIST] Median: {median:F3}");

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
        System.Diagnostics.Debug.WriteLine($"[DIST] Median deviation: {medianDeviation:F3}");

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
}
