---
title: Fluent.Ribbon Localization Reference
description: Complete reference for localizing Fluent.Ribbon UI strings and supporting multiple languages
tags: [localization, i18n, languages, translation]
see_also:
  - fluent-ribbon-accessibility-reference.md
---

# Fluent.Ribbon Localization Reference

**Complete reference for localizing Fluent.Ribbon UI strings and supporting multiple languages.**

---

## Overview

Fluent.Ribbon includes a built-in localization system that provides translated strings for all UI elements (context menus, tooltips, backstage button, etc.). The system supports 37 languages out of the box and automatically detects the user's culture.

```
+----------------------------------------------------------+
| RibbonLocalization.Current (singleton)                   |
| +------------------------------------------------------+ |
| | Culture: CultureInfo     (current UI culture)        | |
| | Localization: RibbonLocalizationBase (active strings)| |
| | LocalizationMap: Dictionary<string, Type>            | |
| +------------------------------------------------------+ |
|                          |                               |
|                          v                               |
| +------------------------------------------------------+ |
| | Language Classes (37 built-in)                       | |
| | - English, German, French, Spanish, Chinese, etc.    | |
| | - Each implements RibbonLocalizationBase             | |
| +------------------------------------------------------+ |
+----------------------------------------------------------+
```

---

## Architecture

### Key Classes

| Class | Namespace | Purpose |
|-------|-----------|---------|
| `RibbonLocalization` | `Fluent` | Singleton manager; holds current culture and localization |
| `RibbonLocalizationBase` | `Fluent.Localization` | Abstract base class with all localizable string properties |
| `RibbonLocalizationAttribute` | `Fluent.Localization` | Marks language classes with culture name and display name |
| `English`, `German`, etc. | `Fluent.Localization.Languages` | Concrete language implementations |

### Class Relationships

```
RibbonLocalization (singleton)
    |
    +-- Culture: CultureInfo
    |       Sets the active localization via culture lookup
    |
    +-- Localization: RibbonLocalizationBase
    |       The active localization instance
    |
    +-- LocalizationMap: Dictionary<string, Type>
            Maps culture names to language class types
            Key: "en", "de", "fr", "zh", "pt-BR", etc.

RibbonLocalizationBase (abstract)
    |
    +-- FallbackLocalization: English (static)
    |       Default when culture not found
    |
    +-- 31 abstract string properties
            All localizable UI strings
```

---

## RibbonLocalization Class

The main entry point for localization. Access via the static `Current` property.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Current` | `RibbonLocalization` | Static singleton instance |
| `Culture` | `CultureInfo` | Get/set the active culture; triggers localization reload |
| `Localization` | `RibbonLocalizationBase` | The current localization instance with all strings |
| `LocalizationMap` | `Dictionary<string, Type>` | Maps culture names to language class types |

### Events

| Event | Description |
|-------|-------------|
| `PropertyChanged` | Fires when `Culture` or `Localization` changes (INotifyPropertyChanged) |

### Example: Access Current Localization

```csharp
// Get current backstage button text
string fileText = RibbonLocalization.Current.Localization.BackstageButtonText;

