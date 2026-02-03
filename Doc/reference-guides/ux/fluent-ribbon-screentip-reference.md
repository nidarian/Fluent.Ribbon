---
title: Fluent.Ribbon ScreenTip Reference
description: Comprehensive reference for ScreenTip - enhanced tooltips for Fluent.Ribbon controls
tags: [screentip, tooltip, help, ux]
see_also:
  - fluent-ribbon-keytips-reference.md
  - ../controls/fluent-ribbon-button-controls-reference.md
---

# Fluent.Ribbon ScreenTip Reference

**Comprehensive reference for ScreenTip - enhanced tooltips for Fluent.Ribbon controls.**

*Based on source analysis of Fluent.Ribbon Controls/ScreenTip.cs, Themes/Controls/ScreenTip.xaml, and Showcase examples.*

---

## Overview

ScreenTip is an enhanced tooltip control that provides rich, Office-style tooltips for ribbon controls. Unlike standard WPF tooltips, ScreenTips support:

- **Structured content**: Title, description text, and optional image
- **Disabled state messaging**: Explains why a control is disabled
- **F1 Help integration**: Press F1 while tooltip is visible to open help
- **Ribbon-aligned positioning**: Automatically positions below the ribbon
- **Accessibility support**: Full automation peer implementation

```
+------------------------------------------+
| Title (Bold)                             |
+------------------------------------------+
| [Image]  Description text that can       |
|          wrap to multiple lines.         |
+------------------------------------------+
| [!] This command is currently disabled.  |
|     Custom disable reason text.          |
+------------------------------------------+
| [?] Press F1 for help                    |
+------------------------------------------+
```

---

## Basic Usage

### Simple ScreenTip

```xml
<fluent:Button Header="Paste">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip Title="Paste (Ctrl+V)"
                          Text="Paste the contents of the clipboard." />
    </fluent:Button.ToolTip>
</fluent:Button>
```

### ScreenTip with Image

```xml
<fluent:Button Header="Insert Chart">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip Title="Insert Chart"
                          Text="Insert a chart to illustrate and compare data.&#x0a;&#x0a;Bar, Pie, Line, Area and Surface are some of the available types."
                          Image="pack://application:,,,/MyApp;component/Images/ChartPreview.png" />
    </fluent:Button.ToolTip>
</fluent:Button>
```

### ScreenTip with Disable Reason

```xml
<fluent:Button Header="Cut"
               IsEnabled="{Binding HasSelection}">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip Title="Cut (Ctrl+X)"
                          Text="Cut the selected content to the Clipboard."
                          DisableReason="Select content first to enable this command." />
    </fluent:Button.ToolTip>
</fluent:Button>
```

### ScreenTip with F1 Help

```xml
<fluent:Button Header="Format Painter">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip Title="Format Painter"
                          Text="Copy formatting from one place and apply it to another."
                          HelpTopic="FormatPainter" />
    </fluent:Button.ToolTip>
</fluent:Button>
```

---

## Dependency Properties

### Content Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Title` | string | "" | Bold title at the top of the ScreenTip |
| `Text` | object | "" | Main description text (supports data binding) |
| `TextTemplate` | DataTemplate | TextBlock with wrapping | Template for rendering Text content |
| `Image` | object | null | Image displayed to the left of Text (max 48px height) |

### Disabled State Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DisableReason` | string | "" | Explanation shown when parent control is disabled |

### Help Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `HelpTopic` | object | null | Help topic identifier passed to HelpPressed event |
| `HelpLabelVisibility` | Visibility | Visible | Controls visibility of "Press F1 for help" label |

### Positioning Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IsRibbonAligned` | bool | true | Positions ScreenTip below the ribbon instead of below the control |
| `Width` | double | 205 | ScreenTip width (from default style) |

---

## Static Events

### HelpPressed Event

Fired when user presses F1 while a ScreenTip with a HelpTopic is visible.

```csharp
// Subscribe to help event (typically in App.xaml.cs or MainWindow)
ScreenTip.HelpPressed += OnScreenTipHelpPressed;

private void OnScreenTipHelpPressed(object? sender, ScreenTipHelpEventArgs e)
{
    // e.HelpTopic contains the value from the ScreenTip's HelpTopic property
    if (e.HelpTopic is string helpId)
    {
        // Open help system with the specific topic
        HelpSystem.ShowHelp(helpId);
    }
    else if (e.HelpTopic is Uri helpUri)
    {
        // Open URL in browser
        Process.Start(new ProcessStartInfo(helpUri.ToString()) { UseShellExecute = true });
    }
}
```

### ScreenTipHelpEventArgs

```csharp
public class ScreenTipHelpEventArgs : EventArgs
{
    public object? HelpTopic { get; }
}
```

