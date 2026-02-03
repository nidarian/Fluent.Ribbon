---
title: Fluent.Ribbon Simplified Ribbon Mode Reference
description: Complete reference for the Simplified Ribbon Mode feature
tags: [simplified, collapse, ribbon-mode, ux]
see_also:
  - ../controls/fluent-ribbon-groupbox-reference.md
  - ../advanced/fluent-ribbon-attached-properties-reference.md
---

# Fluent.Ribbon Simplified Ribbon Mode Reference

**Complete reference for the Simplified Ribbon Mode feature in Fluent.Ribbon.**

---

## Overview

Simplified Ribbon Mode is an alternative, compact display mode for the ribbon that reduces vertical space usage while maintaining functionality. This feature mimics the "Simplified Ribbon" option found in Microsoft Office applications.

```
CLASSIC RIBBON MODE:
+----------------------------------------------------------------------+
| File | Home | Insert | View |                                        |
+----------------------------------------------------------------------+
| +------------+ +------------+ +------------+ +-------------------+   |
| |  [ICON]    | |  [ICON]    | |  [ICON]    | | [ICON] | [ICON]   |   |
| |            | |            | |            | | Cut    | Paste    |   |
| |   Paste    | |    Copy    | |    Cut     | | [ICON] |          |   |
| +------------+ +------------+ +------------+ +-------------------+   |
|     Paste          Copy          Cut             Clipboard           |
+----------------------------------------------------------------------+
| Content Area                                                         |
+----------------------------------------------------------------------+

SIMPLIFIED RIBBON MODE:
+----------------------------------------------------------------------+
| File | Home | Insert | View |                                        |
+----------------------------------------------------------------------+
| [Paste][Copy][Cut] [Bold][Italic] [Align] [Find]  [Icon][Icon][Icon] |
+----------------------------------------------------------------------+
| Content Area                                                         |
+----------------------------------------------------------------------+
```

In simplified mode:
- Controls display horizontally in a single row
- Large icons become medium-sized
- Text labels are hidden for most controls (shown only for "Large" size)
- Group boxes no longer show headers by default
- Overall ribbon height is significantly reduced

---

## Component Hierarchy

```
fluent:Ribbon
+-- CanUseSimplified (bool) - Enables/disables simplified mode feature
+-- IsSimplified (bool) - Current mode state
|
+-- fluent:RibbonTabControl
|   +-- CanUseSimplified (synced from Ribbon)
|   +-- IsSimplified (synced from Ribbon)
|
+-- fluent:RibbonTabItem
    +-- IsSimplified (read-only, propagated from parent)
    |
    +-- fluent:RibbonGroupBox
        +-- IsSimplified (read-only, propagated from parent)
        +-- StateDefinition (classic mode states)
        +-- SimplifiedStateDefinition (simplified mode states)
        |
        +-- Control Items (Button, DropDownButton, etc.)
            +-- IsSimplified (read-only, propagated from parent)
            +-- SizeDefinition (classic mode sizing)
            +-- SimplifiedSizeDefinition (simplified mode sizing)
```

---

## Ribbon Properties

### CanUseSimplified

Enables or disables the ability to switch to simplified mode.

| Aspect | Value |
|--------|-------|
| Type | `bool` |
| Default | `false` |
| Dependency Property | `CanUseSimplifiedProperty` |

When `CanUseSimplified` is `true`:
- Context menu shows "Use Classic Ribbon" / "Use Simplified Ribbon" options
- Display Options dropdown (ribbon icon button) shows layout options
- Keyboard shortcut `Ctrl+F2` toggles simplified mode

When `CanUseSimplified` is `false`:
- Toggle options are hidden from all menus
- `IsSimplified` can still be set programmatically

```xml
<fluent:Ribbon CanUseSimplified="True" />
```

### IsSimplified

Gets or sets whether the ribbon is currently in simplified mode.

| Aspect | Value |
|--------|-------|
| Type | `bool` |
| Default | `false` |
| Dependency Property | `IsSimplifiedProperty` |

```xml
<fluent:Ribbon CanUseSimplified="True"
               IsSimplified="{Binding UseSimplifiedRibbon}" />
```

When `IsSimplified` changes:
1. All `RibbonTabItem` controls are notified via `ISimplifiedStateControl.UpdateSimplifiedState()`
2. Each `RibbonGroupBox` is notified and updates its state
3. Each control within groups is notified and adapts its appearance

---

## RibbonGroupBox Properties

### StateDefinition

Defines the state transitions for classic (non-simplified) mode.

