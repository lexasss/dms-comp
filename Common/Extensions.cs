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
}
