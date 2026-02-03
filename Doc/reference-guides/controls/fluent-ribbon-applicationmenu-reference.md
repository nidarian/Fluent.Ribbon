---
title: Fluent.Ribbon ApplicationMenu Reference
description: Complete reference for the ApplicationMenu control - the classic Office 2007-style File menu
tags: [applicationmenu, file-menu, controls]
see_also:
  - fluent-ribbon-backstage-reference.md
  - fluent-ribbon-menu-controls-reference.md
---

# Fluent.Ribbon ApplicationMenu Reference

**Complete reference for the ApplicationMenu control - the classic Office 2007-style File menu.**

---

## Overview

`ApplicationMenu` is the classic drop-down "File" menu that appeared in Microsoft Office 2007 and early Office 2010 versions. When clicked, it displays a popup with menu items on the left, optional custom content on the right, and an optional footer at the bottom.

### ApplicationMenu vs Backstage

| Feature | ApplicationMenu | Backstage |
|---------|-----------------|-----------|
| **Appearance** | Drop-down popup from File button | Full-page overlay replacing ribbon |
| **Office Version** | Office 2007 style | Office 2010+ style |
| **Content Area** | Two-panel layout (left menu + right pane) | Full tab-based content area |
| **Recommended For** | Simple file operations, legacy apps | Modern apps, complex file operations |
| **Navigation** | Single-level menu with submenus | Multi-tab navigation with content areas |

### When to Use ApplicationMenu

- Maintaining Office 2007 visual compatibility
- Simple applications with few file operations
- Legacy application modernization where minimal UI change is preferred
- When the full-page Backstage is overkill for your needs

### When to Use Backstage Instead

- Modern Office-style applications
- Complex file operations (Print Preview, Info pages, etc.)
- Applications requiring extensive settings/options screens
- When you need full-page content areas for each menu item

---

## Architecture

```
ApplicationMenu (extends DropDownButton)
    |
    +-- Items (left panel)
    |      |
    |      +-- MenuItem (with optional submenus)
    |      +-- Separator
    |
    +-- RightPaneContent (right panel)
    |      |
    |      +-- Custom UIElement (e.g., recent files list)
    |
    +-- FooterPaneContent (bottom panel)
           |
           +-- Custom UIElement (e.g., Options/Exit buttons)
```

### Visual Layout

```
+------------------------------------------+
|  [File Button]                           |
+------------------+-----------------------+
|  Left Panel      |  Right Panel          |
|  (Menu Items)    |  (RightPaneContent)   |
|                  |                       |
|  New             |  Recent Files:        |
|  Open      >     |  - Document1.docx     |
|  Save            |  - Report.xlsx        |
|  Save As   >     |  - Notes.txt          |
|  ---------------  |                       |
|  Exit            |                       |
+------------------+-----------------------+
|  Footer Panel (FooterPaneContent)        |
|  [Options]                    [Exit]     |
+------------------------------------------+
```

---

## ApplicationMenu Properties

### Own Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `RightPaneWidth` | `double` | `300` | Width of the right content pane |
| `RightPaneContent` | `object` | `null` | Content displayed in the right panel |
| `FooterPaneContent` | `object` | `null` | Content displayed in the footer panel |

### Inherited from DropDownButton

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | `object` | Default icon | Content shown on the button (usually "File" text or icon) |
| `IsDropDownOpen` | `bool` | `false` | Whether the menu popup is open |
| `MaxDropDownHeight` | `double` | `NaN` | Maximum height of the dropdown |
| `Items` | Collection | - | Menu items (MenuItem, Separator) |
| `KeyTip` | `string` | Localized | KeyTip for keyboard access |
| `Icon` | `object` | `null` | Icon for the button |

### Non-Supported Properties

The following inherited properties are explicitly disabled:

| Property | Reason |
|----------|--------|
| `CanAddToQuickAccessToolBar` | Always `false` - ApplicationMenu cannot be added to QAT |

---

## Events

### Inherited from DropDownButton

| Event | Description |
|-------|-------------|
| `DropDownOpened` | Fired when the menu popup opens |
| `DropDownClosed` | Fired when the menu popup closes |

