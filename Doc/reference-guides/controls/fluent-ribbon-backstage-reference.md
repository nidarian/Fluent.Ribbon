---
title: Fluent.Ribbon Backstage Reference
description: Complete reference for Backstage, BackstageTabControl, BackstageTabItem, and related controls
tags: [backstage, file-menu, controls, tabs]
see_also:
  - fluent-ribbon-applicationmenu-reference.md
  - fluent-ribbon-startscreen-reference.md
---

# Fluent.Ribbon Backstage Reference

**Complete reference for Backstage, BackstageTabControl, BackstageTabItem, and related controls.**

---

## Overview

The Backstage is the "File" menu panel in Office-style applications. When opened, it displays a full-page view replacing the ribbon and content area temporarily. It's designed for file operations (Open, Save, Print, etc.) and application settings.

### Architecture

```
Backstage (the "File" button)
    |
    +-- Content (UIElement)
           |
           +-- BackstageTabControl
                   |
                   +-- BackstageTabItem (tabs with content)
                   +-- Button (action buttons like "Save")
                   +-- SeparatorTabItem (section headers)
                   +-- Separator (divider lines)
```

### Control Summary

| Control | Purpose | Base Class |
|---------|---------|------------|
| `Backstage` | The "File" button that toggles the backstage view | `RibbonControl` |
| `BackstageTabControl` | Container for backstage tabs and buttons | `Selector` |
| `BackstageTabItem` | Tab with header and content area | `ContentControl` |
| `Button` (in backstage) | Action button (Save, Print) | `Fluent.Button` |
| `SeparatorTabItem` | Non-selectable section header | `TabItem` |
| `Separator` | Visual divider line | `System.Windows.Controls.Separator` |

---

## Backstage Control

The `Backstage` control is the "File" button shown in the ribbon. It controls opening/closing the backstage panel.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IsOpen` | `bool` | `false` | Gets or sets whether backstage is shown |
| `CanChangeIsOpen` | `bool` | `true` | Whether backstage can be opened or closed |
| `Content` | `UIElement` | `null` | The backstage content (usually a `BackstageTabControl`) |
| `Header` | `object` | Localized "File" | Text shown on the backstage button |
| `Icon` | `object` | `null` | Icon for the backstage button |
| `CloseOnEsc` | `bool` | `true` | Whether pressing Escape closes the backstage |
| `AreAnimationsEnabled` | `bool` | System setting | Whether open/close animations are enabled |
| `HideContextTabsOnOpen` | `bool` | `true` | Whether to hide contextual tabs when backstage opens |
| `UseHighestAvailableAdornerLayer` | `bool` | `true` | Use topmost adorner layer for display |

### Events

| Event | Description |
|-------|-------------|
| `IsOpenChanged` | Fired when `IsOpen` property changes |

### Basic Usage

```xml
<Fluent:Ribbon>
    <Fluent:Ribbon.Menu>
        <Fluent:Backstage>
            <Fluent:BackstageTabControl>
                <!-- Tabs and buttons here -->
            </Fluent:BackstageTabControl>
        </Fluent:Backstage>
    </Fluent:Ribbon.Menu>
    <!-- Ribbon tabs -->
</Fluent:Ribbon>
```

### Programmatic Control

```csharp
// Open backstage
backstage.IsOpen = true;

// Close backstage
backstage.IsOpen = false;

// Check if open
if (backstage.IsOpen)
{
    // ...
}

// Prevent closing (e.g., during an operation)
backstage.CanChangeIsOpen = false;
// ... do work ...
backstage.CanChangeIsOpen = true;
```

### MVVM Binding

```xml
<Fluent:Backstage IsOpen="{Binding IsBackstageOpen, Mode=TwoWay}">
    <Fluent:BackstageTabControl>
        <!-- ... -->
    </Fluent:BackstageTabControl>
