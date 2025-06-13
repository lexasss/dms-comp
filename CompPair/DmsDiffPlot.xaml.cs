using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DmsComparison;

public partial class DmsDiffPlot : UserControl, INotifyPropertyChanged
{
    public double Scale
    {
        get => field;
        set
        {
            field = value;
            DisplayDifference();
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
                _theme = new PlotColors(Painter.DiffThemes[value]);
                DisplayDifference();
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
                DisplayDifference();
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
                DisplayDifference();
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
                DisplayDifference();
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
                DisplayDifference();
            }
        }
    }

    public bool CanComputeDifference => _dms1 != null && _dms2 != null && DataService.IsSameShape(_dms1, _dms2);

    public class DmsLoadedEventArgs(Dms? dms1, Dms? dms2) : EventArgs
    {
        public Dms? Dms1 { get; } = dms1;
        public Dms? Dms2 { get; } = dms2;
    }

    public event EventHandler<DmsLoadedEventArgs>? DmsLoaded;
    public event PropertyChangedEventHandler? PropertyChanged;

    public DmsDiffPlot()
    {
        InitializeComponent();

        var settings = Properties.Settings.Default;

        Scale = settings.Vis_DiffScale;
        DataType = (Data.Type)settings.DataProc_DataType;
        DataSource = (Data.Source)settings.DataProc_DataSource;
        DataFilter = (Data.Filter)settings.DataProc_DataFilter;
        DataFilterSettings = new Data.FilterSettings(
            settings.DataProc_FilterSettings_From,
            settings.DataProc_FilterSettings_To,
            (Data.Limits)settings.DataProc_FilterSettings_LimitType);
        ThemeIndex = settings.Vis_DiffTheme;

        _theme = new PlotColors(Painter.DiffThemes[ThemeIndex]);

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
    PlotColors _theme;

    private void DisplayDifference()
    {
        Painter.Clear(imgDmsDiff);
        lblDmsDiff.Content = null;

        if (_dms1 == null || _dms2 == null)
            return;

        if (DataService.IsSameShape(_dms1, _dms2))
        {
            var diff = DataService.GetDifference(_dms1, _dms2, DataType, DataSource, DataFilter, DataFilterSettings);
            if (diff != null)
            {
                Painter.DrawPlot(imgDmsDiff, diff.Rows, diff.Columns, diff.Values, (float)(100.1 - Scale * 10), _theme);
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
            DmsLoaded?.Invoke(this, new DmsLoadedEventArgs(dms1, dms2));
        });
    }
}
