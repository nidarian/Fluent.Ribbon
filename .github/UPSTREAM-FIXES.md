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

## The other open issues (not worked on, to save usage)

| Issue | Why not now |
|---|---|
| #1279 QAT icon size | Already solved upstream (`QATIconSize`, #1281/#1282). Waiting for a release. |
| #1247 Cancel backstage close | The maintainer called it "a good addition". It adds public API, so agree the design first. |
| #813 Disable auto focus of first menu item | Small feature. Adds public API, so agree the design first. |
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
