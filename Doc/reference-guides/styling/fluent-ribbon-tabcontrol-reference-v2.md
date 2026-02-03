---
title: Fluent.Ribbon TabControl Reference
description: Styling standard WPF TabControls to match Fluent.Ribbon themes
tags: [tabcontrol, tabs, styling, wpf]
see_also:
  - fluent-ribbon-brushes-reference-v2.md
  - controlzex-theming-reference-v2.md
---

# Fluent.Ribbon TabControl Reference (v2)

**Styling standard WPF TabControls to match Fluent.Ribbon themes.**

*v2 Changes: Fixed Color resource references (Gray7 not Fluent.Ribbon.Colors.Gray7), added TabStripPlacement info*

---

## Overview

Fluent.Ribbon does **NOT** provide a pre-built style for standard WPF `TabControl`. It only provides:

| Control | Style | Use Case |
|---------|-------|----------|
| `Fluent:RibbonTabControl` | Built-in | The ribbon tabs at top |
| `Fluent:BackstageTabControl` | Built-in | Tabs inside Backstage (File menu) |
| `TabControl` (standard WPF) | `Fluent.Ribbon.Styles.InnerBackstageTabControl` | Nested tabs inside Backstage |
| `TabControl` (standard WPF) | **You must style it** | Main content area |

---

## Quick Start: Use Built-in Style

For TabControls inside Backstage:

```xml
<TabControl Style="{DynamicResource Fluent.Ribbon.Styles.InnerBackstageTabControl}">
    <TabItem Header="Tab 1">Content 1</TabItem>
    <TabItem Header="Tab 2">Content 2</TabItem>
</TabControl>
```

This style:
- Left-aligned vertical tabs
- Theme-aware colors (hover, selected)
- Uses `Fluent.Ribbon.Brushes.Button.MouseOver.Background` for hover
- Uses `Fluent.Ribbon.Brushes.Button.Pressed.Background` for selected

---

## Main Content TabControl (Theme-Aware)

For TabControls in your main content area, create a style using Fluent.Ribbon brushes.

### Option 1: Inline Resources (Simplest)

```xml
<TabControl Background="Transparent" BorderThickness="0">
    <TabControl.Resources>
        <!-- Theme-aware brushes for tab states -->
        <!-- NOTE: Color resources use short names, NOT Fluent.Ribbon.Colors prefix -->
        <SolidColorBrush x:Key="TabHoverBackground"
                         Color="{DynamicResource Fluent.Ribbon.Colors.Accent60}" />
        <SolidColorBrush x:Key="TabSelectedBackground"
                         Color="{DynamicResource Gray7}" />
        <SolidColorBrush x:Key="TabHoverBorder"
                         Color="{DynamicResource Fluent.Ribbon.Colors.Accent80}" />

        <Style TargetType="{x:Type TabItem}">
            <Setter Property="Background" Value="{DynamicResource Fluent.Ribbon.Brushes.White}" />
            <Setter Property="Foreground" Value="{DynamicResource Fluent.Ribbon.Brushes.Black}" />
            <Setter Property="Padding" Value="12 6" />
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type TabItem}">
                        <Border x:Name="Border"
                                Padding="{TemplateBinding Padding}"
                                Background="{TemplateBinding Background}"
                                BorderBrush="Transparent"
                                BorderThickness="1 1 1 0">
                            <ContentPresenter ContentSource="Header" />
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter TargetName="Border" Property="Background"
                                        Value="{StaticResource TabHoverBackground}" />
                            </Trigger>
                            <Trigger Property="IsSelected" Value="True">
                                <Setter TargetName="Border" Property="Background"
                                        Value="{StaticResource TabSelectedBackground}" />
                                <Setter Property="Panel.ZIndex" Value="1" />
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </TabControl.Resources>

    <TabItem Header="Home">
        <!-- Content -->
    </TabItem>
    <TabItem Header="Settings">
        <!-- Content -->
    </TabItem>
</TabControl>
```

