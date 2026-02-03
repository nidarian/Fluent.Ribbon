---
title: Fluent.Ribbon State Persistence Reference
description: Complete reference for saving and restoring ribbon UI state
tags: [state, persistence, save, restore, qat]
see_also:
  - ../controls/fluent-ribbon-qat-reference-v2.md
  - fluent-ribbon-simplified-ribbon-reference.md
---

# Fluent.Ribbon State Persistence Reference

**Complete reference for saving and restoring ribbon UI state.**

---

## Overview

Fluent.Ribbon provides built-in state persistence that automatically saves and restores:

- **IsMinimized** - Whether the ribbon is collapsed
- **ShowQuickAccessToolBarAboveRibbon** - QAT position (above/below)
- **IsSimplified** - Classic vs Simplified ribbon mode

State is stored in **IsolatedStorage** using a unique filename per window/ribbon combination.

```
[App Starts]
    |
    v
[Ribbon Loads] ---> [Load State from IsolatedStorage]
    |                        |
    |                        v
    |               [Apply IsMinimized]
    |               [Apply QAT Position]
    |               [Apply IsSimplified]
    |
    v
[User Interacts]
    |
    +--> [Toggle Minimize] ---> [SaveTemporary to MemoryStream]
    +--> [Move QAT] ---------> [SaveTemporary to MemoryStream]
    +--> [Toggle Simplified] -> [SaveTemporary to MemoryStream]
    |
    v
[Window Closes/Unloads]
    |
    v
[Save State to IsolatedStorage]
```

---

## Quick Start

### Automatic State Management (Default)

State persistence is enabled by default. No configuration needed:

```xml
<fluent:Ribbon x:Name="ribbon" />
<!-- AutomaticStateManagement="True" is the default -->
```

### Disable State Management

```xml
<fluent:Ribbon AutomaticStateManagement="False" />
```

### Reset Saved State

```csharp
// Clear all saved state and restart
ribbon.AutomaticStateManagement = false;
ribbon.RibbonStateStorage.Reset();

// Restart app to see default state
System.Windows.Forms.Application.Restart();
Application.Current.Shutdown();
```

---

## Core Classes

### IRibbonStateStorage Interface

```csharp
public interface IRibbonStateStorage : IDisposable
{
    bool IsLoading { get; }      // True during Load()
    bool IsLoaded { get; }       // True after Load() completes

    void SaveTemporary();        // Save to MemoryStream
    void Save();                 // Save to IsolatedStorage
    void LoadTemporary();        // Load from MemoryStream
    void Load();                 // Load from IsolatedStorage
    void Reset();                // Delete all saved state files
}
```

### RibbonStateStorage Class

Default implementation that stores state in IsolatedStorage.

| Member | Type | Description |
|--------|------|-------------|
| `IsLoading` | `bool` | True while loading state |
| `IsLoaded` | `bool` | True after state loaded |
| `IsolatedStorageFileName` | `string` | Generated filename (protected) |
| `Save()` | `void` | Persist to IsolatedStorage |
| `Load()` | `void` | Load from IsolatedStorage |
| `SaveTemporary()` | `void` | Save to MemoryStream (in-session) |
| `LoadTemporary()` | `void` | Load from MemoryStream |
| `Reset()` | `void` | Delete all state files |

---

## State Data Format

State is stored as a comma-separated string:

```
IsMinimized,ShowQuickAccessToolBarAboveRibbon,IsSimplified

Example: "False,True,False"
         ^      ^     ^
         |      |     +-- Classic ribbon mode
         |      +-------- QAT above ribbon
         +--------------- Ribbon expanded
```

### Serialization Code (Internal)

```csharp
protected virtual StringBuilder CreateStateData()
{
    var builder = new StringBuilder();
    builder.Append(this.ribbon.IsMinimized.ToString(CultureInfo.InvariantCulture));
    builder.Append(',');
    builder.Append(this.ribbon.ShowQuickAccessToolBarAboveRibbon.ToString(CultureInfo.InvariantCulture));
    builder.Append(',');
    builder.Append(this.ribbon.IsSimplified.ToString(CultureInfo.InvariantCulture));
    return builder;
}
```

### Deserialization Logic (Internal)

