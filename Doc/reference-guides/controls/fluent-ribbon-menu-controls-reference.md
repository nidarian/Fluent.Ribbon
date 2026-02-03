---
title: Fluent.Ribbon Menu Controls Reference
description: Comprehensive guide for MenuItem, RibbonMenu, ContextMenu, and GroupSeparatorMenuItem controls
tags: [menuitem, menu, contextmenu, controls]
see_also:
  - fluent-ribbon-applicationmenu-reference.md
  - fluent-ribbon-button-controls-reference.md
---

# Fluent.Ribbon Menu Controls Reference

**Comprehensive guide for MenuItem, RibbonMenu, ContextMenu, and GroupSeparatorMenuItem controls.**

---

## Overview

Fluent.Ribbon provides a complete menu system that integrates seamlessly with the Office-style ribbon interface. These controls extend standard WPF menu controls with ribbon-specific features including:

- Office-style visual appearance
- KeyTip (keyboard navigation) support
- Quick Access Toolbar integration
- Resizable dropdown menus
- Split menu item behavior
- Description support for application menu items
- Consistent theming with the ribbon

---

## Control Hierarchy

```
Menu Controls
├── fluent:MenuItem              Ribbon-styled menu item
│   ├── Inherits from System.Windows.Controls.MenuItem
│   ├── Implements IQuickAccessItemProvider
│   ├── Implements IRibbonControl
│   ├── Implements IDropDownControl
│   └── Implements IToggleButton
├── fluent:RibbonMenu            Container for dropdown menus
│   └── Inherits from System.Windows.Controls.Primitives.MenuBase
├── fluent:ContextMenu           Right-click menu with ribbon styling
│   └── Inherits from System.Windows.Controls.ContextMenu
└── fluent:GroupSeparatorMenuItem  Section header in menus
    └── Inherits from fluent:MenuItem
```

---

## fluent:MenuItem

The primary menu item control for all Fluent.Ribbon menus. Extends the standard WPF MenuItem with ribbon-specific features.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | object | null | Menu item text |
| `Icon` | object | null | Small icon (16x16 recommended) |
| `InputGestureText` | string | null | Keyboard shortcut display (e.g., "Ctrl+S") |
| `Description` | string | null | Extended description for application menu items |
| `Command` | ICommand | null | Click command |
| `CommandParameter` | object | null | Command parameter |
| `IsCheckable` | bool | false | Allow checked state |
| `IsChecked` | bool | false | Current checked state |
| `GroupName` | string | null | Radio group for mutually exclusive items |
| `KeyTip` | string | null | Keyboard navigation character |
| `IsSplit` | bool | false | Split button behavior (click + dropdown) |
| `IsDefinitive` | bool | true | Closes backstage/dropdown when clicked |
| `ResizeMode` | ContextMenuResizeMode | None | Submenu resize mode |
| `MaxDropDownHeight` | double | NaN | Maximum submenu height |
| `Size` | RibbonControlSize | Large | Control size |
| `SizeDefinition` | RibbonControlSizeDefinition | null | Size definition for group states |
| `CanAddToQuickAccessToolBar` | bool | true | Allow QAT addition |
| `RecognizesAccessKey` | bool | true | Process access key underscores |

### Events

| Event | Description |
|-------|-------------|
| `DropDownOpened` | Submenu opened |
| `DropDownClosed` | Submenu closed |
| `Click` | Menu item clicked (inherited) |
| `Checked` | Item became checked (inherited) |
| `Unchecked` | Item became unchecked (inherited) |

### Template Parts

| Part Name | Type | Purpose |
|-----------|------|---------|
| `PART_Popup` | Popup | Submenu popup |
| `PART_ScrollViewer` | ScrollViewer | Submenu scrolling |
| `PART_MenuPanel` | Panel | Submenu items container |
| `PART_ButtonBorder` | Border | Split button click area |

### Basic Usage