// Get all available localizations
var available = RibbonLocalization.Current.LocalizationMap.Keys;
// Returns: "en", "de", "fr", "es", "zh", "ja", "ko", etc.
```

---

## All Localizable Strings

The `RibbonLocalizationBase` class defines all localizable strings. Here is the complete list with English defaults:

### Backstage / Application Menu

| Property | English Default | Used In |
|----------|-----------------|---------|
| `BackstageButtonText` | "File" | Backstage button header |
| `BackstageButtonKeyTip` | "F" | Backstage button KeyTip |
| `BackstageBackButtonUid` | "Close Backstage" | Screen reader accessibility |

### Quick Access Toolbar (QAT)

| Property | English Default | Used In |
|----------|-----------------|---------|
| `QuickAccessToolBarDropDownButtonTooltip` | "Customize Quick Access Toolbar" | QAT dropdown button tooltip |
| `QuickAccessToolBarMenuHeader` | "Customize Quick Access Toolbar" | QAT menu header |
| `QuickAccessToolBarMenuShowAbove` | "Show Above the Ribbon" | QAT position menu item |
| `QuickAccessToolBarMenuShowBelow` | "Show Below the Ribbon" | QAT position menu item |
| `QuickAccessToolBarMoreControlsButtonTooltip` | "More controls" | QAT overflow button tooltip |

### Ribbon Context Menu

| Property | English Default | Used In |
|----------|-----------------|---------|
| `RibbonContextMenuAddItem` | "Add to Quick Access Toolbar" | Right-click on button |
| `RibbonContextMenuAddGroup` | "Add Group to Quick Access Toolbar" | Right-click on group |
| `RibbonContextMenuAddMenu` | "Add Menu to Quick Access Toolbar" | Right-click on menu |
| `RibbonContextMenuAddGallery` | "Add Gallery to Quick Access Toolbar" | Right-click on gallery |
| `RibbonContextMenuRemoveItem` | "Remove from Quick Access Toolbar" | Right-click on QAT item |
| `RibbonContextMenuCustomizeQuickAccessToolBar` | "Customize Quick Access Toolbar..." | Context menu |
| `RibbonContextMenuCustomizeRibbon` | "Customize the Ribbon..." | Context menu |
| `RibbonContextMenuMinimizeRibbon` | "Minimize the Ribbon" | Context menu |
| `RibbonContextMenuShowAbove` | "Show Quick Access Toolbar Above the Ribbon" | Context menu |
| `RibbonContextMenuShowBelow` | "Show Quick Access Toolbar Below the Ribbon" | Context menu |

### Ribbon Display Options

| Property | English Default | Used In |
|----------|-----------------|---------|
| `ShowRibbon` | "Show Ribbon" | Display options menu header |
| `ExpandRibbon` | "Expand the Ribbon" | Display options menu |
| `MinimizeRibbon` | "Minimize the Ribbon" | Display options menu |
| `RibbonLayout` | "Ribbon Layout" | Display options menu header |
| `UseClassicRibbon` | "_Use Classic Ribbon" | Display options menu (has access key) |
| `UseSimplifiedRibbon` | "_Use Simplified Ribbon" | Display options menu (has access key) |
| `DisplayOptionsButtonScreenTipTitle` | "Ribbon Display Options" | Screentip title |
| `DisplayOptionsButtonScreenTipText` | "Configure Ribbon display options." | Screentip text |

### ScreenTips

| Property | English Default | Used In |
|----------|-----------------|---------|
| `ScreenTipDisableReasonHeader` | "This command is currently disabled." | Disabled command tooltip |
| `ScreenTipF1LabelHeader` | "Press F1 for help" | Help hint in screentips |

### Color Gallery

| Property | English Default | Used In |
|----------|-----------------|---------|
| `Automatic` | "Automatic" | Color gallery automatic option |
| `MoreColors` | "More colors..." | Color gallery more colors option |
| `NoColor` | "No color" | Color gallery no color option |

### Status Bar

| Property | English Default | Used In |
|----------|-----------------|---------|
| `CustomizeStatusBar` | "Customize Status Bar" | Status bar context menu |

---

## Culture Detection

On startup, `RibbonLocalization` automatically sets `Culture` to `CultureInfo.CurrentUICulture`.

### Culture Lookup Order

When a culture is set, the system looks up localizations in this order:

1. **Exact match** - Full culture name (e.g., "pt-BR" for Brazilian Portuguese)
2. **Two-letter fallback** - Language code only (e.g., "pt" for Portuguese)
3. **English fallback** - If no match found, defaults to English

```csharp
// Example: User's culture is "pt-BR" (Brazilian Portuguese)
// 1. Check LocalizationMap for "pt-BR" -> Found! Use Portuguese_Brazil
//
// Example: User's culture is "pt-PT" (European Portuguese)
// 1. Check LocalizationMap for "pt-PT" -> Not found
// 2. Check LocalizationMap for "pt" -> Found! Use Portuguese
//
// Example: User's culture is "xyz" (unsupported)
// 1. Check LocalizationMap for "xyz" -> Not found
// 2. Check LocalizationMap for "xy" -> Not found
// 3. Fall back to English
```

---

## Built-in Languages

Fluent.Ribbon includes 37 language localizations:

| Language | Culture Code | Class Name |
|----------|--------------|------------|
| Arabic | ar | Arabic |
| Azerbaijani | az | Azerbaijani |
| Bulgarian | bg | Bulgarian |
| Catalan | ca | Catalan |
| Chinese | zh | Chinese |
| Czech | cs | Czech |
| Danish | da | Danish |
| Dutch | nl | Dutch |
| English | en | English |
| Estonian | et | Estonian |
| Finnish | fi | Finnish |
| French | fr | French |
| German | de | German |
| Greek | el | Greek |
| Hebrew | he | Hebrew |
| Hungarian | hu | Hungarian |
| Italian | it | Italian |
| Japanese | ja | Japanese |
| Korean | ko | Korean |
| Lithuanian | lt | Lithuanian |
| Norwegian | no | Norwegian |
| Norwegian Bokmal | nb | Norwegian_Bokmal |
| Norwegian Nynorsk | nn | Norwegian_Nynorsk |
| Persian | fa | Persian |
| Polish | pl | Polish |
| Portuguese | pt | Portuguese |
| Portuguese (Brazil) | pt-BR | Portuguese_Brazil |
| Romanian | ro | Romanian |
| Russian | ru | Russian |
| Sinhala | si | Sinhala |
| Slovak | sk | Slovak |
| Slovenian | sl | Slovenian |
| Spanish | es | Spanish |
| Swedish | sv | Swedish |
| Turkish | tr | Turkish |
| Ukrainian | uk | Ukrainian |
| Vietnamese | vi | Vietnamese |

---

## Overriding Default Strings

There are several ways to customize localization strings.

### Method 1: Set Individual Properties (Runtime)

Create a custom localization class and assign it directly:

```csharp
public class CustomEnglish : RibbonLocalizationBase
{
    public CustomEnglish() : base("en-custom", "Custom English") { }