</Fluent:Backstage>
```

```csharp
// ViewModel
public bool IsBackstageOpen
{
    get => _isBackstageOpen;
    set => SetProperty(ref _isBackstageOpen, value);
}
```

---

## BackstageTabControl

Container control that holds tabs, buttons, and separators. It displays a back button at the top and a list of items on the left side.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SelectedItem` | `object` | First selectable tab | Currently selected item |
| `SelectedIndex` | `int` | `0` | Index of selected item |
| `SelectedContent` | `object` | (read-only) | Content of selected tab |
| `IsBackButtonVisible` | `bool` | `true` | Whether the back button is visible |
| `BackButtonUid` | `string` | Localized | Uid for automation/accessibility |
| `ItemsPanelMinWidth` | `double` | `125` | Minimum width of the left panel |
| `ItemsPanelBackground` | `Brush` | Theme brush | Background of the left panel |
| `ContentTemplate` | `DataTemplate` | `null` | Template for content area |
| `ContentTemplateSelector` | `DataTemplateSelector` | `null` | Selector for content templates |

### Template Parts

| Part Name | Type | Purpose |
|-----------|------|---------|
| `PART_SelectedContentHost` | `ContentPresenter` | Displays selected tab content |
| `PART_ItemsPanelContainer` | `UIElement` | Container for left panel |
| `PART_BackButton` | `UIElement` | The back/close button |

### Supported Child Types

The `BackstageTabControl` automatically applies appropriate styles to these child types:

| Type | Container Style | Behavior |
|------|-----------------|----------|
| `BackstageTabItem` | `Fluent.Ribbon.Styles.BackstageTabItem` | Selectable tab with content |
| `Button` | `Fluent.Ribbon.Styles.BackstageTabControl.Button` | Click action (not selectable) |
| `SeparatorTabItem` | `Fluent.Ribbon.Styles.BackstageTabControl.SeparatorTabItem` | Section header (not selectable) |
| `Separator` | Default | Horizontal divider line |

---

## BackstageTabItem

A selectable tab that displays content when selected. Similar to a standard `TabItem` but styled for the backstage.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | `object` | `null` | Tab header text/content |
| `HeaderTemplate` | `DataTemplate` | `null` | Template for header |
| `HeaderTemplateSelector` | `DataTemplateSelector` | `null` | Selector for header template |
| `Content` | `object` | `null` | Content shown when selected |
| `ContentTemplate` | `DataTemplate` | `null` | Template for content |
| `ContentTemplateSelector` | `DataTemplateSelector` | `null` | Selector for content template |
| `ContentStringFormat` | `string` | `null` | Format string for content |
| `Icon` | `object` | `null` | Icon shown next to header |
| `IsSelected` | `bool` | `false` | Whether this tab is selected |
| `KeyTip` | `string` | `null` | KeyTip for keyboard navigation |

### Template Parts

| Part Name | Type | Purpose |
|-----------|------|---------|
| `PART_Header` | `FrameworkElement` | Header content host |

### Usage Examples

#### Basic Tab

```xml
<Fluent:BackstageTabItem Header="Home">
    <StackPanel Margin="20">
        <TextBlock Text="Welcome to the application" />
    </StackPanel>
</Fluent:BackstageTabItem>
```

#### Tab with Icon

```xml
<Fluent:BackstageTabItem Header="Recent"
                         Icon="{StaticResource RecentIcon}">
    <ListBox ItemsSource="{Binding RecentFiles}" />
</Fluent:BackstageTabItem>
```

#### Tab with KeyTip

```xml
<Fluent:BackstageTabItem Header="Info"
                         KeyTip="I">
    <StackPanel>
        <TextBlock Text="Application Information" />
    </StackPanel>
</Fluent:BackstageTabItem>
```

#### Tab with Content Template

```xml
<Fluent:BackstageTabItem Header="Settings"
                         DataContext="{Binding SettingsViewModel}">
    <Fluent:BackstageTabItem.ContentTemplate>
        <DataTemplate>
            <local:SettingsView />
        </DataTemplate>
    </Fluent:BackstageTabItem.ContentTemplate>
</Fluent:BackstageTabItem>
```

---

## Buttons in BackstageTabControl

Regular `Fluent:Button` controls placed in the `BackstageTabControl` act as action buttons - they execute commands but don't display content.

### Usage

```xml
<Fluent:BackstageTabControl>
    <Fluent:Button Header="Save"
                   Icon="{StaticResource SaveIcon}"
                   Command="{Binding SaveCommand}"
                   KeyTip="S" />

    <Fluent:Button Header="Save As"
                   Icon="{StaticResource SaveAsIcon}"
                   Command="{Binding SaveAsCommand}"
                   KeyTip="A" />
</Fluent:BackstageTabControl>
```