```xml
<!-- Simple menu item -->
<fluent:MenuItem Header="Open"
                 Icon="{StaticResource OpenIcon}"
                 Command="{Binding OpenCommand}"
                 InputGestureText="Ctrl+O"
                 KeyTip="O" />

<!-- Checkable menu item -->
<fluent:MenuItem Header="Show Toolbar"
                 IsCheckable="True"
                 IsChecked="{Binding ShowToolbar}" />
```

### Menu Item with Submenu

```xml
<fluent:MenuItem Header="Recent Files" Icon="{StaticResource RecentIcon}">
    <fluent:MenuItem Header="Document1.txt" Command="{Binding OpenRecentCommand}"
                     CommandParameter="Document1.txt" />
    <fluent:MenuItem Header="Document2.txt" Command="{Binding OpenRecentCommand}"
                     CommandParameter="Document2.txt" />
    <Separator />
    <fluent:MenuItem Header="Clear Recent" Command="{Binding ClearRecentCommand}" />
</fluent:MenuItem>
```

### Split Menu Item

When a MenuItem has both a `Command` and child items, it automatically becomes a split menu item. The left portion executes the command; the right portion opens the submenu.

```xml
<!-- Auto-split: has Command AND child items -->
<fluent:MenuItem Header="Save"
                 Icon="{StaticResource SaveIcon}"
                 Command="{Binding SaveCommand}">
    <fluent:MenuItem Header="Save As..." Command="{Binding SaveAsCommand}" />
    <fluent:MenuItem Header="Save All" Command="{Binding SaveAllCommand}" />
</fluent:MenuItem>

<!-- Explicit split control -->
<fluent:MenuItem Header="Save"
                 IsSplit="True"
                 Command="{Binding SaveCommand}">
    <!-- submenu items -->
</fluent:MenuItem>

<!-- Prevent split (dropdown only, no direct action) -->
<fluent:MenuItem Header="Export"
                 IsSplit="False">
    <!-- submenu items -->
</fluent:MenuItem>
```

### Menu Item with Description

For application menu items, use the `Description` property to show extended text below the header:

```xml
<fluent:MenuItem Header="Print"
                 Icon="{StaticResource PrintIcon}"
                 Description="Send the document to a printer."
                 Command="{Binding PrintCommand}" />
```

**Note:** The Description property automatically switches to the `Fluent.Ribbon.Templates.MenuItemWithDescription` template.

### Radio Button Group

Use `GroupName` for mutually exclusive menu items:

```xml
<fluent:MenuItem Header="View Mode">
    <fluent:MenuItem Header="Normal"
                     IsCheckable="True"
                     GroupName="ViewMode"
                     IsChecked="{Binding IsNormalView}" />
    <fluent:MenuItem Header="Page Layout"
                     IsCheckable="True"
                     GroupName="ViewMode"
                     IsChecked="{Binding IsPageLayoutView}" />
    <fluent:MenuItem Header="Outline"
                     IsCheckable="True"
                     GroupName="ViewMode"
                     IsChecked="{Binding IsOutlineView}" />
</fluent:MenuItem>
```

### Resizable Submenu

```xml
<fluent:MenuItem Header="Recent"
                 ResizeMode="Both"
                 MaxDropDownHeight="400">
    <!-- many items -->
</fluent:MenuItem>
```

---

## fluent:RibbonMenu

A lightweight menu container designed for use inside dropdown buttons, galleries, and combo boxes. It provides the vertical separator line and proper keyboard navigation.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | ItemCollection | - | Menu items (inherited) |
| `Focusable` | bool | false | Not focusable by default |

### Usage

```xml
<fluent:DropDownButton Header="Options">
    <fluent:RibbonMenu>
        <fluent:MenuItem Header="Option 1" />
        <fluent:MenuItem Header="Option 2" />
        <Separator />
        <fluent:MenuItem Header="More Options..." />
    </fluent:RibbonMenu>
</fluent:DropDownButton>
```

### Inside Gallery

