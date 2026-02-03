---
title: Fluent.Ribbon StartScreen Reference
description: Complete reference for the StartScreen overlay and StartScreenTabControl controls
tags: [startscreen, overlay, controls, startup]
see_also:
  - fluent-ribbon-backstage-reference.md
---

# Fluent.Ribbon StartScreen Reference

**Complete reference for the StartScreen overlay and StartScreenTabControl controls.**

---

## Overview

The `StartScreen` is an Office-style startup overlay that appears when an application first launches. It displays over the entire window content (similar to Backstage) but is designed to show initial options like recent files, templates, or a welcome screen before the user begins working.

Key differences from Backstage:
- **StartScreen** displays only once at application startup (by default)
- **Backstage** can be opened repeatedly via the File button
- **StartScreen** hides the title bar when visible
- **StartScreen** has no animations by default
- **StartScreen** uses a two-panel layout (left and right content)

### Visual Layout

```
+-------------------------------------------------------------------------+
|                                                           [-][O][x]     |
+-------------------------------------------------------------------------+
|                                                                         |
|  +--------------------+  +-------------------------------------------+  |
|  |                    |  |                                           |  |
|  |   LEFT CONTENT     |  |          RIGHT CONTENT                    |  |
|  |                    |  |                                           |  |
|  |   - App name/logo  |  |   - Recent files list                     |  |
|  |   - Branding       |  |   - Templates                             |  |
|  |                    |  |   - Getting started                       |  |
|  |                    |  |   - Close button                          |  |
|  |                    |  |                                           |  |
|  +--------------------+  +-------------------------------------------+  |
|                                                                         |
+-------------------------------------------------------------------------+
```

### Architecture

```
Ribbon
    |
    +-- Ribbon.StartScreen
           |
           +-- StartScreen (extends Backstage)
                   |
                   +-- Content (StartScreenTabControl)
                          |
                          +-- LeftContent (branding/logo area)
                          +-- RightContent (main content area)
```

---

## StartScreen Control

The `StartScreen` control extends `Backstage` and inherits all its functionality. It adds special behavior for one-time display at startup.

### Inheritance Chain

```
Fluent.StartScreen
    |
    +-- Fluent.Backstage
           |
           +-- Fluent.RibbonControl
                  |
                  +-- System.Windows.Controls.Control
```

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Shown` | `bool` | `false` | Whether the StartScreen has already been displayed (persists across open/close) |
| `IsOpen` | `bool` | `false` | (Inherited) Whether the StartScreen is currently visible |
| `Content` | `UIElement` | `null` | (Inherited) The content, typically a `StartScreenTabControl` |
| `CloseOnEsc` | `bool` | `true` | (Inherited) Whether Escape closes the StartScreen |
| `CanChangeIsOpen` | `bool` | `true` | (Inherited) Whether the StartScreen can be opened/closed |
| `AreAnimationsEnabled` | `bool` | `false` | (Overridden) Animations disabled by default |
| `HideContextTabsOnOpen` | `bool` | `true` | (Overridden) Always hides context tabs |

### Events

| Event | Description |
|-------|-------------|
| `IsOpenChanged` | (Inherited) Fired when `IsOpen` changes |

### Important Behavior: Show Once

The `Shown` property controls the one-time display behavior:

1. When `IsOpen` is set to `true` for the first time:
   - If `Shown` is `false`, the StartScreen displays and `Shown` becomes `true`
   - The title bar collapses automatically

2. On subsequent attempts to open:
   - If `Shown` is `true`, the StartScreen will NOT display
   - `Show()` method returns `false`

3. To re-show the StartScreen:
   - Set `Shown = false` before setting `IsOpen = true`

```csharp
// Reset to show again
startScreen.Shown = false;
startScreen.IsOpen = true;
```

---

## StartScreenTabControl

A specialized version of `BackstageTabControl` designed for the two-panel StartScreen layout.

### Inheritance Chain

```
Fluent.StartScreenTabControl
    |
    +-- Fluent.BackstageTabControl
           |
           +-- System.Windows.Controls.Primitives.Selector
