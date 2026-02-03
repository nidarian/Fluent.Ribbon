---
title: Fluent.Ribbon Icon System Reference
description: Built-in icons, recommended icon packs, IconPresenter, ObjectToImageConverter, sizing, and DPI awareness
tags: [icons, images, dpi, styling, iconpresenter, mahapps, licensing]
see_also:
  - ../advanced/fluent-ribbon-converters-reference.md
  - ../controls/fluent-ribbon-button-controls-reference.md
---

# Fluent.Ribbon Icon System Reference

**Complete guide to the Fluent.Ribbon icon infrastructure: IconPresenter, ObjectToImageConverter, icon properties, sizing, and best practices.**

Source Files:
- `Fluent.Ribbon/Controls/IconPresenter.cs`
- `Fluent.Ribbon/Converters/ObjectToImageConverter.cs`
- `Fluent.Ribbon/Converters/IconConverter.cs`
- `Fluent.Ribbon/Enumerations/IconSize.cs`
- `Fluent.Ribbon/ILargeIconProvider.cs`
- `Fluent.Ribbon/IMediumIconProvider.cs`
- `Fluent.Ribbon/AttachedProperties/RibbonProperties.cs`
- `Fluent.Ribbon/Effects/GrayscaleEffect.cs`

---

## Built-in Icons

Fluent.Ribbon includes 15 built-in vector icons in `Themes/Images.xaml`. These are MIT licensed and free for any use.

### Available Icons

| Resource Key | Description | Usage |
|--------------|-------------|-------|
| `Fluent.Ribbon.Images.ApplicationMenu` | Document with dropdown arrow | Application menu button |
| `Fluent.Ribbon.Images.Checked` | Checkmark | Menu item checked state |
| `Fluent.Ribbon.Images.Copy` | Two overlapping documents | Copy command |
| `Fluent.Ribbon.Images.Cut` | Scissors | Cut command |
| `Fluent.Ribbon.Images.DefaultPlaceholder` | Green circle placeholder | Missing icon fallback |
| `Fluent.Ribbon.Images.DialogLauncher` | Arrow pointing to corner | GroupBox dialog launcher |
| `Fluent.Ribbon.Images.Help` | Question mark in circle | Help button |
| `Fluent.Ribbon.Images.MoreColors` | Color wheel segments | Color picker "More Colors" |
| `Fluent.Ribbon.Images.Paste` | Clipboard with document | Paste command |
| `Fluent.Ribbon.Images.QuickAccessToolbarDropDown` | Horizontal line + down arrow | QAT dropdown (above ribbon) |
| `Fluent.Ribbon.Images.QuickAccessToolbarDropDown.BelowRibbon` | Same, different color | QAT dropdown (below ribbon) |
| `Fluent.Ribbon.Images.QuickAccessToolbarExtender` | Double chevron right | QAT overflow button |
| `Fluent.Ribbon.Images.QuickAccessToolbarExtender.BelowRibbon` | Same, different color | QAT overflow (below ribbon) |
| `Fluent.Ribbon.Images.RibbonDisplayOptions` | Down chevron | Ribbon display options |
| `Fluent.Ribbon.Images.Warning` | Yellow triangle with exclamation | Warning indicator |

### Using Built-in Icons

```xml
<!-- Direct usage -->
<Fluent:Button Header="Help"
               Icon="{DynamicResource Fluent.Ribbon.Images.Help}"
               LargeIcon="{DynamicResource Fluent.Ribbon.Images.Help}" />

<!-- In a style -->
<Setter Property="Icon" Value="{DynamicResource Fluent.Ribbon.Images.Copy}" />
```

**Note:** Use `DynamicResource` (not `StaticResource`) because these icons reference theme-aware brushes that can change at runtime.

---

## Recommended Icon Packs

For icons beyond the built-in set, these free icon packs work well with Fluent.Ribbon:

### MahApps.Metro.IconPacks (Recommended)

**License:** MIT (library) + various open licenses (icon sets)
**NuGet:** `MahApps.Metro.IconPacks.Material`, `MahApps.Metro.IconPacks.FontAwesome`, etc.
**Commercial Use:** Yes

```xml
<!-- Add namespace -->
xmlns:iconPacks="http://metro.mahapps.com/winfx/xaml/iconpacks"

<!-- Usage -->
<Fluent:Button Header="Save"
               LargeIcon="{iconPacks:Material Kind=ContentSave}" />

<Fluent:Button Header="Settings"
               Icon="{iconPacks:Material Kind=Cog}" />
```

