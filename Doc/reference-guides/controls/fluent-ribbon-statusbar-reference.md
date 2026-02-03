---
title: Fluent.Ribbon StatusBar Reference
description: Complete reference for the StatusBar control and all related components
tags: [statusbar, controls, window]
see_also:
  - fluent-ribbon-titlebar-reference.md
---

# Fluent.Ribbon StatusBar Reference

**Complete reference for the StatusBar control and all related components.**

---

## Overview

The StatusBar is a horizontal bar typically placed at the bottom of a window, displaying application state information and providing quick access to toggleable features. Fluent.Ribbon's StatusBar extends the WPF `StatusBar` with automatic context menu support for showing/hiding items.

```
+-----------------------------------------------------------------------+
| [Status Text]  [Word Count]  |      |  [Memory]  [Zoom: 100%] [====] |
+-----------------------------------------------------------------------+
          Left-aligned items           Right-aligned items
```

Key features:
- **Automatic context menu** - Right-click generates a menu to show/hide items
- **Left/right alignment** - Items automatically position based on `HorizontalAlignment`
- **Checkable items** - Users can toggle item visibility from context menu
- **Title/Value display** - Context menu shows item title with optional value

---

## Component Hierarchy

```
fluent:StatusBar
+-- Items (ItemCollection)
|   +-- fluent:StatusBarItem (individual items)
|   |   +-- Content (any UIElement or text)
|   |   +-- Title (shown in context menu)
|   |   +-- Value (shown in context menu)
|   |
|   +-- Separator (visual dividers)
|
+-- ContextMenu (auto-generated)
    +-- GroupSeparatorMenuItem ("Customize Status Bar" header)
    +-- fluent:StatusBarMenuItem (one per StatusBarItem)
    +-- Separator (one per StatusBar Separator)
```

Layout panel:
```
fluent:StatusBarPanel (internal ItemsPanel)
+-- leftChildren (HorizontalAlignment.Left items, arranged left-to-right)
+-- rightChildren (HorizontalAlignment.Right items, arranged right-to-left)
+-- otherChildren (other alignments, collapsed)
```

---

## fluent:StatusBar

The main container control. Extends `System.Windows.Controls.Primitives.StatusBar`.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `ItemCollection` | empty | Child StatusBarItem and Separator elements |
| `Background` | `Brush` | `AccentBase` | Background color (typically accent color) |
| `MinHeight` | `double` | `23` | Minimum height |
| `ContextMenu` | `ContextMenu` | auto-created | Auto-generated context menu for item visibility |

### Behavior

- **Auto-generated context menu**: The StatusBar automatically creates a context menu with an entry for each `StatusBarItem`. Users can check/uncheck items to show/hide them.
- **Separator visibility management**: Adjacent separators auto-collapse to prevent duplicates when items are hidden.
- **Container generation**: Non-`StatusBarItem` items are wrapped in `StatusBarItem` containers automatically.

### Example: Basic StatusBar

```xml
<fluent:StatusBar Grid.Row="2"
                  HorizontalAlignment="Stretch"
                  VerticalAlignment="Bottom">
    <fluent:StatusBarItem Title="Status"
                          HorizontalAlignment="Left"
                          Content="Ready" />

    <Separator HorizontalAlignment="Left" />

    <fluent:StatusBarItem Title="Page"
                          HorizontalAlignment="Left"
                          Content="Page 1 of 10"
                          Value="1/10" />

    <fluent:StatusBarItem Title="Zoom"
                          HorizontalAlignment="Right"
                          Content="100%"
                          Value="100%" />
</fluent:StatusBar>
```

---

## fluent:StatusBarItem