### IsDefinitive Property

By default, clicking a button closes the backstage. Use `IsDefinitive="False"` to keep it open:

```xml
<Fluent:Button Header="Save As"
               Command="{Binding SaveAsCommand}"
               IsDefinitive="False"
               KeyTip="A" />
```

---

## SeparatorTabItem

A non-selectable header for grouping related tabs. It displays text but cannot be selected or focused.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | `object` | Section header text |
| `HasHeader` | `bool` | Whether header is set (read-only) |

### Usage

```xml
<Fluent:BackstageTabControl>
    <!-- File operations group -->
    <Fluent:Button Header="Save" KeyTip="S" />
    <Fluent:Button Header="Save As" KeyTip="A" />

    <Fluent:SeparatorTabItem Header="Recent" />

    <Fluent:BackstageTabItem Header="Documents">
        <!-- Recent documents content -->
    </Fluent:BackstageTabItem>

    <Fluent:SeparatorTabItem Header="Settings" />

    <Fluent:BackstageTabItem Header="Options">
        <!-- Options content -->
    </Fluent:BackstageTabItem>
</Fluent:BackstageTabControl>
```

---

## Separator (Divider Line)

A standard WPF `Separator` creates a horizontal divider line.

```xml
<Fluent:BackstageTabControl>
    <Fluent:Button Header="Save" />
    <Fluent:Button Header="Save As" />

    <Separator />

    <Fluent:BackstageTabItem Header="Recent" />
</Fluent:BackstageTabControl>
```

---

## DockPanel.Dock for Item Positioning

Items in `BackstageTabControl` use `DockPanel.Dock` to control vertical positioning. Default is `Top`.

```xml
<Fluent:BackstageTabControl>
    <!-- These dock to top (default) -->
    <Fluent:Button Header="Save" />
    <Fluent:BackstageTabItem Header="Home" />

    <!-- This docks to bottom -->
    <Fluent:Button Header="Exit"
                   DockPanel.Dock="Bottom"
                   KeyTip="X" />

    <!-- Bottom-docked items appear in reverse order from bottom -->
    <Fluent:BackstageTabItem Header="Options"
                             DockPanel.Dock="Bottom" />
</Fluent:BackstageTabControl>
```

---

## Complete Example

From the Showcase application (`TestContent.xaml`):

```xml
<Fluent:Ribbon.Menu>
    <Fluent:Backstage x:Name="Backstage"
                      IsOpen="{Binding IsBackstageOpen, Mode=TwoWay}">
        <Fluent:Backstage.ToolTip>
            <Fluent:ScreenTip Title="Backstage"
                              Text="File operations and settings" />
        </Fluent:Backstage.ToolTip>

        <Fluent:BackstageTabControl>
            <!-- Action buttons (no content) -->
            <Fluent:Button Header="Save"
                           Icon="{StaticResource SaveIcon}"
                           Command="{Binding SaveCommand}"
                           KeyTip="S" />

            <Fluent:Button Header="Save As"
                           Click="HandleSaveAsClick"
                           IsDefinitive="False"
                           KeyTip="A" />

            <!-- Hidden/Disabled tabs for testing -->
            <Fluent:BackstageTabItem Header="Invisible"
                                     Visibility="Collapsed" />
            <Fluent:BackstageTabItem Header="Disabled"
                                     IsEnabled="False" />

            <!-- Home tab with content -->
            <Fluent:BackstageTabItem Header="Home"
                                     Icon="{StaticResource HomeIcon}"
                                     KeyTip="H">
                <Fluent:BackstageTabItem.ToolTip>
                    <Fluent:ScreenTip Title="Home"
                                      Text="Application home page" />
                </Fluent:BackstageTabItem.ToolTip>

                <StackPanel Margin="20">
                    <TextBlock Text="Welcome to the application"
                               FontSize="24" />
                    <TextBlock Text="Select an option from the menu" />
                </StackPanel>
            </Fluent:BackstageTabItem>

            <!-- Section separator -->
            <Fluent:SeparatorTabItem Header="Documents" />

            <!-- Recent files tab with nested TabControl -->
            <Fluent:BackstageTabItem Header="Recent"
                                     Icon="{StaticResource RecentIcon}"
                                     KeyTip="R">
                <TabControl Style="{DynamicResource Fluent.Ribbon.Styles.InnerBackstageTabControl}"
                            Margin="20 5">
                    <Fluent:SeparatorTabItem Header="Documents" />
                    <TabItem Header="Report.docx" />
                    <TabItem Header="Budget.xlsx" />
                    <Fluent:SeparatorTabItem Header="Folders" />
                    <TabItem Header="Projects" />
                    <TabItem Header="Downloads" />
                </TabControl>
            </Fluent:BackstageTabItem>

            <Separator />

            <!-- Bottom-docked exit button -->
            <Fluent:Button Header="Exit"
                           Command="{Binding ExitCommand}"
                           DockPanel.Dock="Bottom"
                           KeyTip="X" />

            <!-- Bottom-docked options tab -->
            <Fluent:BackstageTabItem Header="Options"
                                     DockPanel.Dock="Bottom"
                                     KeyTip="O">
                <local:OptionsView DataContext="{Binding OptionsViewModel}" />
            </Fluent:BackstageTabItem>
        </Fluent:BackstageTabControl>
    </Fluent:Backstage>
</Fluent:Ribbon.Menu>
```