### Option 2: App-Level Style (Reusable)

In `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <!-- Merge Fluent.Ribbon themes first -->
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Fluent;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>

        <!-- Your TabControl Style -->
        <Style x:Key="FluentTabItem" TargetType="{x:Type TabItem}">
            <Setter Property="Background" Value="{DynamicResource Fluent.Ribbon.Brushes.White}" />
            <Setter Property="Foreground" Value="{DynamicResource Fluent.Ribbon.Brushes.Black}" />
            <Setter Property="Padding" Value="12 6" />
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type TabItem}">
                        <Border x:Name="Border"
                                Padding="{TemplateBinding Padding}"
                                Background="{TemplateBinding Background}"
                                BorderBrush="{DynamicResource Fluent.Ribbon.Brushes.Gray6}"
                                BorderThickness="1 1 1 0"
                                CornerRadius="4 4 0 0">
                            <ContentPresenter ContentSource="Header"
                                              HorizontalAlignment="Center" />
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter TargetName="Border" Property="Background"
                                        Value="{DynamicResource Fluent.Ribbon.Brushes.Button.MouseOver.Background}" />
                                <Setter TargetName="Border" Property="BorderBrush"
                                        Value="{DynamicResource Fluent.Ribbon.Brushes.Button.MouseOver.Border}" />
                            </Trigger>
                            <Trigger Property="IsSelected" Value="True">
                                <Setter TargetName="Border" Property="Background"
                                        Value="{DynamicResource Fluent.Ribbon.Brushes.Button.Pressed.Background}" />
                                <Setter TargetName="Border" Property="BorderBrush"
                                        Value="{DynamicResource Fluent.Ribbon.Brushes.Button.Pressed.Border}" />
                                <Setter Property="Panel.ZIndex" Value="1" />
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <Style x:Key="FluentTabControl" TargetType="{x:Type TabControl}">
            <Setter Property="Background" Value="Transparent" />
            <Setter Property="BorderThickness" Value="0" />
            <Style.Resources>
                <Style TargetType="{x:Type TabItem}" BasedOn="{StaticResource FluentTabItem}" />
            </Style.Resources>
        </Style>
    </ResourceDictionary>
</Application.Resources>
```

Usage:

```xml
<TabControl Style="{StaticResource FluentTabControl}">
    <TabItem Header="Home">Content</TabItem>
    <TabItem Header="Settings">Content</TabItem>
</TabControl>
```

---

## Theme-Aware Brush Reference

Use these DynamicResource brushes to ensure tabs update with theme changes:

### Tab Item Colors

| State | Background | Border |
|-------|------------|--------|
| Normal | `Fluent.Ribbon.Brushes.White` | `Fluent.Ribbon.Brushes.Gray6` |
| Hover | `Fluent.Ribbon.Brushes.Button.MouseOver.Background` | `Fluent.Ribbon.Brushes.Button.MouseOver.Border` |
| Selected | `Fluent.Ribbon.Brushes.Button.Pressed.Background` | `Fluent.Ribbon.Brushes.Button.Pressed.Border` |
| Disabled | `Fluent.Ribbon.Brushes.Gray9` | `Fluent.Ribbon.Brushes.Gray10` |

### Accent Colors (for highlights)

| Brush | Use |
|-------|-----|
| `Fluent.Ribbon.Colors.AccentBase` | Full accent |
| `Fluent.Ribbon.Colors.Accent80` | 80% opacity |
| `Fluent.Ribbon.Colors.Accent60` | 60% opacity |
| `Fluent.Ribbon.Colors.Accent40` | 40% opacity |
| `Fluent.Ribbon.Colors.Accent20` | 20% opacity |

### Gray Scale Colors (Short Names!)

**IMPORTANT:** Gray colors use short names in resource references:

| Resource Key | Description |
|--------------|-------------|
| `Gray1` | Darkest gray |
| `Gray2` | ... |
| `Gray7` | Mid-gray (commonly used for selected states) |
| `Gray10` | Lightest gray |

```xml
<!-- CORRECT -->
<SolidColorBrush Color="{DynamicResource Gray7}" />

<!-- WRONG - This prefix doesn't exist -->
<SolidColorBrush Color="{DynamicResource Fluent.Ribbon.Colors.Gray7}" />
```

### Text Colors

| Brush | Use |
|-------|-----|
| `Fluent.Ribbon.Brushes.Black` | Primary text (adapts to theme) |
| `Fluent.Ribbon.Brushes.White` | Inverse text |
| `Fluent.Ribbon.Brushes.LabelText` | Label/caption text |

---

## Built-in Style: InnerBackstageTabControl

The `Fluent.Ribbon.Styles.InnerBackstageTabControl` provides:

```xml
<!-- From BackstageControls.xaml -->
<Style x:Key="Fluent.Ribbon.Styles.InnerBackstageTabControl"
       TargetType="{x:Type TabControl}">
    <!--
    Features:
    - Vertical left-aligned tabs (TabStripPlacement="Left")
    - Separator line between tabs and content
    - TabItem uses Button.MouseOver/Pressed brushes
    - Supports Fluent:SeparatorTabItem for section headers
    -->
</Style>
```

Usage:

```xml
<Fluent:BackstageTabItem Header="Recent">
    <TabControl Style="{DynamicResource Fluent.Ribbon.Styles.InnerBackstageTabControl}">
        <Fluent:SeparatorTabItem Header="Documents" />
        <TabItem Header="File 1.docx" />
        <TabItem Header="File 2.docx" />
        <Fluent:SeparatorTabItem Header="Folders" />
        <TabItem Header="Projects" />
        <TabItem Header="Downloads" />
    </TabControl>
</Fluent:BackstageTabItem>
```

---

## TabStripPlacement Options

Standard WPF TabControl supports different tab positions:

```xml
<!-- Tabs on top (default) -->
<TabControl TabStripPlacement="Top" />

<!-- Tabs on left (like Backstage) -->
<TabControl TabStripPlacement="Left" />

<!-- Tabs on bottom -->
<TabControl TabStripPlacement="Bottom" />

<!-- Tabs on right -->
<TabControl TabStripPlacement="Right" />
```

When styling, adjust `BorderThickness` and `CornerRadius` based on placement:

| Placement | BorderThickness | CornerRadius |
|-----------|-----------------|--------------|
| Top | `1 1 1 0` | `4 4 0 0` |
| Bottom | `1 0 1 1` | `0 0 4 4` |
| Left | `1 1 0 1` | `4 0 0 4` |
| Right | `0 1 1 1` | `0 4 4 0` |

---

## Showcase Example

The Showcase app creates a fully custom TabControl style inline at `TestContent.xaml:2972-3095`:

```xml
<TabControl Grid.Row="1"
            Background="Transparent"
            BorderThickness="0">
    <TabControl.Resources>
        <!-- Custom brushes using theme colors -->
        <SolidColorBrush x:Key="TabItemHotBackground"
                         Color="{DynamicResource Fluent.Ribbon.Colors.Accent60}" />
        <SolidColorBrush x:Key="TabItemSelectedBackground"
                         Color="{DynamicResource Gray7}" />
        <SolidColorBrush x:Key="TabItemHotBorderBrush"
                         Color="{DynamicResource Fluent.Ribbon.Colors.Accent80}" />

        <!-- Full TabItem template with triggers -->
        <Style TargetType="{x:Type TabItem}">
            <!-- ... see source for full template ... -->
        </Style>
    </TabControl.Resources>

    <TabItem Header="Home">...</TabItem>
    <TabItem Header="Settings">...</TabItem>
</TabControl>
```

---

## Example: WPF Application Implementation