| Aspect | Value |
|--------|-------|
| Type | `RibbonGroupBoxStateDefinition` |
| Default | `null` (all states available) |

```xml
<fluent:RibbonGroupBox Header="Clipboard"
                       StateDefinition="Large,Middle,Small,Collapsed">
```

### SimplifiedStateDefinition

Defines the state transitions for simplified mode.

| Aspect | Value |
|--------|-------|
| Type | `RibbonGroupBoxStateDefinition` |
| Default | `"Large,Middle,Collapsed"` |

```xml
<fluent:RibbonGroupBox Header="Clipboard"
                       SimplifiedStateDefinition="Large,Middle,Collapsed">
```

The default simplified state definition skips the "Small" state, meaning groups transition from Middle directly to Collapsed when space is constrained.

### IsSimplified (Read-Only)

Indicates whether the parent ribbon is in simplified mode.

| Aspect | Value |
|--------|-------|
| Type | `bool` |
| Default | `false` |
| Access | Read-only (set internally) |

Used primarily in styles and templates:

```xml
<Style TargetType="fluent:RibbonGroupBox">
    <Style.Triggers>
        <Trigger Property="IsSimplified" Value="True">
            <Setter Property="Padding" Value="2 0" />
        </Trigger>
    </Style.Triggers>
</Style>
```

---

## Control Properties

### SizeDefinition

Defines how control size adapts to group box state in classic mode.

| Aspect | Value |
|--------|-------|
| Type | `RibbonControlSizeDefinition` |
| Default | `"Large,Middle,Small"` |

Format: `"Large,Middle,Small"` where each value corresponds to the control's size when the group is in Large, Middle, or Small state.

### SimplifiedSizeDefinition

Defines how control size adapts to group box state in simplified mode.

| Aspect | Value |
|--------|-------|
| Type | `RibbonControlSizeDefinition` |
| Default | `"Large,Middle,Small"` |

```xml
<fluent:Button Header="Paste"
               SizeDefinition="Large,Large,Middle"
               SimplifiedSizeDefinition="Middle,Small,Small" />
```

### IsSimplified (Read-Only)

Indicates whether the control is currently in simplified mode.

| Aspect | Value |
|--------|-------|
| Type | `bool` |
| Default | `false` |
| Access | Read-only (set internally) |

---

## User Toggle Behavior

### Context Menu

When `CanUseSimplified="True"`, right-clicking the ribbon shows:

| Menu Item | Shown When | Action |
|-----------|------------|--------|
| "Use Classic Ribbon" | `IsSimplified=true` | Sets `IsSimplified=false` |
| "Use Simplified Ribbon" | `IsSimplified=false` | Sets `IsSimplified=true` |

### Display Options Button

The Display Options button (small icon near tab headers) shows a dropdown with:

```
+---------------------------+
| Show Ribbon               |  <- Header (disabled)
|---------------------------|
| [ ] Expand Ribbon         |
| [x] Minimize Ribbon       |
|---------------------------|
| Ribbon Layout             |  <- Header (disabled)
|---------------------------|
| [x] Use Classic Ribbon    |
| [ ] Use Simplified Ribbon |
+---------------------------+
```

### Keyboard Shortcut

| Shortcut | Action | Condition |
|----------|--------|-----------|
| `Ctrl+F1` | Toggle minimize | `CanMinimize=true` |
| `Ctrl+F2` | Toggle simplified | `CanUseSimplified=true` |

```csharp
// From Ribbon.cs OnKeyDown handler
case Key.F2:
{
    if (this.TabControl?.HasItems == true)
    {
        if (this.CanUseSimplified)
        {
            this.IsSimplified = !this.IsSimplified;
        }
    }
    break;
}
```

---

## How Controls Adapt

### Visual Changes in Simplified Mode

| Control | Classic Mode (Large) | Simplified Mode |
|---------|---------------------|-----------------|
| Button | Large icon + 2-line text | Medium icon + single-line text (or icon only for Small/Middle) |
| DropDownButton | Large icon + text + arrow | Medium icon + arrow (text hidden) |
| SplitButton | Large icon + text + split arrow | Medium icon + split arrow |
| ComboBox | Label + dropdown | Dropdown only (narrower) |
| TextBox | Label + input field | Input field only (narrower) |
| Spinner | Label + up/down buttons | Compact spinner |
| CheckBox | Check + text | Check + text (single line) |
| RibbonGroupBox | Vertical layout + header | Horizontal layout, no header |

### Template Switching

Controls use different templates in simplified mode:

```xml
<!-- From Button.xaml -->
<Style x:Key="Fluent.Ribbon.Styles.Button" TargetType="{x:Type Fluent:Button}">
    <Setter Property="Template" Value="{DynamicResource Fluent.Ribbon.Templates.Button}" />
    <Style.Triggers>
        <Trigger Property="IsSimplified" Value="True">
            <Setter Property="Template"
                    Value="{DynamicResource Fluent.Ribbon.Templates.Button.Simplified}" />
            <Setter Property="Fluent:RibbonProperties.IconSize" Value="Medium" />
            <Setter Property="Height" Value="Auto" />
            <Setter Property="MinHeight" Value="30" />
        </Trigger>
    </Style.Triggers>
</Style>
```

### Icon Size Mapping

| Classic Mode Size | Simplified Mode Size |
|-------------------|----------------------|
| Large | Medium |
| Middle | Small |
| Small | Small |

---

## Which Controls Support Simplified Mode

### Full Support (Implement ISimplifiedRibbonControl)

These controls have `IsSimplified` property and `SimplifiedSizeDefinition`:

| Control | Simplified Template | Notes |
|---------|---------------------|-------|
| `Button` | Yes | Horizontal layout, medium icons |
| `ToggleButton` | Yes | Same as Button |
| `RadioButton` | Yes | Same as Button |
| `CheckBox` | Yes | Compact layout |
| `DropDownButton` | Yes | Horizontal, icon-focused |
| `SplitButton` | Yes | Horizontal, icon-focused |
| `ComboBox` | Yes | Narrower, no label space |
| `TextBox` | Yes | Narrower layout |
| `Spinner` | Yes | Compact number input |
| `InRibbonGallery` | Yes | Different presentation |

### State Control Only (Implement ISimplifiedStateControl)

These controls respond to simplified state but don't have their own templates:

| Control | Notes |
|---------|-------|
| `RibbonGroupBox` | Changes layout orientation |
| `RibbonTabItem` | Propagates state to groups |
| `RibbonToolBar` | Layout adjustments |

### No Simplified Support

| Control | Notes |
|---------|-------|
| `Gallery` | Complex control, same in both modes |
| `Separator` | Simple control, adapts automatically |
| `GalleryPanel` | Internal use |

---

## Styling in Simplified Mode

### Default Style Keys

| Key | Description |
|-----|-------------|
| `Fluent.Ribbon.Templates.Button.Simplified` | Button template for simplified mode |
| `Fluent.Ribbon.Templates.ToggleButton.Simplified` | Toggle button template |
| `Fluent.Ribbon.Templates.DropDownButton.Simplified` | Dropdown button template |
| `Fluent.Ribbon.Templates.SplitButton.Simplified` | Split button template |
| `Fluent.Ribbon.Templates.ComboBox.Simplified` | ComboBox template |
| `Fluent.Ribbon.Templates.RibbonTextBox.Simplified` | TextBox template |
| `Fluent.Ribbon.Templates.Spinner.Simplified` | Spinner template |

### Custom Styling Example

```xml
<Style TargetType="fluent:Button" BasedOn="{StaticResource Fluent.Ribbon.Styles.Button}">
    <Style.Triggers>
        <Trigger Property="IsSimplified" Value="True">
            <!-- Custom simplified appearance -->
            <Setter Property="MinWidth" Value="40" />
            <Setter Property="Margin" Value="2 0" />
        </Trigger>
    </Style.Triggers>
</Style>
```

### Group Box Separator in Simplified Mode

```xml
<!-- From RibbonGroupBox.xaml -->
<Style x:Key="Fluent.Ribbon.Styles.GroupBoxSeparator" TargetType="Separator">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Separator">
                <Border x:Name="SeparatorBorder" Width="1" Height="55" ... />
                <ControlTemplate.Triggers>
                    <DataTrigger Binding="{Binding IsSimplified,
                                 RelativeSource={RelativeSource AncestorType={x:Type Fluent:RibbonGroupBox}}}"
                                 Value="True">
                        <Setter TargetName="SeparatorBorder" Property="Height" Value="Auto" />
                    </DataTrigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

---

## Interface Reference

### ISimplifiedStateControl

Base interface for controls that need to know about simplified state.

```csharp
public interface ISimplifiedStateControl
{
    /// <summary>
    /// Update simplified state.
    /// </summary>
    void UpdateSimplifiedState(bool isSimplified);
}
```

Implemented by: `RibbonTabItem`, `RibbonGroupBox`, `RibbonToolBar`, and all ribbon controls.

### ISimplifiedRibbonControl

Extended interface for controls with full simplified mode support.

```csharp
public interface ISimplifiedRibbonControl : ISimplifiedStateControl
{
    /// <summary>
    /// Gets or sets SimplifiedSizeDefinition for element in Simplified mode
    /// </summary>
    RibbonControlSizeDefinition SimplifiedSizeDefinition { get; set; }