---

## Basic Usage

### Minimal Example

```xml
<Fluent:Ribbon>
    <Fluent:Ribbon.Menu>
        <Fluent:ApplicationMenu>
            <Fluent:MenuItem Header="New" Icon="{StaticResource NewIcon}" />
            <Fluent:MenuItem Header="Open" Icon="{StaticResource OpenIcon}" />
            <Fluent:MenuItem Header="Save" Icon="{StaticResource SaveIcon}" />
            <Separator />
            <Fluent:MenuItem Header="Exit" Icon="{StaticResource ExitIcon}" />
        </Fluent:ApplicationMenu>
    </Fluent:Ribbon.Menu>
    <!-- Ribbon tabs -->
</Fluent:Ribbon>
```

### With Right Pane Content

```xml
<Fluent:ApplicationMenu>
    <Fluent:ApplicationMenu.RightPaneContent>
        <StackPanel>
            <TextBlock Text="Recent Files"
                       FontWeight="SemiBold"
                       Margin="10" />
            <ListBox ItemsSource="{Binding RecentFiles}"
                     BorderThickness="0">
                <ListBox.ItemTemplate>
                    <DataTemplate>
                        <TextBlock Text="{Binding Name}" />
                    </DataTemplate>
                </ListBox.ItemTemplate>
            </ListBox>
        </StackPanel>
    </Fluent:ApplicationMenu.RightPaneContent>

    <Fluent:MenuItem Header="New" />
    <Fluent:MenuItem Header="Open" />
    <Fluent:MenuItem Header="Save" />
</Fluent:ApplicationMenu>
```

### With Footer Pane Content

```xml
<Fluent:ApplicationMenu>
    <Fluent:ApplicationMenu.FooterPaneContent>
        <StackPanel Orientation="Horizontal"
                    HorizontalAlignment="Right"
                    Margin="10 5">
            <Button Content="Options"
                    Margin="0 0 10 0"
                    Command="{Binding OptionsCommand}" />
            <Button Content="Exit Application"
                    Command="{Binding ExitCommand}" />
        </StackPanel>
    </Fluent:ApplicationMenu.FooterPaneContent>

    <Fluent:MenuItem Header="New" />
    <Fluent:MenuItem Header="Open" />
    <Fluent:MenuItem Header="Save" />
</Fluent:ApplicationMenu>
```

---

## Menu Structure

### Adding Menu Items

Menu items use `Fluent:MenuItem` which provides special styling within the ApplicationMenu context.

```xml
<Fluent:ApplicationMenu>
    <!-- Simple menu item -->
    <Fluent:MenuItem Header="New"
                     Icon="pack://application:,,,/Images/New.png"
                     Command="{Binding NewCommand}"
                     KeyTip="N" />

    <!-- Menu item with description (shown to the right) -->
    <Fluent:MenuItem Header="Open"
                     Icon="pack://application:,,,/Images/Open.png"
                     Description="CTRL + O"
                     ToolTip="Open an existing document" />

    <!-- Separator -->
    <Separator />

    <!-- Menu item with submenu -->
    <Fluent:MenuItem Header="Save As"
                     Icon="pack://application:,,,/Images/SaveAs.png">
        <Fluent:MenuItem Header="Word Document"
                         Description="Save as .docx format" />
        <Fluent:MenuItem Header="PDF"
                         Description="Export as PDF" />
    </Fluent:MenuItem>
</Fluent:ApplicationMenu>
```

### Split Menu Items

Use `IsSplit="True"` for menu items that have both a default action and a submenu:

```xml
<Fluent:MenuItem Header="Save As"
                 Icon="pack://application:,,,/Images/SaveAs.png"
                 IsSplit="True"
                 Command="{Binding SaveAsDefaultCommand}">
    <!-- Clicking the main area executes SaveAsDefaultCommand -->
    <!-- Clicking the arrow shows submenu -->
    <Fluent:MenuItem Header="Standard format"
                     Description="Save in standard format"
                     Command="{Binding SaveAsStandardCommand}" />
    <Fluent:MenuItem Header="Export"
                     Description="Export to other format"
                     Command="{Binding ExportCommand}" />
</Fluent:MenuItem>
```