### Step 1: Add Style to App.xaml (or Window.Resources)

```xml
<Style x:Key="MainTabItem" TargetType="{x:Type TabItem}">
    <Setter Property="Background" Value="{DynamicResource Fluent.Ribbon.Brushes.White}" />
    <Setter Property="Foreground" Value="{DynamicResource Fluent.Ribbon.Brushes.Black}" />
    <Setter Property="Padding" Value="16 8" />
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type TabItem}">
                <Border x:Name="Border"
                        Padding="{TemplateBinding Padding}"
                        Background="{TemplateBinding Background}"
                        BorderBrush="{DynamicResource Fluent.Ribbon.Brushes.Gray6}"
                        BorderThickness="1 1 1 0"
                        CornerRadius="6 6 0 0">
                    <ContentPresenter ContentSource="Header"
                                      HorizontalAlignment="Center" />
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter TargetName="Border" Property="Background"
                                Value="{DynamicResource Fluent.Ribbon.Brushes.Button.MouseOver.Background}" />
                    </Trigger>
                    <Trigger Property="IsSelected" Value="True">
                        <Setter TargetName="Border" Property="Background"
                                Value="{DynamicResource Fluent.Ribbon.Brushes.Button.Pressed.Background}" />
                        <Setter Property="Panel.ZIndex" Value="1" />
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>

<Style x:Key="MainTabControl" TargetType="{x:Type TabControl}">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="BorderThickness" Value="0" />
    <Setter Property="Padding" Value="0" />
    <Style.Resources>
        <Style TargetType="{x:Type TabItem}" BasedOn="{StaticResource MainTabItem}" />
    </Style.Resources>
</Style>
```

### Step 2: Apply to TabControl

```xml
<TabControl Grid.Row="1" Style="{StaticResource MainTabControl}">
    <TabItem Header="Scan Results">
        <!-- Your scan results content -->
    </TabItem>
    <TabItem Header="History">
        <!-- Your history content -->
    </TabItem>
    <TabItem Header="Settings">
        <!-- Your settings content -->
    </TabItem>
</TabControl>
```

---

## DO NOT DO

```csharp
// WRONG - Don't walk visual tree to find tab elements
foreach (var child in GetVisualChildren(tabControl))
{
    if (child is TabItem tabItem)
        tabItem.Background = myBrush; // Don't do this
}

// WRONG - Don't use hardcoded colors
<TabItem Background="#0078D4" />  // Won't adapt to theme

// WRONG - Don't use non-existent resource keys
<SolidColorBrush Color="{DynamicResource Fluent.Ribbon.Colors.Gray7}" />
```

**Use DynamicResource bindings to Fluent.Ribbon brushes - they adapt to theme changes automatically.**

---

## Source Files

| File | Purpose |
|------|---------|
| `Fluent.Ribbon\Themes\Controls\BackstageControls.xaml:99-134` | InnerBackstageTabControl style |
| `Fluent.Ribbon.Showcase\TestContent.xaml:2972-3095` | Custom TabControl styling example |
| `Fluent.Ribbon\Themes\Common.xaml` | Default styles (no TabControl) |
| `Fluent.Ribbon\Themes\Themes\Theme.Template.xaml` | Gray color definitions |

---

## Summary

| Need | Solution |
|------|----------|
| Tabs inside Backstage | `Style="{DynamicResource Fluent.Ribbon.Styles.InnerBackstageTabControl}"` |
| Tabs in main content | Create custom style using `Fluent.Ribbon.Brushes.*` |
| Theme-aware hover | Use `Fluent.Ribbon.Brushes.Button.MouseOver.Background` |
| Theme-aware selected | Use `Fluent.Ribbon.Brushes.Button.Pressed.Background` |
| Text color | Use `Fluent.Ribbon.Brushes.Black` (adapts to light/dark) |
| Gray colors | Use short name like `Gray7`, NOT `Fluent.Ribbon.Colors.Gray7` |
