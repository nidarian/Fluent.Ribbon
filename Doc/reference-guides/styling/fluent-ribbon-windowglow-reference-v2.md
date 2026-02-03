---
title: Fluent.Ribbon WindowGlow Reference
description: Window glow effects, borders, and DWM integration
tags: [windowglow, window, borders, dwm, styling]
see_also:
  - controlzex-theming-reference-v2.md
  - ../controls/fluent-ribbon-titlebar-reference.md
---

# Fluent.Ribbon WindowGlow Reference (v2)

**The RIGHT way to add window glow effects - no hacking required.**

*v2 Changes: Added DWMSupportsBorderColor property, clarified Windows 11 integration, added WindowState considerations*

---

## Overview

WindowGlow is a feature from **ControlzEx** (not Fluent.Ribbon directly). Fluent.Ribbon's `RibbonWindow` inherits from `ControlzEx.WindowChromeWindow`, which provides all glow functionality.

```
ControlzEx.WindowChromeWindow (provides glow)
    +-- Fluent.RibbonWindow (inherits glow)
```

---

## Quick Start

### XAML - Set Glow in Window Declaration

```xml
<fluent:RibbonWindow x:Class="MyApp.MainWindow"
    xmlns:fluent="urn:fluent-ribbon"
    GlowColor="DodgerBlue"
    NonActiveGlowColor="Gray"
    GlowDepth="9">
    <!-- content -->
</fluent:RibbonWindow>
```

### Code-Behind - Change Glow at Runtime

```csharp
using System.Windows.Media;

// Change glow color
this.GlowColor = Colors.DodgerBlue;

// Change inactive glow
this.NonActiveGlowColor = Colors.Gray;

// Change glow size
this.GlowDepth = 12;

// Enable smooth transitions
this.IsGlowTransitionEnabled = true;
```

---

## All Glow Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `GlowColor` | `Color?` | `Fluent.Ribbon.Colors.AccentBase` | Glow color when window is **active** |
| `NonActiveGlowColor` | `Color?` | `#434346` | Glow color when window is **inactive** |
| `GlowDepth` | `int` | 9 | Size/thickness of the glow in pixels |
| `IsGlowTransitionEnabled` | `bool` | true | Animate color transitions |
| `UseRadialGradientForCorners` | `bool` | true | Use radial gradient for rounded corners |
| `PreferDWMBorderColor` | `bool` | false | Use Windows DWM border color instead |
| `DWMSupportsBorderColor` | `bool` | (read-only) | True if Windows 11 DWM border is available |

---

## Windows 11 DWM Border Integration

Windows 11 introduced native window border coloring through DWM (Desktop Window Manager). Fluent.Ribbon can detect and use this feature.

### Check DWM Support

```csharp
// Read-only property - true on Windows 11+
if (this.DWMSupportsBorderColor)
{
    // Windows 11 native borders available
    Console.WriteLine("DWM border coloring supported");
}
```

### Use DWM Borders Instead of Glow

```xml
<fluent:RibbonWindow PreferDWMBorderColor="True">
    <!-- Uses Windows 11 native border instead of glow effect -->
</fluent:RibbonWindow>
```

When `PreferDWMBorderColor="True"`:
- On Windows 11: Uses native DWM border (cleaner, better performance)
- On Windows 10: Falls back to traditional glow

---

## Theme-Aware Glow (Recommended)

The default setup uses theme resources, so glow color changes automatically with theme:

```xml
<!-- This is the default - glow follows accent color -->
<Setter Property="GlowColor" Value="{DynamicResource Fluent.Ribbon.Colors.AccentBase}" />
```

To keep this behavior while customizing:

```csharp
// Override the theme resource instead of the property
var accentColor = Color.FromRgb(0x00, 0x78, 0xD4);
Application.Current.Resources["Fluent.Ribbon.Colors.AccentBase"] = accentColor;
```

---

## Custom Static Glow Color

If you want a fixed color that ignores theme:

```xml
<fluent:RibbonWindow GlowColor="Purple"
                     NonActiveGlowColor="DarkGray">
```

Or in code:

```csharp
// In constructor or Loaded event
this.GlowColor = Colors.Purple;
this.NonActiveGlowColor = Color.FromRgb(64, 64, 64);
```

---

## Disable Glow Entirely

```xml
<fluent:RibbonWindow GlowColor="{x:Null}">
```

Or:

```csharp
this.GlowColor = null;
```

---

## Window State Considerations

Glow behavior changes based on window state:

| Window State | Glow Behavior |
|--------------|---------------|
| Normal | Full glow visible around all edges |
| Maximized | Glow hidden (window fills screen) |
| Minimized | Glow hidden |

### Glow and WindowState

```csharp
// Glow automatically hides when maximized
this.WindowState = WindowState.Maximized; // No glow visible

// Glow returns when restored
this.WindowState = WindowState.Normal; // Glow visible again
```

---

## Related Window Features

### Corner Preference (Windows 11)

```csharp
using ControlzEx;

// Options: Default, DoNotRound, Round, RoundSmall
this.CornerPreference = WindowCornerPreference.Round;
```

### Backdrop Type (Windows 11 Mica/Acrylic)

```xml
<fluent:RibbonWindow xmlns:controlzex="urn:controlzex"
                     controlzex:WindowBackdropManager.BackdropType="Mica">
```

Available backdrop types:
- `None` - No backdrop effect
- `Auto` - System decides
- `Mica` - Windows 11 Mica effect (default in Fluent.Ribbon)
- `Acrylic` - Blurred transparency
- `Tabbed` - Windows 11 tabbed window style

```csharp
using ControlzEx;

WindowBackdropManager.SetBackdropType(this, WindowBackdropType.Mica);
```

### Combining Mica with Glow

When using Mica backdrop on Windows 11:

```xml
<fluent:RibbonWindow
    controlzex:WindowBackdropManager.BackdropType="Mica"
    PreferDWMBorderColor="True"
    GlowColor="{DynamicResource Fluent.Ribbon.Colors.AccentBase}">
```

This gives you:
- Mica background effect
- DWM border coloring (if supported)
- Fallback glow on older Windows versions

---

## Default Style Reference

From `Fluent.Ribbon\Themes\RibbonWindow.xaml`:

```xml
<Style x:Key="Fluent.Ribbon.Styles.RibbonWindow"
       TargetType="{x:Type Fluent:RibbonWindow}">
    <Setter Property="Background" Value="{DynamicResource Fluent.Ribbon.Brushes.RibbonWindow.Background}" />
    <Setter Property="BorderBrush" Value="{DynamicResource Fluent.Ribbon.Brushes.AccentBase}" />
    <Setter Property="GlowColor" Value="{DynamicResource Fluent.Ribbon.Colors.AccentBase}" />
    <Setter Property="NonActiveBorderBrush" Value="#434346" />
    <Setter Property="NonActiveGlowColor" Value="#434346" />
    <Setter Property="TitleBackground" Value="{DynamicResource Fluent.Ribbon.Brushes.RibbonWindow.TitleBackground}" />
    <Setter Property="TitleForeground" Value="{DynamicResource Fluent.Ribbon.Brushes.RibbonWindow.TitleForeground}" />
    <Setter Property="controlzex:WindowBackdropManager.BackdropType" Value="Mica" />
</Style>
```

---

## Showcase App Example

The Showcase app demonstrates glow in the **Settings tab > WindowGlow section**:

```
File: Fluent.Ribbon.Showcase\TestContent.xaml (lines 3182-3258)
```

Key bindings from Showcase:

```xml
<!-- Binding glow color to a ComboBox -->
<Fluent:ComboBox Header="GlowColor"
    SelectedValue="{Binding Path=GlowColor,
        RelativeSource={RelativeSource FindAncestor,
        AncestorType={x:Type controlzEx:WindowChromeWindow}},
        Mode=TwoWay}" />

<!-- Binding glow depth to a TextBox -->
<Fluent:TextBox Header="GlowDepth"
    Text="{Binding RelativeSource={RelativeSource AncestorType=controlzEx:WindowChromeWindow},
        Path=GlowDepth, Mode=TwoWay}" />

<!-- Binding DWM border preference -->
<Fluent:CheckBox Header="PreferDWMBorderColor"
    IsChecked="{Binding RelativeSource={RelativeSource AncestorType=controlzEx:WindowChromeWindow},
        Path=PreferDWMBorderColor, Mode=TwoWay}" />
```

---

## Example: WPF Application Implementation

### Step 1: Ensure Window Inherits from RibbonWindow

```xml
<fluent:RibbonWindow x:Class="MyApp.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:fluent="urn:fluent-ribbon"
    Title="My Application"
    GlowColor="{DynamicResource Fluent.Ribbon.Colors.AccentBase}"
    NonActiveGlowColor="#434346">
```

### Step 2: Code-Behind (if needed)

```csharp
using System.Windows.Media;

public partial class MainWindow : Fluent.RibbonWindow
{
    public MainWindow()
    {
        InitializeComponent();

        // Optional: Custom glow settings
        this.GlowDepth = 12;
        this.IsGlowTransitionEnabled = true;

        // Use DWM borders on Windows 11
        if (this.DWMSupportsBorderColor)
        {
            this.PreferDWMBorderColor = true;
        }
    }

    // Optional: Method to change glow based on app state
    public void SetErrorGlow()
    {
        this.GlowColor = Colors.Red;
    }

    public void SetNormalGlow()
    {
        this.GlowColor = (Color)FindResource("Fluent.Ribbon.Colors.AccentBase");
    }
}
```

### Step 3: Theme-Aware Custom Color (Optional)

If you want a custom color that still responds to dark/light mode:

```csharp
// In App.xaml.cs or theme initialization
var customAccent = Color.FromRgb(0x00, 0x78, 0xD4); // Your brand color
var theme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme("Dark", customAccent, false);
ThemeManager.Current.ChangeTheme(Application.Current, theme);
// Glow will automatically use the new accent color
```

---

## Source Files

| File | Purpose |
|------|---------|
| `Fluent.Ribbon\Controls\RibbonWindow.cs` | RibbonWindow class (extends WindowChromeWindow) |
| `Fluent.Ribbon\Themes\RibbonWindow.xaml` | Default style with glow settings |
| `Fluent.Ribbon.Showcase\TestContent.xaml:3182` | Working glow demo UI |
| `Fluent.Ribbon.Showcase\TestContent.xaml.cs:179-199` | Glow color initialization |
| `ControlzEx\WindowChromeWindow.cs` | Base class with glow implementation |

---

## DO NOT DO

```csharp
// WRONG - Don't walk visual tree to find glow elements
foreach (var child in GetVisualChildren(window))
{
    if (child.Name.Contains("Glow"))
        // Don't do this!
}

// WRONG - Don't try to modify internal glow windows
var glowWindow = GetGlowWindow(this);
glowWindow.Background = myBrush;
```

**Use the properties directly on RibbonWindow - they exist for this purpose.**

---

## Summary

| Task | Solution |
|------|----------|
| Change active glow color | `this.GlowColor = Colors.Blue;` |
| Change inactive glow color | `this.NonActiveGlowColor = Colors.Gray;` |
| Change glow size | `this.GlowDepth = 12;` |
| Theme-aware glow | Use default or override `Fluent.Ribbon.Colors.AccentBase` |
| Disable glow | `this.GlowColor = null;` |
| Use Windows 11 DWM border | `this.PreferDWMBorderColor = true;` |
| Check DWM support | `if (this.DWMSupportsBorderColor) { ... }` |
| Windows 11 Mica | `WindowBackdropManager.SetBackdropType(this, WindowBackdropType.Mica);` |
