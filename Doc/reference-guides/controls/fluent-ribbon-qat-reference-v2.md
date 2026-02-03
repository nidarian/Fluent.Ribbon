---
title: Fluent.Ribbon Quick Access Toolbar Reference
description: Complete reference for the Quick Access Toolbar control and related components
tags: [qat, quickaccesstoolbar, controls, customization]
see_also:
  - fluent-ribbon-titlebar-reference.md
  - ../ux/fluent-ribbon-state-persistence-reference.md
---

# Fluent.Ribbon Quick Access Toolbar (QAT) Reference (v2)

**Complete reference for the Quick Access Toolbar control and related components.**

*v2 Changes: Clarified KeyTip assignment table, added Refresh() method, added ItemsChanged event details*

---

## Overview

The Quick Access Toolbar (QAT) is a customizable toolbar that provides quick access to frequently used commands. It can be positioned above or below the ribbon.

```
+---------------------------------------------------------------------+
| [Icon] [Save] [Undo] [Redo] [v]  Title                    [-][O][x] |  <- QAT Above Ribbon
+---------------------------------------------------------------------+
| File | Home | View | Tools |                                        |
+---------------------------------------------------------------------+
| [Buttons and controls in selected tab]                              |
+---------------------------------------------------------------------+
| [Save] [Undo] [Redo] [v]                                            |  <- QAT Below Ribbon
+---------------------------------------------------------------------+
```

---

## Component Hierarchy

```
fluent:Ribbon
+-- fluent:Ribbon.QuickAccessToolBar
    +-- fluent:QuickAccessToolBar
        +-- Items (ObservableCollection<UIElement>)
        |   +-- Buttons, DropDownButtons, etc. (actual toolbar items)
        |
        +-- QuickAccessItems (ItemCollection<QuickAccessMenuItem>)
        |   +-- QuickAccessMenuItem (menu items for customize dropdown)
        |       +-- Target -> IRibbonControl
        |
        +-- PART_ToolBarPanel (visible items)
        +-- PART_ToolBarOverflowPanel (overflow items)
        +-- PART_MenuDownButton (customize dropdown)
        +-- PART_ToolbarDownButton (overflow dropdown)
```

---

## fluent:QuickAccessToolBar

The main QAT control.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `ObservableCollection<UIElement>` | empty | Toolbar items (buttons, etc.) |
| `QuickAccessItems` | `ItemCollection<QuickAccessMenuItem>` | empty | Menu items in customize dropdown |
| `ShowAboveRibbon` | `bool` | `true` | Position above (true) or below (false) ribbon |
| `CanQuickAccessLocationChanging` | `bool` | `true` | Allow user to move QAT position |
| `IsMenuDropDownVisible` | `bool` | `true` | Show/hide the customize dropdown button |
| `HasOverflowItems` | `bool` | (read-only) | True when items overflow to dropdown |
| `UpdateKeyTipsAction` | `Action<QuickAccessToolBar>` | null | Custom KeyTip generation logic |

### Events

| Event | Description |
|-------|-------------|
| `ItemsChanged` | Fires when items are added/removed from QAT (NotifyCollectionChangedEventArgs) |

### Methods

| Method | Description |
|--------|-------------|
| `Refresh()` | Forces re-measurement and layout of QAT items. Call after programmatic changes. |

### Example: Basic QAT Setup

```xml
<fluent:Ribbon>
    <fluent:Ribbon.QuickAccessToolBar>
        <fluent:QuickAccessToolBar ShowAboveRibbon="True">
            <!-- Toolbar items -->
            <fluent:Button Header="Save" Icon="{StaticResource SaveIcon}"
                           Command="{Binding SaveCommand}" />
            <fluent:Button Header="Undo" Icon="{StaticResource UndoIcon}"
                           Command="{Binding UndoCommand}" />
            <fluent:Button Header="Redo" Icon="{StaticResource RedoIcon}"
                           Command="{Binding RedoCommand}" />

            <!-- Menu items for customize dropdown -->
            <fluent:QuickAccessToolBar.QuickAccessItems>
                <fluent:QuickAccessMenuItem Target="{Binding ElementName=PrintButton}" />
                <fluent:QuickAccessMenuItem Target="{Binding ElementName=ExportButton}" />
            </fluent:QuickAccessToolBar.QuickAccessItems>
        </fluent:QuickAccessToolBar>
    </fluent:Ribbon.QuickAccessToolBar>

    <fluent:RibbonTabItem Header="Home">
        <fluent:RibbonGroupBox Header="File">
            <fluent:Button x:Name="PrintButton" Header="Print" />
            <fluent:Button x:Name="ExportButton" Header="Export" />
        </fluent:RibbonGroupBox>
    </fluent:RibbonTabItem>
</fluent:Ribbon>
```

