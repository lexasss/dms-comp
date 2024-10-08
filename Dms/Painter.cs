﻿using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DmsComparison;

/// <summary>
/// Draw DMS plot onto a canvas
/// </summary>
public static class Painter
{
    // Target: Canvas

    /// <summary>
    /// Plots difference between two DMS measurements into Canvas
    /// </summary>
    /// <param name="canvas">The canvas to draw the plot onto</param>
    /// <param name="rows">Number of rows (same for both datasets)</param>
    /// <param name="cols">Number of columns (same for both datasets)</param>
    /// <param name="data1">Dataset 1</param>
    /// <param name="data2">Dataset 2</param>
    /// <param name="theme">Optional color theme</param>
    public static void DrawDiff(Canvas canvas, int rows, int cols, float[] data1, float[] data2, PlotColorTheme? theme = null)
    {
        float[] values = new float[data1.Length];
        for (int i = 0; i < data1.Length; i++)
            values[i] = data1[i] - data2[i];

        theme ??= new(_defaultDiffTheme);

        float range = (data1.Max() + data2.Max()) / 2 - (data1.Min() + data2.Min()) / 2;
        float origin = 0;

        Draw(canvas, rows, cols, values, range, origin, theme);
    }

    /// <summary>
    /// Plots a DMS measurement into Canvas
    /// </summary>
    /// <param name="canvas">The canvas to draw the plot onto</param>
    /// <param name="rows">Number of rows (same for both datasets)</param>
    /// <param name="cols">Number of columns (same for both datasets)</param>
    /// <param name="data">Dataset</param>
    /// <param name="saturationValue">Value at which the color reaches its most saturated value (last color in the color theme).
    /// Must be greater than 0, or 0 if this value is the max value from the dataset</param>
    /// <param name="theme">Optional color theme</param>
    public static void DrawPlot(Canvas canvas, int rows, int cols, float[] data, float saturationValue = 0, PlotColorTheme? theme = null)
    {
        var minValue = data.Min();
        var maxValue = saturationValue > 0 ? saturationValue : data.Max();

        float range = maxValue - minValue;
        float origin = minValue;

        Draw(canvas, rows, cols, data, range, origin, theme);
    }

    /// <summary>
    /// Plots a DMS measurement into Canvas
    /// </summary>
    /// <param name="canvas">The canvas to draw the plot onto</param>
    /// <param name="rows">Number of rows (same for both datasets)</param>
    /// <param name="cols">Number of columns (same for both datasets)</param>
    /// <param name="data">Dataset</param>
    /// <param name="range">Range of the dataset values</param>
    /// <param name="origin">The values to be subtracted from the dataset values</param>
    /// <param name="theme">Optional color theme</param>
    public static void Draw(Canvas canvas, int rows, int cols, float[] data, float range, float origin, PlotColorTheme? theme = null)
    {
        canvas.Children.Clear();

        double width = canvas.ActualWidth;
        double height = canvas.ActualHeight;

        double colSize = width / cols;
        double rowSize = height / rows;

        double cellWidth = Math.Ceiling(colSize);
        double cellHeight = Math.Ceiling(rowSize);

        theme ??= new(_defaultDmsTheme);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var value = data[y * cols + x];
                var pixel = new Rectangle()
                {
                    Width = cellWidth,
                    Height = cellHeight,
                    Fill = new SolidColorBrush(theme.ValueToColor(value, origin, range)),
                };
                canvas.Children.Add(pixel);
                Canvas.SetLeft(pixel, (int)(x * colSize));
                Canvas.SetTop(pixel, (int)(height - (y + 1) * rowSize));
            }
        }
    }

    // Target: Image

    /// <summary>
    /// Plots difference between two DMS measurements into Canvas
    /// </summary>
    /// <param name="canvas">The canvas to draw the plot onto</param>
    /// <param name="rows">Number of rows (same for both datasets)</param>
    /// <param name="cols">Number of columns (same for both datasets)</param>
    /// <param name="data1">Dataset 1</param>
    /// <param name="data2">Dataset 2</param>
    /// <param name="theme">Optional color theme</param>
    public static void DrawDiff(Image image, int rows, int cols, float[] data1, float[] data2, PlotColorTheme? theme = null)
    {
        float[] values = new float[data1.Length];
        for (int i = 0; i < data1.Length; i++)
            values[i] = data1[i] - data2[i];

        theme ??= new(_defaultDiffTheme);

        float range = (data1.Max() + data2.Max()) / 2 - (data1.Min() + data2.Min()) / 2;
        float origin = 0;

        Draw(image, rows, cols, values, range, origin, theme);
    }

    /// <summary>
    /// Plots a DMS measurement into the WriteableBitmapEx
    /// </summary>
    /// <param name="image">The image to draw the plot into</param>
    /// <param name="rows">Number of rows (same for both datasets)</param>
    /// <param name="cols">Number of columns (same for both datasets)</param>
    /// <param name="data">Dataset</param>
    /// <param name="saturationValue">Value at which the color reaches its most saturated value (last color in the color theme).
    /// Must be greater than 0, or 0 if this value is the max value from the dataset</param>
    /// <param name="theme">Optional color theme</param>
    public static void DrawPlot(Image image, int rows, int cols, float[] data, float saturationValue = 0, PlotColorTheme? theme = null)
    {
        var minValue = data.Min();
        var maxValue = saturationValue > 0 ? saturationValue : data.Max();

        float range = maxValue - minValue;
        float origin = minValue;

        Draw(image, rows, cols, data, range, origin, theme);
    }


    /// <summary>
    /// Plots a DMS measurement into Canvas
    /// </summary>
    /// <param name="image">The image to draw the plot into</param>
    /// <param name="rows">Number of rows (same for both datasets)</param>
    /// <param name="cols">Number of columns (same for both datasets)</param>
    /// <param name="data">Dataset</param>
    /// <param name="range">Range of the dataset values</param>
    /// <param name="origin">The values to be subtracted from the dataset values</param>
    /// <param name="theme">Optional color theme</param>
    public static void Draw(Image image, int rows, int cols, float[] data, float range, float origin, PlotColorTheme? theme = null)
    {
        image.Source = null;

        theme ??= new(_defaultDmsTheme);

        byte[] pixels = new byte[cols * rows * 3];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var value = data[y * cols + x];
                var color = theme.ValueToColor(value, origin, range);
                var row = rows - y - 1;
                pixels[3 * row * cols + 3 * x] = color.R;
                pixels[3 * row * cols + 3 * x + 1] = color.G;
                pixels[3 * row * cols + 3 * x + 2] = color.B;
            }
        }

        var writeableBmp = new WriteableBitmap(cols, rows, 96, 96, PixelFormats.Rgb24, null);
        writeableBmp.WritePixels(new(0, 0, cols, rows), pixels, cols * 3, 0);
        image.Source = writeableBmp;
    }

    // Internal

    static KeyValuePair<double, Color>[] _defaultDiffTheme = [
        new(-1, Colors.Blue),
        //new(-0.3, Colors.Black),
        new(0, Colors.White),
        //new(0.3, Colors.Black),
        new(1, Colors.Red),
    ];

    static KeyValuePair<double, Color>[] _defaultDmsTheme = [
        new(0, Color.FromRgb(240, 240, 240)),    // white
        new(0.03, Color.FromRgb(0, 208, 208)),   // cyan
        new(0.2, Color.FromRgb(0, 176, 0)),      // green
        new(0.4, Color.FromRgb(128, 190, 0)),    // brown
        new(0.7, Color.FromRgb(128, 0, 0)),      // red
        new(1, Color.FromRgb(216, 216, 216)),    // whitish
    ];
}