```xml
<fluent:InRibbonGallery Header="Styles" ItemsSource="{Binding Styles}">
    <fluent:InRibbonGallery.Menu>
        <fluent:RibbonMenu>
            <fluent:MenuItem Header="Create New Style..." />
            <fluent:MenuItem Header="Clear Formatting" />
            <fluent:MenuItem Header="Apply Styles..." />
        </fluent:RibbonMenu>
    </fluent:InRibbonGallery.Menu>
</fluent:InRibbonGallery>
```

### Template Structure

The RibbonMenu template includes:
- A vertical separator line (24px from left edge)
- A StackPanel for items with vertical orientation
- DirectionalNavigation="Continue" for keyboard support

```xml
<!-- Template structure (simplified) -->
<Grid>
    <Rectangle Width="1" Margin="24 0 0 0" HorizontalAlignment="Left"
               Stroke="{DynamicResource Fluent.Ribbon.Brushes.Separator.Border}" />
    <StackPanel IsItemsHost="True" Orientation="Vertical" />
</Grid>
```

---

## fluent:ContextMenu

A right-click context menu with ribbon styling. Extends the standard WPF ContextMenu with optional resize capability.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ResizeMode` | ContextMenuResizeMode | None | Resize behavior |
| `Items` | ItemCollection | - | Menu items (inherited) |

### ContextMenuResizeMode Enum

| Value | Description |
|-------|-------------|
| `None` | Cannot be resized (default) |
| `Vertical` | Can resize height only |
| `Both` | Can resize width and height |

### Template Parts

| Part Name | Type | Purpose |
|-----------|------|---------|
| `PART_ResizeVerticalThumb` | Thumb | Vertical resize handle |
| `PART_ResizeBothThumb` | Thumb | Both-direction resize handle |

### Basic Usage

```xml
<fluent:Button Header="Document">
    <fluent:Button.ContextMenu>
        <fluent:ContextMenu>
            <fluent:MenuItem Header="Cut" InputGestureText="Ctrl+X" />
            <fluent:MenuItem Header="Copy" InputGestureText="Ctrl+C" />
            <fluent:MenuItem Header="Paste" InputGestureText="Ctrl+V" />
            <Separator />
            <fluent:MenuItem Header="Delete" />
        </fluent:ContextMenu>
    </fluent:Button.ContextMenu>
</fluent:Button>
```

### Resizable Context Menu

```xml
<fluent:ContextMenu ResizeMode="Both">
    <fluent:GroupSeparatorMenuItem Header="Recent Files" />
    <fluent:MenuItem Header="Document1.txt" />
    <fluent:MenuItem Header="Document2.txt" />
    <fluent:MenuItem Header="Document3.txt" />
    <!-- ... more items ... -->
</fluent:ContextMenu>
```

### Global Context Menu Style

Apply ribbon styling to all WPF ContextMenus:

```xml
<Application.Resources>
    <Style TargetType="ContextMenu"
           BasedOn="{StaticResource Fluent.Ribbon.Styles.FluentRibbonDefaultContextMenu}" />
</Application.Resources>
```

---

## fluent:GroupSeparatorMenuItem

A non-interactive header that groups menu items into sections. Displays bold text on a highlighted background.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | object | null | Section header text |
| `IsEnabled` | bool | false | Always false (coerced) |
| `IsTabStop` | bool | false | Always false (coerced) |

### Usage

```xml
<fluent:ContextMenu>
    <fluent:GroupSeparatorMenuItem Header="File Operations" />
    <fluent:MenuItem Header="New" />
    <fluent:MenuItem Header="Open" />
    <fluent:MenuItem Header="Save" />

    <fluent:GroupSeparatorMenuItem Header="Edit Operations" />
    <fluent:MenuItem Header="Undo" />
    <fluent:MenuItem Header="Redo" />
    <fluent:MenuItem Header="Cut" />
    <fluent:MenuItem Header="Copy" />
    <fluent:MenuItem Header="Paste" />
