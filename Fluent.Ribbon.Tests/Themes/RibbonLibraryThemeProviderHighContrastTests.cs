namespace Fluent.Tests.Themes;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ControlzEx.Theming;
using Fluent.Tests.Helper;
using Fluent.Theming;
using NUnit.Framework;

/// <summary>
/// Tests for the High Contrast palette of <see cref="RibbonLibraryThemeProvider"/> (issue #1018).
/// </summary>
[TestFixture]
public class RibbonLibraryThemeProviderHighContrastTests
{
    [Test]
    [TestCaseSource(nameof(HighContrastMapping))]
    public void FillColorSchemeValues_HighContrast_UsesSystemColor(string key, string systemColorName)
    {
        var values = new Dictionary<string, string>();

        RibbonLibraryThemeProvider.DefaultInstance.FillColorSchemeValues(values, CreateColorValues(isHighContrast: true));

        // TryGetValue instead of the indexer, so a missing key fails as an assertion and not as a KeyNotFoundException.
        values.TryGetValue(key, out var actual);

        Assert.That(actual, Is.EqualTo(GetSystemColor(systemColorName).ToString(CultureInfo.InvariantCulture)), key);
    }

    [Test]
    public void FillColorSchemeValues_NotHighContrast_KeepsAccentPalette()
    {
        var values = new Dictionary<string, string>();
        var accentColor = GetAccentColor();

        RibbonLibraryThemeProvider.DefaultInstance.FillColorSchemeValues(values, CreateColorValues(isHighContrast: false));

        // Apps that don't opt in to High Contrast must keep getting exactly the accent based palette.
        Assert.That(values["Fluent.Ribbon.Colors.AccentBase"], Is.EqualTo(accentColor.ToString()));
        Assert.That(values, Does.Not.ContainKey("Fluent.Ribbon.Colors.Black"));
        Assert.That(values, Does.Not.ContainKey("Fluent.Ribbon.Colors.White"));
    }

    [Test]
    [TestCase("Light")]
    [TestCase("Dark")]
    public void GenerateRuntimeLibraryTheme_HighContrast_UsesSystemColors(string baseColorScheme)
    {
        // This is the same call ControlzEx makes when ThemeSyncMode.SyncWithHighContrast is active and Windows High Contrast is on.
        var libraryTheme = RuntimeThemeGenerator.Current.GenerateRuntimeLibraryTheme(baseColorScheme, GetAccentColor(), true, RibbonLibraryThemeProvider.DefaultInstance);

        Assert.That(libraryTheme, Is.Not.Null);
        Assert.That(libraryTheme.IsHighContrast, Is.True);

        foreach (var mapping in HighContrastMapping())
        {
            var key = (string)mapping[0];

            // "Fluent.Ribbon.Brushes.*" entries are SolidColorBrush resources, only the Color resources are compared here.
            if (key.StartsWith("Fluent.Ribbon.Colors.", StringComparison.Ordinal) == false)
            {
                continue;
            }

            Assert.That(libraryTheme.Resources[key], Is.EqualTo(GetSystemColor((string)mapping[1])), key);
        }
    }

    [Test]
    [TestCase("Light")]
    [TestCase("Dark")]
    public void GenerateRuntimeLibraryTheme_HighContrast_GalleryHeaderText_Has_Text_Contrast(string baseColorScheme)
    {
        // The gallery filter label ("All" etc.) sits on Gallery.Header.Background, which is Gray3 (= WindowText) in High Contrast.
        // With a fixed black text the label disappears in every High Contrast theme with black or dark WindowText
        // (High Contrast White, Desert, and also the normal Windows colors the build machine has): black on black is 1:1.
        var libraryTheme = RuntimeThemeGenerator.Current.GenerateRuntimeLibraryTheme(baseColorScheme, GetAccentColor(), true, RibbonLibraryThemeProvider.DefaultInstance);

        Assert.That(libraryTheme, Is.Not.Null);

        AssertTextContrast(libraryTheme, "Gallery.Header.Foreground", "Gallery.Header.Background");
        AssertTextContrast(libraryTheme, "Gallery.Header.MouseOver.Foreground", "Gallery.Header.MouseOver.Background");
    }

