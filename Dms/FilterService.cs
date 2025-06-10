using LinqStatistics;

namespace DmsComparison.Data;

public enum DataLimitType
{
    Absolute,
    RelativeToMinimum,
    RelativeToMedian,
    Percentage
}

public record class FilterSettings(float? From, float? To, DataLimitType LimitType = DataLimitType.Absolute)
{
    public static FilterSettings Default => new(null, null, DataLimitType.Absolute);
}

internal static class FilterService
{
    public static float[] ApplyFilter(float[] data, float? from, float? to, DataLimitType limitType)
    {
        if (from == null && to == null)
            return data;

        var result = new float[data.Length];

        var range = data.MinMax();

        float low = from ?? 0;
        float high = to ?? range.Max;

        if (limitType == DataLimitType.RelativeToMinimum)
        {
            low = range.Min + (from ?? 0);
            high = range.Min + (to ?? (range.Max - range.Min));
        }
        else if (limitType == DataLimitType.RelativeToMedian)
        {
            float median = data.Median();
            low = median + (from ?? 0);
            high = median + (to ?? (range.Max - median));
        }
        else if (limitType == DataLimitType.Percentage)
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
