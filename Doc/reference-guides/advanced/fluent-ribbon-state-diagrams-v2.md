---
title: Fluent.Ribbon State Machine Diagrams
description: Visual representation of how ribbon components change state
tags: [state, state-machine, diagrams, advanced]
see_also:
  - fluent-ribbon-attached-properties-reference.md
  - ../controls/fluent-ribbon-groupbox-reference.md
---

# Fluent.Ribbon State Machine Diagrams (v2)

**Visual representation of how ribbon components change state.**

*v2 Changes: Fixed RibbonTabItem border brush key, added QuickAccess state to RibbonGroupBox, added CanMinimize property, clarified hierarchy diagram*

---

## Ribbon Visibility States

```
                    ┌─────────────────┐
                    │     NORMAL      │
                    │  (Full ribbon)  │
                    └────────┬────────┘
                             │
              Double-click   │   Double-click tab
              tab / Ctrl+F1  │   / Ctrl+F1
                             ▼
                    ┌─────────────────┐
                    │   MINIMIZED     │
                    │ (Tabs only)     │◄────────┐
                    └────────┬────────┘         │
                             │                  │
                      Click  │                  │ Click elsewhere
                      tab    │                  │ / Escape
                             ▼                  │
                    ┌─────────────────┐         │
                    │   EXPANDED      │─────────┘
                    │ (Temp popup)    │
                    └─────────────────┘

Properties:
- IsMinimized = true/false
- CanMinimize = true/false (set to false to disable minimization entirely)
```

---

## RibbonTabItem States

```
                              Mouse Enter
                    ┌────────────────────────┐
                    │                        ▼
            ┌───────┴───────┐       ┌─────────────────┐
            │    NORMAL     │       │   MOUSE OVER    │
            │               │       │                 │
            └───────┬───────┘       └────────┬────────┘
                    │                        │
                    │ Click                  │ Click
                    │                        │
                    ▼                        ▼
            ┌─────────────────────────────────────────┐
            │              SELECTED                   │
            │  (Content area shows this tab's groups) │
            └─────────────────────────────────────────┘
                             │
                             │ Click different tab
                             ▼
                    ┌─────────────────┐
                    │  NORMAL (again) │
                    └─────────────────┘

Brushes:
- Normal:     Fluent.Ribbon.Brushes.RibbonTabItem.Background
- MouseOver:  Fluent.Ribbon.Brushes.RibbonTabItem.MouseOver.Background
- Selected:   Fluent.Ribbon.Brushes.RibbonTabItem.Active.Background
              Fluent.Ribbon.Brushes.RibbonTabItem.Border  (NOT .Active.Border!)
```

---

## Contextual Tab Group States

```
        ┌─────────────────┐                    ┌─────────────────┐
        │     HIDDEN      │───────────────────►│    VISIBLE      │
        │                 │   Visibility =     │                 │
        │                 │   Visible          │                 │
        └─────────────────┘◄───────────────────└─────────────────┘
                            Visibility =
                            Collapsed

Typically bound to selection:

    User selects image  ──►  PictureTools group visible
    User selects text   ──►  PictureTools group hidden

XAML:
<Fluent:RibbonContextualTabGroup
    Header="Picture Tools"
    Visibility="{Binding IsPictureSelected,
                 Converter={StaticResource BoolToVis}}" />
```

---

## Button States

```
                                    Mouse Enter
                           ┌──────────────────────┐
                           │                      ▼
                   ┌───────┴───────┐      ┌─────────────────┐
                   │    NORMAL     │      │   MOUSE OVER    │
                   │               │      │                 │
                   └───────────────┘      └────────┬────────┘
                                                   │
                                           Mouse   │
                                           Down    │
                                                   ▼
                                          ┌─────────────────┐
                                          │    PRESSED      │
                                          │                 │
                                          └────────┬────────┘
                                                   │
                                           Mouse   │
                                           Up      │
                                                   ▼
                                          ┌─────────────────┐
                                          │  CLICK EVENT    │
                                          │    FIRED        │
                                          └─────────────────┘


        ┌─────────────────┐
        │    DISABLED     │  (IsEnabled = false)
        │   (Grayed out)  │
        └─────────────────┘

Brushes:
- Normal:     Fluent.Ribbon.Brushes.Button.Background
- MouseOver:  Fluent.Ribbon.Brushes.Button.MouseOver.Background
- Pressed:    Fluent.Ribbon.Brushes.Button.Pressed.Background
- Disabled:   (uses opacity, grayed icon)
```

