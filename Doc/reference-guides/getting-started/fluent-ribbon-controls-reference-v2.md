---
title: Fluent.Ribbon Controls Reference
description: Quick reference for all major Fluent.Ribbon controls and their properties
tags: [controls, overview, getting-started, reference]
see_also:
  - ../controls/fluent-ribbon-groupbox-reference.md
  - ../controls/fluent-ribbon-button-controls-reference.md
  - fluent-ribbon-common-tasks-v2.md
---

# Fluent.Ribbon Controls Reference (v2)

**Quick reference for all major Fluent.Ribbon controls and their properties.**

*v2 Changes: Added RadioButton, ColorGallery, StatusBar components, RibbonWindow, ScreenTip, KeyTip, StartScreen, QuickAccessToolBar*

---

## Control Hierarchy

```
fluent:RibbonWindow
├── fluent:Ribbon
│   ├── fluent:Ribbon.Menu → fluent:ApplicationMenu or fluent:Backstage
│   ├── fluent:RibbonTabItem (multiple)
│   │   └── fluent:RibbonGroupBox (multiple)
│   │       └── Controls (Button, DropDownButton, etc.)
│   ├── fluent:Ribbon.QuickAccessToolBar → fluent:QuickAccessToolBar
│   └── fluent:Ribbon.ContextualGroups → fluent:RibbonContextualTabGroup
└── fluent:StatusBar (optional)
```

---

## fluent:RibbonWindow (NEW in v2)

The base window class for ribbon applications. Inherits from ControlzEx.WindowChromeWindow.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `TitleBar` | RibbonTitleBar | The title bar control |
| `GlowColor` | Color? | Window glow color when active |
| `NonActiveGlowColor` | Color? | Glow color when inactive |
| `GlowDepth` | int | Glow thickness in pixels (default: 9) |
| `IsGlowTransitionEnabled` | bool | Animate glow color changes |
| `DWMSupportsBorderColor` | bool | (read-only) Windows 11 feature detection |
| `CornerPreference` | WindowCornerPreference | Window corner style (Windows 11) |

### Example

```xml
<fluent:RibbonWindow x:Class="MyApp.MainWindow"
                     xmlns:fluent="urn:fluent-ribbon"
                     Title="My Application"
                     GlowColor="{DynamicResource Fluent.Ribbon.Colors.AccentBase}"
                     NonActiveGlowColor="#434346">
    <Grid>
        <fluent:Ribbon />
    </Grid>
</fluent:RibbonWindow>
```

---

## fluent:Ribbon

The main ribbon container.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Menu` | UIElement | ApplicationMenu or Backstage |
| `IsMinimized` | bool | Collapse ribbon to tabs only |
| `IsCollapsed` | bool | Full collapse (narrow window) |
| `IsSimplified` | bool | Simplified ribbon mode |
| `SelectedTabItem` | RibbonTabItem | Currently selected tab |
| `SelectedTabIndex` | int | Index of selected tab |
| `ShowQuickAccessToolBarAboveRibbon` | bool | QAT position |
| `IsQuickAccessToolBarVisible` | bool | Show/hide QAT |
| `CanMinimize` | bool | Allow minimizing |
| `CanUseSimplified` | bool | Allow simplified mode |
| `AreTabHeadersVisible` | bool | Show tab headers |
| `ContentHeight` | double | Height of content area |
| `QuickAccessToolBar` | QuickAccessToolBar | The QAT control (read-only) |

### QAT Methods

| Method | Description |
|--------|-------------|
| `AddToQuickAccessToolBar(UIElement)` | Add control to QAT (creates clone) |
| `RemoveFromQuickAccessToolBar(UIElement)` | Remove control from QAT |
| `IsInQuickAccessToolBar(UIElement)` | Check if control is in QAT |
| `ClearQuickAccessToolBar()` | Remove all items from QAT |

### Example

```xml
<fluent:Ribbon IsMinimized="False"
               CanMinimize="True"
               ShowQuickAccessToolBarAboveRibbon="True">
    <fluent:Ribbon.Menu>
        <fluent:ApplicationMenu Header="File">
            <!-- menu items -->
        </fluent:ApplicationMenu>
    </fluent:Ribbon.Menu>

    <fluent:RibbonTabItem Header="Home">
        <!-- groups -->
    </fluent:RibbonTabItem>