</fluent:ContextMenu>
```

### Template Structure

```xml
<!-- Template (simplified) -->
<Grid Background="{DynamicResource Fluent.Ribbon.Brushes.GroupSeparator.Background}">
    <TextBlock Margin="7 0"
               FontWeight="Bold"
               Text="{Binding Header, RelativeSource={RelativeSource TemplatedParent}}" />
</Grid>
```

---

## Nested Menus (Deep Submenus)

Fluent.Ribbon supports multiple levels of nested menus with proper keyboard navigation.

### Example: Three-Level Menu

```xml
<fluent:MenuItem Header="Insert">
    <fluent:MenuItem Header="Picture">
        <fluent:MenuItem Header="From File..." Command="{Binding InsertPictureFromFile}" />
        <fluent:MenuItem Header="From Camera" Command="{Binding InsertPictureFromCamera}" />
        <fluent:MenuItem Header="Online Pictures...">
            <fluent:MenuItem Header="Bing Image Search" />
            <fluent:MenuItem Header="OneDrive" />
            <fluent:MenuItem Header="Flickr" />
        </fluent:MenuItem>
    </fluent:MenuItem>
    <fluent:MenuItem Header="Shapes">
        <fluent:GroupSeparatorMenuItem Header="Basic Shapes" />
        <fluent:MenuItem Header="Rectangle" />
        <fluent:MenuItem Header="Circle" />
        <fluent:MenuItem Header="Triangle" />
        <fluent:GroupSeparatorMenuItem Header="Lines" />
        <fluent:MenuItem Header="Line" />
        <fluent:MenuItem Header="Arrow" />
    </fluent:MenuItem>
</fluent:MenuItem>
```

### Keyboard Navigation in Nested Menus

| Key | Action |
|-----|--------|
| `Right Arrow` | Open submenu |
| `Left Arrow` | Close current submenu, go to parent |
| `Up/Down Arrow` | Navigate items |
| `Enter` | Activate item or open submenu |
| `Escape` | Close current level |

---

## Commands and Click Handling

### Command Binding

```xml
<fluent:MenuItem Header="Save"
                 Command="{Binding SaveCommand}"
                 CommandParameter="{Binding CurrentDocument}" />
```

### CanExecute Integration

The menu item automatically disables when `CanExecute` returns false:

```csharp
public ICommand SaveCommand => new RelayCommand(
    execute: () => Save(),
    canExecute: () => HasUnsavedChanges
);
```

### Click Event (Code-Behind)

```xml
<fluent:MenuItem Header="About" Click="AboutMenuItem_Click" />
```

```csharp
private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
{
    var aboutWindow = new AboutWindow();
    aboutWindow.ShowDialog();
}
```

### IsDefinitive Behavior

By default, clicking a MenuItem closes all parent popups (backstage, dropdown, etc.). Set `IsDefinitive="False"` to keep menus open:

```xml
<!-- Clicking this won't close the dropdown -->
<fluent:MenuItem Header="Toggle Option"
                 IsCheckable="True"
                 IsDefinitive="False" />