**Available icon sets:**
| Package | Icons | License |
|---------|-------|---------|
| `IconPacks.Material` | 7000+ | Apache 2.0 |
| `IconPacks.FontAwesome` | 2000+ | CC BY 4.0 / SIL OFL |
| `IconPacks.Modern` | 1200+ | CC BY-ND 3.0 |
| `IconPacks.Entypo` | 400+ | CC BY-SA 4.0 |
| `IconPacks.Octicons` | 500+ | MIT |

### Segoe MDL2 Assets (Windows Built-in)

**License:** Part of Windows, free for Windows apps
**Commercial Use:** Yes (on Windows)

```xml
<Fluent:Button Header="Save">
    <Fluent:Button.LargeIcon>
        <TextBlock FontFamily="Segoe MDL2 Assets"
                   FontSize="32"
                   Text="&#xE74E;" />
    </Fluent:Button.LargeIcon>
</Fluent:Button>
```

### Fluent UI System Icons (Microsoft)

**License:** MIT
**GitHub:** https://github.com/microsoft/fluentui-system-icons
**Commercial Use:** Yes

Available as fonts or SVG. Convert SVG to DrawingImage for WPF use.

### Custom DrawingImage (Vector)

Create your own scalable icons:

```xml
<DrawingImage x:Key="MyCustomIcon">
    <DrawingImage.Drawing>
        <GeometryDrawing Brush="{DynamicResource Fluent.Ribbon.Brushes.LabelText}"
                         Geometry="M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22..." />
    </DrawingImage.Drawing>
</DrawingImage>
```

**Tip:** Use theme-aware brushes like `Fluent.Ribbon.Brushes.LabelText` so icons adapt to light/dark themes.

---

## Overview

The Fluent.Ribbon icon system consists of:

1. **IconPresenter** - A ContentControl that displays icons with automatic size management
2. **ObjectToImageConverter** - Converts various input types to ImageSource/Image
3. **IconConverter** - Extends ObjectToImageConverter with window/app icon fallback
4. **Icon Properties** - Icon, MediumIcon, LargeIcon on ribbon controls
5. **IconSize Enum** - Small, Medium, Large, Custom
6. **GrayscaleEffect** - Shader effect for disabled state

---

## IconPresenter Control

`IconPresenter` is a specialized `ContentControl` that manages icon display based on size requirements.

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IconSize` | `IconSize` | `Small` | Current size mode: Small, Medium, Large, Custom |
| `SmallIcon` | `object` | null | Icon source for small size |
| `MediumIcon` | `object` | null | Icon source for medium size |
| `LargeIcon` | `object` | null | Icon source for large size |
| `SmallSize` | `Size` | 16x16 | Pixel dimensions for small icons |
| `MediumSize` | `Size` | 24x24 | Pixel dimensions for medium icons |
| `LargeSize` | `Size` | 32x32 | Pixel dimensions for large icons |
| `CustomSize` | `Size` | 0x0 | Pixel dimensions when IconSize=Custom |
| `OptimalIcon` | `object` | (computed) | The best-match icon for current size (read-only) |
| `CurrentIconSizeSize` | `Size` | (computed) | Current rendered size in pixels (read-only) |

### How IconPresenter Works

```
IconSize=Large
    ↓
GetOptimalIcon() → LargeIcon ?? MediumIcon ?? SmallIcon
    ↓
UpdateSize() → Width=32, Height=32
    ↓
MultiBinding with ObjectToImageConverter
    ↓
