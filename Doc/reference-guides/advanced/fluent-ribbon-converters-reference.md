---
title: Fluent.Ribbon Converters Reference
description: StaticConverters, IconConverter, and value converters for Fluent.Ribbon
tags: [converters, staticconverters, binding, advanced]
see_also:
  - ../styling/fluent-ribbon-icon-system-reference.md
  - ../controls/fluent-ribbon-input-controls-reference.md
---

# Fluent.Ribbon Converters Reference

**Version:** Based on Fluent.Ribbon source analysis
**Last Updated:** 2026-01-23
**Source:** [Fluent.Ribbon/Converters](https://github.com/fluentribbon/Fluent.Ribbon/tree/develop/Fluent.Ribbon/Converters)

---

## Overview

Fluent.Ribbon provides several value converters for common XAML binding scenarios. Many are available as static instances through `StaticConverters` for efficient reuse.

---

## StaticConverters Class

**Namespace:** `Fluent.Converters`

Provides static instances of commonly used converters to avoid creating new instances.

```xml
xmlns:converters="clr-namespace:Fluent.Converters;assembly=Fluent"

<Button Visibility="{Binding IsEnabled,
    Converter={x:Static converters:StaticConverters.EqualsToVisibilityConverter}}" />
```

### Available Static Instances

| Property | Type | Description |
|----------|------|-------------|
| `InvertNumericConverter` | `InvertNumericConverter` | Negates numeric values |
| `ThicknessConverter` | `ThicknessConverter` | Converts/manipulates Thickness |
| `CornerRadiusConverter` | `CornerRadiusConverter` | Converts/manipulates CornerRadius |
| `ObjectToImageConverter` | `ObjectToImageConverter` | Converts various types to Image |
| `ColorToSolidColorBrushValueConverter` | `ColorToSolidColorBrushValueConverter` | Color to SolidColorBrush |
| `EqualsToVisibilityConverter` | `EqualsToVisibilityConverter` | Equality check to Visibility |
| `InverseBoolConverter` | `InverseBoolConverter` | Inverts boolean values |

---

## Individual Converters

### InverseBoolConverter

Inverts a boolean value.

**Source:** `Converters\InverseBoolConverter.cs`

```xml
<CheckBox IsEnabled="{Binding IsReadOnly,
    Converter={x:Static converters:StaticConverters.InverseBoolConverter}}" />
```

| Input | Output |
|-------|--------|
| `true` | `false` |
| `false` | `true` |

---

### InvertNumericConverter

Negates a numeric value (multiplies by -1).

**Source:** `Converters\InvertNumericConverter.cs`

```xml
<TranslateTransform Y="{Binding Offset,
    Converter={x:Static converters:StaticConverters.InvertNumericConverter}}" />
```

| Input | Output |
|-------|--------|
| `5` | `-5` |
| `-10` | `10` |
| `0` | `0` |

---

### EqualsToVisibilityConverter

Returns `Visible` if value equals parameter, otherwise `Collapsed`.

**Source:** `Converters\EqualsToVisibilityConverter.cs`

```xml
<!-- Show only when Mode equals "Edit" -->
<StackPanel Visibility="{Binding Mode,
    Converter={x:Static converters:StaticConverters.EqualsToVisibilityConverter},
    ConverterParameter=Edit}" />
```

**Parameters:**
- `ConverterParameter`: The value to compare against

| Value | Parameter | Output |
|-------|-----------|--------|
| `"Edit"` | `"Edit"` | `Visible` |
| `"View"` | `"Edit"` | `Collapsed` |

---

### ColorToSolidColorBrushValueConverter

Converts a `Color` to a `SolidColorBrush`.

**Source:** `Converters\ColorToSolidColorBrushConverter.cs`

```xml
<Border Background="{Binding ThemeColor,
    Converter={x:Static converters:StaticConverters.ColorToSolidColorBrushValueConverter}}" />
```

| Input | Output |
|-------|--------|
| `Color` | `SolidColorBrush` |
| `null` | `null` |

---

### ThicknessConverter

Manipulates `Thickness` values using parameter multipliers.

**Source:** `Converters\ThicknessConverter.cs`

```xml
<!-- Double the margin on all sides -->
<Border Margin="{Binding BaseMargin,
    Converter={x:Static converters:StaticConverters.ThicknessConverter},
    ConverterParameter=2}" />

<!-- Apply different multipliers to each side: Left,Top,Right,Bottom -->
<Border Margin="{Binding BaseMargin,
    Converter={x:Static converters:StaticConverters.ThicknessConverter},
    ConverterParameter='1,0,1,0'}" />
```

**Parameters:**
- Single value: Multiplies all sides
- Four comma-separated values: `Left,Top,Right,Bottom` multipliers

---

### CornerRadiusConverter

Manipulates `CornerRadius` values using parameter multipliers.

**Source:** `Converters\CornerRadiusConverter.cs`

```xml
<!-- Apply multipliers to each corner -->
<Border CornerRadius="{Binding BaseRadius,
    Converter={x:Static converters:StaticConverters.CornerRadiusConverter},
    ConverterParameter='1,1,0,0'}" />
```

**Parameters:**
- Four comma-separated values: `TopLeft,TopRight,BottomRight,BottomLeft` multipliers

---

### ObjectToImageConverter

Converts various object types to an `Image` element or `ImageSource`.

**Source:** `Converters\ObjectToImageConverter.cs`

Supports conversion from:
- `string` (file path or pack URI)
- `Uri`
- `System.Drawing.Icon`
- `ImageSource`

```xml
<Image Source="{Binding IconPath,
    Converter={x:Static converters:StaticConverters.ObjectToImageConverter}}" />
```

---

### IconConverter

Specialized version of `ObjectToImageConverter` that provides default window/application icons when value is null.

**Source:** `Converters\IconConverter.cs`

```csharp
// Constructor takes the icon binding
var converter = new IconConverter(iconBinding);

// With target visual for DPI awareness
var converter = new IconConverter(iconBinding, targetVisualBinding);

// With custom desired size
var converter = new IconConverter(iconBinding, desiredSize, targetVisualBinding);
```

**Supported Conversions:**
| Source Type | Target Type |
|-------------|-------------|
| `string` | `Image` or `ImageSource` |
| `Uri` | `Image` or `ImageSource` |
| `System.Drawing.Icon` | `Image` or `ImageSource` |
| `ImageSource` | `Image` or `ImageSource` |

When value is `null`, it retrieves:
1. Window icon (if available)
2. Application main window icon
3. Process main window icon
4. System default application icon

---

### SpinnerTextToValueConverter

Converts text to double values for the Spinner control.

**Source:** `Converters\SpinnerTextToValueConverter.cs`

Used internally by `Spinner` control. Default instance available:

```csharp
SpinnerTextToValueConverter.DefaultInstance
```

**Convert (Text → Value):**
- Parameter: `Tuple<string, double>` containing format and fallback value
- Returns parsed double or fallback value

**ConvertBack (Value → Text):**
- Parameter: Format string (e.g., "F1", "N0")
- Returns formatted string

---

### SizeDefinitionConverter

Converts string representations to `RibbonControlSizeDefinition`.

**Source:** `Converters\SizeDefinitionConverter.cs`

Used in XAML to parse size definition strings:

```xml
<Fluent:Button SizeDefinition="Large,Middle,Small" />
```

**String Format:** `Size1,Size2,Size3`

Valid size values:
- `Large`
- `Middle`
- `Small`

---

### RibbonGroupBoxStateDefinitionConverter

Converts string representations to `RibbonGroupBoxStateDefinition`.

**Source:** `Converters\RibbonGroupBoxStateDefinitionConverter.cs`

```xml
<Fluent:RibbonGroupBox StateDefinition="Large,Middle,Small,Collapsed" />
```

**String Format:** Comma-separated `RibbonGroupBoxState` values

Valid state values:
- `Large`
- `Middle`
- `Small`
- `Collapsed`
- `QuickAccess`

---

### IsNullConverter

Returns `true` if value is `null`, otherwise `false`.

**Source:** `Converters\IsNullConverter.cs`

```xml
<Button IsEnabled="{Binding SelectedItem,
    Converter={StaticResource IsNullConverter}}" />
```

| Input | Output |
|-------|--------|
| `null` | `true` |
| Any object | `false` |

---

### ExtractLeftRightFromThicknessConverter

Extracts left and right values from a Thickness.

**Source:** `Converters\ExtractLeftRightFromThicknessConverter.cs`

```xml
<Border Margin="{Binding FullMargin,
    Converter={StaticResource ExtractLeftRightFromThicknessConverter}}" />
```

| Input | Output |
|-------|--------|
| `Thickness(5,10,15,20)` | `Thickness(5,0,15,0)` |

---

### ApplicationMenuRightScrollViewerExtractorConverter

Internal converter for extracting scroll viewer width from ApplicationMenu.

**Source:** `Converters\ApplicationMenuRightScrollViewerExtractorConverter.cs`

Used internally in ApplicationMenu template.

---

## Usage Patterns

### Multiple Converters in Binding

Use `MultiBinding` when you need multiple converters:

```xml
<TextBlock>
    <TextBlock.Text>
        <MultiBinding StringFormat="{}{0} - {1}">
            <Binding Path="FirstName" />
            <Binding Path="LastName" />
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

### Creating Custom Converter

```csharp
public class MyConverter : IValueConverter
{
    public object Convert(object value, Type targetType,
        object parameter, CultureInfo culture)
    {
        // Conversion logic
        return result;
    }

    public object ConvertBack(object value, Type targetType,
        object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}
```

### Converter in Style

```xml
<Style TargetType="Border">
    <Setter Property="Background">
        <Setter.Value>
            <Binding Path="AccentColor"
                Converter="{x:Static converters:StaticConverters.ColorToSolidColorBrushValueConverter}" />
        </Setter.Value>
    </Setter>
</Style>
```

---

## DO NOT DO

### Don't Create New Instances When Static Available
```xml
<!-- WRONG - Creates new instance each time -->
<Button Visibility="{Binding IsVisible,
    Converter={converters:InverseBoolConverter}}" />

<!-- RIGHT - Use static instance -->
<Button Visibility="{Binding IsVisible,
    Converter={x:Static converters:StaticConverters.InverseBoolConverter}}" />
