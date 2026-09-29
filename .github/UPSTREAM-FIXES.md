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

## Proposals (add public API, so ask the maintainer first)

These two add new public members. The code is tested, but whether to add the
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

---

## Open question for the maintainer (#1251 follow-up)

`QuickAccessMenuItem.OnUnchecked` returns early while the item isn't loaded. So
unchecking an item from code before its menu has ever been opened doesn't
remove it from the toolbar. The guard dates from before 2017 and its reason
isn't recorded. It may protect bindings or state loading, so I left it alone.
Ask whether it's still needed.

---

## Fork maintenance (not for upstream)

- 2026-09-29: moved the fork's workflows to Node 24 action versions (checkout v7,
  setup-dotnet v6, upload-artifact v7, download-artifact v8). Build, sync and
  archive all passed.

---

## The other open issues (not worked on)

| Issue | Why not |
|---|---|
| #1279 QAT icon size | Already solved upstream (`QATIconSize`, #1281/#1282). Waiting for a release. |
| #1176 Touch scrolling | Needs a real touch screen to reproduce. |
| #647, #1018, #1233, #1265, #1270, #708 | Large features or theme/design work. |
| #803, #962 | Documentation / logo, not code. |
| #1283 | Support question, the maintainer already answered. |

## How to submit once you have permission

1. On GitHub, open a pull request **from** `nidarian/Fluent.Ribbon`, branch
   `upstream-pr/1251-qat-ischecked`, **to** `fluentribbon/Fluent.Ribbon`, branch `develop`.
2. Title: `Fix #1251: show checked QAT items added from code-behind`. Paste the
   #1251 section above as the description.
3. Do the same for `upstream-pr/357-keytip-placement`, after the Showcase check.
4. For #813 and #1247, first comment on each issue with the proposed API (the
   "Adds" line above) and ask if it's wanted. Open the pull request only after
   a yes.
