using DmsComparison.Algorithms;
using DmsComparison.Common;
using System.ComponentModel;
using System.Windows.Controls;

namespace DmsComparison;

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

    public Data.Type DataType
    {
        get => _dataType;
        set
        {
            _dataType = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataType)));
            DataTypeChanged?.Invoke(this, _dataType);
            Update(_dms1, _dms2);
        }
    }

    public Data.Source DataSource
    {
        get => _dataSource;
        set
        {
            _dataSource = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataSource)));
            DataSourceChanged?.Invoke(this, _dataSource);
            Update(_dms1, _dms2);
        }
    }

    public Data.Filter DataFilter
    {
        get => _dataFilter;
        set
        {
            _dataFilter = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataFilter)));
            DataFilterChanged?.Invoke(this, _dataFilter);
            Update(_dms1, _dms2);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<Data.Type>? DataTypeChanged;
    public event EventHandler<Data.Source>? DataSourceChanged;
    public event EventHandler<Data.Filter>? DataFilterChanged;

    public Distance()
    {
        InitializeComponent();
        DataContext = this;

        var settings = Properties.Settings.Default;
        _normalizationType = (NormalizationType)settings.DataProc_Normalization;
        _shouldRectify = settings.DataProc_Rectification;
        _dataType = (Data.Type)settings.DataProc_DataType;
        _dataSource = (Data.Source)settings.DataProc_DataSource;
        _dataFilter = (Data.Filter)settings.DataProc_DataFilter;

        CreateUiListOfAlgorithms(settings.Alg_Name);
        CreateUiListOfNormalizations();

        chkRectify.IsChecked = _shouldRectify;
    }

    public void Clear()
    {
        _data1 = null;
        _data2 = null;

        Update();
    }

    public void Update(Dms? dms1, Dms? dms2)
    {
        _dms1 = dms1;
        _dms2 = dms2;

        if (dms1 != null && dms2 != null && DataService.IsSameShape(dms1, dms2))
        {
            var data1 = DataService.GetRaw(dms1, DataType, DataFilter, DataSource);
            var data2 = DataService.GetRaw(dms2, DataType, DataFilter, DataSource);

            _data1 = data1.Values;
            _data2 = data2.Values;
            _size = new Size(data1.Columns, data1.Rows);

            Update();
        }
    }

    // Internal

    Algorithm? _algorithm = null;
    bool _shouldRectify;
    NormalizationType _normalizationType;
    Data.Type _dataType;
    Data.Source _dataSource;
    Data.Filter _dataFilter;

    Dms? _dms1 = null;
    Dms? _dms2 = null;

    float[]? _data1 = null;
    float[]? _data2 = null;
    Size? _size = null;

    private void CreateUiListOfAlgorithms(string selectedAlgorithm)
    {
        var algorithmTypes = Algorithm.GetDescendantTypes();
        foreach (var algorithmType in algorithmTypes)
        {
            if (Activator.CreateInstance(algorithmType) is not Algorithm algorithm || !algorithm.IsVisible)
            {
                continue;
            }

            if (algorithm.Name == selectedAlgorithm)
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

        if (_algorithm == null)
        {
            _algorithm = (Algorithm?)(stpAlgorithms.Children[0] as RadioButton)?.Tag;
        }
    }

    private void CreateUiListOfNormalizations()
    {
        var normalizationTypes = Enum.GetValues<NormalizationType>();
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
                var normalizationType = (NormalizationType?)(s as RadioButton)?.Tag;
                if (normalizationType != null && normalizationType != _normalizationType)
                {
                    _normalizationType = (NormalizationType)normalizationType;
                    Update();
                }
            };

            stpNormalizationType.Children.Add(rdb);
        }
    }

    private void Update()
    {
        var settings = Properties.Settings.Default;
        settings.Alg_Name = _algorithm?.Name ?? "";
        settings.DataProc_Normalization = (int)_normalizationType;
        settings.DataProc_Rectification = _shouldRectify;
        settings.DataProc_DataType = (int)_dataType;
        settings.DataProc_DataSource = (int)_dataSource;
        settings.DataProc_DataFilter = (int)_dataFilter;
        settings.Save();

        txbDistance.Text = "";

        if (_data1 == null || _data2 == null || _size == null || _algorithm == null)
        {
            return;
        }

        double result = _algorithm.ComputeDistance(_data1, _data2, _size, new Options(_shouldRectify, _normalizationType, false));

        txbDistance.Text = $"{result:F4}";
    }
}