Individual item displayed in the StatusBar. Extends `System.Windows.Controls.Primitives.StatusBarItem`.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Title` | `string` | `null` | Text shown in context menu (left column) |
| `Value` | `string` | `null` | Value shown in context menu (right column). Falls back to Content if null. |
| `Content` | `object` | `null` | Actual content displayed in StatusBar |
| `IsCheckable` | `bool` | `true` | Whether item can be toggled via context menu |
| `IsChecked` | `bool` | `true` | Whether item is currently visible |
| `HorizontalAlignment` | `HorizontalAlignment` | `Stretch` | `Left` or `Right` for positioning |
| `IsEnabled` | `bool` | `true` | Enable/disable the item |

### Events

| Event | Description |
|-------|-------------|
| `Checked` | Fires when item becomes visible (IsChecked = true) |
| `Unchecked` | Fires when item becomes hidden (IsChecked = false) |

### Visibility Behavior

The `IsChecked` property controls visibility:
- When `IsChecked = true`: Item is `Visible`
- When `IsChecked = false`: Item is `Collapsed`
- The visibility coercion happens automatically via property coercion

### Content vs Value Fallback

If `Content` is null but `Value` is set, `Value` is used as content:
```xml
<!-- These are equivalent when Content is null -->
<fluent:StatusBarItem Title="Words" Value="150" />
<fluent:StatusBarItem Title="Words" Content="150" Value="150" />
```

### Example: Different Item Configurations

```xml
<!-- Text content with title for context menu -->
<fluent:StatusBarItem Title="Word Count"
                      HorizontalAlignment="Left"
                      Content="350 words"
                      Value="350" />

<!-- Complex content (slider) - not checkable -->
<fluent:StatusBarItem Title="Zoom Slider"
                      HorizontalAlignment="Right"
                      IsCheckable="False">
    <Slider Style="{DynamicResource Fluent.Ribbon.Styles.ZoomSlider}"
            Minimum="0.5" Maximum="2.0" Value="1.0" />
</fluent:StatusBarItem>

<!-- Disabled item -->
<fluent:StatusBarItem Title="Status"
                      HorizontalAlignment="Left"
                      IsEnabled="False"
                      Content="Offline" />

<!-- Initially hidden -->
<fluent:StatusBarItem Title="Debug Info"
                      HorizontalAlignment="Right"
                      IsChecked="False"
                      Content="Debug: ON" />
```

---

## fluent:StatusBarPanel

Layout panel that arranges StatusBar children. Used internally as the `ItemsPanel`.

### Layout Algorithm

1. **Sort children** by `HorizontalAlignment`:
   - `Left` items go to `leftChildren` list
   - `Right` items go to `rightChildren` list
   - Other alignments go to `otherChildren` (collapsed)

2. **Measure phase** (priority order):
   - Right-aligned items measured first (higher priority)
   - Left-aligned items measured next
   - If space runs out, remaining items get zero size

3. **Arrange phase**:
   - Right items arranged from right edge, stacking leftward
   - Left items arranged from left edge, stacking rightward
   - Items that didn't fit are arranged at (0,0,0,0) - collapsed

### Overflow Behavior

When the StatusBar is too narrow:
- Right-aligned items take priority (measured first)
- Left-aligned items overflow first (collapse from right side of left group)
- No visual indicator for overflow - items simply disappear

### Key Point: Alignment Matters

```xml
<!-- Will appear on LEFT side, left-to-right order -->
<fluent:StatusBarItem HorizontalAlignment="Left" Content="First" />
<fluent:StatusBarItem HorizontalAlignment="Left" Content="Second" />

<!-- Will appear on RIGHT side, maintaining order from right edge -->
<fluent:StatusBarItem HorizontalAlignment="Right" Content="RightMost" />
<fluent:StatusBarItem HorizontalAlignment="Right" Content="SecondRight" />
```

Result: `[First] [Second]                [SecondRight] [RightMost]`

---

## fluent:StatusBarMenuItem

Menu item in the auto-generated context menu. Links to a `StatusBarItem`.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `StatusBarItem` | `StatusBarItem` | The linked StatusBarItem |
| `IsCheckable` | `bool` | Bound to StatusBarItem.IsCheckable |
| `IsChecked` | `bool` | Two-way bound to StatusBarItem.IsChecked |

### Template Behavior

The menu item template displays:
- **Left column**: Checkmark icon when checked
- **Center column**: `StatusBarItem.Title`
- **Right column**: `StatusBarItem.Value`

```
+------------------------------------------+
| [Check] Title Text              Value    |
+------------------------------------------+
```

### Example Context Menu Appearance

For this StatusBar:
```xml
<fluent:StatusBar>
    <fluent:StatusBarItem Title="Word Count" Value="350" />
    <fluent:StatusBarItem Title="Page" Value="1/10" />
    <fluent:StatusBarItem Title="Zoom" Value="100%" />