```

### Key Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `LeftContent` | `object` | `null` | Content for the left panel (branding area) |
| `LeftContentMargin` | `Thickness` | `default` | Margin around the left content |
| `RightContent` | `object` | `null` | Content for the right panel (main area) |
| `ItemsPanelMinWidth` | `double` | `342` | (Overridden) Minimum width of the left panel |
| `ItemsPanelBackground` | `Brush` | Theme brush | (Inherited) Background of the left panel |
| `Background` | `Brush` | Theme brush | Background of the right panel |

### Template Parts

| Part Name | Type | Purpose |
|-----------|------|---------|
| `PART_LeftContentGrid` | `Grid` | Container for left content |
| `PART_SelectedContentGrid` | `Grid` | Container for right content |
| `PART_SelectedContentHost` | `ContentPresenter` | Displays `RightContent` |

### Content Model

Unlike `BackstageTabControl`, `StartScreenTabControl` does NOT use tab items:

- **No tabs or buttons** in the left panel
- **Left panel** shows `LeftContent` directly (logo, branding, app name)
- **Right panel** shows `RightContent` directly (recent files, templates, close button)

```xml
<!-- BackstageTabControl uses tab items -->
<Fluent:BackstageTabControl>
    <Fluent:BackstageTabItem Header="Home" />
    <Fluent:BackstageTabItem Header="Recent" />
</Fluent:BackstageTabControl>

<!-- StartScreenTabControl uses direct content -->
<Fluent:StartScreenTabControl>
    <Fluent:StartScreenTabControl.LeftContent>
        <!-- Branding content -->
    </Fluent:StartScreenTabControl.LeftContent>
    <Fluent:StartScreenTabControl.RightContent>
        <!-- Main content -->
    </Fluent:StartScreenTabControl.RightContent>
</Fluent:StartScreenTabControl>
```

---

## Integration with Ribbon

### Basic Setup

```xml
<Fluent:RibbonWindow x:Class="MyApp.MainWindow"
                     xmlns:Fluent="urn:fluent-ribbon">
    <Grid>
        <Fluent:Ribbon x:Name="ribbon">
            <!-- StartScreen declaration -->
            <Fluent:Ribbon.StartScreen>
                <Fluent:StartScreen x:Name="startScreen">
                    <Fluent:StartScreenTabControl>
                        <Fluent:StartScreenTabControl.LeftContent>
                            <StackPanel>
                                <Label FontSize="48" Content="My App" />
                            </StackPanel>
                        </Fluent:StartScreenTabControl.LeftContent>

                        <Fluent:StartScreenTabControl.RightContent>
                            <StackPanel>
                                <TextBlock Text="Welcome! Click below to start." />
                                <Fluent:Button Header="Close" IsDefinitive="True" />
                            </StackPanel>
                        </Fluent:StartScreenTabControl.RightContent>
                    </Fluent:StartScreenTabControl>
                </Fluent:StartScreen>
            </Fluent:Ribbon.StartScreen>

            <!-- Backstage (File menu) - separate from StartScreen -->
            <Fluent:Ribbon.Menu>
                <Fluent:Backstage>
                    <Fluent:BackstageTabControl>
                        <!-- Backstage tabs -->
                    </Fluent:BackstageTabControl>
                </Fluent:Backstage>
            </Fluent:Ribbon.Menu>

            <!-- Ribbon tabs -->
            <Fluent:RibbonTabItem Header="Home">
                <!-- ... -->
            </Fluent:RibbonTabItem>
        </Fluent:Ribbon>

        <!-- Main content area -->
        <ContentControl Content="{Binding MainContent}" />
    </Grid>
</Fluent:RibbonWindow>
```

### Ribbon Properties

| Property | Type | Description |
|----------|------|-------------|
| `Ribbon.StartScreen` | `StartScreen` | The StartScreen instance |
| `Ribbon.IsBackstageOrStartScreenOpen` | `bool` | (Read-only) True if either Backstage or StartScreen is open |

---

## Showing and Hiding

### Auto-Show at Startup

The StartScreen opens automatically when `IsOpen="True"` is set in XAML or during Loaded event:

```xml
<!-- XAML approach - opens on load -->
<Fluent:StartScreen x:Name="startScreen" IsOpen="True">
    <!-- ... -->
