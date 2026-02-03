---
title: Fluent.Ribbon RibbonTitleBar Reference
description: Complete reference for the RibbonTitleBar control - the title bar area
tags: [titlebar, window, controls, qat, contextual-tabs]
see_also:
  - fluent-ribbon-qat-reference-v2.md
  - fluent-ribbon-contextual-tabs-reference.md
  - ../styling/fluent-ribbon-windowglow-reference-v2.md
---

# Fluent.Ribbon RibbonTitleBar Reference (v2)

**Complete reference for the RibbonTitleBar control - the title bar area that hosts the window title, Quick Access Toolbar, and contextual tab group headers.**

*The RibbonTitleBar is the horizontal area at the top of a RibbonWindow that orchestrates multiple elements: application title, Quick Access Toolbar (QAT), and contextual tab group headers.*

---

## Overview

The `RibbonTitleBar` is a specialized `HeaderedItemsControl` that manages the layout of three distinct areas in the window's title bar region:

1. **Quick Access Toolbar** - Fast access buttons (left side)
2. **Header/Title** - Window title text (center or positioned around other elements)
3. **Contextual Tab Group Headers** - Colored headers for contextual tabs (above corresponding tabs)

```
+--------------------------------------------------------------------------------+
| [QAT Buttons]       MyApplication Title       [Picture Tools]     [-][O][X]    |
+--------------------------------------------------------------------------------+
| [Icon] [QAT Area]          [Title]            [Contextual Headers] [Commands]  |
+--------------------------------------------------------------------------------+
```

**Key Concepts:**
- `RibbonTitleBar` is a `HeaderedItemsControl` where items are `RibbonContextualTabGroup` instances
- The control performs custom measure/arrange logic to position its three areas
- Supports window dragging via `WindowSteeringHelper` integration
- The `IsCollapsed` property hides QAT and contextual groups when the ribbon is minimized

---

## Class Definition

```csharp
[StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(RibbonContextualTabGroup))]
[TemplatePart(Name = "PART_QuickAccessToolbarHolder", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_HeaderHolder", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_ItemsContainer", Type = typeof(Panel))]
public class RibbonTitleBar : HeaderedItemsControl
```

**Inheritance:** `HeaderedItemsControl` -> `ItemsControl` -> `Control` -> `FrameworkElement`

---

## Template Parts

| Part Name | Type | Description |
|-----------|------|-------------|
| `PART_QuickAccessToolbarHolder` | `FrameworkElement` | Container for the Quick Access Toolbar |
| `PART_HeaderHolder` | `FrameworkElement` | Container for the window title (Header property) |
| `PART_ItemsContainer` | `Panel` | Container for contextual tab group headers (`RibbonContextualGroupsContainer`) |

---

## Key Properties

### QuickAccessToolBar

