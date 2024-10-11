using System.Windows;
using System.Windows.Controls;

namespace CompPair;

public partial class Visualization : UserControl
{
    public event EventHandler<bool>? UsingAbsoluteScaleChanged;
    public event EventHandler<double>? AbsoluteScaleChanged;

    public bool IsUsingAbosluteScale => chkAbsoluteScale.IsChecked == true;
    public float AbsoluteScale => (float)sldAbsoluteScale.Value;

    public Visualization()
    {
        InitializeComponent();

        var settings = Properties.Settings.Default;
        chkAbsoluteScale.IsChecked = settings.Vis_UseAbsoluteScale;
        sldAbsoluteScale.Value = settings.Vis_AbsoluteScale;

        _isInitialized = true;
    }

    // Internal

    CancellationTokenSource _cts = new CancellationTokenSource();
    DateTime _timerStarted = DateTime.UtcNow.AddYears(-1);

    bool _isInitialized = false;

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

    public void ReportAbsoluteScaleChanged()
    {
        Dispatcher.Invoke(() =>
        {
            var settings = Properties.Settings.Default;
            settings.Vis_UseAbsoluteScale = chkAbsoluteScale.IsChecked ?? false;
            settings.Vis_AbsoluteScale = sldAbsoluteScale.Value;
            settings.Save();

            AbsoluteScaleChanged?.Invoke(this, sldAbsoluteScale.Value);
        });
    }


    // UI

    private void AbsoluteScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitialized)
            Throttle(500, ReportAbsoluteScaleChanged);
    }

    private void AbsoluteScale_CheckChanged(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
            ReportAbsoluteScaleChanged();
    }
}
