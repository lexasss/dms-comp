namespace DmsComparison.Algorithms;

internal class Wasserstein : Algorithm
{
    // https://visualstudiomagazine.com/Articles/2021/08/16/wasserstein-distance.aspx

    public override string Name => "Wasserstein";

    public override bool IsVisible => false;

    protected override double ComputeDistance(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2)
    {
        float[] dirt = (float[])data1.ToArray().Clone();
        float[] holes = (float[])data2.ToArray().Clone();
        double totalWork = 0.0;
        while (true)
        {
            int fromIdx = FirstNonZero(dirt);
            int toIdx = FirstNonZero(holes);
            if (fromIdx == -1 || toIdx == -1)
                break;
            double work = MoveDirt(dirt, fromIdx, holes, toIdx);
            totalWork += work;
        }
        return totalWork;
    }

    static int FirstNonZero(float[] vec)
    {
        int dim = vec.Length;
        for (int i = 0; i < dim; ++i)
            if (vec[i] > 0.0)
                return i;
        return -1;
    }

    static double MoveDirt(float[] dirt, int di, float[] holes, int hi)
    {
        float flow = 0.0f;
        if (dirt[di] <= holes[hi])
        {
            flow = dirt[di];
            dirt[di] = 0.0f;
            holes[hi] -= flow;
        }
        else if (dirt[di] > holes[hi])
        {
            flow = holes[hi];
            dirt[di] -= flow;
            holes[hi] = 0.0f;
        }
        int dist = Math.Abs(di - hi);
        return flow * dist;
    }
}
