namespace Fluent.Tests.Themes;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Fluent.Tests.Helper;
using NUnit.Framework;

/// <summary>
/// Checks WCAG 2.x contrast ratios in the generated themes ("Themes/Themes/{BaseColorScheme}.{ColorScheme}.xaml").
/// Those files are generated at build time from Theme.Template.xaml and GeneratorParameters.json,
/// so these tests load the real, generated resource dictionaries for every accent in Light and Dark.
/// </summary>
/// <remarks>
/// Some checks look up a brush key that the contrast fix introduced.
/// When that key is missing, they use the brush the control templates used before the fix,
/// so that an old theme fails for the contrast reason and not for a missing resource.
/// </remarks>
[TestFixture]
public class ThemeContrastTests
{
    // WCAG 1.4.11 (non-text contrast): borders and state indicators need 3:1 against the colors next to them.
    private const double NonTextMinimum = 3.0;

    // WCAG 1.4.3 (text contrast): normal size text needs 4.5:1.
    private const double TextMinimum = 4.5;

    private const string BrushPrefix = "Fluent.Ribbon.Brushes.";

    private static readonly Dictionary<string, ResourceDictionary> themeCache = new();

    // Every "ColorSchemes" entry of GeneratorParameters.json.
    private static readonly string[] colorSchemes =
    {
        "Amber", "Blue", "Brown", "Cobalt", "Crimson", "Cyan", "Emerald", "Green", "Indigo", "Lime", "Magenta", "Mauve",
        "Olive", "Orange", "Pink", "Purple", "Red", "Sienna", "Steel", "Taupe", "Teal", "Violet", "Yellow"
    };

    private static IEnumerable<string> AllThemes()
    {
        foreach (var baseColorScheme in new[] { "Light", "Dark" })
        {
            foreach (var colorScheme in colorSchemes)
            {
                yield return $"{baseColorScheme}.{colorScheme}";
            }
        }
    }

    [TestCaseSource(nameof(AllThemes))]
    public void TextBox_Border_Has_NonText_Contrast(string themeName)
    {
        var theme = GetTheme(themeName);

        // The border is the only thing that shows where an empty text box is.
        AssertContrast(themeName, "TextBox.Border on TextBox.Background", GetBrushColor(theme, "TextBox.Border"), GetBrushColor(theme, "TextBox.Background"), NonTextMinimum);
        AssertContrast(themeName, "TextBox.Border on the ribbon background", GetBrushColor(theme, "TextBox.Border"), GetBrushColor(theme, "RibbonTabControl.Content.Background"), NonTextMinimum);
    }

    [TestCaseSource(nameof(AllThemes))]
    public void TextBox_Focus_Border_Has_NonText_Contrast(string themeName)
    {
        var theme = GetTheme(themeName);

        // Keyboard focus in a text box is shown only by the border color.
        AssertContrast(themeName, "TextBox.Focus.Border on TextBox.Focus.Background", GetBrushColor(theme, "TextBox.Focus.Border"), GetBrushColor(theme, "TextBox.Focus.Background"), NonTextMinimum);
        AssertContrast(themeName, "TextBox.Focus.Border on the ribbon background", GetBrushColor(theme, "TextBox.Focus.Border"), GetBrushColor(theme, "RibbonTabControl.Content.Background"), NonTextMinimum);
    }

    [TestCaseSource(nameof(AllThemes))]
    public void TextBox_Placeholder_Has_Text_Contrast(string themeName)
    {
        var theme = GetTheme(themeName);

        // The placeholder (PART_Watermark of the simplified text box) is the text box foreground (LabelText) drawn with reduced opacity.
        var background = GetBrushColor(theme, "TextBox.Background");
        var placeholder = ContrastHelper.Blend(GetBrushColor(theme, "LabelText"), background, GetSimplifiedTextBoxWatermarkOpacity());

        AssertContrast(themeName, "placeholder text on TextBox.Background", placeholder, background, TextMinimum);
    }

    [TestCaseSource(nameof(AllThemes))]
    public void ToggleButton_Checked_Border_Has_NonText_Contrast(string themeName)
    {
        var theme = GetTheme(themeName);

        AssertContrast(themeName, "ToggleButton.Checked.Border on the ribbon background", GetBrushColor(theme, "ToggleButton.Checked.Border"), GetBrushColor(theme, "RibbonTabControl.Content.Background"), NonTextMinimum);
    }

    [TestCaseSource(nameof(AllThemes))]
    public void BackstageTabItem_Selected_Indicator_Has_NonText_Contrast(string themeName)
    {
        var theme = GetTheme(themeName);

        // The selected backstage tab is marked only by a colored bar on its left side.
        AssertContrast(themeName, "BackstageTabItem.Selected.Background on BackstageTabControl.ItemsPanelBackground", GetBrushColor(theme, "BackstageTabItem.Selected.Background"), GetBrushColor(theme, "BackstageTabControl.ItemsPanelBackground"), NonTextMinimum);
    }

