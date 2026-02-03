---
title: Fluent.Ribbon Attached Properties Reference
description: RibbonProperties class - Size, SizeDefinition, MouseOverBackground, and more
tags: [attached-properties, ribbonproperties, sizing, advanced]
see_also:
  - fluent-ribbon-state-diagrams-v2.md
  - ../controls/fluent-ribbon-groupbox-reference.md
  - ../styling/fluent-ribbon-layout-reference-v2.md
---

# Fluent.Ribbon Attached Properties Reference

**Version:** Based on Fluent.Ribbon source analysis
**Last Updated:** 2026-01-23
**Source:** [Fluent.Ribbon/AttachedProperties/RibbonProperties.cs](https://github.com/fluentribbon/Fluent.Ribbon/blob/develop/Fluent.Ribbon/AttachedProperties/RibbonProperties.cs)

---

## Overview

Fluent.Ribbon provides attached properties through the `RibbonProperties` class that can be applied to any `DependencyObject`. These properties control sizing, visual states, and icon behavior across ribbon controls.

---

## RibbonProperties Class

**Namespace:** `Fluent`

All attached properties are accessed via:
```xml
xmlns:Fluent="urn:fluent-ribbon"

<Button Fluent:RibbonProperties.Size="Large" />
```

---

## Size Properties

### Size

Controls the current display size of a ribbon control.

| Property | Type | Default |
|----------|------|---------|
| `Size` | `RibbonControlSize` | `Large` |

#### RibbonControlSize Values
| Value | Description |
|-------|-------------|
| `Large` | Full size with icon and text stacked |
| `Middle` | Compact with icon and text horizontal |
| `Small` | Minimal, typically icon only |

#### Usage

```xml
<!-- Force a control to always be small -->
<Fluent:Button Fluent:RibbonProperties.Size="Small" />
```

```csharp
// Set programmatically
RibbonProperties.SetSize(myButton, RibbonControlSize.Middle);

// Get current size
var size = RibbonProperties.GetSize(myButton);
```

#### Framework Metadata
- `AffectsArrange`: Yes
- `AffectsMeasure`: Yes
- `AffectsRender`: Yes
- `AffectsParentArrange`: Yes
- `AffectsParentMeasure`: Yes

---

### SizeDefinition

Defines how a control should resize as the ribbon group collapses.

| Property | Type | Default |
|----------|------|---------|
| `SizeDefinition` | `RibbonControlSizeDefinition` | `Large,Middle,Small` |

The definition specifies sizes for each group state:
- **Position 1:** Size when group is Large
- **Position 2:** Size when group is Middle
- **Position 3:** Size when group is Small/Collapsed

#### Usage

```xml
<!-- Standard progression -->
<Fluent:Button Fluent:RibbonProperties.SizeDefinition="Large,Middle,Small" />

<!-- Always stay large until collapsed -->
<Fluent:Button Fluent:RibbonProperties.SizeDefinition="Large,Large,Small" />

<!-- Always small -->
<Fluent:Button Fluent:RibbonProperties.SizeDefinition="Small,Small,Small" />

<!-- Jump from large to small (skip middle) -->
<Fluent:Button Fluent:RibbonProperties.SizeDefinition="Large,Small,Small" />
```

```csharp
// Programmatic access
var definition = new RibbonControlSizeDefinition(
    RibbonControlSize.Large,
    RibbonControlSize.Middle,
    RibbonControlSize.Small);
RibbonProperties.SetSizeDefinition(myButton, definition);
```

---

### SimplifiedSizeDefinition

Defines sizing when the ribbon is in simplified mode.

| Property | Type | Default |
|----------|------|---------|
| `SimplifiedSizeDefinition` | `RibbonControlSizeDefinition` | `Large,Middle,Small` |

#### Usage

```xml
<!-- In simplified mode, keep control middle-sized -->
<Fluent:Button Fluent:RibbonProperties.SimplifiedSizeDefinition="Middle,Middle,Small" />
```

---

## Visual State Properties

### MouseOverBackground

Background brush when mouse hovers over the control.

| Property | Type | Default |
|----------|------|---------|
| `MouseOverBackground` | `Brush` | `null` |

```xml
<Fluent:Button Fluent:RibbonProperties.MouseOverBackground="#FF3399FF" />
```

---

### PressedBackground

Background brush when the control is pressed.

| Property | Type | Default |
|----------|------|---------|
| `PressedBackground` | `Brush` | `null` |

```xml
<Fluent:Button Fluent:RibbonProperties.PressedBackground="#FF0066CC" />
```

---

### MouseOverForeground

Foreground brush when mouse hovers over the control.

| Property | Type | Default |
|----------|------|---------|
| `MouseOverForeground` | `Brush` | `null` |

```xml
<Fluent:Button Fluent:RibbonProperties.MouseOverForeground="White" />
```

---

### IsSelectedBackground

Background brush when the control is selected.

| Property | Type | Default |
|----------|------|---------|
| `IsSelectedBackground` | `Brush` | `null` |

```xml
<Fluent:ToggleButton Fluent:RibbonProperties.IsSelectedBackground="#FF00AA00" />
```

---

## Icon Properties

### IconSize

Specifies the desired icon size for the element.

| Property | Type | Default |
|----------|------|---------|
| `IconSize` | `IconSize` | `Small` |

#### IconSize Values
| Value | Description |
|-------|-------------|
| `Small` | 16x16 pixels |
| `Medium` | 24x24 pixels |
| `Large` | 32x32 pixels |
| `Custom` | Use `CustomIconSize` property |

```xml
<Fluent:Button Fluent:RibbonProperties.IconSize="Medium" />
```

---

### CustomIconSize

Defines a custom icon size when `IconSize` is set to `Custom`.

| Property | Type | Default |
|----------|------|---------|
| `CustomIconSize` | `Size` | `0,0` |

```xml
<Fluent:Button Fluent:RibbonProperties.IconSize="Custom"
               Fluent:RibbonProperties.CustomIconSize="48,48" />
```

---

## Utility Properties

### LastVisibleWidth

Stores the last visible width of an element. Used internally for layout calculations.

| Property | Type | Default |
|----------|------|---------|
| `LastVisibleWidth` | `double` | `0` |

**Note:** This is primarily for internal use by the ribbon layout system.

---

### IsElementInQuickAccessToolBar

Indicates whether an element is currently part of the Quick Access Toolbar.

| Property | Type | Default |
|----------|------|---------|
| `IsElementInQuickAccessToolBar` | `bool` | `false` |

```csharp
// Check if control is in QAT
bool isInQat = RibbonProperties.GetIsElementInQuickAccessToolBar(myButton);
```

Useful for styling controls differently when in the QAT:

```xml
<Style TargetType="Fluent:Button">
    <Style.Triggers>
        <Trigger Property="Fluent:RibbonProperties.IsElementInQuickAccessToolBar" Value="True">
            <Setter Property="Foreground" Value="White" />
        </Trigger>
    </Style.Triggers>
</Style>
```

---

### CornerRadius

Defines corner radius used in control template parts.

| Property | Type | Default |
|----------|------|---------|
| `CornerRadius` | `CornerRadius` | `0` |

```xml
<Fluent:Button Fluent:RibbonProperties.CornerRadius="4" />
```

---

## Helper Methods

### SetAppropriateSize

Sets the appropriate size of a control based on group state and size definition.

```csharp
// Based on group box state
RibbonProperties.SetAppropriateSize(
    element: myButton,
    state: RibbonGroupBoxState.Middle,
    isSimplified: false);

// Based on ribbon control size
RibbonProperties.SetAppropriateSize(
    element: myButton,
    size: RibbonControlSize.Middle);
```

---

### FindParentRibbonGroupBox

Finds the parent `RibbonGroupBox` of an element.

```csharp
// Internal helper method
RibbonGroupBox? groupBox = RibbonProperties.FindParentRibbonGroupBox(myButton);
```

---

## Styling Examples

### Custom Button Style with Visual States

```xml
<Style TargetType="Fluent:Button" x:Key="HighlightButton">
    <Setter Property="Fluent:RibbonProperties.MouseOverBackground"
            Value="{DynamicResource Fluent.Ribbon.Brushes.Button.MouseOver.Background}" />
    <Setter Property="Fluent:RibbonProperties.PressedBackground"
            Value="{DynamicResource Fluent.Ribbon.Brushes.Button.Pressed.Background}" />
    <Setter Property="Fluent:RibbonProperties.MouseOverForeground"
            Value="{DynamicResource Fluent.Ribbon.Brushes.Button.MouseOver.Foreground}" />
</Style>
```

### Size-Aware Styling

```xml
<Style TargetType="Fluent:Button">
    <Style.Triggers>
        <Trigger Property="Fluent:RibbonProperties.Size" Value="Small">
            <Setter Property="Padding" Value="2" />
        </Trigger>
        <Trigger Property="Fluent:RibbonProperties.Size" Value="Middle">
            <Setter Property="Padding" Value="4" />
        </Trigger>
        <Trigger Property="Fluent:RibbonProperties.Size" Value="Large">
            <Setter Property="Padding" Value="6" />
        </Trigger>
    </Style.Triggers>
</Style>
```

### QAT-Specific Styling

```xml
<Style TargetType="Fluent:Button">
    <Style.Triggers>
        <Trigger Property="Fluent:RibbonProperties.IsElementInQuickAccessToolBar" Value="True">
            <Setter Property="Fluent:RibbonProperties.IconSize" Value="Small" />
            <Setter Property="ToolTipService.ShowOnDisabled" Value="True" />
        </Trigger>
    </Style.Triggers>
</Style>
```

---

## Common Patterns

### Force Consistent Sizing

```xml
<!-- Keep button large regardless of group collapse -->
<Fluent:Button Header="Important"
               Fluent:RibbonProperties.SizeDefinition="Large,Large,Large" />
```

### Custom Visual Feedback

```xml
<Fluent:Button Header="Delete"
               Fluent:RibbonProperties.MouseOverBackground="#FFFF6666"
               Fluent:RibbonProperties.PressedBackground="#FFFF0000"
               Fluent:RibbonProperties.MouseOverForeground="White" />
```

### Mixed Icon Sizes

```xml
<Fluent:RibbonGroupBox Header="Actions">
    <Fluent:Button Header="Save"
                   Fluent:RibbonProperties.IconSize="Large" />
    <Fluent:Button Header="Open"
                   Fluent:RibbonProperties.IconSize="Medium" />
    <Fluent:Button Header="New"
                   Fluent:RibbonProperties.IconSize="Small" />
</Fluent:RibbonGroupBox>
```

---

## DO NOT DO

### Don't Override Size Directly When SizeDefinition Exists
```xml
<!-- WRONG - Size will be overwritten by layout system -->
<Fluent:Button Fluent:RibbonProperties.Size="Large"
               Fluent:RibbonProperties.SizeDefinition="Middle,Small,Small" />

<!-- RIGHT - Use SizeDefinition to control all states -->
<Fluent:Button Fluent:RibbonProperties.SizeDefinition="Large,Large,Large" />
```

### Don't Rely on LastVisibleWidth
```csharp
// WRONG - Internal property, may change
double width = RibbonProperties.GetLastVisibleWidth(element);
// Use for calculations...

// RIGHT - Use ActualWidth for layout calculations
double width = element.ActualWidth;
```

---

## Quick Reference

| Property | Type | Use Case |
|----------|------|----------|
| `Size` | `RibbonControlSize` | Current display size |
| `SizeDefinition` | `RibbonControlSizeDefinition` | Size progression during collapse |
| `SimplifiedSizeDefinition` | `RibbonControlSizeDefinition` | Sizing in simplified mode |
| `MouseOverBackground` | `Brush` | Hover background |
| `PressedBackground` | `Brush` | Pressed background |
| `MouseOverForeground` | `Brush` | Hover text color |
| `IsSelectedBackground` | `Brush` | Selected/checked background |
| `IconSize` | `IconSize` | Icon dimensions |
| `CustomIconSize` | `Size` | Custom icon dimensions |
| `IsElementInQuickAccessToolBar` | `bool` | Check if in QAT |
| `CornerRadius` | `CornerRadius` | Border radius |

---

*Reference verified against Fluent.Ribbon source code as of 2026-01-23.*
