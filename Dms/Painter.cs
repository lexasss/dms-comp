using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace DmsComparison;

public static class Painter
{
    public static void DrawDiff(Canvas canvas, int rows, int cols, float[] values1, float[] values2)
    {
        float[] values = new float[values1.Length];
        for (int i = 0; i < values1.Length; i++)
            values[i] = values1[i] - values2[i];

        PlotColorTheme theme = new(_diffThemeDefinition.ToArray());

        float range = (values1.Max() + values2.Max()) / 2 - (values1.Min() + values2.Min()) / 2;
        float origin = 0;

        Draw(canvas, theme, rows, cols, values, range, origin);
    }

    public static void DrawPlot(Canvas canvas, PlotColorTheme? theme, int rows, int cols, float[] values)
    {
        var minValue = values.Min();
        var maxValue = values.Max();

        float range = maxValue - minValue;
        float origin = minValue;

        Draw(canvas, theme, rows, cols, values, range, origin);
    }

    public static void Draw(Canvas canvas, PlotColorTheme? theme, int rows, int cols, float[] values, float range, float origin)
    {
        canvas.Children.Clear();

        double width = canvas.ActualWidth;
        double height = canvas.ActualHeight;

        double colSize = width / cols;
        double rowSize = height / rows;

        double cellWidth = Math.Ceiling(colSize);
        double cellHeight = Math.Ceiling(rowSize);

        theme ??= new(_dmsThemeDefinition.ToArray());

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var value = values[y * cols + x];
                var pixel = new Rectangle()
                {
                    Width = cellWidth,
                    Height = cellHeight,
                    Fill = new SolidColorBrush(theme.ValueToColor(origin, value, range)),
                };
                canvas.Children.Add(pixel);
                Canvas.SetLeft(pixel, (int)(x * colSize));
                Canvas.SetTop(pixel, (int)(height - (y + 1) * rowSize));
            }
        }
    }

    // Internal

    static Dictionary<double, Color> _diffThemeDefinition = new() {
        { -1, Colors.Blue },
        { -0.3, Colors.Black },
        { 0, Colors.White },
        { 0.3, Colors.Black },
        { 1, Colors.Red },
        /*{ 0, Colors.White },
        { 0.03, Color.FromRgb(128, 128, 128) },
        { 0.25, Color.FromRgb(64, 64, 64) },
        { 0.5, Colors.Black },
        { 0.75, Color.FromRgb(64, 64, 64) },
        { 0.97, Color.FromRgb(128, 128, 128) },
        { 1, Colors.White },
        */
    };

    static Dictionary<double, Color> _dmsThemeDefinition = new() {
        { 0, Color.FromRgb(240, 240, 240) },    // white
        { 0.03, Color.FromRgb(0, 208, 208) },   // cyan
        { 0.2, Color.FromRgb(0, 176, 0) },      // green
        { 0.4, Color.FromRgb(128, 190, 0) },    // brown
        { 0.7, Color.FromRgb(128, 0, 0) },      // red
        { 1, Color.FromRgb(216, 216, 216) },    // whitish
    };
}