---

## ToggleButton / CheckBox States

```
                    ┌─────────────────┐
            ┌──────►│    UNCHECKED    │◄──────┐
            │       │  IsChecked=F    │       │
            │       └────────┬────────┘       │
            │                │                │
            │         Click  │                │ Click
            │                ▼                │
            │       ┌─────────────────┐       │
            └───────│    CHECKED      │───────┘
                    │  IsChecked=T    │
                    └─────────────────┘

Three-State (IsThreeState=true):

    UNCHECKED ──► CHECKED ──► INDETERMINATE ──► UNCHECKED
        │                          │
        └──────────────────────────┘
```

---

## Backstage States

```
        ┌─────────────────┐                    ┌─────────────────┐
        │     CLOSED      │───────────────────►│      OPEN       │
        │                 │   Click "File"     │                 │
        │ (Ribbon visible)│   or IsOpen=true   │(Full-screen UI) │
        └─────────────────┘◄───────────────────└─────────────────┘
                            Click outside /
                            Escape /
                            IsOpen=false

Properties:
- IsOpen = true/false

Code:
backstage.IsOpen = true;   // Open
backstage.IsOpen = false;  // Close
```

---

## DropDownButton / SplitButton States

```
                   ┌─────────────────┐
                   │     CLOSED      │
                   │                 │
                   └────────┬────────┘
                            │
                     Click  │  (or click arrow for SplitButton)
                            ▼
                   ┌─────────────────┐
                   │      OPEN       │
                   │  (Menu visible) │
                   └────────┬────────┘
                            │
                   Click    │  / Escape / Click outside
                   item     │
                            ▼
                   ┌─────────────────┐
                   │  CLOSED + EVENT │
                   │   (if clicked)  │
                   └─────────────────┘

SplitButton has two click zones:
┌────────────┬───┐
│   Button   │ ▼ │
│   Click    │   │
└────────────┴───┘
      │        │
      │        └──► Opens dropdown
      └───────────► Fires Command
```

---

## Window States (Glow)

```
                   ┌─────────────────┐
                   │     ACTIVE      │
                   │                 │
                   │ GlowColor used  │
                   └────────┬────────┘
                            │
                   Focus    │   Focus to
                   lost     │   another window
                            ▼
                   ┌─────────────────┐
                   │    INACTIVE     │
                   │                 │
                   │ NonActiveGlow-  │
                   │ Color used      │
                   └────────┬────────┘
                            │
                   Focus    │   Click window /
                   gained   │   Alt+Tab back
                            ▼
                   ┌─────────────────┐
                   │  ACTIVE (again) │
                   └─────────────────┘

Properties:
- GlowColor         (active window)
- NonActiveGlowColor (inactive window)
- IsGlowTransitionEnabled (animate change)
- DWMSupportsBorderColor (read-only, Windows 11 feature detection)
```

---

## RibbonGroupBox States

```
        ┌─────────────────┐                    ┌─────────────────┐
        │    EXPANDED     │───────────────────►│   COLLAPSED     │
        │                 │  Window too narrow │   (Icon only)   │
        │ (Full controls) │                    │                 │
        └─────────────────┘◄───────────────────└────────┬────────┘
                            Window wide enough          │
                                                 Click  │
                                                        ▼
                                               ┌─────────────────┐
                                               │  POPUP OPEN     │
                                               │ (Shows controls │
                                               │  in dropdown)   │
                                               └─────────────────┘

Properties:
- State = Large / Middle / Small / Collapsed / QuickAccess
- IsCollapsedDefinitionResolutionEnabled

State Enum (RibbonGroupBoxState):
- Large     (0) - Full size, all controls visible
- Middle    (1) - Medium size
- Small     (2) - Small size
- Collapsed (3) - Icon only, click to expand popup
- QuickAccess (4) - When group is in Quick Access Toolbar
```

