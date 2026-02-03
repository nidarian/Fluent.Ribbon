---
title: Fluent.Ribbon Layout & Spacing Reference
description: Button spacing, padding, margins, and sizing system
tags: [layout, spacing, margins, sizing, styling]
see_also:
  - fluent-ribbon-brushes-reference-v2.md
  - ../controls/fluent-ribbon-groupbox-reference.md
  - ../advanced/fluent-ribbon-attached-properties-reference.md
---

# Fluent.Ribbon Layout & Spacing Reference (v2)

**Purpose:** Help understand button spacing, padding, section headers, and subtitles WITHOUT hacking.

*v2 Changes: Added internal RibbonGroupBox margins, clarified panel padding, added Size enum note*

---

## STOP - Read This First

If you're about to:
- Walk the visual tree
- Reference `PART_xxx` element names
- Set properties directly on child elements

**STOP.** You're doing it wrong. Use the resource keys and properties documented below.

---

## Default Values (Common.xaml)

These can be overridden in `Application.Current.Resources`:

```xml
<!-- Button/Control spacing -->
<Thickness x:Key="Fluent.Ribbon.Values.Default.Margin">1 1 1 1</Thickness>
<Thickness x:Key="Fluent.Ribbon.Values.Default.Padding">2 0 2 0</Thickness>

<!-- Tab content area -->
<Thickness x:Key="Fluent.Ribbon.Values.RibbonTabControl.Content.Margin">8 0 8 0</Thickness>
<Thickness x:Key="Fluent.Ribbon.Values.RibbonTabControl.Content.BorderThickness">0</Thickness>
<CornerRadius x:Key="Fluent.Ribbon.Values.RibbonTabControl.Content.CornerRadius">8</CornerRadius>

<!-- Active tab indicator -->
<Thickness x:Key="Fluent.Ribbon.Values.RibbonTabItem.Active.BorderThickness">0 0 0 2</Thickness>
```

### How to Override

In App.xaml or at runtime:

```xml
<!-- App.xaml -->
<Application.Resources>
    <Thickness x:Key="Fluent.Ribbon.Values.Default.Margin">2 2 2 2</Thickness>
    <Thickness x:Key="Fluent.Ribbon.Values.Default.Padding">4 2 4 2</Thickness>
</Application.Resources>
```

Or in code:
```csharp
Application.Current.Resources["Fluent.Ribbon.Values.Default.Margin"] = new Thickness(2);
Application.Current.Resources["Fluent.Ribbon.Values.Default.Padding"] = new Thickness(4, 2, 4, 2);
```

---

## Button Layout

### Button Sizes and Heights (Button.xaml lines 253-290)

| Size | Height | IconSize | Orientation |
|------|--------|----------|-------------|
| Large | 68px | Large | Vertical (icon above text) |
| Middle | 22px | Small | Horizontal (icon beside text) |
| Small | 22px | Small | Horizontal, no text |

**IMPORTANT:** The Size enum uses `Middle`, NOT `Medium`:

```csharp
// Correct - use Middle
button.Size = RibbonControlSize.Middle;

// WRONG - Medium doesn't exist
button.Size = RibbonControlSize.Medium; // Compile error!
```

### Button Properties You CAN Set

```xml
<fluent:Button Header="My Button"
               Size="Large"
               Margin="2"
               Padding="4 2"
               Icon="{StaticResource MyIcon}"
               LargeIcon="{StaticResource MyLargeIcon}" />
```

### Button Header Template (TwoLineLabel)

Large buttons use `Fluent:TwoLineLabel` for headers, which automatically:
- Splits text into two lines
- Shows dropdown arrow if needed

The split happens at spaces. Example: "Save As" becomes:
```
Save
As ▼
```

---

## RibbonGroupBox (Section) Layout

### GroupBox Properties (RibbonGroupBox.xaml)

