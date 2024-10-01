namespace DmsComparison.Algorithms;

internal class KullbackLeibler : Algorithm
{
    // https://datascience.stackexchange.com/questions/9262/calculating-kl-divergence-in-python

    public override string Name => "Kullback-Leibler";

    public override bool IsVisible => false;

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
                    sum += (data1[i] > 0 && data2[i] > 0) ? data1[i] * Math.Log(data1[i] / data2[i]) : 0;
                    count += 1;
                }
            }
        }
        else
        {
            for (int i = 0; i < data1.Length; i++)
            {
                sum += (data1[i] > 0 && data2[i] > 0) ? data1[i] * Math.Log(data1[i] / data2[i]) : 0;
                count += 1;
            }
        }

        return sum / (count > 0 ? count : 1);
    }
}
