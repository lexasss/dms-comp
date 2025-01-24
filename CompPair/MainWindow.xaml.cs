using System.Windows;
using System.Windows.Input;

namespace DmsComparison;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var settings = Properties.Settings.Default;
        stpTools.HorizontalAlignment = (HorizontalAlignment)settings.UI_ToolPanel_HorzAlign;
        stpTools.VerticalAlignment = (VerticalAlignment)settings.UI_ToolPanel_VertAlign;

        _diffThemeIndex = settings.Vis_DiffTheme;
        _diffTheme = new PlotColors(Painter.DiffThemes[_diffThemeIndex]);
    }

    // Internal

    bool _canDragToolPanel = false;
    Point? _toolPanelClickPoint = null;

    int _diffThemeIndex = 0;
    PlotColors _diffTheme;

    private void DisplayDmsDiff(Dms? dms1, Dms? dms2)
    {
        Painter.Clear(imgDmsDiff);
        dstDistance.Clear();
        lblDmsDiff.Content = null;

        if (dms1 == null || dms2 == null)
            return;

        if (dms1.Height == dms2.Height && dms1.Width == dms2.Width)
        {
            Painter.DrawDiff(imgDmsDiff, dms1.Height, dms1.Width, dms1.Data, dms2.Data, (float)visVisOptions.DiffScale, _diffTheme);
            lblDmsDiff.Content = $"{dms1.MixType} vs {dms2.MixType}";
            dstDistance.Update(dms1, dms2);
        }
        else
        {
            MessageBox.Show("DMS data have distinct number of rows or colunms and therefore their difference cannot be displayed.",
                "DMS loader", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void FlickerElement(FrameworkElement el, int durationMs)
    {
        var visibility = el.Visibility;
        el.Visibility = Visibility.Visible;

        Task.Run(async () =>
        {
            await Task.Delay(durationMs);
            Dispatcher.Invoke(() => el.Visibility = visibility);
        });
    }

    // UI events

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (_canDragToolPanel)
        {
            var cellSizeX = ActualWidth / 3;
            var cellSizeY = ActualHeight / 3;
            var mousePos = e.GetPosition(this);
            stpTools.HorizontalAlignment = mousePos.X < cellSizeX ? HorizontalAlignment.Left :
                                           mousePos.X > 2 * cellSizeX ? HorizontalAlignment.Right :
                                           HorizontalAlignment.Center;
            stpTools.VerticalAlignment = mousePos.Y < cellSizeY ? VerticalAlignment.Top :
                                         mousePos.Y > 2 * cellSizeY ? VerticalAlignment.Bottom :
                                         VerticalAlignment.Center;
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        var settings = Properties.Settings.Default;
        settings.UI_ToolPanel_HorzAlign = (int)stpTools.HorizontalAlignment;
        settings.UI_ToolPanel_VertAlign = (int)stpTools.VerticalAlignment;
        settings.Save();
    }

    private void DiffPrompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Loader.LoadTwoDmsFiles((dms1, dms2) => {
            dmsPlot1.Dms = dms1;
            dmsPlot2.Dms = dms2;
            DisplayDmsDiff(dms1, dms2);
        });
    }

    private void ToolPanel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _toolPanelClickPoint = e.GetPosition(stpTools);

        stpTools.Cursor = Cursors.Hand;
        stpTools.CaptureMouse();
    }

    private void ToolPanel_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _canDragToolPanel = false;
        _toolPanelClickPoint = null;

        stpTools.Cursor = Cursors.Arrow;
        stpTools.ReleaseMouseCapture();
    }

    private void ToolPanel_MouseMove(object sender, MouseEventArgs e)
    {
        if (_toolPanelClickPoint != null && !_canDragToolPanel)
        {
            var mousePos = e.GetPosition(stpTools);
            if (Math.Sqrt(Math.Pow(mousePos.X - _toolPanelClickPoint.Value.X, 2) + Math.Pow(mousePos.Y - _toolPanelClickPoint.Value.Y, 2)) > 8)
            {
                _canDragToolPanel = true;
            }
        }
    }

    private void VisOptions_AbsoluteScaleChanged(object sender, double e)
    {
        var absScale = visVisOptions.IsUsingAbosluteScale ? (float)e : 0;
        dmsPlot1.AbsoluteScale = absScale;
        dmsPlot2.AbsoluteScale = absScale;
    }

    private void VisOptions_DiffScaleChanged(object sender, double e)
    {
        DisplayDmsDiff(dmsPlot1.Dms, dmsPlot2.Dms);
    }

    private void DmsPlot_DmsLoaded(object sender, Dms? e)
    {
        DisplayDmsDiff(dmsPlot1.Dms, dmsPlot2.Dms);
    }

    private void VisOptions_DmsThemeChanged(object sender, int e)
    {
        dmsPlot1.ThemeIndex = e;
        dmsPlot2.ThemeIndex = e;
    }

    private void VisOptions_DiffThemeChanged(object sender, int e)
    {
        _diffThemeIndex = e;
        _diffTheme = new PlotColors(Painter.DiffThemes[_diffThemeIndex]);
        DisplayDmsDiff(dmsPlot1.Dms, dmsPlot2.Dms);
    }

    private void Dms1CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (dmsPlot1.CopyToMemory())
        {
            FlickerElement(lblDms1Copied, 2000);
        }
    }

    private void Dms2CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (dmsPlot2.CopyToMemory())
        {
            FlickerElement(lblDms2Copied, 2000);
        }
    }
}