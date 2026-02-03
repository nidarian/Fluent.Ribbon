---
title: Fluent.Ribbon Performance Guide
description: Optimizing Fluent.Ribbon performance for large and complex ribbon applications
tags: [performance, optimization, advanced]
see_also:
  - ../reference/fluent-ribbon-troubleshooting.md
  - ../controls/fluent-ribbon-galleries-reference.md
---

# Fluent.Ribbon Performance Guide

**Comprehensive guide to optimizing Fluent.Ribbon performance for large and complex ribbon applications.**

*Based on source analysis of Fluent.Ribbon 10.x*

---

## Table of Contents

1. [Overview](#overview)
2. [Large Ribbon Optimization](#large-ribbon-optimization)
3. [Gallery Virtualization](#gallery-virtualization)
4. [Lazy Loading Tab Content](#lazy-loading-tab-content)
5. [Icon Caching Strategies](#icon-caching-strategies)
6. [Reducing Visual Tree Complexity](#reducing-visual-tree-complexity)
7. [Avoiding Unnecessary Bindings](#avoiding-unnecessary-bindings)
8. [Memory Management](#memory-management)
9. [Startup Time Optimization](#startup-time-optimization)
10. [Profiling Ribbon Performance](#profiling-ribbon-performance)
11. [When NOT to Use Certain Features](#when-not-to-use-certain-features)
12. [DO NOT DO Section](#do-not-do-section)

---

## Overview

Fluent.Ribbon is a feature-rich control library, but with great power comes potential performance pitfalls. This guide covers the key areas where performance problems typically occur and how to avoid or mitigate them.

### Key Performance Factors

1. **Measure/Arrange Cycles** - Ribbon controls perform adaptive layout that can trigger multiple measure passes
2. **Visual Tree Depth** - Each control adds multiple visual elements
3. **Binding Overhead** - Complex bindings can slow down property changes
4. **Image Loading** - Icons are loaded and scaled frequently
5. **State Caching** - The ribbon caches layout states that can become stale

---

## Large Ribbon Optimization

### Use ReduceOrder for Predictable Layout

The `ReduceOrder` property on `RibbonTabItem` controls how groups shrink when space is limited. Defining this explicitly prevents expensive trial-and-error layout calculations.

```xml
<fluent:RibbonTabItem Header="Home"
                       ReduceOrder="Clipboard,Font,Paragraph,(Clipboard),(Font)">
    <fluent:RibbonGroupBox x:Name="Clipboard" Header="Clipboard">
        <!-- content -->
    </fluent:RibbonGroupBox>
    <fluent:RibbonGroupBox x:Name="Font" Header="Font">
        <!-- content -->
    </fluent:RibbonGroupBox>
    <fluent:RibbonGroupBox x:Name="Paragraph" Header="Paragraph">
        <!-- content -->
    </fluent:RibbonGroupBox>
</fluent:RibbonTabItem>
```

**How ReduceOrder works:**
- Names without parentheses change the group state (Large -> Middle -> Small -> Collapsed)
- Names in parentheses `(Name)` scale internal `IScalableRibbonControl` items (like `InRibbonGallery`)
- The order specifies which group reduces first

**Performance Impact:**
- Without `ReduceOrder`: Multiple measure passes to find optimal layout
- With `ReduceOrder`: Predictable single-pass layout in most cases

### StateDefinition for Controlled Transitions

Use `StateDefinition` to limit which states a group can transition through:

```xml
<fluent:RibbonGroupBox Header="Large Only"
                        StateDefinition="Large">
    <!-- Always stays Large, collapses directly to dropdown -->
</fluent:RibbonGroupBox>

<fluent:RibbonGroupBox Header="Skip Middle"
                        StateDefinition="Large,Small,Collapsed">
    <!-- Skips Middle state entirely -->
</fluent:RibbonGroupBox>
```

### Limit Number of Groups Per Tab

Each `RibbonGroupBox` adds layout complexity. Consider:
- **Maximum 6-8 groups per tab** for optimal performance
- Move less-used items to the Backstage or ApplicationMenu
- Group related commands into single groups with submenus

### Cache Invalidation Awareness

The ribbon's `RibbonGroupsContainer` maintains a `MeasureCache` to prevent layout "flicker". Be aware that these actions invalidate the cache:

- Changing `ReduceOrder`
- Adding/removing items from groups
- Changing visibility of controls
- Font size/family changes
- `StateDefinition` changes

```csharp
// Source: RibbonGroupsContainer.cs
private readonly struct MeasureCache
{
    public Size AvailableSize { get; }
    public Size DesiredSize { get; }
}
```

---

## Gallery Virtualization

### Understanding Gallery Architecture

Fluent.Ribbon galleries (`Gallery`, `InRibbonGallery`) use a `GalleryPanel` that creates `GalleryGroupContainer` instances for grouping. **Standard WPF virtualization is NOT automatically enabled.**

### GalleryPanel Behavior

```csharp
// Source: GalleryPanel.cs - Creates containers on refresh
private void Refresh()
{
    // Clear and recreate all group containers
    foreach (var galleryGroupContainer in this.galleryGroupContainers)
    {
        BindingOperations.ClearAllBindings(galleryGroupContainer);
        this.visualCollection.Remove(galleryGroupContainer);
    }
    this.galleryGroupContainers.Clear();
    // ... recreates containers
}
```

### Suspending Updates for Batch Changes

When adding many items, suspend updates to prevent multiple refresh cycles:

```csharp
// Source: GalleryPanel provides SuspendUpdates/ResumeUpdates
galleryPanel.SuspendUpdates();
try
{
    foreach (var item in largeCollection)
    {
        gallery.Items.Add(item);
    }
}
finally
{
    galleryPanel.ResumeUpdatesRefresh();
}
```

### Limit Gallery Item Count

For large collections, implement your own pagination or filtering:

```xml
<!-- Instead of binding 1000 items directly -->
<fluent:InRibbonGallery ItemsSource="{Binding FilteredStyles}"
                         MaxItemsInRow="8"
                         MinItemsInRow="4"
                         MaxItemsInDropDownRow="10"
                         MinItemsInDropDownRow="5" />
```

```csharp
// ViewModel - implement filtering
public ICollectionView FilteredStyles =>
    CollectionViewSource.GetDefaultView(_allStyles)
        .Cast<Style>()
        .Take(100); // Limit visible items
```

### Use Fixed Item Sizes

Always specify `ItemWidth` and `ItemHeight` to avoid per-item measurement:

```xml
<fluent:InRibbonGallery ItemWidth="72"
                         ItemHeight="56"
                         ItemsSource="{Binding Styles}" />
```

---

## Lazy Loading Tab Content

### The Problem

By default, all `RibbonTabItem` content is created when the ribbon loads, even for tabs never viewed.

### Deferred Content Loading Pattern

```xml
<fluent:RibbonTabItem Header="Reports"
                       IsSelected="{Binding IsReportsTabSelected, Mode=OneWayToSource}">
    <fluent:RibbonGroupBox Header="Generate">
        <!-- Light placeholder content -->
        <fluent:Button Header="Load Reports"
                       Command="{Binding LoadReportsCommand}"
                       Visibility="{Binding ReportsLoaded, Converter={StaticResource InverseBoolToVis}}" />

        <!-- Actual content, loaded on demand -->
        <ItemsControl ItemsSource="{Binding ReportButtons}"
                      Visibility="{Binding ReportsLoaded, Converter={StaticResource BoolToVis}}" />
    </fluent:RibbonGroupBox>
</fluent:RibbonTabItem>
```

```csharp
// ViewModel
public bool IsReportsTabSelected
{
    set
    {
        if (value && !ReportsLoaded)
        {
            LoadReportsAsync();
        }
    }
}

private async void LoadReportsAsync()
{
    ReportButtons = await _reportService.GetReportButtonsAsync();
    ReportsLoaded = true;
}
```

### Contextual Tabs for On-Demand Content

Contextual tabs are naturally lazy - they only appear when needed:

```xml
<fluent:Ribbon.ContextualGroups>
    <fluent:RibbonContextualTabGroup x:Name="ChartTools"
                                      Header="Chart Tools"
                                      Visibility="Collapsed" />
</fluent:Ribbon.ContextualGroups>

<fluent:RibbonTabItem Header="Design" Group="{Binding ElementName=ChartTools}">
    <!-- Only created when ChartTools becomes visible -->
</fluent:RibbonTabItem>
```

---

## Icon Caching Strategies

### How ObjectToImageConverter Works

Fluent.Ribbon's `ObjectToImageConverter` handles icon loading and DPI scaling. It creates frozen `ImageSource` instances:

```csharp
// Source: ObjectToImageConverter.cs
private static ImageSource? GetAsFrozenIfPossible(ImageSource? imageSource)
{
    if (imageSource?.CanFreeze == true)
    {
        return (ImageSource)imageSource.GetAsFrozen();
    }
    return imageSource;
}
```

### Pre-freeze Icons in Resources

```xml
<Application.Resources>
    <BitmapImage x:Key="SaveIcon16" UriSource="/Icons/Save16.png"
                 PresentationOptions:Freeze="True" />
    <BitmapImage x:Key="SaveIcon32" UriSource="/Icons/Save32.png"
                 PresentationOptions:Freeze="True" />
</Application.Resources>
```

### Use DrawingImage for Vector Icons

Vector icons scale without quality loss and are typically lighter than bitmaps:

```xml
<DrawingImage x:Key="SaveIconVector" PresentationOptions:Freeze="True">
    <DrawingImage.Drawing>
        <GeometryDrawing Brush="#FF000000"
                         Geometry="M19,12v7H5v-7H3v7c0,1.1,0.9,2,2,2h14c1.1,0,2-0.9,2-2v-7H19z M13,12.67l2.59-2.58L17,11.5l-5,5l-5-5l1.41-1.41L11,12.67V3h2V12.67z" />
    </DrawingImage.Drawing>
</DrawingImage>
```

### Provide All Icon Sizes

Avoid runtime scaling by providing icons at all needed sizes:

```xml
<fluent:Button Header="Save"
               Icon="{StaticResource SaveIcon16}"
               MediumIcon="{StaticResource SaveIcon24}"
               LargeIcon="{StaticResource SaveIcon32}" />
```

### IconPresenter Behavior

The `IconPresenter` control selects the optimal icon based on current size:

```csharp
// Source: IconPresenter.cs
public object? GetOptimalIcon()
{
    return this.IconSize switch
    {
        IconSize.Small => this.SmallIcon ?? this.MediumIcon ?? this.LargeIcon,
        IconSize.Medium => this.MediumIcon ?? this.LargeIcon ?? this.SmallIcon,
        IconSize.Large => this.LargeIcon ?? this.MediumIcon ?? this.SmallIcon,
        _ => this.LargeIcon ?? this.MediumIcon ?? this.SmallIcon
    };
}
```

---

## Reducing Visual Tree Complexity

### Understand Control Depth

A single `fluent:Button` creates a visual tree approximately 15-20 elements deep. Multiply this by dozens of buttons across multiple tabs.

### Simplify Group Content

**Avoid:**
```xml
<fluent:RibbonGroupBox Header="Actions">
    <StackPanel>
        <fluent:Button Header="New" />
        <fluent:Button Header="Open" />
        <fluent:Button Header="Save" />
    </StackPanel>
</fluent:RibbonGroupBox>
```

**Prefer:**
```xml
<fluent:RibbonGroupBox Header="Actions">
    <fluent:Button Header="New" Size="Middle" />
    <fluent:Button Header="Open" Size="Middle" />
    <fluent:Button Header="Save" Size="Middle" />
</fluent:RibbonGroupBox>
```

The `RibbonGroupBox` already handles layout - don't add unnecessary containers.

### Use DropDownButton for Many Related Items

Instead of 10 individual buttons, use a dropdown:

```xml
<!-- Instead of 10 buttons -->
<fluent:DropDownButton Header="Insert" LargeIcon="{StaticResource InsertIcon}">
    <fluent:MenuItem Header="Table" />
    <fluent:MenuItem Header="Picture" />
    <fluent:MenuItem Header="Chart" />
    <!-- etc. -->
</fluent:DropDownButton>
```

### Collapse Unused Features

Disable unused control features:

```xml
<!-- Hide separator when not needed -->
<fluent:RibbonGroupBox Header="Edit" IsSeparatorVisible="False">

<!-- Hide launcher button when not used -->
<fluent:RibbonGroupBox Header="Font" IsLauncherVisible="False">

<!-- Disable QAT adding if not needed -->
<fluent:Button Header="Exit" CanAddToQuickAccessToolBar="False" />
```

---

## Avoiding Unnecessary Bindings

### Binding Performance Hierarchy

From fastest to slowest:
1. Static values (`Header="Save"`)
2. StaticResource (`Icon="{StaticResource SaveIcon}"`)
3. DynamicResource (`Foreground="{DynamicResource TextBrush}"`)
4. Simple binding (`Text="{Binding Name}"`)
5. Multi-binding with converter
6. Complex binding paths (`Text="{Binding Parent.Child.Grandchild.Name}"`)

### Use OneTime Bindings Where Possible

For values that don't change:

```xml
<fluent:Button Header="{Binding ButtonLabel, Mode=OneTime}"
               Icon="{Binding Icon, Mode=OneTime}" />
```

### Avoid Binding in DataTemplates for Gallery Items

Gallery items are created frequently. Keep templates simple:

```xml
<!-- Fast -->
<fluent:InRibbonGallery.ItemTemplate>
    <DataTemplate>
        <Border Background="{Binding Background}">
            <TextBlock Text="{Binding Name}" />
        </Border>
    </DataTemplate>
</fluent:InRibbonGallery.ItemTemplate>

<!-- Slow - multiple converters and complex bindings -->
<fluent:InRibbonGallery.ItemTemplate>
    <DataTemplate>
        <Border Background="{Binding Color, Converter={StaticResource ColorToBrush}}"
                BorderBrush="{Binding IsSelected, Converter={StaticResource BoolToBrush}}"
                Visibility="{Binding IsVisible, Converter={StaticResource BoolToVis}}">
            <StackPanel>
                <Image Source="{Binding Icon, Converter={StaticResource IconConverter}}" />
                <TextBlock Text="{Binding Name, StringFormat='Style: {0}'}" />
            </StackPanel>
        </Border>
    </DataTemplate>
</fluent:InRibbonGallery.ItemTemplate>
```

### Clear Bindings When Removing Controls

The source code shows proper cleanup patterns:

```csharp
// Source: GalleryPanel.cs
foreach (var galleryGroupContainer in this.galleryGroupContainers)
{
    BindingOperations.ClearAllBindings(galleryGroupContainer);
    this.visualCollection.Remove(galleryGroupContainer);
}
```

When dynamically removing ribbon controls, clear bindings:

```csharp
public void RemoveButton(fluent.Button button)
{
    BindingOperations.ClearAllBindings(button);
    groupBox.Items.Remove(button);
}
```

---

## Memory Management

### WeakReference Pattern for Toggle Button Groups

Fluent.Ribbon uses `WeakReference` for toggle button groups to prevent memory leaks:

```csharp
// Source: ToggleButtonHelper.cs
private static void Register(string groupName, IToggleButton toggleButton)
{
    // ...
    elements.Add(new WeakReference(toggleButton));
}

private static void PurgeDead(ArrayList elements, object? elementToRemove)
{
    var index = 0;
    while (index < elements.Count)
    {
        var target = ((WeakReference?)elements[index])?.Target;
        if (target is null || target == elementToRemove)
        {
            elements.RemoveAt(index);
        }
        else
        {
            ++index;
        }
    }
}
```

### Unsubscribe Events Properly

Controls subscribe to events in `OnLoaded` and unsubscribe in `OnUnloaded`:

```csharp
// Source: RibbonGroupBox.cs
private void OnLoaded(object sender, RoutedEventArgs e)
{
    this.SubscribeEvents();
}

private void OnUnloaded(object sender, RoutedEventArgs e)
{
    this.SetCurrentValue(IsDropDownOpenProperty, false);
    this.UnSubscribeEvents();
}

private void SubscribeEvents()
{
    // Always unsubscribe first to prevent double subscription
    this.UnSubscribeEvents();

    if (this.LauncherButton is not null)
    {
        this.LauncherButton.Click += this.OnDialogLauncherButtonClick;
    }
    // ...
}
```

**Follow this pattern in your code:**

```csharp
public class MyRibbonViewModel : IDisposable
{
    private readonly CompositeDisposable _subscriptions = new();

    public MyRibbonViewModel()
    {
        // Track subscriptions
        _subscriptions.Add(Observable.FromEventPattern<EventArgs>(
            h => _ribbon.IsMinimizedChanged += h,
            h => _ribbon.IsMinimizedChanged -= h)
            .Subscribe(_ => OnMinimizedChanged()));
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }
}
```

### Avoid Capturing in Lambdas

```csharp
// Bad - captures 'this' and 'heavyObject'
var heavyObject = LoadData();
button.Click += (s, e) => ProcessData(heavyObject);

// Better - weak event or explicit unsubscription
button.Click += OnButtonClick;

private void OnButtonClick(object sender, RoutedEventArgs e)
{
    var data = GetDataWhenNeeded();
    ProcessData(data);
}
```

### RenderTargetBitmap Memory

`InRibbonGallery` and `RibbonGroupBox` use `RenderTargetBitmap` for "snapping" (freezing visual state). This can consume significant memory:

```csharp
// Source: InRibbonGallery.cs
if (value && (int)this.ActualWidth > 0 && (int)this.ActualHeight > 0)
{
    var renderTargetBitmap = new RenderTargetBitmap(
        (int)this.galleryPanel.ActualWidth,
        (int)this.galleryPanel.ActualHeight,
        96, 96,
        PixelFormats.Pbgra32);
    renderTargetBitmap.Render(this.galleryPanel);
    this.snappedImage.Source = renderTargetBitmap;
}
```

Large galleries or frequent open/close cycles can accumulate memory. Consider:
- Limiting gallery visible item count
- Avoiding rapid open/close animations

---

## Startup Time Optimization

### Issue: Window Initialization Slowdown

From Changelog issue #995: "Window initialization slows down after upgrade to 9.0"

### Defer Non-Essential Content

```csharp
public partial class MainWindow : RibbonWindow
{
    public MainWindow()
    {
        InitializeComponent();

        // Load essential tab immediately
        LoadHomeTab();

        // Defer other tabs
        Dispatcher.BeginInvoke(DispatcherPriority.Background,
            new Action(LoadOtherTabs));
    }

    private void LoadOtherTabs()
    {
        // Load additional tabs after window is shown
    }
}
```

### Reduce Initial Tab Count

Show only 2-3 tabs initially, load others on demand:

```xml
<fluent:Ribbon>
    <fluent:RibbonTabItem Header="Home">
        <!-- Essential content -->
    </fluent:RibbonTabItem>

    <!-- These can be loaded later -->
    <fluent:RibbonTabItem Header="Insert"
                           Visibility="{Binding IsInitialized, Converter={StaticResource BoolToVis}}" />
</fluent:Ribbon>
```

### Pre-compile XAML Resources

Use XAML compilation and merge resources early:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Load Fluent.Ribbon theme first -->
            <fluent:FluentRibbonThemeResourceDictionary />

            <!-- Then your icons (pre-frozen) -->
            <ResourceDictionary Source="/Resources/Icons.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Measure Pass Optimization

From Changelog: "Performance of measuring of RibbonTabItem was improved by reducing measure calls. Most of the time there should only be one or two measure calls when switching tabs."

The key insight: **caching was more expensive than not caching**. Avoid premature optimization that adds overhead.

---

## Profiling Ribbon Performance

### Visual Studio Performance Profiler

1. Run with `Debug > Performance Profiler > CPU Usage`
2. Look for methods in `Fluent.` namespace
3. Key hot spots:
   - `MeasureOverride` methods
   - `ArrangeOverride` methods
   - `OnApplyTemplate` methods

### WPF Performance Suite

Use `WPF Performance Suite` (part of Windows SDK) to analyze:
- Visual tree depth
- Layout pass count
- Render time per frame

### Measure Pass Counter

Add diagnostic code to count measure passes:

```csharp
public class DiagnosticRibbonGroupBox : RibbonGroupBox
{
    private int _measureCount;

    protected override Size MeasureOverride(Size constraint)
    {
        _measureCount++;
        Debug.WriteLine($"GroupBox '{Header}' measured: {_measureCount} times");
        return base.MeasureOverride(constraint);
    }
}
```

### Timeline Recording

Use Visual Studio's XAML UI debugging:
1. Debug > Windows > Live Visual Tree
2. Check "Show runtime tools in XAML"
3. Use Timeline recording to see layout events

---

## When NOT to Use Certain Features

### Don't Use InRibbonGallery for Large Collections

`InRibbonGallery` creates all items upfront. For 100+ items, use:
- Regular `Gallery` in a `DropDownButton`
- Custom virtualized list in Backstage

### Don't Use Backstage for Simple Dialogs

Backstage creates a full `AdornerLayer` overlay. For simple operations:

```csharp
// Instead of opening Backstage
backstage.IsOpen = true;

// Use a regular dialog
var dialog = new OptionsDialog();
dialog.ShowDialog();
```

### Don't Animate Everything

Disable animations when not needed:

```xml
<fluent:Backstage AreAnimationsEnabled="False" />
```

### Don't Use Complex ReduceOrder

Simple `ReduceOrder` is faster:

```xml
<!-- Simple and efficient -->
ReduceOrder="Group1,Group2,Group3"

<!-- Complex - more calculations -->
ReduceOrder="Group1,Group2,(Group1),Group3,(Group2),(Group3),Group1,Group2"
```

### Don't Nest Containers Inside Groups

```xml
<!-- DON'T -->
<fluent:RibbonGroupBox>
    <StackPanel>
        <Grid>
            <fluent:Button />
        </Grid>
    </StackPanel>
</fluent:RibbonGroupBox>

<!-- DO -->
<fluent:RibbonGroupBox>
    <fluent:Button />
</fluent:RibbonGroupBox>
```

---

## DO NOT DO Section

### DON'T: Force Layout During Property Changes

```csharp
// WRONG - Forces immediate expensive layout
public bool ShowAdvanced
{
    set
    {
        advancedGroup.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        ribbon.InvalidateMeasure();
        ribbon.UpdateLayout(); // DON'T force this
    }
}
```

```csharp
// RIGHT - Let WPF batch layout updates
public bool ShowAdvanced
{
    set
    {
        advancedGroup.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        // WPF will handle layout automatically
    }
}
```

### DON'T: Recreate Items Frequently

```csharp
// WRONG - Recreates all items on every change
public void UpdateItems()
{
    gallery.Items.Clear();
    foreach (var item in _source)
    {
        gallery.Items.Add(new GalleryItem { Content = item });
    }
}
```

```csharp
// RIGHT - Use data binding with observable collection
public ObservableCollection<StyleItem> Styles { get; } = new();

// Update collection in place
public void UpdateItems()
{
    var toRemove = Styles.Except(_source).ToList();
    var toAdd = _source.Except(Styles).ToList();

    foreach (var item in toRemove) Styles.Remove(item);
    foreach (var item in toAdd) Styles.Add(item);
}
```

### DON'T: Bind Commands to Heavy Operations

```csharp
// WRONG - Blocks UI during CanExecute
public ICommand SaveCommand => new RelayCommand(
    execute: _ => Save(),
    canExecute: _ => ValidateAllDocuments() // Expensive!
);
```

```csharp
// RIGHT - Cache validation state
private bool _canSave;
public bool CanSave
{
    get => _canSave;
    set { _canSave = value; OnPropertyChanged(); }
}

// Update asynchronously
private async void OnDocumentChanged()
{
    CanSave = await ValidateAllDocumentsAsync();
}
```

### DON'T: Create New Brushes Without Freezing

```csharp
// WRONG - Creates unfrozen brush on every access
public Brush AccentBrush => new SolidColorBrush(Colors.Blue);
```

```csharp
// RIGHT - Create once, freeze, and reuse
private static readonly Brush _accentBrush;

static MyClass()
{
    _accentBrush = new SolidColorBrush(Colors.Blue);
    _accentBrush.Freeze();
}

public Brush AccentBrush => _accentBrush;
```

### DON'T: Use FindVisualChild in Hot Paths

```csharp
// WRONG - Expensive tree traversal
private void OnSelectionChanged()
{
    var textBlock = FindVisualChild<TextBlock>(ribbon);
    textBlock.Text = "Selected";
}
```

```csharp
// RIGHT - Use proper APIs or cache references
private void OnSelectionChanged()
{
    // Use the control's exposed properties
    ribbon.Title = "Selected";
}
```

### DON'T: Subscribe to Events Multiple Times

```csharp
// WRONG - Accumulates handlers
public void OnNavigatedTo()
{
    button.Click += HandleClick;
}
```

```csharp
// RIGHT - Unsubscribe first or use weak events
public void OnNavigatedTo()
{
    button.Click -= HandleClick;
    button.Click += HandleClick;
}

public void OnNavigatedFrom()
{
    button.Click -= HandleClick;
}
```

### DON'T: Modify Collections While Iterating

```csharp
// WRONG - Modifies collection during iteration
foreach (var item in ribbon.QuickAccessToolBar.Items)
{
    if (ShouldRemove(item))
        ribbon.QuickAccessToolBar.Items.Remove(item); // Throws!
}
```

```csharp
// RIGHT - Collect items first, then modify
var toRemove = ribbon.QuickAccessToolBar.Items
    .Cast<UIElement>()
    .Where(ShouldRemove)
    .ToList();

foreach (var item in toRemove)
{
    ribbon.QuickAccessToolBar.Items.Remove(item);
}
```

---

## Performance Checklist

Before release, verify:

- [ ] `ReduceOrder` defined for tabs with 3+ groups
- [ ] Icons provided at all sizes (16, 24, 32)
- [ ] Icons frozen in resources
- [ ] Gallery item count limited or paginated
- [ ] `ItemWidth`/`ItemHeight` specified for galleries
- [ ] No unnecessary containers in groups
- [ ] Events properly unsubscribed
- [ ] Complex bindings minimized in item templates
- [ ] Backstage animations disabled if not needed
- [ ] Contextual tabs used instead of hidden regular tabs
- [ ] Measure pass count verified (aim for 1-2 per tab switch)

---

## Summary

| Area | Key Optimization |
|------|------------------|
| Layout | Define `ReduceOrder`, use `StateDefinition` |
| Galleries | Limit items, set fixed sizes, suspend updates for batch changes |
| Icons | Pre-freeze, provide all sizes, use vector when possible |
| Bindings | Prefer `OneTime`, avoid complex paths in templates |
| Memory | Unsubscribe events, use WeakReference for long-lived references |
| Startup | Defer non-essential content, minimize initial tabs |

**Remember:** Profile before optimizing. The Fluent.Ribbon team found that their internal caching was often more expensive than recalculating. Measure first, optimize second.
