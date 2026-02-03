---
title: Fluent.Ribbon Common Tasks
description: How to do common things the RIGHT way without hacking
tags: [tasks, how-to, getting-started, theming, icons, commands]
see_also:
  - fluent-ribbon-controls-reference-v2.md
  - fluent-ribbon-mvvm-patterns.md
  - ../styling/controlzex-theming-reference-v2.md
---

# Fluent.Ribbon Common Tasks (v2)

**How to do common things the RIGHT way (without hacking).**

*v2 Changes: Added RuntimeThemeGenerator 3rd parameter, added Size enum note, added Gallery/ApplicationMenu/ToggleButton/Launcher sections*

---

## FIRST: See It Working

**Build and run the Showcase app to see working examples of everything:**

```bash
# Clone or navigate to your local Fluent.Ribbon repository
# https://github.com/fluentribbon/Fluent.Ribbon
dotnet build Fluent.Ribbon.Showcase
dotnet run --project Fluent.Ribbon.Showcase
```

The Showcase demonstrates all controls, theming, and proper patterns. If you're unsure how something works, **see it working first**.

---

## Theming

### Switch to Dark Mode

```csharp
using ControlzEx.Theming;

// Option 1: Use built-in dark theme
ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Blue");

// Option 2: Sync with Windows theme
ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
ThemeManager.Current.SyncTheme();
```

### Create Custom Theme with Brand Color

```csharp
using ControlzEx.Theming;

var brandColor = Color.FromRgb(0x00, 0x78, 0xD4);  // Your brand color
// Note: Third parameter is isHighContrast (use false for normal themes)
var theme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme("Dark", brandColor, false);
ThemeManager.Current.ChangeTheme(Application.Current, theme);
```

### Override Specific Colors

```csharp
// Override individual brushes (must be new frozen brush)
var brush = new SolidColorBrush(Colors.DarkBlue);
brush.Freeze();
Application.Current.Resources["Fluent.Ribbon.Brushes.AccentBase"] = brush;
```

---

## Layout & Spacing

### Change Button Spacing

```csharp
// All buttons
Application.Current.Resources["Fluent.Ribbon.Values.Default.Margin"] = new Thickness(2);
```

```xml
<!-- Single button -->
<fluent:Button Margin="4" Header="My Button" />
```

### Change Button Padding

```csharp
// All buttons
Application.Current.Resources["Fluent.Ribbon.Values.Default.Padding"] = new Thickness(4, 2, 4, 2);
```

```xml
<!-- Single button -->
<fluent:Button Padding="6 3" Header="My Button" />
```

### Change Tab Content Area Margin

```csharp
Application.Current.Resources["Fluent.Ribbon.Values.RibbonTabControl.Content.Margin"] = new Thickness(4, 0, 4, 0);
```

---

## Text & Headers

### Change Section Header Color

```csharp
Application.Current.Resources["Fluent.Ribbon.Brushes.RibbonGroupBox.Header.Foreground"] =
    new SolidColorBrush(Colors.White);
```

### Add Subtitle to Section Header

```xml
<fluent:RibbonGroupBox Header="File Operations">
    <fluent:RibbonGroupBox.HeaderTemplate>
        <DataTemplate>
            <StackPanel HorizontalAlignment="Center">
                <TextBlock Text="{Binding}" HorizontalAlignment="Center" />
                <TextBlock Text="Ctrl+F" FontSize="9" Opacity="0.6" HorizontalAlignment="Center" />
            </StackPanel>
        </DataTemplate>
    </fluent:RibbonGroupBox.HeaderTemplate>
</fluent:RibbonGroupBox>
```

### Add Subtitle to Button

```xml
<fluent:Button Size="Large" Icon="{StaticResource SaveIcon}">
    <fluent:Button.HeaderTemplate>
        <DataTemplate>
            <StackPanel HorizontalAlignment="Center">
                <TextBlock Text="Save" HorizontalAlignment="Center" />
                <TextBlock Text="Ctrl+S" FontSize="9" Opacity="0.6" HorizontalAlignment="Center" />
            </StackPanel>
        </DataTemplate>
    </fluent:Button.HeaderTemplate>
</fluent:Button>
```

### Change Button Text Color

```xml
<fluent:Button Header="My Button"
               Foreground="{DynamicResource MyTextBrush}" />
```

### Make Button Text White in Dark Mode

Define your brush that changes with theme:

```xml
<!-- In your theme resources -->
<SolidColorBrush x:Key="MyButtonText" Color="White" />
```

```xml
<fluent:Button Foreground="{DynamicResource MyButtonText}" Header="Click Me" />
```

