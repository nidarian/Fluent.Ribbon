# Fixes ready for the original project

Fixes built and tested in this fork for open issues on
[fluentribbon/Fluent.Ribbon](https://github.com/fluentribbon/Fluent.Ribbon).
**Nothing has been posted upstream.** You asked for permission in
[#1284](https://github.com/fluentribbon/Fluent.Ribbon/issues/1284), which had no
reply yet as of 2026-09-28.

Each fix has two branches:

- `fix/...`: the working branch, including this fork's CI files. The Windows
  builds ran on this one.
- `upstream-pr/...`: the same code on top of the original's `develop`, without
  this fork's `.github` files. Open the upstream pull request from this one.

---

## #1251: QAT items added from code-behind ignore IsChecked

Issue: https://github.com/fluentribbon/Fluent.Ribbon/issues/1251
Branch to submit: `upstream-pr/1251-qat-ischecked`

**Bug.** You create a `QuickAccessMenuItem` with `IsChecked = true` and add it
to `ribbon.QuickAccessItems` from code. The item doesn't appear on the toolbar
until the user opens the quick access menu.

**Cause.** A checked item only puts itself on the toolbar in its first `Loaded`
event. XAML items get `Loaded` together with the ribbon. Items added later from
code only get `Loaded` when the menu containing them opens.

**Fix** (`QuickAccessMenuItem.cs`, 1 property). When the item is assigned to a
ribbon whose toolbar already exists, and the item is checked, it adds itself
right away. Before the template exists it does nothing, so the XAML path is
unchanged.

**Evidence (Windows CI):**

| Run | Code | Result |
|---|---|---|
| [#3](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36395245385) | tests only, no fix | Fails as expected on net462, net6.0, net8.0: `Checked_item_added_from_code_behind_is_shown_in_toolbar`, *Expected True, was False*. The other 308 tests pass. |
| [#4](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36395305347) | tests + fix | All pass |

The 4 new tests are in `Fluent.Ribbon.Tests/Controls/QuickAccessMenuItemTests.cs`.
They cover the bug, the workaround path (checking after adding), unchecked
items, and the XAML order (added before the template exists).

**Not covered:** unchecking an item from code before the menu has ever been
opened still doesn't remove it (`OnUnchecked` returns early while the item isn't
loaded). I left that alone: the guard looks deliberate, and changing it needs
the maintainer's view.

---

## #357: KeyTip placement for standard controls implementing IKeyTipedControl

Issue: https://github.com/fluentribbon/Fluent.Ribbon/issues/357 (open since 2016)
Branch to submit: `upstream-pr/357-keytip-placement`
Fork PR: nidarian/Fluent.Ribbon#1 (your original February fix)

**Fix** (your code, unchanged). `KeyTipAdorner.IsTextBoxShapedControl()` now
also returns true for controls that implement `IKeyTipedControl` but aren't
Fluent ribbon controls. Those get top-left placement instead of the centered
placement meant for large buttons.

**Added today:**

- Merged the current `develop` into the branch. This is a merge commit, not a
  rebase, so your history is intact.
- A unit test in `Fluent.Ribbon.Tests/Adorners/KeyTipAdornerTests.cs`. A custom
  slider with KeyTips is treated as text box shaped. `TextBox` still is. A plain
  `Slider` and a Fluent `Button` are not.

**Evidence:** [run #5](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36395412411)
passed, all tests on all three frameworks.

**Needs a human check on Windows:** the test proves the placement *decision*,
not how the KeyTip looks. Before submitting, run `Fluent.Ribbon.Showcase`, open
the KeyTips tab, group "Issue #357 Test", press Alt, and confirm the KeyTip sits
top-left on the slider. (The fork PR #1 for this was closed without merging; nothing to tick there anymore.)

---

## #1176: ribbon groups can't be scrolled with touch

Issue: https://github.com/fluentribbon/Fluent.Ribbon/issues/1176
Branch to submit: `upstream-pr/1176-touch-scrolling`

**Bug.** When the window is narrow, the ribbon groups scroll with the arrow
buttons and the mouse wheel, but dragging a finger does nothing (1, 2 or 3
fingers, in any direction).

**Cause.** `RibbonGroupsContainerScrollViewer` never sets `PanningMode`, and a
WPF `ScrollViewer` ignores touch drags while it's `None` (the default). This
likely explains why the maintainer couldn't reproduce it: a phone-to-PC touch
forwarding tool may send mouse-wheel events instead of touch.

**Fix** (2 files):

- `PanningMode = HorizontalOnly` in the default style. It has to be a style
  setter: `ScrollViewer` only turns on `IsManipulationEnabled` when
  `PanningMode` *changes*, so a metadata default wouldn't enable touch.
- Swallow `ManipulationBoundaryFeedback`, so dragging past the first or last
  group doesn't make Windows bounce the whole window.

**Evidence (Windows CI):**

| Run | Code | Result |
|---|---|---|
| [#14](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36510887638) | test only, no fix | Fails as expected on all 3 frameworks: *PanningMode expected HorizontalOnly, was None* |
| [#15](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36510890291) | test + fix | All pass |

**Needs a human check:** the test proves touch panning is switched on. It can't
move a real finger. Before submitting, try it on a touch screen: make the
Showcase window narrow and drag across the ribbon groups.

---

## Simplified ribbon ignores SimplifiedStateDefinition (found while looking at #1233)

No issue of its own yet. It's the reason the workaround the maintainer suggested
in https://github.com/fluentribbon/Fluent.Ribbon/issues/1233 doesn't work.
Branch to submit: `upstream-pr/simplified-state-definition-reset`

**Bug.** In the simplified ribbon, a group's starting state is taken from
`StateDefinition` (the classic ribbon's setting) instead of
`SimplifiedStateDefinition`. `RibbonGroupsContainer` only changes a group's state
later if the group is listed in the tab's `ReduceOrder`, and most apps don't set
that. So `SimplifiedStateDefinition="Collapsed"` or `"Middle,Collapsed"` has no
effect: the group stays Large.

**Fix** (`RibbonGroupBox.TryClearCacheAndResetStateAndScale`, 1 line of logic):
start from the first state of the definition for the current mode. Both
defaults start with Large, so nothing changes for apps using the defaults.

**Evidence (Windows CI):**

| Run | Code | Result |
|---|---|---|
| [#18](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36531412624) | tests only, no fix | Fails as expected on all 3 frameworks for `"Collapsed"` and `"Middle,Collapsed"` (*expected Middle, was Large*). The default case and everything else pass. |
| [#19](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36531461643) | tests + fix | 309/309 pass on each framework |

### #1233 Showcase sample (for the discussion, not a feature)

Branch: `feature/issue-1233-overflow-sample` (built in
[run #20](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36531568958); it
needs the fix above). The Simplified Ribbon window of the Showcase gets a last group
on tab 1 with `Header="..."` and `SimplifiedStateDefinition="Collapsed"`,
holding two "rarely used" commands. In the simplified ribbon it's a single "..."
dropdown at the end of the tab. In the classic ribbon it's a normal group, like
in Office.

That covers what #1233 asked for with existing controls. What it doesn't do is
the maintainer's "sub-groups" point (sections from several groups inside one
overflow menu). That would be a real new feature, so it's worth asking whether
this sample is enough before building it.

**Needs a human check:** run the Showcase, open the Simplified Ribbon window,
and look at the end of tab 1. Toggle to the classic ribbon and back.

## Bugs found by the architecture review (no issue of their own)

Found while writing the state diagrams in `.github/architecture/` (see
`FINDINGS.md` there). Each was proven the same way: a tests-only commit fails on
Windows CI, then the fix commit passes on all 3 frameworks.

### Backstage keeps replaced content bound to its visibility

Branch to submit: `upstream-pr/backstage-content-visibility-binding`

**Bug.** `Backstage.OnContentChanged` binds the new content's `Visibility` to
the backstage. Inside the block for the *old* content it cleared the binding on
`e.NewValue`, a copy/paste slip. So replaced content keeps appearing and
disappearing with the backstage.

**Fix** (1 line): clear the binding on `e.OldValue`.

| Run | Code | Result |
|---|---|---|
| [#25](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678264930) | test only | Fails: *expected False, was True* |
| [#26](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678305314) | test + fix | 306/306 on each framework |

### KeyTips in ribbon groups never snap to the group's rows (#572 regression)

Branch to submit: `upstream-pr/keytip-row-snapping`

**Bug.** Commit ddaa57fb (2018, "Fixes #572 by adding row snapping in all
cases") moved the snapping into `KeyTipAdorner.SnapToRowsIfPresent`, which takes
the position as a `Point`. `Point` is a struct, so the method changed its own
copy and the callers kept the unsnapped position. Snapping has done nothing
since then.

**Fix:** the method returns the snapped point, and both callers use it.

| Run | Code | Result |
|---|---|---|
| [#27](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678426690) | test only | Fails: *KeyTip center 18.96 should be on one of the rows 0, 14, 28, 47* |
| [#28](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678474843) | test + fix | 306/306 on each framework |

**Needs a human check:** this moves the KeyTips of all controls inside groups
(small, middle and large) up or down onto the nearest row line. The left/right
position doesn't change. That's
what #572 asked for, but after 8 years people are used to the current spots.
Run the Showcase, press Alt, then a tab's KeyTip, and look at the group KeyTips.

### Up opens a drop down with the first item focused

Branch to submit: `upstream-pr/dropdown-up-key-focus`

**Bug.** `DropDownButton.OnKeyDown` focuses the last item when Up opens the drop
down. But opening had already queued a callback that always focuses item 0, and
it runs afterwards. So Up behaves like Down.

**Fix:** the key handler records which item it wants (`itemIndexToFocusOnOpen`),
and the callback uses it and then resets it.

| Run | Code | Result |
|---|---|---|
| [#30](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678603225) | tests only | Down passes, *Up should focus the last item* fails |
| [#31](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36679578827) | tests + fix | 307/307 on each framework |

### `QuickAccessItems.Clear()` leaves entries in the customize menu

Branch to submit: `upstream-pr/qat-items-clear`

**Bug.** `QuickAccessToolBar.OnQuickAccessItemsCollectionChanged` copies Add,
Remove and Replace into the customize menu (the toolbar's small arrow), but has
no case for Reset, which `ObservableCollection.Clear()` raises. The cleared
entries stay in the menu.

**Fix:** on Reset, remove every `QuickAccessMenuItem` from the menu and insert
the collection's current items again, the way `OnApplyTemplate` fills the menu.
The template's own menu entries aren't `QuickAccessMenuItem`s, so they stay.
Like Remove, it doesn't touch items already on the toolbar.

| Run | Code | Result |
|---|---|---|
| [#38](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36682235757) | test only | Fails: the menu still holds "First" and "Second" after `Clear()` |
| [#39](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36682295012) | test + fix | 306/306 on each framework |

### `RibbonStateStorage` temporary state keeps stale text

Branch to submit: `upstream-pr/state-storage-temporary-truncate`

**Bug.** `SaveTemporary` sets the memory stream's `Position` to 0 and writes,
but never truncates it. The same goes for the copy in `Load`. A shorter state
written over a longer one keeps the old tail ("True,True,True" over
"False,False,False" gives "True,True,Truelse"), so on `LoadTemporary` the last
value doesn't parse and is skipped. The library itself never calls
`LoadTemporary`, so this only affects apps that call it through
`IRibbonStateStorage`.

**Fix** (2 lines): `SetLength(0)` before writing, in both places.

| Run | Code | Result |
|---|---|---|
| [#40](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36682494127) | test only | Fails: *IsSimplified expected True, was False* |
| [#41](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36682530646) | test + fix | 306/306 on each framework |

My first version of the test was wrong. It changed
`ShowQuickAccessToolBarAboveRibbon` between saving and loading, and that
property calls `SaveTemporary` by itself, so it overwrote the saved state. It
failed both without the fix (run #36) and with it (run #37). The final test
leaves that property alone and says why.

### Hidden KeyTips count as a prefix

Branch to submit: `upstream-pr/keytip-hidden-prefix`

**Bug.** An expanded group's own KeyTip (like "ZC") is hidden and can't be pressed: `TryGetKeyTipInformation` only considers visible KeyTips. `ContainsKeyTipStartingWith` didn't check visibility, so typing "Z" counted as the start of "ZC". The service then filtered away every visible KeyTip and swallowed the key. In the Showcase: Alt, K, Z.

**Fix:** the prefix check uses the same conditions as activation (enabled and visible). One line.

| Run | Code | Result |
|---|---|---|
| [#44](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36687765504) | test only | Fails: *Hidden KeyTip "ZC" must not match the prefix "Z"* |
| [#45](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36687859550) | test + fix | 306/306 on each framework |

With this fix, "Z" is treated like any key that matches nothing. At the tab level that currently closes all KeyTips, because of the dead "beep" branch listed in `FINDINGS.md`.

### "Add Gallery to Quick Access Toolbar" is always disabled

Branch to submit: `upstream-pr/qat-gallery-can-execute`

**Bug.** Right-clicking a `Gallery` shows "Add Gallery to Quick Access Toolbar", and `AddToQuickAccessToolBar` supports galleries by adding the control hosting them. But the command's can-execute check first required the Gallery itself to be an `IQuickAccessItemProvider`, which it isn't, so its Gallery branch never ran and the entry was always disabled.

**Fix:** the redirect (Gallery, or menu item without icon, becomes its host control) moved into one helper that both `AddToQuickAccessToolBar` and the can-execute check use. For an icon-less menu item, can-execute now answers for the control that would really be added.

| Run | Code | Result |
|---|---|---|
| [#46](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36687986486) | test only | Fails: *CanExecute expected True, was False* |
| [#47](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688066693) | test + fix | 306/306 on each framework |

### Quick Access elements added before the template never appear

Branch to submit: `upstream-pr/qat-add-before-template`

**Bug.** Apps often restore their toolbar in the window constructor. The Ribbon has no template yet then, so `AddToQuickAccessToolBar` records the element but `QuickAccessToolBar?.Items.Add(...)` does nothing, and `OnApplyTemplate` never adds recorded copies to the new toolbar. The element never shows, and since it counts as added it can't be added again.

**Fix:** when `OnApplyTemplate` creates the toolbar, it adds the recorded copies. Re-templating is unchanged.

| Run | Code | Result |
|---|---|---|
| [#48](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688151084) | test only | Fails: the toolbar is empty |
| [#49](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688230067) | test + fix | 306/306 on each framework |

### Disabling KeyTip handling leaves KeyTips on screen

Branch to submit: `upstream-pr/keytip-detach-terminates`

**Bug.** `KeyTipService.Detach` (run when `IsKeyTipHandlingEnabled` becomes false, and on unload) removes the keyboard and window handlers but didn't end a KeyTip chain that was showing. Nothing could close those KeyTips anymore.

**Fix:** `Detach` terminates a showing chain first: the same clean-up as Escape. Does nothing when no KeyTips are showing.

| Run | Code | Result |
|---|---|---|
| [#50](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688349862) | test only | Fails: KeyTips still visible after `Detach` |
| [#51](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688418462) | test + fix | 306/306 on each framework |

### Enter clicks a SplitButton's disabled button part

Branch to submit: `upstream-pr/splitbutton-enter-disabled-button`

**Bug.** `SplitButton.OnKeyDown` calls `InvokeClick()` on every Enter, and `InvokeClick` doesn't check `IsEnabled`. With `IsButtonEnabled = false` (or a command that can't execute, which disables only the button part), Enter still raised `Click` and toggled `IsChecked`. A bound `Command` itself was still protected by WPF's can-execute check.

**Fix:** only click when the button part is enabled; otherwise Enter just opens the drop down.

| Run | Code | Result |
|---|---|---|
| [#52](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688670103) | test only | Fails: 2 clicks instead of 1 (the enabled baseline clicked once) |
| [#53](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688741470) | test + fix | 306/306 on each framework |

### Unloading an open backstage leaves the ribbon stuck

Branch to submit: `upstream-pr/backstage-unload-while-open`

**Bug.** `Backstage.OnBackstageUnloaded` destroys the adorner, but `IsOpen` stays true and nothing that `Show()` changed on the ribbon is restored: `IsBackstageOrStartScreenOpen` (which hides the Quick Access Toolbar), the window's Esc handler, the tab control's close request, collapsed WindowsFormsHosts. A later close returns early in `Hide()` because there's no adorner. Happens when an app moves the ribbon or swaps window content while the backstage is open.

**Fix:** on unload while open, restore the ribbon before destroying the adorner, and re-arm the existing delayed show so a reload with `IsOpen` still true shows the backstage again (the same rule as a backstage opened before it was loaded). A StartScreen keeps its own "show once" rule.

| Run | Code | Result |
|---|---|---|
| [#54](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688881286) | test only | Fails: *IsBackstageOrStartScreenOpen expected False, was True* after unloading |
| [#55](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36688992864) | test + fix | 306/306 on each framework (the test also reloads and closes) |

### An expanded group keeps its drop down open

Branch to submit: `upstream-pr/groupbox-close-dropdown-on-expand`

**Bug.** `RibbonGroupBox.CoerceIsDropDownOpen` only allows an open drop down while the group is Collapsed or QuickAccess, but it only runs when `IsDropDownOpen` is set. When a collapsed group with an open drop down goes back to a normal state (the ribbon got wider, a reset), an empty drop down stays open under it.

**Fix:** `OnStateChanged` closes the drop down (`SetCurrentValue`) when leaving those states. Not re-coercing on purpose: that would reopen it by itself on the next collapse. The test checks that too.

| Run | Code | Result |
|---|---|---|
| [#56](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36689167900) | test only | Fails: *IsDropDownOpen expected False, was True* |
| [#57](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36689246700) | test + fix | 306/306 on each framework |

### A ribbon in an already small window doesn't collapse

Branch to submit: `upstream-pr/ribbon-collapse-on-load`

**Bug.** `Ribbon.MaintainIsCollapsed` only runs on the owner window's `SizeChanged` (subscribed on Loaded) and when `IsAutomaticCollapseEnabled` changes. The window's first size change happens before the ribbon is loaded, so a window that starts (or is restored) below `MinimalVisibleWidth`/`Height` shows a full ribbon until the user resizes it.

**Fix:** `AttachToWindow` runs `MaintainIsCollapsed` once after subscribing. One line.

| Run | Code | Result |
|---|---|---|
| [#58](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36689585574) | test only | Fails: *IsCollapsed expected True, was False* |
| [#59](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36689653003) | test + fix | 306/306 on each framework |

### Accessibility fixes (screen readers and keyboard)

Found by the accessibility review; details and what's left are in
`.github/architecture/ACCESSIBILITY.md`. Each was proven like the others.

| Fix | Branch to submit | Tests only | With fix |
|---|---|---|---|
| A collapsed group reports "collapsed" to UI Automation while its drop down is open: `RibbonGroupBoxAutomationPeer`'s state now reads `IsDropDownOpen`. | `upstream-pr/uia-groupbox-expand-state` | [#72](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36776740350) | [#73](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36776785882) |
| UI Automation can open a disabled drop down or backstage, or click a SplitButton's disabled part: the peers now throw `ElementNotEnabledException`, like the InRibbonGallery peer. | `upstream-pr/uia-respect-disabled` | [#63](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775121538) | [#67](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775298146) |
| The backstage ("File") button has no automation name: `RibbonControlAutomationPeer` falls back to a string `Header`. | `upstream-pr/uia-backstage-name` | [#64](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775125648) | [#68](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775301012) |
| The ribbon lists its menu twice in the automation tree: already-listed peers are skipped. | `upstream-pr/uia-ribbon-menu-once` | [#65](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775128710) | [#69](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775303570) |
| Closing the backstage without animation leaves focus on hidden content: the non-animated path now focuses the backstage button, like the animated one. | `upstream-pr/backstage-focus-no-animation` | [#70](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775488430) | [#71](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775561237) |

### `ReduceOrder` documentation says the opposite of the code

Branch to submit: `upstream-pr/reduceorder-xml-doc`

**Doc bug.** The XML doc on `RibbonGroupsContainer.ReduceOrder` said entries go
"from the first to reduce to the last to reduce". The code starts at the last
entry and steps backwards. The Showcase's own XAML comment already says it
right. The new doc on both `ReduceOrder` properties says: the last entry is
reduced first, enlarging goes the other way, each entry is one step, `(Name)`
entries scale a control, and groups not listed never shrink. Every sentence was
checked against `RibbonGroupsContainer.cs`. Documentation only, built in
[run #29](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36678547621).

---

## Proposals (add public API, so ask the maintainer first)

These add new public members (a property, an event, a class or a resource key). The code is tested, but whether to add the
API, and under what name, is the maintainer's call. Ask in the issue before
opening a pull request.

### #813: don't focus the first item when a drop down opens

Issue: https://github.com/fluentribbon/Fluent.Ribbon/issues/813
Branch to submit: `upstream-pr/813-focus-first-item`

**Adds** `DropDownButton.FocusFirstItemOnDropDownOpen` (dependency property,
default `true`). `SplitButton` inherits it.

- `true` (default): exactly the current behavior. That code path is unchanged.
- `false`: the drop down opens with nothing focused or highlighted. Focus stays
  on the button, and the first Up/Down key press moves to the last/first item,
  like standard menus opened with the mouse. Escape still closes it.

**Evidence:**

- [Run #9](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36502205599)
  failed, and that was useful. The first version tried to put focus on the popup
  content, but that control (`ResizeableContentControl`) isn't focusable.
- [Run #11](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36503188134)
  passed after the redesign: 307/307 on each framework, 0 inconclusive. The test
  runs a baseline with the default first, so it can't pass by accident if
  keyboard focus doesn't work on the CI machine.

**Depends on the Up-key fix above.** Both change the same open callback, so
`upstream-pr/813-focus-first-item` is built on top of
`upstream-pr/dropdown-up-key-focus`. Submit the Up-key fix first.

**Checked after merging the two:** does `FocusFirstItemOnDropDownOpen="False"`
stop Down/Up from focusing an item? I expected yes. The test
`Keyboard_open_focuses_an_item_even_when_FocusFirstItemOnDropDownOpen_is_false`
passed on the unchanged code ([run #32](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36679860547), 310/310, 0 inconclusive),
so I was wrong. The key handler focuses the item itself, before the callback
runs. A guard written for it was reverted, and the test stays as a regression
guard.

**Found along the way (existing code, not changed):** when every item in the
drop down is disabled, `DropDownButton` falls back to
`Keyboard.Focus(DropDownPopup.Child)`. That call can never succeed, because the
child is a `ResizeableContentControl`, which sets `Focusable = false`. Worth
mentioning to the maintainer.

### #1247: cancel closing the backstage

Issue: https://github.com/fluentribbon/Fluent.Ribbon/issues/1247
Branch to submit: `upstream-pr/1247-backstage-closing`

**Adds** `Backstage.Closing` (`EventHandler<CancelEventArgs>`) and
`protected virtual void OnClosing(CancelEventArgs e)`. Set `e.Cancel = true` to
keep the backstage open, for example until a form is saved. The pattern is the
same as `Window.Closing`.

- It's raised for every user close (Escape, click outside, back button,
  backstage button, commands, KeyTips), because they all go through the internal
  `SetIsOpen`.
- It's not raised when `IsOpen` is set from code, when the backstage is already
  closed, or when `CanChangeIsOpen` is `false`.

**Evidence:** [run #10](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36502318713)
passed. The 3 new tests are in `BackstageTests.cs`.

### #647: automation peer for Spinner

Issue: https://github.com/fluentribbon/Fluent.Ribbon/issues/647
Branch to submit: `upstream-pr/647-spinner-automation-peer`

The issue's checklist still has 4 unticked controls, but only `Spinner` really
lacks a peer. `BackstageTabControl` and `BackstageTabItem` already have one, and
`StartScreen` inherits the `Backstage` one. Worth telling the maintainer so the
checklist can be ticked.

**Adds** `RibbonSpinnerAutomationPeer` (Spinner used the generic
`RibbonControlAutomationPeer` before):

- control type `Spinner`, name from `Header`, like the other ribbon peers
- RangeValue pattern: `Value`, `Minimum`, `Maximum`,
  `SmallChange`/`LargeChange` = `Increment`, `IsReadOnly` when disabled
- `SetValue` follows WPF's `RangeBaseAutomationPeer` rules: it rejects
  disabled controls and out-of-range values, and uses `SetCurrentValue` so a
  binding on `Value` survives
- raises the RangeValue `Value` changed event, so screen readers announce changes

**Evidence:** [run #13](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36510725550)
passed, 311/311 per framework. The 6 new tests are in
`Automation/Peers/RibbonSpinnerAutomationPeerTests.cs`.

### #1265: brush key for the backstage tab focus frame

Issue: https://github.com/fluentribbon/Fluent.Ribbon/issues/1265
Branch to submit: `upstream-pr/1265-backstage-focus-brush`

The unused `BackstageTabControl.Button.MouseOver.Background` brush the
maintainer mentioned was already removed upstream (a7c5a6dd). What's left from
the reporter's request is a resource for the focus frame color.

**Adds** `Fluent.Ribbon.Brushes.BackstageTabItem.Focus.Border`, generated for
every theme. It defaults to `Fluent.Ribbon.Colors.Black`, the color used
before, so nothing changes visually. The `IsKeyboardFocused` trigger in the
`BackstageTabItem` template now uses it. `ReferenceData/vNextResourceKeys.txt`
is updated.

**Evidence:** [run #16](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36511096327)
passed. The tests check that the key exists in Light and Dark with the old
color, and that the template uses it.

---

## Open question for the maintainer (#1251 follow-up)

`QuickAccessMenuItem.OnUnchecked` returns early while the item isn't loaded. So
unchecking an item from code before its menu has ever been opened doesn't
remove it from the toolbar. The guard dates from before 2017 and its reason
isn't recorded. It may protect bindings or state loading, so I left it alone.
Ask whether it's still needed.

---

## Fork maintenance (not for upstream)

- `ci/1176-test-only` only existed to prove the #1176 test fails without the
  fix. Its run is linked above. Delete the branch on GitHub (Branches page, trash
  icon). This session isn't allowed to delete branches.

- 2026-09-29: moved the fork's workflows to Node 24 action versions (checkout v7,
  setup-dotnet v6, upload-artifact v7, download-artifact v8). Build, sync and
  archive all passed.

---

## The other open issues (not worked on)

| Issue | Why not |
|---|---|
| #1279 QAT icon size | Already solved upstream (`QATIconSize`, #1281/#1282). Waiting for a release. |
| #1018, #1270, #708 | Theme/design work (High Contrast, Office look). |
| #803, #962 | Documentation / logo, not code. |
| #1283 | Support question, the maintainer already answered. |

## How to submit once you have permission

1. On GitHub, open a pull request **from** `nidarian/Fluent.Ribbon`, branch
   `upstream-pr/1251-qat-ischecked`, **to** `fluentribbon/Fluent.Ribbon`, branch `develop`.
2. Title: `Fix #1251: show checked QAT items added from code-behind`. Paste the
   #1251 section above as the description.
3. Do the same for `upstream-pr/357-keytip-placement` after the Showcase check,
   and for `upstream-pr/1176-touch-scrolling` after the touch screen check,
   and for `upstream-pr/simplified-state-definition-reset`.
   The same goes for the architecture review bugs: `upstream-pr/backstage-content-visibility-binding`,
   `upstream-pr/keytip-row-snapping` (after the Showcase check),
   `upstream-pr/dropdown-up-key-focus`, `upstream-pr/qat-items-clear`,
   `upstream-pr/state-storage-temporary-truncate`, `upstream-pr/keytip-hidden-prefix`,
   `upstream-pr/qat-gallery-can-execute`, `upstream-pr/qat-add-before-template`,
   `upstream-pr/keytip-detach-terminates`, `upstream-pr/splitbutton-enter-disabled-button`,
   `upstream-pr/backstage-unload-while-open`, `upstream-pr/groupbox-close-dropdown-on-expand`,
   `upstream-pr/ribbon-collapse-on-load`, the five accessibility branches above,
   `upstream-pr/reduceorder-xml-doc`, and `upstream-pr/xml-doc-corrections`
   (41 doc comment fixes and the README SDK line; see `architecture/DOCUMENTATION.md`).
   These have no issue, so the description is the section above. Review them
   yourself first, then say in the pull request that an AI found and wrote
   them and that you reviewed them.
4. For #813, #1247, #647 and #1265, first comment on each issue with the proposed API (the
   "Adds" line above) and ask if it's wanted. Open the pull request only after
   a yes. #813 goes after `upstream-pr/dropdown-up-key-focus` is merged.
