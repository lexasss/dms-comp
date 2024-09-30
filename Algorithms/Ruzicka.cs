namespace DmsComparison.Algorithms;

internal class Ruzicka : Algorithm
{
    // https://search.r-project.org/CRAN/refmans/abdiv/html/ruzicka.html

    public override string Name => "Ruzicka";

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
                    numerator += Math.Min(data1[i], data2[i]);
                    denominator += Math.Max(data1[i], data2[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < data1.Length; i++)
            {
                numerator += Math.Min(data1[i], data2[i]);
                denominator += Math.Max(data1[i], data2[i]);
            }
        }

        return 1.0 - numerator / (denominator != 0 ? denominator : 1);
    }
}
