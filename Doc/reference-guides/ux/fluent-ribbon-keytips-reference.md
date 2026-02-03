---
title: Fluent.Ribbon KeyTips and Keyboard Navigation Reference
description: Complete reference for KeyTips, keyboard shortcuts, and accessibility
tags: [keytips, keyboard, navigation, accessibility]
see_also:
  - fluent-ribbon-accessibility-reference.md
  - ../controls/fluent-ribbon-button-controls-reference.md
---

# Fluent.Ribbon KeyTips and Keyboard Navigation Reference

**Complete reference for KeyTips, keyboard shortcuts, and accessibility in Fluent.Ribbon.**

*Source files: `Controls\KeyTip.cs`, `Services\KeyTipService.cs`, `Adorners\KeyTipAdorner.cs`, `Data\KeyTipInformation.cs`*

---

## Overview

KeyTips provide keyboard-only access to ribbon controls, following the Microsoft Office ribbon keyboard navigation model. Users can press Alt or F10 to display KeyTips, then type the displayed characters to activate controls.

```
User presses Alt:
+---------------------------------------------------------------------+
|  [F]  File  | [H] Home  | [V] View  | [T] Tools |                   |
+---------------------------------------------------------------------+
|  [ZC]                   [Clipboard]                                 |
|  +------------------------+                                         |
|  | [V]        [X]   [C]  |                                         |
|  | Paste      Cut   Copy |                                         |
|  +------------------------+                                         |
+---------------------------------------------------------------------+

User presses "H" to select Home tab, then "V" for Paste:
KeyTip navigation chain: Alt -> H -> V -> (action triggered)
```

---

## Component Hierarchy

```
fluent:Ribbon
+-- KeyTipService (internal, handles Alt/F10 activation)
    +-- KeyTipAdorner (displays keytips for current scope)
        +-- KeyTipInformation (position + keytip data per element)
            +-- KeyTip control (visual badge)

Control Tree Search:
+-- Ribbon
    +-- RibbonTabItem (KeyTip="H")
    +-- RibbonTabControl
        +-- RibbonGroupBox (KeyTip="ZC")
            +-- Button (KeyTip="V")
            +-- SplitButton (KeyTip="P")
                +-- Primary action (KeyTip="PA")
                +-- Secondary action (KeyTip="PB")
```

---

## KeyTip Attached Property

The `KeyTip.Keys` attached property is the primary way to assign keyboard shortcuts.

### Basic Usage

```xml
<fluent:Button Header="Paste"
               KeyTip="V"
               Command="{Binding PasteCommand}" />

<fluent:RibbonTabItem Header="Home" KeyTip="H">
    <!-- Tab content -->
</fluent:RibbonTabItem>

<fluent:RibbonGroupBox Header="Clipboard" KeyTip="ZC">
    <!-- Group content -->
</fluent:RibbonGroupBox>
```

### Attached Property Syntax

For controls that don't have a `KeyTip` property directly, use the attached property syntax:

```xml
<Image Width="24" Height="24"
       Fluent:KeyTip.Keys="K"
       Source="PasteImage.png" />

<MenuItem Header="Save As"
          Fluent:KeyTip.Keys="A" />
```

### Property Definition (from KeyTip.cs)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `KeyTip.Keys` | `string` | `null` | Key sequence to activate the control |
| `KeyTip.AutoPlacement` | `bool` | `true` | Use automatic positioning or manual |
| `KeyTip.HorizontalAlignment` | `HorizontalAlignment` | `Center` | Manual horizontal position |
| `KeyTip.VerticalAlignment` | `VerticalAlignment` | `Center` | Manual vertical position |
| `KeyTip.Margin` | `Thickness` | `0` | Additional offset for positioning |

---

## Automatic vs Manual Assignment

### Automatic Assignment

Most Fluent.Ribbon controls expose a `KeyTip` property that wraps `KeyTip.Keys`:

```csharp
// In Button.cs
public string? KeyTip
{
    get => (string?)this.GetValue(KeyTipProperty);
    set => this.SetValue(KeyTipProperty, value);
}

public static readonly DependencyProperty KeyTipProperty =
    Fluent.KeyTip.KeysProperty.AddOwner(typeof(Button));
```

**Controls with built-in KeyTip property:**
- `Button`, `ToggleButton`, `RadioButton`, `CheckBox`
- `DropDownButton`, `SplitButton`
- `RibbonTabItem`
- `RibbonGroupBox`
- `Spinner`, `ComboBox`, `TextBox`
- `MenuItem`
- `BackstageTabItem`
- `InRibbonGallery`

### Manual Assignment via Attached Property

For non-Fluent controls or custom elements:

```xml
<!-- Attached property on any FrameworkElement -->
<Image Fluent:KeyTip.Keys="I" Source="icon.png" />
<TextBlock Fluent:KeyTip.Keys="T" Text="Label" />

<!-- On standard WPF controls -->
<MenuItem Fluent:KeyTip.Keys="S" Header="Save" />
```

---

## Alt Key Activation Behavior

### KeyTipService (from KeyTipService.cs)

The `KeyTipService` class handles all keyboard input for KeyTip navigation.

#### Activation Keys

| Key | Action |
|-----|--------|
| `Alt` (left or right) | Toggle KeyTip display |
| `F10` | Toggle KeyTip display |
| `Escape` | Go back one level or cancel KeyTips |
| `Alt + NumPad0-9` | Enter special characters (cancels KeyTips) |

#### Default Activation Keys

```csharp
public static IList<Key> DefaultKeyTipKeys =>
    new List<Key>
    {
        Key.LeftAlt,
        Key.RightAlt,
        Key.F10
    };
```

#### Customizing Activation Keys

```csharp
// In code-behind or ViewModel
ribbon.KeyTipKeys.Clear();
ribbon.KeyTipKeys.Add(Key.LeftAlt);  // Only left Alt
ribbon.KeyTipKeys.Add(Key.F10);       // And F10
// Removed: RightAlt
```

#### Timing

- **Delayed show**: KeyTips appear after 0.7 seconds when holding Alt
- **Immediate show**: KeyTips appear instantly when pressing Alt then releasing

```csharp
// From KeyTipService.cs
this.timer = new DispatcherTimer(
    TimeSpan.FromSeconds(0.7),  // Delay before showing
    DispatcherPriority.SystemIdle,
    this.OnDelayedShow,
    Dispatcher.CurrentDispatcher);
```

---

## Keyboard Navigation Patterns

### Navigation Chain

1. Press **Alt** or **F10** to show root-level KeyTips
2. Type KeyTip character(s) to navigate deeper or activate
3. Press **Escape** to go back one level
4. Clicking anywhere or pressing **Alt** again cancels KeyTips

### Focus Management

```csharp
// KeyTipService saves and restores focus
private void Show()
{
    // If focus is inside the Ribbon already we don't want to jump around
    if (UIHelper.GetParent<Ribbon>(Keyboard.FocusedElement as DependencyObject) is null)
    {
        this.backUpFocusedControl = FocusWrapper.GetWrapperForCurrentFocus();
    }
    // ... show KeyTips
}

private void OnAdornerChainTerminated(object? sender, KeyTipPressedResult e)
{
    if (e.PressedElementAquiredFocus == false)
    {
        this.RestoreFocus();  // Return focus to original control
    }
}
```

### Multi-Character KeyTips

KeyTips can be multiple characters for unique identification:

```xml
<fluent:RibbonTabItem Header="Home" KeyTip="H" />
<fluent:RibbonTabItem Header="History" KeyTip="HI" />

<fluent:Button Header="Format Painter" KeyTip="FP" />
<fluent:ComboBox Header="Font Family" KeyTip="FF" />
<fluent:ComboBox Header="Font Size" KeyTip="FS" />
```