---

## Tab Customization

### Change Tab Background When Selected

```csharp
Application.Current.Resources["Fluent.Ribbon.Brushes.RibbonTabItem.Active.Background"] =
    new SolidColorBrush(Colors.DarkBlue);
```

### Change Tab Text Color

```csharp
Application.Current.Resources["Fluent.Ribbon.Brushes.RibbonTabItem.Foreground"] =
    new SolidColorBrush(Colors.White);
```

### Custom Tab Header

```xml
<fluent:RibbonTabItem KeyTip="H">
    <fluent:RibbonTabItem.HeaderTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <Image Source="{StaticResource HomeIcon}" Width="16" Height="16" Margin="0 0 4 0" />
                <TextBlock Text="Home" VerticalAlignment="Center" />
            </StackPanel>
        </DataTemplate>
    </fluent:RibbonTabItem.HeaderTemplate>
</fluent:RibbonTabItem>
```

---

## Window Customization

### Change Window Background

```csharp
Application.Current.Resources["Fluent.Ribbon.Brushes.RibbonWindow.Background"] =
    new SolidColorBrush(Color.FromRgb(30, 30, 30));
```

### Change Title Bar Color

```csharp
Application.Current.Resources["Fluent.Ribbon.Brushes.RibbonWindow.TitleBackground"] =
    new SolidColorBrush(Colors.DarkBlue);
Application.Current.Resources["Fluent.Ribbon.Brushes.RibbonWindow.TitleForeground"] =
    new SolidColorBrush(Colors.White);
```

---

## Button States

### Change Button Hover Color

```csharp
Application.Current.Resources["Fluent.Ribbon.Brushes.Button.MouseOver.Background"] =
    new SolidColorBrush(Colors.LightBlue);
```

### Change Button Pressed Color

```csharp
Application.Current.Resources["Fluent.Ribbon.Brushes.Button.Pressed.Background"] =
    new SolidColorBrush(Colors.DarkBlue);
```

---

## Control Sizes

**Important:** The size enum values are `Large`, `Middle`, `Small` - NOT "Medium"!

```csharp
// RibbonControlSize enum:
public enum RibbonControlSize
{
    Large = 0,
    Middle,   // NOT "Medium"
    Small
}
```

### Force Button to Small Size

```xml
<fluent:Button Header="Cut" Size="Small" />
```

### Force Button to Middle Size

```xml
<fluent:Button Header="Cut" Size="Middle" />
```

### Make All Buttons in Group Same Size

```xml
<fluent:RibbonGroupBox Header="Editing">
    <fluent:Button Header="Cut" Size="Middle" />
    <fluent:Button Header="Copy" Size="Middle" />
    <fluent:Button Header="Paste" Size="Middle" />
</fluent:RibbonGroupBox>
```

---

## ToggleButton (NEW in v2)

### Basic Toggle Button

```xml
<fluent:ToggleButton Header="Bold"
                     Icon="{StaticResource BoldIcon}"
                     IsChecked="{Binding IsBold}" />
```

### Toggle Button with Command

```xml
<fluent:ToggleButton Header="Grid Lines"
                     IsChecked="{Binding ShowGridLines}"
                     Command="{Binding ToggleGridCommand}" />
```

### Toggle Button Brushes

```csharp
// Checked state background
Application.Current.Resources["Fluent.Ribbon.Brushes.ToggleButton.Checked.Background"] =
    new SolidColorBrush(Colors.DarkBlue);

// Checked + hover
Application.Current.Resources["Fluent.Ribbon.Brushes.ToggleButton.CheckedMouseOver.Background"] =
    new SolidColorBrush(Colors.Blue);
```

---

## Icons

### Use Different Icon Sizes

```xml
<fluent:Button Header="Save"
               Icon="{StaticResource SaveIcon16}"
               MediumIcon="{StaticResource SaveIcon24}"
               LargeIcon="{StaticResource SaveIcon32}" />
```

### Icon Paths

- `Icon` - Used for Small/Middle size (16x16)
- `MediumIcon` - Used for Medium size (24x24)
- `LargeIcon` - Used for Large size (32x32)

---

## Keyboard Navigation

### Add KeyTip to Button

```xml
<fluent:Button Header="Paste" KeyTip="V" />
```

### Add KeyTip to Tab

```xml
<fluent:RibbonTabItem Header="Home" KeyTip="H" />
```

### Add KeyTip to Group

```xml
<fluent:RibbonGroupBox Header="Clipboard" KeyTip="ZC" />
```

---

## Quick Access Toolbar

