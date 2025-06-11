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

    public float DataFilterFrom
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataFilterFrom)));
            DataFilterSettingsChanged?.Invoke(this, new DataFilterSettingsChangedEventArgs(GetFilterSettings()));
            Update(_dms1, _dms2);
        }
    }

    public float DataFilterTo
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataFilterTo)));
            DataFilterSettingsChanged?.Invoke(this, new DataFilterSettingsChangedEventArgs(GetFilterSettings()));
            Update(_dms1, _dms2);
        }
    }

    public Data.Limits DataFilterLimits
    {
        get => field;
        set
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataFilterLimits)));
            DataFilterSettingsChanged?.Invoke(this, new DataFilterSettingsChangedEventArgs(GetFilterSettings()));
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
    public class DataFilterSettingsChangedEventArgs(Data.FilterSettings filterSettings) : EventArgs
    {
        public Data.FilterSettings FilterSettings { get; } = filterSettings;
    }


    public event EventHandler<DataTypeChangedEventArgs>? DataTypeChanged;
    public event EventHandler<DataSourceChangedEventArgs>? DataSourceChanged;
    public event EventHandler<DataFilterChangedEventArgs>? DataFilterChanged;
    public event EventHandler<DataFilterSettingsChangedEventArgs>? DataFilterSettingsChanged;

    public Distance()
    {
        InitializeComponent();

        var settings = Properties.Settings.Default;
        _normalizationType = (NormalizationType)settings.DataProc_Normalization;
        ShouldRectify = settings.DataProc_Rectification;
        DataType = (Data.Type)settings.DataProc_DataType;
        DataSource = (Data.Source)settings.DataProc_DataSource;
        DataFilter = (Data.Filter)settings.DataProc_DataFilter;
        DataFilterFrom = settings.DataProc_FilterSettings_From;
        DataFilterTo = settings.DataProc_FilterSettings_To;
        DataFilterLimits = (Data.Limits)settings.DataProc_FilterSettings_LimitType;

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
            var data1 = DataService.GetRaw(dms1, DataType, DataSource, DataFilter, GetFilterSettings());
            var data2 = DataService.GetRaw(dms2, DataType, DataSource, DataFilter, GetFilterSettings());

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

    private Data.FilterSettings GetFilterSettings() => new(DataFilterFrom, DataFilterTo, DataFilterLimits);

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
        if (_algorithm == null)
            return;

        var settings = Properties.Settings.Default;
        settings.Alg_Name = _algorithm.Name;
        settings.DataProc_Normalization = (int)_normalizationType;
        settings.DataProc_Rectification = ShouldRectify;
        settings.DataProc_DataType = (int)DataType;
        settings.DataProc_DataSource = (int)DataSource;
        settings.DataProc_DataFilter = (int)DataFilter;
        settings.DataProc_FilterSettings_From = DataFilterFrom;
        settings.DataProc_FilterSettings_To = DataFilterTo;
        settings.DataProc_FilterSettings_LimitType = (int)DataFilterLimits;
        settings.Save();

        txbDistance.Text = "";

        if (_data1 == null || _data2 == null || _size == null)
            return;

        double result = _algorithm.ComputeDistance(_data1, _data2, _size, new Options(ShouldRectify, _normalizationType, false));

        txbDistance.Text = $"{result:F4}";
    }

    private void FilterSettings_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != System.Windows.Input.Key.Enter)
            return;

        var txb = sender as TextBox ?? throw new ArgumentNullException(nameof(sender));
        txb.MoveFocus(new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
    }
}