```

---

## Styling Differences from Standard WPF Menus

### Visual Differences

| Feature | Standard WPF | Fluent.Ribbon |
|---------|--------------|---------------|
| Icon column | Fixed width | Configurable via `Fluent.Ribbon.Values.MenuItem.IconColumnWidth` |
| Checkmark | System checkmark | Custom checkmark image |
| Hover effect | System highlight | Office-style highlight with border |
| Submenu arrow | System arrow | Custom arrow path |
| Separator | Simple line | Dashed line with left margin |
| Group header | Not built-in | `GroupSeparatorMenuItem` with bold text |

### Key Brushes

| Brush | Purpose |
|-------|---------|
| `Fluent.Ribbon.Brushes.MenuItem.Background` | Item background |
| `Fluent.Ribbon.Brushes.Button.MouseOver.Background` | Hover background |
| `Fluent.Ribbon.Brushes.Button.MouseOver.Border` | Hover border |
| `Fluent.Ribbon.Brushes.MenuItem.SubMenu.Arrow.Fill` | Submenu arrow color |
| `Fluent.Ribbon.Brushes.ToggleButton.Checked.Background` | Checked icon background |
| `Fluent.Ribbon.Brushes.ToggleButton.Checked.Border` | Checked icon border |
| `Fluent.Ribbon.Brushes.GroupSeparator.Background` | Group separator background |
| `Fluent.Ribbon.Brushes.Separator.Border` | Separator line color |

### Template Selection Logic

The MenuItem style automatically selects the appropriate template:

1. **Has Description?** Use `Fluent.Ribbon.Templates.MenuItemWithDescription`
2. **Has Items + IsSplit?** Use `Fluent.Ribbon.Templates.SplitMenuItem`
3. **Has Items (no split)?** Use `Fluent.Ribbon.Templates.HeaderMenuItem`
4. **Leaf item?** Use `Fluent.Ribbon.Templates.MenuItem`

### Available Templates

| Template Key | Usage |
|--------------|-------|
| `Fluent.Ribbon.Templates.MenuItem` | Standard leaf menu item |
| `Fluent.Ribbon.Templates.HeaderMenuItem` | Menu item with submenu |
| `Fluent.Ribbon.Templates.SplitMenuItem` | Split button menu item |
| `Fluent.Ribbon.Templates.MenuItemWithDescription` | Application menu item with description |

---

## Quick Access Toolbar Integration

MenuItem implements `IQuickAccessItemProvider`, allowing items to be added to the QAT.

### Automatic QAT Item Creation

When a MenuItem is added to the QAT, it creates the appropriate control:

| MenuItem Type | QAT Control |
|---------------|-------------|
| Leaf item (no children) | `Button` |
| Leaf + IsCheckable | `ToggleButton` |
| Has children + IsSplit | `SplitButton` |
| Has children (no split) | `DropDownButton` |

### Prevent QAT Addition

```xml
<fluent:MenuItem Header="Exit"
                 Command="{Binding ExitCommand}"
                 CanAddToQuickAccessToolBar="False" />
```

---

## DO NOT DO

### Do NOT use standard WPF MenuItem inside Fluent menus

```xml
<!-- WRONG: Standard WPF MenuItem won't have ribbon styling -->
<fluent:DropDownButton Header="Options">
    <MenuItem Header="Option 1" />  <!-- Wrong type -->
</fluent:DropDownButton>

<!-- CORRECT: Use fluent:MenuItem -->
<fluent:DropDownButton Header="Options">
    <fluent:MenuItem Header="Option 1" />
</fluent:DropDownButton>
```

### Do NOT mix menu container types incorrectly

```xml
<!-- WRONG: ContextMenu inside DropDownButton items -->
<fluent:DropDownButton Header="File">
    <fluent:ContextMenu>  <!-- Wrong container -->
        <fluent:MenuItem Header="New" />
    </fluent:ContextMenu>
</fluent:DropDownButton>

<!-- CORRECT: Items directly or use RibbonMenu -->
<fluent:DropDownButton Header="File">
    <fluent:MenuItem Header="New" />
</fluent:DropDownButton>

<!-- OR with RibbonMenu for complex scenarios -->
<fluent:DropDownButton Header="File">
    <fluent:RibbonMenu>
        <fluent:MenuItem Header="New" />
    </fluent:RibbonMenu>
</fluent:DropDownButton>
```

### Do NOT forget InputGestureText is display-only

```xml
<!-- WRONG: Thinking InputGestureText creates a shortcut -->
<fluent:MenuItem Header="Save"
                 InputGestureText="Ctrl+S" />  <!-- Just displays text! -->

<!-- CORRECT: Use InputBindings for actual shortcuts -->
<Window.InputBindings>
    <KeyBinding Key="S" Modifiers="Control" Command="{Binding SaveCommand}" />
