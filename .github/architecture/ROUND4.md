# Round 4: the "left for the maintainer" items, keyboard, contrast, and a new bug hunt

Done 2026-10-01. **AI (Claude) was used** for all of it. Nothing was sent to the
original project.

Until now, `FINDINGS.md` and `ACCESSIBILITY.md` had tables of real problems we
had **not** fixed, mostly because they change behaviour ("maintainer's call").
The owner of this fork asked for all of them to be done here anyway, so they
can be tested in the app first. This file is the record.

## How each one was proven

Same rule as before, for every branch:

1. A separate agent re-read the code and **confirmed** the problem (or refuted it).
2. A **tests-only** commit was built on Windows CI. It had to fail on net462,
   net6.0 and net8.0, **in the new test, with the predicted message**, and
   nothing else may fail. A compile error, a broken precondition or an
   inconclusive test does **not** count, and happened several times (see
   "What went wrong on the way").
3. The **fix** commit was built. All tests had to pass.

"Fail run" and "pass run" are run numbers of **Build (Windows)** in this fork's
Actions tab; the branch filter there shows them.

Each fix is on `fix/<name>` (proof history) and `upstream-pr/<name>` (the same
change on top of upstream `develop`; retired on 2026-10-06, since the original
project won't take them, see `../README.md`).
All of them are merged together on `integration/next`, and `integration/all-fixes` (the app package) was moved up to it once the combined build passed.

## Drop downs and menus

| Problem | Branch | Fail → pass | What changes in your app |
|---|---|---|---|
| An app's **OneWay binding on `IsDropDownOpen` is lost** the first time the user opens or closes the drop down (mouse, keys, KeyTips, automation and click-outside all wrote a local value). | `dropdown-keeps-oneway-binding` | #102 → #140 | Bindings survive. ~60 internal writes now use `SetCurrentValue`. Public setters are unchanged. |
| When **every item is disabled**, opening the drop down left keyboard focus where it was (it tried to focus a non-focusable panel). | `dropdown-disabled-items-focus` | #89 → #108 | Focus moves to the button, so Esc/Tab work. Same for collapsed groups. |
| **ClosePopupOnMouseDown's delayed close** could close a drop down the user had reopened in the meantime. | `dropdown-delayed-close-cancel` | #90 → #104 | A stale close is ignored. |
| A **submenu could stay open** after its drop down closed (the open-submenu list popped the wrong item when a plain WPF menu item closed). | `dropdown-submenu-stack` | #91 → #103 | All submenus close with the drop down. |
| **Tab** with focus still on an open drop-down button moved focus away but left the drop down open. | `dropdown-tab-closes` | #93 → #105 | Tab closes it (like a ComboBox). |
| **Opening an InRibbonGallery with Enter applied the first item** (the Enter key-up landed on the newly focused item). | `gallery-item-enter-keyup-only` | #92 → #109 | A gallery item only reacts to Enter that was pressed on it. |

## KeyTips, Backstage and StartScreen

| Problem | Branch | Fail → pass | What changes |
|---|---|---|---|
| A **StartScreen with `Shown = true` and `IsOpen = true`** (e.g. `Shown` saved in settings) took over KeyTips although it wasn't on screen: Alt did nothing. | `keytip-ignores-hidden-startscreen` | #112 → #139 | Alt shows the ribbon's KeyTips. |
| A **wrong key at a nested KeyTip level closed all KeyTips**; the intended "beep and keep" branch could never run. | `keytip-wrong-key-nested-level-keeps-keytips` | #136 → #167 | Inside a tab/group a wrong key beeps and keeps the KeyTips. At the first level it still closes them. Also inside an open Backstage. |
| Toggling **`IsKeyTipHandlingEnabled` before the ribbon is in a window** left KeyTips permanently off. | `keytip-attach-before-window` | #115 → #154 | KeyTips work once the window exists. |
| A KeyTip level **waiting for its element to load could not be cancelled**, and `Show` dropped an old chain without ending it (ghost KeyTips later). | `keytip-waiting-level-cancel` | #116 → #137 | Esc/terminate really ends it. |
| **Two backstages (or Backstage + StartScreen) in one window**: closing one cleared *all* command bindings on the shared adorner layer, disabling the other's back button. | `backstage-adorner-keeps-other-bindings` | #121 → #161 | Each removes only its own binding. |
| The **StartScreen wrote its saved title bar state back on every close**, even when it wasn't shown. | `startscreen-titlebar-restore-once` | #117 → #151 | Restored once, after a real show. |
| Turning **`AutomaticStateManagement` on after load** never read the saved state, then overwrote it. | `state-storage-load-after-enabling` | #114 → #138 | Enabling it later loads the saved state (the ribbon may minimize/switch to simplified at that moment). |

## Quick Access Toolbar and ribbon layout

| Problem | Branch | Fail → pass | What changes |
|---|---|---|---|
| **Re-templating the Ribbon** at runtime emptied the toolbar, and the old sync helpers stayed subscribed so a later `Tabs.Add` threw. | `ribbon-retemplate-keeps-qat-and-sync` | #118 → #153 | Pinned items survive; no exception. |
| `QuickAccessItems.Clear()` left `Ribbon` set on the removed entries (checking one later still pinned its target). | `qat-items-clear-detaches-ribbon` | #120 → #156 | Removed entries are detached. |
| Clearing toolbar items didn't unsubscribe their `SizeChanged` (small leak). | `qat-reset-unsubscribes-sizechanged` | #119 → #155 | None visible. |
| Changing `ReduceOrder` undid one reduction too many. | `reduceorder-change-undo-off-by-one` | #122 → #152 | None visible with built-in controls. |
| Changing `ReduceOrder` at runtime could wait for the next resize. | `reduceorder-change-clears-measure-cache` | #123 → #165 | Applies at once. |
| A state definition with **more than 4 parts** parsed wrong ("…,Small,Collapsed" lost Small). | `state-definition-more-than-four` | #124 → #164 | Parsed correctly. |
| `ColorGallery` re-subscribed the old "More Colors" button instead of unsubscribing it. | `colorgallery-old-button-unsubscribe` | #81 → #82 | None visible (fixes a leak). |
| `Gallery.IsLastItemPropertyKey` is public, so any code could set a read-only property. | `gallery-islastitem-key-obsolete` | compiles #185 | Marked `[Obsolete]` (a warning for code that uses it). Not made private: that would break apps. |

## Screen readers (UI Automation)

| Problem | Branch | Fail → pass | What a screen reader user notices |
|---|---|---|---|
| Toolbar **"Customize" and "More controls" buttons had no name**, and "More controls" wasn't in the tree. | `uia-qat-buttons-named` | #135 → #168 | "Customize Quick Access Toolbar, button". |
| The **toolbar was listed twice** in a RibbonWindow. | `uia-qat-listed-once` | #111 → #188 | Listed once, under the title bar. |
| **Backstage tabs** were plain list items without "selected". | `uia-backstage-tab-selection` | #96 → #107 | "Info, tab item, selected"; Select() works. |
| **InRibbonGallery advertised a Scroll pattern** it didn't implement (clients could crash). | `uia-inribbongallery-scroll-pattern` | #84 → #99 | No broken pattern. |
| A tab's **`AutomationProperties.Name` was overridden by its header**. | `uia-tab-name-respects-automation-name` | #85 → #100 | The name you set wins. |
| **KeyTips and ScreenTip text reached UIA only for Button.** | `uia-keytip-screentip-all-controls` | #97 → #106 | Access key and help text for all ribbon controls. |
| **Toolbar copies dropped `AutomationProperties.Name`/`HelpText`** (icon-only copies were unnamed). | `uia-qat-copy-keeps-automation-name` | #86 → #101 | Copies have the name. `AutomationId` is deliberately **not** copied (it must stay unique). |
| **DropDownButton reported control type "Custom"** and its class name instead of a localized type. | `uia-dropdownbutton-control-type` | #98 → #133 | "button" (SplitButton: "split button"). UI tests that look for "Custom" need updating. |

## Keyboard

| Problem | Branch | Fail → pass | What changes |
|---|---|---|---|
| **ColorGallery: the first arrow key picked a color and closed the drop down.** | `colorgallery-arrow-keys-browse` | #125 → #158 | Arrows browse; **Enter or Space** picks. Mouse unchanged. |
| In the Backstage, **Shift+Tab moved forward** into the content. | `backstage-shift-tab` | #169 → #183 | Shift+Tab goes back. |
| The **backstage tab control was an invisible tab stop**. | `backstage-tabcontrol-not-tab-stop` | #127 → #162 | One Tab fewer. |
| **ColorGallery and the toolbar holder** (toolbar below the ribbon) were empty tab stops. | `empty-tab-stops-qat-colorgallery` | #166 → #180 | One Tab fewer each. |
| A collapsed group opened on **Space but not Enter**; Space didn't activate a gallery item. | `groupbox-enter-gallery-space` | #129 → #160 | Both keys work. |
| The **simplified Spinner showed no focus highlight**. | `simplified-spinner-focus-highlight` | #130 → #159 | It does now. |

## Contrast and High Contrast

| Problem | Branch | Fail → pass | What changes |
|---|---|---|---|
| Text box border 1.5:1, placeholder 3.95:1, checked toggle border, focus border, check mark, backstage selection, gallery header text and selection, button hover text below WCAG in some or all of the 46 themes. | `contrast-wcag` | #131 → #157 | **Colors change**: text box border #7F7F7F in both themes (was #CCCCCC in both: more visible in Light, but **less** visible in Dark, 8.7:1 → 3.5:1; see `AUDIT.md`), a darker accent shade for indicators, black gallery header text with a yellow mouse-over background, a 1px outline on the selected gallery item, palette foregrounds for button hover text. Checked in all 46 Light/Dark themes. The "Colorful" title bar was left alone (brand decision). |
| **No Windows High Contrast support** (#1018). | `high-contrast-basic` | #132 → #163 | **Opt-in**: with `ThemeManager.Current.ThemeSyncMode = SyncWithHighContrast` the ribbon uses the user's High Contrast colors. Limits are listed in `HIGH-CONTRAST.md`; needs a manual check under the Windows High Contrast themes. |

## New bugs found by the round 3 bug hunt

| Problem | Branch | Fail → pass | What changes |
|---|---|---|---|
| **Adding a Spinner to the toolbar could change its value** (e.g. -50 became -10 in the view model): the copy bound Value before Minimum/Maximum. | `spinner-qat-copy-keeps-value` | #141 → #170 | Value is kept. |
| **Negative numbers turned positive** in cultures that use "−" (U+2212) as minus (sv-SE, nb-NO, fi-FI… on .NET 5+), on every Enter or focus loss. | `spinner-unicode-minus` | #142 → #171 | Sign kept. |
| **Leaving the Spinner rounded the value** to its display format even when nothing was typed (0.25 with F1 became 0.3). | `spinner-focus-out-keeps-value` | #148 → #177 | Value kept unless edited. |
| Spinner **Minimum/Maximum were not re-checked** when the other one changed. | `spinner-minimum-recoerce` | #145 → #174 | Range stays consistent. |
| **Text typed into a TextBox's toolbar copy never reached the view model** (default LostFocus binding on the original). | `textbox-qat-copy-updates-source` | #143 → #172 | Updated when the copy loses focus. |
| **RadioButton toolbar copies from different groups unchecked each other**, and the originals with them. | `radiobutton-qat-copy-groupname` | #144 → #173 | Copies keep their own groups. |
| CheckBox toolbar copy ignored `IsThreeState`. | `checkbox-qat-copy-threestate` | #149 → #178 | Copied. |
| **Clearing or removing Gallery filters** left stale menu entries and kept filtering by a removed filter. | `gallery-filters-reset-and-removed` | #146 → #175 | Menu and filter follow the collection. |
| Setting `Gallery.SelectedFilter` from code left the wrong checkmark. | `gallery-selectedfilter-checkmark` | #147 → #176 | Right checkmark. |
| An icon size string like "16.5" was read as 165 under German culture. | `image-converter-invariant-size` | #150 → #179 | Invariant parsing. |

Not fixed:

- Opening an InRibbonGallery's toolbar copy can push `SelectedItem = null`
  into an app's binding. A safe fix needs a redesign of how the copy borrows the
  items; a coerce guard (like ComboBox's) would probably not stop the binding.
- `StatusBar`'s Add branch inserts the new menu entry at the item's index,
  while Move and Remove use index + 1 (entry 0 is the menu's header), so an
  added entry lands one place too early. Reading the code, the menu is rebuilt
  (`RecreateMenu`) once the new container is generated, which should put it
  right again; that was not tested, so it stays open.
- `ApplicationMenu`'s default KeyTip comes from the localization only once
  (`CoerceKeys`, in the constructor), so it doesn't follow a runtime language
  switch like the menu headers now do (`localization-runtime-switch`).

## Window, theme and localization

| Problem | Branch | Fail → pass | What changes |
|---|---|---|---|
| **Moving an item in a StatusBar's `ItemsSource`** (`ObservableCollection.Move`) threw "Element already has a logical parent": the code called `Items.Remove(index)`, which looks for the boxed number as an item and removes nothing. | `statusbar-move-keeps-menu-in-sync` | #192 → #196 | Moving works; the right-click menu follows the new order. |
| **Switching `RibbonLocalization.Current.Culture` at runtime** left the ribbon's right-click menu and the StatusBar's "Customize Status Bar" header in the old language (they were bound to the old localization object). | `localization-runtime-switch` | #193 → #197 | Those headers follow a runtime language switch. |
| On a display with **more than 100% scaling**, the default maximum drop-down height used physical pixels as WPF units (200% scaling: twice too high, the drop down could run off screen). | `dropdown-max-height-dpi` | #194 → #198 | A third of the screen height at any scaling. |
| A **custom RibbonWindow template without `PART_QuickAccessToolbarHolder`** gave the title a 50px fallback width, and with contextual groups visible the title grew by 2px on every layout pass. | `titlebar-without-qat-holder` | #195 → #199 | The title is laid out normally. Only affects custom templates. |

## All of it together

`integration/next` at `6828bf41` (all round 4 branches plus everything that was
already on `integration/all-fixes`) passed **Build (Windows) #201**:
net6.0 902/902, net8.0 902/902, net462 901/901 with one test inconclusive
(its keyboard focus precondition didn't hold on that run, see below).
`integration/all-fixes` was then moved up to that commit (a fast-forward,
nothing on it was lost), so the app package now contains round 4.

## Two fixes that only broke when combined

`gallery-item-enter-keyup-only` (Enter must go down on the item) and
`groupbox-enter-gallery-space` (Space clicks too) each passed alone. Merged
together, Space never clicked: only Enter was remembered on key-down. The
combined build caught it (`Space_clicks_the_item` failed on all 3 frameworks,
877 other tests passed), and `integration/next` remembers whichever key went
down. This is why the combined branch gets its own full build.

## What went wrong on the way (and was fixed before counting)

- 4 tests-only runs failed to **compile** (WPF's own Ribbon assembly has peer
  classes with the same names; a StyleCop blank-line rule; CA1812 on a test
  helper created by reflection; a WpfAnalyzers naming rule). None was counted.
- One test **crashed** (a key event without a routed event) and one had a
  **precondition** that didn't hold headless (toolbar overflow needs layout).
  Both were rewritten; only the rewritten runs count.
- One new test **leaked saved ribbon state** into a later test (toolbar
  position saved to isolated storage). It now turns state saving off.
- One test **didn't run** (group box state reset on load) until rewritten.
- **Focus tests failed at random in the long combined build.** The full
  builds #186, #187, #191 and #200 each had one or two keyboard focus tests
  fail on one framework (a different test almost every time), with nothing
  focused at all, while they passed on the other frameworks.
  The test window had stopped being the active window. All tests that assert
  "this element has focus" now first check that *something* has focus and
  report "inconclusive" if not. This can't hide the bugs they test: in those,
  focus lands on another element (the original backstage bug put it on the
  window, run #70).
