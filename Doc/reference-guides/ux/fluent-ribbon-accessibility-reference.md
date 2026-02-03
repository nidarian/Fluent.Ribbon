---
title: Fluent.Ribbon Accessibility Reference
description: UI Automation peers, screen reader support, and accessibility best practices
tags: [accessibility, automation, screen-reader, a11y]
see_also:
  - fluent-ribbon-keytips-reference.md
  - fluent-ribbon-screentip-reference.md
---

# Fluent.Ribbon Accessibility Reference

**Version:** Based on Fluent.Ribbon source analysis
**Last Updated:** 2026-01-23
**Source:** [Fluent.Ribbon/Automation/Peers](https://github.com/fluentribbon/Fluent.Ribbon/tree/develop/Fluent.Ribbon/Automation/Peers)

---

## Overview

Fluent.Ribbon provides comprehensive accessibility support through UI Automation peers. Every ribbon control has a corresponding `AutomationPeer` that exposes its functionality to screen readers and other assistive technologies.

---

## AutomationPeer Architecture

All Fluent.Ribbon automation peers inherit from WPF's `FrameworkElementAutomationPeer` or related base classes and are located in the `Fluent.Automation.Peers` namespace.

### Peer Creation

Each control creates its peer via `OnCreateAutomationPeer()`:

```csharp
protected override AutomationPeer OnCreateAutomationPeer()
    => new Fluent.Automation.Peers.RibbonButtonAutomationPeer(this);
```

---

## Available AutomationPeers

### Core Control Peers

| Peer Class | Control | Base Class |
|------------|---------|------------|
| `RibbonAutomationPeer` | `Ribbon` | `FrameworkElementAutomationPeer` |
| `RibbonControlAutomationPeer` | `RibbonControl` | `FrameworkElementAutomationPeer` |
| `RibbonTabControlAutomationPeer` | `RibbonTabControl` | `SelectorAutomationPeer` |
| `RibbonTabItemAutomationPeer` | `RibbonTabItem` | `TabItemAutomationPeer` |
| `RibbonGroupBoxAutomationPeer` | `RibbonGroupBox` | `ItemsControlAutomationPeer` |
| `RibbonTitleBarAutomationPeer` | `RibbonTitleBar` | `FrameworkElementAutomationPeer` |

### Button Control Peers

| Peer Class | Control | Pattern Support |
|------------|---------|-----------------|
| `RibbonButtonAutomationPeer` | `Button` | Invoke |
| `RibbonToggleButtonAutomationPeer` | `ToggleButton` | Toggle |
| `RibbonCheckBoxAutomationPeer` | `CheckBox` | Toggle |
| `RibbonRadioButtonAutomationPeer` | `RadioButton` | SelectionItem |
| `RibbonSplitButtonAutomationPeer` | `SplitButton` | Invoke, ExpandCollapse |
| `RibbonDropDownButtonAutomationPeer` | `DropDownButton` | ExpandCollapse |

### Input Control Peers

| Peer Class | Control | Pattern Support |
|------------|---------|-----------------|
| `RibbonTextBoxAutomationPeer` | `TextBox` | Value |
| `RibbonComboBoxAutomationPeer` | `ComboBox` | Selection, ExpandCollapse, Value |

### Backstage Peers

| Peer Class | Control |
|------------|---------|
| `RibbonBackstageAutomationPeer` | `Backstage` |
| `RibbonBackstageTabControlAutomationPeer` | `BackstageTabControl` |
| `RibbonBackstageTabItemAutomationPeer` | `BackstageTabItem` |

### Gallery Peers

| Peer Class | Control |
|------------|---------|
| `RibbonInRibbonGalleryAutomationPeer` | `InRibbonGallery` |
| `GalleryItemAutomationPeer` | `GalleryItem` |
| `GalleryItemWrapperAutomationPeer` | Gallery item wrapper |

### Other Peers

| Peer Class | Control |
|------------|---------|
| `RibbonQuickAccessToolBarAutomationPeer` | `QuickAccessToolBar` |
| `RibbonScreenTipAutomationPeer` | `ScreenTip` |
| `TwoLineLabelAutomationPeer` | `TwoLineLabel` |

---

## RibbonControlAutomationPeer

**Source:** `Automation\Peers\RibbonControlAutomationPeer.cs`

Base peer for `RibbonControl` that provides class name based on the actual control type.

```csharp
public class RibbonControlAutomationPeer : FrameworkElementAutomationPeer
{
    public RibbonControlAutomationPeer(RibbonControl owner)
        : base(owner)
    {
    }

    protected override string GetClassNameCore()
    {
        return this.Owner.GetType().Name;
    }
}
```

---

## Key Accessibility Features

### 1. KeyTips (Access Keys)

KeyTips provide keyboard access to all ribbon controls. When activated:
- Screen readers announce the KeyTip sequence
- Focus moves to the control
- Control is activated or opens submenu

```xml
<Fluent:Button Header="Save" KeyTip="S" />
```

### 2. ScreenTips (Enhanced Tooltips)

ScreenTips provide detailed descriptions for screen readers:

```xml
<Fluent:Button Header="Save">
    <Fluent:Button.ToolTip>
        <Fluent:ScreenTip Title="Save (Ctrl+S)"
                         Text="Save the current document."
                         HelpTopic="save-command" />
    </Fluent:Button.ToolTip>
</Fluent:Button>
```

### 3. Automation Properties

Standard WPF automation properties work with ribbon controls:

```xml
<Fluent:Button Header="Delete"
               AutomationProperties.Name="Delete selected items"
               AutomationProperties.HelpText="Permanently removes selected items" />
```

---

## UI Automation Patterns

### Invoke Pattern (Buttons)

```csharp
// Screen reader can invoke buttons
var pattern = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
pattern?.Invoke();
```

### Toggle Pattern (ToggleButtons, CheckBoxes)

```csharp
// Screen reader can toggle state
var pattern = peer.GetPattern(PatternInterface.Toggle) as IToggleProvider;
pattern?.Toggle();
```

### ExpandCollapse Pattern (DropDowns, SplitButtons)

```csharp
// Screen reader can expand/collapse
var pattern = peer.GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
pattern?.Expand();
pattern?.Collapse();
```

The `RibbonGroupBoxAutomationPeer` raises ExpandCollapse events when the group popup opens/closes:

```csharp
// In RibbonGroupBoxAutomationPeer
internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
{
    this.RaisePropertyChangedEvent(
        ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
        oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
        newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
}
```

### Selection Pattern (TabControls, ComboBoxes)

```csharp
// Screen reader can navigate selections
var pattern = peer.GetPattern(PatternInterface.Selection) as ISelectionProvider;
var items = pattern?.GetSelection();
```

---

## Accessibility Best Practices

### 1. Provide Meaningful Names

```xml
<!-- GOOD - Descriptive name -->
<Fluent:Button Header="Save Document"
               AutomationProperties.Name="Save Document" />

<!-- BAD - Icon-only without name -->
<Fluent:Button Icon="{StaticResource SaveIcon}" />

<!-- FIXED - Icon with accessible name -->
<Fluent:Button Icon="{StaticResource SaveIcon}"
               AutomationProperties.Name="Save Document" />
```

### 2. Use ScreenTips for Help Text

```xml
<Fluent:Button Header="Format">
    <Fluent:Button.ToolTip>
        <Fluent:ScreenTip Title="Format Painter"
                         Text="Copy formatting from one place and apply it to another."
                         DisableReason="Select text first to enable Format Painter." />
    </Fluent:Button.ToolTip>
</Fluent:Button>
```

### 3. Group Related Controls

```xml
<Fluent:RibbonGroupBox Header="Clipboard"
                       AutomationProperties.Name="Clipboard commands">
    <Fluent:Button Header="Cut" KeyTip="X" />
    <Fluent:Button Header="Copy" KeyTip="C" />
    <Fluent:Button Header="Paste" KeyTip="V" />
</Fluent:RibbonGroupBox>
```

### 4. Provide Keyboard Navigation

All controls should be keyboard accessible:
- Tab navigation between groups
- Arrow keys within groups
- KeyTips for direct access
- Escape to close popups

### 5. Announce State Changes

Use `AutomationPeer.RaisePropertyChangedEvent` for state changes:

```csharp
// Example from RibbonGroupBoxAutomationPeer
peer.RaisePropertyChangedEvent(
    ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
    ExpandCollapseState.Collapsed,
    ExpandCollapseState.Expanded);
```

---

## Testing Accessibility

### Using Inspect.exe

1. Open Inspect.exe from Windows SDK
2. Hover over ribbon controls
3. Verify:
   - Control type is correct
   - Name is meaningful
   - Patterns are available
   - State is accurate

### Using Narrator

1. Enable Windows Narrator (Win + Ctrl + Enter)
2. Navigate the ribbon using Tab and Arrow keys
3. Verify:
   - All controls are announced
   - States are read correctly
   - KeyTips are functional

### Programmatic Testing

```csharp
// Get automation peer
var peer = UIElementAutomationPeer.CreatePeerForElement(myButton);

// Verify properties
Assert.IsNotNull(peer);
Assert.AreEqual("Save", peer.GetName());
Assert.AreEqual(AutomationControlType.Button, peer.GetAutomationControlType());

// Verify patterns
var invokeProvider = peer.GetPattern(PatternInterface.Invoke);
Assert.IsNotNull(invokeProvider);
```

---

## Common Accessibility Issues

### Issue: Icon-Only Buttons

**Problem:** Buttons without text have no accessible name.

```xml
<!-- PROBLEM -->
<Fluent:Button Icon="{StaticResource Icon}" />
```

**Solution:**

```xml
<Fluent:Button Icon="{StaticResource Icon}"
               AutomationProperties.Name="Save" />
```

### Issue: Dynamic Content

**Problem:** Dynamically changing content not announced.

**Solution:** Raise property changed events:

```csharp
AutomationPeer peer = UIElementAutomationPeer.FromElement(element);
peer?.RaisePropertyChangedEvent(
    AutomationElementIdentifiers.NameProperty,
    oldValue,
    newValue);
```

### Issue: Custom Controls

**Problem:** Custom controls lack automation support.

**Solution:** Create custom AutomationPeer:

```csharp
public class MyControlAutomationPeer : FrameworkElementAutomationPeer
{
    public MyControlAutomationPeer(MyControl owner) : base(owner) { }

    protected override string GetClassNameCore() => "MyControl";

    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.Custom;

    protected override string GetNameCore()
        => ((MyControl)Owner).Header?.ToString() ?? base.GetNameCore();
}
```

---

## Localization Considerations

Accessible names should be localized:

```xml
<Fluent:Button Header="{x:Static props:Resources.Save}"
               AutomationProperties.Name="{x:Static props:Resources.SaveAccessibleName}" />
```

---

## DO NOT DO

### Don't Skip AutomationProperties
```xml
<!-- WRONG - No accessible name for icon button -->
<Fluent:Button Icon="{StaticResource DeleteIcon}" />

<!-- RIGHT -->
<Fluent:Button Icon="{StaticResource DeleteIcon}"
               AutomationProperties.Name="Delete" />
```

### Don't Use Visual-Only Indicators
```xml
<!-- WRONG - Color is the only indicator -->
<Fluent:Button Background="Red" />

<!-- RIGHT - Include text or accessible name -->
<Fluent:Button Background="Red"
               Header="Error"
               AutomationProperties.Name="Error occurred" />
```

### Don't Ignore Focus Indicators
```xml
<!-- WRONG - Removes focus visibility -->
<Style TargetType="Fluent:Button">
    <Setter Property="FocusVisualStyle" Value="{x:Null}" />
</Style>

<!-- Let default focus styles work -->
```

---

## Quick Reference

| Feature | Implementation |
|---------|----------------|
| Control Names | `AutomationProperties.Name` |
| Help Text | `AutomationProperties.HelpText` or `ScreenTip` |
| Keyboard Access | `KeyTip` property |
| Group Labels | `RibbonGroupBox.Header` + `AutomationProperties.Name` |
| State Announcements | `AutomationPeer.RaisePropertyChangedEvent` |
| Custom Controls | Create `AutomationPeer` subclass |

### AutomationPeer by Control

| Control | Peer | Primary Pattern |
|---------|------|-----------------|
| `Button` | `RibbonButtonAutomationPeer` | Invoke |
| `ToggleButton` | `RibbonToggleButtonAutomationPeer` | Toggle |
| `DropDownButton` | `RibbonDropDownButtonAutomationPeer` | ExpandCollapse |
| `SplitButton` | `RibbonSplitButtonAutomationPeer` | Invoke, ExpandCollapse |
| `RibbonGroupBox` | `RibbonGroupBoxAutomationPeer` | ExpandCollapse |
| `TextBox` | `RibbonTextBoxAutomationPeer` | Value |
| `ComboBox` | `RibbonComboBoxAutomationPeer` | Selection, ExpandCollapse |

---

*Reference verified against Fluent.Ribbon source code as of 2026-01-23.*
