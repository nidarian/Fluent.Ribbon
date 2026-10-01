namespace Fluent.Helpers;

using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

/// <summary>
/// Helper class for drop downs.
/// </summary>
public static class DropDownHelper
{
    /// <summary>
    /// Coerces the maximum drop down height.
    /// </summary>
    public static object? CoerceMaxDropDownHeight(DependencyObject d, object? baseValue)
    {
        return baseValue is not double value
            ? baseValue
            : GetMaxDropDownHeight(d, value);
    }

    /// <summary>
    /// Gets the maximum drop down height.
    /// </summary>
    public static double GetMaxDropDownHeight(DependencyObject d, double baseValue)
    {
        if (double.IsNaN(baseValue) is false)
        {
            return baseValue;
        }

        if (Window.GetWindow(d) is not { } window)
        {
            return double.NaN;
        }

        // WinForms reports the working area in physical pixels, but WPF sizes are device independent units.
        // Without converting, the drop down would be too high on displays with more than 100% scaling.
        var workingAreaHeightInPixels = Screen.FromHandle(new WindowInteropHelper(window).Handle).WorkingArea.Height;
        var workingAreaHeight = workingAreaHeightInPixels / VisualTreeHelper.GetDpi(window).DpiScaleY;
        return Math.Floor(workingAreaHeight / 3D);
    }
}