KeyTips filter as you type:
- Press "F" - shows only KeyTips starting with "F"
- Press "P" - matches "FP", triggers Format Painter

---

## SplitButton KeyTip Behavior

SplitButtons have two actions requiring two KeyTips. Fluent.Ribbon provides flexible configuration:

### Default Behavior (Single KeyTip)

```xml
<fluent:SplitButton Header="Paste" KeyTip="P">
    <!-- Primary action: P + "A" = "PA" -->
    <!-- Secondary action: P + "B" = "PB" -->
</fluent:SplitButton>
```

### Custom Postfixes

```xml
<fluent:SplitButton Header="Paste"
                    KeyTip="V"
                    PrimaryActionKeyTipPostfix="P"
                    SecondaryActionKeyTipPostfix="D">
    <!-- Primary (button): "VP" -->
    <!-- Secondary (dropdown): "VD" -->
</fluent:SplitButton>
```

### Separate KeyTips

```xml
<fluent:SplitButton Header="Paste"
                    KeyTip="V"
                    SecondaryKeyTip="PO">
    <!-- Primary (button): "V" (no postfix when SecondaryKeyTip is set) -->
    <!-- Secondary (dropdown): "PO" -->
</fluent:SplitButton>
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `KeyTip` | `string` | `null` | Base KeyTip for the control |
| `PrimaryActionKeyTipPostfix` | `string` | `"A"` | Appended to KeyTip for button action |
| `SecondaryActionKeyTipPostfix` | `string` | `"B"` | Appended to KeyTip for dropdown action |
| `SecondaryKeyTip` | `string` | `""` | Explicit KeyTip for dropdown (ignores postfix) |

---

## IKeyTipedControl Interface

Controls that support KeyTip activation implement `IKeyTipedControl`:

```csharp
public interface IKeyTipedControl
{
    /// <summary>
    /// Gets and sets KeyTip for element.
    /// </summary>
    string? KeyTip { get; set; }

    /// <summary>
    /// Handles key tip pressed
    /// </summary>
    KeyTipPressedResult OnKeyTipPressed();

    /// <summary>
    /// Handles back navigation with KeyTips
    /// </summary>
    void OnKeyTipBack();
}
```

### KeyTipPressedResult

Controls return information about what happened when activated:

```csharp
public class KeyTipPressedResult
{
    public static readonly KeyTipPressedResult Empty = new();

    public bool PressedElementAquiredFocus { get; }  // True if control took focus
    public bool PressedElementOpenedPopup { get; }   // True if popup opened
}
```

**Behavior implications:**
- If `PressedElementAquiredFocus` is false, focus returns to original control
- If `PressedElementOpenedPopup` is false, popups are closed after activation

### Example Implementation (Button.cs)

```csharp
public KeyTipPressedResult OnKeyTipPressed()
{
    this.OnClick();
    return KeyTipPressedResult.Empty;  // Button doesn't take focus or open popup
}

