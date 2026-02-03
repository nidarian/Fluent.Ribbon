---
title: Fluent.Ribbon Galleries Reference
description: Comprehensive reference for Gallery, InRibbonGallery, GalleryItem, and related components
tags: [gallery, inribbongallery, controls, selection]
see_also:
  - fluent-ribbon-groupbox-reference.md
  - fluent-ribbon-button-controls-reference.md
---

# Fluent.Ribbon Galleries Reference

**Comprehensive reference for Gallery, InRibbonGallery, GalleryItem, and related components.**

*Source: Fluent.Ribbon source code analysis - Gallery.cs, InRibbonGallery.cs, GalleryItem.cs, GalleryPanel.cs, GalleryGroupFilter.cs, GalleryGroupContainer.cs*

---

## Overview

Fluent.Ribbon provides two gallery controls for displaying collections of selectable items in a visual grid format:

- **Gallery** - A popup-based gallery typically hosted inside dropdown menus or split buttons
- **InRibbonGallery** - An inline gallery that displays items directly in the ribbon with popup expansion

Both controls share similar functionality for grouping, filtering, and selection, but differ in their display behavior and sizing.

---

## Control Hierarchy

```
fluent:InRibbonGallery (inline display + popup)
├── GalleryPanel (items host)
│   └── GalleryGroupContainer (per group)
│       └── GalleryItem (individual items)
├── Filters (GalleryGroupFilter collection)
└── Menu (RibbonMenu - optional footer)

fluent:Gallery (popup-only display)
├── GalleryPanel (items host)
│   └── GalleryGroupContainer (per group)
│       └── GalleryItem (individual items)
└── Filters (GalleryGroupFilter collection)
```

---

## Gallery vs InRibbonGallery Comparison

| Feature | Gallery | InRibbonGallery |
|---------|---------|-----------------|
| **Display** | Popup only (in menus/dropdowns) | Inline in ribbon + popup expansion |
| **Base Class** | ListBox | Selector |
| **Collapse to Button** | N/A | Yes (CanCollapseToButton) |
| **Ribbon Scaling** | N/A | Yes (IScalableRibbonControl) |
| **Quick Access Support** | No | Yes (IQuickAccessItemProvider) |
| **Menu Footer** | No | Yes (Menu property) |
| **Separate Dropdown Sizing** | No | Yes (MinItemsInDropDownRow, MaxItemsInDropDownRow) |
| **Simplified Mode** | No | Yes (ISimplifiedRibbonControl) |
| **Typical Use** | Inside SplitButton/DropDownButton | Standalone in RibbonGroupBox |

---

## fluent:Gallery

A gallery control typically hosted inside context menus, dropdown buttons, or split buttons.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MinItemsInRow` | int | 1 | Minimum items per row |
| `MaxItemsInRow` | int | 0 | Maximum items per row (0 = unlimited) |
| `ItemWidth` | double | NaN | Fixed width for each item |
| `ItemHeight` | double | NaN | Fixed height for each item |
| `Orientation` | Orientation | Horizontal | Layout direction |
| `IsGrouped` | bool | false | Show group headers |
| `GroupBy` | string | null | Property name for grouping |
| `GroupByAdvanced` | Func<object, string> | null | Custom grouping function |
| `Selectable` | bool | true | Allow item selection |
| `SelectedFilter` | GalleryGroupFilter | null | Currently active filter |
| `Filters` | ObservableCollection<GalleryGroupFilter> | - | Available filter definitions |
| `HasFilter` | bool | (read-only) | True if filters exist |
| `SelectedFilterTitle` | string | (read-only) | Title of current filter |
| `SelectedFilterGroups` | string | (read-only) | Groups in current filter |
| `IsLastItem` | bool | (read-only) | True if last item in parent |

### Basic Example

```xml
<!-- Gallery inside a SplitButton -->
<fluent:SplitButton Header="Paste" Icon="paste.png">
    <fluent:Gallery GroupBy="Tag"
                    ItemHeight="32"
                    ItemWidth="32"
                    MaxItemsInRow="6"
                    MinItemsInRow="2"
                    Selectable="False">
        <fluent:GalleryItem Tag="Clipboard" Command="ApplicationCommands.Paste">
            <Image Source="paste.png" />
        </fluent:GalleryItem>
        <fluent:GalleryItem Tag="Clipboard" Command="ApplicationCommands.PasteSpecial">
            <Image Source="paste-special.png" />
        </fluent:GalleryItem>
    </fluent:Gallery>
