namespace DmsComparison.Common;

public static class FloatArrayExt
{
    public static float Median(this Array data)
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

    public static float Mean(this Span<float> values, Func<float, float>? proc = null)
    {
        if (values.Length == 0)
            return 0;

        float sum = 0;
        for (int i = 0; i < values.Length; i++)
        {
            sum += proc == null ? values[i] : proc(values[i]);
        }

        return sum / values.Length;
    }
    public static float StandardDeviation(this Span<float> values)
    {
        double avg = values.Mean();
        return (float)Math.Sqrt(values.Mean(v => (float)Math.Pow(v - avg, 2)));
    }

    public static (float, float) MeanAndStandardDeviation(this Span<float> values)
    {
        double avg = values.Mean();
        return ((float)avg, (float)Math.Sqrt(values.Mean(v => (float)Math.Pow(v - avg, 2))));
    }
}
