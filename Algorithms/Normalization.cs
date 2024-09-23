namespace DmsComparison.Algorithms;

public class Normalization
{
    public enum Type
    {
        None,
        Linear,
        MinMax,
    }

    public static void Process(float[] data, Type type)
    {
        if (type == Type.None)
        {
            // do nothing
        }
        else if (type == Type.Linear)
        {
            var max = data.Max();
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = data[i] / max;
            }
        }
        else if (type == Type.MinMax)
        {
            var min = data.Min();
            var max = data.Max();
            var range = max - min;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (data[i] - min) / range;
            }
        }
        else
        {
            throw new NotImplementedException($"Normalization of type '{type}' is not implemented");
        }
    }
}