```csharp
public FrameworkElement? QuickAccessToolBar
{
    get => (FrameworkElement?)this.GetValue(QuickAccessToolBarProperty);
    set => this.SetValue(QuickAccessToolBarProperty, value);
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `QuickAccessToolBar` | `FrameworkElement?` | `null` | The Quick Access Toolbar control to display |

**Behavior:**
- When set, triggers a layout pass via `ScheduleForceMeasureAndArrange()`
- The QAT is positioned at the left side of the title bar
- The QAT's `MinWidth` is respected when calculating contextual group positions

### HeaderAlignment

```csharp
public HorizontalAlignment HeaderAlignment
{
    get => (HorizontalAlignment)this.GetValue(HeaderAlignmentProperty);
    set => this.SetValue(HeaderAlignmentProperty, value);
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `HeaderAlignment` | `HorizontalAlignment` | `Center` | How the title text is aligned |

**Alignment Behaviors:**
- `Left` - Title positioned immediately after QAT, or after contextual groups if space is limited
- `Center` - Title centered in available space (default, like Microsoft Office)
- `Right` - Title right-aligned in available space
- `Stretch` - Title fills available space

### IsCollapsed

```csharp
public bool IsCollapsed
{
    get => (bool)this.GetValue(IsCollapsedProperty);
    set => this.SetValue(IsCollapsedProperty, BooleanBoxes.Box(value));
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IsCollapsed` | `bool` | `false` | Whether the title bar is in collapsed mode |

**Behavior:**
- When `true`, QAT and contextual tab groups are hidden
- Only the header/title remains visible
- Typically bound to `Ribbon.IsMinimized` or `RibbonWindow.IsCollapsed`

### HideContextTabs

```csharp
public bool HideContextTabs
{
    get => (bool)this.GetValue(HideContextTabsProperty);
    set => this.SetValue(HideContextTabsProperty, BooleanBoxes.Box(value));
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `HideContextTabs` | `bool` | `true` | Whether contextual tab group headers are hidden |

**Behavior:**
- When `true`, contextual group headers are set to `Visibility.Hidden` (space preserved)
- When `false`, contextual groups display above their corresponding tabs
- This property controls display in the title bar; tab visibility is controlled separately

### Inherited Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | `object` | Window title text (inherited from `HeaderedItemsControl`) |
| `HeaderTemplate` | `DataTemplate` | Template for rendering the title |
| `Items` | `ItemCollection` | Collection of `RibbonContextualTabGroup` items |
| `ItemsSource` | `IEnumerable` | Binding source for contextual groups |
| `ItemContainerStyle` | `Style` | Style applied to contextual group containers |

---

## Title Text Customization

### Basic Title Binding

The title is typically bound to the window's `Title` property:

```xml
<Fluent:RibbonWindow x:Class="MyApp.MainWindow"
                     Title="My Application">
    <!-- RibbonTitleBar.Header is automatically bound to Title -->
</Fluent:RibbonWindow>
```

### Custom Header Template

```xml
<Fluent:RibbonTitleBar Header="{Binding Title, RelativeSource={RelativeSource AncestorType=Window}}">
    <Fluent:RibbonTitleBar.HeaderTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="{Binding}"
                           FontWeight="Bold"
                           VerticalAlignment="Center" />
                <TextBlock Text=" - "
                           VerticalAlignment="Center" />
                <TextBlock Text="{Binding CurrentDocument.Name}"
                           FontStyle="Italic"
                           VerticalAlignment="Center" />
            </StackPanel>
        </DataTemplate>
    </Fluent:RibbonTitleBar.HeaderTemplate>
</Fluent:RibbonTitleBar>
```

### Default Header Template

The default template centers the text and applies ellipsis trimming:

```xml
<DataTemplate>
    <TextBlock HorizontalAlignment="Center"
               VerticalAlignment="Center"
               Text="{Binding}"
               TextTrimming="CharacterEllipsis"
               TextWrapping="NoWrap" />
</DataTemplate>
```

### Title Alignment Examples

```xml
<!-- Left-aligned title (uncommon) -->
<Fluent:RibbonTitleBar HeaderAlignment="Left" />

<!-- Center-aligned title (default, Office-style) -->
<Fluent:RibbonTitleBar HeaderAlignment="Center" />

<!-- Right-aligned title -->
<Fluent:RibbonTitleBar HeaderAlignment="Right" />

<!-- Stretch to fill available space -->
<Fluent:RibbonTitleBar HeaderAlignment="Stretch" />
```

---

## Contextual Tab Group Headers Display

The `RibbonTitleBar` is responsible for displaying the colored headers of contextual tab groups. These headers appear above the corresponding tabs in the ribbon.

### How It Works

1. `RibbonTitleBar` is an `ItemsControl` for `RibbonContextualTabGroup` items
2. The `ItemsSource` is bound to `Ribbon.ContextualGroups`
3. A `RibbonContextualGroupsContainer` panel positions the headers to align with their tabs

### Integration with Ribbon

```csharp
// In Ribbon.cs - when TitleBar is set
private static void OnTitleBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    var ribbon = (Ribbon)d;

    if (e.NewValue is RibbonTitleBar newValue)
    {
        newValue.ItemsSource = ribbon.ContextualGroups;
        // ...
    }
}
```

### Contextual Group Positioning Logic

The title bar's `Update()` method calculates positions based on:

1. **First visible tab position** - Where the leftmost contextual tab starts
2. **Last visible tab position** - Where the rightmost contextual tab ends
3. **Available width** - Total title bar width minus other elements

```csharp
// Simplified from Update() method
var startX = visibleGroups.First().FirstVisibleItem?.TranslatePoint(pointZero, this).X ?? 0;
var endX = lastItem?.TranslatePoint(new Point(lastItem.DesiredSize.Width, 0), this).X ?? 0;

// Position contextual headers
this.itemsRect = new Rect(startX, 0, endX - startX, constraint.Height);
```

### HideContextTabs Behavior

```xml
<!-- Hide contextual tab headers (but tabs still visible in ribbon) -->
<Fluent:RibbonTitleBar HideContextTabs="True" />

