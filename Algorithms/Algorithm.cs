using DmsComparison.Common;

namespace DmsComparison.Algorithms;

public class Options(bool useRectification, NormalizationType normalizationType, bool cropArea)
{
    public bool UseRectification => useRectification;
    public NormalizationType NormalizationType => normalizationType;
    public bool Crop => cropArea;
    public override string ToString()
    {
        string rect = UseRectification ? "R" : "-";
        string norm = NormalizationType == NormalizationType.None ? "-" : NormalizationType.ToString()[0].ToString();
        string crop = Crop ? "C" : "-";
        return $"{rect},{norm},{crop}";
    }
}

public abstract class Algorithm
{
    public abstract string Name { get; }

    public virtual bool IsVisible => true;

    /// <summary>
    /// Returns types of all algorithms derived from "Algorithm"
    /// </summary>
    /// <returns>List of types</returns>
    public static Type[] GetDescendantTypes() => System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
        .Where(type => type.IsSubclassOf(typeof(Algorithm))).ToArray();

    public double ComputeDistance(float[] data1, float[] data2, Size size, Options options)
    {
        data1 = (float[])data1.Clone();
        data2 = (float[])data2.Clone();

        if (options.Crop)
        {
            data1 = DataProcessing.Crop(data1, size);
            data2 = DataProcessing.Crop(data2, size);
        }

        if (options.UseRectification)
        {
            data1 = DataProcessing.Rectify(data1);
            data2 = DataProcessing.Rectify(data2);
            _isDataRectified = options.UseRectification;
        }

        DataProcessing.Normalize(data1, size, options.NormalizationType);
        DataProcessing.Normalize(data2, size, options.NormalizationType);

        return Math.Abs(ComputeDistance(data1, data2));
    }

    // Internal

    protected bool _isDataRectified { get; private set; } = false;

    protected abstract double ComputeDistance(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2);
}