```csharp
protected virtual void LoadState(string data)
{
    var ribbonProperties = data.Split(',');

    // Property 0: IsMinimized
    if (ribbonProperties.Length > 0
        && this.ribbon.CanMinimize
        && bool.TryParse(ribbonProperties[0], out var isMinimized))
    {
        this.ribbon.IsMinimized = isMinimized;
    }

    // Property 1: ShowQuickAccessToolBarAboveRibbon
    if (ribbonProperties.Length > 1
        && bool.TryParse(ribbonProperties[1], out var showAbove))
    {
        this.ribbon.ShowQuickAccessToolBarAboveRibbon = showAbove;
    }

    // Property 2: IsSimplified
    if (ribbonProperties.Length > 2
        && this.ribbon.CanUseSimplified
        && bool.TryParse(ribbonProperties[2], out var isSimplified))
    {
        this.ribbon.IsSimplified = isSimplified;
    }
}
```

---

## Storage Location

### IsolatedStorage File Naming

The filename is generated using MD5 hash:

```
Fluent.Ribbon.State.{MD5HashHex}

Where MD5HashHex = MD5(stringForHash) as hex int
And stringForHash = "." + WindowTypeName + "." + WindowName + "." + RibbonName
```

### Storage Location (Windows)

```
%LocalAppData%\IsolatedStorage\{domain-identity}\{assembly-identity}\Files\

Example:
C:\Users\{User}\AppData\Local\IsolatedStorage\{guid}\{guid}\Files\Fluent.Ribbon.State.DEADBEEF
```

### Finding Storage Files

The actual path varies by .NET version and application identity. Use `IsolatedStorageFile` APIs:

```csharp
using System.IO.IsolatedStorage;

var storage = IsolatedStorageFile.GetUserStoreForDomain();
var files = storage.GetFileNames("*Fluent.Ribbon.State*");
foreach (var file in files)
{
    Console.WriteLine($"Found: {file}");
}
```

---

## Ribbon Properties

### AutomaticStateManagement

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AutomaticStateManagement` | `bool` | `True` | Enable/disable automatic persistence |

```xml
<!-- Enable (default) -->
<fluent:Ribbon AutomaticStateManagement="True" />

<!-- Disable -->
<fluent:Ribbon AutomaticStateManagement="False" />
```

### Properties Affected by State Persistence

| Property | Persisted | Condition |
|----------|-----------|-----------|
| `IsMinimized` | Yes | Only if `CanMinimize="True"` |
| `ShowQuickAccessToolBarAboveRibbon` | Yes | Always |
| `IsSimplified` | Yes | Only if `CanUseSimplified="True"` |

### Properties NOT Persisted

The following are NOT automatically persisted:

- Selected tab
- QAT items (see Custom Implementation section)
- Theme/accent color
- Window size/position
- Custom user preferences

---

## Lifecycle

### When State Loads

```
Window.Loaded
    |
    v
Ribbon.OnLoaded()
    |
    v
Ribbon.LoadInitialState()
    |
    +-- Check: RibbonStateStorage.IsLoaded?
    |   |
    |   +-- Yes: Return (already loaded)
    |   +-- No: Continue
    |
    v
RibbonStateStorage.Load()
    |
    +-- Check: AutomaticStateManagement?
    |   |
    |   +-- False: Set IsLoaded=True, Return
    |   +-- True: Continue
    |
    +-- Check: Design mode?
    |   |
    |   +-- Yes: Set IsLoaded=True, Return
    |   +-- No: Continue
    |
    v
Open IsolatedStorageFileStream
    |
    v
Read state string, parse, apply
    |
    v
Set IsLoaded=True
```

### When State Saves

**Save triggers:**

1. **Window Close/Unload** - `Ribbon.OnUnloaded()` calls `Save()`
2. **Window Detach** - `DetachFromWindow()` calls `Save()`
3. **QAT Position Change** - `OnShowQuickAccessToolBarAboveRibbonChanged()` calls `SaveTemporary()`

```
[Save Trigger]
    |
    v
RibbonStateStorage.Save()
    |
    +-- Check: AutomaticStateManagement?
    |   |
    |   +-- False: Return (debug log)
    |   +-- True: Continue
    |
    +-- Check: IsLoaded?
    |   |
    |   +-- False: Return (debug log)
    |   +-- True: Continue
    |
    v
Open IsolatedStorageFileStream
    |
    v
