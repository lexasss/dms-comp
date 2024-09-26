using BenchmarkDotNet.Attributes;

namespace Tests;

[MemoryDiagnoser(false)]
public class Benchmarks
{
    public Benchmarks()
    {
        const int size = 10000;
        _data1 = new float[size];
        _data2 = new float[size];
        for (int i = 0; i < size; i++)
        {
            _data1[i] = (float)Math.Sin(i * Math.PI / 180 / 20);
            _data2[i] = (float)Math.Sin((i + 5) * Math.PI / 180 / 20);
        }
    }

    [Benchmark]
    public double Calc1()
    {
        return ComputeDistance1(_data1, _data2);
    }

    [Benchmark]
    public double Calc2()
    {
        return ComputeDistance2(_data1, _data2);
    }

    // Internal

    const int WindowSize = 5;

    readonly float[] _data1;
    readonly float[] _data2;

    double ComputeDistance1(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2)
    {
        float[,] result = new float[data1.Length + 1, data2.Length + 1];

        for (int i = 0; i <= data1.Length; i++)
            for (int j = 0; j <= data2.Length; j++)
                result[i, j] = float.MaxValue;

        result[0, 0] = 0;
        for (int i = 1; i <= data1.Length; i++)
            for (int j = Math.Max(1, i - WindowSize); j <= Math.Min(data2.Length, i + WindowSize); j++)
                result[i, j] = 0;

        float cost;
        for (int i = 1; i <= data1.Length; i++)
            for (int j = Math.Max(1, i - WindowSize); j <= Math.Min(data2.Length, i + WindowSize); j++)
            {
                cost = Math.Abs(data1[i - 1] - data2[j - 1]);
                result[i, j] = cost + Math.Min(result[i - 1, j],        // insertion
                                        Math.Min(result[i, j - 1],      // deletion
                                                 result[i - 1, j - 1]));// match
            }


        return result[data1.Length, data2.Length] / ((data1.Length + data2.Length + 2) / 2);
    }

    double ComputeDistance2(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2)
    {
        float[,] result = new float[data1.Length + 1, data2.Length + 1];

        float cost;
        var size2 = data2.Length;
        var winStart = Math.Max(1, -WindowSize);
        var winEnd = Math.Min(size2, WindowSize);
        for (int i = 0; i <= data1.Length; i += 1)
        {
            for (int j = 0; j < winStart; j += 1)
                result[i, j] = float.MaxValue;
            
            if (i == 0)
            {
                result[0, 0] = 0;
                winStart = 1;
                winEnd = 1 + WindowSize;
                continue;
            }

            for (int j = winStart; j <= winEnd; j++)
            {
                cost = Math.Abs(data1[i - 1] - data2[j - 1]);
                result[i, j] = cost + Math.Min(result[i - 1, j],        // insertion
                                        Math.Min(result[i, j - 1],      // deletion
                                                 result[i - 1, j - 1]));// match
            }

            for (int j = winEnd + 1; j <= size2; j += 1)
                result[i, j] = float.MaxValue;

            winStart = Math.Max(1, i + 1 - WindowSize);
            winEnd = Math.Min(size2, i + 1 + WindowSize);
        }

        return result[data1.Length, data2.Length] / ((data1.Length + data2.Length + 2) / 2);
    }
}