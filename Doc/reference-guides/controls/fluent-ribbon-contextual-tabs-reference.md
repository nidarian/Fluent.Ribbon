---
title: Fluent.Ribbon Contextual Tabs Reference
description: Complete reference for contextual tab groups - tabs that appear only when relevant content is selected
tags: [contextual-tabs, tabs, controls, visibility]
see_also:
  - fluent-ribbon-titlebar-reference.md
  - fluent-ribbon-groupbox-reference.md
---

# Fluent.Ribbon Contextual Tabs Reference (v2)

**Complete reference for contextual tab groups - tabs that appear only when relevant content is selected.**

*Like Microsoft Office's "Picture Tools" or "Table Tools" that appear when you click on an image or table.*

---

## Overview

Contextual tabs are ribbon tabs that appear only when relevant content is selected. They are grouped under colored headers in the title bar area, making it clear they relate to the current selection.

```
+---------------------------------------------------------------------+
| [Icon] [QAT]    MyApp    [Picture Tools]           [-][O][x]        |  <- Contextual group header
+---------------------------------------------------------------------+
| Home | Insert | View | [Format] | [Adjust] |                        |  <- Format & Adjust are contextual
+---------------------------------------------------------------------+
| [Picture editing controls when image selected]                      |
+---------------------------------------------------------------------+
```

**Key Concepts:**
- `RibbonContextualTabGroup` - The colored header that appears in the title bar
- `RibbonTabItem.Group` - Property that associates a tab with a contextual group
- `Visibility` - Controls when contextual groups (and their tabs) appear

---

## Component Hierarchy

```
fluent:Ribbon
+-- fluent:Ribbon.ContextualGroups
    +-- fluent:RibbonContextualTabGroup (Header="Picture Tools", Visibility bound to selection)
        +-- Items[] (RibbonTabItem references, auto-populated)
            +-- RibbonTabItem (Group="{Binding ElementName=pictureGroup}")
            +-- RibbonTabItem (Group="{Binding ElementName=pictureGroup}")

fluent:Ribbon.Tabs
+-- fluent:RibbonTabItem (regular tab, no Group property)
+-- fluent:RibbonTabItem (contextual tab, Group=pictureGroup)
+-- fluent:RibbonTabItem (contextual tab, Group=pictureGroup)
```

---

## RibbonContextualTabGroup

The header control that appears in the title bar when the group is visible.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | `string` | "RibbonContextualTabGroup" | Text displayed in title bar |
| `Visibility` | `Visibility` | `Collapsed` | Controls when group appears |
| `Background` | `Brush` | theme default | Background color of the header |
| `BorderBrush` | `Brush` | theme default | Color of the top accent bar |
| `Foreground` | `Brush` | theme default | Header text color |
| `TabItemForeground` | `Brush` | theme default | Tab header text color (normal state) |
| `TabItemSelectedForeground` | `Brush` | theme default | Tab header text color (selected) |
| `TabItemMouseOverForeground` | `Brush` | theme default | Tab header text color (hover) |
| `TabItemSelectedMouseOverForeground` | `Brush` | theme default | Tab header text (selected + hover) |
| `InnerVisibility` | `Visibility` | (read-only) | Actual visibility (considers child tabs) |
| `Items` | `List<RibbonTabItem>` | (read-only) | Collection of associated tabs |
| `FirstVisibleItem` | `RibbonTabItem?` | (read-only) | First visible tab in group |
| `LastVisibleItem` | `RibbonTabItem?` | (read-only) | Last visible tab in group |
| `FirstVisibleAndEnabledItem` | `RibbonTabItem?` | (read-only) | First visible and enabled tab |

### Behavior Notes

- **Default Visibility is Collapsed**: Unlike most controls, `RibbonContextualTabGroup` defaults to `Visibility="Collapsed"`. You must explicitly set or bind `Visibility` to show it.
- **InnerVisibility**: Even if `Visibility="Visible"`, the group hides itself if all associated tabs are hidden. This is automatic.
- **Click behavior**: Clicking the contextual group header selects the first visible and enabled tab.