    /// <summary>
    /// Gets whether the ribbon is in Simplified mode
    /// </summary>
    bool IsSimplified { get; }
}
```

Implemented by: `Button`, `ToggleButton`, `DropDownButton`, `SplitButton`, `ComboBox`, `TextBox`, `Spinner`, `CheckBox`, `RadioButton`, `InRibbonGallery`.

---

## Localization Keys

| Property | Default (English) |
|----------|-------------------|
| `UseClassicRibbon` | "Use Classic Ribbon" |
| `UseSimplifiedRibbon` | "Use Simplified Ribbon" |
| `RibbonLayout` | "Ribbon Layout" |

### Customizing Localization

```csharp
RibbonLocalization.Current.Localization.UseClassicRibbon = "Classic View";
RibbonLocalization.Current.Localization.UseSimplifiedRibbon = "Compact View";
```

---

## Common Tasks

### Enable Simplified Mode

```xml
<fluent:Ribbon CanUseSimplified="True">
    <!-- Ribbon content -->
</fluent:Ribbon>
```

### Start in Simplified Mode

```xml
<fluent:Ribbon CanUseSimplified="True" IsSimplified="True">
    <!-- Ribbon content -->
</fluent:Ribbon>
```

### Toggle Simplified Mode Programmatically

```csharp
ribbon.IsSimplified = !ribbon.IsSimplified;
```

### Bind Simplified State to ViewModel

```xml
<fluent:Ribbon CanUseSimplified="True"
               IsSimplified="{Binding Settings.UseCompactRibbon, Mode=TwoWay}">
```

### Control-Specific Size in Simplified Mode

```xml
<fluent:Button Header="Important Action"
               SizeDefinition="Large,Large,Large"
               SimplifiedSizeDefinition="Large,Large,Middle">
    <!-- This button stays Large in simplified mode (showing text)
         while other buttons might be Medium (icon only) -->
</fluent:Button>
```

### Always Show Text in Simplified Mode

```xml
<fluent:Button Header="Save"
               SimplifiedSizeDefinition="Large,Large,Large">
    <!-- Large size shows text even in simplified mode -->
</fluent:Button>
```

### Respond to Simplified Mode Changes

```csharp
// In code-behind or custom control
ribbon.PropertyChanged += (s, e) =>
{
    if (e.Property == Ribbon.IsSimplifiedProperty)
    {
        var isSimplified = ribbon.IsSimplified;
        // React to mode change
    }
};
```

### Custom Group Box State Sequence

```xml
<fluent:RibbonGroupBox Header="Clipboard"
                       StateDefinition="Large,Middle,Small,Collapsed"
                       SimplifiedStateDefinition="Large,Collapsed">
    <!-- In simplified mode, goes directly from Large to Collapsed
         (skips Middle state entirely) -->
</fluent:RibbonGroupBox>
```

---

## State Diagram

```
                    CanUseSimplified = true
                            |
                            v
    +------------------+         +-------------------+
    |  CLASSIC MODE    | <-----> | SIMPLIFIED MODE   |
    | (IsSimplified=   |         | (IsSimplified=    |
    |     false)       |         |     true)         |
    +------------------+         +-------------------+
            |                            |
            | Toggle via:                | Toggle via:
            | - Ctrl+F2                  | - Ctrl+F2
            | - Context menu             | - Context menu
            | - Display Options          | - Display Options
            | - Programmatic             | - Programmatic
            v                            v
    +------------------+         +-------------------+
    | Controls use:    |         | Controls use:     |
    | - SizeDefinition |         | - Simplified-     |
    | - Large icons    |         |   SizeDefinition  |
    | - Full headers   |         | - Medium icons    |
    | - Vertical       |         | - Compact headers |
    |   groups         |         | - Horizontal      |
    |                  |         |   groups          |
    +------------------+         +-------------------+


    GROUP BOX STATE TRANSITIONS:

    Classic Mode (StateDefinition default: all states)
    Large -> Middle -> Small -> Collapsed
      ^                            |
      |____________________________| (when width increases)

    Simplified Mode (SimplifiedStateDefinition default: "Large,Middle,Collapsed")
    Large -> Middle -> Collapsed
      ^                    |
      |____________________| (when width increases)