Rendered Image
```

### Icon Fallback Logic

IconPresenter automatically falls back to available icons based on current `IconSize`:

```csharp
// From IconPresenter.GetOptimalIcon()
return this.IconSize switch
{
    IconSize.Small  => SmallIcon  ?? MediumIcon ?? LargeIcon,
    IconSize.Medium => MediumIcon ?? LargeIcon  ?? SmallIcon,
    IconSize.Large  => LargeIcon  ?? MediumIcon ?? SmallIcon,
    _               => LargeIcon  ?? MediumIcon ?? SmallIcon
};
```

**Key behavior:** If you only provide `LargeIcon`, it will be used for all sizes (scaled down as needed).

### Disabled State Behavior

When `IsEnabled=False`:
- **GrayscaleEffect** shader is applied (converts to grayscale)
- **Opacity** reduced to 0.5

```csharp
// From IconPresenter.OnIsEnabledChanged
control.Effect = newValue ? null : control.grayscaleEffect ??= new GrayscaleEffect();
control.Opacity = newValue ? 1 : 0.5;
```

### Static Constructor Settings

IconPresenter is configured with these defaults:
- `SnapsToDevicePixels = true` (crisp rendering)
- `Focusable = false` (not in tab order)
- `IsHitTestVisible = false` (mouse events pass through)

---

## IconSize Enum

```csharp
public enum IconSize
{
    Small,   // Usually 16x16
    Medium,  // Usually 24x24
    Large,   // Usually 32x32
    Custom   // User-defined via CustomSize property
}
```

### Size Usage by Control State

| Control State | IconSize | Typical Use |
|---------------|----------|-------------|
| Large button | Large | Primary ribbon buttons |
| Middle button | Small | Collapsed ribbon buttons |
| Small button | Small | Quick Access Toolbar |
| Simplified ribbon | Medium | Simplified ribbon mode |
| Menu items | Small | Dropdown menus |

### Size Mapping in Styles

From `Button.xaml`:
```xml
<Style.Triggers>
    <Trigger Property="Size" Value="Large">
        <!-- IconSize defaults to Large -->
    </Trigger>
    <Trigger Property="Size" Value="Middle">
        <Setter Property="Fluent:RibbonProperties.IconSize" Value="Small" />
    </Trigger>
    <Trigger Property="Size" Value="Small">
        <Setter Property="Fluent:RibbonProperties.IconSize" Value="Small" />
    </Trigger>
    <Trigger Property="IsSimplified" Value="True">
        <Setter Property="Fluent:RibbonProperties.IconSize" Value="Medium" />
    </Trigger>
</Style.Triggers>
```

---

## Icon Properties on Ribbon Controls

Most ribbon controls implement these interfaces:

### IRibbonControl.Icon
```csharp
public object? Icon { get; set; }  // Small icon (16x16)
```

### ILargeIconProvider.LargeIcon
```csharp
public object? LargeIcon { get; set; }  // Large icon (32x32)
```

### IMediumIconProvider.MediumIcon
```csharp
public object? MediumIcon { get; set; }  // Medium icon (24x24)
```

### Controls That Support All Three

| Control | Icon | MediumIcon | LargeIcon |
|---------|------|------------|-----------|
| Button | Yes | Yes | Yes |
| ToggleButton | Yes | Yes | Yes |
| DropDownButton | Yes | Yes | Yes |
| SplitButton | Yes | Yes | Yes |
| ComboBox | Yes | Yes | Yes |
| Spinner | Yes | Yes | Yes |
| CheckBox | Yes | Yes | Yes |
| RadioButton | Yes | Yes | Yes |
| InRibbonGallery | Yes | Yes | Yes |
| TextBox | Yes | Yes | Yes |

---

## ObjectToImageConverter

The workhorse converter that transforms various types into displayable images.

### Accepted Input Types

| Input Type | Description |
|------------|-------------|
| `string` | File path or pack URI |
| `Uri` | Absolute or relative URI |
| `System.Drawing.Icon` | Windows ICO format |
| `ImageSource` | BitmapImage, BitmapFrame, DrawingImage, etc. |
| `BitmapFrame` | Multi-resolution frame from BitmapDecoder |
| Resource expressions | Deferred resource references |

### String/Uri Resolution

```csharp
// Relative paths are converted to pack URIs
"Images/MyIcon.png" → "pack://application:,,,/Images/MyIcon.png"

// Absolute file paths work directly
"C:\Icons\MyIcon.ico"

// Pack URIs work as-is
"pack://application:,,,/MyAssembly;component/Resources/Icon.png"
```

### Frame Selection for Multi-Resolution Images

For ICO files and multi-frame images, the converter selects the best frame:

```csharp
// Frames ordered by width, then height
// Returns first frame >= desired size, or largest if none match
return framesOrderedByWidth
    .FirstOrDefault(f => f.Width >= scaledDesiredSize.Width
                      && f.Height >= scaledDesiredSize.Height)
    ?? framesOrderedByWidth.Last();
```

### DPI Awareness

The converter scales desired size by DPI:

```csharp
// At 150% DPI (1.5x), requesting 16x16 looks for 24x24 frame
scaledDesiredSize = new Size(
    desiredSize.Width * dpiScale.DpiScaleX,
    desiredSize.Height * dpiScale.DpiScaleY
);
```

DPI is obtained from:
1. Target visual (if available)
2. Application main window
3. Fallback to 1.0x (96 DPI)

### Image Freezing

**All returned ImageSources are frozen** for thread safety and performance:

```csharp
private static ImageSource? GetAsFrozenIfPossible(ImageSource? imageSource)
{
    if (imageSource is null) return null;
    if (imageSource.CanFreeze)
    {
        return (ImageSource)imageSource.GetAsFrozen();
    }
    return imageSource;
}
```

### XAML Usage

```xml
<!-- As markup extension -->
<Image Source="{Fluent:ObjectToImageConverter {Binding IconPath}, 32}" />