Write state string
```

### Temporary vs Persistent Storage

| Method | Storage | Purpose |
|--------|---------|---------|
| `SaveTemporary()` | MemoryStream | Style changes, immediate state |
| `Save()` | IsolatedStorage | Permanent persistence |
| `LoadTemporary()` | MemoryStream | Restore from style change |
| `Load()` | IsolatedStorage | App startup |

---

## Custom State Storage

### Override CreateRibbonStateStorage

Create a derived Ribbon class:

```csharp
public class MyRibbon : Fluent.Ribbon
{
    protected override IRibbonStateStorage CreateRibbonStateStorage()
    {
        return new MyCustomStateStorage(this);
    }
}
```

### Custom Storage Implementation

```csharp
public class MyCustomStateStorage : RibbonStateStorage
{
    public MyCustomStateStorage(Ribbon ribbon) : base(ribbon) { }

    // Override to add custom data
    protected override StringBuilder CreateStateData()
    {
        var builder = base.CreateStateData();

        // Add custom properties
        builder.Append(',');
        builder.Append(MyCustomProperty);

        return builder;
    }

    // Override to load custom data
    protected override void LoadState(string data)
    {
        base.LoadState(data);

        var parts = data.Split(',');
        if (parts.Length > 3)
        {
            MyCustomProperty = parts[3];
        }
    }
}
```

### Alternative Storage Backend

Replace IsolatedStorage entirely:

```csharp
public class ApplicationSettingsStateStorage : IRibbonStateStorage
{
    private readonly Ribbon ribbon;
    private MemoryStream memoryStream = new();

    public bool IsLoading { get; private set; }
    public bool IsLoaded { get; private set; }

    public ApplicationSettingsStateStorage(Ribbon ribbon)
    {
        this.ribbon = ribbon;
    }

    public void Save()
    {
        if (!IsLoaded) return;

        Properties.Settings.Default.RibbonIsMinimized = ribbon.IsMinimized;
        Properties.Settings.Default.RibbonQatAbove = ribbon.ShowQuickAccessToolBarAboveRibbon;
        Properties.Settings.Default.RibbonIsSimplified = ribbon.IsSimplified;
        Properties.Settings.Default.Save();
    }

    public void Load()
    {
        IsLoading = true;
        try
        {
            if (ribbon.CanMinimize)
                ribbon.IsMinimized = Properties.Settings.Default.RibbonIsMinimized;

            ribbon.ShowQuickAccessToolBarAboveRibbon = Properties.Settings.Default.RibbonQatAbove;

            if (ribbon.CanUseSimplified)
                ribbon.IsSimplified = Properties.Settings.Default.RibbonIsSimplified;
        }
        finally
        {
            IsLoading = false;
            IsLoaded = true;
        }
    }

    public void SaveTemporary() { /* Save to memoryStream */ }
    public void LoadTemporary() { /* Load from memoryStream */ }

    public void Reset()
    {
        Properties.Settings.Default.Reset();
    }

    public void Dispose()
    {
        memoryStream?.Dispose();
    }
}
```

---

## Saving QAT Items

The default implementation does NOT save QAT items. Implement manually:

### Save QAT to Settings

```csharp
public void SaveQatItems()
{
    var qatItems = ribbon.QuickAccessToolBar?.Items
        .OfType<FrameworkElement>()
        .Select(e => e.Name)
        .Where(n => !string.IsNullOrEmpty(n))
        .ToList();

    if (qatItems != null)
    {
        Properties.Settings.Default.QatItems = string.Join(";", qatItems);
        Properties.Settings.Default.Save();
    }
}