</fluent:SplitButton>
```

### Gallery with Filters

```xml
<fluent:Gallery GroupBy="Tag"
                ItemHeight="128"
                ItemWidth="128"
                IsGrouped="True"
                MaxItemsInRow="6"
                MinItemsInRow="2">
    <fluent:Gallery.Filters>
        <fluent:GalleryGroupFilter Title="All" Groups="Group1,Group2" />
        <fluent:GalleryGroupFilter Title="Group 1" Groups="Group1" />
        <fluent:GalleryGroupFilter Title="Group 2" Groups="Group2" />
    </fluent:Gallery.Filters>

    <Border Tag="Group1"><TextBlock>Item 1</TextBlock></Border>
    <Border Tag="Group2"><TextBlock>Item 2</TextBlock></Border>
</fluent:Gallery>
```

---

## fluent:InRibbonGallery

An inline gallery that displays items directly in the ribbon with scroll buttons and an expand button for popup access.

### Key Properties - Layout

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MinItemsInRow` | int | 1 | Minimum items in inline display |
| `MaxItemsInRow` | int | 8 | Maximum items in inline display |
| `MinItemsInDropDownRow` | int | 1 | Minimum items per row in popup |
| `MaxItemsInDropDownRow` | int | 0 | Maximum items per row in popup |
| `ItemWidth` | double | NaN | Fixed width for each item |
| `ItemHeight` | double | NaN | Fixed height for each item |
| `Orientation` | Orientation | Horizontal | Layout direction |
| `GalleryPanelContainerHeight` | double | 68 | Height of inline gallery panel |

### Key Properties - Display

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | object | null | Gallery header text |
| `HeaderTemplate` | DataTemplate | null | Header template |
| `Icon` | object | null | Small icon |
| `MediumIcon` | object | null | Medium icon (for middle size) |
| `LargeIcon` | object | null | Large icon (32x32) |
| `IsCollapsed` | bool | false | Collapse to button |
| `CanCollapseToButton` | bool | true | Allow automatic collapse |
| `IsDropDownOpen` | bool | false | Popup open state |
| `IsSimplified` | bool | (read-only) | Simplified ribbon mode active |

### Key Properties - Dropdown

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DropDownHeight` | double | NaN | Initial popup height |
| `DropDownWidth` | double | NaN | Initial popup width |
| `MaxDropDownHeight` | double | NaN | Maximum popup height |
| `MaxDropDownWidth` | double | 1/3 screen | Maximum popup width |
| `ResizeMode` | ContextMenuResizeMode | None | Popup resize behavior |
| `Menu` | RibbonMenu | null | Footer menu in popup |

### Key Properties - Grouping & Filtering

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `GroupBy` | string | null | Property name for grouping |
| `GroupByAdvanced` | Func<object, string> | null | Custom grouping function |
| `Selectable` | bool | true | Allow item selection |
| `SelectedFilter` | GalleryGroupFilter | null | Currently active filter |
| `Filters` | ObservableCollection<GalleryGroupFilter> | - | Available filter definitions |

### Key Properties - Ribbon Integration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Size` | RibbonControlSize | Large | Control size state |
| `SizeDefinition` | RibbonControlSizeDefinition | - | Size behavior per group state |
| `SimplifiedSizeDefinition` | RibbonControlSizeDefinition | - | Size in simplified mode |
| `KeyTip` | string | null | Keyboard access key |
| `CanAddToQuickAccessToolBar` | bool | true | Allow QAT addition |