---

## RibbonTabItem.Group Property

Associates a tab with a contextual group.

### Key Properties on RibbonTabItem

| Property | Type | Description |
|----------|------|-------------|
| `Group` | `RibbonContextualTabGroup?` | The contextual group this tab belongs to |
| `IsContextual` | `bool` (read-only) | True if tab is associated with a group |
| `HasLeftGroupBorder` | `bool` (read-only) | True if this is the first visible tab in group |
| `HasRightGroupBorder` | `bool` (read-only) | True if this is the last visible tab in group |

### How Association Works

When you set `RibbonTabItem.Group`, the tab automatically:
1. Gets added to the group's `Items` collection
2. Sets `IsContextual = true`
3. Inherits foreground colors from the group
4. Shows/hides with the group's visibility

```xml
<!-- The Group property creates the association -->
<fluent:RibbonTabItem Header="Format"
                      Group="{Binding ElementName=pictureToolsGroup}" />
```

---

## Basic Usage

### Example: Picture Tools Contextual Group

```xml
<fluent:Ribbon>
    <!-- Define contextual groups -->
    <fluent:Ribbon.ContextualGroups>
        <fluent:RibbonContextualTabGroup x:Name="pictureToolsGroup"
                                         Header="Picture Tools"
                                         Background="#E8D089"
                                         BorderBrush="#C4A84D"
                                         Visibility="{Binding IsPictureSelected,
                                             Converter={StaticResource BoolToVisibilityConverter}}" />
    </fluent:Ribbon.ContextualGroups>

    <!-- Regular tabs -->
    <fluent:RibbonTabItem Header="Home">
        <!-- ... -->
    </fluent:RibbonTabItem>

    <fluent:RibbonTabItem Header="Insert">
        <!-- ... -->
    </fluent:RibbonTabItem>

    <!-- Contextual tab - appears when picture selected -->
    <fluent:RibbonTabItem Header="Format"
                          Group="{Binding ElementName=pictureToolsGroup}">
        <fluent:RibbonGroupBox Header="Adjust">
            <fluent:Button Header="Brightness" />
            <fluent:Button Header="Contrast" />
        </fluent:RibbonGroupBox>
        <fluent:RibbonGroupBox Header="Size">
            <fluent:Button Header="Crop" />
            <fluent:Button Header="Resize" />
        </fluent:RibbonGroupBox>
    </fluent:RibbonTabItem>
</fluent:Ribbon>
```

### ViewModel

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    private bool _isPictureSelected;

    public bool IsPictureSelected
    {
        get => _isPictureSelected;
        set
        {
            _isPictureSelected = value;
            OnPropertyChanged();
        }
    }

    // Call when selection changes
    public void OnSelectionChanged(object selectedItem)
    {
        IsPictureSelected = selectedItem is Picture;
    }
}
```

---

## Visibility Binding Patterns

### Pattern 1: Boolean to Visibility Converter

```xml
<!-- In resources -->
<BooleanToVisibilityConverter x:Key="BoolToVisibilityConverter" />

<!-- In contextual group -->
<fluent:RibbonContextualTabGroup
    Visibility="{Binding IsTableSelected,
        Converter={StaticResource BoolToVisibilityConverter}}" />
```

### Pattern 2: Direct Visibility Binding

```xml
<fluent:RibbonContextualTabGroup
    Visibility="{Binding TableToolsVisibility}" />