</Fluent:StartScreen>
```

```csharp
// Code-behind approach
public MainWindow()
{
    InitializeComponent();
    Loaded += (s, e) => startScreen.IsOpen = true;
}
```

### Manual Show (Re-show After First Time)

```csharp
// In code-behind or command handler
private void ShowStartScreen_Click(object sender, RoutedEventArgs e)
{
    // Reset the Shown flag to allow re-display
    startScreen.Shown = false;
    startScreen.IsOpen = true;
}
```

### Closing the StartScreen

The StartScreen closes when:

1. **User presses Escape** (if `CloseOnEsc="True"`)
2. **User clicks a button with `IsDefinitive="True"`**
3. **Code sets `IsOpen = false`**

```xml
<!-- Close button with IsDefinitive -->
<Fluent:Button Header="Close start screen" IsDefinitive="True">
    Close
</Fluent:Button>
```

```csharp
// Programmatic close
startScreen.IsOpen = false;
```

---

## Complete Example

From the Fluent.Ribbon Showcase application:

```xml
<Fluent:Ribbon.StartScreen>
    <Fluent:StartScreen x:Name="startScreen">
        <Fluent:StartScreenTabControl>
            <!-- Left panel: Branding -->
            <Fluent:StartScreenTabControl.LeftContent>
                <StackPanel Orientation="Vertical">
                    <Label Foreground="{DynamicResource Fluent.Ribbon.Brushes.IdealForeground}"
                           FontSize="48"
                           Content="Fluent.Ribbon" />
                </StackPanel>
            </Fluent:StartScreenTabControl.LeftContent>

            <!-- Right panel: Main content -->
            <Fluent:StartScreenTabControl.RightContent>
                <StackPanel Orientation="Vertical">
                    <TextBlock HorizontalAlignment="Center"
                               VerticalAlignment="Center">
                        You can close the start screen by either clicking
                        the button below or by pressing ESC
                    </TextBlock>
                    <Fluent:Button LargeIcon="{iconPacks:Material Kind=ExitToApp}"
                                   IsDefinitive="True">
                        Close start screen
                    </Fluent:Button>
                </StackPanel>
            </Fluent:StartScreenTabControl.RightContent>
        </Fluent:StartScreenTabControl>
    </Fluent:StartScreen>
</Fluent:Ribbon.StartScreen>
```

### Re-showing from a Button

```csharp
private void ShowStartScreen_OnClick(object sender, RoutedEventArgs e)
{
    this.startScreen.Shown = false;
    this.startScreen.IsOpen = true;
}
```

---

## Common Patterns

### Recent Files List

```xml
<Fluent:StartScreenTabControl.RightContent>
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Header -->
        <TextBlock Grid.Row="0"
                   Text="Recent Documents"
                   FontSize="24"
                   Margin="0 0 0 20" />

        <!-- Recent files list -->
        <ListBox Grid.Row="1"
                 ItemsSource="{Binding RecentFiles}"
                 BorderThickness="0"
                 Background="Transparent">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <StackPanel Orientation="Horizontal" Margin="5">
                        <Image Source="{StaticResource DocumentIcon}" Width="32" Height="32" />
                        <StackPanel Margin="10 0">
                            <TextBlock Text="{Binding FileName}" FontWeight="SemiBold" />
                            <TextBlock Text="{Binding FilePath}" FontSize="11" Opacity="0.7" />
                        </StackPanel>
                    </StackPanel>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>

        <!-- Close button -->
        <Fluent:Button Grid.Row="2"
                       Header="Start Without Document"
                       IsDefinitive="True"
                       Margin="0 20 0 0" />
    </Grid>