    // Override only what you need; others inherit from base
    public override string BackstageButtonText => "Menu";  // Changed from "File"
    public override string BackstageButtonKeyTip => "M";   // Changed from "F"

    // Implement all abstract members (use FallbackLocalization for unchanged)
    public override string Automatic => FallbackLocalization.Automatic;
    public override string BackstageBackButtonUid => FallbackLocalization.BackstageBackButtonUid;
    // ... (implement all 31 abstract properties)
}

// Apply at startup
RibbonLocalization.Current.Localization = new CustomEnglish();
```

### Method 2: XAML Binding Override

Override specific strings in XAML by setting properties directly:

```xml
<!-- Override backstage button text -->
<fluent:Backstage Header="Menu" fluent:KeyTip.Keys="M">
    <!-- ... -->
</fluent:Backstage>
```

### Method 3: Register Custom Localization Class

Add your custom localization to the map (must be done before first use):

```csharp
// In App.xaml.cs OnStartup, before any ribbon is created
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Add custom localization to map
        RibbonLocalization.Current.LocalizationMap["en-US"] = typeof(AmericanEnglish);

        base.OnStartup(e);
    }
}

[RibbonLocalization("American English", "en-US")]
public class AmericanEnglish : RibbonLocalizationBase
{
    // Customize for American English
    public override string BackstageButtonText => "File";
    // ... implement all abstract properties
}
```

---

## Adding New Language Support

To add support for a new language:

### Step 1: Create Language Class

```csharp
using Fluent.Localization;

