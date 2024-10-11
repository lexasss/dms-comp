using DmsComparison;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace CompPair;

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
        }
    }

    public event EventHandler<Dms?>? DmsLoaded;
    public event PropertyChangedEventHandler? PropertyChanged;

    public DmsPlot()
    {
        InitializeComponent();
        DataContext = this;

        //RenderOptions.SetBitmapScalingMode(imgDms, BitmapScalingMode.NearestNeighbor);
        //RenderOptions.SetEdgeMode(imgDms, EdgeMode.Aliased);
    }

    // Internal

    Dms? _dms = null;

    double _absoluteScale = 400;

    private void DisplayDms()
    {
        if (_dms != null)
        {
            Painter.DrawPlot(imgDms, _dms.Height, _dms.Width, _dms.Data, (float)AbsoluteScale);
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