### Key Properties - Expand Button

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ExpandButtonContent` | object | null | Content for expand button |
| `ExpandButtonContentTemplate` | DataTemplate | (arrow icon) | Template for expand button |

### Events

| Event | Description |
|-------|-------------|
| `DropDownOpened` | Fired when popup opens |
| `DropDownClosed` | Fired when popup closes |
| `Scaled` | Fired when gallery scales (ribbon resize) |
| `SelectionChanged` | Inherited - item selection changed |

### Methods

| Method | Description |
|--------|-------------|
| `ScrollIntoView(object item)` | Scroll to bring item into view |
| `Reduce()` | Scale down (fewer items or collapse) |
| `Enlarge()` | Scale up (more items or expand) |
| `ResetScale()` | Reset to original scale |

### Basic Example

```xml
<fluent:InRibbonGallery Header="Styles"
                        Icon="styles-small.png"
                        LargeIcon="styles-large.png"
                        ItemHeight="56"
                        ItemWidth="40"
                        MinItemsInRow="3"
                        MaxItemsInRow="5"
                        MinItemsInDropDownRow="4"
                        MaxItemsInDropDownRow="5"
                        ResizeMode="Both"
                        KeyTip="S">
    <fluent:GalleryItem>Style 1</fluent:GalleryItem>
    <fluent:GalleryItem>Style 2</fluent:GalleryItem>
    <fluent:GalleryItem>Style 3</fluent:GalleryItem>
</fluent:InRibbonGallery>
```

### InRibbonGallery with Grouping and Filters

```xml
<fluent:InRibbonGallery Header="Grouped Gallery"
                        GroupBy="Group"
                        ItemHeight="56"
                        ItemWidth="40"
                        ItemsSource="{Binding Items}"
                        MaxItemsInRow="5"
                        MinItemsInRow="1"
                        ResizeMode="Both">
    <fluent:InRibbonGallery.Filters>
        <fluent:GalleryGroupFilter Title="All" Groups="Group A,Group B" />
        <fluent:GalleryGroupFilter Title="Group A" Groups="Group A" />
        <fluent:GalleryGroupFilter Title="Group B" Groups="Group B" />
    </fluent:InRibbonGallery.Filters>

    <fluent:InRibbonGallery.ItemTemplate>
        <DataTemplate>
            <Border ToolTip="{Binding Name}">
                <Image Source="{Binding Icon}" />
            </Border>
        </DataTemplate>
    </fluent:InRibbonGallery.ItemTemplate>
</fluent:InRibbonGallery>
```

### InRibbonGallery with Footer Menu

```xml
<fluent:InRibbonGallery Header="Shapes"
                        ItemsSource="{Binding Shapes}"
                        ItemHeight="56"
                        ItemWidth="56">
    <fluent:InRibbonGallery.Menu>
        <fluent:RibbonMenu>
            <fluent:MenuItem Header="More Shapes..." />
            <fluent:MenuItem Header="Shape Options..." />
        </fluent:RibbonMenu>
    </fluent:InRibbonGallery.Menu>
</fluent:InRibbonGallery>
```

---

## fluent:GalleryItem

The individual item container for gallery controls. Inherits from ListBoxItem.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Group` | string | null | Group name for this item |
| `IsPressed` | bool | (read-only) | Mouse pressed state |
| `IsSelected` | bool | false | Selection state |
| `IsDefinitive` | bool | true | Close popup on click |
| `Command` | ICommand | null | Click command |
| `CommandParameter` | object | null | Command parameter |
| `CommandTarget` | IInputElement | null | Command target |
| `PreviewCommand` | ICommand | null | Command on mouse enter |
| `CancelPreviewCommand` | ICommand | null | Command on mouse leave |
| `KeyTip` | string | null | Keyboard access key |

### Events

| Event | Description |
|-------|-------------|
| `Click` | Fired when item is clicked |

### Methods

| Method | Description |
|--------|-------------|
| `RaiseClick()` | Programmatically trigger click |

### Example with Commands

```xml
<fluent:InRibbonGallery ItemsSource="{Binding Styles}">
    <fluent:InRibbonGallery.ItemContainerStyle>
        <Style TargetType="{x:Type fluent:GalleryItem}">
            <Setter Property="Command" Value="{Binding ApplyStyleCommand}" />
            <Setter Property="CommandParameter" Value="{Binding}" />
            <Setter Property="PreviewCommand" Value="{Binding PreviewStyleCommand}" />
            <Setter Property="CancelPreviewCommand" Value="{Binding CancelPreviewCommand}" />
        </Style>
    </fluent:InRibbonGallery.ItemContainerStyle>
</fluent:InRibbonGallery>
```