---

## Gallery States

```
        ┌─────────────────┐
        │   INLINE VIEW   │
        │                 │
        │ (Shows N items) │
        └────────┬────────┘
                 │
          Click  │  expand arrow
                 ▼
        ┌─────────────────┐
        │   POPUP VIEW    │
        │                 │
        │ (Shows all)     │
        └────────┬────────┘
                 │
          Click  │  item / outside
                 ▼
        ┌─────────────────┐
        │  ITEM SELECTED  │
        └─────────────────┘
```

---

## Quick Access Toolbar (QAT) States

```
        ┌─────────────────┐                    ┌─────────────────┐
        │  ABOVE RIBBON   │───────────────────►│  BELOW RIBBON   │
        │                 │  User preference   │                 │
        │ (In title bar)  │                    │ (Under tabs)    │
        └─────────────────┘◄───────────────────└─────────────────┘

Properties:
- ShowAboveRibbon = true/false
- CanQuickAccessLocationChanging = true/false (disable position change)
- IsMenuDropDownVisible = true/false (hide customize dropdown)
- HasOverflowItems = true/false (read-only)

Customize menu:
- Right-click QAT ──► "Show Below the Ribbon"
- Right-click button ──► "Add to Quick Access Toolbar"
```

---

## StartScreen States

```
        ┌─────────────────┐                    ┌─────────────────┐
        │     HIDDEN      │───────────────────►│    VISIBLE      │
        │                 │   IsOpen=true      │                 │
        │                 │   Shown=false      │ (Covers window) │
        └─────────────────┘◄───────────────────└─────────────────┘
                            IsOpen=false

Properties:
- IsOpen = true/false
- Shown = true (only show once, tracks if already shown)

Typically shown on app launch, then dismissed.
```

---

## ComboBox States

```
                   ┌─────────────────┐
                   │     CLOSED      │
                   │ (Shows selected │
                   │  item or text)  │
                   └────────┬────────┘
                            │
                     Click  │
                            ▼
                   ┌─────────────────┐
                   │      OPEN       │
                   │  (Dropdown list │
                   │   visible)      │
                   └────────┬────────┘
                            │
                   Select   │  / Click outside / Escape
                   item     │
                            ▼
                   ┌─────────────────┐
                   │  CLOSED + EVENT │
                   │ (SelectionChanged)│
                   └─────────────────┘

Properties:
- IsDropDownOpen = true/false
- IsEditable = true/false (allow text input)
- ResizeMode = None/Vertical/Both
```

---

## Overall Ribbon Component Hierarchy

```
RibbonWindow (Properties: GlowColor, NonActiveGlowColor, DWMSupportsBorderColor)
├── Glow (active/inactive colors)
├── TitleBar
│   ├── Icon
│   ├── Title
│   ├── QuickAccessToolBar (ShowAboveRibbon property)
│   └── WindowCommands (min/max/close)
│
├── Ribbon (Properties: IsMinimized, CanMinimize, IsCollapsed, CanUseSimplified)
│   ├── Backstage (Property: IsOpen)
│   ├── ContextualTabGroups (Property: Visibility per group)
│   ├── RibbonTabItems (States: normal/hover/selected)
│   │   └── RibbonGroupBoxes (Property: State = Large/Middle/Small/Collapsed/QuickAccess)
│   │       └── Buttons/Controls (States: normal/hover/pressed/disabled)
│   └── QuickAccessToolBar (if below ribbon)
│
├── Content Area
│   └── Your application content
│
└── StatusBar (optional)
```

---

## Key Takeaways

1. **Most states are automatic** - WPF triggers handle hover/pressed/selected
2. **You control visibility** - Contextual tabs, Backstage open/close
3. **Theme changes all colors** - States use DynamicResource brushes
4. **Minimized ribbon has 3 sub-states** - Normal, minimized, temporarily expanded
5. **RibbonGroupBox has 5 states** - Large, Middle, Small, Collapsed, QuickAccess
6. **Use CanMinimize=false** to prevent users from minimizing the ribbon