</Fluent:StartScreenTabControl.RightContent>
```

### Templates Gallery

```xml
<Fluent:StartScreenTabControl.RightContent>
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <TextBlock Grid.Row="0" Text="New Document" FontSize="24" Margin="0 0 0 20" />

        <ItemsControl Grid.Row="1" ItemsSource="{Binding Templates}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <WrapPanel />
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Button Command="{Binding DataContext.CreateFromTemplateCommand,
                                             RelativeSource={RelativeSource AncestorType=ItemsControl}}"
                            CommandParameter="{Binding}"
                            Margin="5"
                            Padding="0"
                            Background="Transparent"
                            BorderThickness="1">
                        <StackPanel Width="120">
                            <Image Source="{Binding Thumbnail}" Height="90" />
                            <TextBlock Text="{Binding Name}"
                                       TextAlignment="Center"
                                       Margin="5" />
                        </StackPanel>
                    </Button>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </Grid>
</Fluent:StartScreenTabControl.RightContent>
```

### Branding Panel with Version Info

```xml
<Fluent:StartScreenTabControl.LeftContent>
    <DockPanel Margin="30">
        <!-- Logo at top -->
        <Image DockPanel.Dock="Top"
               Source="{StaticResource AppLogo}"
               Width="200"
               Margin="0 0 0 20" />

        <!-- App name -->
        <TextBlock DockPanel.Dock="Top"
                   Text="My Application"
                   FontSize="36"
                   Foreground="{DynamicResource Fluent.Ribbon.Brushes.IdealForeground}" />

        <!-- Version at bottom -->
        <StackPanel DockPanel.Dock="Bottom" VerticalAlignment="Bottom">
            <TextBlock Text="{Binding Version}"
                       Foreground="{DynamicResource Fluent.Ribbon.Brushes.IdealForeground}"
                       Opacity="0.7" />
            <TextBlock Text="{Binding Copyright}"
                       Foreground="{DynamicResource Fluent.Ribbon.Brushes.IdealForeground}"
                       Opacity="0.5"
                       FontSize="10" />
        </StackPanel>

        <!-- Spacer -->
        <Grid />
    </DockPanel>
</Fluent:StartScreenTabControl.LeftContent>
```

---

## Styling and Theming

### Key Style Resources

| Style Key | Target | Description |
|-----------|--------|-------------|
| `Fluent.Ribbon.Styles.RibbonStartScreen` | `StartScreen` | Main StartScreen style (extends Backstage style) |
| `Fluent.Ribbon.Styles.StartScreenTabControl` | `StartScreenTabControl` | Tab control style |

### Key Template Resources

| Template Key | Target | Description |
|--------------|--------|-------------|
| `Fluent.Ribbon.Templates.StartScreenTabControl` | `StartScreenTabControl` | Layout template |

### Default Style Settings

From `StartScreen.xaml`:

```xml
<Style x:Key="Fluent.Ribbon.Styles.RibbonStartScreen"
       TargetType="{x:Type Fluent:StartScreen}"
       BasedOn="{StaticResource Fluent.Ribbon.Styles.RibbonBackstage}">
    <Setter Property="AreAnimationsEnabled" Value="False" />
    <Setter Property="Fluent:KeyTip.Keys" Value="{x:Null}" />
    <Setter Property="HideContextTabsOnOpen" Value="True" />
    <Setter Property="Template" Value="{x:Null}" />
</Style>
```

### Customizing Left Panel Width

```xml
<Fluent:StartScreenTabControl ItemsPanelMinWidth="400">
    <!-- ... -->
</Fluent:StartScreenTabControl>
```

### Customizing Left Panel Background

```xml
<Fluent:StartScreenTabControl
    ItemsPanelBackground="{DynamicResource Fluent.Ribbon.Brushes.AccentBase}">
    <!-- ... -->
</Fluent:StartScreenTabControl>
```

### Customizing Left Content Margin

```xml
<Fluent:StartScreenTabControl LeftContentMargin="30 50">
    <!-- ... -->