</fluent:Ribbon>
```

---

## fluent:RibbonTabItem

A tab in the ribbon.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Tab header text |
| `HeaderTemplate` | DataTemplate | Custom header template |
| `IsSelected` | bool | Is this tab active |
| `KeyTip` | string | Keyboard shortcut letter |
| `Group` | RibbonContextualTabGroup | For contextual tabs |
| `HeaderPadding` | Thickness | Padding around header (default: 9,3,9,6) |
| `ActiveTabBackground` | Brush | Background when selected |
| `ActiveTabBorderBrush` | Brush | Border when selected |

### Example

```xml
<fluent:RibbonTabItem Header="Home" KeyTip="H">
    <fluent:RibbonGroupBox Header="Clipboard">
        <!-- controls -->
    </fluent:RibbonGroupBox>
</fluent:RibbonTabItem>
```

---

## fluent:RibbonGroupBox

A section/group within a tab.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Section header text |
| `HeaderTemplate` | DataTemplate | Custom header template |
| `Padding` | Thickness | Internal padding (default: 4,2,4,2) |
| `Icon` | object | Icon for collapsed state |
| `LargeIcon` | object | Large icon |
| `MediumIcon` | object | Medium icon |
| `IsLauncherVisible` | bool | Show launcher button |
| `LauncherCommand` | ICommand | Launcher click command |
| `LauncherText` | string | Launcher tooltip text |
| `LauncherToolTip` | object | Launcher tooltip |
| `LauncherIcon` | object | Launcher icon |
| `LauncherKeys` | string | Launcher KeyTip |
| `IsSeparatorVisible` | bool | Show right separator |
| `State` | RibbonGroupBoxState | Large/Middle/Small/Collapsed/QuickAccess |
| `KeyTip` | string | Keyboard shortcut |

### States (RibbonGroupBoxState enum)
- `Large` (0) - Full size, large icons
- `Middle` (1) - Medium size
- `Small` (2) - Small icons
- `Collapsed` (3) - Dropdown only
- `QuickAccess` (4) - In Quick Access Toolbar

### Example

```xml
<fluent:RibbonGroupBox Header="File Operations"
                        IsLauncherVisible="True"
                        LauncherCommand="{Binding OpenSettingsCommand}"
                        LauncherKeys="FO"
                        IsSeparatorVisible="True">
    <fluent:Button Header="Open" Icon="{StaticResource OpenIcon}" />
    <fluent:Button Header="Save" Icon="{StaticResource SaveIcon}" />
</fluent:RibbonGroupBox>
```

---

## fluent:Button

Standard ribbon button.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Button text |
| `HeaderTemplate` | DataTemplate | Custom text template |
| `Icon` | object | Small icon (16x16) |
| `MediumIcon` | object | Medium icon (24x24) |
| `LargeIcon` | object | Large icon (32x32) |
| `Size` | RibbonControlSize | Large/Middle/Small |
| `Command` | ICommand | Click command |
| `CommandParameter` | object | Command parameter |
| `KeyTip` | string | Keyboard shortcut |
| `IsDefinitive` | bool | Closes dropdown when clicked |
| `CanAddToQuickAccessToolBar` | bool | Allow adding to QAT |

### Sizes
- `Large` - 68px height, vertical layout, large icon
- `Middle` - 22px height, horizontal layout, small icon
- `Small` - 22px height, icon only

### Example

```xml
<fluent:Button Header="Paste"
               Size="Large"
               LargeIcon="{StaticResource PasteIcon32}"
               Icon="{StaticResource PasteIcon16}"
               Command="{Binding PasteCommand}"
               KeyTip="V" />
```

---

## fluent:RadioButton (NEW in v2)

Radio button for mutually exclusive options.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Button text |
| `Icon` | object | Small icon |
| `LargeIcon` | object | Large icon |
| `MediumIcon` | object | Medium icon |
| `Size` | RibbonControlSize | Large/Middle/Small |
| `IsChecked` | bool | Selected state |
| `GroupName` | string | Radio group name |
| `KeyTip` | string | Keyboard shortcut |
| `CanAddToQuickAccessToolBar` | bool | Allow adding to QAT |

### Example

```xml
<fluent:RibbonGroupBox Header="View">
    <fluent:RadioButton Header="Normal" GroupName="ViewMode"
                        IsChecked="{Binding IsNormalView}" />
    <fluent:RadioButton Header="Page Layout" GroupName="ViewMode"
                        IsChecked="{Binding IsPageLayoutView}" />
    <fluent:RadioButton Header="Outline" GroupName="ViewMode"
                        IsChecked="{Binding IsOutlineView}" />
</fluent:RibbonGroupBox>
```

---

## fluent:ToggleButton

Checkable button.

### Key Properties

All Button properties, plus:

| Property | Type | Description |
|----------|------|-------------|
| `IsChecked` | bool? | Toggle state |
| `GroupName` | string | Radio group name |

### Example

```xml
<fluent:ToggleButton Header="Bold"
                      Size="Small"
                      Icon="{StaticResource BoldIcon}"
                      IsChecked="{Binding IsBold}"
                      KeyTip="B" />
