using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DmsComparison.Data;

public enum Source
{
    Positive,
    Negative,
}

public enum Type
{
    Raw,
    Gradient
}

public enum Filter
{
    Unfiltered,
    Range
}

[TypeConverter(typeof(FriendlyEnumConverter))]
public enum Limits
{
    Absolute,
    RelativeToMinimum,
    RelativeToMedian,
    Percentage
}

public enum ValueState
{
    BelowRange,
    InsideRange,
    AboveRange
}

public record class FilterSettings(float? From, float? To, Limits Limits = Limits.Absolute)
{
    public static FilterSettings Default => new(null, null, Limits.Absolute);
}

public class FriendlyEnumConverter(System.Type type) : EnumConverter(type)
{
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, System.Type destinationType)
    {
        if (destinationType == typeof(string))
        {
            return value == null ? string.Empty :
                Regex.Replace(
                        value.ToString() ?? "",
                        "([A-Z])", " $1",
                        RegexOptions.Compiled
                    ).Trim();
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
