# Round 16: icon-only controls get a name for screen readers

Done 2026-11-10. **AI (Claude) was used** for all of it. Nothing was sent to the
original project.

How: the first item of a read-only audit for accessibility gaps. It was proven
by tests-only builds that fail in the new tests on net462, net6.0 and net8.0,
then fixed. Blind reviews (pessimistic and adversarial) ran until two rounds in
a row were clean.

In the table, #605, #606 and #611 (and #615 to #617, below) are tests-only
builds on the code before this round. #608 and #612 are builds of an earlier version of the fix with the new
tests a review asked for (two in #608, one in #612); they fail in those only.

#605 holds 22 new failing cases per .NET version, but GitHub shows only about
ten failure notes per build. The ten shown cover a control without a header
(`Button`, `TextBox`, `SplitButton`, `DropDownButton`, `Spinner`), a header that
isn't text (`RadioButton`, `DropDownButton`, `CheckBox`, `ToggleButton`) and the
changed title. #606 re-ran three cases that were cut off: `ComboBox` and
`InRibbonGallery` without a header, and the launcher button. The remaining
nine combinations got three more tests-only builds, each small enough to show
every failure three times (once per .NET version): #615 (`ToggleButton`,
`CheckBox`, `RadioButton` without a header), #616 (`Button`, `SplitButton`,
`Spinner` with a header that isn't text) and #617 (`ComboBox`, `TextBox`,
`InRibbonGallery` with a header that isn't text). So every new failing case was
seen failing on the code before this round.

## Fixed

| Problem | What a user heard | Branch | Failing build → passing build |
|---|---|---|---|
| A ribbon control without a text `Header` (an icon-only button, common in small groups and on the Quick Access Toolbar) had no name for screen readers, even when its `ScreenTip` has a title like "Bold". The same for a `Header` that is empty or not text | Only "button" | `fix/automation-names-r16` | #605, #606, #615 to #617 (cases #605's list cut off) → #607; #611 (empty header) → #610 |
| A group's dialog launcher button had no name unless the app set `LauncherText` (or a launcher command that has a text) | Only "button" (when reached with the screen reader's own navigation; Tab doesn't stop on it) | same | #606 → #607 |
| The Spinner's text box, which takes the keyboard focus, did not get that name, and would have kept an old title after the app changed it (both found by the reviews of this round) | Only "edit", or an outdated name | same | #608 → #610; #612 → #613 |

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

- Not changed in this round, same gap: gallery items, menu items, the generic
  `RibbonControl` peer (for example `RibbonToolBar`), a collapsed group and a
  backstage page with a `Header` that isn't text; the text box inside an
  editable `ComboBox`.
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

Build of the branch (built as `diag/names-r16-green3`, which only adds the CI
step that lists failed tests): **#613 green** on net462, net6.0 and net8.0.
GitHub caps the failure notes of one step at about ten (see above for what
#605 and #606 show).

Blind reviews:

- Round 1 (two reviewers): a split button named by its command would have been
  renamed to the ScreenTip title, and the Spinner's text box stayed unnamed.
  Both fixed, each proven by its own failing test (#608).
- Round 2 (two reviewers): the Spinner's text box read the title only once
  (fixed, #612 → #613). One claim was checked and rejected: that a split
  button's inner button gets a different name than the split button when it has
  a command (both get the command text; a guard test now pins that).
- Rounds 3 and 4 (one reviewer each, pessimistic then adversarial): safe, no
  bugs. **Two clean rounds in a row.** Their remaining notes are in "Deferred"
  above and in `CHANGES-FOR-APPS.md`.