```

---

## fluent:DropDownButton

Button with dropdown menu.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Button text |
| `Icon` / `LargeIcon` / `MediumIcon` | object | Icons |
| `Size` | RibbonControlSize | Large/Middle/Small |
| `IsDropDownOpen` | bool | Dropdown state |
| `HasTriangle` | bool | Show dropdown arrow |
| `ResizeMode` | ContextMenuResizeMode | None/Vertical/Both |
| `MaxDropDownHeight` | double | Max dropdown height |
| `DropDownHeight` | double | Fixed dropdown height |
| `KeyTip` | string | Keyboard shortcut |
| `Items` | Collection | Dropdown items |

### Example

```xml
<fluent:DropDownButton Header="New"
                        Size="Large"
                        LargeIcon="{StaticResource NewIcon}">
    <fluent:MenuItem Header="Document" />
    <fluent:MenuItem Header="Folder" />
    <fluent:MenuItem Header="Project" />
</fluent:DropDownButton>
```

---

## fluent:SplitButton

Button with separate click and dropdown.

### Key Properties

All DropDownButton properties, plus:

| Property | Type | Description |
|----------|------|-------------|
| `Command` | ICommand | Primary button command |
| `CommandParameter` | object | Command parameter |
| `IsCheckable` | bool | Can be toggled |
| `IsChecked` | bool | Toggle state |
| `IsButtonEnabled` | bool | Enable primary button |
| `DropDownToolTip` | object | Tooltip for dropdown part |
| `PrimaryActionKeyTipPostfix` | string | KeyTip for button (default: "A") |
| `SecondaryActionKeyTipPostfix` | string | KeyTip for dropdown (default: "B") |

### Example

```xml
<fluent:SplitButton Header="Save"
                    Size="Large"
                    LargeIcon="{StaticResource SaveIcon}"
                    Command="{Binding SaveCommand}"
                    KeyTip="S">
    <fluent:MenuItem Header="Save As..." Command="{Binding SaveAsCommand}" />
    <fluent:MenuItem Header="Save All" Command="{Binding SaveAllCommand}" />
</fluent:SplitButton>
```

---

## fluent:ComboBox

Dropdown selection control.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Label text |
| `Icon` | object | Icon |
| `Size` | RibbonControlSize | Size |
| `ItemsSource` | IEnumerable | Items |
| `SelectedItem` | object | Selected item |
| `SelectedIndex` | int | Selected index |
| `IsEditable` | bool | Allow text input |
| `IsReadOnly` | bool | Read-only mode |
| `Text` | string | Current text |
| `InputWidth` | double | Width of input area |
| `KeyTip` | string | Keyboard shortcut |
| `TopPopupContent` | object | Content above items |
| `ResizeMode` | ContextMenuResizeMode | None/Vertical/Both |
| `DropDownHeight` | double | Dropdown height |

### Example

```xml
<fluent:ComboBox Header="Font:"
                  InputWidth="150"
                  ItemsSource="{Binding Fonts}"
                  SelectedItem="{Binding SelectedFont}"
                  KeyTip="FF" />
```

---

## fluent:TextBox

Text input control.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Label |
| `Icon` | object | Icon |
| `Size` | RibbonControlSize | Size |
| `Text` | string | Text value |
| `InputWidth` | double | Input width |
| `KeyTip` | string | Keyboard shortcut |

### Example

```xml
<fluent:TextBox Header="Search:"
                 InputWidth="200"
                 Text="{Binding SearchText}"
                 KeyTip="E" />
```

---

## fluent:Spinner

Numeric up/down control.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Label |
| `Value` | double | Current value |
| `Minimum` | double | Min value |
| `Maximum` | double | Max value |
| `Increment` | double | Step size |
| `Format` | string | Display format |
| `InputWidth` | double | Input width |

### Example

```xml
<fluent:Spinner Header="Size:"
                 Value="{Binding FontSize}"
                 Minimum="8"
                 Maximum="72"
                 Increment="1"
                 Format="0"
                 InputWidth="60" />
```

---

## fluent:CheckBox

Checkbox control.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Label text |
| `IsChecked` | bool? | Check state |
| `Size` | RibbonControlSize | Size |
| `KeyTip` | string | Keyboard shortcut |

### Example

```xml
<fluent:CheckBox Header="Show Gridlines"
                  IsChecked="{Binding ShowGridlines}"
                  KeyTip="G" />