public void LoadQatItems()
{
    var itemIds = Properties.Settings.Default.QatItems?.Split(';') ?? Array.Empty<string>();

    foreach (var id in itemIds)
    {
        if (string.IsNullOrEmpty(id)) continue;

        var control = FindName(id) as UIElement;
        if (control != null && !ribbon.IsInQuickAccessToolBar(control))
        {
            ribbon.AddToQuickAccessToolBar(control);
        }
    }
}
```

### Wire Up Save/Load

```csharp
public MainWindow()
{
    InitializeComponent();
    Loaded += (s, e) => LoadQatItems();
    Closing += (s, e) => SaveQatItems();
}
```

---

## Saving Theme Preferences

Theme is separate from Fluent.Ribbon state. Use ControlzEx ThemeManager:

### Save Theme

```csharp
public void SaveTheme()
{
    var theme = ControlzEx.Theming.ThemeManager.Current.DetectTheme(this);
    if (theme != null)
    {
        Properties.Settings.Default.ThemeName = theme.Name;
        Properties.Settings.Default.Save();
    }
}
```

### Load Theme

```csharp
public void LoadTheme()
{
    var themeName = Properties.Settings.Default.ThemeName;
    if (!string.IsNullOrEmpty(themeName))
    {
        var theme = ControlzEx.Theming.ThemeManager.Current.GetTheme(themeName);
        if (theme != null)
        {
            ControlzEx.Theming.ThemeManager.Current.ChangeTheme(this, theme);
        }
    }
}
```

---

## Application Settings Integration

### Settings.settings File

Add these settings to your project:

| Name | Type | Scope | Default |
|------|------|-------|---------|
| `RibbonIsMinimized` | `bool` | User | `False` |
| `RibbonQatAbove` | `bool` | User | `True` |
| `RibbonIsSimplified` | `bool` | User | `False` |
| `QatItems` | `string` | User | `""` |
| `ThemeName` | `string` | User | `"Light.Blue"` |

### Complete Settings-Based Solution

```csharp
public partial class MainWindow : RibbonWindow
{
    public MainWindow()
    {
        InitializeComponent();

        // Disable built-in state management
        ribbon.AutomaticStateManagement = false;

        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        LoadAllState();
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
        SaveAllState();
    }

    private void LoadAllState()
    {
        // Load ribbon state
        if (ribbon.CanMinimize)
            ribbon.IsMinimized = Properties.Settings.Default.RibbonIsMinimized;

        ribbon.ShowQuickAccessToolBarAboveRibbon = Properties.Settings.Default.RibbonQatAbove;

        if (ribbon.CanUseSimplified)
            ribbon.IsSimplified = Properties.Settings.Default.RibbonIsSimplified;

        // Load QAT items
        LoadQatItems();

        // Load theme
        LoadTheme();
    }

    private void SaveAllState()
    {
        Properties.Settings.Default.RibbonIsMinimized = ribbon.IsMinimized;
        Properties.Settings.Default.RibbonQatAbove = ribbon.ShowQuickAccessToolBarAboveRibbon;
        Properties.Settings.Default.RibbonIsSimplified = ribbon.IsSimplified;

        SaveQatItems();
        SaveTheme();

        Properties.Settings.Default.Save();
    }
}
```

---

## State Diagram

```
                    +------------------------+
                    |    APP STARTS          |
                    +------------------------+
                               |
                               v
                    +------------------------+
                    |  Window/Ribbon Loads   |
                    +------------------------+
                               |
              AutomaticStateManagement?
                      /            \
                   True           False
                    |               |
                    v               v
         +------------------+   +------------------+
         | Load from        |   | Use XAML         |
         | IsolatedStorage  |   | defaults only    |
         +------------------+   +------------------+
                    |               |
                    +-------+-------+
                            |
                            v
                    +------------------------+
                    |   USER INTERACTS       |
                    +------------------------+
                            |
         +------------------+------------------+
         |                  |                  |
         v                  v                  v
    [Minimize]        [Move QAT]        [Simplify]
         |                  |                  |
         v                  v                  v
    +-------------------------------------------+
    |       SaveTemporary() called              |
    |       (in-memory for style changes)       |
    +-------------------------------------------+
                            |
                            v
                    +------------------------+
                    |   WINDOW CLOSES        |
                    +------------------------+
                               |
              AutomaticStateManagement?
                      /            \
                   True           False
                    |               |
                    v               v
         +------------------+   +------------------+
         | Save to          |   | No automatic     |
         | IsolatedStorage  |   | save             |
         +------------------+   +------------------+
```

---

## DO NOT DO

### DON'T: Assume State Exists

```csharp
// WRONG - State may not exist on first run
var state = ribbon.RibbonStateStorage;
// Assuming state.IsLoaded is true immediately

// RIGHT - Wait for Loaded event
Loaded += (s, e) =>
{
    // Now state is loaded (if AutomaticStateManagement=True)
};
```

### DON'T: Save State During Load

```csharp
// WRONG - Saving during load creates race condition
ribbon.IsMinimized = true; // Triggers save internally
ribbon.RibbonStateStorage.Load(); // Then tries to load