### MenuItem Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | `object` | Menu item text |
| `Icon` | `object` | Large icon (32x32 recommended) |
| `Description` | `string` | Secondary text shown on right side |
| `IsSplit` | `bool` | Whether item has split button behavior |
| `Command` | `ICommand` | Command to execute on click |
| `KeyTip` | `string` | KeyTip for keyboard access |
| `IsChecked` | `bool` | Whether item shows check mark |

---

## RightPaneContent Customization

The right pane typically displays contextual information like recent files. Width is controlled by `RightPaneWidth`.

### Recent Files Pattern

```xml
<Fluent:ApplicationMenu RightPaneWidth="350">
    <Fluent:ApplicationMenu.RightPaneContent>
        <StackPanel HorizontalAlignment="Stretch"
                    VerticalAlignment="Stretch"
                    Orientation="Vertical">

            <!-- Header -->
            <Border Background="#F6F7F8"
                    BorderBrush="#DCDDDE"
                    BorderThickness="0 0 0 1">
                <TextBlock Padding="12 4"
                           Foreground="#64647F"
                           FontSize="12"
                           FontWeight="SemiBold"
                           Text="Recent Files" />
            </Border>

            <!-- File list -->
            <ItemsControl ItemsSource="{Binding RecentFiles}"
                          Margin="8">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <TextBlock Text="{Binding}"
                                   Margin="4"
                                   TextTrimming="CharacterEllipsis"
                                   ToolTip="{Binding}" />
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </StackPanel>
    </Fluent:ApplicationMenu.RightPaneContent>

    <!-- Menu items -->
</Fluent:ApplicationMenu>
```

### Dynamic Submenu Content

When a MenuItem with submenu is hovered, its submenu replaces the right pane content. This is automatic behavior.

```xml
<Fluent:MenuItem Header="New"
                 Icon="pack://application:,,,/Images/NewLarge.png">
    <!-- These items appear in the right pane when "New" is hovered -->
    <Fluent:MenuItem Header="Text document"
                     Description="Create a blank text document"
                     Icon="pack://application:,,,/Images/DocLarge.png" />
    <Fluent:MenuItem Header="Spreadsheet"
                     Description="Create a blank spreadsheet"
                     Icon="pack://application:,,,/Images/XlsLarge.png" />
</Fluent:MenuItem>
```

---

## FooterPaneContent

The footer pane spans the full width below both the left and right panels. Commonly used for Options and Exit buttons.

```xml
<Fluent:ApplicationMenu.FooterPaneContent>
    <Grid Margin="10 5">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="Auto" />
        </Grid.ColumnDefinitions>

        <Button Grid.Column="0"
                Content="Application Options"
                HorizontalAlignment="Left"
                Command="{Binding OptionsCommand}" />

        <Button Grid.Column="1"
                Content="Exit Application"
                HorizontalAlignment="Right"
                Command="{Binding ExitCommand}" />
    </Grid>
</Fluent:ApplicationMenu.FooterPaneContent>
```

---

## Integration with Ribbon

### Placement in Ribbon.Menu

`ApplicationMenu` (or `Backstage`) goes in the `Ribbon.Menu` property:

```xml
<Fluent:Ribbon>
    <Fluent:Ribbon.Menu>
        <Fluent:ApplicationMenu>
            <!-- Menu content -->
        </Fluent:ApplicationMenu>
    </Fluent:Ribbon.Menu>

    <Fluent:RibbonTabItem Header="Home">
        <!-- Tab content -->
    </Fluent:RibbonTabItem>
</Fluent:Ribbon>
```

### Switching Between ApplicationMenu and Backstage

From the Showcase application - you can programmatically switch between menu types:

```xml
<Fluent:Ribbon.Menu>
    <Grid>
        <!-- Both controls exist, visibility controls which is shown -->
        <Fluent:Backstage x:Name="Backstage"
                          Visibility="Visible" />
        <Fluent:ApplicationMenu x:Name="ApplicationMenu"
                                Visibility="Collapsed" />
    </Grid>
</Fluent:Ribbon.Menu>
```

