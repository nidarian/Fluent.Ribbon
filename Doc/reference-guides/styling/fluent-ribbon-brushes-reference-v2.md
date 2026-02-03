---
title: Fluent.Ribbon Brushes Reference
description: Complete list of all ~145 brush resource keys that can be overridden
tags: [brushes, colors, styling, resources]
see_also:
  - controlzex-theming-reference-v2.md
  - fluent-ribbon-layout-reference-v2.md
---

# Fluent.Ribbon Brushes Reference (v2)

**Complete list of all brush resource keys that can be overridden.**

*v2 Changes: Added Images/QAT brushes, ApplicationMenuItem brushes, RibbonTabItem contextual opacity mask, Ribbon core background, LinearGradientBrush note*

Source: `Fluent.Ribbon/Themes/Themes/Theme.Template.xaml`

---

## How to Override Brushes

```csharp
// In code
var brush = new SolidColorBrush(Colors.DarkBlue);
brush.Freeze();  // Important for performance
Application.Current.Resources["Fluent.Ribbon.Brushes.AccentBase"] = brush;
```

```xml
<!-- In App.xaml -->
<Application.Resources>
    <SolidColorBrush x:Key="Fluent.Ribbon.Brushes.AccentBase" Color="#0078D4" />
</Application.Resources>
```

---

## Accent Brushes (Brand Colors)

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.AccentBase` | Primary accent color |
| `Fluent.Ribbon.Brushes.Accent80` | 80% opacity accent |
| `Fluent.Ribbon.Brushes.Accent60` | 60% opacity accent |
| `Fluent.Ribbon.Brushes.Accent40` | 40% opacity accent |
| `Fluent.Ribbon.Brushes.Accent20` | 20% opacity accent |
| `Fluent.Ribbon.Brushes.Highlight` | Selection/highlight color |

### Accent Light Variants
| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.AccentLight1` | Light accent variant 1 |
| `Fluent.Ribbon.Brushes.AccentLight1.Foreground` | Text on AccentLight1 |
| `Fluent.Ribbon.Brushes.AccentLight2` | Light accent variant 2 |
| `Fluent.Ribbon.Brushes.AccentLight2.Foreground` | Text on AccentLight2 |
| `Fluent.Ribbon.Brushes.AccentLight3` | Light accent variant 3 |
| `Fluent.Ribbon.Brushes.AccentLight3.Foreground` | Text on AccentLight3 |

### Accent Dark Variants
| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.AccentDark1` | Dark accent variant 1 |
| `Fluent.Ribbon.Brushes.AccentDark1.Foreground` | Text on AccentDark1 |
| `Fluent.Ribbon.Brushes.AccentDark2` | Dark accent variant 2 |
| `Fluent.Ribbon.Brushes.AccentDark2.Foreground` | Text on AccentDark2 |
| `Fluent.Ribbon.Brushes.AccentDark3` | Dark accent variant 3 |
| `Fluent.Ribbon.Brushes.AccentDark3.Foreground` | Text on AccentDark3 |

---

## Base Color Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.White` | White backgrounds |
| `Fluent.Ribbon.Brushes.White20` | 20% white |
| `Fluent.Ribbon.Brushes.Black` | Black text/borders |
| `Fluent.Ribbon.Brushes.Black20` | 20% black |
| `Fluent.Ribbon.Brushes.Gray1` | Darkest gray |
| `Fluent.Ribbon.Brushes.Gray2` | |
| `Fluent.Ribbon.Brushes.Gray3` | |
| `Fluent.Ribbon.Brushes.Gray4` | |
| `Fluent.Ribbon.Brushes.Gray5` | Mid gray |
| `Fluent.Ribbon.Brushes.Gray6` | |
| `Fluent.Ribbon.Brushes.Gray7` | |
| `Fluent.Ribbon.Brushes.Gray8` | |
| `Fluent.Ribbon.Brushes.Gray9` | |
| `Fluent.Ribbon.Brushes.Gray10` | Lightest gray |
| `Fluent.Ribbon.Brushes.TransparentWhite` | Fully transparent |
| `Fluent.Ribbon.Brushes.HighTransparentWhite` | Slightly visible white |

---

## Text/Foreground Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.LabelText` | Default control text |
| `Fluent.Ribbon.Brushes.IdealForeground` | Text on accent backgrounds |
| `Fluent.Ribbon.Brushes.IdealForegroundDisabled` | Disabled text on accent |
| `Fluent.Ribbon.Brushes.DarkIdealForegroundDisabled` | Disabled dark text |

---

