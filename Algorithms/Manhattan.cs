namespace DmsComparison.Algorithms;

internal class Manhattan : Algorithm
{
    // https://pypi.org/project/distance-metrics-mcda/

    public override string Name => "Manhattan";

    protected override double ComputeDistance(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2)
    {
        double sum = 0;
        int count = 0;

        if (_isDataRectified)
        {
            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] != 0 || data2[i] != 0)
                {
                    sum += Math.Abs(data1[i] - data2[i]);
                    count += 1;
                }
            }
        }
        else
        {
            count = data1.Length;
            for (int i = 0; i < data1.Length; i++)
            {
                sum += Math.Abs(data1[i] - data2[i]);
            }
        }

        return Math.Sqrt(sum / (count > 0 ? count : 1));
    }
}
