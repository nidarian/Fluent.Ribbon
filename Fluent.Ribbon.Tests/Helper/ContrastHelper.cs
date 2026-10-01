namespace Fluent.Tests.Helper;

using System;
using System.Windows.Media;

/// <summary>
/// WCAG 2.x contrast ratio, as defined in https://www.w3.org/TR/WCAG21/#dfn-contrast-ratio .
/// </summary>
public static class ContrastHelper
{
    /// <summary>
    /// Gets the contrast ratio (1 to 21) of <paramref name="foreground"/> drawn on the opaque <paramref name="background"/>.
    /// </summary>
    public static double GetContrastRatio(Color foreground, Color background)
    {
        // The ratio is only defined for opaque colors, so a translucent foreground is first blended over the background.
        var opaqueForeground = Blend(foreground, background, 1.0);

        var foregroundLuminance = GetRelativeLuminance(opaqueForeground);
        var backgroundLuminance = GetRelativeLuminance(background);

        var lighter = Math.Max(foregroundLuminance, backgroundLuminance);
        var darker = Math.Min(foregroundLuminance, backgroundLuminance);

        // The 0.05 stands for the flare of a real screen, which keeps black on black at 1:1 instead of 0/0.
        return (lighter + 0.05) / (darker + 0.05);
    }

    /// <summary>
    /// Gets the opaque color seen when <paramref name="foreground"/> is drawn with <paramref name="opacity"/> over <paramref name="background"/>.
    /// </summary>
    public static Color Blend(Color foreground, Color background, double opacity)
    {
        var alpha = (foreground.A / 255.0) * opacity;

        return Color.FromRgb(
            BlendChannel(foreground.R, background.R, alpha),
            BlendChannel(foreground.G, background.G, alpha),
            BlendChannel(foreground.B, background.B, alpha));
    }

    /// <summary>
    /// Gets the relative luminance (0 for black, 1 for white) of an sRGB color.
    /// </summary>
    public static double GetRelativeLuminance(Color color)
    {
        // Green weighs most because the eye is most sensitive to it.
        return (0.2126 * Linearize(color.R)) + (0.7152 * Linearize(color.G)) + (0.0722 * Linearize(color.B));
    }

    private static byte BlendChannel(byte foreground, byte background, double alpha)
    {
        return (byte)Math.Round((foreground * alpha) + (background * (1 - alpha)));
    }

    private static double Linearize(byte channel)
    {
        // Undo the sRGB gamma curve, so that the luminance is proportional to light.
        var value = channel / 255.0;

        return value <= 0.03928
            ? value / 12.92
            : Math.Pow((value + 0.055) / 1.055, 2.4);
    }
}