```

### Don't Override SpinnerTextToValueConverter Without Need
```csharp
// WRONG - Usually unnecessary
spinner.TextToValueConverter = new MySpinnerConverter();

// RIGHT - Use Format property instead
spinner.Format = "C2"; // Currency format
```

---

## Quick Reference

| Converter | Purpose | Static Instance |
|-----------|---------|-----------------|
| `InverseBoolConverter` | Negate boolean | Yes |
| `InvertNumericConverter` | Negate number | Yes |
| `EqualsToVisibilityConverter` | Equality to Visibility | Yes |
| `ColorToSolidColorBrushValueConverter` | Color to Brush | Yes |
| `ThicknessConverter` | Manipulate Thickness | Yes |
| `CornerRadiusConverter` | Manipulate CornerRadius | Yes |
| `ObjectToImageConverter` | Convert to Image | Yes |
| `IconConverter` | Icon with fallback | No (needs binding) |
| `SpinnerTextToValueConverter` | Text to double | Yes (DefaultInstance) |
| `SizeDefinitionConverter` | String to SizeDefinition | TypeConverter |
| `RibbonGroupBoxStateDefinitionConverter` | String to StateDefinition | TypeConverter |
| `IsNullConverter` | Null check | No |
| `ExtractLeftRightFromThicknessConverter` | Extract LR from Thickness | No |

---

*Reference verified against Fluent.Ribbon source code as of 2026-01-23.*