</fluent:StatusBar>
```

Context menu shows:
```
+------------------------------+
| Customize Status Bar         |
+------------------------------+
| [v] Word Count         350   |
| [v] Page              1/10   |
| [v] Zoom              100%   |
+------------------------------+
```

---

## Left vs Right Alignment

### How It Works

The `StatusBarPanel` uses `HorizontalAlignment` to position items:

```xml
<fluent:StatusBar>
    <!-- These go LEFT -->
    <fluent:StatusBarItem HorizontalAlignment="Left" Content="Status: Ready" />
    <fluent:StatusBarItem HorizontalAlignment="Left" Content="Words: 350" />

    <!-- These go RIGHT (arranged right-to-left from edge) -->
    <fluent:StatusBarItem HorizontalAlignment="Right" Content="100%" />
    <fluent:StatusBarItem HorizontalAlignment="Right" Content="Ln 1, Col 1" />
</fluent:StatusBar>
```

Visual result:
```
+------------------------------------------------------------------+
| [Status: Ready] [Words: 350]              [Ln 1, Col 1] [100%]   |
+------------------------------------------------------------------+
  Left items (left-to-right)         Right items (right-to-left)
```

### Separator Alignment

Separators also need `HorizontalAlignment`:

```xml
<fluent:StatusBarItem HorizontalAlignment="Left" Content="A" />
<Separator HorizontalAlignment="Left" />
<fluent:StatusBarItem HorizontalAlignment="Left" Content="B" />

<fluent:StatusBarItem HorizontalAlignment="Right" Content="X" />
<Separator HorizontalAlignment="Right" />
<fluent:StatusBarItem HorizontalAlignment="Right" Content="Y" />
```

### Priority During Overflow

Right-aligned items have priority. When space is limited:
1. Right items are measured first - they get space
2. Left items get remaining space
3. Left items collapse from the right side of the left group

---

## Showing/Hiding Items via Context Menu

### Automatic Behavior

1. Right-click StatusBar opens context menu
2. Each `StatusBarItem` appears as checkable menu item
3. Unchecking hides the item (`IsChecked = false`)
4. Checking shows the item (`IsChecked = true`)

### Prevent Item from Being Toggled

Set `IsCheckable="False"` to prevent users from hiding an item:

```xml
<fluent:StatusBarItem Title="Critical Status"
                      IsCheckable="False"
                      Content="System OK" />
```

The item still appears in context menu but cannot be unchecked.

### Programmatic Show/Hide

```csharp
// Hide an item
myStatusBarItem.IsChecked = false;

// Show an item
myStatusBarItem.IsChecked = true;

// Listen for changes
myStatusBarItem.Checked += (s, e) => Console.WriteLine("Item shown");
myStatusBarItem.Unchecked += (s, e) => Console.WriteLine("Item hidden");
```

### Initially Hidden Items

```xml
<fluent:StatusBarItem Title="Debug Panel"
                      IsChecked="False"
                      Content="Debug info here" />
