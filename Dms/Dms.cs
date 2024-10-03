using System.IO;
using System.Text.Json;
using System.Windows;

namespace DmsComparison;

public class Dms
{
    public int Width { get; init; }
    public int Height { get; init; }
    public string? Info { get; init; }
    public string Filename { get; init; }
    public string Date { get; init; }
    public string Time { get; init; }
    public float[] Data => _scan.MeasurementData.IntensityTop;

    public Dms(IonVision.Scan scan, string filename)
    {
        _scan = scan;

        Filename = filename;

        var dateTime = DateTime.Parse(scan.FinishTime);
        Date = dateTime.ToString("yyyy-MM-dd");
        Time = dateTime.ToString("HH-mm-ss");

        var usv = scan.MeasurementData.Usv;
        var firstUsv = usv[0];
        int i = 1;
        while (usv[i] == firstUsv && ++i < usv.Length) { }

        Width = i;
        Height = usv.Length / i;

        var str = _scan.Comments.ToString();
        var textComment = JsonSerializer.Deserialize<Comments>(str);
        Info = textComment?.text;
    }

    public static Dms? Load(string filename)
    {
        using StreamReader reader = new(filename);
        var json = reader.ReadToEnd();

        try
        {
            var dms = JsonSerializer.Deserialize<IonVision.Scan>(json);
            if (dms != null)
            {
                return new Dms(dms, Path.GetFileNameWithoutExtension(filename));
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

    record Comments(string text);

    readonly IonVision.Scan _scan;
}