```csharp
// Switch to ApplicationMenu
ApplicationMenu.Visibility = Visibility.Visible;
Backstage.Visibility = Visibility.Collapsed;

// Switch to Backstage
ApplicationMenu.Visibility = Visibility.Collapsed;
Backstage.Visibility = Visibility.Visible;
```

---

## Programmatic Control

### Opening/Closing

```csharp
// Open the menu
applicationMenu.IsDropDownOpen = true;

// Close the menu
applicationMenu.IsDropDownOpen = false;

// Toggle
applicationMenu.IsDropDownOpen = !applicationMenu.IsDropDownOpen;
```

### Handling Events

```csharp
applicationMenu.DropDownOpened += (sender, e) =>
{
    // Refresh recent files list
    LoadRecentFiles();
};

applicationMenu.DropDownClosed += (sender, e) =>
{
    // Cleanup or save state
};
```

---

## Styling and Theming

### Key Style Resources

| Style Key | Target | Description |
|-----------|--------|-------------|
| `Fluent.Ribbon.Styles.ApplicationMenu` | `ApplicationMenu` | Main application menu button |
| `Fluent.Ribbon.Styles.ApplicationMenu.MenuItem` | `MenuItem` | First-level menu items |
| `Fluent.Ribbon.Styles.ApplicationMenu.MenuItemSecondLevel` | `MenuItem` | Submenu items |

### Key Template Resources

| Template Key | Target | Description |
|--------------|--------|-------------|
| `Fluent.Ribbon.Templates.ApplicationMenuButton` | `ApplicationMenu` | Button + popup template |
| `Fluent.Ribbon.Templates.ApplicationMenuItem` | `MenuItem` | First-level item template |
| `Fluent.Ribbon.Templates.HeaderApplicationMenuItem` | `MenuItem` | Item with submenu arrow |
| `Fluent.Ribbon.Templates.SplitApplicationMenuItem` | `MenuItem` | Split button item template |

### Key Brush Resources

| Brush Key | Usage |
|-----------|-------|
| `Fluent.Ribbon.Brushes.AccentBase` | Default button background |
| `Fluent.Ribbon.Brushes.IdealForeground` | Button text color |
| `Fluent.Ribbon.Brushes.DropDown.Background` | Popup background |
| `Fluent.Ribbon.Brushes.DropDown.Border` | Popup border |
| `Fluent.Ribbon.Brushes.Separator.Border` | Separator and panel divider lines |
| `Fluent.Ribbon.Brushes.Ribbon.Background` | Footer pane background |
| `Fluent.Ribbon.Brushes.Button.MouseOver.Background` | Item hover background |
| `Fluent.Ribbon.Brushes.ApplicationMenuItem.CheckBox.Background` | Checked item background |
| `Fluent.Ribbon.Brushes.ApplicationMenuItem.CheckBox.Border` | Checked item border |

### Customizing Button Appearance

```xml
<Fluent:ApplicationMenu Header="File"
                        Background="{DynamicResource Fluent.Ribbon.Brushes.AccentBase}"
                        Foreground="White">
    <!-- Menu content -->
</Fluent:ApplicationMenu>
```

### Customizing Right Pane Width

```xml
<Fluent:ApplicationMenu RightPaneWidth="400">
    <!-- Wider right panel for more content -->
</Fluent:ApplicationMenu>
```

### Default Header

The default `Header` is set via converter to show the application menu icon. To show text instead:

```xml
<Fluent:ApplicationMenu>
    <Fluent:ApplicationMenu.Header>
        <TextBlock Text="File" />
    </Fluent:ApplicationMenu.Header>
</Fluent:ApplicationMenu>
```

---

## Keyboard Navigation

| Key | Action |
|-----|--------|
| `Alt + KeyTip` | Open menu (default KeyTip from localization) |
| `Down/Up` | Navigate between menu items |
| `Right` | Open submenu |
| `Left` | Close submenu |
| `Enter/Space` | Activate menu item |
| `Escape` | Close menu or submenu |

