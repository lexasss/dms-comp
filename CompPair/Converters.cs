using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DmsComparison;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (Visibility)value == Visibility.Visible;
    }
}

[ValueConversion(typeof(Data.ValueState), typeof(Brush))]
public class ValueStateToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
        (Data.ValueState)value switch {
            Data.ValueState.BelowRange => Brushes.Blue,
            Data.ValueState.AboveRange => Brushes.Red,
            _ => Brushes.Black
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack is not implemented for ValueStateToColorConverter.");
    }
}