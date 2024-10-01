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

        CreateUiListOfAlgorithms();
        CreateUiListOfNormalizations();
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
    Normalization.Type _normalizationType = Normalization.Type.None;

    float[]? _data1 = null;
    float[]? _data2 = null;

    private void CreateUiListOfAlgorithms()
    {
        var algorithmTypes = Algorithm.GetDescendantTypes();
        foreach (var algorithmType in algorithmTypes)
        {
            var algorithm = Activator.CreateInstance(algorithmType) as Algorithm;
            if (algorithm == null || !algorithm.IsVisible)
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

    private void CreateUiListOfNormalizations()
    {
        var normalizationTypes = Enum.GetValues<Normalization.Type>();
        foreach (var normalizationType in normalizationTypes)
        {
            var rdb = new RadioButton()
            {
                GroupName = "Normalization",
                Content = normalizationType,
                IsChecked = normalizationType == _normalizationType,
                Tag = normalizationType
            };
            rdb.Click += (s, e) =>
            {
                var normalizationType = (Normalization.Type?)(s as RadioButton)?.Tag;
                if (normalizationType != null && normalizationType != _normalizationType)
                {
                    _normalizationType = (Normalization.Type)normalizationType;
                    Update();
                }
            };

            stpNormalizationType.Children.Add(rdb);
        }
    }

    private void Update()
    {
        txbDistance.Text = "";

        if (_data1 == null || _data2 == null || _algorithm == null)
        {
            return;
        }

        double result = _algorithm.ComputeDistance(_data1, _data2, _shouldRectify, _normalizationType);

        txbDistance.Text = $"{result:F3}";
    }
}