```

The item starts hidden but can be enabled via context menu.

---

## Styling and Theming

### Style Keys

| Key | Target | Description |
|-----|--------|-------------|
| `Fluent.Ribbon.Styles.StatusBar` | `fluent:StatusBar` | Main StatusBar style |
| `Fluent.Ribbon.Styles.RibbonStatusBarItem` | `fluent:StatusBarItem` | Item style |
| `Fluent.Ribbon.Styles.StatusBarMenuItem` | `fluent:StatusBarMenuItem` | Context menu item style |
| `Fluent.Ribbon.Templates.RibbonStatusBarContextMenuItem` | ControlTemplate | Menu item template |
| `Fluent.Ribbon.Styles.ZoomSlider` | `Slider` | Pre-styled zoom slider for StatusBar |

### Brush Resources

| Brush | Usage |
|-------|-------|
| `Fluent.Ribbon.Brushes.AccentBase` | StatusBar background |
| `Fluent.Ribbon.Brushes.IdealForeground` | StatusBarItem text |
| `Fluent.Ribbon.Brushes.IdealForegroundDisabled` | Disabled item text |
| `Fluent.Ribbon.Brushes.LabelText` | Context menu item text |
| `Fluent.Ribbon.Brushes.Button.MouseOver.Background` | Menu item hover |
| `Fluent.Ribbon.Brushes.Button.MouseOver.Border` | Menu item hover border |

### Separator Style

The StatusBar has a built-in separator style (`StatusBar.SeparatorStyleKey`):
- Width: 10px
- Transparent background and border (acts as spacer)

### Custom StatusBar Background

```xml
<fluent:StatusBar Background="{DynamicResource Fluent.Ribbon.Brushes.AccentBase}">
    <!-- Or custom brush -->
    <fluent:StatusBar.Background>
        <LinearGradientBrush StartPoint="0,0" EndPoint="0,1">
            <GradientStop Color="#FF0078D4" Offset="0"/>
            <GradientStop Color="#FF005A9E" Offset="1"/>
        </LinearGradientBrush>
    </fluent:StatusBar.Background>
</fluent:StatusBar>
```

### Resize Grip Accommodation

The StatusBar template automatically adds right margin when the window has a resize grip:

```xml
<!-- In template triggers -->
<DataTrigger Binding="{Binding ResizeMode, RelativeSource={RelativeSource AncestorType={x:Type Window}}}"
             Value="CanResizeWithGrip">
    <Setter TargetName="itemsPresenter" Property="Margin" Value="0 0 16 0" />
</DataTrigger>
```

---

## Common Patterns

### Zoom Slider

The most common right-side pattern - a slider for document zoom:

```xml
<fluent:StatusBarItem Title="Zoom"
                      HorizontalAlignment="Right"
                      Value="{Binding Value, ElementName=zoomSlider, StringFormat={}{0:P0}}">
    <TextBlock Text="{Binding Value, ElementName=zoomSlider, StringFormat={}{0:P0}}" />
</fluent:StatusBarItem>

<fluent:StatusBarItem Title="Zoom Slider"
                      HorizontalAlignment="Right"
                      IsCheckable="False">
    <Slider x:Name="zoomSlider"
            Style="{DynamicResource Fluent.Ribbon.Styles.ZoomSlider}"
            Minimum="0.5"
            Maximum="2.0"
            Value="{Binding Zoom}"
            SmallChange="0.1"
            LargeChange="0.1"
            TickFrequency="0.1"
            IsSnapToTickEnabled="True" />
</fluent:StatusBarItem>
```

### Page Information

```xml
<fluent:StatusBarItem Title="Page"
                      HorizontalAlignment="Left"
                      Value="{Binding CurrentPage, StringFormat=Page {0}}">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="Page " />
        <TextBlock Text="{Binding CurrentPage}" />
        <TextBlock Text=" of " />
        <TextBlock Text="{Binding TotalPages}" />
    </StackPanel>
</fluent:StatusBarItem>
```

### Status Text with Icon

```xml
<fluent:StatusBarItem Title="Status"
                      HorizontalAlignment="Left"
                      IsCheckable="False">
    <StackPanel Orientation="Horizontal">
        <Image Source="{StaticResource StatusIcon}" Width="16" Height="16" Margin="0,0,4,0" />
        <TextBlock Text="{Binding StatusText}" />
    </StackPanel>
</fluent:StatusBarItem>
```

### Memory/Performance Info

```xml
<fluent:StatusBarItem Title="Memory Usage"
                      HorizontalAlignment="Right"
                      Value="{Binding UsedMemory, StringFormat={}{0:##\,000} KB}" />
```

### Line/Column Position (Code Editors)

```xml
<fluent:StatusBarItem Title="Cursor Position"
                      HorizontalAlignment="Right"
                      Value="{Binding CursorPosition}">
    <TextBlock>
        <Run Text="Ln " />
        <Run Text="{Binding Line, Mode=OneWay}" />
        <Run Text=", Col " />
        <Run Text="{Binding Column, Mode=OneWay}" />
    </TextBlock>