namespace YourApp.Localization
{
    [RibbonLocalization("Esperanto", "eo")]
    public class Esperanto : RibbonLocalizationBase
    {
        public override string Automatic => "Aŭtomata";
        public override string BackstageBackButtonUid => FallbackLocalization.BackstageBackButtonUid;
        public override string BackstageButtonKeyTip => "D";  // Dosiero
        public override string BackstageButtonText => "Dosiero";
        public override string CustomizeStatusBar => "Agordi Statostrion";
        public override string DisplayOptionsButtonScreenTipText => "Agordi Rubando-opciojn.";
        public override string DisplayOptionsButtonScreenTipTitle => "Rubando-Opcioj";
        public override string ExpandRibbon => "Vastigi la Rubandon";
        public override string MinimizeRibbon => "Malgrandigi la Rubandon";
        public override string MoreColors => "Pli da koloroj...";
        public override string NoColor => "Neniu koloro";
        public override string QuickAccessToolBarDropDownButtonTooltip => "Agordi Rapidan Aliron";
        public override string QuickAccessToolBarMenuHeader => "Agordi Rapidan Aliron";
        public override string QuickAccessToolBarMenuShowAbove => "Montri Super la Rubando";
        public override string QuickAccessToolBarMenuShowBelow => "Montri Sub la Rubando";
        public override string QuickAccessToolBarMoreControlsButtonTooltip => "Pli da kontroloj";
        public override string RibbonContextMenuAddGallery => "Aldoni Galerion al Rapida Aliro";
        public override string RibbonContextMenuAddGroup => "Aldoni Grupon al Rapida Aliro";
        public override string RibbonContextMenuAddItem => "Aldoni al Rapida Aliro";
        public override string RibbonContextMenuAddMenu => "Aldoni Menuon al Rapida Aliro";
        public override string RibbonContextMenuCustomizeQuickAccessToolBar => "Agordi Rapidan Aliron...";
        public override string RibbonContextMenuCustomizeRibbon => "Agordi la Rubandon...";
        public override string RibbonContextMenuMinimizeRibbon => "Malgrandigi la Rubandon";
        public override string RibbonContextMenuRemoveItem => "Forigi el Rapida Aliro";
        public override string RibbonContextMenuShowAbove => "Montri Rapidan Aliron Super la Rubando";
        public override string RibbonContextMenuShowBelow => "Montri Rapidan Aliron Sub la Rubando";
        public override string RibbonLayout => "Rubando-Aranĝo";
        public override string ScreenTipDisableReasonHeader => "Ĉi tiu komando estas malŝaltita.";
        public override string ScreenTipF1LabelHeader => "Premu F1 por helpo";
        public override string ShowRibbon => "Montri Rubandon";
        public override string UseClassicRibbon => "_Uzi Klasikan Rubandon";
        public override string UseSimplifiedRibbon => "_Uzi Simpligitan Rubandon";
    }
}
```

### Step 2: Register at Startup

```csharp
// In App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    // Register before any ribbon is created
    RibbonLocalization.Current.LocalizationMap["eo"] = typeof(Esperanto);

    base.OnStartup(e);
}
```

### Step 3: Use the Language

```csharp
// Set programmatically
RibbonLocalization.Current.Culture = new CultureInfo("eo");

// Or it will auto-detect if user's system is set to Esperanto
```

---

## Runtime Language Switching

You can change the language at runtime. The UI updates automatically through data binding.

### Basic Language Switch

```csharp
// Switch to German
RibbonLocalization.Current.Culture = new CultureInfo("de");

// Switch to Chinese
RibbonLocalization.Current.Culture = new CultureInfo("zh");

// Switch back to English
RibbonLocalization.Current.Culture = new CultureInfo("en");
```

### Language Selector in Ribbon

The Showcase app demonstrates a language selector:

```xml
<!-- In your XAML -->
<fluent:ComboBox Header="Language"
                 DisplayMemberPath="DisplayName"
                 IsEditable="False"
                 ItemsSource="{Binding Localizations}"
                 SelectedItem="{Binding Path=Localization,
                                Source={x:Static fluent:RibbonLocalization.Current}}" />
```

```csharp
// In your ViewModel or code-behind
public List<RibbonLocalizationBase> Localizations { get; } = GetLocalizations();