public void OnKeyTipBack()
{
    // Nothing to do - simple control
}
```

---

## IKeyTipInformationProvider Interface

For advanced controls that need multiple KeyTips or custom KeyTip generation:

```csharp
public interface IKeyTipInformationProvider
{
    IEnumerable<KeyTipInformation> GetKeyTipInformations(bool hide);
}
```

### Example: SplitButton Implementation

```csharp
public IEnumerable<KeyTipInformation> GetKeyTipInformations(bool hide)
{
    if (string.IsNullOrEmpty(this.KeyTip) == false && this.button is not null)
    {
        if (string.IsNullOrEmpty(this.SecondaryKeyTip))
        {
            // No secondary KeyTip: use postfix for primary
            yield return new KeyTipInformation(
                this.KeyTip + this.PrimaryActionKeyTipPostfix,
                this.button,
                hide)
            {
                VisualTarget = this
            };
        }
        else
        {
            // Has secondary KeyTip: primary uses base KeyTip
            yield return new KeyTipInformation(
                this.KeyTip!,
                this.button,
                hide)
            {
                VisualTarget = this
            };
        }
    }

    // Secondary action KeyTip
    if (string.IsNullOrEmpty(this.SecondaryKeyTip) == false)
    {
        yield return new KeyTipInformation(this.SecondaryKeyTip, this, hide);
    }
    else if (string.IsNullOrEmpty(this.KeyTip) == false)
    {
        yield return new KeyTipInformation(
            this.KeyTip + this.SecondaryActionKeyTipPostfix,
            this,
            hide);
    }
}
```

---

## KeyTip Positioning

### Automatic Placement

By default, KeyTips are positioned automatically based on control type:

| Control Type | Position |
|--------------|----------|
| Large buttons | Bottom center of control |
| Small/Medium buttons | Left edge, vertically centered |
| RibbonTabItem | Bottom center of tab header |
| Backstage | Bottom center |
| Quick Access Toolbar items | Bottom center of button |
| Menu items | Left side, offset from edge |
| Dialog Launcher | Below the group box |

### Manual Placement

Disable auto-placement and set alignment:

```xml
<fluent:Button Header="Custom"
               KeyTip="C"
               Fluent:KeyTip.AutoPlacement="False"
               Fluent:KeyTip.HorizontalAlignment="Left"
               Fluent:KeyTip.VerticalAlignment="Top"
               Fluent:KeyTip.Margin="5,5,0,0" />
```

### Quick Access Toolbar KeyTips

QAT items have special positioning:
- KeyTip appears at bottom-center of the button
- Can be customized with `KeyTip.AutoPlacement="False"`

```xml
<fluent:QuickAccessToolBar>
    <fluent:Button Header="Save"
                   KeyTip="1"
                   Fluent:KeyTip.AutoPlacement="False"
                   Fluent:KeyTip.HorizontalAlignment="Center" />
</fluent:QuickAccessToolBar>
```

---

## Accessibility Considerations

### Screen Reader Support

KeyTips enhance accessibility by providing keyboard alternatives to mouse interaction:

1. **Discoverability**: Press Alt to see all available shortcuts
2. **Sequential access**: Navigate complex menus one level at a time
3. **Escape route**: Always press Escape to go back

### Best Practices

```xml
<!-- Good: Meaningful, memorable KeyTips -->
<fluent:Button Header="Save" KeyTip="S" />
<fluent:Button Header="Print" KeyTip="P" />
<fluent:RibbonTabItem Header="Home" KeyTip="H" />

<!-- Good: Two-character for disambiguation -->
<fluent:Button Header="Save As" KeyTip="SA" />
<fluent:Button Header="Send" KeyTip="SE" />

<!-- Avoid: Non-intuitive KeyTips -->
<fluent:Button Header="Save" KeyTip="X" />  <!-- X doesn't relate to Save -->
```

### Automation Peers

Fluent.Ribbon controls implement automation peers for UI Automation support:

```csharp
// From Button.cs
protected override AutomationPeer OnCreateAutomationPeer() =>
    new Fluent.Automation.Peers.RibbonButtonAutomationPeer(this);
```

---

## KeyTip Styling

### Default Style

```xml
<!-- From Themes\Controls\KeyTip.xaml -->
<Style x:Key="Fluent.Ribbon.Styles.KeyTip"
       TargetType="{x:Type Fluent:KeyTip}">
    <Setter Property="Background"
            Value="{DynamicResource Fluent.Ribbon.Brushes.KeyTip.Background}" />
    <Setter Property="BorderBrush"
            Value="{DynamicResource Fluent.Ribbon.Brushes.KeyTip.Border}" />
    <Setter Property="BorderThickness" Value="0" />
    <Setter Property="Foreground"
            Value="{DynamicResource Fluent.Ribbon.Brushes.White}" />
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type Fluent:KeyTip}">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}">
                    <TextBlock Margin="4 -1 4 1"
                               HorizontalAlignment="Center"
                               VerticalAlignment="Center"
                               Foreground="{TemplateBinding Foreground}"
                               Text="{TemplateBinding Content}"
                               TextWrapping="Wrap" />
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
    <Style.Triggers>
        <Trigger Property="IsEnabled" Value="False">
            <Setter Property="Opacity" Value="0.5" />
        </Trigger>
    </Style.Triggers>
