using System.Windows.Media;

namespace DmsComparison;

public class PlotColorTheme
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="theme">Optional theme (keys must be constantly increasing)</param>
    public PlotColorTheme(KeyValuePair<double, Color>[]? theme = null)
    {
        if (theme != null)
        {
            var levels = theme.Select(kv => kv.Key).ToArray();
            if (levels.Length < 2 ||
                levels.Aggregate(-100.0, (accum, v) => v > accum ? v : double.MaxValue) > 100)    // the final will be >1 if the order is not ascending
                goto defaults;

            _levels = levels.ToArray();
            _r = MakeColorScale(theme.Select(kv => kv.Value.R).ToArray());
            _g = MakeColorScale(theme.Select(kv => kv.Value.G).ToArray());
            _b = MakeColorScale(theme.Select(kv => kv.Value.B).ToArray());

            return;
        }

    defaults:
        // Default levels
        _levels = [0, 0.05, 0.2, 0.4, 0.7, 1];

        // Default colors: grey cyan green brown red white
        _r = MakeColorScale(240, 0, 0, 128, 128, 216);
        _g = MakeColorScale(240, 208, 176, 190, 0, 216);
        _b = MakeColorScale(240, 208, 0, 0, 0, 216);
    }

    /// <summary>
    /// Creates a plot color for a measurement value. Origin and range are used to normalize value
    /// </summary>
    /// <param name="value">The value for which a color must be computed</param>
    /// <param name="origin">Used to subtract from the value </param>
    /// <param name="range">The range the values stays within</param>
    /// <returns>The corresponding color</returns>
    public Color ValueToColor(double value, double origin, double range)
    {
        value = (value - origin) / range;
        var i = _levels.Skip(1).SkipLast(1).TakeWhile(level => value > level).Count();
        return Color.FromRgb(_r[i](value), _g[i](value), _b[i](value));
    }


    // Internal

    readonly double[] _levels;

    // RGB functions, each array is one element less than the number of levels.
    readonly Func<double, byte>[] _r;
    readonly Func<double, byte>[] _g;
    readonly Func<double, byte>[] _b;

    // Constant and transition functions for a custom scale, X..Y where 0 <= X,Y <= 255 and X < Y
    private static Func<double, byte> Keep(byte value) => (double _) => value;
    private Func<double, byte> Up(int levelIndex, byte from, byte to)
    {
        var min = _levels[levelIndex];
        var max = _levels[levelIndex + 1];
        return (double value) => (byte)Math.Max(from, Math.Min(to, from + (to - from) * (value - min) / (max - min)));
    }
    private Func<double, byte> Down(int levelIndex, byte from, byte to)
    {
        var min = _levels[levelIndex];
        var max = _levels[levelIndex + 1];
        return (double value) => (byte)Math.Max(to, Math.Min(from, to + (from - to) * (max - value) / (max - min)));
    }

    private Func<double, byte>[] MakeColorScale(params byte[] colorValues)
    {
        var result = new List<Func<double, byte>>();

        byte prevColorValue = colorValues[0];
        int levelIndex = 0;

        foreach (var newColorValue in colorValues.Skip(1))
        {
            if (prevColorValue < newColorValue)
                result.Add(Up(levelIndex, prevColorValue, newColorValue));
            else if (prevColorValue > newColorValue)
                result.Add(Down(levelIndex, prevColorValue, newColorValue));
            else
                result.Add(Keep(newColorValue));

            prevColorValue = newColorValue;
            levelIndex++;
        }

        return result.ToArray();
    }
}