```

---

## fluent:ColorGallery (NEW in v2)

Color picker gallery control.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `SelectedColor` | Color? | Selected color |
| `Mode` | ColorGalleryMode | HighlightColors/StandardColors/ThemeColors |
| `Columns` | int | Number of columns |
| `IsAutomaticColorButtonVisible` | bool | Show "Automatic" button |
| `IsNoColorButtonVisible` | bool | Show "No Color" button |
| `IsMoreColorsButtonVisible` | bool | Show "More Colors..." button |

### ColorGalleryMode
- `HighlightColors` - Highlight marker colors
- `StandardColors` - Standard color palette
- `ThemeColors` - Theme-based colors

### Example

```xml
<fluent:DropDownButton Header="Font Color">
    <fluent:ColorGallery SelectedColor="{Binding FontColor}"
                         Mode="ThemeColors"
                         Columns="10"
                         IsMoreColorsButtonVisible="True" />
</fluent:DropDownButton>
```

---

## fluent:Gallery / fluent:InRibbonGallery

Visual selection gallery.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `ItemsSource` | IEnumerable | Items |
| `SelectedItem` | object | Selected item |
| `ItemTemplate` | DataTemplate | Item template |
| `ItemWidth` | double | Item width |
| `ItemHeight` | double | Item height |
| `Orientation` | Orientation | Layout direction |
| `GroupBy` | string | Grouping property |
| `MinItemsInRow` | int | Min items per row |
| `MaxItemsInRow` | int | Max items per row |
| `Filters` | Collection | Filter options |

---

## fluent:ScreenTip (NEW in v2)

Enhanced tooltip for ribbon controls.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Title` | string | Tooltip title |
| `Text` | string | Tooltip description |
| `Image` | ImageSource | Tooltip image |
| `DisableReason` | string | Text shown when disabled |
| `HelpTopic` | object | F1 help topic |
| `IsRibbonAligned` | bool | Align to ribbon edge |
| `Width` | double | Tooltip width |

### Example

```xml
<fluent:Button Header="Paste">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip Title="Paste (Ctrl+V)"
                          Text="Paste the contents of the clipboard."
                          DisableReason="Nothing to paste."
                          Image="{StaticResource PasteImage}" />
    </fluent:Button.ToolTip>
</fluent:Button>
```

---

## fluent:MenuItem

Menu item for dropdowns and menus.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Text |
| `Icon` | object | Icon |
| `Command` | ICommand | Click command |
| `IsCheckable` | bool | Can be checked |
| `IsChecked` | bool | Check state |
| `InputGestureText` | string | Shortcut text (e.g., "Ctrl+S") |
| `Items` | Collection | Sub-menu items |
| `IsSeparator` | bool | Render as separator |

### Example

```xml
<fluent:MenuItem Header="Recent Files"
                  Icon="{StaticResource RecentIcon}">
    <fluent:MenuItem Header="Document1.txt" />
    <fluent:MenuItem Header="Document2.txt" />
</fluent:MenuItem>
```

---

## fluent:ApplicationMenu

File menu (top-left button).

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Button text (usually "File") |
| `Items` | Collection | Menu items |
| `FooterPaneContent` | object | Footer content |
| `RightPaneContent` | object | Right pane content |

### Example

```xml
<fluent:ApplicationMenu Header="File">
    <fluent:MenuItem Header="New" Icon="{StaticResource NewIcon}" />
    <fluent:MenuItem Header="Open" Icon="{StaticResource OpenIcon}" />
    <fluent:MenuItem Header="Save" Icon="{StaticResource SaveIcon}" />
    <fluent:MenuItem IsSeparator="True" />
    <fluent:MenuItem Header="Exit" />

    <fluent:ApplicationMenu.FooterPaneContent>
        <fluent:Button Header="Options" />
    </fluent:ApplicationMenu.FooterPaneContent>
</fluent:ApplicationMenu>
```

---

## fluent:Backstage

Full-screen backstage view (like Office).

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Button text |
| `IsOpen` | bool | Backstage open state |
| `Items` | Collection | Tab items |

### Example

```xml
<fluent:Backstage Header="File">
    <fluent:BackstageTabItem Header="Info">
        <!-- info content -->
    </fluent:BackstageTabItem>
    <fluent:BackstageTabItem Header="New">
        <!-- new document templates -->
    </fluent:BackstageTabItem>
</fluent:Backstage>
```

---

## fluent:StartScreen (NEW in v2)

