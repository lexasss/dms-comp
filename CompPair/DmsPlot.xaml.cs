using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace DmsComparison;

public partial class DmsPlot : UserControl, INotifyPropertyChanged
{
    public double AbsoluteScale
    {
        get => field;
        set
        {
            field = value;
            DisplayDms();
        }
    }

    public bool IsDmsReady => Dms != null;

    public Dms? Dms
    {
        get => field;
        set
        {
            field = value;

            DisplayDms();

            List<string> parts = [];
            if (value != null)
            {
                parts.Add(value.Folder);
                parts.Add(value.Filename);
                if (!string.IsNullOrEmpty(value.Info))
                {
                    parts.Add(value.Info);
                }
                else if (value.Pulses != null)
                {
                    parts.Add(string.Join(' ', value.Pulses));
                }
            }

            lblDms.Content = string.Join(" | ", parts);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDmsReady)));
        }
    }

    public int ThemeIndex
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                _theme = new PlotColors(Painter.DmsThemes[value]);
                DisplayDms();
            }
        }
    }

    public Data.Type DataType
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                DisplayDms();
            }
        }
    }

    public Data.Source DataSource
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                DisplayDms();
            }
        }
    }

    public Data.Filter DataFilter
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                DisplayDms();
            }
        }
    }

    public Data.FilterSettings DataFilterSettings
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                DisplayDms();
            }
        }
    }

    public class DmsLoadedEventArgs(Dms? dms) : EventArgs
    {
        public Dms? Dms { get; } = dms;
    }

    public event EventHandler<DmsLoadedEventArgs>? DmsLoaded;
    public event PropertyChangedEventHandler? PropertyChanged;

    public DmsPlot()
    {
        InitializeComponent();
        DataContext = this;

        var settings = Properties.Settings.Default;

        AbsoluteScale = settings.Vis_UseAbsoluteScale ? settings.Vis_AbsoluteScale : 0;
        DataType = (Data.Type)settings.DataProc_DataType;
        DataSource = (Data.Source)settings.DataProc_DataSource;
        DataFilter = (Data.Filter)settings.DataProc_DataFilter;
        DataFilterSettings = new Data.FilterSettings(
            settings.DataProc_FilterSettings_From,
            settings.DataProc_FilterSettings_To,
            (Data.Limits)settings.DataProc_FilterSettings_LimitType);
        ThemeIndex = settings.Vis_DmsTheme;

        _theme = new PlotColors(Painter.DmsThemes[ThemeIndex]);

        //RenderOptions.SetBitmapScalingMode(imgDms, BitmapScalingMode.NearestNeighbor);
        //RenderOptions.SetEdgeMode(imgDms, EdgeMode.Aliased);
    }

    public bool CopyToMemory()
    {
        if (imgDms.Source is System.Windows.Media.Imaging.BitmapSource bmp)
        {
            System.Windows.Clipboard.SetImage(bmp);
            return true;
        }
        return false;
    }

    // Internal

    PlotColors _theme;

    private void DisplayDms()
    {
        if (Dms != null)
        {
            var data = DataService.GetRaw(Dms, DataType, DataSource, DataFilter, DataFilterSettings);
            Painter.DrawPlot(imgDms, data.Rows, data.Columns, data.Values, (float)AbsoluteScale, _theme);
        }
    }

    // UI

    private void DmsPrompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Loader.LoadDmsFile(dms =>
        {
            Dms = dms;
            DmsLoaded?.Invoke(this, new DmsLoadedEventArgs(dms));
        });
    }
}
