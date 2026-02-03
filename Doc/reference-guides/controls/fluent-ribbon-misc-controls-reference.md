---
title: Fluent.Ribbon Miscellaneous Controls Reference
description: Reference for TwoLineLabel, Separator, and interfaces
tags: [twolinelabel, separator, interfaces, controls]
see_also:
  - fluent-ribbon-button-controls-reference.md
  - ../glossary.md
---

# Fluent.Ribbon Miscellaneous Controls Reference

**Version:** Based on Fluent.Ribbon source analysis
**Last Updated:** 2026-01-23
**Source:** [Fluent.Ribbon/Controls](https://github.com/fluentribbon/Fluent.Ribbon/tree/develop/Fluent.Ribbon/Controls)

---

## Overview

This reference covers specialized Fluent.Ribbon controls that don't fit into other categories: `TwoLineLabel` for button text display, and various utility controls.

---

## TwoLineLabel

**Source File:** `Controls\TwoLineLabel.cs`

A specialized label control that automatically splits text across two lines for use in large ribbon buttons.

### Class Definition

```csharp
[DefaultProperty(nameof(Text))]
[ContentProperty(nameof(Text))]
[TemplatePart(Name = "PART_TextRun", Type = typeof(AccessText))]
[TemplatePart(Name = "PART_TextRun2", Type = typeof(AccessText))]
public class TwoLineLabel : Control
```

### Template Parts

| Part Name | Type | Description |
|-----------|------|-------------|
| `PART_TextRun` | `AccessText` | First line of text |
| `PART_TextRun2` | `AccessText` | Second line of text |

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Text` | `string` | `""` | The text to display |
| `HasTwoLines` | `bool` | `true` | Whether to split into two lines |
| `HasGlyph` | `bool` | `false` | Whether text has dropdown glyph |

### Text Splitting Algorithm

When `HasTwoLines` is true, the control splits text:

1. **Soft Hyphen (Unicode 173):** If found, breaks at hyphen position and displays a visible hyphen
2. **Space Near Center:** Finds the space closest to the center of the text
3. **No Space:** Displays as single line

```csharp
// Example splits:
"Format Painter" → "Format" / "Painter"
"Save As" → "Save" / "As"
"PasteSpecial" → "PasteSpecial" (no space, stays one line)
"Quick­Print" → "Quick-" / "Print" (soft hyphen at ­)
```

### Basic Usage

```xml
<Fluent:TwoLineLabel Text="Format Painter" HasTwoLines="True" />
```

### In Button Context

TwoLineLabel is typically used internally by large buttons:

```xml
<Fluent:Button Header="Format Painter"
               SizeDefinition="Large,Large,Large">
    <!-- Internal TwoLineLabel displays "Format" on first line, "Painter" on second -->
</Fluent:Button>
```

### Control Soft Hyphen Placement

Use soft hyphen (Unicode 173, `&#173;`) to control break point:

```xml
<!-- Break after "Auto" -->
<Fluent:Button Header="Auto&#173;Correct" />

<!-- Displays as:
     Auto-
     Correct
-->
```

### Single Line Mode

```xml
<Fluent:TwoLineLabel Text="Short Label" HasTwoLines="False" />
```

### AutomationPeer

Has dedicated automation peer:
```csharp
protected override AutomationPeer OnCreateAutomationPeer()
    => new Fluent.Automation.Peers.TwoLineLabelAutomationPeer(this);
```

---

## Separator

A visual separator between ribbon controls.

### Basic Usage

```xml
<Fluent:RibbonGroupBox Header="Clipboard">
    <Fluent:Button Header="Paste" />
    <Fluent:Separator />
    <Fluent:Button Header="Cut" />
    <Fluent:Button Header="Copy" />
</Fluent:RibbonGroupBox>
```

### Orientation

```xml
<!-- Vertical separator (default in horizontal layout) -->
<Fluent:Separator />
```

---

## RibbonControl (Base Class)

**Source File:** `Controls\RibbonControl.cs`

Base class for most ribbon controls. Provides common functionality.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | `object` | Control label |
| `Icon` | `object` | Small icon (16x16) |
| `Size` | `RibbonControlSize` | Current display size |
| `SizeDefinition` | `RibbonControlSizeDefinition` | Size behavior |
| `KeyTip` | `string` | Keyboard access key |
| `CanAddToQuickAccessToolBar` | `bool` | QAT eligibility |

### Static Helper Methods

```csharp
// Bind source property to target element
public static void Bind(object source, FrameworkElement target,
    string path, DependencyProperty property, BindingMode mode)

// Standard quick access item binding
public static void BindQuickAccessItem(IRibbonControl source, FrameworkElement target)

// Get work area for popup positioning
public static Rect GetControlWorkArea(FrameworkElement control)
```

---

## RibbonControlSize Enum

```csharp
public enum RibbonControlSize
{
    Large,   // Full size with stacked icon/text
    Middle,  // Compact horizontal layout
    Small    // Minimal, icon only
}
```

---

## RibbonControlSizeDefinition

Defines how a control resizes as its parent group box collapses.

### String Syntax

```xml
<!-- Three sizes: Large state, Middle state, Small/Collapsed state -->
<Fluent:Button SizeDefinition="Large,Middle,Small" />
```

### Programmatic Creation

```csharp
var definition = new RibbonControlSizeDefinition(
    RibbonControlSize.Large,
    RibbonControlSize.Middle,
    RibbonControlSize.Small);
```

### Common Patterns

| Definition | Behavior |
|------------|----------|
| `Large,Middle,Small` | Standard progression |
| `Large,Large,Small` | Stay large until collapsed |
| `Middle,Middle,Small` | Always middle or small |
| `Small,Small,Small` | Always small |
| `Large,Small,Small` | Jump from large to small |

---

## ScopeGuard (Internal)

**Source File:** Internal helper class

Used internally to manage state during cache operations:

```csharp
internal class ScopeGuard
{
    public bool IsActive { get; }
    public IDisposable Start();
}
```

Used in `RibbonGroupBox` for cache management:

```csharp
// Prevents cache reset during measuring
using (this.CacheResetGuard.Start())
{
    // Operations here don't trigger cache reset
}
```

---

## ItemContainerGeneratorAction (Internal)

**Source File:** Internal helper class

Queues actions to execute when ItemContainerGenerator is ready:

```csharp
internal class ItemContainerGeneratorAction
{
    public ItemContainerGeneratorAction(
        ItemContainerGenerator generator,
        Action action);

    public void QueueAction();
}
```

Used for deferred child updates:

```csharp
this.updateChildSizesItemContainerGeneratorAction.QueueAction();
```

---

## DropDownHelper (Internal)

**Source File:** `Helpers\DropDownHelper.cs`

Helper for dropdown popup positioning and sizing.

```csharp
// Coerce MaxDropDownHeight based on available screen space
public static object CoerceMaxDropDownHeight(
    DependencyObject d,
    object basevalue);
```

---

## UIHelper (Internal)

**Source File:** `Internal\UIHelper.cs`

Utility methods for visual tree navigation:

```csharp
// Find parent of specific type
public static T? GetParent<T>(DependencyObject element) where T : DependencyObject;

// Get first visual child
public static DependencyObject? GetFirstVisualChild(DependencyObject element);
```

---

## LogicalChildSupportHelper (Internal)

**Source File:** `Helpers\LogicalChildSupportHelper.cs`

Helper for managing logical tree for custom properties:

```csharp
// Property changed callback that manages logical children
public static void OnLogicalChildPropertyChanged(
    DependencyObject d,
    DependencyPropertyChangedEventArgs e);
```

Used when controls need logical children for properties like `Icon`:

```csharp
public static readonly DependencyProperty IconProperty =
    DependencyProperty.Register(
        nameof(Icon),
        typeof(object),
        typeof(RibbonControl),
        new PropertyMetadata(LogicalChildSupportHelper.OnLogicalChildPropertyChanged));
```

---

## Interface Reference

### IQuickAccessItemProvider

```csharp
public interface IQuickAccessItemProvider
{
    bool CanAddToQuickAccessToolBar { get; set; }
    FrameworkElement CreateQuickAccessItem();
}
```

### IRibbonControl

```csharp
public interface IRibbonControl : IHeaderedControl, IKeyTipedControl
{
    RibbonControlSize Size { get; set; }
    RibbonControlSizeDefinition SizeDefinition { get; set; }
    object? Icon { get; set; }
}
```

### IKeyTipedControl

```csharp
public interface IKeyTipedControl
{
    string? KeyTip { get; set; }
    KeyTipPressedResult OnKeyTipPressed();
    void OnKeyTipBack();
}
```

### IDropDownControl

```csharp
public interface IDropDownControl
{
    Popup? DropDownPopup { get; }
    bool IsDropDownOpen { get; set; }
    bool IsContextMenuOpened { get; set; }
    event EventHandler? DropDownOpened;
    event EventHandler? DropDownClosed;
}
```

### ISimplifiedRibbonControl

```csharp
public interface ISimplifiedRibbonControl : ISimplifiedStateControl
{
    RibbonControlSizeDefinition SimplifiedSizeDefinition { get; set; }
}

public interface ISimplifiedStateControl
{
    bool IsSimplified { get; }
    void UpdateSimplifiedState(bool isSimplified);
}
```

### IMediumIconProvider

```csharp
public interface IMediumIconProvider
{
    object? MediumIcon { get; set; }
}
```

### ILargeIconProvider

```csharp
public interface ILargeIconProvider
{
    object? LargeIcon { get; set; }
}
```

### IScalableRibbonControl

```csharp
public interface IScalableRibbonControl
{
    void Enlarge();
    void Reduce();
    void ResetScale();
}
```

---

## KeyTipPressedResult

Result structure for KeyTip activation:

```csharp
public readonly struct KeyTipPressedResult
{
    public bool Handled { get; }
    public bool HasActivePopup { get; }

    public static KeyTipPressedResult Empty { get; }

    public KeyTipPressedResult(bool handled, bool hasActivePopup);
}
```

---

## DO NOT DO

### Don't Manually Set TwoLineLabel Breaking
```xml
<!-- WRONG - Don't try to control line breaks with newlines -->
<Fluent:TwoLineLabel Text="Format&#10;Painter" />

<!-- RIGHT - Let the control handle splitting -->
<Fluent:TwoLineLabel Text="Format Painter" />

<!-- Or use soft hyphen for specific break point -->
<Fluent:TwoLineLabel Text="Format&#173;Painter" />
```

### Don't Bypass Interface Implementations
```csharp
// WRONG - Don't cast and call directly
((ISimplifiedStateControl)control).UpdateSimplifiedState(true);

// RIGHT - Let the ribbon manage simplified state
// It will call UpdateSimplifiedState when needed
```

---

## Quick Reference

### TwoLineLabel Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Text` | `string` | `""` | Text content |
| `HasTwoLines` | `bool` | `true` | Enable line splitting |
| `HasGlyph` | `bool` | `false` | Has dropdown indicator |

### Common Interfaces

| Interface | Purpose |
|-----------|---------|
| `IRibbonControl` | Base ribbon control interface |
| `IQuickAccessItemProvider` | QAT support |
| `IKeyTipedControl` | KeyTip support |
| `IDropDownControl` | Dropdown popup support |
| `ISimplifiedRibbonControl` | Simplified mode support |
| `IMediumIconProvider` | Medium icon support |
| `ILargeIconProvider` | Large icon support |
| `IScalableRibbonControl` | Scalable size support |

---

*Reference verified against Fluent.Ribbon source code as of 2026-01-23.*
