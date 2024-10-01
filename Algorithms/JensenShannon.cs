namespace DmsComparison.Algorithms;

internal class JensenShannon : Algorithm
{
    // https://github.com/scipy/scipy/blob/v1.14.1/scipy/spatial/distance.py#L1186

    public override string Name => "Jensen-Shannon";

    public override bool IsVisible => false;

    protected override double ComputeDistance(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2)
    {
        double sum = 0;
        int count = 0;

        double sum1 = data1.ToArray().Sum();
        double sum2 = data2.ToArray().Sum();

        if (_isDataRectified)
        {
            for (int i = 0; i < data1.Length; i++)
            {
                var a = data1[i];
                var b = data2[i];
                if (a != 0 || b != 0)
                {
                    var v1 = a / sum1;
                    var v2 = b / sum2;
                    var mean = (v1 + v2) / 2;

                    var left = (v1 > 0 && mean > 0) ? v1 * Math.Log(v1 / mean) : 0;
                    var right = (v2 > 0 && mean > 0) ? v2 * Math.Log(v2 / mean) : 0;

                    sum += left + right;
                    count += 1;
                }
            }
        }
        else
        {
            for (int i = 0; i < data1.Length; i++)
            {
                var a = data1[i];
                var b = data2[i];

                var v1 = a / sum1;
                var v2 = b / sum2;
                var mean = (v1 + v2) / 2;

                var left = (v1 > 0 && mean > 0) ? v1 * Math.Log(v1 / mean) : 0;
                var right = (v2 > 0 && mean > 0) ? v2 * Math.Log(v2 / mean) : 0;

                sum += left + right;
                count += 1;
            }
        }

        return Math.Sqrt(sum) / (count > 0 ? count : 1);
    }
}
