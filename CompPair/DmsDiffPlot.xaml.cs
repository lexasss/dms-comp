using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DmsComparison;

public partial class DmsDiffPlot : UserControl, INotifyPropertyChanged
{
    public double Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            DisplayDifference();
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
                _theme = new PlotColors(Painter.DiffThemes[_themeIndex]);
                DisplayDifference();
            }
        }
    }

    public bool CanComputeDifference => _dms1 != null && _dms2 != null && DataService.IsSameShape(_dms1, _dms2);

    public event EventHandler<(Dms?, Dms?)>? DmsLoaded;
    public event PropertyChangedEventHandler? PropertyChanged;

    public DmsDiffPlot()
    {
        InitializeComponent();
        DataContext = this;

        var settings = Properties.Settings.Default;

        _themeIndex = settings.Vis_DiffTheme;
        _theme = new PlotColors(Painter.DiffThemes[_themeIndex]);

        _scale = settings.Vis_DiffScale;
    }

    public bool CopyToMemory()
    {
        if (imgDmsDiff.Source is System.Windows.Media.Imaging.BitmapSource bmp)
        {
            Clipboard.SetImage(bmp);
            return true;
        }

        return false;
    }

    public void SetDms(Dms? dms1, Dms? dms2)
    {
        _dms1 = dms1;
        _dms2 = dms2;

        DisplayDifference();

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanComputeDifference)));
    }

    // Internal

    Dms? _dms1 = null;
    Dms? _dms2 = null;
    double _scale = 4;
    int _themeIndex = 0;
    PlotColors _theme;

    private void DisplayDifference()
    {
        Painter.Clear(imgDmsDiff);
        lblDmsDiff.Content = null;

        if (_dms1 == null || _dms2 == null)
            return;

        if (DataService.IsSameShape(_dms1, _dms2))
        {
            var diff = DataService.GetDifference(_dms1, _dms2, DataType.Raw);
            if (diff != null)
            {
                Painter.DrawPlot(imgDmsDiff, diff.Rows, diff.Columns, diff.Values, (float)(1000.0 - 999.9 * Math.Sqrt(Scale / 10)), _theme);
                lblDmsDiff.Content = $"{_dms1!.MixType ?? _dms1.Info} [VS] {_dms2!.MixType ?? _dms2.Info}";
            }
            //Painter.DrawDiff(imgDmsDiff, _dms1.Height, _dms1.Width, _dms1.Data, _dms2.Data, (float)AbsoluteScale, _theme);
        }
        else
        {
            MessageBox.Show("DMS data have distinct number of rows or colunms and therefore their difference cannot be displayed.",
                "DMS loader", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DiffPrompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Loader.LoadTwoDmsFiles((dms1, dms2) => {
            SetDms(dms1, dms2);
            DmsLoaded?.Invoke(this, (dms1, dms2));
        });
    }
}
