# Round 16: icon-only controls get a name for screen readers

Done 2026-11-10. **AI (Claude) was used** for all of it. Nothing was sent to the
original project.

How: the first item of a read-only audit for accessibility gaps. Most tests were
written first and seen failing, then the fix was written; the empty-header
cases were added after the fix and fail only on the code from before the round
(#618), and one split button test is a guard that passes before and after. Blind reviews
(pessimistic and adversarial) ran until two rounds in a row were clean.

Proof: build **#618** runs all tests of this round on the code from before the
round (tests only). On each of net462, net6.0 and net8.0 it fails in the same 34
test cases and in nothing else: for each of the ten controls a missing header, a
header that isn't text and an empty header (30), the changed title, the launcher
button and the two Spinner text box tests. Build **#613** is the fix with the
same tests; everything passes. #618 lists the failed tests per .NET version in
one line each, because GitHub shows only about ten failure notes per build step;
the earlier tests-only builds (#605, #606, #611, #615 to #617) each showed only
a part for that reason and are superseded by #618.

Two more builds show the tests that the reviews asked for failing on earlier
versions of the fix: #608 on the first version (split button renamed, Spinner
text box unnamed) and #612 on the second (Spinner text box keeps an old title).

## Fixed

| Problem | What a user heard | Branch | Failing build → passing build |
|---|---|---|---|
| A ribbon control without a text `Header` (an icon-only button, common in small groups and on the Quick Access Toolbar) had no name for screen readers, even when its `ScreenTip` has a title like "Bold". The same for a `Header` that is empty or not text | Only "button" | `fix/automation-names-r16` | #618 → #613 |
| A group's dialog launcher button had no name unless the app set `LauncherText` (or a launcher command that has a text) | Only "button" | same | #618 → #613 |
| The Spinner's text box, which takes the keyboard focus, did not get that name, and would have kept an old title after the app changed it (both found by the reviews of this round) | Only "edit", or an outdated name | same | #618, #608, #612 → #613 |

Covered controls: `Button`, `ToggleButton`, `CheckBox`, `RadioButton`,
`DropDownButton` (and `ApplicationMenu`, which is one), `SplitButton`, `Spinner`,
`ComboBox`, `TextBox` and `InRibbonGallery`, and through `Button` the launcher
button of a group (`LauncherToolTip` set to a `ScreenTip`).

The order of names is now: a name the app set explicitly or that WPF finds
itself (a labelling element, text content, the text of a command), then a
non-empty text `Header`, then for a split button its command text or inner
button text (as before), then the `ScreenTip` title. A name that was not empty
before this round stays the same; build #608 proved that for a split button
with a command (the first version of the fix would have renamed it from "Paste"
to the longer ScreenTip title).

A plain text `ToolTip` is not used as a name: WPF already gives it to screen
readers as help text, so it would be read twice.

## Deferred (tracked)

- Not changed in this round: gallery items and menu items (no name from the
  ScreenTip title); the generic `RibbonControl` peer (for example
  `RibbonToolBar`), a collapsed group and a backstage page when their `Header`
  isn't text; the text box inside an editable `ComboBox` (read in the code, not
  tested).
- A `ScreenTip` title that comes from a binding to the control's `DataContext`
  (or a title looked up live from the window's resources) is only reliable while
  the ScreenTip is shown (WPF connects a tooltip to its control only then). Before
  it was first shown the title is empty and the control has no name. Not tested;
  this is how WPF tooltips work. For the Spinner's text box the same applies. The help text
  (`ScreenTip.Text`) always had this limit.
- One `ScreenTip` object shared by several controls through a style, with its
  title bound to the control it is shown for, names each of them after the
  control that showed it last. Give each control its own ScreenTip or a fixed
  title.
- UI Automation is not told separately when only the title changes; it sees the
  new name with the next layout update, like a changed header.
- A copy on the Quick Access Toolbar made before the app assigned the ScreenTip
  doesn't get it (the copy takes the ScreenTip or the header once, older
  behaviour).
- A Spinner with a header like "_Size" reports "_Size" itself and "Size" on its
  text box (older behaviour, only the text box removes the access key marker).
- Not tested: a ScreenTip set by a style, trigger or binding on a Spinner; a
  split button taking its name from its inner button's text; the inner button
  of a split button getting the ScreenTip title; a Quick Access Toolbar copy.
- Two small changes of the last fix commit have no failing-first test of their
  own: a control that nothing names reports the same empty result as before the
  round (`null`, or the empty header), and a Spinner header that is only an
  access key marker ("_") no longer blocks the ScreenTip title for the text box.

`diag/names-r16-green3` (#613) is the fix branch plus only the CI step that
lists failed tests; `diag/names-r16all` (#618) is the code before the round plus
that step, a per-report summary line and the tests.

Blind reviews:

- Round 1 (two reviewers): a split button named by its command would have been
  renamed to the ScreenTip title, and the Spinner's text box stayed unnamed.
  Both fixed, each proven by its own failing test (#608 → #610).
- Round 2 (two reviewers): the Spinner's text box read the title only once
  (fixed, #612 → #613). One claim was checked and rejected: that a split
  button's inner button gets a different name than the split button when it has
  a command (both get the command text; a guard test now pins that).
- Rounds 3 and 4 (one reviewer each, pessimistic then adversarial): safe, no
  bugs. **Two clean rounds in a row.** Their remaining notes are in "Deferred"
  above and in `CHANGES-FOR-APPS.md`.