```xml
<fluent:RibbonGroupBox Header="File Operations"
                        Padding="4 2 4 2"
                        IsLauncherVisible="True"
                        LauncherText="More options..."
                        LauncherToolTip="Open dialog">
    <!-- buttons go here -->
</fluent:RibbonGroupBox>
```

### GroupBox Default Padding

Line 69: `<Setter Property="Padding" Value="4 2 4 2" />`

### Internal Panel Margins

The RibbonGroupBox uses internal panels with their own margins:

| Panel | Margin | Purpose |
|-------|--------|---------|
| Content Panel | `2 0 2 0` | Space around buttons |
| Header Panel | `0 1 0 0` | Space above header text |
| Launcher Button | `0 0 1 1` | Space around launcher icon |

These are defined in the template and typically don't need overriding.

### GroupBox Header (Section Title)

The header appears at the bottom of the group. Two templates available:

**One Line (default for expanded groups):**
```xml
<DataTemplate x:Key="Fluent.Ribbon.DataTemplates.RibbonGroupBox.OneLineHeader">
    <TextBlock Text="{Binding}"
               TextAlignment="Center"
               TextTrimming="CharacterEllipsis" />
</DataTemplate>
```

**Two Line (for collapsed groups):**
```xml
<DataTemplate x:Key="Fluent.Ribbon.DataTemplates.RibbonGroupBox.TwoLineHeader">
    <Fluent:TwoLineLabel Text="{Binding}" HasGlyph="True" />
</DataTemplate>
```

### Custom Header Template

To add a subtitle to a section:

```xml
<fluent:RibbonGroupBox Header="File Operations">
    <fluent:RibbonGroupBox.HeaderTemplate>
        <DataTemplate>
            <StackPanel HorizontalAlignment="Center">
                <TextBlock Text="{Binding}"
                           HorizontalAlignment="Center"
                           FontWeight="SemiBold" />
                <TextBlock Text="v2.0"
                           HorizontalAlignment="Center"
                           FontSize="9"
                           Opacity="0.6" />
            </StackPanel>
        </DataTemplate>
    </fluent:RibbonGroupBox.HeaderTemplate>

    <!-- buttons -->
</fluent:RibbonGroupBox>
```

### GroupBox Header Foreground

Override the brush:
```csharp
Application.Current.Resources["Fluent.Ribbon.Brushes.RibbonGroupBox.Header.Foreground"] = new SolidColorBrush(Colors.White);
```

---

## TwoLineLabel (Button Text with Subtitle)

`Fluent:TwoLineLabel` is what buttons use for their header text.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| Text | string | The text to display (splits at spaces) |
| HasTwoLines | bool | Force two-line layout |
| HasGlyph | bool | Show dropdown arrow |

### How Text Splits

The control splits `Text` at the last space that creates balanced lines:
- "Save" → one line
- "Save As" → "Save" / "As"
- "Export to PDF" → "Export to" / "PDF"

### Custom Button Header with Subtitle

If you need true subtitles (not just split text), use a custom HeaderTemplate:

```xml
<fluent:Button Size="Large" Icon="{StaticResource SaveIcon}">
    <fluent:Button.HeaderTemplate>
        <DataTemplate>
            <StackPanel HorizontalAlignment="Center">
                <TextBlock Text="Save"
                           HorizontalAlignment="Center"
                           FontWeight="SemiBold" />
                <TextBlock Text="Ctrl+S"
                           HorizontalAlignment="Center"
                           FontSize="9"
                           Foreground="{DynamicResource Fluent.Ribbon.Brushes.LabelText}"
                           Opacity="0.6" />
            </StackPanel>
        </DataTemplate>
    </fluent:Button.HeaderTemplate>
</fluent:Button>
```

---

## Separator Between Groups

GroupBox has a built-in separator on the right side:

```xml
<fluent:RibbonGroupBox Header="Group 1" IsSeparatorVisible="True">
```