</Fluent:StartScreenTabControl>
```

### Using Theme-Aware Brushes

The StartScreen inherits Backstage colors. Key brushes:

| Brush | Usage |
|-------|-------|
| `Fluent.Ribbon.Brushes.BackstageTabControl.ItemsPanelBackground` | Left panel background |
| `Fluent.Ribbon.Brushes.BackstageTabControl.Background` | Right panel background |
| `Fluent.Ribbon.Brushes.IdealForeground` | Text on colored backgrounds |

---

## MVVM Pattern

### ViewModel Binding

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    private bool _isStartScreenOpen;
    public bool IsStartScreenOpen
    {
        get => _isStartScreenOpen;
        set => SetProperty(ref _isStartScreenOpen, value);
    }

    private bool _startScreenShown;
    public bool StartScreenShown
    {
        get => _startScreenShown;
        set => SetProperty(ref _startScreenShown, value);
    }

    public ICommand ShowStartScreenCommand { get; }
    public ICommand CloseStartScreenCommand { get; }

    public ObservableCollection<RecentFile> RecentFiles { get; }

    public MainViewModel()
    {
        ShowStartScreenCommand = new RelayCommand(() =>
        {
            StartScreenShown = false;
            IsStartScreenOpen = true;
        });

        CloseStartScreenCommand = new RelayCommand(() =>
        {
            IsStartScreenOpen = false;
        });
    }
}
```

```xml
<Fluent:StartScreen IsOpen="{Binding IsStartScreenOpen, Mode=TwoWay}"
                    Shown="{Binding StartScreenShown, Mode=TwoWay}">
    <!-- ... -->
</Fluent:StartScreen>
```

### Closing via IsDefinitive Button

When a button has `IsDefinitive="True"`, it raises the `DismissPopupEvent` which closes the StartScreen automatically:

```xml
<!-- No command binding needed for simple close -->
<Fluent:Button Header="Close" IsDefinitive="True" />

<!-- With command for additional action -->
<Fluent:Button Header="Open Document"
               Command="{Binding OpenSelectedCommand}"
               IsDefinitive="True" />
```

---

## DO NOT DO

### DON'T: Expect StartScreen to Show Multiple Times Without Reset

```csharp
// WRONG - Won't work after first display
startScreen.IsOpen = true;  // Shows
startScreen.IsOpen = false; // Closes
startScreen.IsOpen = true;  // WON'T SHOW - Shown is already true

// RIGHT - Reset Shown flag first
startScreen.Shown = false;
startScreen.IsOpen = true;  // NOW it shows
```

### DON'T: Use BackstageTabItems in StartScreenTabControl

```xml
<!-- WRONG - BackstageTabItems are for Backstage, not StartScreen -->
<Fluent:StartScreenTabControl>
    <Fluent:BackstageTabItem Header="Home" />  <!-- Won't work as expected -->
</Fluent:StartScreenTabControl>

<!-- RIGHT - Use LeftContent and RightContent -->
<Fluent:StartScreenTabControl>
    <Fluent:StartScreenTabControl.LeftContent>
        <!-- Branding -->
    </Fluent:StartScreenTabControl.LeftContent>
    <Fluent:StartScreenTabControl.RightContent>
        <!-- Main content -->
    </Fluent:StartScreenTabControl.RightContent>
</Fluent:StartScreenTabControl>
```

### DON'T: Confuse StartScreen with Backstage

```xml
<!-- WRONG - StartScreen goes in Ribbon.StartScreen, not Ribbon.Menu -->
<Fluent:Ribbon.Menu>
    <Fluent:StartScreen>  <!-- Wrong location! -->
        <!-- ... -->
    </Fluent:StartScreen>
</Fluent:Ribbon.Menu>

<!-- RIGHT - Use proper property -->
<Fluent:Ribbon.StartScreen>
    <Fluent:StartScreen>
        <!-- ... -->
    </Fluent:StartScreen>
</Fluent:Ribbon.StartScreen>

<Fluent:Ribbon.Menu>
    <Fluent:Backstage>
        <!-- Separate Backstage for File menu -->
    </Fluent:Backstage>
</Fluent:Ribbon.Menu>
```

