using System.ComponentModel;
using System.Data.Common;
using System.Windows.Controls;
using System.Windows.Input;

namespace DmsComparison;

public partial class DmsPlot : UserControl, INotifyPropertyChanged
{
    public double AbsoluteScale
    {
        get => _absoluteScale;
        set
        {
            _absoluteScale = value;
            DisplayDms();
        }
    }

    public bool IsDmsReady => _dms != null;

    public Dms? Dms
    {
        get => _dms;
        set
        {
            _dms = value;

            DisplayDms();

            List<string> parts = [];
            if (_dms != null)
            {
                parts.Add(_dms.Folder);
                parts.Add(_dms.Filename);
                if (!string.IsNullOrEmpty(_dms.Info))
                {
                    parts.Add(_dms.Info);
                }
                else if (_dms.Pulses != null)
                {
                    parts.Add(string.Join(' ', _dms.Pulses));
                }
            }

            lblDms.Content = string.Join(" | ", parts);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDmsReady)));
        }
    }

    public int ThemeIndex
    {
        get => _themeIndex;
        set
        {
            if (_themeIndex != value)
            {
                _themeIndex = value;
                _theme = new PlotColors(Painter.DmsThemes[_themeIndex]);
                DisplayDms();
            }
        }
    }

    public Data.Source DataSource
    {
        get => _dataSource;
        set
        {
            if (_dataSource != value)
            {
                _dataSource = value;
                DisplayDms();
            }
        }
    }

    public Data.Filter DataFilter
    {
        get => _dataFilter;
        set
        {
            if (_dataFilter != value)
            {
                _dataFilter = value;
                DisplayDms();
            }
        }
    }

    public event EventHandler<Dms?>? DmsLoaded;
    public event PropertyChangedEventHandler? PropertyChanged;

    public DmsPlot()
    {
        InitializeComponent();
        DataContext = this;

        var settings = Properties.Settings.Default;

        _dataSource = (Data.Source)settings.DataProc_DataSource;
        _dataFilter = (Data.Filter)settings.DataProc_DataFilter;
        _themeIndex = settings.Vis_DmsTheme;
        _absoluteScale = settings.Vis_UseAbsoluteScale ? settings.Vis_AbsoluteScale : 0;

        _theme = new PlotColors(Painter.DmsThemes[_themeIndex]);

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

    Dms? _dms = null;
    double _absoluteScale = 400;
    int _themeIndex = 0;
    Data.Source _dataSource;
    Data.Filter _dataFilter;
    PlotColors _theme;

    private void DisplayDms()
    {
        if (_dms != null)
        {
            var data = DataService.GetRaw(_dms, _dataFilter, _dataSource);
            Painter.DrawPlot(imgDms, data.Rows, data.Columns, data.Values, (float)AbsoluteScale, _theme);
        }
    }

    // UI

    private void DmsPrompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Loader.LoadDmsFile(dms =>
        {
            Dms = dms;
            DmsLoaded?.Invoke(this, dms);
        });
    }
}