</Style>
```

### Theme Brushes

```xml
<!-- KeyTip colors from theme files -->
<SolidColorBrush x:Key="Fluent.Ribbon.Brushes.KeyTip.Background"
                 Color="{StaticResource Fluent.Ribbon.Colors.Gray1}" />
<SolidColorBrush x:Key="Fluent.Ribbon.Brushes.KeyTip.Border"
                 Color="{StaticResource Fluent.Ribbon.Colors.Gray2}" />
```

### Custom KeyTip Appearance

```xml
<Style TargetType="Fluent:KeyTip"
       BasedOn="{StaticResource Fluent.Ribbon.Styles.KeyTip}">
    <Setter Property="Background" Value="#0078D4" />
    <Setter Property="Foreground" Value="White" />
    <Setter Property="FontWeight" Value="Bold" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="BorderBrush" Value="#005A9E" />
</Style>
```

---

## Events and Callbacks

### Ribbon-Level Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsKeyTipHandlingEnabled` | `bool` | Enable/disable KeyTip handling entirely |
| `AreAnyKeyTipsVisible` | `bool` (read-only) | True if KeyTips are currently displayed |
| `KeyTipKeys` | `ObservableCollection<Key>` | Keys that activate KeyTip mode |

### Disabling KeyTips

```xml
<!-- Disable KeyTips for the entire ribbon -->
<fluent:Ribbon IsKeyTipHandlingEnabled="False">
    <!-- ... -->
</fluent:Ribbon>
```

```csharp
// Dynamically disable/enable
ribbon.IsKeyTipHandlingEnabled = false;  // Disable
ribbon.IsKeyTipHandlingEnabled = true;   // Re-enable
```

### Checking KeyTip Visibility

```csharp
if (ribbon.AreAnyKeyTipsVisible)
{
    // KeyTips are currently showing
    // Avoid conflicting keyboard operations
}
```

---

## Programmatic KeyTip Management

### KeyTipAdorner (Internal)

The `KeyTipAdorner` class manages KeyTip display for a scope of controls:

```csharp
// Properties
public bool IsAdornerChainAlive { get; }      // True if any adorner in chain is active
public bool AreAnyKeyTipsVisible { get; }     // True if visible KeyTips exist
public KeyTipAdorner ActiveKeyTipAdorner { get; }  // Current deepest adorner
public IReadOnlyList<KeyTipInformation> KeyTipInformations { get; }

// Methods
public void Attach();                          // Show KeyTips
public void Detach();                          // Hide KeyTips
public void Terminate(KeyTipPressedResult);    // End entire chain
public void Back();                            // Go up one level
public bool Forward(string keys, bool click);  // Navigate to child
public void FilterKeyTips(string keys);        // Filter visible KeyTips
public bool ContainsKeyTipStartingWith(string keys);  // Check for matches

// Events
public event EventHandler<KeyTipPressedResult>? Terminated;
```

---

## DO NOT DO Section

### Common Mistakes

**DO NOT use duplicate KeyTips in the same scope:**

