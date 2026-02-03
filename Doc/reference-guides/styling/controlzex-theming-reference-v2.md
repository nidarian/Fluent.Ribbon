---
title: ControlzEx.Theming Reference Guide
description: ThemeManager, runtime themes, and color customization for Fluent.Ribbon apps
tags: [theming, thememanager, colors, controlzex, styling]
see_also:
  - fluent-ribbon-brushes-reference-v2.md
  - fluent-ribbon-windowglow-reference-v2.md
  - ../getting-started/fluent-ribbon-common-tasks-v2.md
---

# ControlzEx.Theming Reference Guide (v2)

**For WPF Applications Using Fluent.Ribbon**

*v2 Changes: Clarified ColorPalette generates 11 shades (not 9), added RuntimeThemeGenerator 3rd parameter*

*Last Updated: 2026-01-23*

---

## Overview

ControlzEx.Theming is the theming engine that powers Fluent.Ribbon's theme system. It provides:

- **Runtime theme generation** - Create themes on-the-fly from accent colors
- **Theme synchronization** - Sync with Windows system theme
- **Built-in themes** - 23 accent colors x Light/Dark/Colorful = 50+ themes
- **Custom theme registration** - Add your own themes

Most Fluent.Ribbon apps already use ControlzEx.Theming (it's a transitive dependency). If your app manually overrides brushes via `Application.Current.Resources`, it works but may require visual tree walking for some controls.

---

## Architecture

```
+----------------------------------------------------------------+
|                    ControlzEx.Theming                           |
+----------------------------------------------------------------+
|                                                                 |
|  ThemeManager.Current                                           |
|  +-- Themes (ReadOnlyObservableCollection<Theme>)              |
|  +-- BaseColors (Light, Dark, Colorful)                        |
|  +-- ColorSchemes (Blue, Red, Green, etc.)                     |
|  +-- ThemeSyncMode                                              |
|  |   +-- DoNotSync          - App controls themes              |
|  |   +-- SyncWithAppMode    - Follow Windows light/dark        |
|  |   +-- SyncAll            - Follow Windows + accent color    |
|  |                                                              |
|  +-- Methods:                                                   |
|      +-- ChangeTheme(app/window, theme)                        |
|      +-- ChangeThemeBaseColor(app, "Light"/"Dark")             |
|      +-- ChangeThemeColorScheme(app, "Blue")                   |
|      +-- DetectTheme(app/window/resourceDict)                  |
|      +-- GetTheme("Dark.Blue")                                 |
|      +-- GetInverseTheme(theme) -> Dark<->Light                |
|                                                                 |
|  RuntimeThemeGenerator.Current                                  |
|  +-- GenerateRuntimeTheme(baseColor, accentColor, isHighContrast)|
|      -> Creates Theme from any Color                            |
|                                                                 |
|  LibraryThemeProvider (base class)                              |
|  +-- RibbonLibraryThemeProvider                                 |
|      +-- FillColorSchemeValues() - Generates Fluent.Ribbon brushes|
|                                                                 |
+----------------------------------------------------------------+
                              |
                              v
+----------------------------------------------------------------+
|                    Fluent.Ribbon Themes                         |
+----------------------------------------------------------------+
|                                                                 |
|  Theme.Template.xaml                                            |
|  +-- Metadata (Theme.Name, Theme.BaseColorScheme, etc.)        |
|  +-- Colors (Fluent.Ribbon.Colors.AccentBase, etc.)            |
|  +-- Brushes (Fluent.Ribbon.Brushes.AccentBase, etc.)          |
|                                                                 |
|  Built-in Themes:                                               |
|  +-- Light.Amber, Light.Blue, Light.Brown, ...                 |
|  +-- Dark.Amber, Dark.Blue, Dark.Brown, ...                    |
|  +-- Light.Amber.Colorful, Light.Blue.Colorful, ...            |
|  +-- Dark.Amber.Colorful, Dark.Blue.Colorful, ...              |
|                                                                 |
+----------------------------------------------------------------+
```

---

## Key Concepts

### Theme Naming Convention

Themes follow the pattern: `{BaseColor}.{AccentColor}[.Colorful]`

| Example | BaseColor | AccentColor | Colorful |
|---------|-----------|-------------|----------|
| `Light.Blue` | Light | Blue | No |
| `Dark.Blue` | Dark | Blue | No |
| `Light.Blue.Colorful` | Light | Blue | Yes (vibrant) |

### BaseColor vs ColorScheme

- **BaseColor**: `Light`, `Dark`, or `Colorful` - determines overall brightness
- **ColorScheme**: The accent color name - `Blue`, `Red`, `Green`, `Amber`, etc.

### Theme Metadata Keys

From `Theme.Template.xaml`:

```xml
<system:String x:Key="Theme.Name">Dark.Blue</system:String>
<system:String x:Key="Theme.Origin">Fluent.Ribbon</system:String>
<system:String x:Key="Theme.DisplayName">Blue (Dark)</system:String>
<system:String x:Key="Theme.BaseColorScheme">Dark</system:String>
<system:String x:Key="Theme.ColorScheme">Blue</system:String>
```

---

## Common Usage Patterns

### 1. Change Theme (Simple)

```csharp
using ControlzEx.Theming;

// Change to a built-in theme
ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Blue");

// Change just the base color (Light <-> Dark)
ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Dark");

// Change just the accent color
ThemeManager.Current.ChangeThemeColorScheme(Application.Current, "Red");
```

### 2. Detect Current Theme

```csharp
var currentTheme = ThemeManager.Current.DetectTheme(Application.Current);
if (currentTheme != null)
{
    Console.WriteLine($"Current: {currentTheme.Name}"); // e.g., "Dark.Blue"
    Console.WriteLine($"Base: {currentTheme.BaseColorScheme}"); // "Dark"
    Console.WriteLine($"Accent: {currentTheme.ColorScheme}"); // "Blue"
}
```

### 3. Generate Runtime Theme from Custom Color

```csharp
using ControlzEx.Theming;

// Generate a theme from any color
var customColor = Colors.Purple; // or parse from "#9C27B0"
var theme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme(
    baseColorScheme: "Dark",
    accentColor: customColor,
    isHighContrast: false  // IMPORTANT: Always pass this 3rd parameter!
);

// Apply the generated theme
ThemeManager.Current.ChangeTheme(Application.Current, theme);
```

**IMPORTANT:** The `isHighContrast` parameter (3rd argument) is required. Pass `false` for normal themes.

### 4. Sync with Windows Theme

```csharp
// In App.xaml.cs OnStartup:
ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
ThemeManager.Current.SyncTheme();

// Handle theme changes from Windows
ThemeManager.Current.ThemeChanged += (sender, args) =>
{
    var newTheme = args.NewTheme;
    Console.WriteLine($"Windows changed to: {newTheme?.Name}");

    // Update any custom controls that need manual theming
    ApplyCustomControlTheming();
};
```

### 5. Get Inverse Theme (Toggle Light/Dark)

```csharp
var currentTheme = ThemeManager.Current.DetectTheme(Application.Current);
var inverseTheme = ThemeManager.Current.GetInverseTheme(currentTheme);

// Dark.Blue -> Light.Blue
ThemeManager.Current.ChangeTheme(Application.Current, inverseTheme);
```

---

## Fluent.Ribbon Color Resources

### Accent Colors (from Theme.Template.xaml)

| Resource Key | Purpose |
|--------------|---------|
| `Fluent.Ribbon.Colors.AccentBase` | Primary accent (100%) |
| `Fluent.Ribbon.Colors.Accent80` | 80% opacity variant |
| `Fluent.Ribbon.Colors.Accent60` | 60% opacity variant |
| `Fluent.Ribbon.Colors.Accent40` | 40% opacity variant |
| `Fluent.Ribbon.Colors.Accent20` | 20% opacity variant |
| `Fluent.Ribbon.Colors.Highlight` | Selection highlight |
| `Fluent.Ribbon.Colors.AccentLight1` | Light shade 1 |
| `Fluent.Ribbon.Colors.AccentLight2` | Light shade 2 |
| `Fluent.Ribbon.Colors.AccentLight3` | Light shade 3 (lightest) |
| `Fluent.Ribbon.Colors.AccentDark1` | Dark shade 1 |
| `Fluent.Ribbon.Colors.AccentDark2` | Dark shade 2 |
| `Fluent.Ribbon.Colors.AccentDark3` | Dark shade 3 (darkest) |

### Base Colors (Gray Scale)

| Resource Key | Purpose |
|--------------|---------|
| `Fluent.Ribbon.Colors.Black` | Pure black |
| `Fluent.Ribbon.Colors.White` | Pure white |
| `Fluent.Ribbon.Colors.Gray1` - `Gray10` | Gradient from dark to light |

### Key Brush Resources

| Resource Key | Used For |
|--------------|----------|
| `Fluent.Ribbon.Brushes.RibbonWindow.Background` | Main window background |
| `Fluent.Ribbon.Brushes.RibbonTabControl.Background` | Tab row area |
| `Fluent.Ribbon.Brushes.RibbonTabControl.Content.Background` | Button content area |
| `Fluent.Ribbon.Brushes.RibbonTabItem.Active.Background` | Selected tab |
| `Fluent.Ribbon.Brushes.RibbonTabItem.MouseOver.Background` | Tab hover |
| `Fluent.Ribbon.Brushes.RibbonGroupBox.Header.Foreground` | Group text |
| `Fluent.Ribbon.Brushes.Button.MouseOver.Background` | Button hover |
| `Fluent.Ribbon.Brushes.Button.Pressed.Background` | Button pressed |
| `Fluent.Ribbon.Brushes.IdealForeground` | Auto-contrast text |

---

## Example: WPF App Integration

### Common Approach (Manual Brush Overrides)

Many WPF apps using Fluent.Ribbon:
1. Uses `ThemeManager.Current.ChangeTheme()` to set base theme
2. Manually overrides accent brushes via `Application.Current.Resources`
3. Has separate theme dictionaries for custom controls (LightTheme.xaml, DarkTheme.xaml)

```csharp
// Common pattern in a ThemeService class:
ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Blue");

// Then override accent colors manually:
Application.Current.Resources["Fluent.Ribbon.Brushes.AccentBase"] = accentBrush;
Application.Current.Resources["Fluent.Ribbon.Brushes.Accent80"] = accent80Brush;
// ... etc
```

### Recommended Improvement

Instead of manual brush overrides, use `RuntimeThemeGenerator`:

```csharp
// Better approach - let ControlzEx generate all the accent shades:
var accentColor = (Color)ColorConverter.ConvertFromString("#2196F3");
var baseColor = theme == AppTheme.Dark ? "Dark" : "Light";

var generatedTheme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme(
    baseColor,
    accentColor,
    isHighContrast: false  // Don't forget this parameter!
);

ThemeManager.Current.ChangeTheme(Application.Current, generatedTheme);
```

Benefits:
- Automatic accent shade generation (Light1, Light2, Dark1, etc.)
- Proper contrast calculations
- No need to manually calculate 80%, 60%, 40%, 20% variants

---

## Full Theme Library (23 Accent Colors)

Fluent.Ribbon ships with these accent colors:

| Color | Light Theme | Dark Theme | Colorful |
|-------|-------------|------------|----------|
| Amber | Light.Amber | Dark.Amber | *.Colorful |
| Blue | Light.Blue | Dark.Blue | *.Colorful |
| Brown | Light.Brown | Dark.Brown | *.Colorful |
| Cobalt | Light.Cobalt | Dark.Cobalt | *.Colorful |
| Crimson | Light.Crimson | Dark.Crimson | *.Colorful |
| Cyan | Light.Cyan | Dark.Cyan | *.Colorful |
| Emerald | Light.Emerald | Dark.Emerald | *.Colorful |
| Green | Light.Green | Dark.Green | *.Colorful |
| Indigo | Light.Indigo | Dark.Indigo | *.Colorful |
| Lime | Light.Lime | Dark.Lime | *.Colorful |
| Magenta | Light.Magenta | Dark.Magenta | *.Colorful |
| Mauve | Light.Mauve | Dark.Mauve | *.Colorful |
| Olive | Light.Olive | Dark.Olive | *.Colorful |
| Orange | Light.Orange | Dark.Orange | *.Colorful |
| Pink | Light.Pink | Dark.Pink | *.Colorful |
| Purple | Light.Purple | Dark.Purple | *.Colorful |
| Red | Light.Red | Dark.Red | *.Colorful |
| Sienna | Light.Sienna | Dark.Sienna | *.Colorful |
| Steel | Light.Steel | Dark.Steel | *.Colorful |
| Taupe | Light.Taupe | Dark.Taupe | *.Colorful |
| Teal | Light.Teal | Dark.Teal | *.Colorful |
| Violet | Light.Violet | Dark.Violet | *.Colorful |
| Yellow | Light.Yellow | Dark.Yellow | *.Colorful |

---

## Known Limitations

### 1. StaticResource vs DynamicResource

Fluent.Ribbon templates use `StaticResource` for brushes:

```xml
<!-- In Fluent.Ribbon control templates -->
<SolidColorBrush x:Key="..." Color="{StaticResource Fluent.Ribbon.Colors.AccentBase}" />
```

**Impact:** Changing resources after window loads doesn't update already-rendered controls.

**Workaround:** Visual tree walking (see `fluent-ribbon-theming-guide.md`) or use `ChangeTheme()` which reloads all resources.

### 2. Non-Ribbon Controls

Custom controls (DataGrids, TreeViews, etc.) don't use Fluent.Ribbon brushes.

**Solution:** Maintain parallel theme dictionaries (LightTheme.xaml, DarkTheme.xaml) with matching colors.

### 3. High Contrast

`RuntimeThemeGenerator` has limited High Contrast support. For High Contrast themes, use SystemColors directly, which is the correct approach.

---

## Example: Building a Theme Selector UI

```xaml
<ComboBox ItemsSource="{Binding Source={x:Static theme:ThemeManager.Current}, Path=Themes}"
          SelectedItem="{Binding CurrentTheme}"
          DisplayMemberPath="DisplayName">
    <ComboBox.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
        </GroupStyle>
    </ComboBox.GroupStyle>
</ComboBox>
```

```csharp
// ViewModel property:
public Theme? CurrentTheme
{
    get => ThemeManager.Current.DetectTheme(Application.Current);
    set
    {
        if (value != null)
        {
            ThemeManager.Current.ChangeTheme(Application.Current, value);
            OnPropertyChanged();
        }
    }
}
```

---

## RibbonLibraryThemeProvider Deep Dive

This class (in `Fluent.Ribbon\Theming\RibbonLibraryThemeProvider.cs`) generates Fluent.Ribbon-specific brush values when themes are created at runtime.

### Key Method: FillColorSchemeValues

```csharp
public override void FillColorSchemeValues(
    Dictionary<string, string> values,
    RuntimeThemeColorValues colorValues)
{
    // Accent opacity variants
    values.Add("Fluent.Ribbon.Colors.AccentBase", colorValues.AccentColor.ToString());
    values.Add("Fluent.Ribbon.Colors.Accent80", colorValues.AccentColor80.ToString());
    values.Add("Fluent.Ribbon.Colors.Accent60", colorValues.AccentColor60.ToString());
    // ...

    // Light/Dark shades via ColorPalette
    var colorPalette = new ColorPalette(accentColor);
    // colorPalette.Palette[0-10] = shades from light to dark
}
```

### ColorPalette Structure

The `ColorPalette` class generates **11 shades** from an accent color (indices 0-10):

| Index | Shade | Fluent.Ribbon Resource |
|-------|-------|------------------------|
| 0 | Lightest | AccentLight3 |
| 1 | ... | AccentLight2 |
| 2 | ... | AccentLight1 |
| 3-4 | ... | (transition shades) |
| 5 | Base accent | AccentBase area |
| 6-7 | ... | (transition shades) |
| 8 | ... | AccentDark1 |
| 9 | ... | AccentDark2 |
| 10 | Darkest | AccentDark3 |

The exact mapping to Fluent.Ribbon resources depends on the theme's base color (Light/Dark).

---

## Migration Path: Manual Overrides to RuntimeThemeGenerator

### Phase 1: Replace Manual Brush Overrides

Common manual approach:
```csharp
// Manual opacity calculations
var accent80 = Color.FromArgb(204, accentColor.R, accentColor.G, accentColor.B);
Application.Current.Resources["Fluent.Ribbon.Brushes.Accent80"] = new SolidColorBrush(accent80);
```

Improved:
```csharp
// Let RuntimeThemeGenerator handle it
var theme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme("Dark", accentColor, false);
ThemeManager.Current.ChangeTheme(Application.Current, theme);
```

### Phase 2: Unify Custom Control Themes

Instead of separate `LightTheme.xaml`/`DarkTheme.xaml`, reference Fluent.Ribbon brushes directly:

```xml
<!-- Before: Hardcoded values -->
<SolidColorBrush x:Key="CardBackgroundBrush" Color="#2D2D30"/>

<!-- After: Reference Fluent.Ribbon brushes -->
<SolidColorBrush x:Key="CardBackgroundBrush"
                 Color="{DynamicResource Fluent.Ribbon.Colors.Gray2}"/>
```

### Phase 3: Handle Theme Changes

Subscribe to theme changes for any controls that need manual updates:

```csharp
ThemeManager.Current.ThemeChanged += (s, e) =>
{
    // Update any controls that don't auto-update
    UpdateCustomControlColors(e.NewTheme);
};
```

---

## Related Documents

- `fluent-ribbon-brushes-reference-v2.md` - Complete brush key reference
- `fluent-ribbon-theming-guide.md` - Visual tree workaround for stubborn controls
- `fluent-ribbon-controls-reference-v2.md` - Control templates and styling
- `fluent-ribbon-lessons-learned-v2.md` - What works and what doesn't

---

## Summary

ControlzEx.Theming provides a robust theming infrastructure that your Fluent.Ribbon app already has access to. The key opportunities are:

1. **Use `RuntimeThemeGenerator`** instead of manual brush overrides (don't forget the 3rd parameter!)
2. **Reference Fluent.Ribbon resources** in custom control themes for consistency
3. **Subscribe to `ThemeChanged`** for dynamic theme switching
4. **Consider Windows sync** for automatic Light/Dark switching

Manual brush override approaches work but can be simplified by leaning more on ControlzEx.Theming's built-in capabilities.
