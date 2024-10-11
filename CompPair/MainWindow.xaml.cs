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

        //RenderOptions.SetBitmapScalingMode(imgDms1, BitmapScalingMode.NearestNeighbor);
        //RenderOptions.SetBitmapScalingMode(imgDms2, BitmapScalingMode.NearestNeighbor);
        //RenderOptions.SetEdgeMode(imgDms1, EdgeMode.Aliased);
        //RenderOptions.SetEdgeMode(imgDms2, EdgeMode.Aliased);

        var settings = Properties.Settings.Default;
        stpTools.HorizontalAlignment = (HorizontalAlignment)settings.UI_ToolPanel_HorzAlign;
        stpTools.VerticalAlignment = (VerticalAlignment)settings.UI_ToolPanel_VertAlign;
    }

    // Internal

    Dms? _dms1 = null;
    Dms? _dms2 = null;

    bool _isToolPanelDragging = false;
    bool _canDragToolPanel = false;
    Point? _toolPanelClickPoint = null;

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
        var scale = visVisOptions.IsUsingAbosluteScale ? visVisOptions.AbsoluteScale : 0;
        Painter.DrawPlot(image, dms.Height, dms.Width, dms.Data, (float)scale);

        if (_dms1 != null && _dms2 != null)
        {
            DisplayDmsDiff(imgDmsDiff, _dms1, _dms2);
        }
    }

    private void DisplayDmsDiff(Image image, Dms dms1, Dms dms2)
    {
        if (dms1.Height == dms2.Height && dms1.Width == dms2.Width)
        {
            Painter.DrawDiff(image, dms1.Height, dms1.Width, dms1.Data, dms2.Data, (float)visVisOptions.DiffScale);
            dstDistance.Update(dms1, dms2);
        }
        else
        {
            Painter.Clear(image);
            dstDistance.Clear();
            MessageBox.Show("DMS data have distinct number of rows or colunms and therefore their difference cannot be displayed.",
                "DMS loader", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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

    private void Dms1Prompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Loader.LoadDmsFile(dms =>
        {
            _dms1 = dms;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDms1Ready)));
            UpdateDmsUI(_dms1, imgDms1, lblDms1);
        });
    }

    private void Dms2Prompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Loader.LoadDmsFile(dms =>
        {
            _dms2 = dms;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDms2Ready)));
            UpdateDmsUI(_dms2, imgDms2, lblDms2);
        });
    }

    private void DiffPrompt_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Loader.LoadTwoDmsFiles(
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

    private void ToolPanel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _isToolPanelDragging = true;

        _toolPanelClickPoint = e.GetPosition(stpTools);

        stpTools.Cursor = Cursors.Hand;
        stpTools.CaptureMouse();
    }

    private void ToolPanel_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _isToolPanelDragging = false;
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
        var scale = visVisOptions.IsUsingAbosluteScale ? (float)e : 0;
        if (_dms1 != null)
            Painter.DrawPlot(imgDms1, _dms1.Height, _dms1.Width, _dms1.Data, scale);
        if (_dms2 != null)
            Painter.DrawPlot(imgDms2, _dms2.Height, _dms2.Width, _dms2.Data, scale);
    }

    private void VisOptions_DiffScaleChanged(object sender, double e)
    {
        if (_dms1 != null && _dms2 != null && _dms1.Height == _dms2.Height && _dms1.Width == _dms2.Width)
        {
            Painter.DrawDiff(imgDmsDiff, _dms1.Height, _dms1.Width, _dms1.Data, _dms2.Data, (float)e);
        }
    }
}