### Example with Custom Context Menu

```xml
<Style x:Key="GalleryItemWithContextMenu" TargetType="{x:Type fluent:GalleryItem}">
    <Setter Property="ContextMenu">
        <Setter.Value>
            <fluent:ContextMenu>
                <fluent:MenuItem Header="Apply to Selection" />
                <fluent:MenuItem Header="Modify Style..." />
            </fluent:ContextMenu>
        </Setter.Value>
    </Setter>
</Style>
```

---

## fluent:GalleryGroupFilter

Defines a filter for showing specific groups in the gallery.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Title` | string | "GalleryGroupFilter" | Display title in filter dropdown |
| `Groups` | string | "" | Comma-separated list of group names to show |

### Example

```xml
<fluent:InRibbonGallery.Filters>
    <!-- Show all items from both groups -->
    <fluent:GalleryGroupFilter Title="All" Groups="Favorites,Recent" />

    <!-- Show only favorites -->
    <fluent:GalleryGroupFilter Title="Favorites Only" Groups="Favorites" />

    <!-- Show only recent items -->
    <fluent:GalleryGroupFilter Title="Recent Only" Groups="Recent" />
</fluent:InRibbonGallery.Filters>
```

---

## fluent:GalleryGroupContainer

Internal container that displays a group of items with an optional header. Created automatically by GalleryPanel.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | object | inherited | Group header text |
| `IsHeadered` | bool | true | Show group header |
| `Orientation` | Orientation | Horizontal | Item layout direction |
| `ItemWidth` | double | NaN | Item width |
| `ItemHeight` | double | NaN | Item height |
| `MinItemsInRow` | int | 0 | Minimum items per row |
| `MaxItemsInRow` | int | 0 | Maximum items per row |

---

## Grouping Items

### Method 1: GroupBy Property (Simple)

Use when items have a property containing the group name.

```csharp
// ViewModel item
public class StyleItem
{
    public string Name { get; set; }
    public string Group { get; set; }  // "Favorites", "Recent", etc.
    public ImageSource Icon { get; set; }
}
```

```xml
<fluent:InRibbonGallery GroupBy="Group"
                        ItemsSource="{Binding StyleItems}" />
```

### Method 2: GroupByAdvanced (Custom Logic)

Use when grouping logic is more complex.

```csharp
// In ViewModel
public Func<object, string> GroupByAdvancedSample => item =>
{
    if (item is StyleItem styleItem)
    {
        return styleItem.IsFavorite ? "Favorites" : "Other Styles";
    }
    return "Undefined";
};
```

```xml
<fluent:InRibbonGallery GroupByAdvanced="{Binding GroupByAdvancedSample}"
                        ItemsSource="{Binding StyleItems}" />
```

### Method 3: GalleryItem.Group Property (Direct)

Use when adding items directly in XAML.

```xml
<fluent:Gallery IsGrouped="True">
    <fluent:GalleryItem Group="Group 1">Item A</fluent:GalleryItem>
    <fluent:GalleryItem Group="Group 1">Item B</fluent:GalleryItem>
    <fluent:GalleryItem Group="Group 2">Item C</fluent:GalleryItem>
</fluent:Gallery>
```

### Method 4: Using Tag Property

For non-GalleryItem content, use the Tag property with GroupBy="Tag".

```xml
<fluent:Gallery GroupBy="Tag" IsGrouped="True">
    <TextBlock Tag="Group1">Item 1</TextBlock>
    <TextBlock Tag="Group1">Item 2</TextBlock>
    <TextBlock Tag="Group2">Item 3</TextBlock>
</fluent:Gallery>
```

---

## Filtering

Filters allow users to show only specific groups in the gallery.

### Setting Up Filters

```xml
<fluent:InRibbonGallery GroupBy="Category"
                        ItemsSource="{Binding Items}">
    <fluent:InRibbonGallery.Filters>
        <fluent:GalleryGroupFilter x:Name="AllFilter"
                                   Title="All Items"
                                   Groups="Business,Personal,Templates" />
        <fluent:GalleryGroupFilter Title="Business"
                                   Groups="Business" />
        <fluent:GalleryGroupFilter Title="Personal"
                                   Groups="Personal" />
        <fluent:GalleryGroupFilter Title="Templates"
                                   Groups="Templates" />
    </fluent:InRibbonGallery.Filters>