---

## Common Patterns

### Recent Files Section

```xml
<Fluent:BackstageTabItem Header="Recent" Icon="{StaticResource ClockIcon}">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <StackPanel Grid.Column="0" Margin="20">
            <TextBlock Text="Recent Documents" FontWeight="SemiBold" />
            <ListBox ItemsSource="{Binding RecentDocuments}"
                     BorderThickness="0"
                     Background="Transparent">
                <ListBox.ItemTemplate>
                    <DataTemplate>
                        <StackPanel Orientation="Horizontal">
                            <Image Source="{StaticResource DocIcon}" Width="16" />
                            <TextBlock Text="{Binding Name}" Margin="8 0" />
                        </StackPanel>
                    </DataTemplate>
                </ListBox.ItemTemplate>
            </ListBox>
        </StackPanel>

        <StackPanel Grid.Column="1" Margin="20">
            <TextBlock Text="Recent Folders" FontWeight="SemiBold" />
            <ListBox ItemsSource="{Binding RecentFolders}"
                     BorderThickness="0"
                     Background="Transparent" />
        </StackPanel>
    </Grid>
</Fluent:BackstageTabItem>
```

### Print Preview Section

```xml
<Fluent:BackstageTabItem Header="Print" Icon="{StaticResource PrintIcon}">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="300" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <!-- Print settings -->
        <StackPanel Grid.Column="0" Margin="20">
            <TextBlock Text="Print" FontSize="24" Margin="0 0 0 20" />

            <TextBlock Text="Copies" />
            <Fluent:Spinner Value="{Binding Copies}" Minimum="1" Maximum="99" />

            <TextBlock Text="Printer" Margin="0 10 0 0" />
            <ComboBox ItemsSource="{Binding Printers}"
                      SelectedItem="{Binding SelectedPrinter}" />

            <Fluent:Button Header="Print"
                           Command="{Binding PrintCommand}"
                           Margin="0 20 0 0"
                           Style="{DynamicResource Fluent.Ribbon.Styles.Backstage.Button}" />
        </StackPanel>

        <!-- Preview area -->
        <Border Grid.Column="1"
                Margin="20"
                Background="White"
                BorderBrush="{DynamicResource Fluent.Ribbon.Brushes.Gray6}"
                BorderThickness="1">
            <DocumentViewer Document="{Binding PreviewDocument}" />
        </Border>
    </Grid>
</Fluent:BackstageTabItem>
```

### Info/About Section

