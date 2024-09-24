namespace DmsComparison.Algorithms;

internal class Hamming : Algorithm
{
    // https://en.wikipedia.org/wiki/Hamming_distance

    public override string Name => "Hamming";

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
                    sum += data1[i] == data2[i] ? 0 : 1;
                    count += 1;
                }
            }
        }
        else
        {
            count = data1.Length;
            for (int i = 0; i < data1.Length; i++)
            {
                sum += data1[i] == data2[i] ? 0 : 1;
            }
        }

        return sum / (count > 0 ? count : 1);
    }
}
