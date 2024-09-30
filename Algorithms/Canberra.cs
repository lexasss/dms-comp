namespace DmsComparison.Algorithms;

internal class Canberra : Algorithm
{
    // https://github.com/scipy/scipy/blob/v1.14.1/scipy/spatial/distance.py

    public override string Name => "Canberra";

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
                    var numerator = Math.Abs(data1[i] - data2[i]);
                    //var denominator = data1[i] + data2[i];    // https://pypi.org/project/distance-metrics-mcda/
                    var denominator = Math.Abs(data1[i]) + Math.Abs(data2[i]);      // scipy
                    sum += numerator / (denominator != 0 ? denominator : 1);
                    count += 1;
                }
            }
        }
        else
        {
            count = data1.Length;
            for (int i = 0; i < data1.Length; i++)
            {
                var numerator = Math.Abs(data1[i] - data2[i]);
                //var denominator = data1[i] + data2[i];    // https://pypi.org/project/distance-metrics-mcda/
                var denominator = Math.Abs(data1[i]) + Math.Abs(data2[i]);      // scipy
                sum += numerator / (denominator != 0 ? denominator : 1);
            }
        }

        return sum / (count > 0 ? count : 1);
    }
}