<!-- With target visual for DPI -->
<Image Source="{Fluent:ObjectToImageConverter
    {Binding IconPath},
    32,
    {Binding RelativeSource={RelativeSource Self}}}" />
```

---

## IconConverter

Extends `ObjectToImageConverter` with automatic fallback to window/application icon.

### Fallback Logic

When icon value is null, IconConverter tries (in order):
1. Current window icon (via HWND)
2. Application main window icon
3. Process main window icon
4. System default application icon (IDI_APPLICATION)

```csharp
protected override object? GetValueToConvert(object? value, Size desiredSize, Visual? targetVisual)
{
    if (value is null)
    {
        var defaultIcon = GetDefaultIcon(targetVisual, desiredSize);
        if (defaultIcon is not null)
            return defaultIcon;
    }
    return base.GetValueToConvert(value, desiredSize, targetVisual);
}
```

---

## Using Different Icon Sources

### PNG Images (Recommended for Simple Icons)

```xml
<Fluent:Button Header="Save"
               Icon="/Resources/Icons/save-16.png"
               LargeIcon="/Resources/Icons/save-32.png" />
```

**Best practice:** Provide separate files for each size to ensure crisp rendering.

### ICO Files (Multi-Resolution)

```xml
<Fluent:Button Header="Save"
               Icon="/Resources/Icons/save.ico"
               LargeIcon="/Resources/Icons/save.ico" />
```

The converter automatically selects the best frame for the target size.

### SVG via DrawingImage (Vector - Scales Perfectly)

```xml
<Fluent:Button Header="Save">
    <Fluent:Button.LargeIcon>
        <DrawingImage>
            <DrawingImage.Drawing>
                <GeometryDrawing Brush="Black"
                                 Geometry="M19,12V19H5V12H3V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V12H19M13,3V14.17L16.59,10.58L18,12L12,18L6,12L7.41,10.59L11,14.17V3H13Z" />
            </DrawingImage.Drawing>
        </DrawingImage>
    </Fluent:Button.LargeIcon>
</Fluent:Button>
```

### Resource Dictionary Icons

```xml
<!-- In ResourceDictionary -->
<DrawingImage x:Key="SaveIcon">
    <DrawingImage.Drawing>
        <GeometryDrawing Brush="{DynamicResource Fluent.Ribbon.Brushes.LabelText}"
                         Geometry="M15,9H5V5H15M12,19A3,3 0 0,1 9,16A3,3 0 0,1 12,13A3,3 0 0,1 15,16A3,3 0 0,1 12,19M17,3H5C3.89,3 3,3.9 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V7L17,3Z" />
    </DrawingImage.Drawing>
</DrawingImage>

<!-- Usage -->
<Fluent:Button Header="Save"
               Icon="{StaticResource SaveIcon}"
               LargeIcon="{StaticResource SaveIcon}" />
```

### BitmapImage with Caching

```xml
<Fluent:Button Header="Save">
    <Fluent:Button.LargeIcon>
        <BitmapImage UriSource="/Resources/Icons/save-32.png"
                     CacheOption="OnLoad"
                     CreateOptions="IgnoreColorProfile" />
    </Fluent:Button.LargeIcon>
</Fluent:Button>
```

---

## CustomSize Property

For non-standard icon sizes:

```xml
<Fluent:Button Header="Custom"
               Fluent:RibbonProperties.IconSize="Custom"
               Fluent:RibbonProperties.CustomIconSize="48,48"
               LargeIcon="/Resources/Icons/custom-48.png" />
```

Or on the IconPresenter directly:

```xml
<Fluent:IconPresenter IconSize="Custom"
                      CustomSize="20,20"
                      SmallIcon="{Binding MyIcon}" />
```

---

## Attached Properties

### RibbonProperties.IconSize

Controls which icon slot IconPresenter uses:

```xml
<Fluent:Button Fluent:RibbonProperties.IconSize="Medium" ... />
```

```csharp
RibbonProperties.SetIconSize(myButton, IconSize.Medium);
```

### RibbonProperties.CustomIconSize

Specifies pixel dimensions when `IconSize=Custom`:

```xml
<Fluent:Button Fluent:RibbonProperties.IconSize="Custom"
               Fluent:RibbonProperties.CustomIconSize="48,48" ... />
