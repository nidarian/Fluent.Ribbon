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
top-left on the slider. Then tick the test plan in the fork PR.

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
4. For #813, #1247, #647 and #1265, first comment on each issue with the proposed API (the
   "Adds" line above) and ask if it's wanted. Open the pull request only after
   a yes.