```xml
<!-- BAD: Duplicate "S" in same group -->
<fluent:RibbonGroupBox Header="File">
    <fluent:Button Header="Save" KeyTip="S" />
    <fluent:Button Header="Send" KeyTip="S" />  <!-- Conflict! -->
</fluent:RibbonGroupBox>

<!-- GOOD: Unique KeyTips -->
<fluent:RibbonGroupBox Header="File">
    <fluent:Button Header="Save" KeyTip="S" />
    <fluent:Button Header="Send" KeyTip="SE" />
</fluent:RibbonGroupBox>
```

**DO NOT use KeyTips that conflict with Windows shortcuts:**

```xml
<!-- BAD: F4 closes windows, F10 activates menus -->
<fluent:Button Header="Function" KeyTip="F10" />

<!-- GOOD: Use single letters or letter combinations -->
<fluent:Button Header="Function" KeyTip="FN" />
```

**DO NOT forget KeyTips for tabs and groups:**

```xml
<!-- BAD: No KeyTip on tab, user can't navigate by keyboard -->
<fluent:RibbonTabItem Header="Home">
    <fluent:RibbonGroupBox Header="Clipboard">
        <fluent:Button Header="Paste" KeyTip="V" />
    </fluent:RibbonGroupBox>
</fluent:RibbonTabItem>

<!-- GOOD: Complete KeyTip chain -->
<fluent:RibbonTabItem Header="Home" KeyTip="H">
    <fluent:RibbonGroupBox Header="Clipboard" KeyTip="CB">
        <fluent:Button Header="Paste" KeyTip="V" />
    </fluent:RibbonGroupBox>
</fluent:RibbonTabItem>
```

**DO NOT mix attached property and property syntax on same control:**

```xml
<!-- BAD: Confusing, which one wins? -->
<fluent:Button Header="Save"
               KeyTip="S"
               Fluent:KeyTip.Keys="X" />

<!-- GOOD: Use one or the other -->
<fluent:Button Header="Save" KeyTip="S" />
```

**DO NOT disable KeyTips without providing alternative keyboard access:**

```xml
<!-- BAD: No keyboard access at all -->
<fluent:Ribbon IsKeyTipHandlingEnabled="False">
    <!-- No other keyboard shortcuts defined -->
</fluent:Ribbon>

<!-- GOOD: If disabling KeyTips, use Command shortcuts -->
<fluent:Ribbon IsKeyTipHandlingEnabled="False">
    <fluent:RibbonTabItem Header="Home">
        <fluent:Button Header="Save"
                       Command="{Binding SaveCommand}"
                       InputGestureText="Ctrl+S" />
    </fluent:RibbonTabItem>
</fluent:Ribbon>
```

**DO NOT use very long KeyTip sequences:**

```xml
<!-- BAD: Too many characters, hard to type -->
<fluent:Button Header="Export to PDF" KeyTip="EXPDF" />

<!-- GOOD: Keep it short (1-3 characters) -->
<fluent:Button Header="Export to PDF" KeyTip="EP" />
```

**DO NOT assume KeyTips work when Ribbon is collapsed:**

```csharp
// KeyTipService.cs checks this
if (this.ribbon.IsCollapsed || this.ribbon.IsEnabled == false)
{
    return;  // KeyTips are ignored
}
```

---

## Complete Example