---

## fluent:QuickAccessMenuItem

A menu item in the QAT customize dropdown that links to a ribbon control.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Target` | `UIElement` | The ribbon control to add/remove from QAT |
| `Header` | `object` | Menu item text (auto-synced from Target if not set) |
| `IsChecked` | `bool` | Whether the target is currently in QAT |
| `IsCheckable` | `bool` | Always `true` (default) |

### Behavior

- When **checked**: Target control is added to QAT
- When **unchecked**: Target control is removed from QAT
- Header automatically binds to target's `Header` property if not explicitly set

### Example: Customize Menu Items

```xml
<fluent:QuickAccessToolBar.QuickAccessItems>
    <!-- Auto-gets header from target -->
    <fluent:QuickAccessMenuItem Target="{Binding ElementName=PasteButton}" />

    <!-- Custom header -->
    <fluent:QuickAccessMenuItem Header="Quick Print"
                                 Target="{Binding ElementName=PrintButton}" />
</fluent:QuickAccessToolBar.QuickAccessItems>
```

---

## IQuickAccessItemProvider Interface

Controls implement this interface to be QAT-compatible.

```csharp
public interface IQuickAccessItemProvider
{
    /// <summary>
    /// Creates a clone/shortcut of this control for QAT.
    /// Must be synchronized with original and route commands back.
    /// </summary>
    FrameworkElement CreateQuickAccessItem();

    /// <summary>
    /// Gets or sets whether this control can be added to QAT.
    /// </summary>
    bool CanAddToQuickAccessToolBar { get; set; }
}
```

### Built-in Controls That Implement This

All standard Fluent.Ribbon controls implement `IQuickAccessItemProvider`:
- `Button`
- `DropDownButton`
- `SplitButton`
- `ToggleButton`
- `ComboBox`
- `TextBox`
- `Spinner`
- `CheckBox`
- `Gallery`
- `InRibbonGallery`

### Prevent Control from Being Added to QAT

```xml
<fluent:Button Header="Exit" CanAddToQuickAccessToolBar="False" />
```

---

## KeyTip Behavior

QAT items get automatic KeyTips assigned sequentially.

### Default KeyTip Assignment

| Item Position | KeyTip | Notes |
|---------------|--------|-------|
| 1st item | `1` | Single digit |
| 2nd item | `2` | Single digit |
| ... | ... | ... |
| 9th item | `9` | Single digit |
| 10th item | `09` | Two digits, zero prefix |
| 11th item | `08` | Two digits, descending |
| ... | ... | ... |
| 18th item | `01` | Two digits |
| 19th item | `0A` | Zero + letter |
| 20th item | `0B` | Zero + letter |
| ... | ... | ... |
| 44th item | `0Z` | Zero + letter (max) |

**Summary:** Items 1-9 get `1-9`, items 10-18 get `09-01` (reversed), items 19-44 get `0A-0Z`.

### Custom KeyTip Generation

```csharp
// Override default KeyTip assignment
quickAccessToolBar.UpdateKeyTipsAction = (qat) =>
{
    for (int i = 0; i < qat.Items.Count; i++)
    {
        KeyTip.SetKeys(qat.Items[i], $"Q{i + 1}");
    }
};
```

---

## State Diagram

```
                   +------------------+
                   |  ABOVE RIBBON    |
                   |  (In title bar)  |
                   +--------+---------+
                            |
              User clicks   |   User clicks
              "Show Below"  |   "Show Above"
                            v
                   +------------------+
                   |  BELOW RIBBON    |
                   |  (Under tabs)    |
                   +------------------+

Property: ShowAboveRibbon = true/false


        +------------------+                    +------------------+
        |   ALL VISIBLE    |------------------>|    OVERFLOW      |
        |                  |  Width too narrow |                  |
        | [1][2][3][4][v]  |                   | [1][2][v][3][4]  |
        +------------------+<------------------+------------------+
                            Width sufficient