### DON'T: Forget IsDefinitive on Close Buttons

```xml
<!-- WRONG - Button won't close the StartScreen -->
<Fluent:Button Header="Close">Close</Fluent:Button>

<!-- RIGHT - IsDefinitive triggers close -->
<Fluent:Button Header="Close" IsDefinitive="True">Close</Fluent:Button>
```

### DON'T: Hide Title Bar Manually

The StartScreen automatically collapses the title bar when open. Don't try to manage this yourself:

```csharp
// WRONG - StartScreen handles this internally
ribbon.TitleBar.IsCollapsed = true;
startScreen.IsOpen = true;
// ... later ...
ribbon.TitleBar.IsCollapsed = false;

// RIGHT - Just open/close the StartScreen
startScreen.IsOpen = true;  // Title bar auto-collapses
startScreen.IsOpen = false; // Title bar auto-restores
```

---

## Keyboard Navigation

| Key | Action |
|-----|--------|
| `Escape` | Close StartScreen (if `CloseOnEsc="True"`) |
| `Tab` | Navigate between focusable elements |
| `Enter/Space` | Activate focused button/control |

**Note:** Unlike Backstage, StartScreen has no KeyTip by default (`KeyTip.Keys` is set to `null`).

---

## Title Bar Behavior

When the StartScreen opens:
1. It stores the original `TitleBar.IsCollapsed` value
2. Sets `TitleBar.IsCollapsed = true` (hides the title bar)
3. When closed or visibility changes, restores the original value

This is handled automatically by the `StartScreen.Show()` and `StartScreen.Hide()` methods.

### Edge Case: Visibility Changes

If you change the StartScreen's `Visibility` directly while `IsOpen` is true, the title bar state is also updated (fixes issue #445 in the source):

```csharp
// This also affects title bar
startScreen.Visibility = Visibility.Collapsed;  // Title bar shows
startScreen.Visibility = Visibility.Visible;    // Title bar hides
```

---

## Source Files Reference

| File | Purpose |
|------|---------|
| `Fluent.Ribbon\Controls\StartScreen.cs` | StartScreen control (extends Backstage) |
| `Fluent.Ribbon\Controls\StartScreenTabControl.cs` | Two-panel layout control |
| `Fluent.Ribbon\Controls\Backstage.cs` | Base class for StartScreen |
| `Fluent.Ribbon\Controls\BackstageTabControl.cs` | Base class for StartScreenTabControl |
| `Fluent.Ribbon\Themes\Controls\StartScreen.xaml` | StartScreen style (minimal, extends Backstage) |
| `Fluent.Ribbon\Themes\Controls\StartScreenTabControl.xaml` | StartScreenTabControl template |
| `Fluent.Ribbon\Controls\Ribbon.cs:567-585` | Ribbon.StartScreen property definition |
| `Fluent.Ribbon.Showcase\TestContent.xaml:161-186` | Usage example |
| `Fluent.Ribbon.Showcase\TestContent.xaml.cs:578-582` | Re-show example |

---

## Summary Table

| Task | Solution |
|------|----------|
| Add StartScreen to Ribbon | Use `<Fluent:Ribbon.StartScreen>` property |
| Show at startup | Set `IsOpen="True"` in XAML or Loaded event |
| Close StartScreen | Set `IsOpen = false` or use `IsDefinitive="True"` button |
| Re-show after first display | Set `Shown = false` then `IsOpen = true` |
| Add branding | Use `StartScreenTabControl.LeftContent` |
| Add main content | Use `StartScreenTabControl.RightContent` |
| Auto-close on button click | Set `IsDefinitive="True"` on button |
| Disable Esc to close | Set `CloseOnEsc="False"` |
| Customize left panel width | Set `ItemsPanelMinWidth` |
| Customize left panel background | Set `ItemsPanelBackground` |
| Bind to ViewModel | Bind `IsOpen` and `Shown` properties with `Mode=TwoWay` |
| Check if open | Read `Ribbon.IsBackstageOrStartScreenOpen` |