```xml
<fluent:Ribbon x:Name="ribbon"
               IsKeyTipHandlingEnabled="True">

    <!-- Application Menu -->
    <fluent:Ribbon.Menu>
        <fluent:ApplicationMenu KeyTip="F">
            <fluent:MenuItem Header="New" KeyTip="N" />
            <fluent:MenuItem Header="Open" KeyTip="O" />
            <fluent:MenuItem Header="Save" KeyTip="S" />
            <fluent:MenuItem Header="Save As" KeyTip="A" />
            <fluent:Separator />
            <fluent:MenuItem Header="Exit" KeyTip="X" />
        </fluent:ApplicationMenu>
    </fluent:Ribbon.Menu>

    <!-- Quick Access Toolbar -->
    <fluent:Ribbon.QuickAccessToolBar>
        <fluent:QuickAccessToolBar>
            <fluent:Button Header="Save" KeyTip="1" Icon="{StaticResource SaveIcon}" />
            <fluent:Button Header="Undo" KeyTip="2" Icon="{StaticResource UndoIcon}" />
            <fluent:Button Header="Redo" KeyTip="3" Icon="{StaticResource RedoIcon}" />
        </fluent:QuickAccessToolBar>
    </fluent:Ribbon.QuickAccessToolBar>

    <!-- Home Tab -->
    <fluent:RibbonTabItem Header="Home" KeyTip="H">
        <fluent:RibbonGroupBox Header="Clipboard" KeyTip="CB">
            <fluent:SplitButton Header="Paste"
                                KeyTip="V"
                                Icon="{StaticResource PasteIcon}"
                                Command="{Binding PasteCommand}">
                <fluent:MenuItem Header="Paste Special" KeyTip="S" />
                <fluent:MenuItem Header="Paste as Link" KeyTip="L" />
            </fluent:SplitButton>
            <fluent:Button Header="Cut" KeyTip="X" SizeDefinition="Middle" />
            <fluent:Button Header="Copy" KeyTip="C" SizeDefinition="Middle" />
            <fluent:Button Header="Format Painter" KeyTip="FP" SizeDefinition="Middle" />
        </fluent:RibbonGroupBox>

        <fluent:RibbonGroupBox Header="Font" KeyTip="FN">
            <fluent:ComboBox Header="Font Family" KeyTip="FF" Width="150" />
            <fluent:ComboBox Header="Font Size" KeyTip="FS" Width="60" />
            <fluent:ToggleButton Header="Bold" KeyTip="B" Icon="{StaticResource BoldIcon}" />
            <fluent:ToggleButton Header="Italic" KeyTip="I" Icon="{StaticResource ItalicIcon}" />
            <fluent:ToggleButton Header="Underline" KeyTip="U" Icon="{StaticResource UnderlineIcon}" />
        </fluent:RibbonGroupBox>
    </fluent:RibbonTabItem>

    <!-- View Tab -->
    <fluent:RibbonTabItem Header="View" KeyTip="V">
        <fluent:RibbonGroupBox Header="Zoom" KeyTip="Z">
            <fluent:Button Header="Zoom In" KeyTip="I" />
            <fluent:Button Header="Zoom Out" KeyTip="O" />
            <fluent:Button Header="100%" KeyTip="1" />
        </fluent:RibbonGroupBox>
    </fluent:RibbonTabItem>

</fluent:Ribbon>
```

### Navigation Flow

1. **Press Alt** - Shows: F (File), H (Home), V (View), 1/2/3 (QAT)
2. **Press H** - Shows Home tab KeyTips: CB (Clipboard), FN (Font)
3. **Press CB** - Shows Clipboard group: V (Paste), X (Cut), C (Copy), FP (Format Painter)
4. **Press V** - Shows Paste options (VA=primary button, VB=dropdown) or triggers paste
5. **Press Escape** - Goes back one level
6. **Press Alt** - Cancels KeyTips

---

## Summary

| Topic | Key Points |
|-------|------------|
| **Activation** | Alt or F10 toggles KeyTips; Escape goes back |
| **Assignment** | Use `KeyTip="X"` property or `Fluent:KeyTip.Keys="X"` attached property |
| **Multi-char** | Use 2-3 character KeyTips for disambiguation |
| **SplitButton** | Has `PrimaryActionKeyTipPostfix` and `SecondaryKeyTip` for dual actions |
| **Positioning** | Automatic by default; use `KeyTip.AutoPlacement="False"` for manual |
| **Styling** | Override `Fluent.Ribbon.Brushes.KeyTip.*` brushes |
| **Accessibility** | Provides keyboard-only navigation; supports screen readers |
| **Disabling** | `IsKeyTipHandlingEnabled="False"` on Ribbon control |