</fluent:InRibbonGallery>
```

### Setting Default Filter

```xml
<fluent:InRibbonGallery SelectedFilter="{Binding ElementName=AllFilter}">
    <fluent:InRibbonGallery.Filters>
        <fluent:GalleryGroupFilter x:Name="AllFilter" Title="All" Groups="A,B,C" />
        <!-- other filters -->
    </fluent:InRibbonGallery.Filters>
</fluent:InRibbonGallery>
```

**Note:** When no filter is explicitly selected, the first filter is automatically selected.

---

## Selection Handling

### Basic Selection

```xml
<fluent:InRibbonGallery x:Name="styleGallery"
                        SelectedItem="{Binding SelectedStyle, Mode=TwoWay}"
                        SelectionChanged="OnStyleSelectionChanged" />
```

### Disabling Selection

Set `Selectable="False"` when the gallery should only trigger commands, not maintain selection state.

```xml
<fluent:Gallery Selectable="False">
    <fluent:GalleryItem Command="{Binding PasteTextCommand}">
        <Image Source="paste-text.png" />
    </fluent:GalleryItem>
</fluent:Gallery>
```

### Preview Commands

Use `PreviewCommand` and `CancelPreviewCommand` for live preview on hover.

```xml
<fluent:InRibbonGallery ItemsSource="{Binding Styles}">
    <fluent:InRibbonGallery.ItemContainerStyle>
        <Style TargetType="{x:Type fluent:GalleryItem}">
            <Setter Property="PreviewCommand"
                    Value="{Binding DataContext.PreviewCommand,
                            RelativeSource={RelativeSource AncestorType={x:Type fluent:InRibbonGallery}}}" />
            <Setter Property="CancelPreviewCommand"
                    Value="{Binding DataContext.CancelPreviewCommand,
                            RelativeSource={RelativeSource AncestorType={x:Type fluent:InRibbonGallery}}}" />
        </Style>
    </fluent:InRibbonGallery.ItemContainerStyle>
</fluent:InRibbonGallery>
```

---

## Sizing (MinItemsInRow, MaxItemsInRow)

### Inline vs Dropdown Sizing

InRibbonGallery has separate sizing for inline and popup display:

| Property | Controls |
|----------|----------|
| `MinItemsInRow` / `MaxItemsInRow` | Inline ribbon display |
| `MinItemsInDropDownRow` / `MaxItemsInDropDownRow` | Popup display |

```xml
<fluent:InRibbonGallery
    MinItemsInRow="2"              <!-- At least 2 items in ribbon -->
    MaxItemsInRow="5"              <!-- At most 5 items in ribbon -->
    MinItemsInDropDownRow="4"      <!-- At least 4 items in popup row -->
    MaxItemsInDropDownRow="8"      <!-- At most 8 items in popup row -->
    ItemWidth="40"
    ItemHeight="56" />
```

### Automatic Scaling

InRibbonGallery participates in ribbon scaling:

1. When ribbon shrinks, `MaxItemsInRow` decreases
2. If `MinItemsInRow` is reached and `CanCollapseToButton=true`, gallery collapses to button
3. When ribbon expands, items scale back up

```xml
<!-- Allow collapse to button when space is tight -->
<fluent:InRibbonGallery CanCollapseToButton="True"
                        MinItemsInRow="2"
                        MaxItemsInRow="8" />

<!-- Prevent collapse - always show inline gallery -->
<fluent:InRibbonGallery CanCollapseToButton="False"
                        MinItemsInRow="2"
                        MaxItemsInRow="8" />
```

### Fixed Collapsed State

```xml
<!-- Start collapsed (as button) -->
<fluent:InRibbonGallery IsCollapsed="True"
                        Header="Many Items"
                        LargeIcon="gallery.png" />
```

---

## ItemsSource Binding

### Basic Data Binding

```csharp
public class MainViewModel
{
    public ObservableCollection<StyleItem> Styles { get; } = new();
}

