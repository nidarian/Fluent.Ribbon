---
title: Fluent.Ribbon Troubleshooting Guide
description: Diagnosing and fixing common Fluent.Ribbon issues
tags: [troubleshooting, debugging, issues, reference]
see_also:
  - fluent-ribbon-lessons-learned-v2.md
  - ../advanced/fluent-ribbon-performance-guide.md
  - ../getting-started/fluent-ribbon-migration-guide.md
---

# Fluent.Ribbon Troubleshooting Guide

**Comprehensive guide to diagnosing and fixing common Fluent.Ribbon issues.**

*Created: 2026-01-23*
*Format: Problem -> Cause -> Solution*

---

## FIRST: Run the Showcase App

**Before troubleshooting, see how things SHOULD work:**

```bash
# Clone or navigate to your local Fluent.Ribbon repository
# https://github.com/fluentribbon/Fluent.Ribbon
dotnet build Fluent.Ribbon.Showcase
dotnet run --project Fluent.Ribbon.Showcase
```

The Showcase app demonstrates every control working correctly. Compare your implementation against it.

---

## Table of Contents

1. [Theme Issues](#theme-issues)
2. [Icon Issues](#icon-issues)
3. [KeyTip Issues](#keytip-issues)
4. [Ribbon Minimization Issues](#ribbon-minimization-issues)
5. [Quick Access Toolbar (QAT) Issues](#quick-access-toolbar-qat-issues)
6. [Backstage Issues](#backstage-issues)
7. [Contextual Tab Issues](#contextual-tab-issues)
8. [Binding Errors](#binding-errors)
9. [Visual Tree Issues](#visual-tree-issues)
10. [Memory Leaks](#memory-leaks)
11. [High DPI Issues](#high-dpi-issues)
12. [Designer Errors](#designer-errors)
13. [Performance Issues](#performance-issues)

---

## Theme Issues

### Problem: Theme not applying to controls

**Symptoms:**
- Controls remain default color after calling `ChangeTheme()`
- Only some controls change color
- Theme works on first load but not after switching

**Cause:** Using `StaticResource` instead of `DynamicResource`

Fluent.Ribbon's internal templates use `StaticResource` for performance. This means resources are resolved once at load time, not dynamically.

**Solution:**

```csharp
// DON'T: Override brushes after window loads (won't update existing controls)
Application.Current.Resources["Fluent.Ribbon.Brushes.AccentBase"] = newBrush;

// DO: Use ThemeManager to swap the entire theme
ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Blue");

// Or generate a runtime theme with your color
var theme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme("Dark", myAccentColor, false);
ThemeManager.Current.ChangeTheme(Application.Current, theme);
```

**If you must override individual resources after load:**

```csharp
// Force all resources to reload (expensive but works)
var currentTheme = ThemeManager.Current.DetectTheme(Application.Current);
ThemeManager.Current.ChangeTheme(Application.Current, ThemeManager.Current.GetInverseTheme(currentTheme));
ThemeManager.Current.ChangeTheme(Application.Current, currentTheme);
```

---

### Problem: Theme only applies to some controls

**Symptoms:**
- Ribbon changes color but custom controls don't
- Some buttons change, others don't

**Cause:** Custom controls don't use Fluent.Ribbon brush resources

**Solution:**

```xml
<!-- DON'T: Hardcode colors -->
<Border Background="#2D2D30" />

<!-- DO: Reference Fluent.Ribbon brushes with DynamicResource -->
<Border Background="{DynamicResource Fluent.Ribbon.Brushes.RibbonWindow.Background}" />
```

For custom controls, bind to theme-aware resources:

```xml
<Style TargetType="local:MyCustomControl">
    <Setter Property="Background"
            Value="{DynamicResource Fluent.Ribbon.Brushes.RibbonTabControl.Content.Background}" />
    <Setter Property="Foreground"
            Value="{DynamicResource Fluent.Ribbon.Brushes.LabelText}" />
</Style>
```

---

### Problem: Theme changes don't persist after restart

**Cause:** Not saving/loading theme preference

**Solution:**

```csharp
// Save theme on change
ThemeManager.Current.ThemeChanged += (s, e) =>
{
    Settings.Default.ThemeName = e.NewTheme?.Name ?? "Light.Blue";
    Settings.Default.Save();
};

// Load theme on startup (App.xaml.cs)
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    var savedTheme = Settings.Default.ThemeName;
    if (!string.IsNullOrEmpty(savedTheme))
    {
        ThemeManager.Current.ChangeTheme(this, savedTheme);
    }
}
```

---

### Problem: Windows theme sync not working

**Symptoms:**
- App doesn't follow Windows light/dark mode
- Theme sync works initially but stops

**Cause:** `ThemeSyncMode` not set or event handler lost

**Solution:**

```csharp
// In App.xaml.cs OnStartup:
ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
ThemeManager.Current.SyncTheme();

// IMPORTANT: SyncTheme must be called AFTER setting ThemeSyncMode
```

---

## Icon Issues

### Problem: Icons not showing

**Symptoms:**
- Button shows text but no icon
- Icon area is blank or shows broken image

**Cause 1:** Incorrect pack URI format

```xml
<!-- WRONG: Missing assembly name -->
<fluent:Button Icon="/Images/save.png" />

<!-- WRONG: Forward slashes in wrong place -->
<fluent:Button Icon="pack://application:,,,Images/save.png" />

<!-- CORRECT: Full pack URI -->
<fluent:Button Icon="pack://application:,,,/MyAssembly;component/Images/save.png" />
```

**Cause 2:** Image build action not set correctly

**Solution:** In Visual Studio, select the image file and set:
- Build Action: `Resource`
- Copy to Output Directory: `Do not copy`

**Cause 3:** Wrong icon property for button size

```xml
<!-- For Small/Middle buttons, use Icon -->
<fluent:Button Size="Small" Icon="{StaticResource SmallIcon}" />

<!-- For Large buttons, also set LargeIcon -->
<fluent:Button Size="Large"
               Icon="{StaticResource SmallIcon}"
               LargeIcon="{StaticResource LargeIcon}" />
```

---

### Problem: Icons appear blurry

**Cause:** Icon size doesn't match control size, or DPI scaling issues

**Solution:**

```xml
<!-- Provide correctly sized icons for each size -->
<fluent:Button Header="Save"
               Icon="{StaticResource SaveIcon16}"           <!-- 16x16 for Small/Middle -->
               MediumIcon="{StaticResource SaveIcon24}"     <!-- 24x24 for Medium -->
               LargeIcon="{StaticResource SaveIcon32}" />   <!-- 32x32 for Large -->
```

For vector icons (recommended for DPI scaling):

```xml
<!-- Use DrawingImage for vector graphics -->
<DrawingImage x:Key="SaveIcon">
    <DrawingImage.Drawing>
        <GeometryDrawing Brush="{DynamicResource Fluent.Ribbon.Brushes.LabelText}"
                         Geometry="M19,12H22.5L17.5,7L12.5,12H16V20H19V12Z" />
    </DrawingImage.Drawing>
</DrawingImage>
```

---

### Problem: Icons show in designer but not at runtime

**Cause:** Design-time vs runtime resource resolution differs

**Solution:**

```xml
<!-- Use pack URI instead of relative path -->
<Image Source="pack://application:,,,/MyApp;component/Resources/icon.png" />

<!-- Or define in a ResourceDictionary that's merged at app level -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Resources/Icons.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

---

## KeyTip Issues

### Problem: KeyTips not showing when pressing Alt

**Symptoms:**
- Pressing Alt does nothing
- KeyTips flicker and disappear immediately

**Cause 1:** KeyTip keys not assigned

```xml
<!-- WRONG: Missing KeyTip -->
<fluent:RibbonTabItem Header="Home" />

<!-- CORRECT: KeyTip assigned -->
<fluent:RibbonTabItem Header="Home" KeyTip="H" />
```

**Cause 2:** Control not visible or not in visual tree

KeyTips only show for visible, enabled controls.

**Solution:** Check control visibility:

```csharp
// Debug: Check if control is in visual tree
var adornerLayer = AdornerLayer.GetAdornerLayer(myControl);
Debug.WriteLine($"AdornerLayer exists: {adornerLayer != null}");
Debug.WriteLine($"Control visible: {myControl.IsVisible}");
Debug.WriteLine($"Control enabled: {myControl.IsEnabled}");
```

**Cause 3:** Focus issue - another control is capturing keyboard

**Solution:**

```csharp
// Ensure ribbon can receive keyboard input
Keyboard.Focus(ribbon);
```

---

### Problem: KeyTips show wrong position

**Cause:** Control layout not complete when KeyTips calculated

**Solution:**

```csharp
// Force layout update before KeyTips
ribbon.UpdateLayout();
```

Or set explicit KeyTip position:

```xml
<fluent:Button Header="Save" KeyTip="S"
               fluent:KeyTip.AutoPlacement="False"
               fluent:KeyTip.HorizontalAlignment="Center"
               fluent:KeyTip.VerticalAlignment="Bottom" />
```

---

### Problem: KeyTips conflict (same key used twice)

**Symptoms:**
- Pressing a key selects wrong control
- Some KeyTips don't work

**Cause:** Duplicate KeyTip values in same scope

**Solution:** Use unique keys, or multi-character keys:

```xml
<fluent:Button Header="Save" KeyTip="S" />
<fluent:Button Header="Send" KeyTip="SE" />  <!-- Multi-character -->
<fluent:Button Header="Settings" KeyTip="ST" />
```

---

## Ribbon Minimization Issues

### Problem: Ribbon won't minimize

**Symptoms:**
- Double-clicking tab header does nothing
- Minimize button in context menu has no effect

**Cause 1:** `CanMinimize` set to false

```xml
<!-- This prevents minimization -->
<fluent:Ribbon CanMinimize="False" />

<!-- Allow minimization -->
<fluent:Ribbon CanMinimize="True" />
```

**Cause 2:** RibbonTabControl template issue

**Solution:** Check if you have a custom template that's missing the minimize trigger.

---

### Problem: Ribbon minimizes but content still shows

**Cause:** Custom content not bound to `IsMinimized` state

**Solution:**

```xml
<fluent:Ribbon x:Name="ribbon">
    <!-- Content should auto-hide based on IsMinimized -->
</fluent:Ribbon>

<!-- If using custom container, bind visibility -->
<Border Visibility="{Binding IsMinimized, ElementName=ribbon,
                     Converter={StaticResource InverseBoolToVisibility}}" />
```

---

### Problem: Ribbon auto-collapses unexpectedly

**Cause:** Window size falls below minimum thresholds

The ribbon auto-collapses when:
- Window width < 300 pixels (`Ribbon.MinimalVisibleWidth`)
- Window height < 250 pixels (`Ribbon.MinimalVisibleHeight`)

**Solution:**

```xml
<!-- Disable auto-collapse -->
<fluent:RibbonWindow IsAutomaticCollapseEnabled="False">
    ...
</fluent:RibbonWindow>

<!-- Or set minimum window size -->
<fluent:RibbonWindow MinWidth="400" MinHeight="300">
    ...
</fluent:RibbonWindow>
```

---

## Quick Access Toolbar (QAT) Issues

### Problem: QAT items not persisting after restart

**Cause:** `RibbonStateStorage` not called

**Solution:**

```csharp
// Save state on window closing
protected override void OnClosing(CancelEventArgs e)
{
    ribbon.RibbonStateStorage.Save();
    base.OnClosing(e);
}

// Load state on window loaded
private void OnLoaded(object sender, RoutedEventArgs e)
{
    ribbon.RibbonStateStorage.Load();
}
```

**Note:** Items must have unique `Name` or `QuickAccessToolBar` can't identify them:

```xml
<!-- Name is required for QAT persistence -->
<fluent:Button x:Name="SaveButton" Header="Save" />
```

---

### Problem: Can't add items to QAT via right-click

**Cause 1:** `CanAddToQuickAccessToolBar` set to false

```xml
<!-- This button can't be added to QAT -->
<fluent:Button Header="Exit" CanAddToQuickAccessToolBar="False" />
```

**Cause 2:** Default context menu disabled

```xml
<!-- This disables the QAT context menu -->
<fluent:Ribbon IsDefaultContextMenuEnabled="False" />
```

---

### Problem: QAT shows below ribbon but should be above (or vice versa)

**Solution:**

```csharp
// Move QAT above ribbon
ribbon.QuickAccessToolBar.ShowAboveRibbon = true;

// Move QAT below ribbon
ribbon.QuickAccessToolBar.ShowAboveRibbon = false;
```

```xml
<!-- Or in XAML -->
<fluent:Ribbon.QuickAccessToolBar>
    <fluent:QuickAccessToolBar ShowAboveRibbon="True" />
</fluent:Ribbon.QuickAccessToolBar>
```

---

### Problem: QAT dropdown menu not visible

**Cause:** `IsMenuDropDownVisible` set to false

```xml
<fluent:QuickAccessToolBar IsMenuDropDownVisible="True" />
```

---

## Backstage Issues

### Problem: Backstage not opening

**Symptoms:**
- Clicking File button does nothing
- `IsOpen = true` has no effect

**Cause 1:** No content defined

```csharp
// Backstage won't open if Content is null
// See Backstage.cs line 334-337
if (this.Content is null)
{
    return false;
}
```

**Solution:**

```xml
<fluent:Backstage>
    <fluent:BackstageTabControl>
        <fluent:BackstageTabItem Header="Info">
            <TextBlock Text="Backstage content" />
        </fluent:BackstageTabItem>
    </fluent:BackstageTabControl>
</fluent:Backstage>
```

**Cause 2:** `CanChangeIsOpen` set to false

```xml
<!-- This prevents opening/closing -->
<fluent:Backstage CanChangeIsOpen="False" />
```

**Cause 3:** Designer mode prevents opening

Backstage intentionally doesn't open in design mode. This is expected behavior.

---

### Problem: Backstage not closing

**Symptoms:**
- Escape key doesn't close backstage
- Close button has no effect

**Cause 1:** `CloseOnEsc` set to false

```xml
<fluent:Backstage CloseOnEsc="True" />  <!-- Default is True -->
```

**Cause 2:** `CanChangeIsOpen` blocking close

**Solution:**

```csharp
// Ensure CanChangeIsOpen is true before attempting close
backstage.CanChangeIsOpen = true;
backstage.IsOpen = false;
```

**Cause 3:** Event handler preventing close

Check if you have a handler on `IsOpenChanged` that's resetting the value.

---

### Problem: Backstage appears behind other content

**Cause:** AdornerLayer issue - happens when `AdornerDecorator` is missing

**Solution:**

```xml
<fluent:RibbonWindow>
    <AdornerDecorator>
        <Grid>
            <fluent:Ribbon>
                <fluent:Backstage>...</fluent:Backstage>
            </fluent:Ribbon>
            <!-- Other content -->
        </Grid>
    </AdornerDecorator>
</fluent:RibbonWindow>
```

---

### Problem: Controls behind backstage are still interactive

**Cause:** By design, Backstage collapses `HwndHost` elements (like `WindowsFormsHost`)

If you have custom elements that should be hidden:

```csharp
// Subscribe to backstage open/close
backstage.IsOpenChanged += (s, e) =>
{
    myControl.Visibility = backstage.IsOpen ? Visibility.Collapsed : Visibility.Visible;
};
```

---

## Contextual Tab Issues

### Problem: Contextual tabs not showing

**Symptoms:**
- Setting `Visibility = Visible` has no effect
- Tab group header doesn't appear

**Cause 1:** `RibbonContextualTabGroup` not defined in `Ribbon.ContextualGroups`

```xml
<fluent:Ribbon>
    <!-- REQUIRED: Define the group -->
    <fluent:Ribbon.ContextualGroups>
        <fluent:RibbonContextualTabGroup x:Name="PictureTools"
                                          Header="Picture Tools"
                                          Background="Purple"
                                          Visibility="Collapsed" />
    </fluent:Ribbon.ContextualGroups>

    <!-- Tab must reference the group -->
    <fluent:RibbonTabItem Header="Format"
                          Group="{Binding ElementName=PictureTools}">
        ...
    </fluent:RibbonTabItem>
</fluent:Ribbon>
```

**Cause 2:** Tab not associated with group

```xml
<!-- WRONG: Tab not linked to group -->
<fluent:RibbonTabItem Header="Format" />

<!-- CORRECT: Tab linked via Group property -->
<fluent:RibbonTabItem Header="Format" Group="{Binding ElementName=PictureTools}" />
```

**Cause 3:** All tabs in group are invisible

If all `RibbonTabItem` elements in a group have `Visibility="Collapsed"`, the group header won't show even if the group itself is visible.

---

### Problem: Contextual tab colors not showing

**Cause:** Background not set on `RibbonContextualTabGroup`

**Solution:**

```xml
<fluent:RibbonContextualTabGroup x:Name="TableTools"
                                  Header="Table Tools"
                                  Background="#FF7F50"  <!-- Must set color -->
                                  Visibility="Visible" />
```

---

### Problem: Contextual tabs appear but can't be selected

**Cause:** Tab is disabled or `InnerVisibility` issue

**Solution:**

```csharp
// Ensure tab is enabled
pictureFormatTab.IsEnabled = true;

// Force visibility update
pictureToolsGroup.Visibility = Visibility.Visible;
pictureToolsGroup.UpdateInnerVisiblityAndGroupBorders();
```

---

## Binding Errors

### Problem: Binding errors in Output window

**Symptoms:**
- Output shows "Cannot find source for binding"
- "Value 'null' is not valid for property"

**Common Causes and Solutions:**

**1. DataContext not set:**

```xml
<!-- Ensure DataContext is set -->
<fluent:RibbonWindow DataContext="{Binding MainViewModel}">
```

**2. Property path typo:**

```xml
<!-- Check property names match exactly -->
<fluent:Button Command="{Binding SaveComand}" />  <!-- Typo! -->
<fluent:Button Command="{Binding SaveCommand}" />  <!-- Correct -->
```

**3. Collection binding without ItemsSource:**

```xml
<!-- For dynamic items, use ItemsSource -->
<fluent:RibbonGroupBox Header="Tools" ItemsSource="{Binding ToolButtons}">
    <fluent:RibbonGroupBox.ItemTemplate>
        <DataTemplate>
            <fluent:Button Header="{Binding Name}" Command="{Binding Command}" />
        </DataTemplate>
    </fluent:RibbonGroupBox.ItemTemplate>
</fluent:RibbonGroupBox>
```

**4. Command parameter binding timing:**

```xml
<!-- CommandParameter binds before Command - use MultiBinding if needed -->
<fluent:Button Command="{Binding DeleteCommand}"
               CommandParameter="{Binding SelectedItem, ElementName=listView}" />
```

---

### Problem: "Value produced by BindingExpression is not valid"

**Cause:** Type mismatch between binding source and target

**Solution:**

```xml
<!-- Use converter for type conversion -->
<fluent:Button Visibility="{Binding IsEnabled,
                            Converter={StaticResource BoolToVisibilityConverter}}" />
```

---

## Visual Tree Issues

### Problem: Cannot find element by name

**Cause:** Element not yet loaded or in different namescope

**Solution:**

```csharp
// Wait for load
Loaded += (s, e) =>
{
    var element = FindName("MyElement");
};

// Or use visual tree helper
var element = FindVisualChild<Button>(ribbon, "SaveButton");

// Helper method
public static T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
{
    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
    {
        var child = VisualTreeHelper.GetChild(parent, i);
        if (child is T element && element.Name == name)
            return element;

        var result = FindVisualChild<T>(child, name);
        if (result != null)
            return result;
    }
    return null;
}
```

---

### Problem: Visual tree walking returns null

**Cause:** Templates not yet applied

**Solution:**

```csharp
// Force template application first
ribbon.ApplyTemplate();
tabControl.ApplyTemplate();

// Then walk tree
var panel = GetTemplateChild("PART_Panel") as Panel;
```

---

## DO NOT DO: Visual Tree Walking for Theming

**This is a common anti-pattern. DON'T do this:**

```csharp
// WRONG - Fragile, breaks between versions
foreach (var child in GetVisualChildren(ribbon))
{
    if (child.Name == "PART_xxx")
        child.Background = myBrush;
}
```

**Why it's wrong:**
- `PART_xxx` names are implementation details
- Template structure changes between versions
- Bypasses the resource system
- Doesn't integrate with ThemeManager

**DO: Use resources and ThemeManager instead** (see Theme Issues section)

---

## Memory Leaks

### Problem: Memory keeps growing, controls not garbage collected

**Cause 1:** Event handlers not unsubscribed

```csharp
// WRONG - Handler keeps control alive
someService.DataChanged += OnDataChanged;

// CORRECT - Unsubscribe in cleanup
protected override void OnUnloaded(object sender, RoutedEventArgs e)
{
    someService.DataChanged -= OnDataChanged;
}
```

**Cause 2:** Strong references in static collections

```csharp
// WRONG - Static holds reference forever
static List<RibbonButton> allButtons = new List<RibbonButton>();

// CORRECT - Use WeakReference
static List<WeakReference<RibbonButton>> allButtons = new();
```

**Cause 3:** Command bindings with closures

```csharp
// WRONG - Lambda captures 'this'
button.Command = new RelayCommand(() => this.Save());

// BETTER - Use weak event pattern or ensure cleanup
```

---

### Problem: Backstage not releasing memory

**Cause:** Adorner layer holds references

**Solution:** Backstage handles this internally via `DestroyAdorner()` when unloaded. If you see leaks:

```csharp
// Force cleanup on window close
protected override void OnClosed(EventArgs e)
{
    // Ensure backstage is closed and cleaned up
    if (backstage.IsOpen)
        backstage.IsOpen = false;

    base.OnClosed(e);
}
```

---

## High DPI Issues

### Problem: Controls appear blurry on high DPI displays

**Cause:** Bitmap images not designed for high DPI

**Solution:**

1. **Use vector graphics** (DrawingImage, PathGeometry)
2. **Provide multiple resolution images**
3. **Enable per-monitor DPI awareness**

```xml
<!-- App.manifest -->
<application xmlns="urn:schemas-microsoft-com:asm.v3">
    <windowsSettings>
        <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true/PM</dpiAware>
        <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
    </windowsSettings>
</application>
```

---

### Problem: Ribbon layout breaks on DPI change

**Cause:** Cached measurements become invalid

**Solution:**

```csharp
// Handle DPI change
protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
{
    base.OnDpiChanged(oldDpi, newDpi);

    // Force ribbon to re-measure
    ribbon.InvalidateMeasure();
    ribbon.InvalidateArrange();
    ribbon.UpdateLayout();
}
```

---

### Problem: Icons wrong size on high DPI

**Cause:** Fixed pixel sizes don't scale

**Solution:**

```xml
<!-- Use IconPresenter with multiple sizes -->
<fluent:Button>
    <fluent:Button.Icon>
        <fluent:IconPresenter SmallIcon="{StaticResource Icon16}"
                              MediumIcon="{StaticResource Icon24}"
                              LargeIcon="{StaticResource Icon32}" />
    </fluent:Button.Icon>
</fluent:Button>
```

---

## Designer Errors

### Problem: "Type 'Ribbon' was not found" in designer

**Cause:** NuGet package not restored or namespace not declared

**Solution:**

1. Restore NuGet packages: `dotnet restore`
2. Rebuild solution
3. Ensure namespace is declared:

```xml
<Window xmlns:fluent="urn:fluent-ribbon"
        ...>
```

---

### Problem: Designer crashes or shows blank

**Cause:** Design-time data issues or theme not available at design time

**Solution:**

```xml
<!-- Add design-time data context -->
<fluent:RibbonWindow d:DataContext="{d:DesignInstance Type=vm:MainViewModel, IsDesignTimeCreatable=True}">
```

Or ignore design-time issues:

```csharp
if (DesignerProperties.GetIsInDesignMode(this))
    return;
```

---

### Problem: "Object reference not set" in designer

**Cause:** Code runs in constructor that requires runtime resources

**Solution:**

```csharp
public MainWindow()
{
    InitializeComponent();

    // Guard design-time code
    if (!DesignerProperties.GetIsInDesignMode(this))
    {
        InitializeRibbon();
    }
}
```

---

## Performance Issues

### Problem: Ribbon loads slowly

**Cause 1:** Too many controls loaded at once

**Solution:** Use virtualization for large galleries:

```xml
<fluent:InRibbonGallery VirtualizingPanel.IsVirtualizing="True"
                         VirtualizingPanel.VirtualizationMode="Recycling" />
```

**Cause 2:** Large images loading synchronously

**Solution:**

```xml
<BitmapImage UriSource="{Binding ImagePath}"
             DecodePixelWidth="32"
             CacheOption="OnLoad" />
```

---

### Problem: Theme switching is slow

**Cause:** Many resources being regenerated

**Solution:**

```csharp
// Disable animations during theme switch
ThemeManager.Current.ChangeTheme(Application.Current, newTheme);

// Or batch multiple changes
Application.Current.Resources.BeginInit();
// ... make changes ...
Application.Current.Resources.EndInit();
```

---

### Problem: Ribbon flickers on resize

**Cause:** Frequent re-layout during resize

**Solution:**

```xml
<!-- Use RenderOptions to improve performance -->
<fluent:Ribbon RenderOptions.BitmapScalingMode="HighQuality"
               RenderOptions.EdgeMode="Aliased" />
```

---

## Quick Diagnostic Checklist

When troubleshooting Fluent.Ribbon issues:

1. **Run the Showcase app** - Does it work there?
2. **Check Output window** - Any binding errors?
3. **Verify NuGet packages** - Correct version installed?
4. **Check DataContext** - Is it set correctly?
5. **Verify namespaces** - `xmlns:fluent="urn:fluent-ribbon"`?
6. **Inspect visual tree** - Use Snoop or Live Visual Tree
7. **Check theme** - Is ThemeManager initialized?
8. **Review template parts** - Using `PART_` names correctly?
9. **Test in isolation** - Does issue occur in minimal repro?
10. **Check GitHub issues** - Known issue with workaround?

---

## Related Documentation

- `fluent-ribbon-common-tasks-v2.md` - How to do things correctly
- `fluent-ribbon-brushes-reference-v2.md` - All brush keys
- `controlzex-theming-reference-v2.md` - ThemeManager details
- `fluent-ribbon-lessons-learned-v2.md` - Anti-patterns to avoid

---

## Getting Help

1. **Search GitHub Issues:** https://github.com/fluentribbon/Fluent.Ribbon/issues
2. **Check Showcase app source:** Working examples of everything
3. **Read source code:** https://github.com/fluentribbon/Fluent.Ribbon
4. **Create minimal repro:** Isolate the issue before asking for help