### KeyTip Usage

```xml
<Fluent:ApplicationMenu>
    <Fluent:MenuItem Header="New" KeyTip="N" />
    <Fluent:MenuItem Header="Open" KeyTip="O" />
    <Fluent:MenuItem Header="Save" KeyTip="S" />
    <Fluent:MenuItem Header="Save As" KeyTip="A">
        <Fluent:MenuItem Header="Word" KeyTip="W" />
        <Fluent:MenuItem Header="PDF" KeyTip="P" />
    </Fluent:MenuItem>
    <Separator />
    <Fluent:MenuItem Header="Exit" KeyTip="X" />
</Fluent:ApplicationMenu>
```

---

## Complete Example

From the Showcase application (`TestContent.xaml`):

```xml
<Fluent:ApplicationMenu x:Name="ApplicationMenu"
                        AutomationProperties.Name="Application menu">
    <Fluent:ApplicationMenu.RightPaneContent>
        <StackPanel HorizontalAlignment="Stretch"
                    VerticalAlignment="Stretch"
                    Orientation="Vertical">

            <!-- Header section -->
            <Border HorizontalAlignment="Stretch"
                    BorderBrush="#64647F"
                    BorderThickness="0">
                <TextBlock Padding="12 4 4 4"
                           Background="#F6F7F8"
                           Foreground="#64647F"
                           FontSize="12"
                           FontWeight="SemiBold"
                           Text="Recent files"
                           TextAlignment="Left" />
            </Border>

            <!-- Divider lines -->
            <StackPanel Height="2" HorizontalAlignment="Stretch">
                <Border Height="1"
                        HorizontalAlignment="Stretch"
                        BorderBrush="#DCDDDE"
                        BorderThickness="1" />
                <Border Height="1"
                        HorizontalAlignment="Stretch"
                        BorderBrush="#FEFEFF"
                        BorderThickness="1" />
            </StackPanel>

            <!-- Recent files list -->
            <ItemsControl ItemsSource="{Binding RecentFiles}"
                          Margin="0">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <TextBlock Margin="8 1"
                                   Text="{Binding}"
                                   TextTrimming="CharacterEllipsis"
                                   ToolTip="{Binding Text}" />
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </StackPanel>
    </Fluent:ApplicationMenu.RightPaneContent>

    <!-- Menu with submenu -->
    <Fluent:MenuItem Header="New"
                     Icon="pack://application:,,,/Images/GreenLarge.png">
        <Fluent:MenuItem Header="Text document"
                         Icon="pack://application:,,,/Images/GreenLarge.png" />
        <Fluent:MenuItem Header="Spreadsheet"
                         Icon="pack://application:,,,/Images/GreenLarge.png" />
    </Fluent:MenuItem>

    <!-- Split menu item -->
    <Fluent:MenuItem Header="Save As"
                     Icon="pack://application:,,,/Images/BlueLarge.png"
                     IsSplit="True">
        <Fluent:MenuItem Header="Standard format"
                         Icon="pack://application:,,,/Images/BlueLarge.png"
                         Description="Lorem ipsum dolor sit amet..."
                         ToolTip="Save something in standard format" />
        <Fluent:MenuItem Header="Export"
                         Icon="pack://application:,,,/Images/BlueLarge.png"
                         ToolTip="Export something" />
    </Fluent:MenuItem>

    <!-- Simple menu item with description -->
    <Fluent:MenuItem Header="Open"
                     Icon="pack://application:,,,/Images/YellowLarge.png"
                     Description="CTRL + O"
                     ToolTip="Open object" />

    <Separator />

    <!-- Exit with command binding -->
    <Fluent:MenuItem Header="Exit"
                     Icon="pack://application:,,,/Images/RedLarge.png"
                     Command="{Binding ExitCommand}"
                     KeyTip="X" />
</Fluent:ApplicationMenu>
```

---

## DO NOT DO (Anti-Patterns)

### Do Not Mix ApplicationMenu and Backstage Simultaneously

