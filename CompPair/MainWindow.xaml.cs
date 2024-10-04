using DmsComparison;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

    private static void LoadDmsFile(Action<Dms?> proceed)
    {
        string? filename = SelectDmsFile();
        if (!string.IsNullOrEmpty(filename))
        {
            var dms = Dms.Load(filename);
            proceed(dms);
        }
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

    private void UpdateDmsUI(Dms? dms, Canvas canvas, Label info)
    {
        if (dms != null)
        {
            DisplayDms(canvas, dms);
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

    private void DisplayDms(Canvas canvas, Dms dms)
    {
        var scale = chkAbsoluteScale.IsChecked ?? false ? (float)sldAbsoluteScale.Value : 0;
        Painter.DrawPlot(canvas, null, dms.Height, dms.Width, dms.Data, scale);

        if (_dms1 != null && _dms2 != null)
        {
            DisplayDmsDiff(cnvDmsDiff, _dms1, _dms2);
        }
    }

    private void DisplayDmsDiff(Canvas canvas, Dms dms1, Dms dms2)
    {
        if (dms1.Height == dms2.Height && dms1.Width == dms2.Width)
        {
            Painter.DrawDiff(canvas, dms1.Height, dms1.Width, dms1.Data, dms2.Data);
            dstDistance.Update(dms1, dms2);
        }
        else
        {
            dstDistance.Clear();
            canvas.Children.Clear();
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
                Painter.DrawPlot(cnvDms1, null, _dms1.Height, _dms1.Width, _dms1.Data, scale);
            if (_dms2 != null)
                Painter.DrawPlot(cnvDms2, null, _dms2.Height, _dms2.Width, _dms2.Data, scale);

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
            UpdateDmsUI(_dms1, cnvDms1, lblDms1);
        });
    }

    private void Dms2Prompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        LoadDmsFile(dms =>
        {
            _dms2 = dms;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDms2Ready)));
            UpdateDmsUI(_dms2, cnvDms2, lblDms2);
        });
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