```

```csharp
RibbonProperties.SetCustomIconSize(myButton, new Size(48, 48));
```

---

## GrayscaleEffect

Applied automatically to IconPresenter when disabled.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Input` | `Brush` | - | Source brush (set automatically) |
| `FilterColor` | `Color` | White | Tint color for grayscale output |

### Manual Use

```xml
<Image Source="{Binding MyIcon}">
    <Image.Effect>
        <Fluent:GrayscaleEffect FilterColor="LightGray" />
    </Image.Effect>
</Image>
```

---

## Template Usage Pattern

Standard pattern from Button.xaml:

```xml
<Fluent:IconPresenter x:Name="iconImage"
                      SmallIcon="{Binding Icon, RelativeSource={RelativeSource TemplatedParent}}"
                      MediumIcon="{Binding MediumIcon, RelativeSource={RelativeSource TemplatedParent}}"
                      LargeIcon="{Binding LargeIcon, RelativeSource={RelativeSource TemplatedParent}}"
                      CustomSize="{Binding Path=(Fluent:RibbonProperties.CustomIconSize), RelativeSource={RelativeSource TemplatedParent}}"
                      IconSize="{Binding Path=(Fluent:RibbonProperties.IconSize), RelativeSource={RelativeSource TemplatedParent}}" />
```

Note: `Icon` property binds to `SmallIcon`, not a generic `Icon` property on IconPresenter.

---

## Performance Best Practices

### 1. Freeze All Icon Resources

```csharp
var icon = new BitmapImage(new Uri("pack://application:,,,/Icons/save.png"));
icon.Freeze();  // Critical for multi-threaded access and memory
```

Or in XAML:
```xml
<BitmapImage x:Key="SaveIcon"
             UriSource="/Icons/save.png"
             PresentationOptions:Freeze="True" />
```

### 2. Use OnLoad Caching

```xml
<BitmapImage CacheOption="OnLoad" ... />
```

Loads the entire image into memory immediately, allowing the file to be released.

### 3. Prefer DrawingImage for Vector Icons

- Scales without quality loss
- Smaller memory footprint
- Can use theme-aware brushes

### 4. Provide Size-Appropriate Images

Instead of relying on scaling:
```xml
<!-- Better: separate files per size -->
<Fluent:Button Icon="/Icons/save-16.png"
               MediumIcon="/Icons/save-24.png"
               LargeIcon="/Icons/save-32.png" />
```

### 5. Use Resources for Repeated Icons

```xml
<!-- Define once -->
<DrawingImage x:Key="SaveIcon" ... />

<!-- Use many times -->
<Fluent:Button LargeIcon="{StaticResource SaveIcon}" />
<Fluent:Button LargeIcon="{StaticResource SaveIcon}" />
```

---

## DPI Awareness Details

### How Frame Selection Works at High DPI

| Display DPI | Icon Requested | Frame Selected |
|-------------|----------------|----------------|
| 96 (100%) | 16x16 | 16x16 or nearest larger |
| 120 (125%) | 16x16 | 20x20 or nearest larger |
| 144 (150%) | 16x16 | 24x24 or nearest larger |
| 192 (200%) | 16x16 | 32x32 or nearest larger |

### ICO File Recommendations

Include these frame sizes in your ICO files:
- 16x16 (100% DPI small)
- 20x20 (125% DPI small)
- 24x24 (150% DPI small, 100% medium)
- 32x32 (200% DPI small, 100% large)
- 48x48 (150% DPI large)
- 64x64 (200% DPI large)
- 256x256 (for Windows Explorer, etc.)

---

## Fallback Behavior Summary

### When Icon is Not Found

1. **File not found:** `imageNotFoundImageSource` is displayed (red X icon)
2. **Design mode:** Shows placeholder instead of throwing

### When Icon Property is Null

IconPresenter cascades to other icon properties:
- Large mode: `LargeIcon ?? MediumIcon ?? SmallIcon`
- Medium mode: `MediumIcon ?? LargeIcon ?? SmallIcon`
- Small mode: `SmallIcon ?? MediumIcon ?? LargeIcon`

### When All Icons are Null

- IconPresenter renders empty (no visual)
- Some templates show a border placeholder in Simplified mode

---

## DO NOT DO

### Do Not Use Unfrozen Images Across Controls