```xml
<Fluent:BackstageTabItem Header="Info" Icon="{StaticResource InfoIcon}">
    <StackPanel Margin="20">
        <TextBlock Text="About Application" FontSize="24" />

        <StackPanel Margin="0 20 0 0">
            <TextBlock Text="Document Information" FontWeight="SemiBold" />
            <Grid Margin="0 10 0 0">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="150" />
                    <ColumnDefinition Width="*" />
                </Grid.ColumnDefinitions>
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>

                <TextBlock Text="File name:" Grid.Row="0" Grid.Column="0" />
                <TextBlock Text="{Binding FileName}" Grid.Row="0" Grid.Column="1" />

                <TextBlock Text="Location:" Grid.Row="1" Grid.Column="0" />
                <TextBlock Text="{Binding FilePath}" Grid.Row="1" Grid.Column="1" />

                <TextBlock Text="Last modified:" Grid.Row="2" Grid.Column="0" />
                <TextBlock Text="{Binding LastModified}" Grid.Row="2" Grid.Column="1" />
            </Grid>
        </StackPanel>

        <StackPanel Margin="0 20 0 0">
            <TextBlock Text="Version" FontWeight="SemiBold" />
            <TextBlock Text="{Binding Version}" Margin="0 5 0 0" />
        </StackPanel>
    </StackPanel>
</Fluent:BackstageTabItem>
```

---

## Styling and Theming

### Key Style Resources

| Style Key | Target | Description |
|-----------|--------|-------------|
| `Fluent.Ribbon.Styles.RibbonBackstage` | `Backstage` | Main backstage button style |
| `Fluent.Ribbon.Styles.BackstageTabControl` | `BackstageTabControl` | Tab control container |
| `Fluent.Ribbon.Styles.BackstageTabItem` | `BackstageTabItem` | Individual tab items |
| `Fluent.Ribbon.Styles.BackstageTabControl.Button` | `Button` | Buttons inside backstage |
| `Fluent.Ribbon.Styles.BackstageTabControl.SeparatorTabItem` | `SeparatorTabItem` | Section headers |
| `Fluent.Ribbon.Styles.BackstageBackButton` | `Button` | The back/close button |

### Key Template Resources

| Template Key | Target | Description |
|--------------|--------|-------------|
| `Fluent.Ribbon.Templates.BackstageToggleButton` | `Backstage` | The "File" button template |
| `Fluent.Ribbon.Templates.BackstageTabControl` | `BackstageTabControl` | Layout template |
| `Fluent.Ribbon.Templates.BackstageTabItem` | `BackstageTabItem` | Tab item template |
| `Fluent.Ribbon.Templates.BackstageTabControl.Button` | `Button` | Button template in backstage |
| `Fluent.Ribbon.Templates.BackstageBackButton` | `Button` | Back button template |

### Key Brush Resources

| Brush Key | Usage |
|-----------|-------|
| `Fluent.Ribbon.Brushes.Backstage.Background` | Backstage button background |
| `Fluent.Ribbon.Brushes.Backstage.Foreground` | Backstage button text |
| `Fluent.Ribbon.Brushes.BackstageTabControl.Background` | Content area background |
| `Fluent.Ribbon.Brushes.BackstageTabControl.ItemsPanelBackground` | Left panel background |
| `Fluent.Ribbon.Brushes.BackstageTabItem.Header.Foreground` | Tab header text color |
| `Fluent.Ribbon.Brushes.BackstageTabItem.MouseOver.Background` | Tab hover background |
| `Fluent.Ribbon.Brushes.BackstageTabItem.Selected.Background` | Selected tab indicator |
| `Fluent.Ribbon.Brushes.Backstage.BackButton.Background` | Back button background |
| `Fluent.Ribbon.Brushes.Backstage.BackButton.Foreground` | Back button arrow color |

### Animation Storyboards

| Storyboard Key | Description |
|----------------|-------------|
| `Fluent.Ribbon.Storyboards.Backstage.IsOpenTrueStoryboard` | Opening animation (slide from left) |
| `Fluent.Ribbon.Storyboards.Backstage.IsOpenFalseStoryboard` | Closing animation (slide to left) |

### Customizing the Back Button

```xml
<Style x:Key="CustomBackButton"
       TargetType="Button"
       BasedOn="{StaticResource Fluent.Ribbon.Styles.BackstageBackButton}">
    <Setter Property="Height" Value="60" />
    <Setter Property="Background" Value="{DynamicResource Fluent.Ribbon.Brushes.AccentBase}" />
</Style>
```

### Customizing Tab Item Height

