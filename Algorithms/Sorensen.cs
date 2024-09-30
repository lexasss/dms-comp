namespace DmsComparison.Algorithms;

internal class Sorensen : Algorithm
{
    // https://pypi.org/project/distance-metrics-mcda/

    public override string Name => "Sorensen";

    protected override double ComputeDistance(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2)
    {
        double numerator = 0;
        double denominator = 0;

        if (_isDataRectified)
        {
            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] != 0 || data2[i] != 0)
                {
                    numerator += Math.Abs(data1[i] - data2[i]);
                    denominator = data1[i] + data2[i];
                }
            }
        }
        else
        {
            for (int i = 0; i < data1.Length; i++)
            {
                numerator += Math.Abs(data1[i] - data2[i]);
                denominator += data1[i] + data2[i];
            }
        }

        return numerator / (denominator > 0 ? denominator : 1);
    }
}
