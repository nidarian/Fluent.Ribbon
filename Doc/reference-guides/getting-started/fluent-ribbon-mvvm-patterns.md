---
title: Fluent.Ribbon MVVM Patterns
description: Comprehensive guide to implementing MVVM patterns with Fluent.Ribbon controls
tags: [mvvm, viewmodel, commands, bindings, getting-started]
see_also:
  - fluent-ribbon-controls-reference-v2.md
  - fluent-ribbon-common-tasks-v2.md
  - ../controls/fluent-ribbon-button-controls-reference.md
---

# Fluent.Ribbon MVVM Patterns Guide

**Comprehensive guide to implementing MVVM patterns with Fluent.Ribbon controls.**

*Based on patterns extracted from the official Fluent.Ribbon.Showcase application.*

---

## Table of Contents

1. [ViewModel Base Class](#viewmodel-base-class)
2. [Command Patterns](#command-patterns)
3. [DataContext Setup](#datacontext-setup)
4. [Command Binding Patterns](#command-binding-patterns)
5. [Property Binding Patterns](#property-binding-patterns)
6. [ItemsSource Binding for Dynamic Ribbons](#itemssource-binding-for-dynamic-ribbons)
7. [Gallery and InRibbonGallery Binding](#gallery-and-inribbongallery-binding)
8. [Backstage ViewModel Patterns](#backstage-viewmodel-patterns)
9. [Menu ViewModel Patterns](#menu-viewmodel-patterns)
10. [Contextual Tab Groups](#contextual-tab-groups)
11. [Ribbon State Binding](#ribbon-state-binding)
12. [DO NOT DO - Code-Behind Anti-Patterns](#do-not-do---code-behind-anti-patterns)

---

## ViewModel Base Class

All ViewModels should implement `INotifyPropertyChanged`. Here's the standard base class pattern from the Showcase:

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### Property Pattern

Always use this pattern for bindable properties:

```csharp
private bool isChecked;

public bool IsChecked
{
    get => this.isChecked;
    set
    {
        if (value == this.isChecked)
        {
            return;
        }

        this.isChecked = value;
        this.OnPropertyChanged();
    }
}
```

**Key points:**
- Use early return for equality check (prevents unnecessary updates)
- Call `OnPropertyChanged()` without parameter (uses `[CallerMemberName]`)
- Use `this.` prefix consistently (Showcase convention)

---

## Command Patterns

### RelayCommand Implementation

The Showcase uses a custom `RelayCommand` with `CommandManager.RequerySuggested` for automatic CanExecute updates:

```csharp
using System;
using System.Windows.Input;

public class RelayCommand : ICommand
{
    private readonly Action action;
    private readonly Func<bool>? canExecute;

    public RelayCommand(Action execute)
    {
        this.action = execute;
    }

    public RelayCommand(Action execute, Func<bool> canExecute)
        : this(execute)
    {
        this.canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return this.canExecute is null || this.canExecute();
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public void Execute(object? parameter)
    {
        this.action();
    }
}
```

### Generic RelayCommand<T>

For commands that need parameters:

```csharp
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> action;
    private readonly Func<T?, bool>? canExecute;

    public RelayCommand(Action<T?> action)
        : this(action, null)
    {
    }

    public RelayCommand(Action<T?> action, Func<T?, bool>? canExecute)
    {
        this.action = action;
        this.canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return this.canExecute is null || this.canExecute((T?)parameter);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public void Execute(object? parameter)
    {
        this.action((T?)parameter);
    }
}
```

### Command with CanExecute

Buttons automatically enable/disable based on `CanExecute`:

```csharp
// ViewModel
public ICommand ExitCommand { get; }

public MainViewModel()
{
    // Button is disabled when BoundSpinnerValue <= 0
    this.ExitCommand = new RelayCommand(
        () => Application.Current.Shutdown(),
        () => this.BoundSpinnerValue > 0
    );
}
```

```xml
<!-- XAML - Button auto-disables when CanExecute returns false -->
<fluent:Button Header="Exit"
               Command="{Binding ExitCommand}" />
```

---

## DataContext Setup

### Window/UserControl Level

Set DataContext in code-behind constructor (recommended pattern from Showcase):

```csharp
public partial class TestContent : UserControl
{
    private readonly MainViewModel viewModel;

    public TestContent()
    {
        this.InitializeComponent();

        this.viewModel = new MainViewModel();
        this.DataContext = this.viewModel;
    }
}
```

### Design-Time DataContext

For XAML IntelliSense support:

```xml
<UserControl x:Class="MyApp.TestContent"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:viewModels="clr-namespace:MyApp.ViewModels"
             d:DataContext="{d:DesignInstance viewModels:MainViewModel, IsDesignTimeCreatable=False}">
```

### Nested ViewModel DataContext

For complex views with sub-ViewModels:

```xml
<!-- Parent uses MainViewModel -->
<fluent:BackstageTabItem Header="Colors"
                         DataContext="{Binding ColorViewModel}">
    <!-- Children bind to ColorViewModel properties directly -->
    <fluent:ColorGallery SelectedColor="{Binding StandardColor, Mode=TwoWay}" />
</fluent:BackstageTabItem>
```

---

## Command Binding Patterns

### Basic Command Binding

```xml
<fluent:Button Header="Test"
               Command="{Binding TestCommand}" />
```

### Command with Parameter

```xml
<fluent:Button Header="{Binding Text}"
               Command="{Binding Command}"
               CommandParameter="{Binding}" />
```

### Nested ViewModel Commands

Use path notation for nested ViewModels:

```xml
<fluent:Button Header="Refresh"
               Command="{Binding GalleryViewModel.RefreshCommand}" />

<fluent:Button Header="Start/Stop"
               Command="{Binding IssueReprosViewModel.ThemeManagerFromThread.StartStopCommand}" />
```

### Preview Commands for Gallery Items

Gallery items can have preview/cancel preview commands:

```csharp
// ViewModel
public ICommand PreviewCommand { get; }
public ICommand CancelPreviewCommand { get; }

public MainViewModel()
{
    this.PreviewCommand = new RelayCommand<GalleryItem>(Preview);
    this.CancelPreviewCommand = new RelayCommand<GalleryItem>(CancelPreview);
}

private static void Preview(GalleryItem? galleryItem)
{
    // Apply preview effect
}

private static void CancelPreview(GalleryItem? galleryItem)
{
    // Remove preview effect
}
```

```xml
<fluent:InRibbonGallery ItemsSource="{Binding GalleryViewModel.Items}">
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

## Property Binding Patterns

### IsChecked Binding (ToggleButton, CheckBox)

```csharp
// ViewModel
private bool? isCheckedToggleButton = true;

public bool? IsCheckedToggleButton
{
    get => this.isCheckedToggleButton;
    set
    {
        if (value == this.isCheckedToggleButton) return;
        this.isCheckedToggleButton = value;
        this.OnPropertyChanged();
    }
}
```

```xml
<fluent:ToggleButton Header="Option"
                     IsChecked="{Binding IsCheckedToggleButton}" />

<fluent:CheckBox IsChecked="{Binding IsEnabled, Mode=TwoWay}">
    Enable Feature
</fluent:CheckBox>
```

### Visibility Binding with Converter

```xml
<fluent:RibbonContextualTabGroup Header="Tools"
                                  Visibility="{Binding AreContextGroupsVisible,
                                               Converter={StaticResource Fluent.Ribbon.Converters.BoolToVisibilityConverter}}" />
```

### Spinner Value Binding

```csharp
private int boundSpinnerValue;

public int BoundSpinnerValue
{
    get => this.boundSpinnerValue;
    set
    {
        if (value == this.boundSpinnerValue) return;
        this.boundSpinnerValue = value;
        this.OnPropertyChanged();
    }
}
```

```xml
<fluent:Spinner Header="Value"
                Value="{Binding BoundSpinnerValue, Mode=TwoWay}" />
```

### Zoom/Scale Transform Binding

```xml
<Grid.LayoutTransform>
    <ScaleTransform ScaleX="{Binding Zoom}"
                    ScaleY="{Binding Zoom}" />
</Grid.LayoutTransform>
```

---

## ItemsSource Binding for Dynamic Ribbons

### ComboBox with ItemsSource

```csharp
// ViewModel
public ObservableCollection<string> FontsData { get; } =
    new(System.Windows.Media.Fonts.SystemFontFamilies
        .Select(f => f.ToString()));
```

```xml
<fluent:ComboBox Header="Font"
                 ItemsSource="{Binding FontsViewModel.FontsData}"
                 IsEditable="True"
                 IsReadOnly="True" />
```

### DropDownButton with Dynamic Items

```xml
<fluent:DropDownButton Header="Themes"
                       DisplayMemberPath="Name"
                       ItemsSource="{Binding Source={x:Static controlzEx:ThemeManager.Current},
                                             Path=Themes}" />
```

### Dynamic Menu Items

```csharp
// MenuItem ViewModel
public class MenuItemViewModel
{
    public string Header { get; set; }
    public bool IsCheckable { get; set; }
    public bool IsChecked { get; set; }
    public ObservableCollection<MenuItemViewModel> Children { get; set; }
}

// Parent ViewModel
public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
```

```xml
<fluent:DropDownButton Header="Menu"
                       ItemsSource="{Binding MenuViewModel.MenuItems}">
    <fluent:DropDownButton.ItemContainerStyle>
        <Style TargetType="fluent:MenuItem">
            <Setter Property="Header" Value="{Binding Header}" />
            <Setter Property="IsCheckable" Value="{Binding IsCheckable}" />
            <Setter Property="IsChecked" Value="{Binding IsChecked}" />
            <Setter Property="ItemsSource" Value="{Binding Children}" />
        </Style>
    </fluent:DropDownButton.ItemContainerStyle>
</fluent:DropDownButton>
```

---

## Gallery and InRibbonGallery Binding

### Gallery Item ViewModel

```csharp
public class GalleryItemViewModel : ViewModel
{
    private string text;
    private string group;

    public GalleryItemViewModel(string group, string text)
    {
        this.Group = group;
        this.Text = text;
    }

    public string Text
    {
        get => this.text;
        set
        {
            if (value == this.text) return;
            this.text = value;
            this.OnPropertyChanged();
        }
    }

    public string Group
    {
        get => this.group;
        set
        {
            if (value == this.group) return;
            this.group = value;
            this.OnPropertyChanged();
        }
    }
}
```

### Gallery ViewModel with Refresh

```csharp
public class GalleryViewModel : ViewModel
{
    public ObservableCollection<GalleryItemViewModel> Items { get; }
    public ICommand RefreshCommand { get; }

    public GalleryViewModel()
    {
        this.Items = new ObservableCollection<GalleryItemViewModel>();
        this.RefreshCommand = new RelayCommand(this.Refresh);
        this.Refresh();
    }

    public void Refresh()
    {
        this.Items.Clear();

        this.Items.Add(new GalleryItemViewModel("Group 1", "Item 1"));
        this.Items.Add(new GalleryItemViewModel("Group 1", "Item 2"));
        this.Items.Add(new GalleryItemViewModel("Group 2", "Item 3"));
        // etc.
    }
}
```

### InRibbonGallery with Grouping

```xml
<fluent:InRibbonGallery Header="Gallery"
                         ItemsSource="{Binding GalleryViewModel.Items}"
                         GroupBy="Group"
                         MaxItemsInRow="5"
                         MinItemsInRow="2">
    <fluent:InRibbonGallery.ItemTemplate>
        <DataTemplate DataType="{x:Type viewModels:GalleryItemViewModel}">
            <Border BorderBrush="Aqua" BorderThickness="1">
                <TextBlock Text="{Binding Text}"
                           HorizontalAlignment="Center"
                           VerticalAlignment="Center" />
            </Border>
        </DataTemplate>
    </fluent:InRibbonGallery.ItemTemplate>
</fluent:InRibbonGallery>
```

### Gallery with Images

```csharp
public class GallerySampleDataItemViewModel : ViewModel
{
    public ImageSource? Icon { get; }
    public ImageSource? IconLarge { get; }
    public string Text { get; }
    public string Group { get; }
    public ICommand Command { get; }

    public static GallerySampleDataItemViewModel Create(
        string icon, string iconLarge, string text, string group)
    {
        return new GallerySampleDataItemViewModel(icon, iconLarge, text, group);
    }

    private GallerySampleDataItemViewModel(
        string icon, string iconLarge, string text, string group)
    {
        this.Icon = LoadImage(icon);
        this.IconLarge = LoadImage(iconLarge);
        this.Text = text;
        this.Group = group;
        this.Command = new RelayCommand(() => Debug.WriteLine("Clicked"));
    }
}
```

```xml
<!-- DataTemplate for gallery items -->
<DataTemplate x:Key="GalleryDataItemTemplate"
              DataType="{x:Type viewModels:GallerySampleDataItemViewModel}">
    <Border Background="Transparent" ToolTip="{Binding Text}">
        <StackPanel Orientation="Horizontal">
            <Image Source="{Binding Icon}" />
            <TextBlock Text="{Binding Text}" VerticalAlignment="Center" />
        </StackPanel>
    </Border>
</DataTemplate>

<fluent:Gallery ItemsSource="{Binding DataItems}"
                ItemTemplate="{StaticResource GalleryDataItemTemplate}"
                MaxItemsInRow="3" />
```

### Advanced Grouping with Function

```csharp
// ViewModel
public Func<object, string> GroupByAdvancedSample { get; }

public MainViewModel()
{
    // Group by first letter
    this.GroupByAdvancedSample = x =>
        ((GallerySampleDataItemViewModel)x).Text.Substring(0, 1);
}
```

```xml
<fluent:InRibbonGallery Header="Advanced Grouping"
                         ItemsSource="{Binding DataItems}"
                         GroupByAdvanced="{Binding GroupByAdvancedSample}" />
```

### Gallery Filters

```xml
<fluent:InRibbonGallery ItemsSource="{Binding DataItems}"
                         GroupBy="Group">
    <fluent:InRibbonGallery.Filters>
        <fluent:GalleryGroupFilter x:Name="FilterAll"
                                   Title="All"
                                   Groups="Group A,Group B" />
        <fluent:GalleryGroupFilter Title="Group A"
                                   Groups="Group A" />
        <fluent:GalleryGroupFilter Title="Group B"
                                   Groups="Group B" />
    </fluent:InRibbonGallery.Filters>
</fluent:InRibbonGallery>
```

---

## Backstage ViewModel Patterns

### Backstage IsOpen Binding

```csharp
// ViewModel
private bool isBackstageOpen;

public bool IsBackstageOpen
{
    get => this.isBackstageOpen;
    set
    {
        if (value == this.isBackstageOpen) return;
        this.isBackstageOpen = value;
        this.OnPropertyChanged();
    }
}
```

```xml
<fluent:Backstage IsOpen="{Binding IsBackstageOpen, Mode=TwoWay}">
    <fluent:BackstageTabControl>
        <!-- tabs and content -->
    </fluent:BackstageTabControl>
</fluent:Backstage>
```

### Backstage with Commands

```xml
<fluent:BackstageTabControl>
    <!-- Command buttons -->
    <fluent:Button Header="Save"
                   Command="{Binding SaveCommand}"
                   Icon="{iconPacks:Material Kind=ContentSave}"
                   KeyTip="S" />

    <fluent:Button Header="Exit"
                   Command="{Binding ExitCommand}"
                   IsDefinitive="True" />

    <!-- Tab items with content -->
    <fluent:BackstageTabItem Header="Home" KeyTip="H">
        <StackPanel>
            <fluent:Button Header="Action"
                           Command="{Binding TestCommand}" />
        </StackPanel>
    </fluent:BackstageTabItem>

    <fluent:SeparatorTabItem Header="Options" />

    <fluent:BackstageTabItem Header="Settings">
        <!-- Settings content -->
    </fluent:BackstageTabItem>
</fluent:BackstageTabControl>
```

### Backstage Tab with Sub-ViewModel

```xml
<fluent:BackstageTabItem Header="Colors"
                         DataContext="{Binding ColorViewModel}">
    <StackPanel>
        <fluent:ComboBox Header="Theme"
                         ItemsSource="{Binding Source={x:Static controlzEx:ThemeManager.Current},
                                               Path=Themes}"
                         SelectedItem="{Binding CurrentTheme, Mode=TwoWay}" />
    </StackPanel>
</fluent:BackstageTabItem>
```

---

## Menu ViewModel Patterns

### Hierarchical Menu Items

```csharp
public class MenuItemViewModel
{
    public string Header { get; set; }
    public bool IsCheckable { get; set; }
    public bool IsChecked { get; set; }
    public ObservableCollection<MenuItemViewModel> Children { get; set; }
    public ObservableCollection<MenuItemViewModel> ContextMenuItems { get; set; }

    public MenuItemViewModel(string header, bool isCheckable = false, bool isChecked = false)
    {
        this.Header = header;
        this.IsCheckable = isCheckable;
        this.IsChecked = isChecked;
        this.Children = new ObservableCollection<MenuItemViewModel>();
        this.ContextMenuItems = new ObservableCollection<MenuItemViewModel>();
    }
}
```

```xml
<fluent:ApplicationMenu Header="File">
    <fluent:MenuItem Header="Recent"
                     ItemsSource="{Binding MenuViewModel.MenuItems}">
        <fluent:MenuItem.ItemContainerStyle>
            <Style TargetType="fluent:MenuItem">
                <Setter Property="Header" Value="{Binding Header}" />
                <Setter Property="IsCheckable" Value="{Binding IsCheckable}" />
                <Setter Property="IsChecked" Value="{Binding IsChecked}" />
                <Setter Property="ItemsSource" Value="{Binding Children}" />
            </Style>
        </fluent:MenuItem.ItemContainerStyle>
    </fluent:MenuItem>
</fluent:ApplicationMenu>
```

---

## Contextual Tab Groups

### Visibility Binding

```csharp
private bool areContextGroupsVisible = true;

public bool AreContextGroupsVisible
{
    get => this.areContextGroupsVisible;
    set
    {
        if (value == this.areContextGroupsVisible) return;
        this.areContextGroupsVisible = value;
        this.OnPropertyChanged();
    }
}
```

```xml
<fluent:Ribbon.ContextualGroups>
    <fluent:RibbonContextualTabGroup x:Name="ToolsGroup"
                                      Header="Tools"
                                      Background="{DynamicResource Fluent.Ribbon.Brushes.AccentBase}"
                                      Visibility="{Binding AreContextGroupsVisible,
                                                   Converter={StaticResource Fluent.Ribbon.Converters.BoolToVisibilityConverter}}" />
</fluent:Ribbon.ContextualGroups>
```

---

## Ribbon State Binding

### Binding to Ribbon Properties

```xml
<!-- Control ribbon state from ViewModel or other UI elements -->
<fluent:CheckBox IsChecked="{Binding IsMinimized, ElementName=ribbon}">
    Minimized
</fluent:CheckBox>

<fluent:CheckBox IsChecked="{Binding CanMinimize, ElementName=ribbon}">
    Allow Minimize
</fluent:CheckBox>

<fluent:CheckBox IsChecked="{Binding IsSimplified, ElementName=ribbon}">
    Simplified Mode
</fluent:CheckBox>

<fluent:CheckBox IsChecked="{Binding ShowQuickAccessToolBarAboveRibbon, ElementName=ribbon}">
    QAT Above Ribbon
</fluent:CheckBox>

<fluent:CheckBox IsChecked="{Binding AreTabHeadersVisible, ElementName=ribbon}">
    Show Tab Headers
</fluent:CheckBox>
```

### Two-Way Binding for Ribbon Window Properties

```xml
<fluent:ComboBox Header="Resize Mode"
                 ItemsSource="{Binding Source={StaticResource ResizeModeEnumValues}}"
                 SelectedItem="{Binding Path=ResizeMode,
                                Mode=TwoWay,
                                UpdateSourceTrigger=PropertyChanged,
                                RelativeSource={RelativeSource AncestorType={x:Type Window}}}" />
```

---

## DO NOT DO - Code-Behind Anti-Patterns

### Anti-Pattern 1: Click Event Handlers Instead of Commands

**Wrong:**
```xml
<fluent:Button Header="Save" Click="SaveButton_Click" />
```

```csharp
private void SaveButton_Click(object sender, RoutedEventArgs e)
{
    // Logic here - NOT testable, NOT reusable
    this.viewModel.Save();
}
```

**Correct:**
```xml
<fluent:Button Header="Save" Command="{Binding SaveCommand}" />
```

```csharp
// ViewModel
public ICommand SaveCommand { get; }

public MainViewModel()
{
    this.SaveCommand = new RelayCommand(Save, CanSave);
}
```

### Anti-Pattern 2: Direct UI Manipulation in Code-Behind

**Wrong:**
```csharp
private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
{
    this.detailsPanel.Visibility = Visibility.Visible;
    this.titleLabel.Text = selectedItem.Name;
}
```

**Correct:**
```xml
<StackPanel Visibility="{Binding IsDetailsVisible,
                         Converter={StaticResource BoolToVisibilityConverter}}">
    <TextBlock Text="{Binding SelectedItem.Name}" />
</StackPanel>
```

### Anti-Pattern 3: Hardcoded Data in Code-Behind

**Wrong:**
```csharp
private void LoadData_Click(object sender, RoutedEventArgs e)
{
    comboBox.Items.Add("Item 1");
    comboBox.Items.Add("Item 2");
    comboBox.Items.Add("Item 3");
}
```

**Correct:**
```csharp
// ViewModel
public ObservableCollection<string> Items { get; } = new()
{
    "Item 1", "Item 2", "Item 3"
};
```

```xml
<fluent:ComboBox ItemsSource="{Binding Items}" />
```

### Anti-Pattern 4: Accessing Named Elements from Code-Behind

**Wrong:**
```csharp
private void UpdateUI()
{
    this.ribbon.IsMinimized = true;
    this.saveButton.IsEnabled = this.hasChanges;
}
```

**Correct:**
```csharp
// ViewModel
public bool IsRibbonMinimized { get; set; }
public bool CanSave => this.hasChanges;
```

```xml
<fluent:Ribbon IsMinimized="{Binding IsRibbonMinimized}" />
<fluent:Button Header="Save"
               Command="{Binding SaveCommand}" /> <!-- CanExecute handles enabling -->
```

### Anti-Pattern 5: Event Handlers for State Changes

**Wrong:**
```csharp
private void CheckBox_Checked(object sender, RoutedEventArgs e)
{
    this.featureEnabled = true;
    UpdateRelatedControls();
}

private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
{
    this.featureEnabled = false;
    UpdateRelatedControls();
}
```

**Correct:**
```csharp
// ViewModel with property change notification
private bool featureEnabled;

public bool FeatureEnabled
{
    get => this.featureEnabled;
    set
    {
        if (value == this.featureEnabled) return;
        this.featureEnabled = value;
        this.OnPropertyChanged();
        this.OnPropertyChanged(nameof(RelatedVisibility));
    }
}

public Visibility RelatedVisibility =>
    FeatureEnabled ? Visibility.Visible : Visibility.Collapsed;
```

### When Code-Behind IS Acceptable

Some scenarios legitimately require code-behind:

1. **Window lifecycle events** (Loaded, Closing)
2. **Focus management** (element.Focus())
3. **Animations and visual states**
4. **Complex drag-and-drop**
5. **Integration with non-WPF components**

```csharp
// Acceptable: Window lifecycle
private void Window_Loaded(object sender, RoutedEventArgs e)
{
    this.searchBox.Focus();
}

// Acceptable: Keyboard shortcuts with complex logic
private void OnKeyDown(object sender, KeyEventArgs e)
{
    if (e.Key == Key.F11 && Keyboard.Modifiers == ModifierKeys.Control)
    {
        this.viewModel.IsBackstageOpen = !this.viewModel.IsBackstageOpen;
    }
}
```

---

## Quick Reference Cheat Sheet

| Scenario | Pattern |
|----------|---------|
| Button action | `Command="{Binding MyCommand}"` |
| Button enable/disable | `CanExecute` in command |
| Checkbox state | `IsChecked="{Binding MyBool}"` |
| Show/hide | `Visibility="{Binding MyBool, Converter={...}}"` |
| ComboBox items | `ItemsSource="{Binding MyCollection}"` |
| ComboBox selection | `SelectedItem="{Binding MySelected, Mode=TwoWay}"` |
| Gallery items | `ItemsSource="{Binding Items}"` + `ItemTemplate` |
| Gallery grouping | `GroupBy="PropertyName"` or `GroupByAdvanced="{Binding Func}"` |
| Nested VM | `DataContext="{Binding SubViewModel}"` |
| Nested command | `Command="{Binding SubVM.MyCommand}"` |
| Backstage open | `IsOpen="{Binding IsBackstageOpen, Mode=TwoWay}"` |
| Context visibility | `Visibility="{Binding IsVisible, Converter={...}}"` |

---

## Related Guides

- [fluent-ribbon-controls-reference-v2.md](fluent-ribbon-controls-reference-v2.md) - Control properties
- [fluent-ribbon-backstage-reference.md](../controls/fluent-ribbon-backstage-reference.md) - Backstage patterns
- [fluent-ribbon-qat-reference-v2.md](../controls/fluent-ribbon-qat-reference-v2.md) - Quick Access Toolbar
- [controlzex-theming-reference-v2.md](../styling/controlzex-theming-reference-v2.md) - Theme binding