    [TestCaseSource(nameof(AllThemes))]
    public void CheckBox_CheckMark_Has_NonText_Contrast(string themeName)
    {
        var theme = GetTheme(themeName);

        // Before the fix, the check mark was drawn with AccentBase.
        var checkMark = GetBrushColor(theme, "CheckBox.CheckMark", "AccentBase");

        // Check boxes are placed on the ribbon and in drop downs (menus).
        AssertContrast(themeName, "check mark on the ribbon background", checkMark, GetBrushColor(theme, "RibbonTabControl.Content.Background"), NonTextMinimum);
        AssertContrast(themeName, "check mark on DropDown.Background", checkMark, GetBrushColor(theme, "DropDown.Background"), NonTextMinimum);
    }

    [TestCaseSource(nameof(AllThemes))]
    public void Gallery_Header_Text_Has_Text_Contrast(string themeName)
    {
        var theme = GetTheme(themeName);

        // Before the fix, the filter header text was "Brushes.White" and turned "Brushes.ExtremeHighlight" on mouse over, on an unchanged background.
        var background = GetBrushColor(theme, "Gallery.Header.Background");
        AssertContrast(themeName, "gallery filter header text", GetBrushColor(theme, "Gallery.Header.Foreground", "White"), background, TextMinimum);

        var mouseOverBackground = GetBrushColor(theme, "Gallery.Header.MouseOver.Background", "Gallery.Header.Background");
        AssertContrast(themeName, "gallery filter header text on mouse over", GetBrushColor(theme, "Gallery.Header.MouseOver.Foreground", "ExtremeHighlight"), mouseOverBackground, TextMinimum);
    }

    [TestCaseSource(nameof(AllThemes))]
    public void GalleryItem_Selected_Indicator_Has_NonText_Contrast(string themeName)
    {
        var theme = GetTheme(themeName);

        // Before the fix, a selected item was marked only by its fill (GalleryItem.Selected).
        var indicator = GetBrushColor(theme, "GalleryItem.Selected.Border", "GalleryItem.Selected");

        AssertContrast(themeName, "selected gallery item on DropDown.Background", indicator, GetBrushColor(theme, "DropDown.Background"), NonTextMinimum);
        AssertContrast(themeName, "selected gallery item on InRibbonGallery.Content.Background", indicator, GetBrushColor(theme, "InRibbonGallery.Content.Background"), NonTextMinimum);
    }

    [TestCaseSource(nameof(AllThemes))]
    public void Button_MouseOver_And_Pressed_Text_Has_Text_Contrast(string themeName)
    {
        var theme = GetTheme(themeName);

        // Before the fix, the text kept the button foreground (LabelText) on the accent colored hover and pressed fills.
        AssertContrast(themeName, "button text on Button.MouseOver.Background", GetBrushColor(theme, "Button.MouseOver.Foreground", "LabelText"), GetBrushColor(theme, "Button.MouseOver.Background"), TextMinimum);
        AssertContrast(themeName, "button text on Button.Pressed.Background", GetBrushColor(theme, "Button.Pressed.Foreground", "LabelText"), GetBrushColor(theme, "Button.Pressed.Background"), TextMinimum);
    }

    private static void AssertContrast(string themeName, string description, Color foreground, Color background, double minimum)
    {
        Assert.That(background.A, Is.EqualTo(255), $"{themeName}: the background of {description} must be opaque to compute a contrast ratio.");

        var ratio = ContrastHelper.GetContrastRatio(foreground, background);

        Assert.That(ratio, Is.GreaterThanOrEqualTo(minimum), $"{themeName}: {description} ({foreground} on {background}) has a contrast ratio of {ratio:0.00}:1, needs {minimum:0.0}:1.");
    }

    private static ResourceDictionary GetTheme(string themeName)
    {
        if (themeCache.TryGetValue(themeName, out var theme) == false)
        {
            theme = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Fluent;component/Themes/Themes/{themeName}.xaml")
            };

            themeCache.Add(themeName, theme);
        }

        return theme;
    }

    private static Color GetBrushColor(ResourceDictionary theme, string key)
    {
        return GetBrushColor(theme, key, key);
    }

    private static Color GetBrushColor(ResourceDictionary theme, string key, string fallbackKey)
    {
        var resourceKey = theme.Contains(BrushPrefix + key)
            ? BrushPrefix + key
            : BrushPrefix + fallbackKey;

        var brush = theme[resourceKey] as SolidColorBrush;

        Assert.That(brush, Is.Not.Null, $"'{resourceKey}' must be a SolidColorBrush.");

        // A brush opacity below 1 makes the color translucent, just like a lower alpha.
        var color = brush.Color;
        return Color.FromArgb((byte)Math.Round(color.A * brush.Opacity), color.R, color.G, color.B);
    }

    private static double GetSimplifiedTextBoxWatermarkOpacity()
    {
        var template = (ControlTemplate)Application.Current.FindResource("Fluent.Ribbon.Templates.TextBox.Simplified");

        var textBox = new Fluent.TextBox
        {
            Template = template
        };
        textBox.ApplyTemplate();

        var watermark = template.FindName("PART_Watermark", textBox) as UIElement;

        Assert.That(watermark, Is.Not.Null, "The simplified text box template must contain PART_Watermark.");

        return watermark.Opacity;
    }
}