---

## Styling and Theming

### Default Style Resources

```xml
<!-- Style key -->
<Style x:Key="Fluent.Ribbon.Styles.ScreenTip" TargetType="{x:Type fluent:ScreenTip}">

<!-- Template key -->
<ControlTemplate x:Key="Fluent.Ribbon.Templates.ScreenTip" TargetType="{x:Type fluent:ScreenTip}">
```

### Theme Brushes

| Brush Key | Usage |
|-----------|-------|
| `Fluent.Ribbon.Brushes.ScreenTip.Background` | ScreenTip background color |
| `Fluent.Ribbon.Brushes.ScreenTip.Border` | ScreenTip border color |
| `Fluent.Ribbon.Brushes.LabelText` | Text foreground color |
| `Fluent.Ribbon.Brushes.Gray6` | Separator line color |

### Theme Images

| Image Key | Usage |
|-----------|-------|
| `Fluent.Ribbon.Images.Warning` | Warning icon for disable reason section |
| `Fluent.Ribbon.Images.Help` | Help icon for F1 help section |

### Custom Width

Override the default 205px width:

```xml
<fluent:ScreenTip Title="Wide ScreenTip"
                  Width="300"
                  Text="This ScreenTip has a custom width of 300 pixels." />
```

### Custom Text Template

Use TextTemplate for rich text formatting:

```xml
<fluent:ScreenTip Title="Paste (Ctrl+V)"
                  Text="Paste the contents of the clipboard.">
    <fluent:ScreenTip.TextTemplate>
        <DataTemplate>
            <TextBlock TextWrapping="Wrap">
                <Run Text="Paste the " />
                <Run FontWeight="Bold" Text="contents" />
                <Run Text=" of the " />
                <Run FontWeight="Bold" Text="clipboard" />
                <Run Text="." />
            </TextBlock>
        </DataTemplate>
    </fluent:ScreenTip.TextTemplate>
</fluent:ScreenTip>
```

---

## Localization

ScreenTip text labels are localized via the Fluent.Ribbon localization system.

### Localizable Strings

| Property | English Default | Description |
|----------|-----------------|-------------|
| `ScreenTipDisableReasonHeader` | "This command is currently disabled." | Header shown above DisableReason text |
| `ScreenTipF1LabelHeader` | "Press F1 for help." | F1 help instruction label |

### Custom Localization

```csharp
// Create custom localization class
public class MyLocalization : RibbonLocalizationBase
{
    public override string ScreenTipDisableReasonHeader => "This command is unavailable.";
    public override string ScreenTipF1LabelHeader => "Press F1 for more information.";
    // ... other overrides
}

// Apply at application startup
RibbonLocalization.Current.Localization = new MyLocalization();
```

---

## Positioning Behavior

### IsRibbonAligned = true (Default)

The ScreenTip appears below the entire ribbon, not below the individual control:

```
+---------------------------+
|      Ribbon Tabs          |
|---------------------------|
|  [Button] [Button] [Btn]  |  <- Hover here
|---------------------------|
+---------------------------+
+---------------------------+
|    ScreenTip appears      |  <- ScreenTip here (below ribbon)
|    below the ribbon       |
+---------------------------+
```

### IsRibbonAligned = false

The ScreenTip appears directly below the control:

```
+---------------------------+
|      Ribbon Tabs          |
|---------------------------|
|  [Button] [Button] [Btn]  |  <- Hover here
|           +-------------+ |
|           | ScreenTip   | |  <- ScreenTip here (below control)
|           +-------------+ |
+---------------------------+
```

### Special Cases

- **Quick Access Toolbar items**: Always position below the control (ignore IsRibbonAligned)
- **Context menu items**: Always position below the control
- **Popup/dropdown items**: Position relative to the popup, not the ribbon

---

## Tooltip Service Integration

Fluent.Ribbon controls use a custom ToolTipService that sets these defaults:

| Property | Default Value | Description |
|----------|---------------|-------------|
| `ShowOnDisabled` | true | Show tooltips even on disabled controls |
| `InitialShowDelay` | 900ms | Delay before tooltip appears |
| `BetweenShowDelay` | 0ms | No delay when moving between controls |
| `ShowDuration` | 20000ms (20 seconds) | How long tooltip stays visible |

This allows ScreenTips to display the DisableReason on disabled controls.

---

## Template Structure

The ScreenTip template consists of these named parts:

```
screenTipPanel (StackPanel)
├── title (TextBlock) - Bold title
├── imageAndTextSection (Grid)
│   ├── image (ContentPresenter) - Optional image
│   └── text (ContentPresenter) - Text content
├── separator (Border) - Line above disable section
├── disableReasonSection (Grid)
│   ├── Warning icon
│   ├── disableReasonHeader (TextBlock) - "This command is currently disabled."
│   ├── disableReasonText (TextBlock) - Custom disable reason
│   └── helpText (TextBlock) - Help text when disabled
├── separator2 (Border) - Line above help section
└── helpSection (Grid)
    ├── Help icon
    └── textBlock (TextBlock) - "Press F1 for help."
```

### Visibility Triggers

| Condition | Result |
|-----------|--------|
| Control is disabled AND DisableReason is set | Show disableReasonSection |
| Control is enabled OR DisableReason is empty | Hide disableReasonSection |
| HelpTopic is null | Hide helpSection and separator2 |
| Text and Image both empty | Hide imageAndTextSection |
| Image is null | Hide image ContentPresenter |

---

## Automation Support

ScreenTip implements `RibbonScreenTipAutomationPeer` for accessibility:

- **ClassName**: "ScreenTip"
- **Name**: Uses `Title` property if AutomationProperties.Name is not set
- **ControlType**: ToolTip

---

## Common Patterns

### Menu Item with ScreenTip

```xml
<fluent:MenuItem Header="Paste Special...">
    <fluent:MenuItem.ToolTip>
        <fluent:ScreenTip Title="Paste Special"
                          Text="Paste with formatting options."
                          HelpTopic="PasteSpecial" />
    </fluent:MenuItem.ToolTip>
</fluent:MenuItem>
```

### SplitButton with ScreenTip

```xml
<fluent:SplitButton Header="Paste"
                    Command="{Binding PasteCommand}">
    <fluent:SplitButton.ToolTip>
        <fluent:ScreenTip Title="Paste (Ctrl+V)"
                          Width="190"
                          DisableReason="Clipboard is empty."
                          Text="Paste the contents of the clipboard." />
    </fluent:SplitButton.ToolTip>
    <!-- Dropdown items -->
</fluent:SplitButton>
```

### Backstage Item with ScreenTip

```xml
<fluent:BackstageTabItem Header="Home">
    <fluent:BackstageTabItem.ToolTip>
        <fluent:ScreenTip Title="Home"
                          Text="Return to the main view." />
    </fluent:BackstageTabItem.ToolTip>
</fluent:BackstageTabItem>
```

### Dynamic DisableReason

```xml
<fluent:Button Header="Save"
               IsEnabled="{Binding CanSave}">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip Title="Save (Ctrl+S)"
                          Text="Save the current document."
                          DisableReason="{Binding SaveDisableReason}" />
    </fluent:Button.ToolTip>
</fluent:Button>
```

```csharp
// ViewModel
public string SaveDisableReason
{
    get
    {
        if (!HasUnsavedChanges) return "No changes to save.";
        if (IsReadOnly) return "Document is read-only.";
        if (IsSaving) return "Save already in progress.";
        return string.Empty;
    }
}
```

---

## Multiline Text

Use XML entities for line breaks in Text:

```xml
<fluent:ScreenTip Title="Insert Chart"
                  Text="Insert a chart to illustrate and compare data.&#x0a;&#x0a;Available types:&#x0a;- Bar&#x0a;- Pie&#x0a;- Line&#x0a;- Area" />
```

Result:
```
Insert a chart to illustrate and compare data.

Available types:
- Bar
- Pie
- Line
- Area
```

---

## DO NOT DO Section

### DO NOT: Use ScreenTip as Content

ScreenTip should only be used as a ToolTip, not as regular content:

```xml
<!-- WRONG - Don't do this -->
<StackPanel>
    <fluent:ScreenTip Title="Info" Text="This won't work properly" />
</StackPanel>

<!-- CORRECT - Use as ToolTip -->
<fluent:Button Header="Info">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip Title="Info" Text="This works correctly" />
    </fluent:Button.ToolTip>
</fluent:Button>
```

### DO NOT: Set HelpTopic Without Event Handler

If you set HelpTopic but don't handle the HelpPressed event, F1 will do nothing:

```csharp
// WRONG - HelpTopic set but event not handled
<fluent:ScreenTip HelpTopic="MyTopic" ... />

// CORRECT - Handle the event
ScreenTip.HelpPressed += (s, e) => OpenHelp(e.HelpTopic);
```

### DO NOT: Use Regular ToolTip Style for ScreenTip

ScreenTip has its own style; don't apply standard ToolTip styles:

```xml
<!-- WRONG - Don't apply standard ToolTip style -->
<fluent:ScreenTip Style="{StaticResource {x:Type ToolTip}}" ... />

<!-- CORRECT - Use ScreenTip's own style or customize it -->
<fluent:ScreenTip Style="{DynamicResource Fluent.Ribbon.Styles.ScreenTip}" ... />
```

