# High Contrast mode (basic, opt-in)

Upstream issue: [#1018](https://github.com/fluentribbon/Fluent.Ribbon/issues/1018).

Fluent.Ribbon ships no High Contrast theme. This adds the smallest piece that
makes one possible: when a theme is generated at runtime with
`isHighContrast = true`, `RibbonLibraryThemeProvider.FillColorSchemeValues`
fills the palette from `System.Windows.SystemColors` instead of the accent
color. Nothing changes for apps that don't opt in: the shipped themes
(`Light.Blue.xaml` and so on) are untouched, and runtime themes generated with
`isHighContrast = false` get exactly the same values as before.

Nothing here was compiled or run (no .NET SDK with WPF was available). The
ControlzEx behavior below was read from the ControlzEx 7.0.3 assembly (IL), and
the colors still need the manual checks at the end.

## How an app turns it on

Let ControlzEx follow the Windows setting (also works with `ThemeSyncMode.SyncAll`):

```csharp
// App.OnStartup, before base.OnStartup(e)
ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithHighContrast; // or combine flags, e.g. SyncWithAppMode | SyncWithHighContrast
ThemeManager.Current.SyncTheme();
```

Or generate and apply a High Contrast theme yourself:

```csharp
var theme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme(ThemeManager.BaseColorLight, accentColor, isHighContrast: true);
ThemeManager.Current.ChangeTheme(Application.Current, theme!);
```

`ThemeManager`, `ThemeSyncMode` and `RuntimeThemeGenerator` are in `ControlzEx.Theming`.

## How it works

1. `ThemeManager.SyncTheme` reads `SystemParameters.HighContrast` (only when
   `SyncWithHighContrast` is set) and asks for a theme with
   `IsHighContrast == true`. Fluent ships none, so ControlzEx calls
   `RuntimeThemeGenerator.GenerateRuntimeThemeFromWindowsSettings(baseColor, isHighContrast: true, providers)`.
2. For every library theme provider, ControlzEx builds a fresh values
   dictionary, calls `FillColorSchemeValues(values, colorValues)` with
   `colorValues.Options.IsHighContrast == true`, and then fills the
   `{{placeholders}}` of `Theme.Template.xaml`.
3. Placeholders are replaced with these values first, then with the base color
   scheme values ("Light"/"Dark") and the default values from
   `GeneratorParameters.json`. The first replacement wins, so keys set by the
   provider override the base and default values. That's why the High Contrast
   palette is the same for "Light" and "Dark".
4. ControlzEx also sets `Theme.IsHighContrast` to `true` on the generated
   dictionary (the template's `{{IsHighContrast}}` gets `True`).
5. ControlzEx listens to `SystemParameters.HighContrast` changes (and to
   `UserPreferenceChanged` with category General) and syncs again.

The colors are read once, when the theme is generated. The brushes are frozen
and use `StaticResource`, so they don't follow later system color changes by
themselves; only a new sync (a new generated theme) does.

## Mapping

Rule: Windows High Contrast themes only promise readable contrast for their
own pairs (WindowText on Window, HighlightText on Highlight, ControlText on
Control, GrayText on Window, HotTrack on Window). Fluent's templates draw normal
text with `Fluent.Ribbon.Colors.Black` and don't switch the text color on hover
or selection. So every surface that has normal text on it gets Window or
Control (never Highlight), and states stay visible through borders.

| Fluent key | SystemColors | Why |
| --- | --- | --- |
| `Colors.Black` | `WindowTextColor` | Main text and glyph color |
| `Colors.Black20` | `GrayTextColor` | Subdued text, made opaque |
| `Colors.Gray1` | `WindowTextColor` | Selected tab text, CheckBox stroke; KeyTip background (KeyTip text is `White`, so KeyTips are inverted) |
| `Colors.Gray2` | `WindowTextColor` | Hover borders, tab/backstage underlines, KeyTip border |
| `Colors.Gray3` | `WindowTextColor` | Pressed border; Gallery header background (its text is `White`) |
| `Colors.Gray4` .. `Colors.Gray7` | `WindowTextColor` | Control, drop down, scroll and separator borders |
| `Colors.Gray8`, `Colors.Gray9`, `Colors.Gray10` | `ControlColor` | Fills: group headers, resize grip, scroll buttons, disabled TextBox |
| `Colors.White` | `WindowColor` | Main background |
| `Colors.White20` | `WindowColor` | Made opaque |
| `Colors.DarkIdealForegroundDisabled` | `GrayTextColor` | Disabled caption buttons |
| `Colors.AccentBase`, `Colors.Accent80`, `Colors.Accent60` | `HighlightColor` | Accent surfaces (StatusBar, application menu button, backstage back button) and accent borders |
| `Colors.Accent40`, `Colors.Accent20` | `ControlColor` | Backgrounds behind normal text (checked ToggleButton, caption button hover/pressed) |
| `Colors.Highlight` | `HighlightColor` | Checked border, RadioButton |
| `Colors.IdealForeground` | `HighlightTextColor` | Text on accent surfaces |
| `Colors.AccentLight1..3` | `ControlColor` | Button and gallery item hover/pressed/selected backgrounds behind normal text |
| `Colors.AccentLight1..3.Foreground` | `ControlTextColor` | Text on those |
| `Colors.AccentDark1..3` | `HotTrackColor` | AccentDark3 is text on the window background (contextual tab headers) and the tab border |
| `Colors.AccentDark1..3.Foreground` | `WindowColor` | Counterpart of HotTrack |
| `Brushes.RibbonWindow.Background` (+ `.Backdrop.Acrylic`, `.Backdrop.Auto`) | `WindowColor` | Literal colors in GeneratorParameters.json |
| `Brushes.RibbonTabControl.Content.Background`, `Brushes.DropDown.Background`, `Brushes.BackstageTabControl.Background` | `WindowColor` | Literal colors in GeneratorParameters.json |
| `Brushes.BackstageTabControl.ItemsPanelBackground` | `ControlColor` | Literal color in GeneratorParameters.json |
| `Brushes.Backstage.BackButton.Foreground` | `HighlightTextColor` | Its background is AccentBase (Highlight) |

All keys are prefixed with `Fluent.Ribbon.`.

## What is NOT covered

Hard-coded colors (found with grep in `Fluent.Ribbon/Themes/**`):

- `Theme.Template.xaml`: `WindowCommands.CloseButton.MouseOver.Background`
  `#E81123` and `.Pressed.Background` `#A92831` (red close button),
  `RibbonContextualTabGroup.Background.OpacityMask` and
  `RibbonTabItem.Contextual.Background.OpacityMask` `#14000000`,
  `Colors.TransparentWhite` / `Colors.HighTransparentWhite`.
  `TextBox.Selection` already uses `SystemColors.HighlightColor`, but only as read when the dictionary loads.
- `GeneratorParameters.json` default values that are left as they are:
  `ExtremeHighlight` `#FFD232` and `DarkExtremeHighlight` `#F29536`
  (ColorGallery selected swatch border, Gallery filter label on hover),
  `ApplicationMenuItem.CheckBox.Background` `#FCF1C2` and `.Border` `#F29536`.
- `RibbonWindow.xaml`: `NonActiveBorderBrush` and `NonActiveGlowColor` `#434346`.
- `Controls/Slider.xaml`: thumb background `Red` on hover (three triggers).
- `Controls/ApplicationMenuItem.xaml`: shadow gradients `#3F000000`.
- `Images.xaml`: gradient stops with fixed colors. Icons and images in general
  (Fluent's and the app's) are not recolored; disabled icons use the grayscale
  effect.

Other gaps:

- Disabled states mostly use opacity (about 26 `IsEnabled = False` triggers
  set `Opacity`), and `Brushes.IdealForegroundDisabled` is IdealForeground at
  40 % opacity. Disabled text is a faded WindowText, not `GrayText`.
- Trade-offs of the mapping: gallery item hover/selected/pressed backgrounds
  are `ControlColor`, so the selected gallery item is not visible; a checked
  ToggleButton is only shown by its Highlight border; the TextBox hover border
  (`Accent40`) blends with the background; ColorGallery item borders and the
  disabled TextBox border (`Gray8`) blend too; the disabled caption button text
  uses `Brushes.White` (= Window) and disappears.
- The Gallery filter label is `White` (= Window) on `Gray3` (= WindowText),
  and turns `ExtremeHighlight` on hover.
- Colors the app sets itself (for example a `RibbonContextualTabGroup.Background`).
- When High Contrast is turned off again, ControlzEx does not go back to the
  shipped theme (for example `Light.Blue`): it generates a normal runtime theme
  from the Windows accent color, because the current color scheme is now the
  runtime one.
- `GenerateRuntimeThemeFromWindowsSettings` returns no theme when the Windows
  accent color can't be read, and then nothing changes.
- Switching from one High Contrast theme to another while High Contrast stays
  on: `SystemParameters.HighContrast` doesn't change, so this relies on
  ControlzEx's `UserPreferenceChanged` (General) handler. Not verified; an app
  can call `ThemeManager.Current.SyncTheme()` again itself.

## Manual checks needed

Check under each of the four Windows High Contrast themes (Windows 11:
Aquatic, Desert, Dusk, Night sky; Windows 10: High Contrast #1, #2, Black,
White), with the app started with High Contrast on and also with High Contrast
switched on and off while it runs, and once switching between two High
Contrast themes:

- Ribbon tabs: normal, hover, selected, contextual groups; group headers;
  minimized ribbon popup.
- Buttons, split and drop down buttons, toggle buttons: hover, pressed,
  checked, disabled.
- Menus and drop downs, in-ribbon and drop down galleries, ColorGallery.
- TextBox, ComboBox, Spinner: normal, hover, focus, disabled.
- KeyTips (press Alt), ScreenTips, Quick Access Toolbar, StatusBar.
- Backstage: tab items, back button, buttons inside.
- RibbonWindow: title, caption buttons (the close button stays red), inactive window border.
