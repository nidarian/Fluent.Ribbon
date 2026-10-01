namespace Fluent.Theming;

using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ControlzEx.Theming;
using Fluent.Helpers.ColorHelpers;

/// <summary>
/// Provides theme resources from Fluent.Ribbon.
/// </summary>
public class RibbonLibraryThemeProvider : LibraryThemeProvider
{
    /// <summary>
    /// Gets the default instance of this class.
    /// </summary>
    public static readonly RibbonLibraryThemeProvider DefaultInstance = new();

    /// <inheritdoc cref="LibraryThemeProvider" />
    public RibbonLibraryThemeProvider()
        : base(true)
    {
    }

    /// <inheritdoc />
    /// <remarks>
    /// When <see cref="RuntimeThemeOptions.IsHighContrast"/> is <c>true</c> the accent color is ignored and the palette is taken from <see cref="SystemColors"/>,
    /// so the generated theme follows the active Windows High Contrast theme.
    /// ControlzEx generates such a theme when <see cref="ThemeSyncMode.SyncWithHighContrast"/> is part of <see cref="ThemeManager.ThemeSyncMode"/>
    /// and Windows High Contrast is on, for example:
    /// <code>
    /// ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithHighContrast;
    /// ThemeManager.Current.SyncTheme();
    /// </code>
    /// You can also generate one yourself with <c>RuntimeThemeGenerator.Current.GenerateRuntimeTheme(baseColor, accentColor, isHighContrast: true)</c>.
    /// The colors are read once, when the theme is generated.
    /// See .github/architecture/HIGH-CONTRAST.md for what is and isn't covered.
    /// </remarks>
    public override void FillColorSchemeValues(Dictionary<string, string> values, RuntimeThemeColorValues colorValues)
    {
        // Only themes generated with isHighContrast = true take this branch, every other theme keeps the accent based palette below.
        if (colorValues.Options is { IsHighContrast: true })
        {
            FillHighContrastColorSchemeValues(values);
            return;
        }

        values.Add("Fluent.Ribbon.Colors.AccentBase", colorValues.AccentColor.ToString());
        values.Add("Fluent.Ribbon.Colors.Accent80", colorValues.AccentColor80.ToString());
        values.Add("Fluent.Ribbon.Colors.Accent60", colorValues.AccentColor60.ToString());
        values.Add("Fluent.Ribbon.Colors.Accent40", colorValues.AccentColor40.ToString());
        values.Add("Fluent.Ribbon.Colors.Accent20", colorValues.AccentColor20.ToString());

        values.Add("Fluent.Ribbon.Colors.Highlight", colorValues.HighlightColor.ToString());
        values.Add("Fluent.Ribbon.Colors.IdealForeground", colorValues.IdealForegroundColor.ToString());

        var accentColor = colorValues.AccentBaseColor;
#pragma warning disable CS0618 // Type or member is obsolete
        var colorPalette = new ColorPalette(accentColor);
        this.UpdateAccentColors(values, accentColor,
            colorPalette.Palette[4], colorPalette.Palette[3], colorPalette.Palette[2],
            colorPalette.Palette[6], colorPalette.Palette[7], colorPalette.Palette[8]);
    }