### Add Button to QAT Programmatically

```csharp
ribbon.QuickAccessToolBar.Items.Add(new Fluent.Button
{
    Header = "Save",
    Icon = FindResource("SaveIcon")
});
```

### Prevent Button from Being Added to QAT

```xml
<fluent:Button Header="Exit" CanAddToQuickAccessToolBar="False" />
```

### Clear All QAT Items

```csharp
ribbon.QuickAccessToolBar.Items.Clear();
```

### Disable QAT Position Changing

```xml
<fluent:QuickAccessToolBar CanQuickAccessLocationChanging="False" />
```

---

## Group Launcher Button (Dialog Launcher) (NEW in v2)

The launcher button appears in the bottom-right corner of a group and typically opens a dialog.

### Show Launcher Button

```xml
<fluent:RibbonGroupBox Header="Font"
                       IsLauncherVisible="True"
                       LauncherClick="OnFontLauncherClick"
                       LauncherKeys="FN">
    <!-- group content -->
</fluent:RibbonGroupBox>
```

### Handle Launcher Click

```csharp
private void OnFontLauncherClick(object sender, RoutedEventArgs e)
{
    var fontDialog = new FontDialog();
    fontDialog.ShowDialog();
}
```

### Use Command Instead

```xml
<fluent:RibbonGroupBox Header="Font"
                       IsLauncherVisible="True"
                       LauncherCommand="{Binding OpenFontDialogCommand}"
                       LauncherKeys="FN" />
```

---

## Contextual Tabs

### Show/Hide Contextual Tab Based on Selection

```xml
<fluent:Ribbon>
    <fluent:Ribbon.ContextualGroups>
        <fluent:RibbonContextualTabGroup x:Name="PictureTools"
                                          Header="Picture Tools"
                                          Background="Purple"
                                          Visibility="{Binding IsPictureSelected,
                                                       Converter={StaticResource BoolToVis}}" />
    </fluent:Ribbon.ContextualGroups>

    <fluent:RibbonTabItem Header="Format" Group="{Binding ElementName=PictureTools}">
        <!-- picture formatting tools -->
    </fluent:RibbonTabItem>
</fluent:Ribbon>
```

### Show Contextual Tab Programmatically

```csharp
// In code-behind
PictureToolsGroup.Visibility = Visibility.Visible;

// Or via binding
public bool IsPictureSelected
{
    get => _isPictureSelected;
    set { _isPictureSelected = value; OnPropertyChanged(); }
}
```

---

## ApplicationMenu (File Menu) (NEW in v2)

### Basic ApplicationMenu

```xml
<fluent:Ribbon.Menu>
    <fluent:ApplicationMenu>
        <fluent:MenuItem Header="New" Icon="{StaticResource NewIcon}" />
        <fluent:MenuItem Header="Open" Icon="{StaticResource OpenIcon}" />
        <fluent:MenuItem Header="Save" Icon="{StaticResource SaveIcon}" />
        <Separator />
        <fluent:MenuItem Header="Exit" Command="{Binding ExitCommand}" />

        <!-- Right pane content -->
        <fluent:ApplicationMenu.RightPaneContent>
            <StackPanel Margin="10">
                <TextBlock Text="Recent Documents" FontWeight="Bold" />
                <ListBox ItemsSource="{Binding RecentFiles}" />
            </StackPanel>
        </fluent:ApplicationMenu.RightPaneContent>

        <!-- Footer content -->
        <fluent:ApplicationMenu.FooterPaneContent>
            <fluent:Button Header="Options" Size="Middle" />
        </fluent:ApplicationMenu.FooterPaneContent>
    </fluent:ApplicationMenu>
</fluent:Ribbon.Menu>
```

---

## Gallery (NEW in v2)

### In-Ribbon Gallery

```xml
<fluent:InRibbonGallery Header="Styles"
                        ItemWidth="72"
                        ItemHeight="56"
                        ItemsSource="{Binding Styles}"
                        SelectedItem="{Binding SelectedStyle}">
    <fluent:InRibbonGallery.ItemTemplate>
        <DataTemplate>
            <Border Background="{Binding Background}"
                    BorderBrush="{Binding Border}"
                    BorderThickness="1">
                <TextBlock Text="{Binding Name}"
                           HorizontalAlignment="Center"
                           VerticalAlignment="Center" />
            </Border>
        </DataTemplate>
    </fluent:InRibbonGallery.ItemTemplate>
</fluent:InRibbonGallery>
```

### DropDown Gallery