</fluent:StatusBarItem>
```

### Toggle StatusBar Visibility

```xml
<!-- Checkbox elsewhere in UI -->
<fluent:CheckBox x:Name="ShowStatusBar" IsChecked="True">
    Show Status Bar
</fluent:CheckBox>

<!-- StatusBar with visibility binding -->
<fluent:StatusBar Visibility="{Binding IsChecked, ElementName=ShowStatusBar,
                              Converter={StaticResource BoolToVisibilityConverter}}" />
```

### Full StatusBar Example

```xml
<fluent:StatusBar Grid.Row="2"
                  HorizontalAlignment="Stretch"
                  VerticalAlignment="Bottom">
    <!-- Left side: Status and document info -->
    <fluent:StatusBarItem Title="Status"
                          HorizontalAlignment="Left"
                          IsCheckable="False"
                          Content="Ready" />

    <Separator HorizontalAlignment="Left" />

    <fluent:StatusBarItem Title="Word Count"
                          HorizontalAlignment="Left"
                          Content="{Binding WordCount, StringFormat={}{0} words}"
                          Value="{Binding WordCount}" />

    <Separator HorizontalAlignment="Left" />

    <fluent:StatusBarItem Title="Language"
                          HorizontalAlignment="Left"
                          Content="{Binding Language}"
                          Value="{Binding Language}" />

    <!-- Right side: View controls -->
    <fluent:StatusBarItem Title="Memory"
                          HorizontalAlignment="Right"
                          Content="{Binding MemoryMB, StringFormat={}{0:N0} MB}"
                          Value="{Binding MemoryMB, StringFormat={}{0:N0} MB}" />

    <Separator HorizontalAlignment="Right" />

    <fluent:StatusBarItem Title="Zoom Level"
                          HorizontalAlignment="Right"
                          Content="{Binding Zoom, StringFormat={}{0:P0}}"
                          Value="{Binding Zoom, StringFormat={}{0:P0}}" />

    <fluent:StatusBarItem Title="Zoom Slider"
                          HorizontalAlignment="Right"
                          IsCheckable="False">
        <Slider Style="{DynamicResource Fluent.Ribbon.Styles.ZoomSlider}"
                Value="{Binding Zoom}"
                Minimum="0.25" Maximum="4.0" />
    </fluent:StatusBarItem>
</fluent:StatusBar>
```

---

## Localization

### Localization Keys

| Property | Default (English) |
|----------|-------------------|
| `CustomizeStatusBar` | "Customize Status Bar" |

This string appears as the header in the context menu.

### Override Localization

```csharp
// Change the context menu header text
RibbonLocalization.Current.Localization.CustomizeStatusBar = "Status Bar Options";
```

### Available Translations

The `CustomizeStatusBar` string is localized in many languages including:
- German: "Statusleiste anpassen"
- French: "Personnaliser la barre de statut"
- Spanish: "Personalizar barra de estado"
- Japanese: "ステータス バーのユーザー設定"
- Chinese: "自定义状态栏"
- Russian: "Настройка строки состояния"

---

## DO NOT DO

### DON'T: Forget HorizontalAlignment

```xml
<!-- WRONG - Items won't position correctly -->
<fluent:StatusBarItem Title="Status" Content="Ready" />

<!-- RIGHT - Explicitly set alignment -->
<fluent:StatusBarItem Title="Status" HorizontalAlignment="Left" Content="Ready" />
```

Without `HorizontalAlignment`, items fall into "other" category and get collapsed.

### DON'T: Use Center or Stretch Alignment

```xml
<!-- WRONG - These alignments are collapsed by StatusBarPanel -->
<fluent:StatusBarItem HorizontalAlignment="Center" Content="Centered?" />
<fluent:StatusBarItem HorizontalAlignment="Stretch" Content="Stretched?" />

<!-- RIGHT - Only Left and Right work -->
<fluent:StatusBarItem HorizontalAlignment="Left" Content="On Left" />
<fluent:StatusBarItem HorizontalAlignment="Right" Content="On Right" />
```

### DON'T: Forget Separator Alignment

```xml
<!-- WRONG - Separator has no alignment, goes to "other" (collapsed) -->
<fluent:StatusBarItem HorizontalAlignment="Left" Content="A" />
<Separator />
<fluent:StatusBarItem HorizontalAlignment="Left" Content="B" />

