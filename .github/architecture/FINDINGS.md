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
| **Up opens a drop down with the first item focused.** The key handler focused the last item, then the queued open callback always focused item 0. | `fix/dropdown-up-key-focus` | Tests only, [run #30](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678603225): Down passes, *Up should focus the last item* fails. With fix: see the branch's latest run. |
| **Simplified groups ignore `SimplifiedStateDefinition`.** The reset always used `StateDefinition`. Found independently by the group-resizing note (S1) and earlier by hand. | `fix/simplified-state-definition-reset` | Runs #18 / #19, see `UPSTREAM-FIXES.md` |
| **`ReduceOrder` XML doc says the opposite of the code.** The code reduces the last entry first. | `docs/reduceorder-xml-doc` | Documentation only. Every sentence checked against `RibbonGroupsContainer.cs`. Build: [run #29](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678547621) |

## Found by re-checking our own fixes against the notes

| Finding | Status |
|---|---|
| **#813 `FocusFirstItemOnDropDownOpen="False"` also blocks keyboard opening.** Opening with Down/Up the first time likely focuses nothing: the key handler runs before the items exist, and the #813 callback then returns early. The option is meant for mouse opening only. | Being fixed together with the Up-key fix: keyboard opens record the item to focus, mouse opens follow the option. |

## Confirmed, not fixed (reason given)

| Finding | Why not fixed |
|---|---|
| `DropDownButton`'s "whole drop down is disabled" fallback calls `Keyboard.Focus(DropDownPopup.Child)`, but that child is a `ResizeableContentControl`, which sets `Focusable = false`. So it does nothing. | What should get focus instead is a design choice for the maintainer. |
| `RibbonGroupsContainer.OnReduceOrderChanged` uses `Skip(reduceOrderIndex)`, which undoes one entry more than was applied. It's harmless for group entries, but a `(Name)` entry's scale goes one step above normal. | The visible effect on scalable controls is unclear. Needs a scenario to reproduce first. |

## Facts worth knowing (not bugs)

- **Quick Access Toolbar contents are not saved.** The state storage writes only `IsMinimized`, `ShowQuickAccessToolBarAboveRibbon` and `IsSimplified` (`RibbonStateStorage.cs:161-165`, checked by hand). Apps that want the toolbar remembered must save it themselves.
- **No `ReduceOrder`, no shrinking.** Groups only change size when listed in the tab's `ReduceOrder`. Without it, the tab scrolls sideways instead.
- **Setting `RibbonGroupBox.State` directly doesn't stick** when the group is in a `ReduceOrder`: each measure copies `StateIntermediate` into `State`.
- **Theming (corrects the January #1259 guide):** control templates use `DynamicResource` for brushes 571 times and `StaticResource` 0 times, and no library code touches application resources. Overriding a `Fluent.Ribbon.Brushes.*` key in app resources is the supported way. See `07-theming.md`.

## Candidates not yet verified

These were read from the code by AI and have not been checked by hand. **Treat
them as questions, not facts.** The note is given for each, for its evidence.

**Quick Access Toolbar (`06`)**

- Calling `AddToQuickAccessToolBar` before the ribbon has its template records the element but shows nothing. After that it can never be added.
- Re-templating the Ribbon empties the toolbar, and nothing re-adds the checked items.
- After a re-template, the old `CollectionSyncHelper` is never unsubscribed.
- `QuickAccessItems.Clear()` (Reset) isn't handled, so cleared entries stay in the drop down.
- `SizeChanged` handlers stay attached when the toolbar is cleared.
- The `Gallery` branch in the Add command's can-execute check can't run (dead code).
- Likely intended: removing a menu entry leaves its target on the toolbar, and turning `CanAddToQuickAccessToolBar` off doesn't remove an existing item.

**KeyTips (`03`)**

- The root adorner is always the Ribbon, so a wrong key at any level closes all KeyTips, and the "beep and keep input" path can never run.
- `Detach` before a delayed attach completes leaves `isAttaching` true.
- `Show` drops an old adorner chain without terminating it.
- `attached = true` is set before the window lookup, so the service can stay inert.
- `Detach` doesn't close KeyTips that are showing.

**Drop downs (`04`)**

- On a `SplitButton`, Enter both opens the drop down and clicks the inner button, whose definitive click closes it again.
- The `ClosePopupOnMouseDown` delay task is never cancelled, so it can close a newly reopened popup.
- The mouse, keyboard and KeyTip paths use `SetValue` for `IsDropDownOpen` (which replaces a OneWay binding), but other paths use `SetCurrentValue`.

**Backstage (`05`) and Ribbon (`01`)**

- Unloading the Backstage while it's open destroys its overlay without restoring the ribbon state, so `IsBackstageOrStartScreenOpen` can stay true. **Found independently by both notes.**
- Reopening during the close animation: the old "completed" handler can hide the Backstage while `IsOpen` is true.
- Destroying the adorner clears all command bindings on a layer the Backstage and StartScreen may share.
- A StartScreen with `Shown = true` still receives KeyTips while invisible.
- The state storage's memory stream is rewound but not truncated, so a shorter write can leave stale characters.
- Turning `AutomaticStateManagement` back on after load never reads the saved file.
- `CanUseSimplified = false` while simplified hides the menu item to switch back.

**Group resizing (`02`)**

- Changing `ReduceOrder` doesn't clear the measure cache, so the new order may not apply until the width changes.
- A state definition with more than 4 entries loses states.
- Leaving the Collapsed state doesn't re-coerce `IsDropDownOpen`.