```

---

## DO NOT DO

### DON'T: Forget CanUseSimplified

```xml
<!-- WRONG - IsSimplified has no effect, no UI to toggle -->
<fluent:Ribbon IsSimplified="True">

<!-- RIGHT - Enable the feature first -->
<fluent:Ribbon CanUseSimplified="True" IsSimplified="True">
```

### DON'T: Set IsSimplified on Child Controls

```csharp
// WRONG - IsSimplified is read-only on controls
button.IsSimplified = true;  // Compile error

// RIGHT - Set on Ribbon, it propagates automatically
ribbon.IsSimplified = true;
```

### DON'T: Assume All Controls Have SimplifiedSizeDefinition

```xml
<!-- WRONG - RibbonGroupBox doesn't have SimplifiedSizeDefinition -->
<fluent:RibbonGroupBox SimplifiedSizeDefinition="Large,Middle">

<!-- RIGHT - RibbonGroupBox uses SimplifiedStateDefinition -->
<fluent:RibbonGroupBox SimplifiedStateDefinition="Large,Middle,Collapsed">
```

### DON'T: Use Custom Templates Without Simplified Version

```xml
<!-- WRONG - Custom template ignores simplified mode -->
<fluent:Button Template="{StaticResource MyCustomButtonTemplate}" />

<!-- RIGHT - Provide both templates -->
<Style TargetType="fluent:Button" BasedOn="{StaticResource Fluent.Ribbon.Styles.Button}">
    <Setter Property="Template" Value="{StaticResource MyCustomButtonTemplate}" />
    <Style.Triggers>
        <Trigger Property="IsSimplified" Value="True">
            <Setter Property="Template"
                    Value="{StaticResource MyCustomButtonTemplate.Simplified}" />
        </Trigger>
    </Style.Triggers>
</Style>
```

### DON'T: Hardcode Heights in Simplified Mode

```xml
<!-- WRONG - Fixed height breaks simplified mode layout -->
<fluent:Button Header="Action" Height="68" />

<!-- RIGHT - Let style handle heights -->
<fluent:Button Header="Action" />
<!-- Or use MinHeight for constraints -->
<fluent:Button Header="Action" MinHeight="30" />
```

### DON'T: Rely on Two-Line Labels in Simplified Mode

```csharp
// WRONG - Assuming header will wrap
var button = new Button { Header = "Very Long Button Text Here" };

// RIGHT - Keep headers concise or use tooltips for full text
var button = new Button
{
    Header = "Long Text",
    ToolTip = "Very Long Button Text Here with Full Description"
};
```

---

## Source Files

| File | Purpose |
|------|---------|
| `Fluent.Ribbon\Controls\Ribbon.cs` | `CanUseSimplified`, `IsSimplified` properties |
| `Fluent.Ribbon\Controls\RibbonGroupBox.cs` | `SimplifiedStateDefinition`, `IsSimplified` |
| `Fluent.Ribbon\Controls\Button.cs` | `SimplifiedSizeDefinition`, `IsSimplified` |
| `Fluent.Ribbon\ISimplifiedStateControl.cs` | Base interface |
| `Fluent.Ribbon\ISimplifiedRibbonControl.cs` | Extended interface |
| `Fluent.Ribbon\AttachedProperties\RibbonProperties.cs` | `SimplifiedSizeDefinitionProperty` |
| `Fluent.Ribbon\Themes\Controls\Button.xaml` | Button simplified template |
| `Fluent.Ribbon\Themes\Controls\RibbonGroupBox.xaml` | GroupBox simplified triggers |
| `Fluent.Ribbon\Themes\Controls\RibbonTabControl.xaml` | Display options menu |

---

## Summary

| Task | Solution |
|------|----------|
| Enable simplified mode feature | `CanUseSimplified="True"` |
| Start in simplified mode | `CanUseSimplified="True" IsSimplified="True"` |
| Toggle programmatically | `ribbon.IsSimplified = !ribbon.IsSimplified` |
| Set control size for simplified | `SimplifiedSizeDefinition="Large,Middle,Small"` |
| Set group state sequence | `SimplifiedStateDefinition="Large,Middle,Collapsed"` |
| Check if simplified | `if (ribbon.IsSimplified) { ... }` |
| Keyboard toggle | `Ctrl+F2` (when enabled) |
| Keep button text visible | `SimplifiedSizeDefinition="Large,Large,Large"` |
| Customize localization | `RibbonLocalization.Current.Localization.UseSimplifiedRibbon = "..."` |
| Custom styling | Use `IsSimplified` trigger in styles |