Property: HasOverflowItems = true/false (read-only)
```

---

## Common Tasks

### Add Item to QAT Programmatically

```csharp
// Add a button
var button = new Fluent.Button
{
    Header = "Quick Save",
    Icon = FindResource("SaveIcon"),
    Command = SaveCommand
};
ribbon.QuickAccessToolBar.Items.Add(button);

// Force layout update after adding
ribbon.QuickAccessToolBar.Refresh();
```

### Remove Item from QAT

```csharp
ribbon.QuickAccessToolBar.Items.Remove(button);

// Or remove by index
ribbon.QuickAccessToolBar.Items.RemoveAt(0);
```

### Clear All QAT Items

```csharp
ribbon.QuickAccessToolBar.Items.Clear();
```

### Move QAT Position Programmatically

```csharp
// Move below ribbon
ribbon.QuickAccessToolBar.ShowAboveRibbon = false;

// Move above ribbon
ribbon.QuickAccessToolBar.ShowAboveRibbon = true;
```

### Disable User Position Changing

```xml
<fluent:QuickAccessToolBar CanQuickAccessLocationChanging="False" />
```

This hides "Show Above/Below Ribbon" menu items.

### Hide Customize Dropdown

```xml
<fluent:QuickAccessToolBar IsMenuDropDownVisible="False" />
```

### Check if Control is in QAT

```csharp
bool isInQat = ribbon.IsInQuickAccessToolBar(myButton);
```

### Add/Remove via Ribbon Methods

```csharp
// Add control to QAT (creates clone automatically)
ribbon.AddToQuickAccessToolBar(myButton);

// Remove control from QAT
ribbon.RemoveFromQuickAccessToolBar(myButton);
```

### Listen for QAT Changes

```csharp
ribbon.QuickAccessToolBar.ItemsChanged += (sender, e) =>
{
    switch (e.Action)
    {
        case NotifyCollectionChangedAction.Add:
            Console.WriteLine($"Added: {e.NewItems?.Count} items");
            foreach (var item in e.NewItems)
            {
                Console.WriteLine($"  - {(item as FrameworkElement)?.Name}");
            }
            break;
        case NotifyCollectionChangedAction.Remove:
            Console.WriteLine($"Removed: {e.OldItems?.Count} items");
            break;
        case NotifyCollectionChangedAction.Reset:
            Console.WriteLine("QAT cleared");
            break;
    }
};
```

### Persist QAT State

```csharp
// Save QAT items (simplified - you'd serialize more data)
public List<string> GetQatItemIds()
{
    return ribbon.QuickAccessToolBar.Items
        .OfType<FrameworkElement>()
        .Select(e => e.Name)
        .Where(n => !string.IsNullOrEmpty(n))
        .ToList();
}

