using System.IO;
using System.Text.Json;
using System.Windows;

namespace DmsComparison;

/// <summary>
/// Stores a DMS measurement and its metadata
/// </summary>
public class Dms
{
    public int Width { get; init; }
    public int Height { get; init; }
    public string? Info { get; init; }
    public string FullPath { get; init; }
    public string Folder { get; init; }
    public string Filename { get; init; }
    public string Date { get; init; }
    public string Time { get; init; }
    public float[] Data => _scan.MeasurementData.IntensityTop;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="scan">Full DMS scan</param>
    /// <param name="fullpath">Filename including full path</param>
    public Dms(IonVision.Scan scan, string fullpath)
    {
        _scan = scan;

        FullPath = fullpath;
        Filename = Path.GetFileNameWithoutExtension(fullpath);
        Folder = fullpath.Split(Path.DirectorySeparatorChar)[^2];

        var dateTime = DateTime.Parse(scan.FinishTime);
        Date = dateTime.ToString("yyyy-MM-dd");
        Time = dateTime.ToString("HH-mm-ss");

        var usv = scan.MeasurementData.Usv;
        var firstUsv = usv[0];
        int i = 1;
        while (usv[i] == firstUsv && ++i < usv.Length)
        {
            // this cycle continues until the next Usv
        }

        Width = i;
        Height = usv.Length / i;

        var str = _scan.Comments.ToString();
        var textComment = JsonSerializer.Deserialize<Comments>(str ?? "");
        Info = textComment?.Text;
    }

    /// <summary>
    /// Creates a DMS object instance by loading data from the file
    /// </summary>
    /// <param name="filename">JSON file storing DMS data</param>
    /// <returns></returns>
    public static Dms? Load(string filename)
    {
        using StreamReader reader = new(filename);
        var json = reader.ReadToEnd();

        try
        {
            var dms = JsonSerializer.Deserialize<IonVision.Scan>(json);
            if (dms != null)
            {
                return new Dms(dms, filename);
            }
            else throw new Exception("Cannot read DMS data");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "DMS loader", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return null;
    }

    // Internal

    record Comments(string Text);

    readonly IonVision.Scan _scan;
}
