---
title: Fluent.Ribbon Button Controls Reference
description: Comprehensive reference for Button, DropDownButton, SplitButton, and ToggleButton controls
tags: [button, togglebutton, splitbutton, dropdownbutton, controls]
see_also:
  - fluent-ribbon-menu-controls-reference.md
  - fluent-ribbon-input-controls-reference.md
  - ../getting-started/fluent-ribbon-mvvm-patterns.md
---

# Fluent.Ribbon Button Controls Deep Dive Reference

**Comprehensive reference for Button, DropDownButton, SplitButton, and ToggleButton controls.**

*Based on Fluent.Ribbon source code analysis - Button.cs, DropDownButton.cs, SplitButton.cs, ToggleButton.cs*

---

## Table of Contents

1. [Control Inheritance Hierarchy](#control-inheritance-hierarchy)
2. [fluent:Button](#fluentbutton)
3. [fluent:ToggleButton](#fluenttogglebutton)
4. [fluent:DropDownButton](#fluentdropdownbutton)
5. [fluent:SplitButton](#fluentsplitbutton)
6. [Size Property Deep Dive](#size-property-deep-dive)
7. [Icon System](#icon-system)
8. [IsDefinitive Property](#isdefinitive-property)
9. [Keyboard Support](#keyboard-support)
10. [Events Reference](#events-reference)
11. [Quick Access Toolbar Integration](#quick-access-toolbar-integration)
12. [DO NOT DO Section](#do-not-do-section)
13. [Source File Reference](#source-file-reference)

---

## Control Inheritance Hierarchy

```
System.Windows.Controls.Button
└── Fluent.Button (IRibbonControl, IQuickAccessItemProvider, ILargeIconProvider,
                   IMediumIconProvider, ISimplifiedRibbonControl)

System.Windows.Controls.Primitives.ToggleButton
└── Fluent.ToggleButton (IToggleButton, IRibbonControl, IQuickAccessItemProvider,
                         ILargeIconProvider, IMediumIconProvider, ISimplifiedRibbonControl)

System.Windows.Controls.ItemsControl
└── Fluent.DropDownButton (IQuickAccessItemProvider, IRibbonControl, IDropDownControl,
                           ILargeIconProvider, IMediumIconProvider, ISimplifiedRibbonControl)
    └── Fluent.SplitButton (IToggleButton, ICommandSource, IKeyTipInformationProvider)
```

---

## fluent:Button

The simplest ribbon button control. Inherits from `System.Windows.Controls.Button` and adds ribbon-specific functionality.

### All Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | object | null | Button text/label (supports two-line display at Large size) |
| `HeaderTemplate` | DataTemplate | TwoLineLabel | Custom template for header content |
| `HeaderTemplateSelector` | DataTemplateSelector | null | Dynamic template selection |
| `Icon` | object | null | Small icon (16x16) for Small/Middle sizes |
| `MediumIcon` | object | null | Medium icon (24x24) for Simplified mode |
| `LargeIcon` | object | null | Large icon (32x32) for Large size |
| `Size` | RibbonControlSize | Large | Control size: Large, Middle, Small |
| `SizeDefinition` | RibbonControlSizeDefinition | null | Responsive sizing rules |
| `SimplifiedSizeDefinition` | RibbonControlSizeDefinition | null | Sizing rules for Simplified mode |
| `KeyTip` | string | null | Keyboard accelerator letter(s) |
| `IsDefinitive` | bool | true | Close popups/backstage when clicked |
| `IsSimplified` | bool | false | (read-only) Currently in Simplified ribbon mode |
| `CanAddToQuickAccessToolBar` | bool | true | Allow adding to QAT |
| `Command` | ICommand | null | (inherited) Click command |
| `CommandParameter` | object | null | (inherited) Command parameter |
| `CommandTarget` | IInputElement | null | (inherited) Command target |
| `ToolTip` | object | null | (inherited) Tooltip or ScreenTip |

### Basic Usage

```xml
<fluent:Button Header="Save"
               Size="Large"
               LargeIcon="{StaticResource SaveIcon32}"
               Icon="{StaticResource SaveIcon16}"
               Command="{Binding SaveCommand}"
               KeyTip="S" />
```

### XAML Template Parts

| Part Name | Type | Description |
|-----------|------|-------------|
| `PART_HeaderContentHost` | ContentControl | Hosts the Header content |

### Visual States by Size

| Size | Height | Layout | Icon Used | Header Visible |
|------|--------|--------|-----------|----------------|
| Large | 68px | Vertical | LargeIcon | Yes (two-line) |
| Middle | 22px | Horizontal | Icon (small) | Yes (single-line) |
| Small | 22px | Horizontal | Icon (small) | No |

---

## fluent:ToggleButton

A checkable button that maintains pressed state. Supports radio button behavior via GroupName.

### All Properties

All Button properties, plus:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IsChecked` | bool? | false | Toggle state (supports three-state) |
| `GroupName` | string | null | Radio group name for mutual exclusion |

### Events

| Event | Description |
|-------|-------------|
| `Checked` | Fired when IsChecked becomes true |
| `Unchecked` | Fired when IsChecked becomes false |
| `Indeterminate` | Fired when IsChecked becomes null |
| `Click` | Fired on every click |

### Basic Usage

```xml
<!-- Standard toggle button -->
<fluent:ToggleButton Header="Bold"
                      Size="Small"
                      Icon="{StaticResource BoldIcon}"
                      IsChecked="{Binding IsBold}"
                      KeyTip="B" />
```

### Radio Button Behavior with GroupName

When `GroupName` is set, ToggleButtons in the same group behave like radio buttons - only one can be checked at a time.

```xml
<fluent:RibbonGroupBox Header="Alignment">
    <fluent:ToggleButton Header="Left" GroupName="Align"
                          Icon="{StaticResource AlignLeftIcon}"
                          IsChecked="{Binding IsLeftAligned}" />
    <fluent:ToggleButton Header="Center" GroupName="Align"
                          Icon="{StaticResource AlignCenterIcon}"
                          IsChecked="{Binding IsCenterAligned}" />
    <fluent:ToggleButton Header="Right" GroupName="Align"
                          Icon="{StaticResource AlignRightIcon}"
                          IsChecked="{Binding IsRightAligned}" />
</fluent:RibbonGroupBox>
```

**Important GroupName Behavior:**
- When a button in the group is checked, it automatically unchecks others in the same group
- A checked button in a group cannot be unchecked by clicking it again (only by checking another button)
- The Click event and Command still fire even when clicking an already-checked button
- GroupName works across the entire visual tree, not just within one container

### Source Code Insight - GroupName Click Handling

From `ToggleButton.cs` (lines 242-272):

```csharp
protected override void OnClick()
{
    if (this.IsDefinitive)
    {
        PopupService.RaiseDismissPopupEvent(this, DismissPopupMode.Always);
    }

    if (string.IsNullOrEmpty(this.GroupName) == false)
    {
        // Only forward click if button is not checked to prevent wrong bound values
        if (this.IsChecked == false)
        {
            base.OnClick();
        }
        else
        {
            // Still raise Click event and execute command for already-checked button
            var newEvent = new RoutedEventArgs(ClickEvent, this);
            this.RaiseEvent(newEvent);
            this.ExecuteCommand();
        }
    }
    else
    {
        base.OnClick();
    }
}
```

---

## fluent:DropDownButton

A button that displays a dropdown menu when clicked. Inherits from `ItemsControl` to host menu items.

### All Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Display Properties** |
| `Header` | object | null | Button text |
| `HeaderTemplate` | DataTemplate | TwoLineLabel | Header template |
| `HeaderTemplateSelector` | DataTemplateSelector | null | Dynamic template selection |
| `Icon` | object | null | Small icon (16x16) |
| `MediumIcon` | object | null | Medium icon (24x24) |
| `LargeIcon` | object | null | Large icon (32x32) |
| `HasTriangle` | bool | true | Show dropdown arrow indicator |
| **Size Properties** |
| `Size` | RibbonControlSize | Large | Control size |
| `SizeDefinition` | RibbonControlSizeDefinition | null | Responsive sizing |
| `SimplifiedSizeDefinition` | RibbonControlSizeDefinition | null | Simplified mode sizing |
| `IsSimplified` | bool | false | (read-only) Simplified mode active |
| **Dropdown State** |
| `IsDropDownOpen` | bool | false | Dropdown open/closed state |
| `DropDownPopup` | Popup | null | (read-only) The popup control |
| `IsContextMenuOpened` | bool | false | Context menu open state |
| **Dropdown Sizing** |
| `MaxDropDownHeight` | double | NaN | Maximum dropdown height |
| `DropDownHeight` | double | NaN | Fixed dropdown height |
| `ResizeMode` | ContextMenuResizeMode | None | User resizing capability |
| **Behavior** |
| `DismissOnClickOutside` | bool | true | Close when clicking outside |
| `ClosePopupOnMouseDown` | bool | false | Close on any mouse down in popup |
| `ClosePopupOnMouseDownDelay` | int | 150 | Delay (ms) before closing (min 100) |
| **Other** |
| `KeyTip` | string | null | Keyboard accelerator |
| `CanAddToQuickAccessToolBar` | bool | true | Allow adding to QAT |
| `Items` | ItemCollection | - | (inherited) Dropdown items |
| `ItemsSource` | IEnumerable | null | (inherited) Data binding source |
| `ItemTemplate` | DataTemplate | null | (inherited) Item template |
| `ItemContainerTemplateSelector` | ItemContainerTemplateSelector | null | Container template selector |
| `UsesItemContainerTemplate` | bool | false | Enable container template selection |

### Template Parts

| Part Name | Type | Description |
|-----------|------|-------------|
| `PART_Popup` | Popup | The dropdown popup |
| `PART_PopupContentControl` | ResizeableContentControl | Resizable content wrapper |
| `PART_ButtonBorder` | UIElement | Clickable button area |

### ContextMenuResizeMode Enum

```csharp
public enum ContextMenuResizeMode
{
    None = 0,      // Cannot be resized
    Vertical,      // Can resize height only
    Both           // Can resize width and height
}
```

### IsDropDownOpen Property

Controls whether the dropdown menu is visible.

```xml
<!-- One-way binding (display only) -->
<fluent:DropDownButton Header="Options"
                        IsDropDownOpen="{Binding IsMenuOpen, Mode=OneWay}" />

<!-- Two-way binding (control can open/close from code) -->
<fluent:DropDownButton x:Name="myDropDown"
                        Header="Options"
                        IsDropDownOpen="{Binding IsMenuOpen, Mode=TwoWay}" />
```

**Programmatic Control:**
```csharp
// Open dropdown
myDropDown.IsDropDownOpen = true;

// Close dropdown
myDropDown.IsDropDownOpen = false;
```

### Dropdown Content Examples

```xml
<!-- Basic menu items -->
<fluent:DropDownButton Header="New" LargeIcon="{StaticResource NewIcon}">
    <fluent:MenuItem Header="Document" Command="{Binding NewDocumentCommand}" />
    <fluent:MenuItem Header="Folder" Command="{Binding NewFolderCommand}" />
    <Separator />
    <fluent:MenuItem Header="From Template..." Command="{Binding NewFromTemplateCommand}" />
</fluent:DropDownButton>

<!-- With resizable dropdown -->
<fluent:DropDownButton Header="Recent Files"
                        ResizeMode="Vertical"
                        MaxDropDownHeight="400"
                        DropDownHeight="200">
    <fluent:MenuItem Header="Document1.txt" />
    <fluent:MenuItem Header="Document2.txt" />
    <!-- More items... -->
</fluent:DropDownButton>

<!-- Data-bound items -->
<fluent:DropDownButton Header="Recent"
                        ItemsSource="{Binding RecentFiles}"
                        MaxDropDownHeight="300">
    <fluent:DropDownButton.ItemTemplate>
        <DataTemplate>
            <fluent:MenuItem Header="{Binding FileName}"
                             Command="{Binding DataContext.OpenFileCommand,
                                       RelativeSource={RelativeSource AncestorType=fluent:DropDownButton}}"
                             CommandParameter="{Binding}" />
        </DataTemplate>
    </fluent:DropDownButton.ItemTemplate>
</fluent:DropDownButton>
```

### DismissOnClickOutside vs ClosePopupOnMouseDown

| Property | When to Use |
|----------|-------------|
| `DismissOnClickOutside="False"` | Dropdown should stay open until explicitly closed (e.g., color picker, complex editor) |
| `ClosePopupOnMouseDown="True"` | Close immediately on any click inside the popup (e.g., gallery selection) |

```xml
<!-- Dropdown that stays open until user clicks outside -->
<fluent:DropDownButton Header="Color Picker"
                        DismissOnClickOutside="True"
                        ClosePopupOnMouseDown="False">
    <!-- Color selection controls that don't auto-close -->
</fluent:DropDownButton>

<!-- Dropdown that closes on any selection -->
<fluent:DropDownButton Header="Quick Actions"
                        ClosePopupOnMouseDown="True"
                        ClosePopupOnMouseDownDelay="100">
    <!-- Items that should immediately close dropdown -->
</fluent:DropDownButton>
```

---

## fluent:SplitButton

Combines a clickable button with a dropdown arrow. The button area and dropdown arrow are separate click targets.

### Inheritance

`SplitButton` inherits from `DropDownButton` and adds:
- Primary button click functionality (separate from dropdown)
- Command binding for primary action
- Toggle/check capability
- Dual KeyTip support

### All Properties

All DropDownButton properties, plus:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Primary Button** |
| `Command` | ICommand | null | Primary button command |
| `CommandParameter` | object | null | Command parameter |
| `CommandTarget` | IInputElement | null | Command target |
| `IsButtonEnabled` | bool | true | Enable/disable button part only |
| `IsDefinitive` | bool | true | Close popups on button click |
| `DropDownToolTip` | object | null | Separate tooltip for dropdown arrow |
| **Toggle Behavior** |
| `IsCheckable` | bool | false | Enable toggle mode |
| `IsChecked` | bool? | false | Toggle state (when IsCheckable=true) |
| `GroupName` | string | null | Radio group for mutual exclusion |
| **KeyTip Configuration** |
| `PrimaryActionKeyTipPostfix` | string | "A" | Suffix for primary action KeyTip |
| `SecondaryActionKeyTipPostfix` | string | "B" | Suffix for dropdown KeyTip |
| `SecondaryKeyTip` | string | "" | Explicit secondary KeyTip (overrides postfix) |
| **Quick Access** |
| `CanAddButtonToQuickAccessToolBar` | bool | true | Allow button part in QAT |

### Template Parts

| Part Name | Type | Description |
|-----------|------|-------------|
| `PART_Button` | ToggleButton | The primary button area |
| `PART_Popup` | Popup | The dropdown popup |
| `PART_PopupContentControl` | ResizeableContentControl | Resizable content wrapper |

### Events

| Event | Description |
|-------|-------------|
| `Click` | Fired when primary button is clicked |
| `Checked` | Fired when IsChecked becomes true (if IsCheckable) |
| `Unchecked` | Fired when IsChecked becomes false (if IsCheckable) |
| `Indeterminate` | Fired when IsChecked becomes null (if IsCheckable) |
| `DropDownOpened` | (inherited) Dropdown opened |
| `DropDownClosed` | (inherited) Dropdown closed |

### ButtonClick vs DropDown Behavior

The key feature of SplitButton is separating the primary action from the dropdown:

```
┌─────────────────────────────────────┐
│  ┌───────────────────┐  ┌────────┐ │
│  │   BUTTON AREA     │  │   ▼    │ │
│  │   (Click event)   │  │ (menu) │ │
│  └───────────────────┘  └────────┘ │
└─────────────────────────────────────┘
```

- **Button Area**: Triggers `Click` event and `Command`
- **Arrow Area**: Opens/closes the dropdown menu

### Basic Usage

```xml
<!-- Simple SplitButton -->
<fluent:SplitButton Header="Save"
                     Size="Large"
                     LargeIcon="{StaticResource SaveIcon}"
                     Command="{Binding SaveCommand}"
                     KeyTip="S">
    <fluent:MenuItem Header="Save As..." Command="{Binding SaveAsCommand}" />
    <fluent:MenuItem Header="Save All" Command="{Binding SaveAllCommand}" />
    <Separator />
    <fluent:MenuItem Header="Export..." Command="{Binding ExportCommand}" />
</fluent:SplitButton>
```

### Checkable SplitButton

```xml
<!-- Toggle button with dropdown options -->
<fluent:SplitButton Header="Highlight"
                     Size="Large"
                     LargeIcon="{StaticResource HighlightIcon}"
                     IsCheckable="True"
                     IsChecked="{Binding IsHighlightEnabled}"
                     Command="{Binding ToggleHighlightCommand}">
    <fluent:ColorGallery SelectedColor="{Binding HighlightColor}" />
</fluent:SplitButton>
```

### Separate Tooltips

```xml
<fluent:SplitButton Header="Paste"
                     ToolTip="Paste from clipboard (Ctrl+V)"
                     DropDownToolTip="Paste options">
    <fluent:MenuItem Header="Paste Special..." />
    <fluent:MenuItem Header="Paste as Text" />
</fluent:SplitButton>
```

### Disabling Button vs Entire Control

```xml
<!-- Disable only the button, dropdown still works -->
<fluent:SplitButton Header="Print"
                     IsButtonEnabled="{Binding CanPrint}"
                     Command="{Binding PrintCommand}">
    <fluent:MenuItem Header="Print Preview" IsEnabled="True" />
    <fluent:MenuItem Header="Print Setup" IsEnabled="True" />
</fluent:SplitButton>

<!-- Disable entire control -->
<fluent:SplitButton Header="Print"
                     IsEnabled="{Binding HasDocument}"
                     Command="{Binding PrintCommand}">
    <!-- All items also disabled -->
</fluent:SplitButton>
```

### KeyTip Behavior

SplitButton generates two KeyTips - one for the button, one for the dropdown:

```xml
<!-- KeyTip "S" becomes "SA" for button, "SB" for dropdown -->
<fluent:SplitButton Header="Save" KeyTip="S" />

<!-- Custom postfixes -->
<fluent:SplitButton Header="Save"
                     KeyTip="S"
                     PrimaryActionKeyTipPostfix="1"
                     SecondaryActionKeyTipPostfix="2" />
<!-- Results in S1 for button, S2 for dropdown -->

<!-- Explicit secondary KeyTip (no postfix) -->
<fluent:SplitButton Header="Save"
                     KeyTip="S"
                     SecondaryKeyTip="D" />
<!-- Results in S for button, D for dropdown -->
```

---

## Size Property Deep Dive

The `Size` property controls the visual appearance and layout of button controls.

### RibbonControlSize Enum

```csharp
public enum RibbonControlSize
{
    Large = 0,   // 68px height, vertical layout, large icon
    Middle,      // 22px height, horizontal layout, small icon, text visible
    Small        // 22px height, horizontal layout, small icon, no text
}
```

### Visual Comparison

```
LARGE (68px):          MIDDLE (22px):           SMALL (22px):
┌─────────────┐        ┌──────────────────┐     ┌────┐
│   [ICON]    │        │ [icon] Save File │     │[ic]│
│    32x32    │        └──────────────────┘     └────┘
│             │
│  Save File  │
│   (2 lines) │
└─────────────┘
```

### Icon Size by Control Size

| Control Size | IconSize Property | Icon Used | Typical Dimensions |
|--------------|-------------------|-----------|-------------------|
| Large | Large | LargeIcon | 32x32 |
| Middle | Small | Icon | 16x16 |
| Small | Small | Icon | 16x16 |
| Simplified + Large | Medium | MediumIcon | 24x24 |
| Simplified + Middle | Medium | MediumIcon | 24x24 |
| Simplified + Small | Small | Icon | 16x16 |

### Header Visibility by Size

| Size | Header Visible | Header Layout |
|------|----------------|---------------|
| Large | Yes | Two-line (via TwoLineLabel) |
| Middle | Yes | Single line |
| Small | No | Hidden |
| Simplified + Large | Yes | Single line |
| Simplified + Middle | No | Hidden |
| Simplified + Small | No | Hidden |

### Forcing Size with Attached Property

```xml
<!-- Force small size regardless of group state -->
<fluent:Button Header="Options"
               fluent:RibbonProperties.Size="Small"
               Icon="{StaticResource OptionsIcon}" />
```

### SizeDefinition for Responsive Sizing

```xml
<!-- Custom size rules based on available space -->
<fluent:Button Header="Format"
               SizeDefinition="Large, Middle, Small"
               LargeIcon="{StaticResource FormatIcon32}"
               Icon="{StaticResource FormatIcon16}" />
```

---

## Icon System

Fluent.Ribbon supports three icon sizes that are automatically selected based on control size and mode.

### Icon Properties

| Property | Size | When Used |
|----------|------|-----------|
| `Icon` | 16x16 (Small) | Size=Middle, Size=Small, Simplified+Small |
| `MediumIcon` | 24x24 (Medium) | Simplified mode (Large/Middle) |
| `LargeIcon` | 32x32 (Large) | Size=Large (normal mode) |

### Icon Selection Priority

The `IconPresenter` control selects icons in this order:

1. If explicit `IconSize` matches available icon, use it
2. Fall back to next best available icon
3. If no icon available, show placeholder border (in Simplified mode)

### Providing All Icon Sizes

```xml
<fluent:Button Header="Save"
               Icon="{StaticResource SaveIcon16}"
               MediumIcon="{StaticResource SaveIcon24}"
               LargeIcon="{StaticResource SaveIcon32}" />
```

### Using Same Icon for All Sizes

```xml
<!-- Vector icons scale well -->
<fluent:Button Header="Save">
    <fluent:Button.Icon>
        <Path Data="{StaticResource SaveGeometry}"
              Fill="{DynamicResource Fluent.Ribbon.Brushes.LabelText}" />
    </fluent:Button.Icon>
    <fluent:Button.LargeIcon>
        <Path Data="{StaticResource SaveGeometry}"
              Fill="{DynamicResource Fluent.Ribbon.Brushes.LabelText}" />
    </fluent:Button.LargeIcon>
</fluent:Button>
```

### Custom Icon Sizes

```xml
<fluent:Button Header="Custom"
               fluent:RibbonProperties.CustomIconSize="48,48">
    <fluent:Button.LargeIcon>
        <Image Source="custom48.png" />
    </fluent:Button.LargeIcon>
</fluent:Button>
```

---

## IsDefinitive Property

Controls whether clicking the button closes open popups, dropdowns, and the Backstage.

### Behavior

| IsDefinitive | Behavior |
|--------------|----------|
| `true` (default) | Click dismisses all open popups in the hierarchy |
| `false` | Click does NOT close popups - they stay open |

### When to Use IsDefinitive="False"

- Buttons inside dropdowns that shouldn't close the dropdown
- Toggle buttons in galleries
- Increment/decrement buttons
- Buttons that open dialogs (let dialog close the popup instead)

### Examples

```xml
<!-- Default: closes popup when clicked -->
<fluent:DropDownButton Header="Options">
    <fluent:Button Header="Apply" Command="{Binding ApplyCommand}" />
    <!-- Clicking Apply closes the dropdown -->
</fluent:DropDownButton>

<!-- Keep dropdown open -->
<fluent:DropDownButton Header="Settings">
    <fluent:ToggleButton Header="Option 1"
                          IsDefinitive="False"
                          IsChecked="{Binding Option1}" />
    <fluent:ToggleButton Header="Option 2"
                          IsDefinitive="False"
                          IsChecked="{Binding Option2}" />
    <Separator />
    <fluent:Button Header="Done" IsDefinitive="True" />
    <!-- Toggle buttons don't close; Done button does -->
</fluent:DropDownButton>
```

### Source Code Implementation

From `Button.cs` (lines 227-236):

```csharp
protected override void OnClick()
{
    // Close popup on click
    if (this.IsDefinitive)
    {
        PopupService.RaiseDismissPopupEvent(this, DismissPopupMode.Always);
    }

    base.OnClick();
}
```

---

## Keyboard Support

### KeyTip Navigation

All button controls support KeyTip for keyboard navigation:

```xml
<fluent:Button Header="Save" KeyTip="S" />
<fluent:DropDownButton Header="Format" KeyTip="O" />
<fluent:SplitButton Header="Paste" KeyTip="V" />
```

### Built-in Keyboard Handling

#### Button & ToggleButton
- `Enter` or `Space`: Trigger click

#### DropDownButton
- `Enter` or `Space`: Toggle dropdown open/closed
- `Down Arrow`: Open dropdown and focus first item
- `Up Arrow`: Open dropdown and focus last item
- `Escape`: Close dropdown

#### SplitButton
- `Enter`: Click the primary button (not dropdown)
- `Down Arrow`: Open dropdown
- `Up Arrow`: Open dropdown
- `Escape`: Close dropdown

### Source Code - DropDownButton KeyDown

From `DropDownButton.cs` (lines 606-668):

```csharp
protected override void OnKeyDown(KeyEventArgs e)
{
    switch (e.Key)
    {
        case Key.Down:
            if (this.HasItems && this.IsDropDownOpen == false)
            {
                this.IsDropDownOpen = true;
                // Focus first item
                var container = this.ItemContainerGenerator.ContainerFromIndex(0);
                NavigateToContainer(container, FocusNavigationDirection.Down);
                handled = true;
            }
            break;

        case Key.Up:
            if (this.HasItems && this.IsDropDownOpen == false)
            {
                this.IsDropDownOpen = true;
                // Focus last item
                var container = this.ItemContainerGenerator.ContainerFromIndex(this.Items.Count - 1);
                NavigateToContainer(container, FocusNavigationDirection.Up);
                handled = true;
            }
            break;

        case Key.Escape:
            if (this.IsDropDownOpen)
            {
                this.IsDropDownOpen = false;
                handled = true;
            }
            break;

        case Key.Enter:
        case Key.Space:
            this.IsDropDownOpen = !this.IsDropDownOpen;
            handled = true;
            break;
    }
}
```

---

## Events Reference

### Button Events

| Event | Inherited From | Description |
|-------|----------------|-------------|
| `Click` | ButtonBase | Button was clicked |
| `PreviewMouseLeftButtonDown` | UIElement | Mouse button pressed (preview) |
| `MouseEnter` / `MouseLeave` | UIElement | Mouse hover |

### ToggleButton Events

| Event | Description |
|-------|-------------|
| `Click` | Button was clicked (fires every time) |
| `Checked` | IsChecked changed to true |
| `Unchecked` | IsChecked changed to false |
| `Indeterminate` | IsChecked changed to null |

### DropDownButton Events

| Event | Description |
|-------|-------------|
| `DropDownOpened` | Dropdown was opened |
| `DropDownClosed` | Dropdown was closed |

### SplitButton Events

| Event | Description |
|-------|-------------|
| `Click` | Primary button was clicked |
| `Checked` | IsChecked changed to true (if IsCheckable) |
| `Unchecked` | IsChecked changed to false (if IsCheckable) |
| `Indeterminate` | IsChecked changed to null (if IsCheckable) |
| `DropDownOpened` | (inherited) Dropdown opened |
| `DropDownClosed` | (inherited) Dropdown closed |

### Event Handling Examples

```csharp
// Code-behind
public MainWindow()
{
    InitializeComponent();

    myDropDown.DropDownOpened += OnDropDownOpened;
    myDropDown.DropDownClosed += OnDropDownClosed;

    mySplitButton.Click += OnSplitButtonClick;
}

private void OnDropDownOpened(object sender, EventArgs e)
{
    // Refresh dropdown content when opened
    RefreshRecentFiles();
}

private void OnDropDownClosed(object sender, EventArgs e)
{
    // Cleanup when closed
}

private void OnSplitButtonClick(object sender, RoutedEventArgs e)
{
    // Handle primary button click
    // Note: This is separate from dropdown item clicks
}
```

```xml
<!-- XAML event handlers -->
<fluent:DropDownButton Header="Recent"
                        DropDownOpened="OnRecentDropDownOpened"
                        DropDownClosed="OnRecentDropDownClosed" />
```

---

## Quick Access Toolbar Integration

All button controls support being added to the Quick Access Toolbar (QAT).

### Properties

| Property | Description |
|----------|-------------|
| `CanAddToQuickAccessToolBar` | Enable/disable QAT context menu option |
| `CanAddButtonToQuickAccessToolBar` | (SplitButton only) Enable for button part |

### How QAT Works

When a control is added to QAT, a clone is created:

1. `CreateQuickAccessItem()` creates a new instance
2. Key properties are bound to the original control
3. The clone uses `Size=Small` in QAT

### Preventing QAT Addition

```xml
<!-- Cannot be added to QAT -->
<fluent:Button Header="Help"
               CanAddToQuickAccessToolBar="False" />
```

### SplitButton QAT Behavior

SplitButton has two QAT options in context menu:
- "Add to Quick Access Toolbar" - adds the entire SplitButton
- "Add Button to Quick Access Toolbar" - adds just the primary button

```xml
<!-- Disable adding just the button part -->
<fluent:SplitButton Header="Save"
                     CanAddToQuickAccessToolBar="True"
                     CanAddButtonToQuickAccessToolBar="False" />
```

---

## DO NOT DO Section

### DO NOT: Use Content Instead of Header

```xml
<!-- WRONG - Content is bound internally to Header -->
<fluent:Button Content="Save" />

<!-- CORRECT -->
<fluent:Button Header="Save" />
```

### DO NOT: Forget LargeIcon for Large Size

```xml
<!-- WRONG - No icon at Large size -->
<fluent:Button Header="Save"
               Size="Large"
               Icon="{StaticResource SaveIcon16}" />

<!-- CORRECT -->
<fluent:Button Header="Save"
               Size="Large"
               Icon="{StaticResource SaveIcon16}"
               LargeIcon="{StaticResource SaveIcon32}" />
```

### DO NOT: Use IsDropDownOpen for Initial State

```xml
<!-- WRONG - Opens immediately on load, causes issues -->
<fluent:DropDownButton Header="Menu" IsDropDownOpen="True" />

<!-- CORRECT - Let user open it -->
<fluent:DropDownButton Header="Menu" />
```

### DO NOT: Bind Command on DropDownButton

```xml
<!-- WRONG - DropDownButton has no Command property -->
<fluent:DropDownButton Header="Options" Command="{Binding OptionsCommand}" />

<!-- CORRECT - Use SplitButton if you need a command -->
<fluent:SplitButton Header="Options" Command="{Binding OptionsCommand}">
    <fluent:MenuItem Header="More Options..." />
</fluent:SplitButton>
```

### DO NOT: Mix GroupName Across Different Contexts

```xml
<!-- WRONG - Same GroupName in different tab items conflicts -->
<fluent:RibbonTabItem Header="Home">
    <fluent:ToggleButton Header="Bold" GroupName="Format" />
</fluent:RibbonTabItem>
<fluent:RibbonTabItem Header="Insert">
    <fluent:ToggleButton Header="Bold" GroupName="Format" /> <!-- Conflicts! -->
</fluent:RibbonTabItem>

<!-- CORRECT - Unique GroupName per context -->
<fluent:RibbonTabItem Header="Home">
    <fluent:ToggleButton Header="Bold" GroupName="HomeFormat" />
</fluent:RibbonTabItem>
<fluent:RibbonTabItem Header="Insert">
    <fluent:ToggleButton Header="Bold" GroupName="InsertFormat" />
</fluent:RibbonTabItem>
```

### DO NOT: Expect Click to Close Dropdown When IsDefinitive="False"

```xml
<!-- User might expect this to close, but it won't -->
<fluent:DropDownButton Header="Settings">
    <fluent:Button Header="Apply"
                   IsDefinitive="False"
                   Command="{Binding ApplyCommand}" />
</fluent:DropDownButton>

<!-- CORRECT - Keep IsDefinitive="True" for closing actions -->
<fluent:DropDownButton Header="Settings">
    <fluent:Button Header="Apply"
                   IsDefinitive="True"
                   Command="{Binding ApplyCommand}" />
</fluent:DropDownButton>
```

### DO NOT: Set Height/Width Directly (Usually)

```xml
<!-- WRONG - Breaks responsive sizing -->
<fluent:Button Header="Save" Height="50" Width="100" />

<!-- CORRECT - Use Size property -->
<fluent:Button Header="Save" Size="Middle" />
```

### DO NOT: Forget to Handle Both Button and Dropdown in SplitButton

```xml
<!-- Incomplete - What happens when dropdown items are clicked? -->
<fluent:SplitButton Header="Save" Command="{Binding SaveCommand}">
    <fluent:MenuItem Header="Save As..." />
    <fluent:MenuItem Header="Save All" />
</fluent:SplitButton>

<!-- CORRECT - All actions handled -->
<fluent:SplitButton Header="Save" Command="{Binding SaveCommand}">
    <fluent:MenuItem Header="Save As..." Command="{Binding SaveAsCommand}" />
    <fluent:MenuItem Header="Save All" Command="{Binding SaveAllCommand}" />
</fluent:SplitButton>
```

---

## Source File Reference

### Button.cs
**Location:** `Fluent.Ribbon/Controls/Button.cs`

Key sections:
- Lines 17-19: Class declaration, interfaces
- Lines 23-35: Size property
- Lines 81-112: Header properties
- Lines 115-161: Icon properties (Icon, LargeIcon, MediumIcon)
- Lines 163-177: IsDefinitive property
- Lines 180-196: IsSimplified property
- Lines 227-236: OnClick override (popup dismissal)
- Lines 243-260: Quick Access Item creation
- Lines 265-277: KeyTip handling

### DropDownButton.cs
**Location:** `Fluent.Ribbon/Controls/DropDownButton.cs`

Key sections:
- Lines 23-31: Class declaration, template parts
- Lines 112-128: DismissOnClickOutside property
- Lines 230-243: IsDropDownOpen property
- Lines 246-262: ResizeMode property
- Lines 264-277: MaxDropDownHeight property
- Lines 280-294: DropDownHeight property
- Lines 297-331: ClosePopupOnMouseDown properties
- Lines 374-378: DropDownOpened/DropDownClosed events
- Lines 555-576: Popup KeyDown handler (Escape)
- Lines 606-668: OnKeyDown (arrow keys, Enter, Space)
- Lines 719-810: IsDropDownOpen change handlers

### SplitButton.cs
**Location:** `Fluent.Ribbon/Controls/SplitButton.cs`

Key sections:
- Lines 17-23: Class declaration, template parts
- Lines 41-83: Command properties
- Lines 86-97: GroupName property
- Lines 100-144: IsChecked property with coercion
- Lines 147-162: IsCheckable property
- Lines 164-178: DropDownToolTip property
- Lines 181-196: IsButtonEnabled property
- Lines 199-213: IsDefinitive property
- Lines 216-254: KeyTip postfix properties
- Lines 258-318: Click/Checked/Unchecked events
- Lines 384-410: Template application
- Lines 418-426: OnKeyDown (Enter triggers button)
- Lines 446-461: Quick Access Item creation
- Lines 500-529: KeyTip information generation

### ToggleButton.cs
**Location:** `Fluent.Ribbon/Controls/ToggleButton.cs`

Key sections:
- Lines 16-21: Class declaration, interfaces
- Lines 81-93: GroupName property
- Lines 178-192: IsDefinitive property
- Lines 241-272: OnClick override (GroupName handling)
- Lines 274-279: OnChecked (group update)
- Lines 294-303: Quick Access Item creation

### XAML Theme Files

| File | Purpose |
|------|---------|
| `Themes/Controls/Button.xaml` | Button styles and templates |
| `Themes/Controls/DropDownButton.xaml` | DropDownButton styles and templates |
| `Themes/Controls/SplitButton.xaml` | SplitButton styles and templates |
| `Themes/Controls/ToggleButton.xaml` | ToggleButton styles and templates |

### Related Interfaces

| Interface | File | Purpose |
|-----------|------|---------|
| `IRibbonControl` | IRibbonControl.cs | Size, Header, Icon, KeyTip |
| `IDropDownControl` | IDropDownControl.cs | IsDropDownOpen, DropDownPopup, events |
| `IToggleButton` | IToggleButton.cs | IsChecked, GroupName |
| `ILargeIconProvider` | ILargeIconProvider.cs | LargeIcon property |
| `IMediumIconProvider` | IMediumIconProvider.cs | MediumIcon property |
| `IQuickAccessItemProvider` | IQuickAccessItemProvider.cs | QAT support |
| `ISimplifiedRibbonControl` | ISimplifiedRibbonControl.cs | Simplified mode |

---

## Quick Reference Card

### Button Type Selection

| Need | Use |
|------|-----|
| Simple click action | `fluent:Button` |
| Toggle on/off state | `fluent:ToggleButton` |
| Radio selection in group | `fluent:ToggleButton` with `GroupName` |
| Dropdown menu only | `fluent:DropDownButton` |
| Primary action + menu | `fluent:SplitButton` |
| Toggle with options | `fluent:SplitButton` with `IsCheckable="True"` |

### Common Property Patterns

```xml
<!-- Minimal button -->
<fluent:Button Header="Click Me" Command="{Binding ClickCommand}" />

<!-- Full-featured button -->
<fluent:Button Header="Save Document"
               Size="Large"
               Icon="{StaticResource SaveIcon16}"
               MediumIcon="{StaticResource SaveIcon24}"
               LargeIcon="{StaticResource SaveIcon32}"
               Command="{Binding SaveCommand}"
               KeyTip="S"
               ToolTip="Save the current document (Ctrl+S)"
               CanAddToQuickAccessToolBar="True" />

<!-- Minimal dropdown -->
<fluent:DropDownButton Header="Options">
    <fluent:MenuItem Header="Option 1" />
</fluent:DropDownButton>

<!-- Minimal split button -->
<fluent:SplitButton Header="Print" Command="{Binding PrintCommand}">
    <fluent:MenuItem Header="Print Preview" Command="{Binding PreviewCommand}" />
</fluent:SplitButton>
```