```xml
<fluent:DropDownButton Header="Quick Styles">
    <fluent:Gallery ItemsSource="{Binding Styles}"
                    SelectedItem="{Binding SelectedStyle}"
                    ItemWidth="72"
                    ItemHeight="56" />
</fluent:DropDownButton>
```

### Gallery with Grouping

```xml
<fluent:Gallery ItemsSource="{Binding Styles}"
                GroupBy="Category">
    <fluent:Gallery.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding Name}" FontWeight="Bold" />
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
        </GroupStyle>
    </fluent:Gallery.GroupStyle>
</fluent:Gallery>
```

---

## Backstage

### Open Backstage Programmatically

```csharp
backstage.IsOpen = true;
```

### Bind Backstage State

```xml
<fluent:Backstage IsOpen="{Binding IsBackstageOpen, Mode=TwoWay}">
    <!-- backstage content -->
</fluent:Backstage>
```

### Close Backstage When Button Clicked

```xml
<fluent:BackstageTabItem Header="Info">
    <Button Content="Close" Click="CloseBackstage_Click" />
</fluent:BackstageTabItem>
```

```csharp
private void CloseBackstage_Click(object sender, RoutedEventArgs e)
{
    backstage.IsOpen = false;
}
```

---

## Dropdown Menus

### Add Separator in Dropdown

```xml
<fluent:DropDownButton Header="New">
    <fluent:MenuItem Header="Document" />
    <fluent:MenuItem Header="Spreadsheet" />
    <Separator />
    <fluent:MenuItem Header="From Template..." />
</fluent:DropDownButton>
```

### Add Submenu

```xml
<fluent:DropDownButton Header="Insert">
    <fluent:MenuItem Header="Picture">
        <fluent:MenuItem Header="From File..." />
        <fluent:MenuItem Header="From Camera..." />
        <fluent:MenuItem Header="Online Pictures..." />
    </fluent:MenuItem>
</fluent:DropDownButton>
```

---

## Ribbon State

### Minimize Ribbon

```csharp
ribbon.IsMinimized = true;
```

### Prevent Ribbon Minimization

```xml
<fluent:Ribbon CanMinimize="False" />
```

### Collapse Ribbon Automatically

```xml
<fluent:Ribbon IsAutomaticCollapseEnabled="True" />
```

---

## Separators

### Add Vertical Separator Between Groups

```xml
<fluent:RibbonGroupBox Header="Group 1" IsSeparatorVisible="True">
    <!-- content -->
</fluent:RibbonGroupBox>
```

### Add Separator Line in Group

```xml
<fluent:RibbonGroupBox Header="Format">
    <fluent:Button Header="Bold" />
    <fluent:Button Header="Italic" />
    <Separator />
    <fluent:Button Header="Font Color" />
</fluent:RibbonGroupBox>
```

---

## ScreenTips (Enhanced Tooltips)

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

## ComboBox and Spinner (NEW in v2)

### Ribbon ComboBox

```xml
<fluent:ComboBox Header="Font"
                 Width="120"
                 ItemsSource="{Binding Fonts}"
                 SelectedItem="{Binding SelectedFont}" />
```

### Editable ComboBox

```xml
<fluent:ComboBox Header="Size"
                 Width="60"
                 IsEditable="True"
                 Text="{Binding FontSize}" />
```

### Spinner (Numeric Up/Down)

```xml
<fluent:Spinner Header="Zoom"
                Minimum="10"
                Maximum="500"
                Increment="10"
                Value="{Binding ZoomLevel}"
                Format="0'%'" />
```

---

## DO NOT DO

### DON'T: Walk the Visual Tree

```csharp
// WRONG - Never do this
foreach (var child in GetVisualChildren(ribbon))
{
    if (child.Name == "PART_xxx")
        child.Background = myBrush;
}
```

### DON'T: Reference PART_xxx Elements

```csharp
// WRONG
var header = FindChild<ContentControl>(groupBox, "PART_HeaderContentControl");
header.Margin = new Thickness(10);
```

### DON'T: Set Properties on Internal Elements

```csharp
// WRONG
foreach (var textBlock in FindVisualChildren<TextBlock>(button))
{
    textBlock.Foreground = Brushes.White;
}
```

### DO: Use Resource Overrides or Templates

```csharp
// RIGHT - Override resources
Application.Current.Resources["Fluent.Ribbon.Brushes.LabelText"] = Brushes.White;
```

```xml
<!-- RIGHT - Use templates -->
<fluent:Button>
    <fluent:Button.HeaderTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding}" Foreground="White" />
        </DataTemplate>
    </fluent:Button.HeaderTemplate>
</fluent:Button>
```