```csharp
// BAD - same mutable instance shared
var icon = new BitmapImage(new Uri(...));
button1.Icon = icon;
button2.Icon = icon;  // Can cause rendering issues

// GOOD - frozen for safe sharing
icon.Freeze();
button1.Icon = icon;
button2.Icon = icon;
```

### Do Not Rely on Implicit Scaling for Quality

```xml
<!-- BAD - 32px image scaled down to 16px, may look blurry -->
<Fluent:Button Icon="/Icons/icon-32.png" />

<!-- GOOD - right-sized image -->
<Fluent:Button Icon="/Icons/icon-16.png"
               LargeIcon="/Icons/icon-32.png" />
```

### Do Not Ignore DPI in Custom Templates

```xml
<!-- BAD - fixed size ignores DPI -->
<Image Width="16" Height="16" Source="{Binding Icon}" />

<!-- GOOD - use IconPresenter which handles DPI -->
<Fluent:IconPresenter IconSize="Small"
                      SmallIcon="{Binding Icon}" />
```

### Do Not Mix Icon Types Carelessly

```xml
<!-- Inconsistent - DrawingImage for some, PNG for others -->
<Fluent:Button Icon="{StaticResource VectorIcon}"
               LargeIcon="/Icons/bitmap-32.png" />

<!-- Better - consistent type -->
<Fluent:Button Icon="{StaticResource VectorIcon}"
               LargeIcon="{StaticResource VectorIconLarge}" />
```

### Do Not Forget Design-Time URIs

```xml
<!-- May fail in designer -->
<BitmapImage UriSource="Icons/save.png" />

<!-- Works in designer -->
<BitmapImage UriSource="pack://application:,,,/MyAssembly;component/Icons/save.png" />
```

### Do Not Set Icon Sizes in Code When Styles Do It

```csharp
// BAD - fights with style triggers
button.SetValue(RibbonProperties.IconSizeProperty, IconSize.Small);

// GOOD - let Size property drive IconSize through triggers
RibbonProperties.SetSize(button, RibbonControlSize.Middle);
```

---

## Source File Quick Reference

| File | Purpose |
|------|---------|
| `Controls/IconPresenter.cs` | Icon display control with size management |
| `Converters/ObjectToImageConverter.cs` | Type conversion and frame selection |
| `Converters/IconConverter.cs` | Window/app icon fallback |
| `Enumerations/IconSize.cs` | Small/Medium/Large/Custom enum |
| `ILargeIconProvider.cs` | LargeIcon interface |
| `IMediumIconProvider.cs` | MediumIcon interface |
| `AttachedProperties/RibbonProperties.cs` | IconSize and CustomIconSize attached properties |
| `Effects/GrayscaleEffect.cs` | Disabled state shader effect |
| `Internal/KnownBoxes/IconSizeBoxes.cs` | Boxed IconSize values for performance |
| `Themes/Controls/Button.xaml` | Example IconPresenter usage in templates |

---

## Common Scenarios

### Scenario: Icon Changes Based on State

```xml
<Fluent:Button Header="Toggle">
    <Fluent:Button.Style>
        <Style TargetType="Fluent:Button" BasedOn="{StaticResource {x:Type Fluent:Button}}">
            <Setter Property="LargeIcon" Value="{StaticResource OffIcon}" />
            <Style.Triggers>
                <DataTrigger Binding="{Binding IsActive}" Value="True">
                    <Setter Property="LargeIcon" Value="{StaticResource OnIcon}" />
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Fluent:Button.Style>
</Fluent:Button>
```

### Scenario: Theme-Aware Vector Icons

```xml
<DrawingImage x:Key="ThemedIcon">
    <DrawingImage.Drawing>
        <GeometryDrawing Brush="{DynamicResource Fluent.Ribbon.Brushes.LabelText}"
                         Geometry="M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z" />
    </DrawingImage.Drawing>
</DrawingImage>
```

### Scenario: Loading Icons from Embedded Resources

```csharp
var uri = new Uri("pack://application:,,,/MyAssembly;component/Resources/icon.png");
button.Icon = new BitmapImage(uri);
```

### Scenario: Custom IconPresenter Sizes

```xml
<Fluent:IconPresenter SmallSize="20,20"
                      MediumSize="28,28"
                      LargeSize="40,40"
                      IconSize="{Binding CurrentIconSize}"
                      SmallIcon="{Binding Icon}"
                      MediumIcon="{Binding MediumIcon}"
                      LargeIcon="{Binding LargeIcon}" />
```
