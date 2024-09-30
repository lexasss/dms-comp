namespace DmsComparison.Algorithms;

internal class Pearson : Algorithm
{
    // https://pypi.org/project/distance-metrics-mcda/ (as Correlation distance)

    public override string Name => "Pearson";

    protected override double ComputeDistance(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2)
    {
        double numerator = 0;
        double den1 = 0;
        double den2 = 0;

        if (_isDataRectified)
        {
            double sum1 = 0;
            double sum2 = 0;
            int count1 = 0;
            int count2 = 0;

            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] != 0)
                {
                    sum1 += data1[i];
                    count1 += 1;
                }
                if (data2[i] != 0)
                {
                    sum2 += data2[i];
                    count2 += 1;
                }
            }


            var mean1 = sum1 / (count1 > 0 ? count1 : 1);
            var mean2 = sum2 / (count2 > 0 ? count2 : 1);

            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] != 0 || data2[i] != 0)
                {
                    var diff1 = data1[i] - mean1;
                    var diff2 = data2[i] - mean2;

                    numerator += diff1 * diff2;
                    den1 += diff1 * diff1;
                    den2 += diff2 * diff2;
                }
            }
        }
        else
        {
            double sum1 = 0;
            double sum2 = 0;
            int count1 = 0;
            int count2 = 0;

            for (int i = 0; i < data1.Length; i++)
            {
                sum1 += data1[i];
                count1 += 1;
                sum2 += data2[i];
                count2 += 1;
            }


            var mean1 = sum1 / (count1 > 0 ? count1 : 1);
            var mean2 = sum2 / (count2 > 0 ? count2 : 1);

            for (int i = 0; i < data1.Length; i++)
            {
                var diff1 = data1[i] - mean1;
                var diff2 = data2[i] - mean2;

                numerator += diff1 * diff2;
                den1 += diff1 * diff1;
                den2 += diff2 * diff2;
            }
        }

        var denominator = Math.Sqrt(den1) * Math.Sqrt(den2);
        return 1.0 - numerator / (denominator > 0 ? denominator : 1);
    }
}