    private void UpdateAccentColors(Dictionary<string, string> values, Color accentColor,
        ColorPaletteEntry accentColorLight1, ColorPaletteEntry accentColorLight2, ColorPaletteEntry accentColorLight3,
        ColorPaletteEntry accentColorDark1, ColorPaletteEntry accentColorDark2, ColorPaletteEntry accentColorDark3)
    {
        values["AccentLight1"] = accentColorLight1.Color.ToString(CultureInfo.InvariantCulture);
        values["AccentLight1.Foreground"] = accentColorLight1.ContrastColor.ToString(CultureInfo.InvariantCulture);
        values["AccentLight2"] = accentColorLight2.Color.ToString(CultureInfo.InvariantCulture);
        values["AccentLight2.Foreground"] = accentColorLight2.ContrastColor.ToString(CultureInfo.InvariantCulture);
        values["AccentLight3"] = accentColorLight3.Color.ToString(CultureInfo.InvariantCulture);
        values["AccentLight3.Foreground"] = accentColorLight3.ContrastColor.ToString(CultureInfo.InvariantCulture);
        values["AccentDark1"] = accentColorDark1.Color.ToString(CultureInfo.InvariantCulture);
        values["AccentDark1.Foreground"] = accentColorDark1.ContrastColor.ToString(CultureInfo.InvariantCulture);
        values["AccentDark2"] = accentColorDark2.Color.ToString(CultureInfo.InvariantCulture);
        values["AccentDark2.Foreground"] = accentColorDark2.ContrastColor.ToString(CultureInfo.InvariantCulture);
        values["AccentDark3"] = accentColorDark3.Color.ToString(CultureInfo.InvariantCulture);
        values["AccentDark3.Foreground"] = accentColorDark3.ContrastColor.ToString(CultureInfo.InvariantCulture);
    }
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Fills <paramref name="values"/> with colors from <see cref="SystemColors"/> for a High Contrast theme.
    /// </summary>
    private static void FillHighContrastColorSchemeValues(Dictionary<string, string> values)
    {
        // WHY these pairs: a Windows High Contrast theme only promises readable contrast for its own pairs,
        // e.g. WindowText on Window, HighlightText on Highlight and ControlText on Control (button face).
        // Fluent's templates draw normal text with "Black" and do not switch the text color on hover or selection,
        // so every surface that has normal text on it gets Window or Control (never Highlight),
        // and hover/checked states stay visible through borders (Gray*, Accent60, Highlight) instead.
        var windowText = ToColorString(SystemColors.WindowTextColor);
        var window = ToColorString(SystemColors.WindowColor);
        var control = ToColorString(SystemColors.ControlColor);
        var controlText = ToColorString(SystemColors.ControlTextColor);
        var grayText = ToColorString(SystemColors.GrayTextColor);
        var highlight = ToColorString(SystemColors.HighlightColor);
        var highlightText = ToColorString(SystemColors.HighlightTextColor);
        var hotTrack = ToColorString(SystemColors.HotTrackColor);

        // WHY this works for Light and Dark: ControlzEx replaces the {{placeholders}} in the theme template with these values first
        // and only then with the base color scheme and default values from GeneratorParameters.json, so a key set here wins.
        // That's why the final "Fluent.Ribbon.Colors.*" keys are set and not "AccentLight1" etc., which the "Dark" scheme swaps.

        // Text, borders and glyphs (all opaque, translucent colors would blend with the background and lose contrast)
        values["Fluent.Ribbon.Colors.Black"] = windowText;
        values["Fluent.Ribbon.Colors.Black20"] = grayText;
        values["Fluent.Ribbon.Colors.Gray1"] = windowText; // also KeyTip background, its text uses "White", so it gets inverted colors
        values["Fluent.Ribbon.Colors.Gray2"] = windowText;
        values["Fluent.Ribbon.Colors.Gray3"] = windowText; // also Gallery header background, its text uses "White"
        values["Fluent.Ribbon.Colors.Gray4"] = windowText;
        values["Fluent.Ribbon.Colors.Gray5"] = windowText;
        values["Fluent.Ribbon.Colors.Gray6"] = windowText;
        values["Fluent.Ribbon.Colors.Gray7"] = windowText;
        values["Fluent.Ribbon.Colors.DarkIdealForegroundDisabled"] = grayText;

        // Surfaces
        values["Fluent.Ribbon.Colors.White"] = window;
        values["Fluent.Ribbon.Colors.White20"] = window;
        values["Fluent.Ribbon.Colors.Gray8"] = control;
        values["Fluent.Ribbon.Colors.Gray9"] = control;
        values["Fluent.Ribbon.Colors.Gray10"] = control;

        // Accent: strong accents (backgrounds paired with IdealForeground, borders, underlines) use Highlight,
        // light tints are backgrounds behind normal text, so they use Control.
        values["Fluent.Ribbon.Colors.AccentBase"] = highlight;
        values["Fluent.Ribbon.Colors.Accent80"] = highlight;
        values["Fluent.Ribbon.Colors.Accent60"] = highlight;
        values["Fluent.Ribbon.Colors.Accent40"] = control;
        values["Fluent.Ribbon.Colors.Accent20"] = control;
        values["Fluent.Ribbon.Colors.Highlight"] = highlight;
        values["Fluent.Ribbon.Colors.IdealForeground"] = highlightText;

        values["Fluent.Ribbon.Colors.AccentLight1"] = control;
        values["Fluent.Ribbon.Colors.AccentLight1.Foreground"] = controlText;
        values["Fluent.Ribbon.Colors.AccentLight2"] = control;
        values["Fluent.Ribbon.Colors.AccentLight2.Foreground"] = controlText;
        values["Fluent.Ribbon.Colors.AccentLight3"] = control;
        values["Fluent.Ribbon.Colors.AccentLight3.Foreground"] = controlText;

        // AccentDark3 is used as text color (contextual tab headers) on the window background,
        // HotTrack (the hyperlink color) is the system color meant for colored text on Window.
        values["Fluent.Ribbon.Colors.AccentDark1"] = hotTrack;
        values["Fluent.Ribbon.Colors.AccentDark1.Foreground"] = window;
        values["Fluent.Ribbon.Colors.AccentDark2"] = hotTrack;
        values["Fluent.Ribbon.Colors.AccentDark2.Foreground"] = window;
        values["Fluent.Ribbon.Colors.AccentDark3"] = hotTrack;
        values["Fluent.Ribbon.Colors.AccentDark3.Foreground"] = window;

        // Brushes that get a color literal (not a palette color) from GeneratorParameters.json
        values["Fluent.Ribbon.Brushes.RibbonWindow.Background"] = window;
        values["Fluent.Ribbon.Brushes.RibbonWindow.Background.Backdrop.Acrylic"] = window;
        values["Fluent.Ribbon.Brushes.RibbonWindow.Background.Backdrop.Auto"] = window;
        values["Fluent.Ribbon.Brushes.RibbonTabControl.Content.Background"] = window;
        values["Fluent.Ribbon.Brushes.DropDown.Background"] = window;
        values["Fluent.Ribbon.Brushes.BackstageTabControl.Background"] = window;
        values["Fluent.Ribbon.Brushes.BackstageTabControl.ItemsPanelBackground"] = control;

        // The back button background is AccentBase (Highlight), so its text needs HighlightText.
        values["Fluent.Ribbon.Brushes.Backstage.BackButton.Foreground"] = highlightText;
    }

    private static string ToColorString(Color color)
    {
        return color.ToString(CultureInfo.InvariantCulture);
    }
}