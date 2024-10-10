using DmsComparison;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CompPair;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public bool IsDms1Ready => _dms1 != null;
    public bool IsDms2Ready => _dms2 != null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        RenderOptions.SetBitmapScalingMode(imgDms1, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(imgDms1, EdgeMode.Aliased);

        var settings = Properties.Settings.Default;
        chkAbsoluteScale.IsChecked = settings.Vis_UseAbsoluteScale;
        sldAbsoluteScale.Value = settings.Vis_AbsoluteScale;

        _isInitialized = true;
    }

    // Internal

    Dms? _dms1 = null;
    Dms? _dms2 = null;

    CancellationTokenSource _cts = new CancellationTokenSource();
    DateTime _timerStarted = DateTime.UtcNow.AddYears(-1);

    bool _isInitialized = false;

    private static bool LoadDmsFile(Action<Dms?> proceed)
    {
        string? filename = SelectDmsFile();
        if (!string.IsNullOrEmpty(filename))
        {
            var dms = Dms.Load(filename);
            proceed(dms);

            return dms != null;
        }

        return false;
    }

    private static bool LoadTwoDmsFiles(Action<Dms?> proceed1, Action<Dms?> proceed2)
    {
        (string? filename1, string? filename2) = SelectTwoDmsFiles();
        if (!string.IsNullOrEmpty(filename1) && !string.IsNullOrEmpty(filename2))
        {
            var dms1 = Dms.Load(filename1);
            proceed1(dms1);

            var dms2 = Dms.Load(filename2);
            proceed2(dms2);

            return dms1 != null && dms2 != null;
        }

        return false;
    }

    private static string? SelectDmsFile()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog();
        ofd.Filter = "JSON files|*.json";
        if (ofd.ShowDialog() == true)
        {
            return ofd.FileName;
        }

        return null;
    }

    private static (string?, string?) SelectTwoDmsFiles()
    {
        var ofd = new Microsoft.Win32.OpenFileDialog();
        ofd.Filter = "JSON files|*.json";
        ofd.Multiselect = true;
        ofd.Title = "Select two DMS files";
        if (ofd.ShowDialog() == true)
        {
            if (ofd.FileNames.Length == 2)
            {
                return (ofd.FileNames[0], ofd.FileNames[1]);
            }
            else
            {
                MessageBox.Show("Please select exactly two DMS files.", "DMS comparison", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        return (null, null);
    }

    private void UpdateDmsUI(Dms? dms, Image image, Label info)
    {
        if (dms != null)
        {
            DisplayDms(image, dms);
        }

        List<string> parts = [];
        if (dms != null)
        {
            parts.Add(dms.Folder);
            parts.Add(dms.Filename);
            if (dms.Info != null)
            {
                parts.Add(dms.Info);
            }
        }

        info.Content = string.Join(" | ", parts);
    }

    private void DisplayDms(Image image, Dms dms)
    {
        var scale = chkAbsoluteScale.IsChecked ?? false ? (float)sldAbsoluteScale.Value : 0;
        Painter.DrawPlot(image, dms.Height, dms.Width, dms.Data, scale);

        if (_dms1 != null && _dms2 != null)
        {
            DisplayDmsDiff(imgDmsDiff, _dms1, _dms2);
        }
    }

    private void DisplayDmsDiff(Image image, Dms dms1, Dms dms2)
    {
        if (dms1.Height == dms2.Height && dms1.Width == dms2.Width)
        {
            Painter.DrawDiff(image, dms1.Height, dms1.Width, dms1.Data, dms2.Data);
            dstDistance.Update(dms1, dms2);
        }
        else
        {
            dstDistance.Clear();
            //canvas.Children.Clear();
            image.Source = null;
            MessageBox.Show("DMS data have distinct number of rows or colunms and therefore their difference cannot be displayed.",
                "DMS loader", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void UpdatePlotOnThresholdChange()
    {
        Dispatcher.Invoke(() =>
        {
            var scale = chkAbsoluteScale.IsChecked ?? false ? (float)sldAbsoluteScale.Value : 0;
            if (_dms1 != null)
                Painter.DrawPlot(imgDms1, _dms1.Height, _dms1.Width, _dms1.Data, scale);
            if (_dms2 != null)
                Painter.DrawPlot(imgDms2, _dms2.Height, _dms2.Width, _dms2.Data, scale);

            var settings = Properties.Settings.Default;
            settings.Vis_UseAbsoluteScale = chkAbsoluteScale.IsChecked ?? false;
            settings.Vis_AbsoluteScale = sldAbsoluteScale.Value;
            settings.Save();
        });
    }

    public void Throttle(int interval, Action action)
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();

        var curTime = DateTime.UtcNow;
        if (curTime.Subtract(_timerStarted).TotalMilliseconds < interval)
            interval -= (int)curTime.Subtract(_timerStarted).TotalMilliseconds;

        Task.Run(async delegate
        {
            try
            {
                await Task.Delay(interval, _cts.Token);
                action();
            }
            catch { }
        });

        _timerStarted = curTime;
    }

    // UI events

    private void Dms1Prompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        LoadDmsFile(dms =>
        {
            _dms1 = dms;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDms1Ready)));
            UpdateDmsUI(_dms1, imgDms1, lblDms1);
        });
    }

    private void Dms2Prompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        LoadDmsFile(dms =>
        {
            _dms2 = dms;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDms2Ready)));
            UpdateDmsUI(_dms2, imgDms2, lblDms2);
        });
    }

    private void DiffPrompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        LoadTwoDmsFiles(
            dms => {
                _dms1 = dms;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDms1Ready)));
                UpdateDmsUI(_dms1, imgDms1, lblDms1);
            },
            dms => {
                _dms2 = dms;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDms2Ready)));
                UpdateDmsUI(_dms2, imgDms2, lblDms2);
            }
        );
    }

    private void AbsoluteScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitialized)
            Throttle(1000, UpdatePlotOnThresholdChange);
    }

    private void AbsoluteScale_CheckChanged(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
            UpdatePlotOnThresholdChange();
    }
}