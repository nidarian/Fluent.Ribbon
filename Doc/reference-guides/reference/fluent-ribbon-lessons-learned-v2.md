---
title: Fluent.Ribbon Lessons Learned
description: Anti-patterns, mistakes to avoid, and lessons from real-world usage
tags: [lessons, anti-patterns, mistakes, reference]
see_also:
  - fluent-ribbon-troubleshooting.md
  - ../getting-started/fluent-ribbon-common-tasks-v2.md
---

# Fluent.Ribbon Theming - Lessons Learned (v2)

**Date:** 2026-01-10 (Original), 2026-01-23 (v2 verified)
**Context:** Attempted to contribute documentation guides to Fluent.Ribbon, received criticism from maintainer
**Purpose:** Preserve this learning for future contributions

*v2 Changes: Verified content still 100% relevant, added verification date*

---

## FIRST: Run the Showcase App

**THE MOST VALUABLE RESOURCE - A working example of everything.**

```
Fluent.Ribbon.Showcase (in the Fluent.Ribbon repository)
https://github.com/fluentribbon/Fluent.Ribbon
```

### How to Build and Run

```bash
# Clone or navigate to your local Fluent.Ribbon repository
# https://github.com/fluentribbon/Fluent.Ribbon
dotnet build Fluent.Ribbon.Showcase
dotnet run --project Fluent.Ribbon.Showcase
```

### What It Demonstrates
- All controls in action (buttons, dropdowns, galleries, backstage, etc.)
- Theme switching (light/dark/custom colors)
- Proper theming patterns with ThemeManager
- Contextual tabs
- Keyboard navigation (KeyTips)
- Simplified ribbon mode
- Color customization

### Key Files in Showcase to Study

| File | What It Shows |
|------|---------------|
| `App.xaml.cs` | ThemeManager setup, theme sync with Windows |
| `Helpers/ThemeHelper.cs` | RuntimeThemeGenerator - create custom themes |
| `ViewModels/ColorViewModel.cs` | Proper brush resource override pattern |
| `TestContent.xaml` | Every control with working examples |
| `RibbonWindowColorized.xaml` | Custom colored window |

**Before guessing how something works, BUILD AND RUN the Showcase to see it working.**

---

## What Happened

