using DmsComparison;
using DmsComparison.Algorithms;
using System.ComponentModel;
using System.Windows.Controls;

namespace CompPair;

public partial class Distance : UserControl, INotifyPropertyChanged
{
    public bool ShouldRectify
    {
        get => _shouldRectify;
        set
        {
            _shouldRectify = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShouldRectify)));
            Update();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Distance()
    {
        InitializeComponent();
        DataContext = this;

        // Creates dynamically a list of radio button in the UI

        var algorithmTypes = Algorithm.GetDescendantTypes();
        foreach (var algorithmType in algorithmTypes)
        {
            var algorithm = Activator.CreateInstance(algorithmType) as Algorithm;
            if (algorithm == null)
            {
                continue;
            }

            if (_algorithm == null)
            {
                _algorithm = algorithm;
            }

            var rdb = new RadioButton()
            {
                GroupName = "Algorithm",
                Content = algorithm.Name,
                IsChecked = algorithm == _algorithm,
                Tag = algorithm
            };
            rdb.Click += (s, e) =>
            {
                var algorithm = (Algorithm?)(s as RadioButton)?.Tag;
                if (algorithm != _algorithm)
                {
                    _algorithm = algorithm;
                    Update();
                }
            };

            stpAlgorithms.Children.Add(rdb);
        }
    }

    public void Clear()
    {
        _data1 = null;
        _data2 = null;

        Update();
    }

    public void Update(Dms dms1, Dms dms2)
    {
        _data1 = dms1.Data;
        _data2 = dms2.Data;

        Update();
    }

    // Internal

    Algorithm? _algorithm = null;
    bool _shouldRectify = false;

    float[]? _data1 = null;
    float[]? _data2 = null;

    private void Update()
    {
        lblDistance.Content = null;

        if (_data1 == null || _data2 == null || _algorithm == null)
        {
            return;
        }

        double result = _algorithm.ComputeDistance(_data1, _data2, _shouldRectify);

        lblDistance.Content = $"{result:F3}";
    }
}