<!-- RIGHT - Separator needs matching alignment -->
<fluent:StatusBarItem HorizontalAlignment="Left" Content="A" />
<Separator HorizontalAlignment="Left" />
<fluent:StatusBarItem HorizontalAlignment="Left" Content="B" />
```

### DON'T: Expect Overflow Indicators

```xml
<!-- StatusBarPanel has no overflow UI - items just disappear -->
<!-- If you need overflow handling, implement it yourself -->
```

### DON'T: Manually Manage ContextMenu Items

```csharp
// WRONG - Context menu is auto-generated and will be overwritten
statusBar.ContextMenu.Items.Add(new MenuItem { Header = "Custom" });

// RIGHT - Let StatusBar manage its own context menu
// Add custom functionality via StatusBarItem properties instead
```

### DON'T: Mix StatusBarItem with Standard Controls Expecting Layout

```xml
<!-- WRONG - Button won't respect StatusBar layout -->
<fluent:StatusBar>
    <Button Content="Click" />  <!-- Gets wrapped but no alignment -->
</fluent:StatusBar>

<!-- RIGHT - Use StatusBarItem wrapper with alignment -->
<fluent:StatusBar>
    <fluent:StatusBarItem HorizontalAlignment="Left">
        <Button Content="Click" />
    </fluent:StatusBarItem>
</fluent:StatusBar>
```

### DON'T: Set IsChecked on Non-Checkable Items

```xml
<!-- CONFUSING - IsChecked has no effect when IsCheckable is False -->
<fluent:StatusBarItem Title="Status"
                      IsCheckable="False"
                      IsChecked="False" />  <!-- Item will still show! -->

<!-- RIGHT - If you want it hidden, use Visibility directly -->
<fluent:StatusBarItem Title="Status"
                      Visibility="Collapsed" />
```

### DO: Use Appropriate Data Binding

```csharp
// RIGHT - Bind both Content and Value for context menu display
<fluent:StatusBarItem Title="Memory"
                      HorizontalAlignment="Right"
                      Content="{Binding MemoryMB, StringFormat={}{0} MB}"
                      Value="{Binding MemoryMB, StringFormat={}{0} MB}" />
```

---

## Source Files

| File | Purpose |
|------|---------|
| `Fluent.Ribbon\Controls\StatusBar.cs` | Main StatusBar control, context menu generation |
| `Fluent.Ribbon\Controls\StatusBarItem.cs` | Individual item with Title, Value, IsChecked |
| `Fluent.Ribbon\Controls\StatusBarPanel.cs` | Layout panel for left/right alignment |
| `Fluent.Ribbon\Controls\StatusBarMenuItem.cs` | Context menu item linked to StatusBarItem |
| `Fluent.Ribbon\Themes\Controls\StatusBar.xaml` | Styles and templates |
| `Fluent.Ribbon\Themes\Controls\Slider.xaml` | ZoomSlider style |
| `Fluent.Ribbon\Localization\RibbonLocalizationBase.cs` | CustomizeStatusBar localization |

---

## Summary

| Task | Solution |
|------|----------|
| Add left-aligned item | `<fluent:StatusBarItem HorizontalAlignment="Left" />` |
| Add right-aligned item | `<fluent:StatusBarItem HorizontalAlignment="Right" />` |
| Set context menu display | Use `Title` and `Value` properties |
| Prevent hiding | `IsCheckable="False"` |
| Start hidden | `IsChecked="False"` |
| Add separator | `<Separator HorizontalAlignment="Left"/>` (match item alignment) |
| Add zoom slider | Use `Fluent.Ribbon.Styles.ZoomSlider` style |
| Hide/show programmatically | Set `IsChecked` property |
| Listen for visibility changes | Subscribe to `Checked`/`Unchecked` events |
| Customize context menu header | Override `RibbonLocalization.Current.Localization.CustomizeStatusBar` |
| Toggle StatusBar visibility | Bind `Visibility` to a bool with converter |