Separator brush:
```
Fluent.Ribbon.Brushes.GroupSeparator.Background
```

---

## Complete Spacing Reference

### All Overridable Thickness Values

| Resource Key | Default | Purpose |
|--------------|---------|---------|
| `Fluent.Ribbon.Values.Default.Margin` | `1 1 1 1` | Space around controls |
| `Fluent.Ribbon.Values.Default.Padding` | `2 0 2 0` | Space inside controls |
| `Fluent.Ribbon.Values.RibbonTabControl.Content.Margin` | `8 0 8 0` | Tab content area margin |
| `Fluent.Ribbon.Values.RibbonTabItem.Active.BorderThickness` | `0 0 0 2` | Active tab underline |

### All Overridable CornerRadius Values

| Resource Key | Default | Purpose |
|--------------|---------|---------|
| `Fluent.Ribbon.Values.RibbonTabControl.Content.CornerRadius` | `8` | Tab content area corners |

---

## Key Brushes for Text

```
Fluent.Ribbon.Brushes.LabelText              - Default control text
Fluent.Ribbon.Brushes.RibbonGroupBox.Header.Foreground - Section headers
Fluent.Ribbon.Brushes.IdealForeground        - Text on accent backgrounds
Fluent.Ribbon.Brushes.Black                  - Dark text
Fluent.Ribbon.Brushes.White                  - Light text
```

---

## Source Files Reference

| What | File |
|------|------|
| Default values | `Fluent.Ribbon/Themes/Common.xaml` |
| Button template | `Fluent.Ribbon/Themes/Controls/Button.xaml` |
| GroupBox template | `Fluent.Ribbon/Themes/Controls/RibbonGroupBox.xaml` |
| TwoLineLabel | `Fluent.Ribbon/Themes/Controls/TwoLineLabel.xaml` |
| All brushes | `Fluent.Ribbon/Themes/Themes/Theme.Template.xaml` |

---

## The Right Way vs Wrong Way

### WRONG - Visual Tree Walking
```csharp
// DON'T DO THIS
foreach (var child in GetVisualChildren(groupBox))
{
    if (child is TextBlock tb)
        tb.Margin = new Thickness(5);
}
```

### RIGHT - Override Resources
```csharp
// Do this
Application.Current.Resources["Fluent.Ribbon.Values.Default.Padding"] = new Thickness(5);
```

### WRONG - Setting Internal Properties
```csharp
// DON'T DO THIS
var headerControl = FindChild<ContentControl>(groupBox, "PART_HeaderContentControl");
headerControl.Margin = new Thickness(10);
```

### RIGHT - Use HeaderTemplate
```xml
<!-- Do this -->
<fluent:RibbonGroupBox>
    <fluent:RibbonGroupBox.HeaderTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding}" Margin="10" />
        </DataTemplate>
    </fluent:RibbonGroupBox.HeaderTemplate>
</fluent:RibbonGroupBox>
```

---

## Quick Answers

**Q: How do I add more space between buttons?**
A: Override `Fluent.Ribbon.Values.Default.Margin`

**Q: How do I add padding inside buttons?**
A: Override `Fluent.Ribbon.Values.Default.Padding` or set `Padding` on individual buttons

**Q: How do I add a subtitle to a section?**
A: Use a custom `HeaderTemplate` on `RibbonGroupBox`

**Q: How do I add a subtitle to a button?**
A: Use a custom `HeaderTemplate` on the button

**Q: How do I change the section header color?**
A: Override `Fluent.Ribbon.Brushes.RibbonGroupBox.Header.Foreground`

**Q: Why isn't my property change working?**
A: You're probably setting it on internal elements. Use templates or resource overrides instead.

**Q: What's the difference between Middle and Medium size?**
A: There is no Medium! The enum values are `Large`, `Middle`, and `Small`.

---

*This reference exists to prevent visual tree walking and hacking. Use the documented customization points.*