```

```csharp
// In ViewModel
public Visibility TableToolsVisibility
{
    get => _selectedItem is Table ? Visibility.Visible : Visibility.Collapsed;
}
```

### Pattern 3: DataTrigger on Type

```xml
<fluent:RibbonContextualTabGroup x:Name="shapeToolsGroup" Header="Shape Tools">
    <fluent:RibbonContextualTabGroup.Style>
        <Style TargetType="fluent:RibbonContextualTabGroup">
            <Setter Property="Visibility" Value="Collapsed" />
            <Style.Triggers>
                <DataTrigger Binding="{Binding SelectedItem,
                    Converter={StaticResource TypeNameConverter}}"
                    Value="Shape">
                    <Setter Property="Visibility" Value="Visible" />
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </fluent:RibbonContextualTabGroup.Style>
</fluent:RibbonContextualTabGroup>
```

### Pattern 4: Code-Behind Control

```csharp
private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
{
    var selected = (sender as ListView)?.SelectedItem;

    pictureToolsGroup.Visibility = selected is Picture
        ? Visibility.Visible
        : Visibility.Collapsed;

    tableToolsGroup.Visibility = selected is Table
        ? Visibility.Visible
        : Visibility.Collapsed;
}
```

---

## Header and Color Customization

### Standard Office-like Colors

```xml
<!-- Blue (like Excel's Chart Tools) -->
<fluent:RibbonContextualTabGroup Header="Chart Tools"
                                 Background="#D5E5F7"
                                 BorderBrush="#4A90D9"
                                 Foreground="#1F4E79" />

<!-- Orange (like Word's Table Tools) -->
<fluent:RibbonContextualTabGroup Header="Table Tools"
                                 Background="#FDEBD0"
                                 BorderBrush="#E67E22"
                                 Foreground="#935116" />

<!-- Green (like Excel's PivotTable Tools) -->
<fluent:RibbonContextualTabGroup Header="PivotTable Tools"
                                 Background="#D4EFDF"
                                 BorderBrush="#27AE60"
                                 Foreground="#196F3D" />

<!-- Purple (like Outlook's Calendar Tools) -->
<fluent:RibbonContextualTabGroup Header="Calendar Tools"
                                 Background="#E8DAEF"
                                 BorderBrush="#8E44AD"
                                 Foreground="#512E5F" />

<!-- Red (like PowerPoint's Video Tools) -->
<fluent:RibbonContextualTabGroup Header="Video Tools"
                                 Background="#FADBD8"
                                 BorderBrush="#E74C3C"
                                 Foreground="#922B21" />
```

### Tab Item Foreground Customization

```xml
<fluent:RibbonContextualTabGroup x:Name="pictureGroup"
                                 Header="Picture Tools"
                                 Background="{DynamicResource Fluent.Ribbon.Brushes.AccentBase}"
                                 BorderBrush="{DynamicResource Fluent.Ribbon.Brushes.AccentBase}"
                                 Foreground="{DynamicResource Fluent.Ribbon.Brushes.Black}"
                                 TabItemForeground="Navy"
                                 TabItemSelectedForeground="DarkBlue"
                                 TabItemMouseOverForeground="Blue"
                                 TabItemSelectedMouseOverForeground="RoyalBlue" />
```

### Using Theme Resources

```xml
<fluent:RibbonContextualTabGroup Header="Design Tools"
                                 Background="{DynamicResource Fluent.Ribbon.Brushes.AccentBase}"
                                 BorderBrush="{DynamicResource Fluent.Ribbon.Brushes.AccentBase}"
                                 Foreground="{DynamicResource Fluent.Ribbon.Brushes.IdealForeground}" />
```

### Empty Header (Tabs Only)

```xml
<!-- No header text, just the colored bar -->
<fluent:RibbonContextualTabGroup x:Name="minimalGroup"
                                 Header=""
                                 Background="#FF9D00"
                                 BorderBrush="#FF9D00" />
```

---

## Multiple Contextual Groups

### Example: Multiple Selection Types

```xml
<fluent:Ribbon>
    <fluent:Ribbon.ContextualGroups>
        <!-- Picture Tools -->
        <fluent:RibbonContextualTabGroup x:Name="pictureGroup"
                                         Header="Picture Tools"
                                         Background="#E8D089"
                                         BorderBrush="#C4A84D"
                                         Visibility="{Binding IsPictureSelected,
                                             Converter={StaticResource BoolToVis}}" />

        <!-- Table Tools -->
        <fluent:RibbonContextualTabGroup x:Name="tableGroup"
                                         Header="Table Tools"
                                         Background="#D089E8"
                                         BorderBrush="#A84DC4"
                                         Visibility="{Binding IsTableSelected,
                                             Converter={StaticResource BoolToVis}}" />

        <!-- Chart Tools -->
        <fluent:RibbonContextualTabGroup x:Name="chartGroup"
                                         Header="Chart Tools"
                                         Background="#89D0E8"
                                         BorderBrush="#4DA8C4"
                                         Visibility="{Binding IsChartSelected,
                                             Converter={StaticResource BoolToVis}}" />
    </fluent:Ribbon.ContextualGroups>

    <!-- Regular tabs -->
    <fluent:RibbonTabItem Header="Home" />
    <fluent:RibbonTabItem Header="Insert" />

    <!-- Picture contextual tabs -->
    <fluent:RibbonTabItem Header="Format"
                          Group="{Binding ElementName=pictureGroup}" />

    <!-- Table contextual tabs -->
    <fluent:RibbonTabItem Header="Design"
                          Group="{Binding ElementName=tableGroup}" />
    <fluent:RibbonTabItem Header="Layout"
                          Group="{Binding ElementName=tableGroup}" />

    <!-- Chart contextual tabs -->
    <fluent:RibbonTabItem Header="Design"
                          Group="{Binding ElementName=chartGroup}" />
    <fluent:RibbonTabItem Header="Format"
                          Group="{Binding ElementName=chartGroup}" />
</fluent:Ribbon>
```

### Multiple Groups Visible Simultaneously

Multiple contextual groups can be visible at once (e.g., when a chart inside a table is selected):

```csharp
// Both can be visible at the same time
tableGroup.Visibility = Visibility.Visible;
chartGroup.Visibility = Visibility.Visible;
```

---

## Common Patterns

### Picture Tools (Word/PowerPoint style)

```xml
<fluent:RibbonContextualTabGroup x:Name="pictureToolsGroup"
                                 Header="Picture Tools"
                                 Background="#FFFACD"
                                 BorderBrush="#FFD700" />

<fluent:RibbonTabItem Header="Format" Group="{Binding ElementName=pictureToolsGroup}">
    <fluent:RibbonGroupBox Header="Adjust">
        <fluent:Button Header="Remove Background" LargeIcon="..." />
        <fluent:Button Header="Corrections" Icon="..." />
        <fluent:Button Header="Color" Icon="..." />
        <fluent:Button Header="Artistic Effects" Icon="..." />
    </fluent:RibbonGroupBox>
    <fluent:RibbonGroupBox Header="Picture Styles">
        <fluent:InRibbonGallery ItemsSource="{Binding PictureStyles}" />
    </fluent:RibbonGroupBox>
    <fluent:RibbonGroupBox Header="Size">
        <fluent:Button Header="Crop" LargeIcon="..." />
        <fluent:Spinner Header="Height" />
        <fluent:Spinner Header="Width" />
    </fluent:RibbonGroupBox>
</fluent:RibbonTabItem>
```

### Table Tools (Word/Excel style)

```xml
<fluent:RibbonContextualTabGroup x:Name="tableToolsGroup"
                                 Header="Table Tools"
                                 Background="#FDEBD0"
                                 BorderBrush="#E67E22" />

<fluent:RibbonTabItem Header="Design" Group="{Binding ElementName=tableToolsGroup}">
    <fluent:RibbonGroupBox Header="Table Styles">
        <fluent:InRibbonGallery ItemsSource="{Binding TableStyles}" />
    </fluent:RibbonGroupBox>
    <fluent:RibbonGroupBox Header="Borders">
        <fluent:SplitButton Header="Borders" Icon="..." />
        <fluent:DropDownButton Header="Border Styles" Icon="..." />
    </fluent:RibbonGroupBox>
</fluent:RibbonTabItem>

<fluent:RibbonTabItem Header="Layout" Group="{Binding ElementName=tableToolsGroup}">
    <fluent:RibbonGroupBox Header="Rows &amp; Columns">
        <fluent:Button Header="Insert Above" Icon="..." />
        <fluent:Button Header="Insert Below" Icon="..." />
        <fluent:Button Header="Insert Left" Icon="..." />
        <fluent:Button Header="Insert Right" Icon="..." />
        <fluent:Button Header="Delete" Icon="..." />
    </fluent:RibbonGroupBox>
    <fluent:RibbonGroupBox Header="Merge">
        <fluent:Button Header="Merge Cells" Icon="..." />
        <fluent:Button Header="Split Cells" Icon="..." />
    </fluent:RibbonGroupBox>
</fluent:RibbonTabItem>
```

### Drawing Tools (PowerPoint style)

```xml
<fluent:RibbonContextualTabGroup x:Name="drawingToolsGroup"
                                 Header="Drawing Tools"
                                 Background="#D4EFDF"
                                 BorderBrush="#27AE60" />

<fluent:RibbonTabItem Header="Format" Group="{Binding ElementName=drawingToolsGroup}">
    <fluent:RibbonGroupBox Header="Insert Shapes">
        <fluent:DropDownButton Header="Shapes" LargeIcon="..." />
    </fluent:RibbonGroupBox>
    <fluent:RibbonGroupBox Header="Shape Styles">
        <fluent:InRibbonGallery ItemsSource="{Binding ShapeStyles}" />
        <fluent:ColorGallery Header="Shape Fill" />
        <fluent:ColorGallery Header="Shape Outline" />
    </fluent:RibbonGroupBox>
    <fluent:RibbonGroupBox Header="Arrange">
        <fluent:SplitButton Header="Bring Forward" Icon="..." />
        <fluent:SplitButton Header="Send Backward" Icon="..." />
        <fluent:Button Header="Align" Icon="..." />
        <fluent:Button Header="Group" Icon="..." />
    </fluent:RibbonGroupBox>
</fluent:RibbonTabItem>
```

---

## Events and Methods

### RibbonContextualTabGroup Methods

| Method | Description |
|--------|-------------|
| `UpdateInnerVisiblityAndGroupBorders()` | Recalculates visibility and border properties. Called automatically. |

### Automatic Event Handling

- **MouseLeftButtonUp**: Clicking the contextual group header selects the first visible and enabled tab
- **Visibility changes**: Automatically triggers layout updates in the title bar

### Programmatic Tab Selection

```csharp
// When showing a contextual group, optionally select its first tab
pictureToolsGroup.Visibility = Visibility.Visible;

// Select first tab in group
var firstTab = pictureToolsGroup.FirstVisibleAndEnabledItem;
if (firstTab != null)
{
    firstTab.IsSelected = true;
}
```

---

## State Diagram

```
                    +------------------------+
                    |   COLLAPSED (Hidden)   |
                    | Visibility = Collapsed |
                    +------------------------+
                              |
                              | Set Visibility = Visible
                              v
            +----------------------------------+
            |  VISIBLE (In Title Bar)          |
            |  Header shows, tabs appear       |
            |  InnerVisibility = Visible       |
            +----------------------------------+
                              |
                              | All child tabs hidden
                              v
            +----------------------------------+
            |  VISIBLE BUT HIDDEN              |
            |  Visibility = Visible            |
            |  InnerVisibility = Collapsed     |
            |  (Auto-hides when no tabs)       |
            +----------------------------------+


TAB ASSOCIATION:
                    +-------------------+
                    | RibbonTabItem     |
                    | Group = null      |
                    | IsContextual=false|
                    +-------------------+
                              |
                              | Set Group = contextualGroup
                              v
                    +-------------------+
                    | RibbonTabItem     |
                    | Group = ref       |
                    | IsContextual=true |
                    | Added to Items[]  |
                    +-------------------+
```

---

## Styles and Templates

### Style Keys

| Key | Description |
|-----|-------------|
| `Fluent.Ribbon.Styles.RibbonContextualTabGroup` | Main contextual group style |
| `Fluent.Ribbon.Templates.RibbonContextualTabGroup` | Control template |

### Brush Resources

| Key | Description |
|-----|-------------|
| `Fluent.Ribbon.Brushes.RibbonContextualTabGroup.TabItemForeground` | Default tab text color |
| `Fluent.Ribbon.Brushes.RibbonContextualTabGroup.TabItemMouseOverForeground` | Tab text on hover |
| `Fluent.Ribbon.Brushes.RibbonContextualTabGroup.TabItemSelectedForeground` | Selected tab text |
| `Fluent.Ribbon.Brushes.RibbonContextualTabGroup.TabItemSelectedMouseOverForeground` | Selected tab hover |
| `Fluent.Ribbon.Brushes.RibbonContextualTabGroup.Background.OpacityMask` | Background gradient mask |

### Template Structure

```xml
<ControlTemplate TargetType="{x:Type fluent:RibbonContextualTabGroup}">
    <Grid Visibility="{TemplateBinding InnerVisibility}">
        <!-- Background with opacity mask -->
        <Rectangle Fill="{TemplateBinding Background}"
                   OpacityMask="{DynamicResource ...OpacityMask}" />

        <!-- Top accent bar -->
        <Rectangle Height="4"
                   VerticalAlignment="Top"
                   Fill="{TemplateBinding BorderBrush}" />

        <!-- Header text -->
        <TextBlock Text="{Binding Header}"
                   Foreground="{TemplateBinding Foreground}"
                   TextTrimming="CharacterEllipsis" />
    </Grid>
</ControlTemplate>
```

---

## DO NOT DO

### DON'T: Forget Default Visibility is Collapsed

```xml
<!-- WRONG - Will never show (Visibility defaults to Collapsed) -->
<fluent:RibbonContextualTabGroup x:Name="myGroup" Header="My Tools" />

<!-- RIGHT - Bind or set Visibility -->
<fluent:RibbonContextualTabGroup x:Name="myGroup" Header="My Tools"
                                 Visibility="{Binding IsToolActive,
                                     Converter={StaticResource BoolToVis}}" />
```

### DON'T: Set Visibility on Tab Instead of Group

```xml
<!-- WRONG - Tab won't hide properly with group -->
<fluent:RibbonTabItem Header="Format"
                      Group="{Binding ElementName=pictureGroup}"
                      Visibility="{Binding IsPictureSelected, Converter={StaticResource BoolToVis}}" />

<!-- RIGHT - Set visibility on the GROUP, not the tab -->
<fluent:RibbonContextualTabGroup x:Name="pictureGroup" Header="Picture Tools"
                                 Visibility="{Binding IsPictureSelected, Converter={StaticResource BoolToVis}}" />
<fluent:RibbonTabItem Header="Format" Group="{Binding ElementName=pictureGroup}" />
```

### DON'T: Manually Add Tabs to Items Collection

```csharp
// WRONG - Items is managed automatically
pictureGroup.Items.Add(formatTab);

// RIGHT - Use the Group property on the tab
formatTab.Group = pictureGroup;
```

### DON'T: Assume Tab Order Matches Definition Order

```xml
<!-- Tabs may not appear in this order in the group -->
<fluent:RibbonTabItem Header="Tab1" Group="{Binding ElementName=group1}" />
<fluent:RibbonTabItem Header="Tab2" />  <!-- Regular tab -->
<fluent:RibbonTabItem Header="Tab3" Group="{Binding ElementName=group1}" />

<!-- BETTER - Keep contextual tabs together in XAML for clarity -->
<fluent:RibbonTabItem Header="Tab2" />  <!-- Regular tab -->
<fluent:RibbonTabItem Header="Tab1" Group="{Binding ElementName=group1}" />
<fluent:RibbonTabItem Header="Tab3" Group="{Binding ElementName=group1}" />
```

### DON'T: Use Same Name for Tabs in Different Groups

```xml
<!-- PROBLEMATIC - "Format" appears twice, confusing for users -->
<fluent:RibbonTabItem Header="Format" Group="{Binding ElementName=pictureGroup}" />
<fluent:RibbonTabItem Header="Format" Group="{Binding ElementName=tableGroup}" />

<!-- BETTER - Be specific -->
<fluent:RibbonTabItem Header="Picture Format" Group="{Binding ElementName=pictureGroup}" />
<fluent:RibbonTabItem Header="Table Format" Group="{Binding ElementName=tableGroup}" />

<!-- OR - Use same name if groups are never visible simultaneously (like Office does) -->
```

### DON'T: Rely on InnerVisibility Directly

```csharp
// WRONG - InnerVisibility is calculated, don't set it
contextualGroup.InnerVisibility = Visibility.Visible;  // Won't compile - read-only

// RIGHT - Set Visibility and let InnerVisibility calculate automatically
contextualGroup.Visibility = Visibility.Visible;
```

### DON'T: Forget to Handle Deselection

```csharp
// WRONG - Only handles selection, not deselection
void OnPictureSelected(Picture p)
{
    pictureGroup.Visibility = Visibility.Visible;
}

// RIGHT - Handle both selection and deselection
void OnSelectionChanged(object newSelection)
{
    pictureGroup.Visibility = newSelection is Picture
        ? Visibility.Visible
        : Visibility.Collapsed;
}
```

---

## Source Files

| File | Purpose |
|------|---------|
| `Fluent.Ribbon\Controls\RibbonContextualTabGroup.cs` | Main contextual group control |
| `Fluent.Ribbon\Controls\Ribbon.cs` | ContextualGroups collection property |
| `Fluent.Ribbon\Controls\RibbonTabItem.cs` | Group property and association logic |
| `Fluent.Ribbon\Themes\Controls\RibbonContextualTabGroup.xaml` | Styles and templates |
| `Fluent.Ribbon.Showcase\TestContent.xaml` | Usage examples |

---

## Summary

| Task | Solution |
|------|----------|
| Create contextual group | `<fluent:Ribbon.ContextualGroups><fluent:RibbonContextualTabGroup ... /></fluent:Ribbon.ContextualGroups>` |
| Associate tab with group | `<fluent:RibbonTabItem Group="{Binding ElementName=myGroup}" />` |
| Show/hide based on selection | Bind `Visibility` on the group, not tabs |
| Customize colors | Set `Background`, `BorderBrush`, `Foreground` |
| Customize tab colors | Set `TabItemForeground`, `TabItemSelectedForeground`, etc. |
| Multiple tabs in group | Multiple tabs with same `Group` binding |
| Multiple groups | Define multiple `RibbonContextualTabGroup` elements |
| Select first tab on show | Use `FirstVisibleAndEnabledItem.IsSelected = true` |
| Check if group is showing | `InnerVisibility == Visibility.Visible` |

---

## Quick Reference: Office-Style Contextual Groups

| Context | Header | Background | Border | Example |
|---------|--------|------------|--------|---------|
| Picture | "Picture Tools" | `#FFFACD` | `#FFD700` | Word, PowerPoint |
| Table | "Table Tools" | `#FDEBD0` | `#E67E22` | Word, Excel |
| Chart | "Chart Tools" | `#D5E5F7` | `#4A90D9` | Excel, PowerPoint |
| Drawing | "Drawing Tools" | `#D4EFDF` | `#27AE60` | PowerPoint |
| Header/Footer | "Header & Footer Tools" | `#E8DAEF` | `#8E44AD` | Word |
| Video | "Video Tools" | `#FADBD8` | `#E74C3C` | PowerPoint |
