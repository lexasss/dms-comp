namespace DmsComparison.Algorithms;

internal class WaveHedges : Algorithm
{
    // https://github.com/aziele/statistical-distance/blob/main/distance.py

    public override string Name => "Wave Hedges";

    protected override double ComputeDistance(float[] data1, float[] data2)
    {
        double sum = 0;
        int count = 0;

        float diff, max;

        if (_isDataRectified)
        {
            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] != 0 || data2[i] != 0)
                {
                    diff = Math.Abs(data1[i] - data2[i]);
                    max = Math.Max(data1[i], data2[i]);
                    sum += max != 0 ? diff / max : 0;

                    count += 1;
                }
            }
        }
        else
        {
            count = data1.Length;
            for (int i = 0; i < data1.Length; i++)
            {
                diff = Math.Abs(data1[i] - data2[i]);
                max = Math.Max(data1[i], data2[i]);
                sum += max != 0 ? diff / max : 0;
            }
        }

        return sum / (count > 0 ? count : 1);
    }
}