// RIGHT - Disable, configure, then enable
ribbon.AutomaticStateManagement = false;
ribbon.IsMinimized = true;
ribbon.AutomaticStateManagement = true;
```

### DON'T: Modify RibbonStateStorage Directly During Load

```csharp
// WRONG - CoerceAutomaticStateManagement returns false during loading
protected virtual void LoadState(string data)
{
    // AutomaticStateManagement is coerced to false here
    // So any changes here won't trigger save
}
```

### DON'T: Delete IsolatedStorage Files Manually

```csharp
// WRONG - Path varies by .NET version and app identity
File.Delete(@"C:\Users\...\IsolatedStorage\...\Fluent.Ribbon.State.ABC");

// RIGHT - Use the Reset() method
ribbon.RibbonStateStorage.Reset();
```

### DON'T: Assume QAT Items Are Persisted

```csharp
// WRONG - QAT items are NOT saved by default
// User adds items to QAT, closes app, opens app
// Expecting: Items still there
// Reality: QAT is empty

// RIGHT - Implement custom QAT persistence (see section above)
```

### DON'T: Mix AutomaticStateManagement with Manual Saves

```csharp
// CONFUSING - Both systems fight each other
ribbon.AutomaticStateManagement = true;
// ... later ...
ribbon.RibbonStateStorage.Save(); // Redundant, automatic does this
MyCustomSave(); // Conflicts with automatic

// RIGHT - Choose one approach
// Either: AutomaticStateManagement = true (let it handle everything)
// Or: AutomaticStateManagement = false + your own save/load
```

### DON'T: Forget Design Mode Check

```csharp
// WRONG - Will fail in Visual Studio designer
public override void OnApplyTemplate()
{
    base.OnApplyTemplate();
    ribbon.RibbonStateStorage.Load(); // Crashes in design mode
}

// RIGHT - Check design mode
if (!DesignerProperties.GetIsInDesignMode(this))
{
    ribbon.RibbonStateStorage.Load();
}
```

---

## Troubleshooting

### State Not Saving

1. Check `AutomaticStateManagement="True"`
2. Check `RibbonStateStorage.IsLoaded == true` (must load before save)
3. Check for exceptions in Output window (state save errors are caught and logged)
4. Verify window closes normally (force close may skip save)

### State Not Loading

1. Verify IsolatedStorage files exist (use `GetFileNames()`)
2. Check `CanMinimize` / `CanUseSimplified` properties
3. Check for design mode issues
4. Look for Debug.WriteLine output in Visual Studio

### State Resets Unexpectedly

1. File naming changed in v8.0 (MD5 hash instead of GetHashCode)
2. Different Window/Ribbon names generate different storage files
3. .NET Core vs .NET Framework may use different storage locations

### Enable Debug Logging

The default implementation uses `Debug.WriteLine()` and `Trace.WriteLine()`:

```csharp
// In Output window (Debug configuration):
// "State not saved to isolated storage. Because automatic state management is disabled."
// "State not saved to isolated storage. Because state was not loaded before."
// "State not loaded from isolated storage. Because we are in design mode."
// "Error while trying to save Ribbon state. Error: ..."
```

---

## Source Files

| File | Purpose |
|------|---------|
| `Fluent.Ribbon\IRibbonStateStorage.cs` | Interface definition |
| `Fluent.Ribbon\Data\RibbonStateStorage.cs` | Default implementation |
| `Fluent.Ribbon\Controls\Ribbon.cs` | Integration points |
| `Fluent.Ribbon.Showcase\TestContent.xaml.cs` | Reset example (HandleResetSavedState_OnClick) |

---

## Summary

| Task | Solution |
|------|----------|
| Enable auto state | `AutomaticStateManagement="True"` (default) |
| Disable auto state | `AutomaticStateManagement="False"` |
| Reset saved state | `ribbon.RibbonStateStorage.Reset()` |
| Custom storage | Override `CreateRibbonStateStorage()` in derived Ribbon |
| Save QAT items | Implement manually (not built-in) |
| Save theme | Use ThemeManager separately |
| Use app settings | Disable auto, use Properties.Settings |
| Debug state issues | Check Output window for Debug/Trace messages |

---

## Version History

| Version | Changes |
|---------|---------|
| 8.0 | IsolatedStorage filename changed to MD5 hash (stable across .NET versions) |
| Pre-8.0 | Used `GetHashCode` for filename (unstable on .NET Core) |