```xml
<Style x:Key="TallBackstageTab"
       TargetType="Fluent:BackstageTabItem"
       BasedOn="{StaticResource Fluent.Ribbon.Styles.BackstageTabItem}">
    <Setter Property="Height" Value="50" />
</Style>
```

### Customizing Left Panel Width

```xml
<Fluent:BackstageTabControl ItemsPanelMinWidth="200">
    <!-- Items -->
</Fluent:BackstageTabControl>
```

---

## Keyboard Navigation

| Key | Action |
|-----|--------|
| `Escape` | Close backstage (if `CloseOnEsc="True"`) |
| `Tab` | Navigate between items and content |
| `F6` | Switch focus between left panel and content |
| `Up/Down` | Navigate between tabs/buttons |
| `Enter/Space` | Select tab or activate button |
| KeyTips | Access items by their KeyTip |

### KeyTip Navigation

```xml
<Fluent:BackstageTabControl>
    <Fluent:Button Header="Save" KeyTip="S" />      <!-- Alt+S -->
    <Fluent:Button Header="Save As" KeyTip="A" />   <!-- Alt+A -->
    <Fluent:BackstageTabItem Header="Home" KeyTip="H" />  <!-- Alt+H -->
    <Fluent:BackstageTabItem Header="Recent" KeyTip="R" /> <!-- Alt+R -->
</Fluent:BackstageTabControl>
```

---

## Automation Support

Each backstage control provides automation peers for accessibility:

| Control | Automation Peer |
|---------|-----------------|
| `Backstage` | `RibbonBackstageAutomationPeer` |
| `BackstageTabControl` | `RibbonBackstageTabControlAutomationPeer` |
| `BackstageTabItem` | `RibbonBackstageTabItemAutomationPeer` |

### AutomationProperties Example

```xml
<Fluent:Backstage AutomationProperties.Name="File Menu">
    <Fluent:BackstageTabControl>
        <Fluent:BackstageTabItem Header="Home"
                                 AutomationProperties.Name="Home Tab"
                                 AutomationProperties.HelpText="Returns to home view">
            <!-- Content -->
        </Fluent:BackstageTabItem>
    </Fluent:BackstageTabControl>
</Fluent:Backstage>
```

---

## Handling Open/Close Events

### In XAML with Event Triggers

```xml
<Fluent:Backstage IsOpenChanged="OnBackstageIsOpenChanged">
    <!-- Content -->
</Fluent:Backstage>
```

```csharp
private void OnBackstageIsOpenChanged(object sender, DependencyPropertyChangedEventArgs e)
{
    bool isOpen = (bool)e.NewValue;
    if (isOpen)
    {
        // Backstage opened - refresh data
        LoadRecentFiles();
    }
    else
    {
        // Backstage closed
    }
}
```

### In ViewModel with PropertyChanged

```csharp
private bool _isBackstageOpen;
public bool IsBackstageOpen
{
    get => _isBackstageOpen;
    set
    {
        if (SetProperty(ref _isBackstageOpen, value))
        {
            if (value)
            {
                // Opened
                OnBackstageOpened();
            }
            else
            {
                // Closed
                OnBackstageClosed();
            }
        }
    }
}
```

---

## Preventing Backstage Close

Use `CanChangeIsOpen` to temporarily prevent closing:

```csharp
// During a save operation
backstage.CanChangeIsOpen = false;

try
{
    await SaveDocumentAsync();
}
finally
{
    backstage.CanChangeIsOpen = true;
}
```

Or with Style Trigger (from Showcase):

```xml
<!--<Fluent:Backstage.Style>
    <Style TargetType="Fluent:Backstage">
        <Style.Triggers>
            <Trigger Property="Fluent:Backstage.IsOpen" Value="True">
                <Setter Property="Fluent:Backstage.CanChangeIsOpen" Value="False" />
            </Trigger>
        </Style.Triggers>
    </Style>
</Fluent:Backstage.Style>-->
```

---

## Nested TabControl (Inner Backstage Style)

For sub-navigation within a BackstageTabItem, use the built-in style:

```xml
<Fluent:BackstageTabItem Header="Recent">
    <TabControl Style="{DynamicResource Fluent.Ribbon.Styles.InnerBackstageTabControl}"
                Margin="20 5">
        <Fluent:SeparatorTabItem Header="Documents" />
        <TabItem Header="Report.docx" />
        <TabItem Header="Budget.xlsx" />
        <Fluent:SeparatorTabItem Header="Folders" />
        <TabItem Header="Projects" />
        <TabItem Header="Downloads" />
    </TabControl>
</Fluent:BackstageTabItem>
```