<!-- Show contextual tab headers -->
<Fluent:RibbonTitleBar HideContextTabs="False" />
```

When `HideContextTabs="True"`:
- Headers in the title bar are hidden (`Visibility.Hidden`)
- The tabs themselves in the ribbon remain visible
- Layout space is still calculated but headers are not rendered

---

## Quick Access Toolbar Integration

The title bar hosts the Quick Access Toolbar when it's positioned above the ribbon.

### How QAT Integration Works

```csharp
// In Ribbon.cs
private void MoveQuickAccessToolBarToTitleBar(RibbonTitleBar? titleBar)
{
    if (titleBar is not null)
    {
        titleBar.QuickAccessToolBar = this.QuickAccessToolBar;
    }
}

private void RemoveQuickAccessToolBarFromTitleBar(RibbonTitleBar? titleBar)
{
    if (titleBar is not null)
    {
        titleBar.QuickAccessToolBar = null;
    }
}
```

### QAT Position Control

The `Ribbon.ShowQuickAccessToolBarAboveRibbon` property controls placement:

```xml
<!-- QAT in title bar (above ribbon) - default -->
<Fluent:Ribbon ShowQuickAccessToolBarAboveRibbon="True" />

<!-- QAT below ribbon -->
<Fluent:Ribbon ShowQuickAccessToolBarAboveRibbon="False" />
```

### Layout Priority

When space is limited, elements are prioritized:

1. **QAT** - Gets minimum required width (respects `MinWidth`)
2. **Contextual Groups** - Positioned above their tabs
3. **Title** - Fills remaining space, may be truncated

```csharp
// From Update() method - QAT width calculation
if (constraint.Width <= this.quickAccessToolbarHolder.DesiredSize.Width + 50)
{
    this.quickAccessToolbarRect = new Rect(0, 0,
        Math.Max(0, constraint.Width - 50),
        this.quickAccessToolbarHolder.DesiredSize.Height);
}
```

---

## Window Commands Area

The window commands (minimize, maximize, close buttons) are NOT part of `RibbonTitleBar`. They are positioned separately in the `RibbonWindow` template.

### RibbonWindow Template Structure

```xml
<DockPanel Height="{TemplateBinding TitleBarHeight}">
    <!-- Icon (left) -->
    <Image x:Name="PART_Icon" DockPanel.Dock="Left" />

    <!-- Window Commands (right) -->
    <ContentPresenter x:Name="PART_WindowCommands"
                      Content="{TemplateBinding WindowCommands}"
                      DockPanel.Dock="Right" />

    <!-- RibbonTitleBar fills remaining space -->
    <Fluent:RibbonTitleBar x:Name="PART_RibbonTitleBar"
                           Header="{TemplateBinding Title}"
                           Foreground="{TemplateBinding TitleForeground}"
                           IsCollapsed="{TemplateBinding IsCollapsed}" />
</DockPanel>
```

### Custom Window Commands

```xml
<Fluent:RibbonWindow.WindowCommands>
    <StackPanel Orientation="Horizontal">
        <Button Content="Help" Command="{Binding HelpCommand}" />
        <!-- Standard buttons handled by RibbonWindow -->
    </StackPanel>
</Fluent:RibbonWindow.WindowCommands>
```

---

## WindowSteeringHelperControl

The `WindowSteeringHelperControl` is a helper control that enables window dragging and system menu functionality in custom areas.

### Class Definition

```csharp
public class WindowSteeringHelperControl : Border
{
    static WindowSteeringHelperControl()
    {
        BackgroundProperty.OverrideMetadata(typeof(WindowSteeringHelperControl),
            new FrameworkPropertyMetadata(Brushes.Transparent));
        IsHitTestVisibleProperty.OverrideMetadata(typeof(WindowSteeringHelperControl),
            new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
        HorizontalAlignmentProperty.OverrideMetadata(typeof(WindowSteeringHelperControl),
            new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));
        VerticalAlignmentProperty.OverrideMetadata(typeof(WindowSteeringHelperControl),
            new FrameworkPropertyMetadata(VerticalAlignment.Stretch));
    }
}
```

### Behavior

| Action | Result |
|--------|--------|
| Left-click + drag | Move window (window drag) |
| Double-click | Toggle maximize/restore |
| Right-click | Show system menu |

### RibbonTitleBar's Built-in Window Steering

The `RibbonTitleBar` itself implements window steering directly:

```csharp
protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
{
    base.OnMouseLeftButtonDown(e);

    if (e.Handled)
        return;

    // Contextual groups handle their own mouse events
    if (e.Source is RibbonContextualGroupsContainer or RibbonContextualTabGroup)
        return;

    WindowSteeringHelper.HandleMouseLeftButtonDown(e, true, true);
}

protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
{
    base.OnMouseRightButtonUp(e);

    if (e.Handled || this.IsMouseDirectlyOver == false)
        return;

    WindowSteeringHelper.ShowSystemMenu(this, e);
}
```

### Using WindowSteeringHelperControl

Use this control when you need window steering in custom areas:

```xml
<!-- Add draggable area in a custom template -->
<Grid>
    <Fluent:WindowSteeringHelperControl />
    <TextBlock Text="Drag me to move the window"
               IsHitTestVisible="False" />
</Grid>
```

### WindowSteeringHelper Methods

```csharp
public static class WindowSteeringHelper
{
    // Handle left-button for drag/double-click maximize
    public static void HandleMouseLeftButtonDown(
        MouseButtonEventArgs e,
        bool handleDragMove,
        bool handleStateChange);

    // Show system menu at mouse position
    public static void ShowSystemMenu(DependencyObject dependencyObject, MouseButtonEventArgs e);

    // Show system menu at specific screen location
    public static void ShowSystemMenu(Window window, Point screenLocation);
}
```

---

## Styling and Theming

### Style Keys

| Key | Description |
|-----|-------------|
| `Fluent.Ribbon.Styles.RibbonTitleBar` | Default style |
| `Fluent.Ribbon.Templates.RibbonTitleBar` | Control template |

### Default Style

```xml
<Style x:Key="Fluent.Ribbon.Styles.RibbonTitleBar"
       TargetType="{x:Type Fluent:RibbonTitleBar}">
    <Setter Property="Focusable" Value="False" />
    <Setter Property="HeaderTemplate">
        <Setter.Value>
            <DataTemplate>
                <TextBlock HorizontalAlignment="Center"
                           VerticalAlignment="Center"
                           Text="{Binding}"
                           TextTrimming="CharacterEllipsis"
                           TextWrapping="NoWrap" />
            </DataTemplate>
        </Setter.Value>
    </Setter>
    <Setter Property="HorizontalAlignment" Value="Stretch" />
    <Setter Property="Template" Value="{DynamicResource Fluent.Ribbon.Templates.RibbonTitleBar}" />
    <Setter Property="VerticalAlignment" Value="Top" />
</Style>
```

### Default Control Template

```xml
<ControlTemplate x:Key="Fluent.Ribbon.Templates.RibbonTitleBar"
                 TargetType="{x:Type Fluent:RibbonTitleBar}">
    <Grid>
        <ContentPresenter x:Name="PART_QuickAccessToolbarHolder"
                          ContentSource="QuickAccessToolBar" />

        <ContentPresenter x:Name="PART_HeaderHolder"
                          ContentSource="Header"
                          IsHitTestVisible="False" />

        <Fluent:RibbonContextualGroupsContainer x:Name="PART_ItemsContainer"
                                                IsItemsHost="True" />
    </Grid>
    <ControlTemplate.Triggers>
        <Trigger Property="IsCollapsed" Value="True">
            <Setter TargetName="PART_ItemsContainer" Property="Visibility" Value="Collapsed" />
            <Setter TargetName="PART_QuickAccessToolbarHolder" Property="Visibility" Value="Collapsed" />
        </Trigger>
        <Trigger Property="HideContextTabs" Value="True">
            <Setter TargetName="PART_ItemsContainer" Property="Visibility" Value="Hidden" />
        </Trigger>
    </ControlTemplate.Triggers>
</ControlTemplate>
```

### Custom Title Bar Styling

```xml
<Style TargetType="Fluent:RibbonTitleBar"
       BasedOn="{StaticResource Fluent.Ribbon.Styles.RibbonTitleBar}">
    <Setter Property="HeaderTemplate">
        <Setter.Value>
            <DataTemplate>
                <TextBlock Text="{Binding}"
                           FontFamily="Segoe UI"
                           FontSize="12"
                           FontWeight="SemiBold"
                           Foreground="{DynamicResource Fluent.Ribbon.Brushes.RibbonWindow.TitleForeground}"
                           HorizontalAlignment="Center"
                           VerticalAlignment="Center"
                           TextTrimming="CharacterEllipsis" />
            </DataTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

### Theming the Title Bar Background

The title bar background is controlled by `RibbonWindow.TitleBackground`:

```xml
<Fluent:RibbonWindow TitleBackground="{DynamicResource Fluent.Ribbon.Brushes.AccentBase}"
                     TitleForeground="White">
    <!-- ... -->
</Fluent:RibbonWindow>
```

---

## Layout Algorithm

The `RibbonTitleBar` uses a sophisticated layout algorithm to position its three areas.

### Update() Method Flow

```
1. Determine visible contextual groups
   - Filter by HideContextTabs property
   - Filter by InnerVisibility == Visible
   - Filter by Items.Count > 0

2. Check if RibbonTabControl can scroll
   - If scrolling, treat as no visible groups

3. Calculate layout based on state:

   IF IsCollapsed:
     - QAT hidden (0 width)
     - Items hidden (0 width)
     - Header fills available space

   ELSE IF no visible groups OR tab control scrolling:
     - Items hidden (0 width)
     - QAT measured and positioned left
     - Header fills remaining space (respecting alignment)

   ELSE (visible contextual groups):
     - Calculate startX from first visible tab position
     - Calculate endX from last visible tab position
     - Position items (contextual headers) from startX to endX
     - QAT gets space up to startX
     - Header positioned in remaining gaps based on alignment
```

### Space Allocation Priority

1. **Contextual groups** - Get exact space above their tabs
2. **Quick Access Toolbar** - Gets requested width (up to startX)
3. **Title** - Gets remaining space, may be positioned left or right of contextual groups

### Thresholds

The layout uses a 150px threshold for title placement decisions:

```csharp
// If space before contextual groups > 150px, put title there
if (startX - quickAccessToolbarWidth > 150)
{
    // Title goes between QAT and contextual groups
}
else
{
    // Title goes after contextual groups
}
```

---

## IRibbonWindow Interface

The `IRibbonWindow` interface provides access to the title bar:

```csharp
public interface IRibbonWindow
{
    RibbonTitleBar? TitleBar { get; }
}
```

This enables interop scenarios where non-`RibbonWindow` windows need ribbon functionality.

---

## DO NOT DO

### DON'T: Manually Set Items or ItemsSource

```xml
<!-- WRONG - ItemsSource is managed by Ribbon -->
<Fluent:RibbonTitleBar>
    <Fluent:RibbonTitleBar.ItemsSource>
        <x:Array Type="Fluent:RibbonContextualTabGroup">
            <Fluent:RibbonContextualTabGroup Header="My Group" />
        </x:Array>
    </Fluent:RibbonTitleBar.ItemsSource>
</Fluent:RibbonTitleBar>

<!-- RIGHT - Define contextual groups in Ribbon.ContextualGroups -->
<Fluent:Ribbon>
    <Fluent:Ribbon.ContextualGroups>
        <Fluent:RibbonContextualTabGroup x:Name="myGroup" Header="My Group" />
    </Fluent:Ribbon.ContextualGroups>
</Fluent:Ribbon>
```

### DON'T: Set QuickAccessToolBar Directly

```csharp
// WRONG - QAT is managed by Ribbon based on ShowQuickAccessToolBarAboveRibbon
titleBar.QuickAccessToolBar = myQAT;

// RIGHT - Let Ribbon manage it
ribbon.ShowQuickAccessToolBarAboveRibbon = true;  // QAT moves to title bar
ribbon.ShowQuickAccessToolBarAboveRibbon = false; // QAT moves below ribbon
```

### DON'T: Confuse HideContextTabs with Group Visibility

```csharp
// WRONG - This only hides HEADERS in title bar, not the tabs
titleBar.HideContextTabs = true;
// The contextual tabs still appear in the ribbon!

// RIGHT - Control group visibility to hide both header AND tabs
myContextualGroup.Visibility = Visibility.Collapsed;
```

### DON'T: Override Template Without All Parts

```xml
<!-- WRONG - Missing required template parts -->
<ControlTemplate TargetType="Fluent:RibbonTitleBar">
    <TextBlock Text="{TemplateBinding Header}" />
</ControlTemplate>

<!-- RIGHT - Include all template parts -->
<ControlTemplate TargetType="Fluent:RibbonTitleBar">
    <Grid>
        <ContentPresenter x:Name="PART_QuickAccessToolbarHolder"
                          ContentSource="QuickAccessToolBar" />
        <ContentPresenter x:Name="PART_HeaderHolder"
                          ContentSource="Header" />
        <Fluent:RibbonContextualGroupsContainer x:Name="PART_ItemsContainer"
                                                IsItemsHost="True" />
    </Grid>
</ControlTemplate>
```

### DON'T: Expect Hit Testing on Header

