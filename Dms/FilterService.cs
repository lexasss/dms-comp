using LinqStatistics;

namespace DmsComparison;

internal static class FilterService
{
    public static float[] ApplyFilter(float[] data, float? from, float? to, Data.Limits limits)
    {
        if (from == null && to == null)
            return data;

        var result = new float[data.Length];

        var range = data.MinMax();

        float low = from ?? 0;
        float high = to ?? range.Max;

        if (limits == Data.Limits.RelativeToMinimum)
        {
            low = range.Min + (from ?? 0);
            high = range.Min + (to ?? (range.Max - range.Min));
        }
        else if (limits == Data.Limits.RelativeToMedian)
        {
            float median = data.Median();
            low = median + (from ?? 0);
            high = median + (to ?? (range.Max - median));
        }
        else if (limits == Data.Limits.Percentage)
        {
            float interval = range.Max - range.Min;
            low = range.Min + (interval * (from ?? 0) / 100);
            high = range.Min + (interval * (to ?? 100) / 100);
        }

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

        return result;
    }
}