public class StyleItem
{
    public string Name { get; set; }
    public string Group { get; set; }
    public ImageSource Icon { get; set; }
    public ImageSource LargeIcon { get; set; }
}
```

```xml
<fluent:InRibbonGallery ItemsSource="{Binding Styles}"
                        GroupBy="Group"
                        DisplayMemberPath="Name"
                        ItemHeight="56"
                        ItemWidth="40" />
```

### With ItemTemplate

```xml
<fluent:InRibbonGallery ItemsSource="{Binding Styles}"
                        GroupBy="Group"
                        ItemHeight="56"
                        ItemWidth="40">
    <fluent:InRibbonGallery.ItemTemplate>
        <DataTemplate DataType="{x:Type local:StyleItem}">
            <Border ToolTip="{Binding Name}">
                <Image Source="{Binding Icon}" Stretch="Uniform" />
            </Border>
        </DataTemplate>
    </fluent:InRibbonGallery.ItemTemplate>
</fluent:InRibbonGallery>
```

### With ItemContainerStyle

```xml
<fluent:InRibbonGallery ItemsSource="{Binding Styles}">
    <fluent:InRibbonGallery.ItemContainerStyle>
        <Style TargetType="{x:Type fluent:GalleryItem}">
            <Setter Property="Command" Value="{Binding ApplyCommand}" />
            <Setter Property="ToolTip" Value="{Binding Description}" />
        </Style>
    </fluent:InRibbonGallery.ItemContainerStyle>
</fluent:InRibbonGallery>
```

---

## Styling Gallery Items

### Default Style Keys

| Key | Description |
|-----|-------------|
| `Fluent.Ribbon.Styles.GalleryItem` | Default GalleryItem style |
| `Fluent.Ribbon.Templates.GalleryItem` | Default GalleryItem template |
| `Fluent.Ribbon.Styles.GalleryGroupContainer` | Group container style |
| `Fluent.Ribbon.Templates.GalleryGroupContainer` | Group container template |

### Visual State Brushes

| Brush | Description |
|-------|-------------|
| `Fluent.Ribbon.Brushes.GalleryItem.Selected` | Selected item background |
| `Fluent.Ribbon.Brushes.GalleryItem.MouseOver` | Hover background |
| `Fluent.Ribbon.Brushes.GalleryItem.Pressed` | Pressed background |
| `Fluent.Ribbon.Brushes.GalleryGroupContainer.Header.Background` | Group header background |

### Custom GalleryItem Style

```xml
<Style x:Key="CustomGalleryItemStyle" TargetType="{x:Type fluent:GalleryItem}"
       BasedOn="{StaticResource Fluent.Ribbon.Styles.GalleryItem}">
    <Setter Property="Padding" Value="4" />
    <Setter Property="Background" Value="Transparent" />
    <Style.Triggers>
        <Trigger Property="IsSelected" Value="True">
            <Setter Property="Background" Value="#FF0078D4" />
            <Setter Property="Foreground" Value="White" />
        </Trigger>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Background" Value="#FFE5F3FF" />
        </Trigger>
    </Style.Triggers>
</Style>

