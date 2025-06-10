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
        var array1 = new float[data1.Length];
        var array2 = new float[data2.Length];

        Array.Copy(data1, array1, data1.Length);
        Array.Copy(data2, array2, data2.Length);

        if (options.Crop)
        {
            array1 = DataProcessing.Crop(array1, size);
            array2 = DataProcessing.Crop(array2, size);
        }

        if (options.UseRectification)
        {
            array1 = DataProcessing.Rectify(array1);
            array2 = DataProcessing.Rectify(array2);
        }

        _isDataRectified = options.UseRectification;

        DataProcessing.Normalize(array1, size, options.NormalizationType);
        DataProcessing.Normalize(array2, size, options.NormalizationType);

        return Math.Abs(ComputeDistance(array1, array2));
    }

    // Internal

    protected bool _isDataRectified { get; private set; } = false;

    protected abstract double ComputeDistance(ReadOnlySpan<float> data1, ReadOnlySpan<float> data2);
}
