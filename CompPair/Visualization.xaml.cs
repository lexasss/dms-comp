using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DmsComparison;

public partial class Visualization : UserControl
{
    public class UsingAbsoluteScaleChangedEventArgs(bool isUsingAbsoluteScale) : RoutedEventArgs
    {
        public bool IsUsingAbsoluteScale { get; } = isUsingAbsoluteScale;
    }
    public class ScaleChangedEventArgs(double scale) : RoutedEventArgs
    {
        public double Scale { get; } = scale;
    }
    public class ThemeChangedEventArgs(int themeIndex) : RoutedEventArgs
    {
        public int ThemeIndex { get; } = themeIndex;
    }

    public event EventHandler<UsingAbsoluteScaleChangedEventArgs>? UsingAbsoluteScaleChanged;
    public event EventHandler<ScaleChangedEventArgs>? AbsoluteScaleChanged;
    public event EventHandler<ScaleChangedEventArgs>? DiffScaleChanged;
    public event EventHandler<ThemeChangedEventArgs>? DmsThemeChanged;
    public event EventHandler<ThemeChangedEventArgs>? DiffThemeChanged;

    public bool IsUsingAbosluteScale => chkAbsoluteScale.IsChecked == true;
    public double AbsoluteScale => sldAbsoluteScale.Value;
    public double DiffScale => sldDiffScale.Value;

    public Visualization()
    {
        InitializeComponent();

        var settings = Properties.Settings.Default;
        chkAbsoluteScale.IsChecked = settings.Vis_UseAbsoluteScale;
        sldAbsoluteScale.Value = settings.Vis_AbsoluteScale;
        sldDiffScale.Value = settings.Vis_DiffScale;

        ListViewItem CreateThemeListItem(PlotColors.Theme theme)
        {
            var lvi = new ListViewItem();
            var border = new Border() { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1) };
            var rect = new Rectangle() { Height = 16 };
            rect.SizeChanged += (s, e) => PlotColors.DrawTheme(theme, rect);
            border.Child = rect;
            lvi.Content = border;
            return lvi;
        }

        foreach (var theme in Painter.DmsThemes)
            cmbDmsThemes.Items.Add(CreateThemeListItem(theme));

        foreach (var theme in Painter.DiffThemes)
            cmbDiffThemes.Items.Add(CreateThemeListItem(theme));

        cmbDmsThemes.SelectedIndex = Math.Min(cmbDmsThemes.Items.Count - 1, settings.Vis_DmsTheme);
        cmbDiffThemes.SelectedIndex = Math.Min(cmbDiffThemes.Items.Count - 1, settings.Vis_DiffTheme);

        _isInitialized = true;
    }

    // Internal

    CancellationTokenSource _cts = new();
    DateTime _timerStarted = DateTime.UtcNow.AddYears(-1);

    readonly bool _isInitialized = false;

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

            AbsoluteScaleChanged?.Invoke(this, new ScaleChangedEventArgs(sldAbsoluteScale.Value));
        });
    }

    public void ReportDiffScaleChanged()
    {
        Dispatcher.Invoke(() =>
        {
            var settings = Properties.Settings.Default;
            settings.Vis_DiffScale = sldDiffScale.Value;
            settings.Save();

            DiffScaleChanged?.Invoke(this, new ScaleChangedEventArgs(sldDiffScale.Value));
        });
    }


    // UI

    private void AbsoluteScale_CheckChanged(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
        {
            ReportAbsoluteScaleChanged();
            UsingAbsoluteScaleChanged?.Invoke(this, new UsingAbsoluteScaleChangedEventArgs(chkAbsoluteScale.IsChecked == true));
        }
    }

    private void AbsoluteScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitialized)
            Throttle(500, ReportAbsoluteScaleChanged);
    }

    private void DiffScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitialized)
            Throttle(500, ReportDiffScaleChanged);
    }

    private void DmsThemes_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        foreach (ListBoxItem item in cmbDmsThemes.Items)
        {
            (item.Content as Border)!.Width = cmbDmsThemes.ActualWidth - 32;
        }
    }

    private void DmsThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var settings = Properties.Settings.Default;
        settings.Vis_DmsTheme = cmbDmsThemes.SelectedIndex;
        DmsThemeChanged?.Invoke(this, new ThemeChangedEventArgs(cmbDmsThemes.SelectedIndex));
    }

    private void DiffThemes_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        foreach (ListBoxItem item in cmbDiffThemes.Items)
        {
            (item.Content as Border)!.Width = cmbDiffThemes.ActualWidth - 32;
        }
    }

    private void DiffThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var settings = Properties.Settings.Default;
        settings.Vis_DiffTheme = cmbDiffThemes.SelectedIndex;
        DiffThemeChanged?.Invoke(this, new ThemeChangedEventArgs(cmbDiffThemes.SelectedIndex));
    }
}