```xml
<!-- The header is NOT hit-testable by default -->
<ContentPresenter x:Name="PART_HeaderHolder"
                  ContentSource="Header"
                  IsHitTestVisible="False" />  <!-- Note: False! -->
```

If you need interactive elements in the title, don't put them in the header template.

### DON'T: Interfere with Window Steering Events

```csharp
// WRONG - Blocking title bar mouse events breaks window dragging
titleBar.MouseLeftButtonDown += (s, e) => e.Handled = true;

// RIGHT - Let events bubble through for window steering
// Only handle specific interactive elements
```

### DON'T: Use WindowSteeringHelperControl Over Interactive Content

```xml
<!-- WRONG - Buttons won't be clickable -->
<Grid>
    <Fluent:WindowSteeringHelperControl />
    <Button Content="Click Me" />
</Grid>

<!-- RIGHT - Content on top of steering control, or use HitTestVisible -->
<Grid>
    <Fluent:WindowSteeringHelperControl />
    <Button Content="Click Me" Panel.ZIndex="1" />
</Grid>
```

---

## Common Patterns

### Pattern 1: Dynamic Title with Document Name

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    private string _documentName = "Untitled";

    public string WindowTitle => $"MyApp - {_documentName}";

    public string DocumentName
    {
        get => _documentName;
        set
        {
            _documentName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WindowTitle));
        }
    }
}
```

```xml
<Fluent:RibbonWindow Title="{Binding WindowTitle}" />
```

### Pattern 2: Responsive Title Hiding

```xml
<Fluent:RibbonTitleBar.HeaderTemplate>
    <DataTemplate>
        <TextBlock Text="{Binding}"
                   Visibility="{Binding ActualWidth,
                       RelativeSource={RelativeSource AncestorType=Fluent:RibbonTitleBar},
                       Converter={StaticResource WidthToVisibilityConverter}}" />
    </DataTemplate>
</Fluent:RibbonTitleBar.HeaderTemplate>
```

### Pattern 3: Custom Title Bar in Non-RibbonWindow

```xml
<Window xmlns:Fluent="urn:fluent-ribbon">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <Fluent:RibbonTitleBar Grid.Row="0"
                               Header="{Binding Title, RelativeSource={RelativeSource AncestorType=Window}}"
                               HeaderAlignment="Center" />

        <Fluent:Ribbon Grid.Row="1" />
    </Grid>
</Window>
```

---

## Source Files

| File | Purpose |
|------|---------|
| `Fluent.Ribbon\Controls\RibbonTitleBar.cs` | Main control implementation with layout logic |
| `Fluent.Ribbon\Themes\Controls\RibbonTitleBar.xaml` | Default styles and templates |
| `Fluent.Ribbon\Controls\WindowSteeringHelperControl.cs` | Helper for window drag behavior |
| `Fluent.Ribbon\Helpers\WindowSteeringHelper.cs` | Static helper methods for window operations |
| `Fluent.Ribbon\Controls\RibbonContextualGroupsContainer.cs` | Panel for positioning contextual group headers |
| `Fluent.Ribbon\Themes\RibbonWindow.xaml` | RibbonWindow template showing title bar integration |
| `Fluent.Ribbon\IRibbonWindow.cs` | Interface for title bar access |
| `Fluent.Ribbon\Controls\Ribbon.cs` | Ribbon control with TitleBar and QAT management |

---

## Summary

| Task | Solution |
|------|----------|
| Change title alignment | `HeaderAlignment="Left\|Center\|Right\|Stretch"` |
| Custom title template | Set `HeaderTemplate` property |
| Hide contextual headers | `HideContextTabs="True"` |
| Collapse title bar | `IsCollapsed="True"` (binds to ribbon state) |
| Move QAT to/from title bar | `Ribbon.ShowQuickAccessToolBarAboveRibbon` |
| Add window dragging to custom area | Use `WindowSteeringHelperControl` |
| Show system menu | `WindowSteeringHelper.ShowSystemMenu()` |
| Get title bar from window | `IRibbonWindow.TitleBar` or `Ribbon.TitleBar` |
| Force layout update | `TitleBar.ScheduleForceMeasureAndArrange()` |

---

## Related References

- [Quick Access Toolbar Reference](fluent-ribbon-qat-reference-v2.md) - QAT positioning and customization
- [Contextual Tabs Reference](fluent-ribbon-contextual-tabs-reference.md) - Contextual group headers
- [Theming Reference](controlzex-theming-reference-v2.md) - Title bar colors and brushes
