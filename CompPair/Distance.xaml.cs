using DmsComparison.Algorithms;
using DmsComparison.Common;
using System.ComponentModel;
using System.Windows.Controls;

namespace DmsComparison;

public partial class Distance : UserControl, INotifyPropertyChanged
{
    public bool ShouldRectify
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShouldRectify)));
            Update();
        }
    }

    public Data.Type DataType
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataType)));
            DataTypeChanged?.Invoke(this, new DataTypeChangedEventArgs(value));
            Update(_dms1, _dms2);
        }
    }

    public Data.Source DataSource
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataSource)));
            DataSourceChanged?.Invoke(this, new DataSourceChangedEventArgs(value));
            Update(_dms1, _dms2);
        }
    }

    public Data.Filter DataFilter
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataFilter)));
            DataFilterChanged?.Invoke(this, new DataFilterChangedEventArgs(value));
            Update(_dms1, _dms2);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public class DataTypeChangedEventArgs(Data.Type type) : EventArgs
    {
        public Data.Type Type { get; } = type;
    }
    public class DataSourceChangedEventArgs(Data.Source source) : EventArgs
    {
        public Data.Source Source { get; } = source;
    }
    public class DataFilterChangedEventArgs(Data.Filter filter) : EventArgs
    {
        public Data.Filter Filter { get; } = filter;
    }

    public event EventHandler<DataTypeChangedEventArgs>? DataTypeChanged;
    public event EventHandler<DataSourceChangedEventArgs>? DataSourceChanged;
    public event EventHandler<DataFilterChangedEventArgs>? DataFilterChanged;

    public Distance()
    {
        InitializeComponent();
        DataContext = this;

        var settings = Properties.Settings.Default;
        _normalizationType = (NormalizationType)settings.DataProc_Normalization;
        ShouldRectify = settings.DataProc_Rectification;
        DataType = (Data.Type)settings.DataProc_DataType;
        DataSource = (Data.Source)settings.DataProc_DataSource;
        DataFilter = (Data.Filter)settings.DataProc_DataFilter;

        CreateUiListOfAlgorithms(settings.Alg_Name);
        CreateUiListOfNormalizations();
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
    NormalizationType _normalizationType;

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

        _algorithm ??= (Algorithm?)(stpAlgorithms.Children[0] as RadioButton)?.Tag;
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
        settings.DataProc_Rectification = ShouldRectify;
        settings.DataProc_DataType = (int)DataType;
        settings.DataProc_DataSource = (int)DataSource;
        settings.DataProc_DataFilter = (int)DataFilter;
        settings.Save();

        txbDistance.Text = "";

        if (_data1 == null || _data2 == null || _size == null || _algorithm == null)
        {
            return;
        }

        double result = _algorithm.ComputeDistance(_data1, _data2, _size, new Options(ShouldRectify, _normalizationType, false));

        txbDistance.Text = $"{result:F4}";
    }
}