This style provides:
- Vertical left-aligned tabs
- Theme-aware hover/selected colors
- Support for `SeparatorTabItem` section headers
- Separator line between tabs and content

---

## Content Area Buttons

For buttons within backstage content areas (not in the left panel), use the backstage button style:

```xml
<Fluent:BackstageTabItem Header="Print">
    <StackPanel>
        <Fluent:Button Header="Print"
                       Icon="{StaticResource PrintIcon}"
                       Command="{Binding PrintCommand}"
                       Style="{DynamicResource Fluent.Ribbon.Styles.Backstage.Button}" />

        <Fluent:DropDownButton Header="Printer"
                               Style="{DynamicResource Fluent.Ribbon.Styles.Backstage.DropDownButton}">
            <!-- Menu items -->
        </Fluent:DropDownButton>
    </StackPanel>
</Fluent:BackstageTabItem>
```

Available content area styles:

| Style | Target | Size |
|-------|--------|------|
| `Fluent.Ribbon.Styles.Backstage.Button` | `Fluent:Button` | 85x81 |
| `Fluent.Ribbon.Styles.Backstage.ToggleButton` | `Fluent:ToggleButton` | 85x81 |
| `Fluent.Ribbon.Styles.Backstage.DropDownButton` | `Fluent:DropDownButton` | 85x81 |
| `Fluent.Ribbon.Styles.Backstage.ComboBox` | `Fluent:ComboBox` | 229x42 |

---

## Source Files Reference

| File | Purpose |
|------|---------|
| `Fluent.Ribbon\Controls\Backstage.cs` | Main Backstage control |
| `Fluent.Ribbon\Controls\BackstageTabControl.cs` | Tab container control |
| `Fluent.Ribbon\Controls\BackstageTabItem.cs` | Tab item control |
| `Fluent.Ribbon\Controls\BackstageAdorner.cs` | Adorner for rendering backstage |
| `Fluent.Ribbon\Controls\SeparatorTabItem.cs` | Section header control |
| `Fluent.Ribbon\Themes\Controls\Backstage.xaml` | Backstage button styles/templates |
| `Fluent.Ribbon\Themes\Controls\BackstageTabControl.xaml` | TabControl styles/templates |
| `Fluent.Ribbon\Themes\Controls\BackstageTabItem.xaml` | TabItem styles/templates |
| `Fluent.Ribbon\Themes\Controls\BackstageControls.xaml` | Content area button styles, InnerBackstageTabControl |
| `Fluent.Ribbon\StyleSelectors\BackstageTabControlItemContainerStyleSelector.cs` | Auto-applies styles to child items |
| `Fluent.Ribbon.Showcase\TestContent.xaml:187-332` | Full usage examples |

---

## Summary Table

| Task | Solution |
|------|----------|
| Open/close backstage | Set `IsOpen` property |
| Bind to ViewModel | `IsOpen="{Binding IsBackstageOpen, Mode=TwoWay}"` |
| Add action button | Add `Fluent:Button` to `BackstageTabControl` |
| Keep backstage open on button click | Set `IsDefinitive="False"` on button |
| Add selectable tab | Add `Fluent:BackstageTabItem` |
| Add section header | Add `Fluent:SeparatorTabItem` |
| Add divider line | Add `Separator` |
| Position item at bottom | Set `DockPanel.Dock="Bottom"` |
| Add nested tabs | Use `TabControl` with `Fluent.Ribbon.Styles.InnerBackstageTabControl` |
| Style content buttons | Use `Fluent.Ribbon.Styles.Backstage.Button` |
| Prevent closing | Set `CanChangeIsOpen="False"` |
| Disable animations | Set `AreAnimationsEnabled="False"` |
| Disable Esc to close | Set `CloseOnEsc="False"` |
| Change left panel width | Set `ItemsPanelMinWidth` |
| Hide back button | Set `IsBackButtonVisible="False"` |
| Add keyboard shortcut | Set `KeyTip` property |