We created three documentation guides for Fluent.Ribbon theming:
1. Custom Theming Guide (#1259)
2. Active Tab Styling Guide (#1260)
3. Dark Mode Button Text Guide (#1261)

Maintainer (Batzen) responded:
> "It's extremely sad that you didn't ask before coming up with so much wrong 'information', strange/wrong architecture 'analysis' and a dangerously fragile solution."

---

## Where We Went Wrong

### 1. We Didn't Read the Existing Architecture

Fluent.Ribbon has a proper theming system built on ControlzEx:

```
ControlzEx.Theming.ThemeManager
    +-- RuntimeThemeGenerator
         +-- LibraryThemeProvider
              +-- RibbonLibraryThemeProvider (Fluent.Ribbon's extension)
```

**Key files we should have read first:**
- `Fluent.Ribbon/Theming/RibbonLibraryThemeProvider.cs`
- `Fluent.Ribbon/Themes/Themes/Theme.Template.xaml`
- `Fluent.Ribbon.Showcase/Helpers/ThemeHelper.cs`
- `Fluent.Ribbon.Showcase/ViewModels/ColorViewModel.cs`

### 2. We Reverse-Engineered Instead of Learning

We tried 15+ approaches that failed, then found one that worked (visual tree walking). We documented the failures without understanding WHY they failed.

**What we missed:** The failures weren't bugs - they were us fighting the framework's design.

### 3. The "Fragile Solution" Problem

Our visual tree walking approach:
```csharp
// What we did - FRAGILE
foreach (var child in GetVisualChildren(ribbon))
{
    if (child.Name == "PART_xxx")  // Implementation detail!
        child.Background = myBrush;
}
```

Why it's fragile:
| Issue | Risk |
|-------|------|
| `PART_xxx` names are implementation details | Change between versions |
| Template structure can be reorganized | Code breaks silently |
| Direct property setting | Bypasses resource system |
| Not integrated with ThemeManager | Doesn't respond to theme changes |

### 4. The Proper Architecture We Missed

**Theme.Template.xaml** - How resources are structured:
```xml
<!-- Colors are placeholders filled at runtime -->
<Color x:Key="Fluent.Ribbon.Colors.AccentBase">{{Fluent.Ribbon.Colors.AccentBase}}</Color>

<!-- Brushes reference colors via StaticResource (one-time) -->
<!-- Brushes are FROZEN - immutable after creation -->
<SolidColorBrush x:Key="Fluent.Ribbon.Brushes.AccentBase"
                 Color="{StaticResource Fluent.Ribbon.Colors.AccentBase}"
                 options:Freeze="True" />
```

**RibbonLibraryThemeProvider.cs** - How colors are provided:
```csharp
public override void FillColorSchemeValues(Dictionary<string, string> values, RuntimeThemeColorValues colorValues)
{
    values.Add("Fluent.Ribbon.Colors.AccentBase", colorValues.AccentColor.ToString());
    values.Add("Fluent.Ribbon.Colors.Accent80", colorValues.AccentColor80.ToString());
    // Derives full color palette from accent color
}
```

**Controls use DynamicResource** to brushes:
```xml
<Setter Property="Background" Value="{DynamicResource Fluent.Ribbon.Brushes.RibbonWindow.Background}" />
```

When theme changes, the entire ResourceDictionary is swapped - controls get new brushes through DynamicResource.

---

## The Correct Approaches

### Option 1: Use Built-in Themes
```csharp
// Switch to a predefined theme
ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Blue");

// Or sync with Windows theme
ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
ThemeManager.Current.SyncTheme();
```

### Option 2: Generate Custom Theme at Runtime
```csharp
// Create theme with custom accent color
var theme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme("Dark", myAccentColor, false);
ThemeManager.Current.ChangeTheme(Application.Current, theme);
```

### Option 3: Override Specific Brush Resources
```csharp
// Replace a brush entirely (must be new brush, can't modify frozen one)
var brush = new SolidColorBrush(myColor);
brush.Freeze();
Application.Current.Resources["Fluent.Ribbon.Brushes.AccentBase"] = brush;
```

### Option 4: Create Custom LibraryThemeProvider
```csharp
public class MyThemeProvider : RibbonLibraryThemeProvider
{
    public override void FillColorSchemeValues(Dictionary<string, string> values, RuntimeThemeColorValues colorValues)
    {
        base.FillColorSchemeValues(values, colorValues);
        // Add custom color mappings
    }
}
```

---

## What We Got Right

The **Dark Mode Button Text Guide** (#1261) was actually correct:
```xml
<fluent:Button Foreground="{DynamicResource TextPrimaryBrush}" ... />
```

This uses the resource system properly - DynamicResource binding to theme-aware brushes.

---

## Lessons for Future Contributions

### Before Contributing Documentation

1. **Read the existing code architecture first**
   - Find the main classes/patterns
   - Look at the Showcase/Demo app for proper usage
   - Check if there are existing docs explaining the design

2. **Ask before submitting**
   - Open an issue asking "Would documentation on X be helpful?"
   - Describe your understanding, ask if it's correct
   - Wait for maintainer feedback

3. **Understand WHY things don't work**
   - Failed approaches aren't bugs
   - They often indicate misunderstanding of design
   - The "working hack" may be fighting the framework

4. **Check the proper extension points**
   - Libraries have intended customization APIs
   - Visual tree walking is almost never the right answer
   - Look for `virtual` methods, `Provider` classes, `Options` objects

### AI-Generated Content Warning

Batzen asked: "Was your solution AI generated?"

The stigma is real because:
- AI can generate confident-sounding but incorrect analysis
- AI tends to find "working hacks" rather than proper patterns
- AI may not understand framework design philosophy
- Documentation requires domain expertise, not just code generation

**Mitigation:**
- Always verify AI suggestions against actual source code
- Have AI explain the architecture it found, check if it makes sense
- Test solutions, but also understand WHY they work
- Be honest about AI assistance when asked

---

## Key Files Reference

| File | Purpose |
|------|---------|
| `Fluent.Ribbon/Theming/RibbonLibraryThemeProvider.cs` | Theme color provider |
| `Fluent.Ribbon/Themes/Themes/Theme.Template.xaml` | Resource definitions |
| `Fluent.Ribbon.Showcase/Helpers/ThemeHelper.cs` | Proper theme creation |
| `Fluent.Ribbon.Showcase/ViewModels/ColorViewModel.cs` | Proper resource override |
| `Fluent.Ribbon.Showcase/App.xaml.cs` | ThemeManager setup |

---

## Summary

| What We Did | What We Should Have Done |
|-------------|-------------------------|
| Tried approaches until one worked | Read architecture first |
| Documented failed attempts as "bugs" | Understood design decisions |
| Visual tree walking | ThemeManager APIs |
| Posted without asking | Asked for guidance first |
| Submitted 3 issues at once | Started with one, got feedback |

---

## Archive Links

- Issue #1259: https://github.com/fluentribbon/Fluent.Ribbon/issues/1259
- Issue #1260: https://github.com/fluentribbon/Fluent.Ribbon/issues/1260
- Issue #1261: https://github.com/fluentribbon/Fluent.Ribbon/issues/1261

---

*This document exists to prevent repeating these mistakes in future open source contributions.*

---

## Pre-Flight Checklist (Before Accepting Any AI Solution)

**Ask these questions before implementing ANY solution AI suggests:**

### 1. Architecture Verification
- [ ] "Is this using the library's built-in APIs, or working around them?"
- [ ] "Show me where in the official code/docs this pattern is used"
- [ ] "Find an example in the Showcase/Demo app that does this"
- [ ] "What class in the library is designed for this customization?"

### 2. Sustainability Check
- [ ] "What happens when the library updates - will this break?"
- [ ] "Does this depend on internal element names or structure?"
- [ ] "Is this integrated with the library's systems (theming, resources) or bypassing them?"

### 3. Red Flag Detection
- [ ] Are we walking the visual tree? **(RED FLAG)**
- [ ] Are we referencing `PART_xxx` element names? **(RED FLAG)**
- [ ] Did we try 5+ approaches that "don't work"? **(RED FLAG - we're missing something)**
- [ ] Is AI saying "the library doesn't support this"? **(RED FLAG - it probably does)**

### 4. Proof of Correctness
- [ ] "Show me the official class/method for this use case"
- [ ] "Find documentation or code comments explaining this pattern"
- [ ] If AI can't point to official examples, **STOP and research more**

---

## Red Flags List

**Warning signs that AI is hacking instead of solving properly:**

| Red Flag | What It Means |
|----------|---------------|
| "Nothing else works, so we have to..." | We missed the proper API |
| Referencing `PART_xxx` element names | Using implementation details |
| Setting properties directly on child elements | Bypassing resource system |
| "The library doesn't support this" | We didn't find where it does |
| 15 failed attempts before one works | Fighting the framework |
| Visual tree walking | Almost never the right answer |
| "This is a workaround because..." | There's a proper way we missed |
| Code works but feels hacky | Trust that instinct |

**When you see these flags:** Stop. Ask AI to find the *intended* customization point. Every mature library has one.

---

## How to Catch AI Mistakes (When You Can't Evaluate Code)

Common situation: Developer can't read code well enough to know if AI is right.

**Solution: Make AI prove it's right.**

### Questions That Expose Bad Solutions

1. **"Show me an example in the Showcase/Demo app that does this"**
   - If the library has a demo app, the proper patterns are demonstrated there
   - If AI can't find an example, the approach is probably wrong

2. **"What class in the library is designed for this?"**
   - Look for: `Provider`, `Manager`, `Options`, `Settings`, `Theme` classes
   - These are customization entry points

3. **"Find the official documentation for this approach"**
   - No docs? Either undocumented (risky) or we're doing it wrong

4. **"What would break if the library updates?"**
   - If AI hesitates or lists many things, the solution is fragile

5. **"Is there a virtual method or event for this?"**
   - Libraries expose extension points intentionally
   - If we're not using one, we're probably hacking

### The Golden Rule

> If AI says "I had to work around the library," assume AI is wrong until proven otherwise.

---

## Instructions for Claude Code: Fix Our Theming Implementation

**Context:** A previous Claude session implemented Fluent.Ribbon theming using visual tree walking. This was wrong. The code needs to be rewritten using the proper APIs.

### What Exists (The Wrong Way)

Location: Example WPF application
Problem: Visual tree walking to set ribbon colors directly on elements

```csharp
// WRONG - What we did (fragile, version-dependent)
foreach (var child in GetVisualChildren(ribbon))
{
    if (child.Name == "PART_xxx")
        child.Background = myBrush;
}
```

### What Should Exist (The Right Way)

**Option A: Use RuntimeThemeGenerator (Recommended)**

```csharp
using ControlzEx.Theming;

// Generate a custom theme with your accent color
var accentColor = Color.FromRgb(0x00, 0x78, 0xD4); // Your brand color
var theme = RuntimeThemeGenerator.Current.GenerateRuntimeTheme("Dark", accentColor, false);
ThemeManager.Current.ChangeTheme(Application.Current, theme);
```

**Option B: Override Specific Brush Resources**

```csharp
// Replace specific brushes in Application.Resources
var brush = new SolidColorBrush(myColor);
brush.Freeze();
Application.Current.Resources["Fluent.Ribbon.Brushes.AccentBase"] = brush;
Application.Current.Resources["Fluent.Ribbon.Brushes.Ribbon.Background"] = anotherBrush;
```

**Option C: Sync with Windows Theme**

```csharp
// In App.xaml.cs OnStartup
ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
ThemeManager.Current.SyncTheme();
```

### Key Files to Read First

Before implementing, read these files in the Fluent.Ribbon source:

1. **`Fluent.Ribbon/Theming/RibbonLibraryThemeProvider.cs`**
   - Shows how theme colors are provided
   - `FillColorSchemeValues()` is the extension point

2. **`Fluent.Ribbon/Themes/Themes/Theme.Template.xaml`**
   - All available brush keys: `Fluent.Ribbon.Brushes.xxx`
   - Shows the Color -> Brush -> Control flow

3. **`Fluent.Ribbon.Showcase/Helpers/ThemeHelper.cs`**
   - Shows proper `RuntimeThemeGenerator` usage

4. **`Fluent.Ribbon.Showcase/ViewModels/ColorViewModel.cs`**
   - Shows proper brush resource override pattern

5. **`Fluent.Ribbon.Showcase/App.xaml.cs`**
   - Shows ThemeManager setup on startup

### Available Brush Keys

These are the brushes you can override in `Application.Current.Resources`:

```
Fluent.Ribbon.Brushes.AccentBase
Fluent.Ribbon.Brushes.Accent80
Fluent.Ribbon.Brushes.Accent60
Fluent.Ribbon.Brushes.Accent40
Fluent.Ribbon.Brushes.Accent20
Fluent.Ribbon.Brushes.Ribbon.Background
Fluent.Ribbon.Brushes.RibbonTabControl.Background
Fluent.Ribbon.Brushes.RibbonTabControl.Content.Background
Fluent.Ribbon.Brushes.RibbonTabItem.Active.Background
Fluent.Ribbon.Brushes.RibbonTabItem.MouseOver.Background
Fluent.Ribbon.Brushes.RibbonWindow.Background
Fluent.Ribbon.Brushes.RibbonWindow.TitleBackground
... (see Theme.Template.xaml for full list)
```

### Implementation Steps

1. **Read the source files listed above** - understand the architecture first
2. **Find where visual tree walking code exists** in your application
3. **Remove the visual tree walking approach entirely**
4. **Implement using ThemeManager or resource overrides**
5. **Test dark mode and light mode switching**
6. **Verify theme changes work without restart**

### Success Criteria

- [ ] No visual tree walking code remains
- [ ] No `PART_xxx` element name references
- [ ] Uses `ThemeManager` or `Application.Current.Resources` properly
- [ ] Theme switching works dynamically
- [ ] Code won't break on Fluent.Ribbon version updates

### Questions to Clarify

Before implementing, clarify:
1. What specific colors need to be customized?
2. Is runtime theme switching (dark/light toggle) needed?
3. Should the app sync with Windows theme automatically?
4. Are there brand colors that must be used?

---

## For Future Open Source Contributions

### The Right Process

```
1. Find something confusing in a library
         |
2. Search existing issues/docs first
         |
3. Open issue ASKING: "Is my understanding correct?"
         |
4. Wait for maintainer response
         |
5. If approved, THEN create documentation/PR
         |
6. One contribution at a time, get feedback
```

### The Wrong Process (What We Did)

```
1. Find something confusing
         |
2. Try many approaches until one works
         |
3. Document the hack as "the solution"
         |
4. Submit multiple issues at once
         |
5. Get criticized for wrong information
```

---

## Session Handoff Notes

**For the next session working on theming:**

- Any visual tree walking theming code needs to be rewritten
- Visual tree walking is WRONG - remove it
- Use ThemeManager APIs instead
- Read the "Instructions for Claude Code" section above
- The Fluent.Ribbon source: https://github.com/fluentribbon/Fluent.Ribbon
- Check `Fluent.Ribbon.Showcase` for proper usage examples
- Clarify specific color requirements before implementing