<fluent:InRibbonGallery ItemContainerStyle="{StaticResource CustomGalleryItemStyle}" />
```

---

## DO NOT DO

### Common Mistakes to Avoid

1. **DO NOT mix GroupBy with manual GalleryItem.Group assignments**
   - Pick one method: either GroupBy property OR manual Group property on items

2. **DO NOT set MaxItemsInRow to 0 expecting unlimited**
   - Zero means "use default" which may not be unlimited
   - Set a specific large value if you want many items

3. **DO NOT forget to set ItemWidth and ItemHeight for consistent layouts**
   - Without these, items size to content which can cause uneven grids

4. **DO NOT use Selectable="True" when items should only execute commands**
   - Selection state persists and can confuse users
   - Use Selectable="False" for command-only galleries

5. **DO NOT bind SelectedItem without TwoWay mode**
   ```xml
   <!-- WRONG -->
   <fluent:InRibbonGallery SelectedItem="{Binding Selected}" />

   <!-- CORRECT -->
   <fluent:InRibbonGallery SelectedItem="{Binding Selected, Mode=TwoWay}" />
   ```

6. **DO NOT forget filter Groups must match GroupBy values exactly**
   - Groups="Group1,Group2" must match actual group names from items
   - Comparison is case-sensitive

7. **DO NOT set CanCollapseToButton="False" with very large MinItemsInRow**
   - Gallery will not scale down and may overflow the ribbon

8. **DO NOT use IsDefinitive="True" on items when you want the popup to stay open**
   ```xml
   <!-- Keeps popup open after click -->
   <fluent:GalleryItem IsDefinitive="False">Item</fluent:GalleryItem>
   ```

9. **DO NOT expect grouping to work without IsGrouped="True" on Gallery**
   - InRibbonGallery shows groups in popup by default
   - Gallery requires explicit IsGrouped="True"

10. **DO NOT use Gallery directly in RibbonGroupBox**
    - Gallery is designed for popups (SplitButton, DropDownButton)
    - Use InRibbonGallery for inline ribbon display

---

## Complete Example

```xml
<fluent:RibbonGroupBox Header="Styles">
    <fluent:InRibbonGallery x:Name="stylesGallery"
                            Header="Quick Styles"
                            Icon="styles-16.png"
                            LargeIcon="styles-32.png"

                            GroupBy="Category"
                            ItemsSource="{Binding Styles}"
                            SelectedItem="{Binding SelectedStyle, Mode=TwoWay}"

                            ItemHeight="56"
                            ItemWidth="72"
                            MinItemsInRow="3"
                            MaxItemsInRow="6"
                            MinItemsInDropDownRow="4"
                            MaxItemsInDropDownRow="8"

                            CanCollapseToButton="True"
                            ResizeMode="Both"
                            KeyTip="S">

        <fluent:InRibbonGallery.Filters>
            <fluent:GalleryGroupFilter x:Name="AllStylesFilter"
                                       Title="All Styles"
                                       Groups="Heading,Paragraph,Character" />
            <fluent:GalleryGroupFilter Title="Headings" Groups="Heading" />
            <fluent:GalleryGroupFilter Title="Paragraphs" Groups="Paragraph" />
            <fluent:GalleryGroupFilter Title="Characters" Groups="Character" />
        </fluent:InRibbonGallery.Filters>

        <fluent:InRibbonGallery.ItemTemplate>
            <DataTemplate DataType="{x:Type local:StyleItem}">
                <Border BorderBrush="LightGray" BorderThickness="1" ToolTip="{Binding Name}">
                    <Grid>
                        <Image Source="{Binding PreviewImage}" Stretch="Uniform" />
                        <TextBlock Text="{Binding Name}"
                                   VerticalAlignment="Bottom"
                                   Background="#80000000"
                                   Foreground="White"
                                   Padding="2"
                                   FontSize="9" />
                    </Grid>
                </Border>
            </DataTemplate>
        </fluent:InRibbonGallery.ItemTemplate>

        <fluent:InRibbonGallery.ItemContainerStyle>
            <Style TargetType="{x:Type fluent:GalleryItem}">
                <Setter Property="PreviewCommand" Value="{Binding DataContext.PreviewStyleCommand,
                        RelativeSource={RelativeSource AncestorType={x:Type fluent:InRibbonGallery}}}" />
                <Setter Property="CancelPreviewCommand" Value="{Binding DataContext.CancelPreviewCommand,
                        RelativeSource={RelativeSource AncestorType={x:Type fluent:InRibbonGallery}}}" />
            </Style>
        </fluent:InRibbonGallery.ItemContainerStyle>

        <fluent:InRibbonGallery.Menu>
            <fluent:RibbonMenu>
                <fluent:MenuItem Header="Create New Style..."
                                 Command="{Binding CreateStyleCommand}" />
                <fluent:MenuItem Header="Apply Style Set..." />
                <Separator />
                <fluent:MenuItem Header="Style Manager..." />
            </fluent:RibbonMenu>
        </fluent:InRibbonGallery.Menu>
    </fluent:InRibbonGallery>
</fluent:RibbonGroupBox>
```

---

## Related Resources

- `fluent-ribbon-controls-reference-v2.md` - Full control reference
- `fluent-ribbon-qat-reference-v2.md` - Quick Access Toolbar (InRibbonGallery supports QAT)
- `fluent-ribbon-layout-reference-v2.md` - Ribbon sizing and scaling
