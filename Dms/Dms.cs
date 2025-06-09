using System.IO;
using System.Text.Json;
using System.Windows;

namespace DmsComparison;

/// <summary>
/// Stores a DMS measurement and its metadata
/// </summary>
public class Dms
{
    public string? Info { get; init; }
    public float[]? Pulses { get; init; }
    public string? MixType { get; init; }
    public string FullPath { get; init; }
    public string Folder { get; init; }
    public string Filename { get; init; }
    public string Date { get; init; }
    public string Time { get; init; }

    public IonVision.Scan Scan => _scan;
    public int Width { get; init; }
    public int Height { get; init; }
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
            // This cycle continues until the next Usv value is detected
            // This way we get the number of DMS columns stored in i
        }

        Width = i;
        Height = usv.Length / i;

        var str = _scan.Comments.ToString();

        List<string?> infoLines = [];

        try
        {
            var textComment = JsonSerializer.Deserialize<CommentsGeneral>(str ?? "");
            infoLines.Add(textComment?.text);
        }
        catch { }

        try
        {
            var quickComments = JsonSerializer.Deserialize<CommentsQuick>(str ?? "");
            infoLines.AddRange(quickComments?._quickComments ?? []);
        }
        catch { }

        var validComments = infoLines.Where(line => line != null);
        Info = validComments.Count() > 0 ? string.Join("; ", validComments) : null;

        var infoFields = Info?.Split(",");
        var mixTypeRecord = infoFields?[0];
        MixType = infoFields?.Length > 1 && mixTypeRecord?.Length > 3 ? mixTypeRecord[3..] : null;

        try
        {
            var pulsesComment = JsonSerializer.Deserialize<CommentsPulses>(str ?? "");
            Pulses = pulsesComment?.Flows;

            if (Info == null && Pulses != null)
            {
                Info = string.Join(' ', Pulses);
            }
        }
        catch { }
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
            var scan = JsonSerializer.Deserialize<IonVision.Scan>(json);
            if (scan != null)
            {
                return new Dms(scan, filename);
            }
            else throw new Exception("Cannot read DMS data");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "DMS loader", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return null;
    }

    /// <summary>
    /// Creates two DMS object instance by loading data from the file with multiple DMS data records
    /// </summary>
    /// <param name="filename">JSON file storing one or more DMS data records</param>
    /// <returns></returns>
    public static Dms[]? LoadMultiple(string filename)
    {
        using StreamReader reader = new(filename);
        var json = reader.ReadToEnd();

        try
        {
            JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            var scans = JsonSerializer.Deserialize<IonVision.Scan[]>(json, options);
            if (scans != null)
            {
                if (scans.Length < 2)
                {
                    throw new Exception("Not enough DMS data");
                }
                return scans
                    .Select(scan => {
                        Dms? obj = null;
                        try { obj = new Dms(scan, filename); }
                        catch { }
                        return obj;
                    })
                    .Where(dms => dms != null)
                    .Cast<Dms>()
                    .ToArray();
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

    record CommentsGeneral(string text);
    record CommentsQuick(string[] _quickComments);

    record CommentsPulses(string[] pulses)
    {
        public float[] Flows => pulses?.Select(p => float.Parse(p.Split('=')[1].Split(',')[0])).ToArray() ?? [];
    }

    readonly IonVision.Scan _scan;
}
