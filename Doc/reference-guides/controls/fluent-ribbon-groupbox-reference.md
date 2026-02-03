---
title: Fluent.Ribbon RibbonGroupBox Reference
description: Deep dive into RibbonGroupBox - the primary container for organizing controls within a ribbon tab
tags: [groupbox, container, controls, collapse, dialog-launcher]
see_also:
  - fluent-ribbon-contextual-tabs-reference.md
  - ../advanced/fluent-ribbon-state-diagrams-v2.md
  - ../advanced/fluent-ribbon-attached-properties-reference.md
---

# Fluent.Ribbon RibbonGroupBox Reference

**Version:** Based on Fluent.Ribbon source analysis
**Last Updated:** 2026-01-23
**Source:** [Fluent.Ribbon/Controls/RibbonGroupBox.cs](https://github.com/fluentribbon/Fluent.Ribbon/blob/develop/Fluent.Ribbon/Controls/RibbonGroupBox.cs)

---

## Overview

`RibbonGroupBox` is the primary container for organizing controls within a ribbon tab. It provides automatic resizing, collapse behavior, dialog launcher buttons, and popup functionality when collapsed.

---

## Class Definition

```csharp
public class RibbonGroupBox : HeaderedItemsControl,
    IQuickAccessItemProvider,
    IDropDownControl,
    IKeyTipedControl,
    IHeaderedControl,
    ILogicalChildSupport,
    IMediumIconProvider,
    ISimplifiedStateControl,
    ILargeIconProvider
```

**Namespace:** `Fluent`

---

## Template Parts

| Part Name | Type | Description |
|-----------|------|-------------|
| `PART_DialogLauncherButton` | `Button` | The dialog launcher button |
| `PART_HeaderContentControl` | `ContentControl` | Header display (normal state) |
| `PART_CollapsedHeaderContentControl` | `ContentControl` | Header display (collapsed state) |
| `PART_Popup` | `Popup` | Popup for collapsed state |
| `PART_UpPanel` | `Panel` | Panel containing items |
| `PART_ParentPanel` | `Panel` | Root layout panel |
| `PART_SnappedImage` | `Image` | Snapshot image for QAT |

---

## Key Properties

### State and Size

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `State` | `RibbonGroupBoxState` | `Large` | Current display state |
| `StateDefinition` | `RibbonGroupBoxStateDefinition` | See below | States for normal mode |
| `SimplifiedStateDefinition` | `RibbonGroupBoxStateDefinition` | `Large,Middle,Collapsed` | States for simplified mode |
| `IsSimplified` | `bool` | `false` | Whether in simplified ribbon mode |
| `IsDropDownOpen` | `bool` | `false` | Whether popup is open (collapsed state) |

### RibbonGroupBoxState Enum

| Value | Description |
|-------|-------------|
| `Large` | Full size, all controls visible |
| `Middle` | Reduced size |
| `Small` | Compact size |
| `Collapsed` | Shows only header, popup on click |
| `QuickAccess` | In Quick Access Toolbar |

### Default StateDefinition

The default `StateDefinition` allows all states. Customize to control collapse behavior:

```xml
<!-- Only collapse to Middle, then Collapsed (skip Small) -->
<Fluent:RibbonGroupBox StateDefinition="Large,Middle,Collapsed" />

<!-- Never collapse (stays Large) -->
<Fluent:RibbonGroupBox StateDefinition="Large" />

<!-- Start at Middle, collapse to Small -->
<Fluent:RibbonGroupBox StateDefinition="Middle,Small,Collapsed" />
```

---

### Dialog Launcher

| Property | Type | Description |
|----------|------|-------------|
| `IsLauncherVisible` | `bool` | Show/hide launcher button |
| `IsLauncherEnabled` | `bool` | Enable/disable launcher |
| `LauncherCommand` | `ICommand` | Command to execute |
| `LauncherCommandParameter` | `object` | Command parameter |
| `LauncherCommandTarget` | `IInputElement` | Command target |
| `LauncherIcon` | `object` | Launcher button icon |
| `LauncherText` | `string` | Launcher button text |
| `LauncherToolTip` | `object` | Launcher tooltip |
| `LauncherKeys` | `string` | KeyTip for launcher |
| `LauncherButton` | `Button` | Reference to launcher button (read-only) |

### Event

| Event | Description |
|-------|-------------|
| `LauncherClick` | Fired when launcher button clicked |

---

### Icons

| Property | Type | Description |
|----------|------|-------------|
| `Icon` | `object` | Small icon (16x16) |
| `MediumIcon` | `object` | Medium icon (24x24) |
| `LargeIcon` | `object` | Large icon (32x32) |

---

### Visual

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IsSeparatorVisible` | `bool` | `true` | Show separator between groups |
| `KeyTip` | `string` | `null` | KeyTip when collapsed |

---

## Events

| Event | Type | Description |
|-------|------|-------------|
| `LauncherClick` | `RoutedEventHandler` | Dialog launcher clicked |
| `DropDownOpened` | `EventHandler` | Popup opened (collapsed state) |
| `DropDownClosed` | `EventHandler` | Popup closed |

---

## Basic Usage

### Simple GroupBox

```xml
<Fluent:RibbonGroupBox Header="Clipboard">
    <Fluent:Button Header="Paste" LargeIcon="paste32.png" />
    <Fluent:Button Header="Cut" Icon="cut16.png" />
    <Fluent:Button Header="Copy" Icon="copy16.png" />
</Fluent:RibbonGroupBox>
```

### With Dialog Launcher

```xml
<Fluent:RibbonGroupBox Header="Font"
                       IsLauncherVisible="True"
                       LauncherCommand="{Binding ShowFontDialogCommand}"
                       LauncherToolTip="Font Dialog (Ctrl+D)"
                       LauncherKeys="FN">
    <!-- Font controls -->
</Fluent:RibbonGroupBox>
```

### Custom State Definition

```xml
<!-- Group that collapses early -->
<Fluent:RibbonGroupBox Header="Optional"
                       StateDefinition="Middle,Small,Collapsed">
    <!-- Controls -->
</Fluent:RibbonGroupBox>

<!-- Group that never collapses past Middle -->
<Fluent:RibbonGroupBox Header="Essential"
                       StateDefinition="Large,Middle">
    <!-- Controls -->
</Fluent:RibbonGroupBox>
```

### With Icons

```xml
<Fluent:RibbonGroupBox Header="Format"
                       Icon="format16.png"
                       LargeIcon="format32.png">
    <!-- Controls -->
</Fluent:RibbonGroupBox>
```

---

## Collapse Behavior

### How Collapse Works

1. Ribbon width decreases
2. `RibbonGroupsContainer` requests groups to shrink
3. Groups transition through states in `StateDefinition` order
4. Child controls resize based on their `SizeDefinition`
5. At `Collapsed` state, group shows only header button with popup

### Scale Property (Internal)

The `Scale` property controls internal scalable item resizing:

```csharp
// Internal use - incrementally scales IScalableRibbonControl items
internal int Scale { get; set; }
```

### IsInButtonState

```csharp
// True when State is Collapsed or QuickAccess
public bool IsInButtonState =>
    this.State == RibbonGroupBoxState.Collapsed ||
    this.State == RibbonGroupBoxState.QuickAccess;
```

When `IsInButtonState` is true:
- Group is focusable
- Clicking opens dropdown popup
- Space key opens popup
- Escape key closes popup

---

## Attached Property

### IsCollapsedHeaderContentPresenter

Identifies content presenters for collapsed header rendering:

```csharp
public static readonly DependencyProperty IsCollapsedHeaderContentPresenterProperty;

// Set on ContentControl in template
RibbonGroupBox.SetIsCollapsedHeaderContentPresenter(element, true);
```

---

## Snapping

When used in Quick Access Toolbar, the group "snaps" - creates a bitmap image of itself:

```csharp
// True when visual is frozen as image
public bool IsSnapped { get; set; }
```

---

## Quick Access Toolbar Support

### Creating QAT Item

```csharp
public virtual FrameworkElement CreateQuickAccessItem()
{
    var groupBox = new RibbonGroupBox();
    RibbonControl.BindQuickAccessItem(this, groupBox);

    groupBox.DropDownOpened += OnQuickAccessOpened;
    groupBox.DropDownClosed += OnQuickAccessClosed;
    groupBox.State = RibbonGroupBoxState.QuickAccess;

    // Bind properties...
    return groupBox;
}
```

### CanAddToQuickAccessToolBar

```xml
<Fluent:RibbonGroupBox Header="Clipboard"
                       CanAddToQuickAccessToolBar="True">
    <!-- Controls -->
</Fluent:RibbonGroupBox>
```

---

## MVVM Patterns

### ViewModel

```csharp
public class RibbonViewModel
{
    public ICommand ShowFontDialogCommand { get; }
    public ICommand ShowParagraphDialogCommand { get; }

    public RibbonViewModel()
    {
        ShowFontDialogCommand = new RelayCommand(ExecuteShowFontDialog);
        ShowParagraphDialogCommand = new RelayCommand(ExecuteShowParagraphDialog);
    }

    private void ExecuteShowFontDialog()
    {
        // Show font dialog
    }
}
```

### XAML

```xml
<Fluent:RibbonGroupBox Header="Font"
                       IsLauncherVisible="True"
                       LauncherCommand="{Binding ShowFontDialogCommand}">
    <Fluent:Button Header="Bold" Command="{Binding BoldCommand}" />
    <Fluent:Button Header="Italic" Command="{Binding ItalicCommand}" />
</Fluent:RibbonGroupBox>
```

### Dynamic Items

```xml
<Fluent:RibbonGroupBox Header="Recent"
                       ItemsSource="{Binding RecentItems}"
                       ItemTemplate="{StaticResource RecentItemTemplate}" />
```

---

## Simplified Mode

When `IsSimplified` is true, the group uses `SimplifiedStateDefinition`:

```xml
<Fluent:RibbonGroupBox Header="Format"
                       SimplifiedStateDefinition="Large,Middle,Collapsed">
    <!-- In simplified mode, uses this definition -->
</Fluent:RibbonGroupBox>
```

The ribbon sets `IsSimplified` on groups when the user toggles simplified mode.

---

## Keyboard Navigation

| Key | Action (when IsInButtonState) |
|-----|-------------------------------|
| Space | Open dropdown |
| Alt+Down | Open dropdown |
| Escape | Close dropdown |
| KeyTip | Open dropdown (if collapsed) |

---

## Methods

### Internal Methods

| Method | Description |
|--------|-------------|
| `GetPanel()` | Returns the `PART_UpPanel` |
| `GetLayoutRoot()` | Returns the `PART_ParentPanel` |
| `InvalidateLayout()` | Invalidates measure recursively |
| `TryClearCacheAndResetStateAndScale()` | Resets state and scale |
| `TryClearCacheAndResetStateAndScaleAndNotifyParentRibbonGroupsContainer()` | Resets and notifies parent |
| `GetDesiredSizeIntermediate()` | Gets size for intermediate state |

---

## Styling

### Default Template Structure

```xml
<ControlTemplate TargetType="Fluent:RibbonGroupBox">
    <Grid x:Name="PART_ParentPanel">
        <!-- Normal state content -->
        <ContentControl x:Name="PART_HeaderContentControl"
                        Content="{TemplateBinding Header}" />
        <Panel x:Name="PART_UpPanel">
            <ItemsPresenter />
        </Panel>
        <Button x:Name="PART_DialogLauncherButton" />

        <!-- Collapsed state -->
        <ContentControl x:Name="PART_CollapsedHeaderContentControl"
                        Fluent:RibbonGroupBox.IsCollapsedHeaderContentPresenter="True" />
        <Popup x:Name="PART_Popup" />
        <Image x:Name="PART_SnappedImage" />
    </Grid>
</ControlTemplate>
```

### Custom Header Style

```xml
<Fluent:RibbonGroupBox Header="Custom">
    <Fluent:RibbonGroupBox.HeaderTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <Image Source="icon.png" Width="16" Height="16" />
                <TextBlock Text="{Binding}" Margin="4,0,0,0" />
            </StackPanel>
        </DataTemplate>
    </Fluent:RibbonGroupBox.HeaderTemplate>
</Fluent:RibbonGroupBox>
```

---

## DO NOT DO

### Don't Access Template Parts Directly
```csharp
// WRONG
var panel = groupBox.GetTemplateChild("PART_UpPanel") as Panel;
panel.Children.Add(newButton);

// RIGHT - Use Items collection
groupBox.Items.Add(newButton);
```

### Don't Set State Directly (Usually)
```csharp
// WRONG - State is managed by layout system
groupBox.State = RibbonGroupBoxState.Small;

// RIGHT - Use StateDefinition to influence behavior
groupBox.StateDefinition = new RibbonGroupBoxStateDefinition("Large,Small,Collapsed");
```

### Don't Ignore Launcher for Dialogs
```xml
<!-- WRONG - Missing launcher for dialog access -->
<Fluent:RibbonGroupBox Header="Font">
    <Fluent:Button Header="Font..." Command="{Binding ShowFontDialog}" />
</Fluent:RibbonGroupBox>

<!-- RIGHT - Use launcher for dialog access (Office convention) -->
<Fluent:RibbonGroupBox Header="Font"
                       IsLauncherVisible="True"
                       LauncherCommand="{Binding ShowFontDialog}">
    <!-- Font controls here -->
</Fluent:RibbonGroupBox>
```

---

## Quick Reference

### Key Properties

| Property | Type | Purpose |
|----------|------|---------|
| `Header` | `object` | Group label |
| `State` | `RibbonGroupBoxState` | Current state |
| `StateDefinition` | `RibbonGroupBoxStateDefinition` | Collapse behavior |
| `IsLauncherVisible` | `bool` | Show dialog launcher |
| `LauncherCommand` | `ICommand` | Launcher action |
| `IsDropDownOpen` | `bool` | Popup state (collapsed) |
| `KeyTip` | `string` | Keyboard shortcut |

### RibbonGroupBoxState Values

| Value | Description |
|-------|-------------|
| `Large` | Full size |
| `Middle` | Medium size |
| `Small` | Compact size |
| `Collapsed` | Header only, popup on click |
| `QuickAccess` | In QAT |

### StateDefinition Patterns

| Pattern | Behavior |
|---------|----------|
| `Large,Middle,Small,Collapsed` | Full progression |
| `Large,Middle,Collapsed` | Skip small |
| `Large,Collapsed` | Large or collapsed only |
| `Large` | Never collapse |
| `Middle,Small,Collapsed` | Start at middle |

---

*Reference verified against Fluent.Ribbon source code as of 2026-01-23.*