## Ribbon Core Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.Ribbon.Background` | Main ribbon control background (default: Transparent) |

---

## Button Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.Button.MouseOver.Background` | Hover state background |
| `Fluent.Ribbon.Brushes.Button.MouseOver.Border` | Hover state border |
| `Fluent.Ribbon.Brushes.Button.Pressed.Background` | Pressed state background |
| `Fluent.Ribbon.Brushes.Button.Pressed.Border` | Pressed state border |

---

## ToggleButton Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.ToggleButton.Checked.Background` | Checked state |
| `Fluent.Ribbon.Brushes.ToggleButton.Checked.Border` | Checked border |
| `Fluent.Ribbon.Brushes.ToggleButton.CheckedMouseOver.Background` | Checked + hover |
| `Fluent.Ribbon.Brushes.ToggleButton.CheckedMouseOver.Border` | Checked + hover border |

---

## RibbonWindow Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.RibbonWindow.Background` | Main window background |
| `Fluent.Ribbon.Brushes.RibbonWindow.Background.Backdrop.Acrylic` | Acrylic effect |
| `Fluent.Ribbon.Brushes.RibbonWindow.Background.Backdrop.Auto` | Auto backdrop |
| `Fluent.Ribbon.Brushes.RibbonWindow.TitleBackground` | Title bar background |
| `Fluent.Ribbon.Brushes.RibbonWindow.TitleForeground` | Title bar text |

---

## RibbonTabControl Brushes (Tab Row)

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.RibbonTabControl.Background` | Tab row background |
| `Fluent.Ribbon.Brushes.RibbonTabControl.Foreground` | Tab row text |
| `Fluent.Ribbon.Brushes.RibbonTabControl.Content.Background` | Content area background |
| `Fluent.Ribbon.Brushes.RibbonTabControl.Content.Border` | Content area border |
| `Fluent.Ribbon.Brushes.RibbonTabControl.Content.Foreground` | Content area text |
| `Fluent.Ribbon.Brushes.RibbonTabControl.TabsGrid.Background` | Tabs grid background |
| `Fluent.Ribbon.Brushes.RibbonTabControl.TabsGrid.Foreground` | Tabs grid text |

---

## RibbonTabItem Brushes (Individual Tabs)

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.RibbonTabItem.Foreground` | Tab text |
| `Fluent.Ribbon.Brushes.RibbonTabItem.Border` | Tab border (also used for Active state) |
| `Fluent.Ribbon.Brushes.RibbonTabItem.Active.Background` | Selected tab background |
| `Fluent.Ribbon.Brushes.RibbonTabItem.MouseOver.Background` | Hover background |
| `Fluent.Ribbon.Brushes.RibbonTabItem.MouseOver.Foreground` | Hover text |
| `Fluent.Ribbon.Brushes.RibbonTabItem.Selected.Foreground` | Selected tab text |
| `Fluent.Ribbon.Brushes.RibbonTabItem.Selected.MouseOver.Foreground` | Selected + hover text |
| `Fluent.Ribbon.Brushes.RibbonTabItem.Contextual.Background.OpacityMask` | Contextual tab dark overlay (20% opacity) |

---

## RibbonGroupBox Brushes (Sections)

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.RibbonGroupBox.Header.Foreground` | Section header text |

---

## Backstage Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.Backstage.Background` | Backstage background |
| `Fluent.Ribbon.Brushes.Backstage.Foreground` | Backstage text |
| `Fluent.Ribbon.Brushes.Backstage.BackButton.Background` | Back button |
| `Fluent.Ribbon.Brushes.Backstage.BackButton.Foreground` | Back button text |
| `Fluent.Ribbon.Brushes.BackstageTabControl.Background` | Tab control bg |
| `Fluent.Ribbon.Brushes.BackstageTabControl.Button.MouseOver.Background` | Hover |
| `Fluent.Ribbon.Brushes.BackstageTabControl.ItemsPanelBackground` | Items panel |
| `Fluent.Ribbon.Brushes.BackstageTabItem.Header.Foreground` | Tab item text |
| `Fluent.Ribbon.Brushes.BackstageTabItem.MouseOver.Background` | Tab hover |
| `Fluent.Ribbon.Brushes.BackstageTabItem.Selected.Background` | Selected tab |

---

## DropDown/Popup Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.DropDown.Background` | Dropdown background |
| `Fluent.Ribbon.Brushes.DropDown.Border` | Dropdown border |
| `Fluent.Ribbon.Brushes.DropDown.Resize.Border` | Resize grip border |
| `Fluent.Ribbon.Brushes.DropDown.Resize.Background` | Resize grip bg |

