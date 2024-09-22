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
    }

    // Internal

    Dms? _dms1 = null;
    Dms? _dms2 = null;

    private void LoadDmsFile(Action<Dms?> proceed)
    {
        string? filename = SelectDmsFile();
        if (!string.IsNullOrEmpty(filename))
        {
            var dms = Dms.Load(filename);
            proceed(dms);
        }
    }

    private string? SelectDmsFile()
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

        info.Content = dms?.Info;
    }

    private void DisplayDms(Canvas canvas, Dms dms)
    {
        Painter.DrawPlot(canvas, null, dms.Height, dms.Width, dms.Data);

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
}