</Window.InputBindings>

<fluent:MenuItem Header="Save"
                 Command="{Binding SaveCommand}"
                 InputGestureText="Ctrl+S" />  <!-- Displays the shortcut -->
```

### Do NOT use GroupSeparatorMenuItem as a clickable item

```xml
<!-- WRONG: GroupSeparatorMenuItem is always disabled -->
<fluent:GroupSeparatorMenuItem Header="Actions" Command="{Binding SomeCommand}" />

<!-- CORRECT: Use it only as a visual grouping header -->
<fluent:GroupSeparatorMenuItem Header="Actions" />
<fluent:MenuItem Header="Run Action" Command="{Binding SomeCommand}" />
```

### Do NOT expect Description to work everywhere

```xml
<!-- WRONG: Description only works in certain contexts (ApplicationMenu) -->
<fluent:DropDownButton Header="File">
    <fluent:MenuItem Header="Save" Description="Will be ignored" />
</fluent:DropDownButton>

<!-- CORRECT: Description works in ApplicationMenu -->
<fluent:ApplicationMenu Header="File">
    <fluent:MenuItem Header="Save"
                     Description="Save the current document"
                     Icon="{StaticResource SaveIcon}" />
</fluent:ApplicationMenu>
```

### Do NOT assume RecognizesAccessKey is always true

```xml
<!-- If you want literal underscores in headers -->
<fluent:MenuItem Header="file_name.txt"
                 fluent:MenuItem.RecognizesAccessKey="False" />
