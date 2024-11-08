using System.ComponentModel;
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
                if (_dms.Info != null)
                {
                    parts.Add(_dms.Info);
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

    public event EventHandler<Dms?>? DmsLoaded;
    public event PropertyChangedEventHandler? PropertyChanged;

    public DmsPlot()
    {
        InitializeComponent();
        DataContext = this;

        var settings = Properties.Settings.Default;
        _themeIndex = settings.Vis_DmsTheme;

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
    PlotColors _theme;

    private void DisplayDms()
    {
        if (_dms != null)
        {
            Painter.DrawPlot(imgDms, _dms.Height, _dms.Width, _dms.Data, (float)AbsoluteScale, _theme);
        }
    }

    // UI

    private void DmsPrompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Loader.LoadDmsFile(dms =>
        {
            Dms = dms;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDmsReady)));
            DmsLoaded?.Invoke(this, dms);
        });
    }
}
