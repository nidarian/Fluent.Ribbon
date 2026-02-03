---
title: Fluent.Ribbon Glossary
description: Definitions of common Fluent.Ribbon terms and concepts
tags: [glossary, reference, terminology]
---

# Fluent.Ribbon Glossary

Quick reference for Fluent.Ribbon terminology and concepts.

---

## A

### ApplicationMenu
The main application menu (File menu) that appears when clicking the application button. Contains recent documents, application commands, and footer buttons.

### AutomationPeer
WPF class that exposes UI elements to accessibility tools and screen readers. Every Fluent.Ribbon control has a corresponding peer in `Fluent.Automation.Peers`.

---

## B

### Backstage
Full-screen overlay that replaces the ribbon content, typically used for file operations (Open, Save, Print). Activated by clicking the application button or pressing Alt+F.

### BackstageTabControl
Container for backstage content, similar to TabControl but styled for backstage use.

---

## C

### ContextualTabGroup
A group of tabs that appear/disappear based on context (e.g., "Picture Tools" when an image is selected). Has a colored header above the tabs.

### ControlzEx
Companion library that provides ThemeManager and window chrome functionality. Namespace: `ControlzEx.Theming`.

---

## D

### Dialog Launcher
Small button in the corner of a RibbonGroupBox that opens a related dialog (e.g., Font dialog). Configured via `IsLauncherVisible` and `LauncherCommand`.

### DropDownButton
Button that opens a dropdown menu when clicked. Unlike SplitButton, the entire button opens the menu.

---

## G

### Gallery
Scrollable grid of selectable items, typically showing visual previews. Can be inline (`InRibbonGallery`) or in a dropdown.

### GalleryItem
Individual item within a Gallery. Can be grouped using `GalleryGroupFilter`.

---

## I

### InRibbonGallery
Gallery that displays directly in the ribbon (not in a dropdown). Shows a preview grid with scroll arrows and an expand button.

### IQuickAccessItemProvider
Interface that controls implement to support being added to the Quick Access Toolbar.

---

## K

### KeyTip
Keyboard shortcut badges that appear when pressing Alt. Allow keyboard-only navigation of the entire ribbon. Set via `KeyTip` property.

### KeyTipService
Static class that manages KeyTip display and navigation.

---

## L

### LargeIcon
32x32 icon used when control is in Large size. Set via `LargeIcon` property.

---

## M

### MediumIcon
24x24 icon used when control is in Middle size. Set via `MediumIcon` property.

---

## Q

### Quick Access Toolbar (QAT)
Customizable toolbar above or below the ribbon containing frequently-used commands. Users can add/remove items via right-click menu.

### QuickAccessMenuItem
Menu item in the QAT's dropdown menu.

---

## R

### Ribbon
The main ribbon control containing tabs, groups, and the Quick Access Toolbar.

### RibbonContextualTabGroup
See ContextualTabGroup.

### RibbonControlSize
Enum defining control display sizes: `Large`, `Middle`, `Small`.

### RibbonControlSizeDefinition
Defines how a control resizes as its group collapses. Format: `"Large,Middle,Small"`.

### RibbonGroupBox
Container for controls within a tab. Has a header, optional dialog launcher, and automatic collapse behavior.

### RibbonGroupBoxState
Enum for group states: `Large`, `Middle`, `Small`, `Collapsed`, `QuickAccess`.

### RibbonProperties
Static class providing attached properties like `Size`, `SizeDefinition`, `MouseOverBackground`, etc.

### RibbonTabControl
The tab strip containing RibbonTabItems.

### RibbonTabItem
Individual tab in the ribbon. Contains RibbonGroupBox items.

### RibbonTitleBar
The title bar area containing window title, contextual tab headers, and optional QAT.

### RibbonWindow
Window class that provides proper ribbon integration with window chrome.

---

## S

### ScreenTip
Enhanced tooltip with title, image, body text, and optional help link. Richer than standard ToolTip.

### Simplified Ribbon
Compact single-row ribbon mode. Toggled via collapse button. Controls use `SimplifiedSizeDefinition`.

### SizeDefinition
See RibbonControlSizeDefinition.

### SplitButton
Button with two click zones: main area executes command, dropdown arrow opens menu.

### StartScreen
Overlay shown on application startup (like Office start screen). Uses `Shown` property for one-time display.

### StateDefinition
Defines the collapse progression of a RibbonGroupBox. Format: `"Large,Middle,Small,Collapsed"`.

### StatusBar
Bar at bottom of window showing status information. Uses `StatusBarItem` and `StatusBarPanel`.

---

## T

### ThemeManager
ControlzEx class for managing application themes. Provides built-in themes and runtime theme generation.

### ToggleButton
Button with checked/unchecked state, like a checkbox styled as a button.

### TwoLineLabel
Internal control that splits button text across two lines for large buttons.

---

## W

### WindowCommands
Container for custom buttons in the title bar area (minimize, settings, etc.).

---

## Size Reference

| Size | Icon | Description |
|------|------|-------------|
| Large | 32x32 | Full size, stacked icon/text |
| Middle | 24x24 | Compact, horizontal layout |
| Small | 16x16 | Minimal, icon only |

---

## Common Brush Prefix Reference

| Prefix | Used For |
|--------|----------|
| `Fluent.Ribbon.Brushes.Button.*` | Button states |
| `Fluent.Ribbon.Brushes.RibbonTabItem.*` | Tab styling |
| `Fluent.Ribbon.Brushes.RibbonGroupBox.*` | Group styling |
| `Fluent.Ribbon.Brushes.Gallery.*` | Gallery styling |
| `Fluent.Ribbon.Brushes.ApplicationMenu.*` | App menu styling |

---

*See individual reference guides for detailed coverage of each topic.*