Full-screen start/welcome screen (inherits from Backstage).

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsOpen` | bool | Start screen visible |
| `Shown` | bool | Tracks if already shown (show once) |

### Example

```xml
<fluent:Ribbon.StartScreen>
    <fluent:StartScreen>
        <fluent:BackstageTabItem Header="Recent">
            <ListBox ItemsSource="{Binding RecentFiles}" />
        </fluent:BackstageTabItem>
        <fluent:BackstageTabItem Header="New">
            <!-- templates -->
        </fluent:BackstageTabItem>
    </fluent:StartScreen>
</fluent:Ribbon.StartScreen>
```

---

## fluent:StatusBar (NEW in v2)

Status bar at the bottom of the window.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Items` | Collection | Status bar items |

### Related Controls
- `fluent:StatusBarItem` - Individual status bar item
- `fluent:StatusBarPanel` - Grouped items in status bar
- `fluent:StatusBarMenuItem` - Menu item in status bar context menu

### Example

```xml
<fluent:StatusBar DockPanel.Dock="Bottom">
    <fluent:StatusBarItem Title="Ready" />
    <fluent:StatusBarItem Title="Page 1 of 10" HorizontalAlignment="Right" />
    <fluent:StatusBarPanel HorizontalAlignment="Right">
        <fluent:Button Header="100%" Size="Small" />
        <Slider Width="100" Value="{Binding ZoomLevel}" />
    </fluent:StatusBarPanel>
</fluent:StatusBar>
```

---

## fluent:RibbonContextualTabGroup

Tabs that appear based on context (e.g., when image selected).

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Header` | object | Group header |
| `Visibility` | Visibility | Show/hide group |
| `Background` | Brush | Header background color |

### Example

```xml
<fluent:Ribbon>
    <fluent:Ribbon.ContextualGroups>
        <fluent:RibbonContextualTabGroup Header="Picture Tools"
                                          Background="Purple"
                                          Visibility="{Binding IsPictureSelected, Converter={StaticResource BoolToVis}}">
        </fluent:RibbonContextualTabGroup>
    </fluent:Ribbon.ContextualGroups>

    <fluent:RibbonTabItem Header="Format" Group="{Binding ElementName=PictureToolsGroup}">
        <!-- picture formatting controls -->
    </fluent:RibbonTabItem>
</fluent:Ribbon>
```

---

## fluent:QuickAccessToolBar

Quick Access Toolbar control. See dedicated QAT reference for full details.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Items` | ObservableCollection<UIElement> | Toolbar items |
| `QuickAccessItems` | Collection | Customize menu items |
| `ShowAboveRibbon` | bool | Position above/below ribbon |
| `CanQuickAccessLocationChanging` | bool | Allow user to move QAT |
| `IsMenuDropDownVisible` | bool | Show customize dropdown |
| `HasOverflowItems` | bool | (read-only) Items overflow |

### Methods

| Method | Description |
|--------|-------------|
| `Refresh()` | Force layout recalculation |

---

## Common Attached Properties

### fluent:KeyTip.Keys

Keyboard shortcut for any control:

```xml
<fluent:Button fluent:KeyTip.Keys="P" Header="Print" />
```

### fluent:RibbonProperties.Size

Force control size:

```xml
<fluent:Button fluent:RibbonProperties.Size="Small" />
```

### fluent:RibbonProperties.IconSize

Icon size: Small (16x16), Medium (24x24), Large (32x32), Custom

```xml
<fluent:Button fluent:RibbonProperties.IconSize="Medium" />
```

---

## Layout Values (Resource Keys)

Override in `Application.Current.Resources`:

| Key | Default | Description |
|-----|---------|-------------|
| `Fluent.Ribbon.Values.Default.Margin` | `1,1,1,1` | Control margin |
| `Fluent.Ribbon.Values.Default.Padding` | `2,0,2,0` | Control padding |
| `Fluent.Ribbon.Values.RibbonTabControl.Content.Margin` | `8,0,8,0` | Tab content margin |

---

## Control Count Summary

| Category | Controls |
|----------|----------|
| Window | RibbonWindow |
| Ribbon Structure | Ribbon, RibbonTabItem, RibbonGroupBox |
| Buttons | Button, DropDownButton, SplitButton, ToggleButton, RadioButton |
| Input | ComboBox, TextBox, Spinner, CheckBox |
| Galleries | Gallery, InRibbonGallery, ColorGallery |
| Menus | MenuItem, ApplicationMenu, Backstage, BackstageTabItem, StartScreen |
| Contextual | RibbonContextualTabGroup |
| QAT | QuickAccessToolBar, QuickAccessMenuItem |
| Status | StatusBar, StatusBarItem, StatusBarPanel, StatusBarMenuItem |
| Tooltips | ScreenTip |
| **Total** | **~30 controls** |
