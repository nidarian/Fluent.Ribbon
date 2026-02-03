---
title: Fluent.Ribbon Input Controls Reference
description: Reference for Spinner, TextBox, and ComboBox ribbon controls
tags: [spinner, textbox, combobox, controls, input]
see_also:
  - fluent-ribbon-button-controls-reference.md
  - ../advanced/fluent-ribbon-converters-reference.md
---

# Fluent.Ribbon Input Controls Reference

**Version:** Based on Fluent.Ribbon source analysis
**Last Updated:** 2026-01-23
**Source:** [Fluent.Ribbon/Controls](https://github.com/fluentribbon/Fluent.Ribbon/tree/develop/Fluent.Ribbon/Controls)

---

## Overview

Fluent.Ribbon provides ribbon-styled versions of standard WPF input controls. These controls implement ribbon-specific interfaces for sizing, KeyTips, and Quick Access Toolbar integration.

---

## Spinner Control

**Source File:** `Controls\Spinner.cs`

A numeric input control with up/down buttons for incrementing/decrementing values.

### Template Parts
| Part Name | Type | Description |
|-----------|------|-------------|
| `PART_TextBox` | `System.Windows.Controls.TextBox` | The text input area |
| `PART_ButtonUp` | `RepeatButton` | Increment button |
| `PART_ButtonDown` | `RepeatButton` | Decrement button |

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `double` | `0` | Current numeric value (two-way binding) |
| `Minimum` | `double` | `0` | Minimum allowed value |
| `Maximum` | `double` | `double.MaxValue` | Maximum allowed value |
| `Increment` | `double` | `1` | Amount to add/subtract per click |
| `Format` | `string` | `"F1"` | String format for display (e.g., "F2", "N0") |
| `Delay` | `int` | `400` | Milliseconds before repeat starts |
| `Interval` | `int` | `80` | Milliseconds between repeats |
| `TextToValueConverter` | `IValueConverter` | `SpinnerTextToValueConverter` | Custom text-to-value conversion |
| `SelectAllTextOnFocus` | `bool` | `false` | Select all text when focused |

### Events

| Event | Type | Description |
|-------|------|-------------|
| `ValueChanged` | `RoutedPropertyChangedEventHandler<double>` | Fired when Value changes |

### Interfaces Implemented
- `RibbonControl` (base class)
- `IMediumIconProvider` - Supports medium-sized icons
- `ISimplifiedRibbonControl` - Supports simplified ribbon mode

### Basic Usage

```xml
<Fluent:Spinner Header="Zoom"
                Value="{Binding ZoomLevel}"
                Minimum="10"
                Maximum="500"
                Increment="10"
                Format="N0"
                KeyTip="Z" />
```

### Custom Format Examples

```xml
<!-- Integer display -->
<Fluent:Spinner Value="100" Format="N0" />

<!-- Two decimal places -->
<Fluent:Spinner Value="3.14" Format="F2" />

<!-- Percentage -->
<Fluent:Spinner Value="0.75" Format="P0" />

<!-- Currency -->
<Fluent:Spinner Value="99.99" Format="C2" />
```

### Keyboard Support
- **Up Arrow:** Increment value
- **Down Arrow:** Decrement value
- **Enter:** Apply text input
- **Escape:** Revert to previous value

### Methods

| Method | Description |
|--------|-------------|
| `SelectAll()` | Selects all text in the textbox |

---

## TextBox Control

**Source File:** `Controls\TextBox.cs`

Ribbon-styled text input control extending `System.Windows.Controls.TextBox`.

### Template Parts
| Part Name | Type | Description |
|-----------|------|-------------|
| `PART_ContentHost` | `UIElement` | The text content host |

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | `object` | Label displayed next to textbox |
| `HeaderTemplate` | `DataTemplate` | Template for header |
| `Icon` | `object` | Small icon (16x16) |
| `MediumIcon` | `object` | Medium icon (24x24) |
| `Size` | `RibbonControlSize` | Current size (Large/Middle/Small) |
| `SizeDefinition` | `RibbonControlSizeDefinition` | Size progression definition |
| `SimplifiedSizeDefinition` | `RibbonControlSizeDefinition` | Size for simplified mode |
| `KeyTip` | `string` | Keyboard shortcut |
| `CanAddToQuickAccessToolBar` | `bool` | Whether can be added to QAT |

### Interfaces Implemented
- `IQuickAccessItemProvider` - Can be added to Quick Access Toolbar
- `IRibbonControl` - Standard ribbon control interface
- `IMediumIconProvider` - Supports medium icons
- `ISimplifiedRibbonControl` - Supports simplified mode

### Basic Usage

```xml
<Fluent:TextBox Header="Find:"
                Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
                Width="150"
                KeyTip="F" />
```

### With Icon

```xml
<Fluent:TextBox Header="Name"
                Icon="{StaticResource UserIcon}"
                Text="{Binding UserName}"
                MaxLength="50" />
```

### Properties Inherited from WPF TextBox
All standard `System.Windows.Controls.TextBox` properties work:
- `Text`, `MaxLength`, `IsReadOnly`
- `CharacterCasing`, `TextAlignment`, `TextWrapping`
- `AcceptsReturn`, `AcceptsTab`
- `SelectionBrush`, `CaretBrush`

---

## ComboBox Control

**Source File:** `Controls\ComboBox.cs`

Ribbon-styled dropdown selection control extending `System.Windows.Controls.ComboBox`.

### Template Parts
| Part Name | Type | Description |
|-----------|------|-------------|
| `PART_ToggleButton` | `ToggleButton` | Dropdown toggle button |
| `PART_MenuPanel` | `Panel` | Panel for menu items |
| `PART_SelectedImage` | `Image` | Snapped selection image |
| `PART_ContentSite` | `ContentPresenter` | Selected item display |
| `PART_ContentBorder` | `Border` | Content border |
| `PART_ScrollViewer` | `ScrollViewer` | Dropdown scroll viewer |
| `PART_Popup` | `Popup` | Dropdown popup |

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | `object` | `null` | Label text |
| `Icon` | `object` | `null` | Small icon |
| `MediumIcon` | `object` | `null` | Medium icon |
| `TopPopupContent` | `object` | `null` | Content above items |
| `Menu` | `RibbonMenu` | `null` | Menu below items |
| `ResizeMode` | `ContextMenuResizeMode` | `None` | Dropdown resize behavior |
| `DropDownHeight` | `double` | `NaN` | Initial dropdown height |
| `Size` | `RibbonControlSize` | | Current ribbon size |
| `SizeDefinition` | `RibbonControlSizeDefinition` | | Size progression |
| `KeyTip` | `string` | `null` | Keyboard shortcut |

### ContextMenuResizeMode Values
| Value | Description |
|-------|-------------|
| `None` | No resize grip |
| `Vertical` | Vertical resize only |
| `Both` | Horizontal and vertical resize |

### Interfaces Implemented
- `IQuickAccessItemProvider`
- `IRibbonControl`
- `IDropDownControl`
- `IMediumIconProvider`
- `ISimplifiedRibbonControl`

### Basic Usage

```xml
<Fluent:ComboBox Header="Font:"
                 SelectedItem="{Binding SelectedFont}"
                 ItemsSource="{Binding Fonts}"
                 Width="150"
                 KeyTip="FF" />
```

### With Custom Content

```xml
<Fluent:ComboBox Header="Color"
                 Icon="{StaticResource ColorIcon}"
                 IsEditable="True"
                 ResizeMode="Both">
    <Fluent:ComboBox.TopPopupContent>
        <TextBlock Text="Recent Colors" Margin="5" FontWeight="Bold"/>
    </Fluent:ComboBox.TopPopupContent>

    <ComboBoxItem Content="Red" />
    <ComboBoxItem Content="Green" />
    <ComboBoxItem Content="Blue" />

    <Fluent:ComboBox.Menu>
        <Fluent:RibbonMenu>
            <Fluent:MenuItem Header="More Colors..." />
        </Fluent:RibbonMenu>
    </Fluent:ComboBox.Menu>
</Fluent:ComboBox>
```

### Editable ComboBox

```xml
<Fluent:ComboBox Header="Style"
                 IsEditable="True"
                 IsReadOnly="False"
                 Text="{Binding CustomStyle}" />
```

---

## Size Definitions

All input controls support `SizeDefinition` for responsive sizing:

```xml
<!-- Always show as middle size -->
<Fluent:TextBox SizeDefinition="Middle,Middle,Middle" />

<!-- Standard progression -->
<Fluent:Spinner SizeDefinition="Large,Middle,Small" />

<!-- Simplified mode definition -->
<Fluent:ComboBox SimplifiedSizeDefinition="Middle,Small,Small" />
```

### RibbonControlSize Values
| Value | Description |
|-------|-------------|
| `Large` | Full size with header visible |
| `Middle` | Compact with header beside control |
| `Small` | Minimal, icon only |

---

## Quick Access Toolbar Support

All input controls implement `IQuickAccessItemProvider`:

```csharp
// Properties
bool CanAddToQuickAccessToolBar { get; set; }

// Method (called internally)
FrameworkElement CreateQuickAccessItem();
```

When added to QAT, controls create synchronized copies with two-way binding.

### Disable QAT Addition

```xml
<Fluent:TextBox CanAddToQuickAccessToolBar="False" />
```

---

## KeyTip Support

All input controls support KeyTips for keyboard navigation:

```xml
<Fluent:Spinner Header="Size" KeyTip="SZ" />
<Fluent:TextBox Header="Find" KeyTip="FD" />
<Fluent:ComboBox Header="Font" KeyTip="FF" />
```

When the KeyTip is activated:
- **Spinner:** Selects all text and focuses the textbox
- **TextBox:** Selects all text and focuses
- **ComboBox (editable):** Focuses without opening dropdown
- **ComboBox (non-editable):** Opens dropdown

---

## MVVM Patterns

### Spinner with ViewModel

```csharp
public class ZoomViewModel : INotifyPropertyChanged
{
    private double _zoomLevel = 100;

    public double ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            _zoomLevel = Math.Clamp(value, 25, 400);
            OnPropertyChanged();
        }
    }
}
```

```xml
<Fluent:Spinner Value="{Binding ZoomLevel}"
                Minimum="25"
                Maximum="400"
                Increment="25"
                Format="N0" />
```

### ComboBox with Enum

```csharp
public enum FontWeight { Light, Regular, Bold, Black }

public class FontViewModel
{
    public FontWeight[] Weights => Enum.GetValues<FontWeight>();
    public FontWeight SelectedWeight { get; set; }
}
```

```xml
<Fluent:ComboBox Header="Weight"
                 ItemsSource="{Binding Weights}"
                 SelectedItem="{Binding SelectedWeight}" />
```

---

## DO NOT DO

### Avoid Direct Template Part Access
```csharp
// WRONG - Don't access template parts directly
var textBox = spinner.GetTemplateChild("PART_TextBox") as TextBox;
textBox.Background = Brushes.Red;

// RIGHT - Use properties or styles
spinner.Background = Brushes.Red;
```

### Avoid Replacing Converters Without Need
```csharp
// WRONG - Only replace if you need custom parsing
spinner.TextToValueConverter = new MyConverter();

// RIGHT - Use Format property for display formatting
spinner.Format = "C2"; // Currency with 2 decimals
```

### Avoid Hardcoding Sizes
```xml
<!-- WRONG - Fixed size ignores ribbon responsiveness -->
<Fluent:TextBox Width="200" Height="25" />

<!-- RIGHT - Let ribbon control sizing -->
<Fluent:TextBox SizeDefinition="Large,Middle,Small" />
```

---

## Quick Reference

| Control | Base Class | Key Feature |
|---------|------------|-------------|
| `Spinner` | `RibbonControl` | Numeric input with increment/decrement |
| `TextBox` | `System.Windows.Controls.TextBox` | Text input with ribbon styling |
| `ComboBox` | `System.Windows.Controls.ComboBox` | Dropdown with ribbon features |

### Common Properties (All Controls)
| Property | Description |
|----------|-------------|
| `Header` | Label text |
| `Icon` | Small icon |
| `MediumIcon` | Medium icon |
| `KeyTip` | Keyboard shortcut |
| `Size` | Current size |
| `SizeDefinition` | Size progression |
| `IsSimplified` | Whether in simplified mode |
| `CanAddToQuickAccessToolBar` | QAT eligibility |

---

*Reference verified against Fluent.Ribbon source code as of 2026-01-23.*
