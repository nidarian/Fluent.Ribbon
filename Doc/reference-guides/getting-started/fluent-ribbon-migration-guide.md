---
title: Microsoft Ribbon to Fluent.Ribbon Migration Guide
description: Guide for migrating from Microsoft.Windows.Controls.Ribbon to Fluent.Ribbon
tags: [migration, microsoft-ribbon, getting-started, conversion]
see_also:
  - fluent-ribbon-controls-reference-v2.md
  - ../reference/fluent-ribbon-troubleshooting.md
  - ../reference/fluent-ribbon-lessons-learned-v2.md
---

# Microsoft Ribbon to Fluent.Ribbon Migration Guide

**A comprehensive guide for migrating from Microsoft.Windows.Controls.Ribbon (System.Windows.Controls.Ribbon) to Fluent.Ribbon.**

*Guide Version: 2.0 | Compatible with Fluent.Ribbon 10.x+*

---

## Table of Contents

1. [Overview](#overview)
2. [Why Migrate to Fluent.Ribbon?](#why-migrate-to-fluentribbon)
3. [Namespace Changes](#namespace-changes)
4. [XAML Declaration Changes](#xaml-declaration-changes)
5. [Control Name Mapping](#control-name-mapping)
6. [Property Name Differences](#property-name-differences)
7. [Structural Differences](#structural-differences)
8. [Feature Comparison](#feature-comparison)
9. [Theme System Differences](#theme-system-differences)
10. [Step-by-Step Migration Checklist](#step-by-step-migration-checklist)
11. [Code Migration Examples](#code-migration-examples)
12. [Common Migration Errors](#common-migration-errors)
13. [Advanced Topics](#advanced-topics)

---

## Overview

Microsoft provides a built-in Ribbon control in WPF through the `System.Windows.Controls.Ribbon` namespace. Fluent.Ribbon is a third-party open-source library that provides a more feature-rich, Office-like Ribbon implementation.

### Key Differences at a Glance

| Aspect | Microsoft Ribbon | Fluent.Ribbon |
|--------|------------------|---------------|
| Namespace | `System.Windows.Controls.Ribbon` | `Fluent` |
| NuGet Package | Built-in (reference assembly) | `Fluent.Ribbon` |
| Control Prefix | `Ribbon*` (e.g., RibbonButton) | No prefix (e.g., Button) |
| Backstage | Limited ApplicationMenu | Full Backstage with tabs |
| Theming | Limited | Rich theming via ControlzEx |
| Simplified Ribbon | Not supported | Supported |
| Start Screen | Not supported | Supported |
| State Persistence | Manual | Automatic |

---

## Why Migrate to Fluent.Ribbon?

### Fluent.Ribbon Advantages

1. **Modern Office-style UI** - More closely matches Office 2016/2019/365 appearance
2. **Backstage View** - Full-featured backstage like Office (not just application menu)
3. **Simplified Ribbon Mode** - Compact single-row mode (Office 365 style)
4. **Start Screen** - Built-in start/welcome screen support
5. **Rich Theming** - Multiple built-in themes, easy customization via ControlzEx
6. **State Persistence** - Automatic saving/loading of ribbon state
7. **Better KeyTip Support** - More robust keyboard navigation
8. **Active Development** - Regular updates and bug fixes
9. **ScreenTip** - Enhanced tooltips with images and disable reasons
10. **Status Bar** - Integrated status bar control

### Microsoft Ribbon Advantages

1. **No Dependencies** - Built into .NET Framework/.NET
2. **Smaller Footprint** - No additional NuGet packages
3. **Microsoft Support** - Official Microsoft control
4. **Simpler for Basic Use** - Less configuration needed for basic ribbons

---

## Namespace Changes

### Assembly References

**Microsoft Ribbon:**
```xml
<!-- Reference in .csproj -->
<Reference Include="System.Windows.Controls.Ribbon" />
```

**Fluent.Ribbon:**
```xml
<!-- NuGet Package -->
<PackageReference Include="Fluent.Ribbon" Version="10.0.0" />
```

### C# Using Statements

**Microsoft Ribbon:**
```csharp
using System.Windows.Controls.Ribbon;
```

**Fluent.Ribbon:**
```csharp
using Fluent;
```

---

## XAML Declaration Changes

### Namespace Declaration

**Microsoft Ribbon:**
```xml
<Window xmlns:ribbon="clr-namespace:System.Windows.Controls.Ribbon;assembly=System.Windows.Controls.Ribbon"
        ...>
```

**Fluent.Ribbon:**
```xml
<fluent:RibbonWindow xmlns:fluent="urn:fluent-ribbon"
                      ...>
```

### Complete Window Declaration Comparison

**Microsoft Ribbon:**
```xml
<ribbon:RibbonWindow x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:ribbon="clr-namespace:System.Windows.Controls.Ribbon;assembly=System.Windows.Controls.Ribbon"
        Title="My Application" Height="600" Width="800">
    <Grid>
        <ribbon:Ribbon>
            <!-- Content -->
        </ribbon:Ribbon>
    </Grid>
</ribbon:RibbonWindow>
```

**Fluent.Ribbon:**
```xml
<fluent:RibbonWindow x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:fluent="urn:fluent-ribbon"
        Title="My Application" Height="600" Width="800">
    <Grid>
        <fluent:Ribbon>
            <!-- Content -->
        </fluent:Ribbon>
    </Grid>
</fluent:RibbonWindow>
```

---

## Control Name Mapping

### Complete Control Mapping Table

| Microsoft Ribbon | Fluent.Ribbon | Notes |
|------------------|---------------|-------|
| `RibbonWindow` | `RibbonWindow` | Same name, different namespace |
| `Ribbon` | `Ribbon` | Same name, different properties |
| `RibbonTab` | `RibbonTabItem` | **Name change** |
| `RibbonGroup` | `RibbonGroupBox` | **Name change** |
| `RibbonButton` | `Button` | Simplified name |
| `RibbonToggleButton` | `ToggleButton` | Simplified name |
| `RibbonCheckBox` | `CheckBox` | Simplified name |
| `RibbonRadioButton` | `RadioButton` | Simplified name |
| `RibbonTextBox` | `TextBox` | Simplified name |
| `RibbonComboBox` | `ComboBox` | Simplified name |
| `RibbonMenuButton` | `DropDownButton` | **Name change** |
| `RibbonSplitButton` | `SplitButton` | Simplified name |
| `RibbonGallery` | `Gallery` / `InRibbonGallery` | Two options in Fluent |
| `RibbonGalleryCategory` | Gallery grouping | Different approach |
| `RibbonGalleryItem` | `GalleryItem` | Simplified name |
| `RibbonMenuItem` | `MenuItem` | Simplified name |
| `RibbonSeparator` | `Separator` | Use WPF Separator |
| `RibbonApplicationMenu` | `ApplicationMenu` / `Backstage` | Enhanced in Fluent |
| `RibbonApplicationMenuItem` | `MenuItem` | Simplified name |
| `RibbonApplicationSplitMenuItem` | `SplitButton` in menu | Use SplitButton |
| `RibbonContextualTabGroup` | `RibbonContextualTabGroup` | Same name |
| `RibbonQuickAccessToolBar` | `QuickAccessToolBar` | Simplified name |
| `RibbonToolTip` | `ScreenTip` | **Name change**, enhanced |
| `RibbonTwoLineText` | `TwoLineLabel` | **Name change** |
| `RibbonControlGroup` | `RibbonToolBar` | Different approach |
| `RibbonContextMenu` | `ContextMenu` | Simplified name |
| `RibbonTabHeader` | (built into RibbonTabItem) | Not separate control |
| `RibbonSplitMenuItem` | `SplitButton` | Use in menu context |
| `RibbonFilterMenuButton` | Gallery filters | Different approach |
| `RibbonControl` | (multiple controls) | Base class, not used directly |

### Controls Unique to Fluent.Ribbon

| Fluent.Ribbon Control | Description |
|-----------------------|-------------|
| `Backstage` | Full-screen backstage view (Office-style) |
| `BackstageTabItem` | Tab within backstage |
| `BackstageTabControl` | Tab control for backstage |
| `StartScreen` | Initial welcome/start screen |
| `StartScreenTabControl` | Tab control for start screen |
| `InRibbonGallery` | Gallery embedded in ribbon (not dropdown) |
| `ColorGallery` | Specialized color picker gallery |
| `Spinner` | Numeric up/down control |
| `StatusBar` | Bottom status bar |
| `StatusBarItem` | Item in status bar |
| `StatusBarPanel` | Panel grouping in status bar |
| `QuickAccessMenuItem` | Item in QAT dropdown menu |
| `SeparatorTabItem` | Visual separator between tabs |

---

## Property Name Differences

### Button Properties

| Microsoft Ribbon | Fluent.Ribbon | Notes |
|------------------|---------------|-------|
| `Label` | `Header` | **Different name** |
| `LargeImageSource` | `LargeIcon` | **Different name**, accepts object |
| `SmallImageSource` | `Icon` | **Different name**, accepts object |
| (none) | `MediumIcon` | **New** - 24x24 icon |
| `KeyTip` | `KeyTip` | Same |
| `Command` | `Command` | Same |
| `CommandParameter` | `CommandParameter` | Same |
| `ToolTipTitle` | (use ScreenTip) | Different approach |
| `ToolTipDescription` | (use ScreenTip) | Different approach |
| `ToolTipImageSource` | (use ScreenTip) | Different approach |
| (none) | `Size` | **New** - Large/Middle/Small |
| (none) | `SizeDefinition` | **New** - Size behavior definition |
| (none) | `IsDefinitive` | **New** - Closes popup on click |

### RibbonTab / RibbonTabItem Properties

| Microsoft Ribbon | Fluent.Ribbon | Notes |
|------------------|---------------|-------|
| `Header` | `Header` | Same |
| `KeyTip` | `KeyTip` | Same |
| `ContextualTabGroupHeader` | `Group` | **Different** - reference to group object |
| `IsSelected` | `IsSelected` | Same |
| (none) | `Groups` | **New** - collection of RibbonGroupBox |
| (none) | `ReduceOrder` | **New** - custom reduce order |
| (none) | `HeaderPadding` | **New** - header spacing |

### RibbonGroup / RibbonGroupBox Properties

| Microsoft Ribbon | Fluent.Ribbon | Notes |
|------------------|---------------|-------|
| `Header` | `Header` | Same |
| `KeyTip` | `KeyTip` | Same |
| `LargeImageSource` | `LargeIcon` | **Different name** |
| `SmallImageSource` | `Icon` | **Different name** |
| (none) | `MediumIcon` | **New** |
| (none) | `IsLauncherVisible` | **New** - dialog launcher |
| (none) | `LauncherCommand` | **New** |
| (none) | `LauncherKeys` | **New** - launcher KeyTip |
| (none) | `LauncherToolTip` | **New** |
| (none) | `State` | **New** - Large/Middle/Small/Collapsed |
| (none) | `StateDefinition` | **New** - state transition order |
| (none) | `IsSeparatorVisible` | **New** |

### Ribbon Properties

| Microsoft Ribbon | Fluent.Ribbon | Notes |
|------------------|---------------|-------|
| `ApplicationMenu` | `Menu` | **Different name** |
| `QuickAccessToolBar` | `QuickAccessToolBar` | Same (read-only in Fluent) |
| `Title` | (use Window.Title) | Title is on window |
| (none) | `IsMinimized` | **New** |
| (none) | `IsCollapsed` | **New** |
| (none) | `IsSimplified` | **New** - simplified mode |
| (none) | `CanMinimize` | **New** |
| (none) | `CanUseSimplified` | **New** |
| (none) | `SelectedTabItem` | **New** |
| (none) | `SelectedTabIndex` | **New** |
| (none) | `ShowQuickAccessToolBarAboveRibbon` | **New** |
| (none) | `TitleBar` | **New** |
| (none) | `ContextualGroups` | **New** - collection |
| (none) | `Tabs` | **New** - collection |
| (none) | `ToolBarItems` | **New** - right toolbar |
| (none) | `StartScreen` | **New** |
| (none) | `AutomaticStateManagement` | **New** |

### ComboBox Properties

| Microsoft Ribbon | Fluent.Ribbon | Notes |
|------------------|---------------|-------|
| `Label` | `Header` | **Different name** |
| `SmallImageSource` | `Icon` | **Different name** |
| `IsEditable` | `IsEditable` | Same |
| `IsReadOnly` | `IsReadOnly` | Same |
| `SelectionBoxWidth` | `InputWidth` | **Different name** |
| (none) | `Size` | **New** |
| (none) | `ResizeMode` | **New** - dropdown resize |
| (none) | `DropDownHeight` | **New** |

### Gallery Properties

| Microsoft Ribbon | Fluent.Ribbon | Notes |
|------------------|---------------|-------|
| `SelectedItem` | `SelectedItem` | Same |
| `SelectedValue` | `SelectedValue` | Same |
| `ColumnsStretchToFill` | (different approach) | Use ItemWidth |
| `MinColumnCount` | `MinItemsInRow` | **Different name** |
| `MaxColumnCount` | `MaxItemsInRow` | **Different name** |
| (none) | `ItemWidth` | **New** |
| (none) | `ItemHeight` | **New** |
| (none) | `GroupBy` | **New** - grouping property |
| (none) | `Orientation` | **New** |

---

## Structural Differences

### Application Menu vs Backstage

**Microsoft Ribbon (Application Menu):**
```xml
<ribbon:Ribbon>
    <ribbon:Ribbon.ApplicationMenu>
        <ribbon:RibbonApplicationMenu SmallImageSource="app.png">
            <ribbon:RibbonApplicationMenuItem Header="New" />
            <ribbon:RibbonApplicationMenuItem Header="Open" />
            <ribbon:RibbonApplicationMenuItem Header="Save" />
            <ribbon:RibbonSeparator />
            <ribbon:RibbonApplicationMenuItem Header="Exit" />
        </ribbon:RibbonApplicationMenu>
    </ribbon:Ribbon.ApplicationMenu>
</ribbon:Ribbon>
```

**Fluent.Ribbon (Application Menu):**
```xml
<fluent:Ribbon>
    <fluent:Ribbon.Menu>
        <fluent:ApplicationMenu Header="File">
            <fluent:MenuItem Header="New" Icon="{StaticResource NewIcon}" />
            <fluent:MenuItem Header="Open" Icon="{StaticResource OpenIcon}" />
            <fluent:MenuItem Header="Save" Icon="{StaticResource SaveIcon}" />
            <Separator />
            <fluent:MenuItem Header="Exit" />

            <fluent:ApplicationMenu.FooterPaneContent>
                <fluent:Button Header="Options" />
            </fluent:ApplicationMenu.FooterPaneContent>
        </fluent:ApplicationMenu>
    </fluent:Ribbon.Menu>
</fluent:Ribbon>
```

**Fluent.Ribbon (Backstage - Office-style):**
```xml
<fluent:Ribbon>
    <fluent:Ribbon.Menu>
        <fluent:Backstage Header="File">
            <fluent:BackstageTabControl>
                <fluent:BackstageTabItem Header="Info">
                    <StackPanel Margin="20">
                        <TextBlock Text="Document Information" FontSize="24" />
                        <!-- Info content -->
                    </StackPanel>
                </fluent:BackstageTabItem>
                <fluent:BackstageTabItem Header="New">
                    <!-- Template gallery -->
                </fluent:BackstageTabItem>
                <fluent:BackstageTabItem Header="Open">
                    <!-- Recent files -->
                </fluent:BackstageTabItem>
                <fluent:Button Header="Save" />
                <fluent:Button Header="Save As" />
                <Separator />
                <fluent:Button Header="Close" />
            </fluent:BackstageTabControl>
        </fluent:Backstage>
    </fluent:Ribbon.Menu>
</fluent:Ribbon>
```

### Tab and Group Structure

**Microsoft Ribbon:**
```xml
<ribbon:Ribbon>
    <ribbon:RibbonTab Header="Home">
        <ribbon:RibbonGroup Header="Clipboard">
            <ribbon:RibbonButton Label="Paste"
                                 LargeImageSource="paste32.png"
                                 SmallImageSource="paste16.png" />
            <ribbon:RibbonButton Label="Cut"
                                 SmallImageSource="cut16.png" />
            <ribbon:RibbonButton Label="Copy"
                                 SmallImageSource="copy16.png" />
        </ribbon:RibbonGroup>
    </ribbon:RibbonTab>
</ribbon:Ribbon>
```

**Fluent.Ribbon:**
```xml
<fluent:Ribbon>
    <fluent:RibbonTabItem Header="Home">
        <fluent:RibbonGroupBox Header="Clipboard">
            <fluent:Button Header="Paste"
                           Size="Large"
                           LargeIcon="{StaticResource PasteIcon32}"
                           Icon="{StaticResource PasteIcon16}" />
            <fluent:Button Header="Cut"
                           Size="Middle"
                           Icon="{StaticResource CutIcon16}" />
            <fluent:Button Header="Copy"
                           Size="Middle"
                           Icon="{StaticResource CopyIcon16}" />
        </fluent:RibbonGroupBox>
    </fluent:RibbonTabItem>
</fluent:Ribbon>
```

### Quick Access Toolbar

**Microsoft Ribbon:**
```xml
<ribbon:Ribbon>
    <ribbon:Ribbon.QuickAccessToolBar>
        <ribbon:RibbonQuickAccessToolBar>
            <ribbon:RibbonButton SmallImageSource="save16.png" />
            <ribbon:RibbonButton SmallImageSource="undo16.png" />
            <ribbon:RibbonButton SmallImageSource="redo16.png" />
        </ribbon:RibbonQuickAccessToolBar>
    </ribbon:Ribbon.QuickAccessToolBar>
</ribbon:Ribbon>
```

**Fluent.Ribbon:**
```xml
<fluent:Ribbon>
    <fluent:Ribbon.QuickAccessItems>
        <fluent:QuickAccessMenuItem Target="{Binding ElementName=SaveButton}" IsChecked="True" />
        <fluent:QuickAccessMenuItem Target="{Binding ElementName=UndoButton}" IsChecked="True" />
    </fluent:Ribbon.QuickAccessItems>

    <!-- Or programmatically add to QAT -->
</fluent:Ribbon>
```

### Contextual Tabs

**Microsoft Ribbon:**
```xml
<ribbon:Ribbon>
    <ribbon:Ribbon.ContextualTabGroups>
        <ribbon:RibbonContextualTabGroup Header="Picture Tools"
                                          Background="Purple"
                                          Visibility="Collapsed" />
    </ribbon:Ribbon.ContextualTabGroups>

    <ribbon:RibbonTab Header="Format"
                      ContextualTabGroupHeader="Picture Tools">
        <!-- Picture formatting controls -->
    </ribbon:RibbonTab>
</ribbon:Ribbon>
```

**Fluent.Ribbon:**
```xml
<fluent:Ribbon>
    <fluent:Ribbon.ContextualGroups>
        <fluent:RibbonContextualTabGroup x:Name="PictureToolsGroup"
                                          Header="Picture Tools"
                                          Background="Purple"
                                          Visibility="Collapsed" />
    </fluent:Ribbon.ContextualGroups>

    <fluent:RibbonTabItem Header="Format"
                          Group="{Binding ElementName=PictureToolsGroup}">
        <!-- Picture formatting controls -->
    </fluent:RibbonTabItem>
</fluent:Ribbon>
```

---

## Feature Comparison

### Features in Fluent.Ribbon Not in Microsoft Ribbon

| Feature | Description |
|---------|-------------|
| **Backstage** | Full-screen backstage view with tabs |
| **Start Screen** | Welcome/start screen on application launch |
| **Simplified Ribbon** | Compact single-row ribbon mode |
| **Automatic State Persistence** | Saves/loads ribbon state automatically |
| **Rich Theming** | Multiple themes, Windows 11 support |
| **ScreenTip** | Enhanced tooltips with images, disable reasons |
| **InRibbonGallery** | Gallery displayed inline (not dropdown) |
| **ColorGallery** | Specialized color picker |
| **Spinner** | Numeric up/down control |
| **StatusBar** | Integrated status bar |
| **Window Glow** | Customizable window glow effects |
| **MediumIcon** | Three icon sizes (Small/Medium/Large) |
| **Size Property** | Explicit control over button size |
| **SizeDefinition** | Custom size behavior definitions |
| **StateDefinition** | Custom group state transitions |
| **Dialog Launcher** | Group dialog launcher button |
| **Reduce Order** | Custom tab reduction behavior |

### Features in Microsoft Ribbon Not Directly in Fluent.Ribbon

| Feature | Fluent.Ribbon Alternative |
|---------|---------------------------|
| `RibbonControlGroup` | Use `RibbonToolBar` for similar layout |
| `RibbonFilterMenuButton` | Use Gallery filters or DropDownButton |
| `RibbonTabHeader` | Built into `RibbonTabItem` |
| Built-in image sizing | Provide images at correct sizes |

---

## Theme System Differences

### Microsoft Ribbon Theming

Microsoft Ribbon uses standard WPF theming. Limited built-in theme support.

```xml
<!-- Override individual styles -->
<Style TargetType="ribbon:RibbonButton">
    <Setter Property="Background" Value="Blue" />
</Style>
```

### Fluent.Ribbon Theming

Fluent.Ribbon uses ControlzEx for advanced theming.

**Built-in Themes:**
- Office 2010 (Blue, Silver, Black)
- Office 2013 (White, Light Gray, Dark Gray)
- Office 2016 (Colorful, White, Dark Gray, Black)
- Windows 11 themes

**Applying a Theme:**
```csharp
// In App.xaml.cs or startup
using ControlzEx.Theming;

// Apply theme
ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");

// Or in XAML (App.xaml)
```

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Fluent.Ribbon theme -->
            <ResourceDictionary Source="pack://application:,,,/Fluent;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

**Custom Theme Colors:**
```xml
<Application.Resources>
    <!-- Override accent color -->
    <Color x:Key="Fluent.Ribbon.Colors.AccentBase">#0078D4</Color>

    <!-- Override specific brushes -->
    <SolidColorBrush x:Key="Fluent.Ribbon.Brushes.Button.MouseOver.Background"
                     Color="#E5F3FF" />
</Application.Resources>
```

---

## Step-by-Step Migration Checklist

### Phase 1: Project Setup

- [ ] **Add NuGet Package**
  ```
  Install-Package Fluent.Ribbon
  ```

- [ ] **Remove Microsoft Ribbon Reference**
  - Remove `System.Windows.Controls.Ribbon` assembly reference

- [ ] **Update App.xaml**
  ```xml
  <Application.Resources>
      <ResourceDictionary>
          <ResourceDictionary.MergedDictionaries>
              <ResourceDictionary Source="pack://application:,,,/Fluent;component/Themes/Generic.xaml" />
          </ResourceDictionary.MergedDictionaries>
      </ResourceDictionary>
  </Application.Resources>
  ```

### Phase 2: XAML Migration

- [ ] **Update XAML Namespaces**
  - Change `xmlns:ribbon="clr-namespace:System.Windows.Controls.Ribbon..."`
  - To `xmlns:fluent="urn:fluent-ribbon"`

- [ ] **Update Window Base Class**
  - Change `<ribbon:RibbonWindow>` to `<fluent:RibbonWindow>`
  - Update code-behind: inherit from `Fluent.RibbonWindow`

- [ ] **Rename Controls**
  - `RibbonTab` -> `RibbonTabItem`
  - `RibbonGroup` -> `RibbonGroupBox`
  - `RibbonButton` -> `Button`
  - `RibbonMenuButton` -> `DropDownButton`
  - (see full mapping table above)

- [ ] **Update Properties**
  - `Label` -> `Header`
  - `LargeImageSource` -> `LargeIcon`
  - `SmallImageSource` -> `Icon`
  - (see full property mapping above)

- [ ] **Update Application Menu**
  - Convert `RibbonApplicationMenu` to `ApplicationMenu` or `Backstage`

- [ ] **Update Quick Access Toolbar**
  - Use `QuickAccessMenuItem` with `Target` binding

- [ ] **Update Contextual Tabs**
  - Use `Group` property instead of `ContextualTabGroupHeader`

### Phase 3: Code-Behind Migration

- [ ] **Update Using Statements**
  ```csharp
  // Remove
  using System.Windows.Controls.Ribbon;

  // Add
  using Fluent;
  ```

- [ ] **Update Type References**
  - `RibbonTab` -> `RibbonTabItem`
  - `RibbonGroup` -> `RibbonGroupBox`
  - etc.

- [ ] **Update Property Access**
  - Update any code accessing renamed properties

- [ ] **Update Event Handlers**
  - Event signatures may differ slightly

### Phase 4: Testing and Polish

- [ ] **Test All Ribbon Functionality**
  - Button clicks
  - Dropdowns and menus
  - Contextual tabs visibility
  - Quick Access Toolbar
  - KeyTip navigation

- [ ] **Test State Persistence**
  - Minimized state
  - QAT position
  - QAT items

- [ ] **Adjust Sizing**
  - Set `Size` property on controls as needed
  - Adjust `SizeDefinition` for custom behavior

- [ ] **Apply Theming**
  - Choose appropriate theme
  - Customize colors if needed

---

## Code Migration Examples

### Example 1: Simple Button

**Microsoft Ribbon:**
```xml
<ribbon:RibbonButton Label="Save"
                      KeyTip="S"
                      LargeImageSource="Images/save32.png"
                      SmallImageSource="Images/save16.png"
                      Command="{Binding SaveCommand}" />
```

**Fluent.Ribbon:**
```xml
<fluent:Button Header="Save"
               KeyTip="S"
               Size="Large"
               LargeIcon="{StaticResource SaveIcon32}"
               Icon="{StaticResource SaveIcon16}"
               Command="{Binding SaveCommand}" />
```

### Example 2: Dropdown Menu Button

**Microsoft Ribbon:**
```xml
<ribbon:RibbonMenuButton Label="New"
                          LargeImageSource="Images/new32.png"
                          KeyTip="N">
    <ribbon:RibbonMenuItem Header="Document" />
    <ribbon:RibbonMenuItem Header="Folder" />
    <ribbon:RibbonMenuItem Header="Project" />
</ribbon:RibbonMenuButton>
```

**Fluent.Ribbon:**
```xml
<fluent:DropDownButton Header="New"
                        Size="Large"
                        LargeIcon="{StaticResource NewIcon32}"
                        KeyTip="N">
    <fluent:MenuItem Header="Document" />
    <fluent:MenuItem Header="Folder" />
    <fluent:MenuItem Header="Project" />
</fluent:DropDownButton>
```

### Example 3: Split Button

**Microsoft Ribbon:**
```xml
<ribbon:RibbonSplitButton Label="Paste"
                           LargeImageSource="Images/paste32.png"
                           Command="{Binding PasteCommand}"
                           KeyTip="V">
    <ribbon:RibbonMenuItem Header="Paste Special..."
                            Command="{Binding PasteSpecialCommand}" />
    <ribbon:RibbonMenuItem Header="Paste as Hyperlink"
                            Command="{Binding PasteAsLinkCommand}" />
</ribbon:RibbonSplitButton>
```

**Fluent.Ribbon:**
```xml
<fluent:SplitButton Header="Paste"
                     Size="Large"
                     LargeIcon="{StaticResource PasteIcon32}"
                     Command="{Binding PasteCommand}"
                     KeyTip="V">
    <fluent:MenuItem Header="Paste Special..."
                      Command="{Binding PasteSpecialCommand}" />
    <fluent:MenuItem Header="Paste as Hyperlink"
                      Command="{Binding PasteAsLinkCommand}" />
</fluent:SplitButton>
```

### Example 4: ComboBox

**Microsoft Ribbon:**
```xml
<ribbon:RibbonComboBox Label="Font:"
                        SelectionBoxWidth="150"
                        IsEditable="True"
                        ItemsSource="{Binding Fonts}"
                        SelectedItem="{Binding SelectedFont}" />
```

**Fluent.Ribbon:**
```xml
<fluent:ComboBox Header="Font:"
                  InputWidth="150"
                  IsEditable="True"
                  ItemsSource="{Binding Fonts}"
                  SelectedItem="{Binding SelectedFont}" />
```

### Example 5: Gallery

**Microsoft Ribbon:**
```xml
<ribbon:RibbonGallery SelectedItem="{Binding SelectedStyle}"
                       MaxColumnCount="5"
                       MinColumnCount="3">
    <ribbon:RibbonGalleryCategory Header="Styles"
                                   ItemsSource="{Binding Styles}" />
</ribbon:RibbonGallery>
```

**Fluent.Ribbon:**
```xml
<fluent:InRibbonGallery SelectedItem="{Binding SelectedStyle}"
                         MaxItemsInRow="5"
                         MinItemsInRow="3"
                         ItemsSource="{Binding Styles}"
                         GroupBy="Category" />
```

### Example 6: Tooltip / ScreenTip

**Microsoft Ribbon:**
```xml
<ribbon:RibbonButton Label="Format"
                      ToolTipTitle="Format Selection"
                      ToolTipDescription="Format the selected text."
                      ToolTipImageSource="Images/format_help.png" />
```

**Fluent.Ribbon:**
```xml
<fluent:Button Header="Format">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip Title="Format Selection"
                          Text="Format the selected text."
                          Image="{StaticResource FormatHelpImage}"
                          DisableReason="Select text first." />
    </fluent:Button.ToolTip>
</fluent:Button>
```

---

## Common Migration Errors

### Error 1: "The name 'RibbonTab' does not exist"

**Cause:** Using old control name.

**Solution:** Use `RibbonTabItem` instead of `RibbonTab`.

```xml
<!-- Wrong -->
<fluent:RibbonTab Header="Home">

<!-- Correct -->
<fluent:RibbonTabItem Header="Home">
```

### Error 2: "The property 'Label' was not found"

**Cause:** Property renamed.

**Solution:** Use `Header` instead of `Label`.

```xml
<!-- Wrong -->
<fluent:Button Label="Save" />

<!-- Correct -->
<fluent:Button Header="Save" />
```

### Error 3: "The property 'LargeImageSource' was not found"

**Cause:** Property renamed and type changed.

**Solution:** Use `LargeIcon` (accepts any object, not just ImageSource).

```xml
<!-- Wrong -->
<fluent:Button LargeImageSource="icon.png" />

<!-- Correct -->
<fluent:Button LargeIcon="{StaticResource IconResource}" />
```

### Error 4: "Cannot find resource 'Fluent.Ribbon...' "

**Cause:** Theme resources not loaded.

**Solution:** Add theme dictionary to App.xaml.

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Fluent;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Error 5: "RibbonGroup does not exist"

**Cause:** Using old control name.

**Solution:** Use `RibbonGroupBox` instead of `RibbonGroup`.

```xml
<!-- Wrong -->
<fluent:RibbonGroup Header="Clipboard">

<!-- Correct -->
<fluent:RibbonGroupBox Header="Clipboard">
```

### Error 6: Contextual Tab Not Showing

**Cause:** Using wrong property for group association.

**Solution:** Use `Group` property with element binding.

```xml
<!-- Wrong (Microsoft Ribbon style) -->
<fluent:RibbonTabItem ContextualTabGroupHeader="Picture Tools">

<!-- Correct -->
<fluent:RibbonTabItem Group="{Binding ElementName=PictureToolsGroup}">
```

### Error 7: Icons Not Displaying

**Cause:** Different icon property types (object vs ImageSource).

**Solution:** Define icons as resources that return appropriate types.

```xml
<!-- Define icon as resource -->
<BitmapImage x:Key="SaveIcon16" UriSource="Images/save16.png" />

<!-- Use in control -->
<fluent:Button Icon="{StaticResource SaveIcon16}" />
```

### Error 8: Window Not Styled Correctly

**Cause:** Not inheriting from `Fluent.RibbonWindow`.

**Solution:** Change base class in XAML and code-behind.

```xml
<!-- XAML -->
<fluent:RibbonWindow x:Class="MyApp.MainWindow" ...>
```

```csharp
// Code-behind
public partial class MainWindow : Fluent.RibbonWindow
```

---

## Advanced Topics

### Custom Size Definitions

Fluent.Ribbon allows custom size behaviors:

```xml
<fluent:Button Header="MyButton"
               SizeDefinition="Large,Large,Middle" />
```

The three values represent button size at different group states.

### Custom Group State Definitions

```xml
<fluent:RibbonGroupBox Header="MyGroup"
                        StateDefinition="Large,Middle,Small,Collapsed" />
```

### Programmatic QAT Management

```csharp
// Add to QAT
ribbon.AddToQuickAccessToolBar(myButton);

// Remove from QAT
ribbon.RemoveFromQuickAccessToolBar(myButton);

// Check if in QAT
bool isInQat = ribbon.IsInQuickAccessToolBar(myButton);
```

### State Persistence

```csharp
// Disable automatic state management
ribbon.AutomaticStateManagement = false;

// Manual save
ribbon.RibbonStateStorage.Save();

// Manual load
ribbon.RibbonStateStorage.Load();
```

### KeyTip Customization

```csharp
// Disable KeyTip handling
ribbon.IsKeyTipHandlingEnabled = false;

// Custom KeyTip keys
ribbon.KeyTipKeys.Add(Key.F10);
ribbon.KeyTipKeys.Add(Key.LeftAlt);
```

---

## References

- [Fluent.Ribbon GitHub Repository](https://github.com/fluentribbon/Fluent.Ribbon)
- [Fluent.Ribbon Documentation](http://fluentribbon.github.io/documentation/)
- [Microsoft WPF Ribbon Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.ribbon)
- [ControlzEx Theming](https://github.com/ControlzEx/ControlzEx)

---

*Last Updated: 2026-01-23*
