namespace DmsComparison.Algorithms;

internal class Canberra : Algorithm
{
    // https://pypi.org/project/distance-metrics-mcda/

    public override string Name => "Canberra";

    protected override double ComputeDistance(float[] data1, float[] data2)
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
                    var denominator = data1[i] + data2[i];
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
                var denominator = data1[i] + data2[i];
                sum += numerator / (denominator != 0 ? denominator : 1);
            }
        }

        return sum / (count > 0 ? count : 1);
    }
}
