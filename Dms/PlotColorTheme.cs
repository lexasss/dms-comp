using System.Windows.Media;

namespace DmsComparison;

public class PlotColorTheme
{
    /// <summary>
    /// Creates a plot color. Origin and range are used to normalize value
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="value"></param>
    /// <param name="range"></param>
    /// <returns>Color</returns>
    public Color ValueToColor(double origin, double value, double range)
    {
        value = (value - origin) / range;
        var i = _levels.Skip(1).SkipLast(1).TakeWhile(level => value > level).Count();
        return Color.FromRgb(_r[i](value), _g[i](value), _b[i](value));
    }

    public PlotColorTheme(KeyValuePair<double, Color>[]? theme = null)
    {
        //         Colors: grey cyan green brown red white
        _r = MakeColorScale(240, 0, 0, 128, 128, 216);
        _g = MakeColorScale(240, 208, 176, 190, 0, 216);
        _b = MakeColorScale(240, 208, 0, 0, 0, 216);

        if (theme != null)
        {
            var levels = theme.Select(kv => kv.Key).ToArray();
            if (levels.Length < 2 ||
                levels.Aggregate(-100.0, (accum, v) => v > accum ? v : double.MaxValue) > 100)    // the final will be >1 if the order is not ascending
                return;

            _levels = levels.ToArray();
            _r = MakeColorScale(theme.Select(kv => kv.Value.R).ToArray());
            _g = MakeColorScale(theme.Select(kv => kv.Value.G).ToArray());
            _b = MakeColorScale(theme.Select(kv => kv.Value.B).ToArray());
        }
    }


    // Internal

    readonly double[] _levels = [0, 0.05, 0.2, 0.4, 0.7, 1];

    // RGB functions, +1 to the number of levels.
    readonly Func<double, byte>[] _r;
    readonly Func<double, byte>[] _g;
    readonly Func<double, byte>[] _b;

    // Helpers
    private (double, double) GetMinMax(int levelIndex)
    {
        var min = levelIndex < 0 ? _levels[0] : _levels[levelIndex];
        var max = levelIndex >= _levels.Length - 1 ? _levels[^1] : _levels[levelIndex + 1];
        return (min, max);
    }

    // Constant and transition functions for a custom scale, X..Y where 0 <= X,Y <= 255 and X < Y
    private static Func<double, byte> Keep(byte value) => (double _) => value;
    private Func<double, byte> Up(int levelIndex, byte from, byte to)
    {
        var (min, max) = GetMinMax(levelIndex);
        return (double value) => (byte)Math.Min(to, from + (to - from) * (value - min) * (1f / (max - min)));
    }
    private Func<double, byte> Down(int levelIndex, byte from, byte to)
    {
        var (min, max) = GetMinMax(levelIndex);
        return (double value) => (byte)Math.Min(from, to + (from - to) * (max - value) * (1f / (max - min)));
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