### DO NOT: Forget DisableReason Requires Disabled Control

DisableReason only appears when the parent control is disabled:

```xml
<!-- WRONG - DisableReason won't show because button is enabled -->
<fluent:Button Header="Save" IsEnabled="True">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip DisableReason="This won't appear" />
    </fluent:Button.ToolTip>
</fluent:Button>

<!-- CORRECT - DisableReason shows when control is disabled -->
<fluent:Button Header="Save" IsEnabled="False">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip DisableReason="Document is read-only." />
    </fluent:Button.ToolTip>
</fluent:Button>
```

### DO NOT: Set Extremely Wide ScreenTips

Keep ScreenTips reasonably sized for readability:

```xml
<!-- WRONG - Too wide, hard to read -->
<fluent:ScreenTip Width="600" ... />

<!-- CORRECT - Use reasonable widths (190-300 typical) -->
<fluent:ScreenTip Width="250" ... />
```

### DO NOT: Put Long Text Without Line Breaks

Long unbroken text is hard to read; use line breaks:

```xml
<!-- WRONG - Long unbroken text -->
<fluent:ScreenTip Text="This is a very long description that goes on and on without any breaks and becomes very difficult to read as a result of being one continuous sentence that never seems to end." />

<!-- CORRECT - Use line breaks for readability -->
<fluent:ScreenTip Text="This is a description with logical breaks.&#x0a;&#x0a;Additional details appear on separate lines for better readability." />
```

### DO NOT: Use ScreenTip When Simple ToolTip Suffices

For simple text-only tooltips, standard ToolTip is lighter weight:

```xml
<!-- OVERKILL - ScreenTip for simple text -->
<fluent:Button Header="OK">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip Title="OK" />
    </fluent:Button.ToolTip>
</fluent:Button>

<!-- BETTER - Use simple ToolTip -->
<fluent:Button Header="OK" ToolTip="OK" />

<!-- USE SCREENTIP WHEN - You need structured content, images, or disable reasons -->
<fluent:Button Header="Save">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip Title="Save (Ctrl+S)"
                          Text="Save the current document."
                          DisableReason="No changes to save."
                          HelpTopic="Saving" />
    </fluent:Button.ToolTip>
</fluent:Button>
```

---

## Quick Access Toolbar Behavior

When controls are added to the Quick Access Toolbar (QAT), their ScreenTips are automatically preserved:

```xml
<!-- Original button with ScreenTip -->
<fluent:Button x:Name="PasteButton" Header="Paste">
    <fluent:Button.ToolTip>
        <fluent:ScreenTip Title="Paste (Ctrl+V)"
                          Text="Paste clipboard contents." />
    </fluent:Button.ToolTip>
</fluent:Button>

<!-- When added to QAT, the ScreenTip is automatically cloned -->
<fluent:QuickAccessMenuItem IsChecked="True" Target="{x:Reference PasteButton}" />
```

Note: QAT items ignore `IsRibbonAligned` and always position tooltips below the control.

---

## Property Summary

| Property | Type | Default | Category |
|----------|------|---------|----------|
| `Title` | string | "" | Content |
| `Text` | object | "" | Content |
| `TextTemplate` | DataTemplate | (wrapping TextBlock) | Content |
| `Image` | object | null | Content |
| `DisableReason` | string | "" | Disabled State |
| `HelpTopic` | object | null | Help |
| `HelpLabelVisibility` | Visibility | Visible | Help |
| `IsRibbonAligned` | bool | true | Positioning |
| `Width` | double | 205 | Sizing |

---

## Related Classes

| Class | Purpose |
|-------|---------|
| `ScreenTip` | The enhanced tooltip control |
| `ScreenTipHelpEventArgs` | Event args for HelpPressed event |
| `RibbonScreenTipAutomationPeer` | Automation peer for accessibility |
| `ToolTipService` | Service that configures tooltip behavior for ribbon controls |
| `RibbonLocalizationBase` | Contains localized ScreenTip strings |

---

## Version History

- **Fluent.Ribbon 10.x**: ScreenTip control with full feature set
- **Earlier versions**: Basic ScreenTip support

---

*Source files referenced:*
- `Fluent.Ribbon/Controls/ScreenTip.cs`
- `Fluent.Ribbon/Themes/Controls/ScreenTip.xaml`
- `Fluent.Ribbon/Automation/Peers/RibbonScreenTipAutomationPeer.cs`
- `Fluent.Ribbon/Services/ToolTipService.cs`
- `Fluent.Ribbon.Showcase/TestContent.xaml`