private static List<RibbonLocalizationBase> GetLocalizations()
{
    return RibbonLocalization.Current.LocalizationMap.Values
        .Select(x => (RibbonLocalizationBase)Activator.CreateInstance(x)!)
        .OrderBy(x => x.DisplayName)
        .ToList();
}
```

### Binding Localized Strings in XAML

Fluent.Ribbon XAML templates bind to localization using this pattern:

```xml
<!-- Standard binding pattern used throughout Fluent.Ribbon -->
<Setter Property="Header"
        Value="{Binding Source={x:Static fluent:RibbonLocalization.Current},
                        Path=Localization.BackstageButtonText,
                        Mode=OneWay}" />
```

You can use the same pattern for custom controls:

```xml
<!-- Bind your own controls to localized strings -->
<TextBlock Text="{Binding Source={x:Static fluent:RibbonLocalization.Current},
                          Path=Localization.MoreColors,
                          Mode=OneWay}" />
```

---

## Fallback Behavior

Languages can fall back to English for untranslated strings using `FallbackLocalization`:

```csharp
// Example from Portuguese_Brazil.cs - some strings fall back to English
public override string CustomizeStatusBar => FallbackLocalization.CustomizeStatusBar;
public override string DisplayOptionsButtonScreenTipText => FallbackLocalization.DisplayOptionsButtonScreenTipText;
public override string ExpandRibbon => FallbackLocalization.ExpandRibbon;
```

This pattern is useful when:
- A translation is not yet available
- The English term is commonly used (technical terms)
- The property is accessibility-related (like UIDs)

---

## How Controls Use Localization

### Code-Behind Binding (Ribbon.cs)

Context menu items are bound in code:

```csharp
// From Ribbon.cs - context menu items bind to localization
RibbonControl.Bind(
    RibbonLocalization.Current.Localization,           // Source
    AddToQuickAccessMenuItem,                          // Target control
    nameof(RibbonLocalizationBase.RibbonContextMenuAddItem),  // Property name
    HeaderedItemsControl.HeaderProperty,              // Target property
    BindingMode.OneWay
);
```

### XAML Style Binding (Backstage.xaml)

Styles bind default values to localization:

```xml
<!-- From Backstage.xaml -->
<Style TargetType="{x:Type fluent:Backstage}">
    <Setter Property="Header"
            Value="{Binding Source={x:Static fluent:RibbonLocalization.Current},
                            Path=Localization.BackstageButtonText,
                            Mode=OneWay}" />
    <Setter Property="fluent:KeyTip.Keys"
            Value="{Binding Source={x:Static fluent:RibbonLocalization.Current},
                            Path=Localization.BackstageButtonKeyTip,
                            Mode=OneWay}" />
</Style>
```

### Menu Items (QuickAccessToolbar.xaml)

QAT menu items:

```xml
<!-- From QuickAccessToolbar.xaml -->
<fluent:MenuItem x:Name="PART_ShowBelow"
                 Header="{Binding Source={x:Static fluent:RibbonLocalization.Current},
                                  Path=Localization.QuickAccessToolBarMenuShowBelow,
                                  Mode=OneWay}"
                 CanAddToQuickAccessToolBar="False" />
```

---

## DO NOT DO

### Do Not Modify Built-in Language Files Directly

The language files are part of the Fluent.Ribbon NuGet package. Changes will be overwritten on package updates.

```csharp
// WRONG - modifying built-in class (will be lost on NuGet update)
// Edit: Fluent.Ribbon\Localization\Languages\English.cs

// CORRECT - create your own class
[RibbonLocalization("Custom English", "en-custom")]
public class CustomEnglish : RibbonLocalizationBase { ... }
```

### Do Not Assume All Strings Are Translated

Some languages have incomplete translations. Always test your supported languages.

```csharp
// Portuguese_Brazil uses fallback for several strings
public override string CustomizeStatusBar => FallbackLocalization.CustomizeStatusBar;
// This will show "Customize Status Bar" (English) in Brazilian Portuguese UI
```

### Do Not Forget Abstract Properties

When creating custom localizations, ALL abstract properties must be implemented:

```csharp
// WRONG - compilation error, missing abstract members
public class MyLocalization : RibbonLocalizationBase
{
    public override string BackstageButtonText => "Custom";
    // Error: does not implement all abstract members
}