```

---

## Source File References

### C# Source Files

| File | Description |
|------|-------------|
| `Fluent.Ribbon/Controls/MenuItem.cs` | Main MenuItem implementation (~770 lines) |
| `Fluent.Ribbon/Controls/RibbonMenu.cs` | RibbonMenu container (~38 lines) |
| `Fluent.Ribbon/Controls/ContextMenu.cs` | ContextMenu with resize (~150 lines) |
| `Fluent.Ribbon/Controls/GroupSeparatorMenuItem.cs` | Group separator (~26 lines) |

### XAML Theme Files

| File | Description |
|------|-------------|
| `Fluent.Ribbon/Themes/Controls/MenuItem.xaml` | MenuItem templates and styles |
| `Fluent.Ribbon/Themes/Controls/Menu.xaml` | ContextMenu templates and styles |
| `Fluent.Ribbon/Themes/Controls/RibbonMenu.xaml` | RibbonMenu template |
| `Fluent.Ribbon/Themes/Controls/MenuSeparator.xaml` | Separator and GroupSeparator styles |

### Key Style Resources

| Resource Key | Target Type |
|--------------|-------------|
| `Fluent.Ribbon.Styles.MenuItem` | fluent:MenuItem |
| `Fluent.Ribbon.Styles.ContextMenu` | fluent:ContextMenu |
| `Fluent.Ribbon.Styles.FluentRibbonDefaultContextMenu` | System ContextMenu |
| `Fluent.Ribbon.Styles.RibbonMenu` | fluent:RibbonMenu |
| `Fluent.Ribbon.Styles.MenuSeparator` | Separator |
| `Fluent.Ribbon.Styles.MenuGroupSeparator` | fluent:GroupSeparatorMenuItem |

---

## Complete Example

```xml
<fluent:Ribbon>
    <fluent:Ribbon.Menu>
        <fluent:ApplicationMenu Header="File">
            <!-- Simple items with descriptions -->
            <fluent:MenuItem Header="New"
                             Icon="{StaticResource NewIcon}"
                             Description="Create a new document"
                             Command="{Binding NewCommand}"
                             KeyTip="N" />

            <fluent:MenuItem Header="Open"
                             Icon="{StaticResource OpenIcon}"
                             Description="Open an existing document"
                             Command="{Binding OpenCommand}"
                             InputGestureText="Ctrl+O"
                             KeyTip="O" />

            <!-- Split save button -->
            <fluent:MenuItem Header="Save"
                             Icon="{StaticResource SaveIcon}"
                             Description="Save the current document"
                             Command="{Binding SaveCommand}"
                             InputGestureText="Ctrl+S"
                             KeyTip="S">
                <fluent:MenuItem Header="Save As..."
                                 Icon="{StaticResource SaveAsIcon}"
                                 Command="{Binding SaveAsCommand}" />
                <fluent:MenuItem Header="Save All"
                                 Command="{Binding SaveAllCommand}" />
            </fluent:MenuItem>

            <Separator />

            <!-- Submenu with groups -->
            <fluent:MenuItem Header="Export"
                             Icon="{StaticResource ExportIcon}"
                             KeyTip="E">
                <fluent:GroupSeparatorMenuItem Header="Document Formats" />
                <fluent:MenuItem Header="PDF" Command="{Binding ExportPdfCommand}" />
                <fluent:MenuItem Header="Word" Command="{Binding ExportWordCommand}" />
                <fluent:MenuItem Header="HTML" Command="{Binding ExportHtmlCommand}" />

                <fluent:GroupSeparatorMenuItem Header="Image Formats" />
                <fluent:MenuItem Header="PNG" Command="{Binding ExportPngCommand}" />
                <fluent:MenuItem Header="JPEG" Command="{Binding ExportJpegCommand}" />
            </fluent:MenuItem>

            <!-- Recent files with resize -->
            <fluent:MenuItem Header="Recent Documents"
                             Icon="{StaticResource RecentIcon}"
                             ResizeMode="Vertical"
                             MaxDropDownHeight="300"
                             KeyTip="R">
                <fluent:MenuItem Header="Document1.txt" />
                <fluent:MenuItem Header="Document2.txt" />
                <fluent:MenuItem Header="Document3.txt" />
                <Separator />
                <fluent:MenuItem Header="Clear Recent"
                                 Command="{Binding ClearRecentCommand}" />
            </fluent:MenuItem>

            <Separator />

            <fluent:MenuItem Header="Exit"
                             Icon="{StaticResource ExitIcon}"
                             Command="{Binding ExitCommand}"
                             CanAddToQuickAccessToolBar="False"
                             KeyTip="X" />

            <fluent:ApplicationMenu.FooterPaneContent>
                <fluent:Button Header="Options"
                               Icon="{StaticResource OptionsIcon}"
                               Command="{Binding OptionsCommand}" />
            </fluent:ApplicationMenu.FooterPaneContent>
        </fluent:ApplicationMenu>
    </fluent:Ribbon.Menu>

    <fluent:RibbonTabItem Header="Home">
        <fluent:RibbonGroupBox Header="Clipboard">
            <!-- DropDownButton with RibbonMenu -->
            <fluent:DropDownButton Header="Paste"
                                   LargeIcon="{StaticResource PasteIcon32}"
                                   Size="Large">
                <fluent:RibbonMenu>
                    <fluent:MenuItem Header="Paste"
                                     Icon="{StaticResource PasteIcon}"
                                     InputGestureText="Ctrl+V" />
                    <fluent:MenuItem Header="Paste Special..."
                                     Icon="{StaticResource PasteSpecialIcon}" />
                    <Separator />
                    <fluent:MenuItem Header="Paste as Hyperlink" />
                </fluent:RibbonMenu>
            </fluent:DropDownButton>
        </fluent:RibbonGroupBox>
    </fluent:RibbonTabItem>
</fluent:Ribbon>
```

---

## Related References

- **Fluent.Ribbon Controls Reference** - Overview of all controls
- **Fluent.Ribbon Backstage Reference** - Backstage view with menu items
- **Fluent.Ribbon QAT Reference** - Quick Access Toolbar integration
- **Fluent.Ribbon KeyTips Reference** - Keyboard navigation
- **Fluent.Ribbon Galleries Reference** - Gallery controls with menus
