using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DmsComparison;

public partial class DmsDiffPlot : UserControl, INotifyPropertyChanged
{
    public float Scale
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

    public bool IsDifferenceReady => _dms1 != null && _dms2 != null && _dms1.Height == _dms2.Height && _dms1.Width == _dms2.Width;

    public event EventHandler<(Dms?, Dms?)>? DmsLoaded;
    public event PropertyChangedEventHandler? PropertyChanged;

    public DmsDiffPlot()
    {
        InitializeComponent();
        DataContext = this;

        _themeIndex = Properties.Settings.Default.Vis_DiffTheme;
        _theme = new PlotColors(Painter.DiffThemes[_themeIndex]);
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

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDifferenceReady)));
    }

    private void DisplayDifference()
    {
        Painter.Clear(imgDmsDiff);
        lblDmsDiff.Content = null;

        if (IsDifferenceReady)
        {
            lblDmsDiff.Content = $"{_dms1!.MixType ?? _dms1.Info} [VS] {_dms2!.MixType ?? _dms2.Info}";
            Painter.DrawDiff(imgDmsDiff, _dms1.Height, _dms1.Width, _dms1.Data, _dms2.Data, Scale, _theme);
        }
        else
        {
            MessageBox.Show("DMS data have distinct number of rows or colunms and therefore their difference cannot be displayed.",
                "DMS loader", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Internal

    Dms? _dms1 = null;
    Dms? _dms2 = null;
    float _scale = 400;
    int _themeIndex = 0;
    PlotColors _theme;

    private void DiffPrompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Loader.LoadTwoDmsFiles((dms1, dms2) => {
            SetDms(dms1, dms2);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDifferenceReady)));
            DmsLoaded?.Invoke(this, (dms1, dms2));
        });
    }
}
