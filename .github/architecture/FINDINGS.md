# Findings from the architecture notes

The seven notes in this folder each end with "Suspicious findings (unverified)":
places where an AI read the code and something looked wrong. This file tracks
what happened to each one. **Nothing here is posted upstream.**

How a finding moves through this list:

1. **Candidate:** listed in a note. Not trusted yet.
2. **Confirmed:** re-read in the code by hand, and the reasoning holds.
3. **Proven:** a test fails on the unfixed code on Windows CI, all 3 frameworks.
4. **Fixed:** the same test passes with the fix, and nothing else fails.

A candidate can also be **rejected** (the code is fine, or it's intended) or
**not pursued** (real but too uncertain, too risky, or needing the maintainer).

## Fixed (proven by a failing test, then fixed)

| Finding | Branch | Evidence |
|---|---|---|
| **KeyTips in groups never snap to rows.** `KeyTipAdorner.SnapToRowsIfPresent` took the position as a `Point` (a struct) by value, so the snapped Y was lost. This has been broken since ddaa57fb (2018), the commit that added snapping for #572. | `fix/keytip-row-snapping` | Tests only, [run #27](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678426690): *KeyTip center 18.96 should be on one of the rows 0, 14, 28, 47*. With fix, [run #28](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678474843): 306/306. **Changes where KeyTips appear: needs a visual check.** |
| **Backstage keeps replaced content bound to its visibility.** `Backstage.OnContentChanged` cleared the binding on `e.NewValue` inside the `e.OldValue` branch. | `fix/backstage-content-visibility-binding` | Tests only, [run #25](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678264930): *expected False, was True*. With fix, [run #26](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678305314): 306/306. |
| **Up opens a drop down with the first item focused.** The key handler focused the last item, then the queued open callback always focused item 0. | `fix/dropdown-up-key-focus` | Tests only, [run #30](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678603225): Down passes, *Up should focus the last item* fails. With fix, [run #31](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36679578827): 307/307, both keyboard tests pass on all 3 frameworks. |
| **`QuickAccessItems.Clear()` leaves the entries in the toolbar's customize menu.** The handler mirrored Add, Remove and Replace into the menu, but ignored Reset, which `Clear()` raises. (Candidate from `06`.) | `fix/qat-items-clear` | Tests only, [run #38](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36682235757): *Cleared entries must leave the menu*, and the menu still held "First" and "Second". With fix, [run #39](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36682295012): 306/306. |
| **`RibbonStateStorage` temporary state keeps stale text.** `SaveTemporary` rewound the memory stream but never truncated it, so a shorter state kept the tail of a longer one, and the last value failed to parse on `LoadTemporary`. The library never calls `LoadTemporary` itself, so only apps that call it are affected. (Candidate from `01`.) | `fix/state-storage-temporary-truncate` | Tests only, [run #40](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36682494127): *IsSimplified expected True, was False*. With fix, [run #41](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36682530646): 306/306. A first version of the test was wrong: changing `ShowQuickAccessToolBarAboveRibbon` saves the temporary state by itself, which overwrote the saved state (runs #36 / #37, failing on `IsMinimized` with and without the fix). |
| **Hidden KeyTips count as a prefix.** An expanded group's own KeyTip ("ZC") is hidden and can't be pressed, but `ContainsKeyTipStartingWith` ignored visibility, so typing "Z" hid every KeyTip and swallowed the key. Found by the KeyTip triage (not in the note). | `fix/keytip-hidden-prefix` | Tests only, [run #44](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36687765504): *Hidden KeyTip "ZC" must not match the prefix "Z"*. With fix, [run #45](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36687859550): 306/306. |
| **"Add Gallery to Quick Access Toolbar" is always disabled.** The can-execute check required the Gallery itself to be addable, but a Gallery is added through the control hosting it. (Candidate 8 of `06`.) | `fix/qat-gallery-can-execute` | Tests only, [run #46](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36687986486): *CanExecute expected True, was False*. With fix, [run #47](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688066693): 306/306. |
| **Quick Access elements added before the ribbon has its template never appear**, for example when restored in a window constructor. They count as added, so they can't be added again either. (Candidate 2 of `06`.) | `fix/qat-add-before-template` | Tests only, [run #48](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688151084): the toolbar was empty. With fix, [run #49](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688230067): 306/306. |
| **Turning off `IsKeyTipHandlingEnabled` leaves KeyTips on screen**, and nothing can close them anymore. (Candidate 6 of `03`.) | `fix/keytip-detach-terminates` | Tests only, [run #50](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688349862): KeyTips still visible after `Detach`. With fix, [run #51](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688418462): 306/306. |
| **Enter clicks a SplitButton's disabled button part.** With `IsButtonEnabled = false` the part is greyed out, but Enter still raised `Click`. (Candidate 3 of `04`.) | `fix/splitbutton-enter-disabled-button` | Tests only, [run #52](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688670103): 2 clicks instead of 1 (the enabled baseline clicked once). With fix, [run #53](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688741470): 306/306. |
| **Unloading an open backstage leaves the ribbon stuck in "backstage mode"** (Quick Access Toolbar hidden, window Esc handler still attached, WinForms hosts collapsed); a later close can't restore it. Found independently by the Ribbon (`01`) and Backstage (`05`) notes. | `fix/backstage-unload-while-open` | Tests only, [run #54](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688881286): *IsBackstageOrStartScreenOpen expected False, was True*. With fix, [run #55](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688992864): 306/306. |
| **An expanded group keeps its drop down open.** The "only open while collapsed" rule was only checked when the drop down itself was set. (S6 of `02`.) | `fix/groupbox-close-dropdown-on-expand` | Tests only, [run #56](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36689167900): *IsDropDownOpen expected False, was True*. With fix, [run #57](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36689246700): 306/306. |
| **A ribbon in a window that is already small doesn't collapse** until the window is resized. Also at every startup: the window's first size change comes before the ribbon subscribes on Loaded. (R6 of `01`.) | `fix/ribbon-collapse-on-load` | Tests only, [run #58](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36689585574): *IsCollapsed expected True, was False*. With fix, [run #59](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36689653003): 306/306. |
| **Simplified groups ignore `SimplifiedStateDefinition`.** The reset always used `StateDefinition`. Found independently by the group-resizing note (S1) and earlier by hand. | `fix/simplified-state-definition-reset` | Runs #18 / #19, see `UPSTREAM-FIXES.md` |
| **`ReduceOrder` XML doc says the opposite of the code.** The code reduces the last entry first. | `docs/reduceorder-xml-doc` | Documentation only. Every sentence checked against `RibbonGroupsContainer.cs`. Build: [run #29](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678547621) |

## Found by re-checking our own fixes against the notes

| Finding | Status |
|---|---|
| **#813 `FocusFirstItemOnDropDownOpen="False"` also blocks keyboard opening.** Suspected: opening with Down/Up the first time focuses nothing, because the #813 callback returns early. | **Rejected: the hypothesis was wrong.** A test for it passed on the unchanged #813 code ([run #32](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36679860547), 310/310, 0 inconclusive, the focus baseline passed). `OnKeyDown` already focuses the item synchronously, and returning early in the callback doesn't undo that. The guard written for it (2a229074) was reverted, and the test was kept as a regression guard. Only the code comments changed. |

## Confirmed, not fixed (reason given)

> **Update 2026-10-01 (round 4):** every row in this table has since been fixed and
> proven test-first in this fork, at the owner's request. See `ROUND4.md` for the
> branch, the CI runs and what changes in an app. The reasons below explain why
> they were first left for the maintainer.

| Finding | Why not fixed |
|---|---|
| `DropDownButton`'s "whole drop down is disabled" fallback calls `Keyboard.Focus(DropDownPopup.Child)`, but that child is a `ResizeableContentControl`, which sets `Focusable = false`. So it does nothing. | What should get focus instead is a design choice for the maintainer. |
| **Drop downs lose an app's OneWay binding on `IsDropDownOpen`.** Mouse, keyboard and KeyTip paths write it with `SetValue`, which replaces a OneWay binding; only two paths use `SetCurrentValue`. After the first user open, the view model can't open or close the menu anymore. Same pattern in `RibbonGroupBox`, `InRibbonGallery`, `RibbonTabControl`. *(agent reading)* | Real and likely in MVVM apps, but the fix (`BindsTwoWayByDefault`, or switching about a dozen call sites in four controls) changes binding behaviour. Maintainer's call. |
| **A StartScreen with `Shown = true` and `IsOpen = true` takes over KeyTips.** It isn't displayed (by design, it shows once), but the KeyTip lookup only checks `IsOpen`, so Alt sends KeyTips to the invisible screen for the whole session. Apps that persist `Shown` (it binds two-way for that) hit this. | `Show()`'s result is ignored, so "open but not displayed" has no state to check. Options: track "displayed", reset `IsOpen`, or treat `IsOpen`+`Shown` differently. Maintainer's call. |
| **The "beep and keep input" path in `KeyTipService` can never run.** `activeAdornerChain.AdornedElement is Ribbon` is always true, so a wrong key at a nested KeyTip level closes all KeyTips instead of beeping (the comment and #908 test say that was meant for the first level only). | Changing it changes behaviour users are used to. Maintainer's call. |
| **Re-templating the Ribbon at runtime empties the Quick Access Toolbar, and the old collection sync helpers stay subscribed**, so a later `QuickAccessItems.Add` (or adding a tab) can throw. *(agent reading)* | Only when an app replaces the Ribbon's template or style at runtime; the built-in theme switching doesn't. A fix needs a detach method on the public `CollectionSyncHelper`. |
| **`ClosePopupOnMouseDown`'s delayed close is never cancelled**, so it can close a drop down reopened within the delay. *(agent reading)* | Only with long delays or code that reopens at once; the test would be timing-based. |
| `RibbonGroupsContainer.OnReduceOrderChanged`'s `Skip(reduceOrderIndex)` undoes one entry more than was applied. | No visible effect with the built-in controls: states stop at the top, and `InRibbonGallery` ignores extra enlarging. Only a custom `IScalableRibbonControl` could show it. |
| Changing `ReduceOrder` at runtime doesn't clear the measure cache, so the new order may only apply after the next width change. *(agent reading)* | Needs a runtime `ReduceOrder` change; one-line fix, a candidate for later. |
| A state definition with more than 4 entries (or duplicates) parses to wrong states (for example `"Large,Large,Middle,Small,Collapsed"` loses Small). *(agent reading)* | Only malformed input. |
| **A submenu can stay open after its drop down closes**: the open-submenu stack pops on every close without checking which item closed, and plain WPF menu items close without having been pushed. *(agent reading)* | Needs mixed WPF and Fluent menu items, or keyboard plus mouse. |
| Two Backstage-type controls in one window share an adorner layer, and destroying one's adorner clears all command bindings on it, disabling the other's back button. *(agent reading)* | Needs two backstages (or a backstage and a start screen) in one window. |
| A StartScreen writes its saved title bar value back on every close, even when it wasn't shown that time. *(agent reading)* | Cosmetic, needs a tiny window at that moment. |
| Turning `AutomaticStateManagement` on after load never reads the saved state (the storage already counts as loaded) and then overwrites it. *(agent reading)* | Changing when state counts as loaded is a behaviour change. Maintainer's call. |
| `KeyTipService.Attach` sets `attached = true` before it finds the window, so toggling `IsKeyTipHandlingEnabled` before the ribbon is in a window leaves KeyTips never working. *(agent reading)* | Low likelihood. |
| A KeyTip level whose element loads late can't be cancelled while it waits (`isAttaching` stays true after `Detach`). *(agent reading)* | No user sequence found that hits it: WPF delivers `Loaded` before the next key. |
| `KeyTipService.Show` drops an old KeyTip chain without terminating it (leaves an invisible adorner, loses the focus backup). *(agent reading)* | Its common trigger was the hidden-prefix bug above, fixed now. |
| `QuickAccessToolBar`'s Reset branch removes `SizeChanged` handlers from `Items`, which is already empty by then. *(agent reading)* | No visible effect (the discarded copy points at the toolbar, not the other way round). |
| `Ribbon.QuickAccessItems.Clear()` leaves `Ribbon` set on the removed entries. | Only matters if an app keeps a removed entry and toggles it. Fixing needs a new collection type. |

## Rejected, or intended

| Candidate | Verdict |
|---|---|
| Removing a Quick Access menu entry leaves its target on the toolbar | Intended: toolbar contents are tracked separately, and right-clicking the copy removes it. |
| Turning off `CanAddToQuickAccessToolBar` leaves an item on the toolbar | Intended: it only controls adding. |
| Esc is marked handled even when closing the backstage is blocked | Intended ("locked" backstage behaves like a modal). |
| `Backstage.CanChangeIsOpen` has no change callback | Not a bug: a refused request isn't replayed, which is right. |
| `IsSimplified` / `IsMinimized` ignore `CanUseSimplified` / `CanMinimize` when set from code | Intended: the flags limit what the user can switch. |
| The Ribbon and `RibbonWindow` each have their own auto-collapse | Deliberate (both carry the same "todo"). |
| `IRibbonStateStorage.LoadTemporary` is never called by the library | True, but the three values are dependency properties that survive re-templating anyway. |
| `if (keyTipsTarget is null)` in `KeyTipService.Show` | Dead code (the expression ends with `?? this.ribbon`), harmless. |

## Still open (need a runtime check)

- Reopening the backstage during its close animation: the old "completed" handler may hide it while `IsOpen` is true. Depends on WPF animation clocks.
- A tab double-click writes `IsMinimized` with `SetValue` onto a two-way binding set by the theme style. Whether WPF keeps the binding decides whether there is a bug. A small test would settle it.
- `DropDownButton.DismissOnClickOutside = false` keeps mouse capture, so clicks elsewhere in the window do nothing until Esc. Maybe intended; needs a manual check.
- `DropDownButton` toggles on Enter or Space bubbling up from its popup content, so a space typed into a text box inside a drop down might close it. Read by an agent, not checked by hand.
- Whether WPF runs `RemoveFromQuickAccessCommand`'s handler without its can-execute check (its `First(...)` would throw for a parameter that isn't on the toolbar). WPF's `CommandBinding` checks first, as far as known.

## Facts worth knowing (not bugs)

- **Quick Access Toolbar contents are not saved.** The state storage writes only `IsMinimized`, `ShowQuickAccessToolBarAboveRibbon` and `IsSimplified` (`RibbonStateStorage.cs:161-165`, checked by hand). Apps that want the toolbar remembered must save it themselves.
- **No `ReduceOrder`, no shrinking.** Groups only change size when listed in the tab's `ReduceOrder`. Without it, the tab scrolls sideways instead.
- **Setting `RibbonGroupBox.State` directly doesn't stick** when the group is in a `ReduceOrder`: each measure copies `StateIntermediate` into `State`.
- **Changing `ShowQuickAccessToolBarAboveRibbon` saves the temporary state by itself** (it's the only place the library calls `SaveTemporary`).
- **Theming (corrects the January #1259 guide):** control templates use `DynamicResource` for brushes 571 times and `StaticResource` 0 times, and no library code touches application resources. Overriding a `Fluent.Ribbon.Brushes.*` key in app resources is the supported way. See `07-theming.md`.

## How the candidates were checked

Four AI agents each took one area and were told to try to *disprove* every
candidate, with line citations. Everything in "Fixed" was then re-read in the
code by hand before its test was written, and went through a tests-only run that
had to fail for the predicted reason first. Rows marked *(agent reading)* were
only read by an agent: treat them as likely, not proven.