---

## TextBox Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.TextBox.Background` | Default background |
| `Fluent.Ribbon.Brushes.TextBox.Border` | Default border |
| `Fluent.Ribbon.Brushes.TextBox.Caret` | Cursor color |
| `Fluent.Ribbon.Brushes.TextBox.Selection` | Selection highlight |
| `Fluent.Ribbon.Brushes.TextBox.MouseOver.Background` | Hover background |
| `Fluent.Ribbon.Brushes.TextBox.MouseOver.Border` | Hover border |
| `Fluent.Ribbon.Brushes.TextBox.Focus.Background` | Focused background |
| `Fluent.Ribbon.Brushes.TextBox.Focus.Border` | Focused border |
| `Fluent.Ribbon.Brushes.TextBox.Disabled.Background` | Disabled background |
| `Fluent.Ribbon.Brushes.TextBox.Disabled.Border` | Disabled border |

---

## CheckBox Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.CheckBox.Background` | Box background |
| `Fluent.Ribbon.Brushes.CheckBox.Border` | Box border |
| `Fluent.Ribbon.Brushes.CheckBox.MouseOver.Background` | Hover background |
| `Fluent.Ribbon.Brushes.CheckBox.MouseOver.Stroke` | Hover stroke |
| `Fluent.Ribbon.Brushes.CheckBox.Pressed.Stroke` | Pressed stroke |

---

## Gallery Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.Gallery.Header.Background` | Gallery header |
| `Fluent.Ribbon.Brushes.InRibbonGallery.Content.Background` | In-ribbon gallery |
| `Fluent.Ribbon.Brushes.GalleryGroupContainer.Header.Background` | Group header |
| `Fluent.Ribbon.Brushes.GalleryItem.MouseOver` | Item hover |
| `Fluent.Ribbon.Brushes.GalleryItem.Selected` | Item selected |
| `Fluent.Ribbon.Brushes.GalleryItem.Pressed` | Item pressed |
| `Fluent.Ribbon.Brushes.ColorGallery.Background` | Color gallery bg |
| `Fluent.Ribbon.Brushes.ColorGallery.Item.Border` | Color item border |

---

## Separator Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.Separator.Background` | Separator line |
| `Fluent.Ribbon.Brushes.Separator.Border` | Separator border |
| `Fluent.Ribbon.Brushes.GroupSeparator.Background` | Group separator |

---

## Window Controls Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Foreground` | Min/Max/Close text |
| `Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background` | Button background |
| `Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.MouseOver.Background` | Hover |
| `Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Pressed.Background` | Pressed |
| `Fluent.Ribbon.Brushes.WindowCommands.CloseButton.MouseOver.Background` | Close hover (red) |
| `Fluent.Ribbon.Brushes.WindowCommands.CloseButton.Pressed.Background` | Close pressed |

---

## Images/Icons Brushes (NEW in v2)

These control the color of Quick Access Toolbar icons and ribbon display option icons.

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.Images.QuickAccessToolbarDropDown` | QAT customize dropdown icon (above ribbon) |
| `Fluent.Ribbon.Brushes.Images.QuickAccessToolbarDropDown.BelowRibbon` | QAT dropdown icon (below ribbon) |
| `Fluent.Ribbon.Brushes.Images.QuickAccessToolbarExtender` | QAT overflow dropdown icon (above ribbon) |
| `Fluent.Ribbon.Brushes.Images.QuickAccessToolbarExtender.BelowRibbon` | QAT overflow icon (below ribbon) |
| `Fluent.Ribbon.Brushes.Images.RibbonDisplayOptions` | Display options menu icon |

---

## ApplicationMenuItem Brushes (NEW in v2)

| Key | Used For | Type |
|-----|----------|------|
| `Fluent.Ribbon.Brushes.ApplicationMenuItem.CheckBox.Background` | Checkbox background in app menu | SolidColorBrush |
| `Fluent.Ribbon.Brushes.ApplicationMenuItem.CheckBox.Border` | Checkbox border in app menu | SolidColorBrush |
| `Fluent.Ribbon.Brushes.MenuItem.SubMenu.Arrow.Fill` | Submenu arrow fill | **LinearGradientBrush** |

**Note:** `MenuItem.SubMenu.Arrow.Fill` is the **only LinearGradientBrush** in the theme. Override carefully:

```csharp
// Override submenu arrow (must be gradient, not solid)
var gradient = new LinearGradientBrush();
gradient.StartPoint = new Point(0.055, 0.128);
gradient.EndPoint = new Point(0.945, 0.872);
gradient.GradientStops.Add(new GradientStop(Colors.Gray, 0.0));
gradient.GradientStops.Add(new GradientStop(Colors.DarkGray, 1.0));
gradient.Freeze();
Application.Current.Resources["Fluent.Ribbon.Brushes.MenuItem.SubMenu.Arrow.Fill"] = gradient;
```

---

## Contextual Tab Group Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.RibbonContextualTabGroup.Background.OpacityMask` | Group opacity |
| `Fluent.Ribbon.Brushes.RibbonContextualTabGroup.TabItemForeground` | Tab text |
| `Fluent.Ribbon.Brushes.RibbonContextualTabGroup.TabItemSelectedForeground` | Selected text |
| `Fluent.Ribbon.Brushes.RibbonContextualTabGroup.TabItemMouseOverForeground` | Hover text |
| `Fluent.Ribbon.Brushes.RibbonContextualTabGroup.TabItemSelectedMouseOverForeground` | Selected+hover |

