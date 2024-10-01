namespace DmsComparison.Algorithms;

internal class Cosine : Algorithm
{
    // https://pypi.org/project/distance-metrics-mcda/

    public override string Name => "Cosine";

    protected override double ComputeDistance(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2)
    {
        double numerator = 0;
        double sum1 = 0;
        double sum2 = 0;

        if (_isDataRectified)
        {
            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] != 0 || data2[i] != 0)
                {
                    numerator += data1[i] * data2[i];
                    sum1 += data1[i] * data1[i];
                    sum2 += data2[i] * data2[i];
                }
            }
        }
        else
        {
            for (int i = 0; i < data1.Length; i++)
            {
                numerator += data1[i] * data2[i];
                sum1 += data1[i] * data1[i];
                sum2 += data2[i] * data2[i];
            }
        }

        var denominator = Math.Sqrt(sum1) * Math.Sqrt(sum2);
        return 10 * (1.0 - numerator / (denominator > 0 ? denominator : 1));
    }
}