    [Test]
    [TestCase("Light")]
    [TestCase("Dark")]
    public void GenerateRuntimeLibraryTheme_HighContrast_GalleryHeader_Uses_HighContrast_Color_Pairs(string baseColorScheme)
    {
        // A Windows High Contrast theme only promises readable contrast for its own pairs,
        // so the label and its mouse over background must be one of those pairs for every High Contrast theme,
        // not just for the system colors of the machine that runs the tests.
        var libraryTheme = RuntimeThemeGenerator.Current.GenerateRuntimeLibraryTheme(baseColorScheme, GetAccentColor(), true, RibbonLibraryThemeProvider.DefaultInstance);

        Assert.That(libraryTheme, Is.Not.Null);

        AssertIsHighContrastPair(libraryTheme, "Gallery.Header.Foreground", "Gallery.Header.Background");
        AssertIsHighContrastPair(libraryTheme, "Gallery.Header.MouseOver.Foreground", "Gallery.Header.MouseOver.Background");
    }

    /// <summary>
    /// Expected mapping from Fluent color keys to the <see cref="SystemColors"/> property that must be used in High Contrast.
    /// </summary>
    private static IEnumerable<object[]> HighContrastMapping()
    {
        // Text, borders and glyphs
        yield return new object[] { "Fluent.Ribbon.Colors.Black", nameof(SystemColors.WindowTextColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Black20", nameof(SystemColors.GrayTextColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Gray1", nameof(SystemColors.WindowTextColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Gray2", nameof(SystemColors.WindowTextColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Gray3", nameof(SystemColors.WindowTextColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Gray4", nameof(SystemColors.WindowTextColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Gray5", nameof(SystemColors.WindowTextColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Gray6", nameof(SystemColors.WindowTextColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Gray7", nameof(SystemColors.WindowTextColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.DarkIdealForegroundDisabled", nameof(SystemColors.GrayTextColor) };

        // Surfaces
        yield return new object[] { "Fluent.Ribbon.Colors.White", nameof(SystemColors.WindowColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.White20", nameof(SystemColors.WindowColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Gray8", nameof(SystemColors.ControlColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Gray9", nameof(SystemColors.ControlColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Gray10", nameof(SystemColors.ControlColor) };

        // Accent
        yield return new object[] { "Fluent.Ribbon.Colors.AccentBase", nameof(SystemColors.HighlightColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Accent80", nameof(SystemColors.HighlightColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Accent60", nameof(SystemColors.HighlightColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Accent40", nameof(SystemColors.ControlColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Accent20", nameof(SystemColors.ControlColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.Highlight", nameof(SystemColors.HighlightColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.IdealForeground", nameof(SystemColors.HighlightTextColor) };

        yield return new object[] { "Fluent.Ribbon.Colors.AccentLight1", nameof(SystemColors.ControlColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.AccentLight1.Foreground", nameof(SystemColors.ControlTextColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.AccentLight2", nameof(SystemColors.ControlColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.AccentLight2.Foreground", nameof(SystemColors.ControlTextColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.AccentLight3", nameof(SystemColors.ControlColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.AccentLight3.Foreground", nameof(SystemColors.ControlTextColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.AccentDark1", nameof(SystemColors.HotTrackColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.AccentDark1.Foreground", nameof(SystemColors.WindowColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.AccentDark2", nameof(SystemColors.HotTrackColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.AccentDark2.Foreground", nameof(SystemColors.WindowColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.AccentDark3", nameof(SystemColors.HotTrackColor) };
        yield return new object[] { "Fluent.Ribbon.Colors.AccentDark3.Foreground", nameof(SystemColors.WindowColor) };

        // Brushes whose color is a literal in GeneratorParameters.json instead of a palette color
        yield return new object[] { "Fluent.Ribbon.Brushes.RibbonWindow.Background", nameof(SystemColors.WindowColor) };
        yield return new object[] { "Fluent.Ribbon.Brushes.RibbonWindow.Background.Backdrop.Acrylic", nameof(SystemColors.WindowColor) };
        yield return new object[] { "Fluent.Ribbon.Brushes.RibbonWindow.Background.Backdrop.Auto", nameof(SystemColors.WindowColor) };
        yield return new object[] { "Fluent.Ribbon.Brushes.RibbonTabControl.Content.Background", nameof(SystemColors.WindowColor) };
        yield return new object[] { "Fluent.Ribbon.Brushes.DropDown.Background", nameof(SystemColors.WindowColor) };
        yield return new object[] { "Fluent.Ribbon.Brushes.BackstageTabControl.Background", nameof(SystemColors.WindowColor) };
        yield return new object[] { "Fluent.Ribbon.Brushes.BackstageTabControl.ItemsPanelBackground", nameof(SystemColors.ControlColor) };
        yield return new object[] { "Fluent.Ribbon.Brushes.Backstage.BackButton.Foreground", nameof(SystemColors.HighlightTextColor) };
    }

    private static void AssertTextContrast(LibraryTheme libraryTheme, string foregroundKey, string backgroundKey)
    {
        var foreground = GetBrushColor(libraryTheme, foregroundKey);
        var background = GetBrushColor(libraryTheme, backgroundKey);

        var ratio = ContrastHelper.GetContrastRatio(foreground, background);

        // WCAG 1.4.3 (text contrast): normal size text needs 4.5:1.
        Assert.That(ratio, Is.GreaterThanOrEqualTo(4.5), $"{foregroundKey} ({foreground}) on {backgroundKey} ({background}) has a contrast ratio of {ratio:0.00}:1, needs 4.5:1.");
    }

    private static void AssertIsHighContrastPair(LibraryTheme libraryTheme, string foregroundKey, string backgroundKey)
    {
        var actual = $"{GetBrushColor(libraryTheme, foregroundKey)} on {GetBrushColor(libraryTheme, backgroundKey)}";

        // Text on background pairs of a High Contrast theme, and their inverse (the contrast ratio is the same both ways).
        var pairs = new List<string>
        {
            $"{SystemColors.WindowTextColor} on {SystemColors.WindowColor}",
            $"{SystemColors.WindowColor} on {SystemColors.WindowTextColor}",
            $"{SystemColors.ControlTextColor} on {SystemColors.ControlColor}",
            $"{SystemColors.ControlColor} on {SystemColors.ControlTextColor}",
            $"{SystemColors.HighlightTextColor} on {SystemColors.HighlightColor}",
            $"{SystemColors.HighlightColor} on {SystemColors.HighlightTextColor}",
        };

        Assert.That(pairs, Does.Contain(actual), $"{foregroundKey} on {backgroundKey} must be a High Contrast system color pair.");
    }

    private static Color GetBrushColor(LibraryTheme libraryTheme, string key)
    {
        var brush = libraryTheme.Resources["Fluent.Ribbon.Brushes." + key] as SolidColorBrush;

        Assert.That(brush, Is.Not.Null, $"'Fluent.Ribbon.Brushes.{key}' must be a SolidColorBrush.");

        return brush!.Color;
    }

    private static Color GetSystemColor(string systemColorName)
    {
        return (Color)typeof(SystemColors).GetProperty(systemColorName)!.GetValue(null, null);
    }

    private static Color GetAccentColor()
    {
        // WHY: the accent must differ from SystemColors.HighlightColor, otherwise the tests could pass
        // even if the provider ignored High Contrast and kept using the accent color.
        return SystemColors.HighlightColor == Colors.Red
            ? Colors.Green
            : Colors.Red;
    }

    private static RuntimeThemeColorValues CreateColorValues(bool isHighContrast)
    {
        // Same options object ControlzEx builds in RuntimeThemeGenerator.GenerateRuntimeLibraryTheme.
        var options = new RuntimeThemeOptions(false, isHighContrast, null, null);

        return RuntimeThemeGenerator.Current.GetColors(GetAccentColor(), options);
    }
}