```xml
<!-- WRONG: Both visible at same time -->
<Fluent:Ribbon.Menu>
    <Fluent:Backstage />
</Fluent:Ribbon.Menu>
<!-- ApplicationMenu would need a separate Ribbon.Menu, which is impossible -->
```

Use one or the other, or wrap both in a Grid with visibility toggles.

### Do Not Add ApplicationMenu to Quick Access Toolbar

```csharp
// This will throw NotImplementedException
var qatItem = applicationMenu.CreateQuickAccessItem(); // THROWS!
```

`CanAddToQuickAccessToolBar` is always `false` for ApplicationMenu.

### Do Not Use Context Menu on ApplicationMenu Button

```xml
<!-- WRONG: Context menu is suppressed on the button itself -->
<Fluent:ApplicationMenu>
    <Fluent:ApplicationMenu.ContextMenu>
        <ContextMenu>
            <!-- This won't appear on the button -->
        </ContextMenu>
    </Fluent:ApplicationMenu.ContextMenu>
</Fluent:ApplicationMenu>
```

The ApplicationMenu explicitly handles `OnContextMenuOpening` to suppress context menus on the button.

### Do Not Use Small Icons for First-Level Menu Items

```xml
<!-- WRONG: Small icons look bad in ApplicationMenu -->
<Fluent:MenuItem Header="New"
                 Icon="pack://application:,,,/Images/Small16x16.png" />
```

First-level items use Large icon size (32x32). Provide appropriately sized icons.

### Do Not Nest ApplicationMenu

```xml
<!-- WRONG: ApplicationMenu cannot be a child of another ApplicationMenu -->
<Fluent:ApplicationMenu>
    <Fluent:ApplicationMenu />  <!-- Invalid -->
</Fluent:ApplicationMenu>
```

### Do Not Use Standard WPF MenuItem

```xml
<!-- WRONG: Use Fluent:MenuItem, not System.Windows.Controls.MenuItem -->
<Fluent:ApplicationMenu>
    <MenuItem Header="Open" />  <!-- Wrong namespace -->
</Fluent:ApplicationMenu>
```

Always use `Fluent:MenuItem` for proper styling within ApplicationMenu.

---

## Source Files Reference

| File | Purpose |
|------|---------|
| `Fluent.Ribbon\Controls\ApplicationMenu.cs` | Main control implementation |
| `Fluent.Ribbon\Controls\DropDownButton.cs` | Base class with popup logic |
| `Fluent.Ribbon\Themes\Controls\ApplicationMenu.xaml` | Button and popup template |
| `Fluent.Ribbon\Themes\Controls\ApplicationMenuItem.xaml` | MenuItem styles and templates |
| `Fluent.Ribbon\StyleSelectors\ApplicationMenuItemContainerStyleSelector.cs` | Auto-styles menu items |
| `Fluent.Ribbon.Showcase\TestContent.xaml:334-417` | Full usage example |
| `Fluent.Ribbon.Showcase\TestContent.xaml.cs:122-147` | Menu switching logic |

---

## Summary Table

| Task | Solution |
|------|----------|
| Add to ribbon | Place in `<Fluent:Ribbon.Menu>` |
| Add menu item | `<Fluent:MenuItem Header="..." Icon="..." />` |
| Add separator | `<Separator />` |
| Add submenu | Nest `Fluent:MenuItem` elements |
| Make split button | Set `IsSplit="True"` on MenuItem |
| Add right pane content | Set `RightPaneContent` property |
| Add footer content | Set `FooterPaneContent` property |
| Change right pane width | Set `RightPaneWidth` (default 300) |
| Open menu programmatically | Set `IsDropDownOpen = true` |
| Close menu programmatically | Set `IsDropDownOpen = false` |
| Handle menu open | Subscribe to `DropDownOpened` event |
| Handle menu close | Subscribe to `DropDownClosed` event |
| Add keyboard shortcut | Set `KeyTip` on menu items |
| Show description text | Set `Description` on MenuItem |
| Switch to Backstage | Toggle `Visibility` of both controls |