---

## Misc Brushes

| Key | Used For |
|-----|----------|
| `Fluent.Ribbon.Brushes.Control.Border` | Generic control border |
| `Fluent.Ribbon.Brushes.Control.Disabled.Border` | Disabled control border |
| `Fluent.Ribbon.Brushes.ExtremeHighlight` | Extreme highlight |
| `Fluent.Ribbon.Brushes.DarkExtremeHighlight` | Dark extreme highlight |
| `Fluent.Ribbon.Brushes.KeyTip.Background` | KeyTip background |
| `Fluent.Ribbon.Brushes.KeyTip.Border` | KeyTip border |
| `Fluent.Ribbon.Brushes.MenuItem.Background` | Menu item background |
| `Fluent.Ribbon.Brushes.ScreenTip.Background` | Tooltip background |
| `Fluent.Ribbon.Brushes.ScreenTip.Border` | Tooltip border |
| `Fluent.Ribbon.Brushes.ScrollBar.Background` | Scrollbar background |
| `Fluent.Ribbon.Brushes.ScrollButton.Default.Background` | Scroll button |
| `Fluent.Ribbon.Brushes.ScrollButton.Default.Border` | Scroll button border |
| `Fluent.Ribbon.Brushes.ScrollThumb.Default.Background` | Scroll thumb |
| `Fluent.Ribbon.Brushes.ScrollThumb.Default.Border` | Scroll thumb border |
| `Fluent.Ribbon.Brushes.ScrollViewer.Button.Background` | Scroll viewer button |
| `Fluent.Ribbon.Brushes.ScrollViewer.Button.Border` | Scroll viewer border |

---

## Quick Dark Mode Setup

```csharp
// Minimal dark mode - override these key brushes
Application.Current.Resources["Fluent.Ribbon.Brushes.RibbonWindow.Background"] = new SolidColorBrush(Color.FromRgb(30, 30, 30));
Application.Current.Resources["Fluent.Ribbon.Brushes.RibbonTabControl.Background"] = new SolidColorBrush(Color.FromRgb(45, 45, 45));
Application.Current.Resources["Fluent.Ribbon.Brushes.RibbonTabControl.Content.Background"] = new SolidColorBrush(Color.FromRgb(37, 37, 37));
Application.Current.Resources["Fluent.Ribbon.Brushes.LabelText"] = new SolidColorBrush(Colors.White);
Application.Current.Resources["Fluent.Ribbon.Brushes.RibbonGroupBox.Header.Foreground"] = new SolidColorBrush(Colors.LightGray);
```

---

## Better: Use ThemeManager

```csharp
// Best approach - let the library handle dark mode
ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Blue");

// Or sync with Windows
ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
ThemeManager.Current.SyncTheme();
```

---

## Brush Count Summary

| Category | Count |
|----------|-------|
| Accent (including Light/Dark variants) | 20 |
| Base Colors | 16 |
| Text/Foreground | 4 |
| Ribbon Core | 1 |
| Button | 4 |
| ToggleButton | 4 |
| RibbonWindow | 5 |
| RibbonTabControl | 7 |
| RibbonTabItem | 8 |
| RibbonGroupBox | 1 |
| Backstage | 10 |
| DropDown/Popup | 4 |
| TextBox | 10 |
| CheckBox | 5 |
| Gallery | 8 |
| Separator | 3 |
| Window Controls | 6 |
| Images/Icons | 5 |
| ApplicationMenuItem | 3 |
| Contextual Tab Group | 5 |
| Misc | 16 |
| **Total** | **~145** |