// Restore QAT items
public void RestoreQatItems(List<string> itemIds)
{
    foreach (var id in itemIds)
    {
        var control = FindName(id) as UIElement;
        if (control != null)
        {
            ribbon.AddToQuickAccessToolBar(control);
        }
    }
}
```

### Force Refresh After Programmatic Changes

```csharp
// If UI doesn't update after adding/removing items:
ribbon.QuickAccessToolBar.Refresh();
```

---

## Styles and Templates

### Style Keys

| Key | Description |
|-----|-------------|
| `Fluent.Ribbon.Styles.QuickAccessToolbar` | Main QAT style |
| `Fluent.Ribbon.Styles.ToolbarDropDownButton` | Dropdown button in QAT |

### Template Parts

| Part Name | Type | Description |
|-----------|------|-------------|
| `PART_RootPanel` | `Panel` | Root container |
| `PART_ToolBarPanel` | `Panel` | Holds visible items |
| `PART_ToolBarOverflowPanel` | `Panel` | Holds overflow items |
| `PART_MenuDownButton` | `DropDownButton` | Customize menu dropdown |
| `PART_ToolbarDownButton` | `DropDownButton` | Overflow items dropdown |
| `PART_ShowAbove` | `MenuItem` | "Show Above Ribbon" menu item |
| `PART_ShowBelow` | `MenuItem` | "Show Below Ribbon" menu item |
| `PART_MenuPanel` | `Panel` | Panel for QuickAccessMenuItems |

### Image Resources

| Key | Description |
|-----|-------------|
| `Fluent.Ribbon.Images.QuickAccessToolbarDropDown` | Customize dropdown icon (above) |
| `Fluent.Ribbon.Images.QuickAccessToolbarDropDown.BelowRibbon` | Customize dropdown icon (below) |
| `Fluent.Ribbon.Images.QuickAccessToolbarExtender` | Overflow dropdown icon (above) |
| `Fluent.Ribbon.Images.QuickAccessToolbarExtender.BelowRibbon` | Overflow dropdown icon (below) |

---

## Localization Keys

The QAT uses these localization strings:

| Property | Default (English) |
|----------|-------------------|
| `QuickAccessToolBarDropDownButtonTooltip` | "Customize Quick Access Toolbar" |
| `QuickAccessToolBarMoreControlsButtonTooltip` | "More controls" |
| `QuickAccessToolBarMenuHeader` | "Customize Quick Access Toolbar" |
| `QuickAccessToolBarMenuShowBelow` | "Show Below the Ribbon" |
| `QuickAccessToolBarMenuShowAbove` | "Show Above the Ribbon" |

### Override Localization

```csharp
// In App.xaml.cs or startup
RibbonLocalization.Current.Localization.QuickAccessToolBarMenuHeader = "My Custom Header";
```

---

## Ribbon Integration

### Ribbon Properties Related to QAT

| Property | Type | Description |
|----------|------|-------------|
| `ShowQuickAccessToolBarAboveRibbon` | `bool` | Syncs with QAT's `ShowAboveRibbon` |
| `IsQuickAccessToolBarVisible` | `bool` | Show/hide entire QAT |
| `QuickAccessToolBar` | `QuickAccessToolBar` | The QAT instance |

### Example: Toggle QAT Visibility

```xml
<fluent:Ribbon IsQuickAccessToolBarVisible="{Binding ShowQat}" />
```

### Example: Bind QAT Position

```xml
<fluent:Ribbon ShowQuickAccessToolBarAboveRibbon="{Binding QatAboveRibbon}" />
```

---

## DO NOT DO

### DON'T: Access Template Parts Directly

```csharp
// WRONG - Implementation detail, will break
var panel = qat.Template.FindName("PART_ToolBarPanel", qat) as Panel;
panel.Background = Brushes.Red;
```

### DON'T: Modify Items While Iterating

```csharp
// WRONG - Collection modified during iteration
foreach (var item in qat.Items)
{
    if (ShouldRemove(item))
        qat.Items.Remove(item);  // Throws!
}

// RIGHT - Use ToList() or iterate backwards
foreach (var item in qat.Items.ToList())
{
    if (ShouldRemove(item))
        qat.Items.Remove(item);
}
```

### DON'T: Add Non-IQuickAccessItemProvider Controls via Ribbon

```csharp
// WRONG - Will throw if control doesn't implement interface
ribbon.AddToQuickAccessToolBar(myCustomControl);

// RIGHT - Add directly to Items collection
qat.Items.Add(myCustomControl);
```

### DO: Use Ribbon Methods for Standard Controls

```csharp
// RIGHT - Uses IQuickAccessItemProvider to create proper clone
ribbon.AddToQuickAccessToolBar(myFluentButton);
```

---

## Source Files

| File | Purpose |
|------|---------|
| `Fluent.Ribbon\Controls\QuickAccessToolBar.cs` | Main QAT control |
| `Fluent.Ribbon\Controls\QuickAccessMenuItem.cs` | Menu item + IQuickAccessItemProvider |
| `Fluent.Ribbon\Themes\Controls\QuickAccessToolbar.xaml` | Styles and templates |
| `Fluent.Ribbon\Localization\RibbonLocalization.cs` | Localization strings |
| `Fluent.Ribbon.Tests\Controls\QuickAccessToolBarTests.cs` | Unit tests (good examples) |

---

## Summary

| Task | Solution |
|------|----------|
| Add item to QAT | `qat.Items.Add(button)` or `ribbon.AddToQuickAccessToolBar(control)` |
| Remove item | `qat.Items.Remove(button)` or `ribbon.RemoveFromQuickAccessToolBar(control)` |
| Move QAT position | `qat.ShowAboveRibbon = false` |
| Disable position change | `CanQuickAccessLocationChanging="False"` |
| Hide customize menu | `IsMenuDropDownVisible="False"` |
| Prevent control from QAT | `CanAddToQuickAccessToolBar="False"` |
| Custom KeyTips | Set `UpdateKeyTipsAction` |
| Listen for changes | Subscribe to `ItemsChanged` event |
| Check if in QAT | `ribbon.IsInQuickAccessToolBar(control)` |
| Force refresh | `qat.Refresh()` |
