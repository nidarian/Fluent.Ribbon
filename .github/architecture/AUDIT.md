# Audit of the fork's fixes (2026-10-06)

The fixes in this fork were written with AI (Claude). The original project's
maintainer doubts that AI fixes help rather than make things worse. That is a
fair question, so this file is an honest check of the work: what held up,
what didn't, and what should be done about it. It reports problems as plainly
as successes.

**Who did it, and the limits.** Five AI reviewers (Claude), working separately
from the sessions that wrote the fixes, read the code. Four reviewed the code by
area, one checked the evidence against GitHub's records. They could only read
code: no reviewer built or ran anything. An AI checking AI work is **not** an
independent human review. What is solid here is what anyone can re-check: the
build runs, the file and line references, and the hands-on checks at the end.

What was compared: the original code (`develop`, upstream `8572cb2`) against
all fixes combined (`integration/all-fixes`, `6828bf41`): 93 library and test
files, about 80 fix branches.

## Summary

| Question | Answer |
|---|---|
| Is the "fail then pass" evidence real? | **Yes.** All 72 cited fail → pass pairs exist on GitHub with the stated results. Every fail commit changes only tests, every fix commit only code. 13 fail logs were opened: each shows the new test failing, with the predicted message, on net462, net6.0 and net8.0, and nothing else failing. |
| Does the combined build pass? | **Yes.** #201 (`integration/next`) and #202 (`integration/all-fixes`, exact head): 902 tests on net6.0 and net8.0. On net462, 1-2 focus tests end as "inconclusive" (not failed, not passed). |
| Is every fix in the app package? | **Yes.** All 82 fix/feature/docs branches are in `integration/all-fixes` (one as equivalent cherry-picks). |
| Were existing tests weakened to get green? | **No.** No existing assert was removed or loosened in any area. |
| Are the fixes right? | **Most are.** About 85 fixes were rated; most are OK. But 4 real problems and about 15 concerns were found that the tests did not catch. See below. |

The lesson: a test that fails before and passes after proves the bug **it
tests** is fixed. It does not prove the fix breaks nothing else. Several
problems below are side effects the tests were never written to look for.

## Problems (should be fixed before relying on them)

| # | What goes wrong for a user | Where | Caused by | Recommendation |
|---|---|---|---|---|
| P1 | When the window is narrow and groups shrink to one button, their KeyTips (Alt-key letters) move to the top-left corner of the group instead of bottom-centre. Gallery item KeyTips move too. | `KeyTipAdorner.cs` `IsTextBoxShapedControl`: `element is IKeyTipedControl && element is not IRibbonControl` also matches Fluent's own `RibbonGroupBox` and `GalleryItem`. Checked by hand. | #357 (Feb 2026 fix) | Limit the rule to controls outside the library, add tests for `RibbonGroupBox` and `GalleryItem`. |
| P2 | In a color picker, move to a color with the arrow keys, then click that highlighted color: nothing happens and the picker stays open. | `ColorGallery.cs:836`, `:1020`. Clicking an already-selected list item raises no selection change. | `colorgallery-arrow-keys-browse` | Commit on mouse click too, or browse by moving focus without selecting. Add a keyboard-then-mouse test. |
| P3 | With Windows High Contrast **and** the opt-in High Contrast support on, in a light High Contrast theme (White, Desert), a gallery's filter label is black on black. | `Theme.Template.xaml:193` hard-codes black text; `RibbonLibraryThemeProvider.cs` maps the label's background (`Gray3`) to WindowText. | `contrast-wcag` and `high-contrast-basic`, each fine alone | Make the label colours High-Contrast-aware; add a test that checks colour pairs under the High Contrast palette. |
| P4 | In a collapsed group's drop down, Enter typed into a text box is swallowed: window-level Enter shortcuts and default buttons stop working. | `RibbonGroupBox.cs:1062` handles Enter without checking where it came from. | `groupbox-enter-gallery-space` | Only handle keys pressed on the group itself (same fix as `fix/dropdown-keys-from-popup-content`, below). |

## Concerns (likely or possible, and smaller)

Behaviour changes that apps may notice:

- **Text colour on hover overrides the app's colour.** `Button.xaml:112,125,203,217`, `ToggleButton.xaml:119,133`: a button with `Foreground="Red"` turns to the theme colour while hovered or pressed. Apps with hand-written theme dictionaries lack the new keys (`CheckBox.CheckMark`, `Button.MouseOver.Foreground`, `Button.Pressed.Foreground`); the check mark may disappear. *(contrast-wcag)*
- **Dark themes: text box borders got less visible, not more.** `Theme.Template.xaml:297` Gray6 → Gray2. In Dark, contrast on the ribbon falls from 8.70:1 to 3.49:1. ROUND4.md says "lighter in Dark", which is wrong. Recommend: change Light only.
- **Theme colours changed more than the notes say.** Light.Blue check marks and focus borders (#0078D7 → #0048A3), Dark.Blue toggle borders, button hover text in 8 colour schemes, and the Light.Yellow palette (`AccentDark3` #B19802 → #9F8902), which also darkens Light.Yellow contextual tab text. Not in Changelog.md.
- **Automated UI tests may break**, not flagged as breaking: Backstage tabs ListItem → TabItem, Spinner Custom → Spinner, the Quick Access Toolbar no longer appears under the Ribbon element, and the DropDownButton → Button change also hits ApplicationMenu and the toolbar's own buttons.
- **KeyTip row snapping now really runs** and uses one group's rows for the whole level, including drop down popups: popup KeyTips may pile up on one line. Needs a visual check. *(keytip-row-snapping)*
- **A wrong KeyTip key beeps instead of closing** also on the first level of an open Backstage, ApplicationMenu and StartScreen, and Esc then closes the Backstage. *(keytip-wrong-key-nested-level-keeps-keytips)*
- **Cancelling `Backstage.Closing`** (new event, #1247): pressing Esc in KeyTip mode then shows the ribbon's KeyTips on top of the still-open Backstage; a screen reader's Collapse is cancelled silently.
- **Touch panning is on for every app.** A horizontal touch drag on a slider in the ribbon may scroll the ribbon instead. Needs a touch screen. *(#1176)*
- **Opening a drop down that starts with a ColorGallery** (the Showcase setup) may put focus somewhere unexpected. Needs a run. *(empty-tab-stops + dropdown-disabled-items-focus)*
- **Toolbar copy of a TextBox** commits an `UpdateSourceTrigger=Explicit` binding on focus loss. *(textbox-qat-copy-updates-source)*
- **`IsCollapsed="True"`** set by an app is undone at load in a large window. *(ribbon-collapse-on-load)*
- **Gallery `SelectedFilter`** that isn't in `Filters` is now replaced by the first filter. *(gallery-filters-reset-and-removed)*
- Smaller: re-templating re-pins toolbar items in a different order; `ObjectToImageConverter` reads "16,5" as 165 under de-DE when passed from code; the screen reader's access key is only the last KeyTip letter; with `AutomaticStateManagement=false` the first tab may be re-selected on every Loaded; custom themes lack the new `BackstageTabItem.Focus.Border` key.

## Test quality

- Each new test would fail on the original code for the stated reason (judged by reading; the fail runs confirm it for the cited ones).
- Weak spots: the Shift+Tab test raises keys by hand, so WPF's own Tab navigation never runs; the DPI test repeats the code's formula; the High Contrast tests restate the mapping table and never check a colour pair; focus tests that lose window focus end as "inconclusive", which CI does not count as a failure (watch the count on net462).
- Four merge conflicts were resolved in library code (`RibbonControlAutomationPeer`, `Backstage.cs`, `DropDownButton.cs` twice). Only the combined builds test those versions. One was spot-read and looks right.

## Small errors in the notes

- #1176's fail run (#14) ran on `ci/1176-test-only`, not the fix branch. The code matches.
- `dropdown-disabled-items-focus` and `colorgallery-arrow-keys-browse` got a later test commit; their current heads passed in #190 and #189.
- `feature/issue-813-focus-first-item`'s head has no run on its exact commit; the difference from run #32 is comments only.
- ROUND4.md says 4 tests-only runs failed to compile; #134 and #182 were fix commits. #186 failed on two frameworks, not one.
- Contrast fail run #131 is 208 failing themed cases (9 test methods × 46 themes), not a single test.

## New fix from this audit

`fix/dropdown-keys-from-popup-content`: Enter pressed in a text box inside an
open drop down closed it, and in a SplitButton also clicked the main button.
Space was suspected too, but its test passed on the original code, so Space was
never part of the bug.

| Run | Code | Result |
|---|---|---|
| #207 | tests only | Failed on all 3 frameworks: the 2 Enter tests only. 308 others passed. |
| #208 | tests + fix | All passed |
| #209 | + comment correction | All passed |

Not yet in `integration/all-fixes`.

## What only a person can check

The reviewers could not run anything. These need someone at a Windows PC with
the app (`APP-REVIEW-CHECKLIST.md` explains how to install the fixed package):

1. Make the window narrow until groups collapse, press Alt then a tab's letter: where do the group letters appear? (P1)
2. Open a color picker, press the right arrow, then click the highlighted color. (P2)
3. Turn on Windows High Contrast (White theme), open a gallery with filters. (P3; only if the app opts in)
4. Hover a button in a Dark theme: is the text readable, and are text box borders visible?
5. Open a drop down whose first item is a color picker: where does keyboard focus go?
6. On a touch screen: drag a slider in the ribbon.

## Recommendation

- Keep the package as it is for now: the problems are real but narrow, and the
  fixes it contains are mostly sound.
- Fix P1-P4 the same way as before (test first, then fix), each on its own
  branch, then build the combined branch again.
- Correct ROUND4.md where it is wrong (Dark text box borders) and list the
  visible colour changes and UI-test-breaking changes in one place.
- Do the hands-on checks above before relying on the package in a release.
