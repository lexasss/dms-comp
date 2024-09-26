namespace DmsComparison.Algorithms;

internal class DTW : Algorithm
{
    // https://en.wikipedia.org/wiki/Dynamic_time_warping

    public override string Name => "DTW";

    public const int WindowSize = 10;

    protected override double ComputeDistance(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2)
    {
        float[,] result = new float[data1.Length + 1, data2.Length + 1];

        for (int i = 0; i <= data1.Length; i++)
            for (int j = 0; j <= data2.Length; j++)
                result[i, j] = float.MaxValue;

        result[0, 0] = 0;
        for (int i = 1; i <= data1.Length; i++)
            for (int j = Math.Max(1, i - WindowSize); j <= Math.Min(data2.Length, i + WindowSize); j++)
                result[i, j] = 0;

        float cost;
        for (int i = 1; i <= data1.Length; i++)
            for (int j = Math.Max(1, i - WindowSize); j <= Math.Min(data2.Length, i + WindowSize); j++)
            {
                cost = Math.Abs(data1[i - 1] - data2[j - 1]);
                result[i, j] = cost + Math.Min(result[i - 1, j],        // insertion
                                        Math.Min(result[i, j - 1],      // deletion
                                                 result[i - 1, j - 1]));// match
            }


        return result[data1.Length, data2.Length] / ((data1.Length + data2.Length + 2) / 2);
    }
}