// CORRECT - implement all 31 abstract properties
public class MyLocalization : RibbonLocalizationBase
{
    public override string Automatic => "Automatic";
    public override string BackstageBackButtonUid => "Close";
    public override string BackstageButtonKeyTip => "F";
    public override string BackstageButtonText => "Custom";
    // ... all 31 properties
}
```

### Do Not Set Culture Before LocalizationMap Registration

Register custom localizations before setting the culture:

```csharp
// WRONG - culture set before registration
RibbonLocalization.Current.Culture = new CultureInfo("eo");
RibbonLocalization.Current.LocalizationMap["eo"] = typeof(Esperanto);
// Result: Falls back to English because "eo" wasn't registered yet

// CORRECT - register first
RibbonLocalization.Current.LocalizationMap["eo"] = typeof(Esperanto);
RibbonLocalization.Current.Culture = new CultureInfo("eo");
// Result: Uses Esperanto localization
```

### Do Not Hardcode Strings in Custom Styles

Use bindings to maintain localization support:

```xml
<!-- WRONG - hardcoded string breaks localization -->
<Setter Property="Header" Value="File" />

<!-- CORRECT - binds to localization system -->
<Setter Property="Header"
        Value="{Binding Source={x:Static fluent:RibbonLocalization.Current},
                        Path=Localization.BackstageButtonText,
                        Mode=OneWay}" />
```

---

## Common Patterns

### Pattern: Application-Wide Language Setting

Store language preference and apply on startup:

```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Load saved preference
        string savedCulture = Settings.Default.UICulture;

        if (!string.IsNullOrEmpty(savedCulture))
        {
            try
            {
                RibbonLocalization.Current.Culture = new CultureInfo(savedCulture);
            }
            catch (CultureNotFoundException)
            {
                // Fall back to system culture
            }
        }

        base.OnStartup(e);
    }
}

// Save when user changes language
void OnLanguageChanged(CultureInfo newCulture)
{
    RibbonLocalization.Current.Culture = newCulture;
    Settings.Default.UICulture = newCulture.Name;
    Settings.Default.Save();
}
```

### Pattern: Match Thread Culture

Synchronize ribbon localization with application thread culture:

```csharp
// Set both thread and ribbon culture together
public void SetApplicationCulture(CultureInfo culture)
{
    Thread.CurrentThread.CurrentUICulture = culture;
    Thread.CurrentThread.CurrentCulture = culture;
    RibbonLocalization.Current.Culture = culture;
}
```

### Pattern: RTL Language Support

Some languages (Arabic, Hebrew, Persian) are right-to-left:

```csharp
void OnLanguageChanged(CultureInfo culture)
{
    RibbonLocalization.Current.Culture = culture;

    // Update FlowDirection for RTL languages
    if (culture.TextInfo.IsRightToLeft)
    {
        Application.Current.MainWindow.FlowDirection = FlowDirection.RightToLeft;
    }
    else
    {
        Application.Current.MainWindow.FlowDirection = FlowDirection.LeftToRight;
    }
}
```

---

## Source File Reference

| File | Purpose |
|------|---------|
| `Fluent.Ribbon\Localization\RibbonLocalization.cs` | Singleton manager class |
| `Fluent.Ribbon\Localization\RibbonLocalizationBase.cs` | Abstract base with all string properties |
| `Fluent.Ribbon\Localization\RibbonLocalizationAttribute.cs` | Attribute for culture metadata |
| `Fluent.Ribbon\Localization\Languages\*.cs` | 37 language implementations |
| `Fluent.Ribbon\Themes\Controls\Backstage.xaml` | Example of XAML localization binding |
| `Fluent.Ribbon\Themes\Controls\QuickAccessToolbar.xaml` | QAT menu localization |
| `Fluent.Ribbon\Controls\Ribbon.cs` | Code-based localization binding |

---

## Version Notes

- Fluent.Ribbon has included localization since early versions
- The `RibbonLocalizationBase` class uses abstract properties (not virtual) requiring full implementation
- RTL languages (Arabic, Hebrew, Persian) are supported with appropriate text alignment
- Some languages use `FallbackLocalization` for untranslated strings - this is intentional design
