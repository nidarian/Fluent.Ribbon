# Open items: everything not done, not fixed or not tested yet

One page for all loose ends. Status: 2026-10-14, after round 7 (round 7 is on
`integration/next`, see `architecture/ROUND7.md`; the package branch is still at
round 6, build #383). Each item points to the note with the details.
When an item is done, delete it here and record it in that note.

The older lists in `architecture/FINDINGS.md` ("Confirmed, not fixed") and
`architecture/ACCESSIBILITY.md` ("found, not fixed") are **all fixed** (round
4, `ROUND4.md`). Only the items below are still open.

## 1. Proven bugs without a fix yet

None. The tab double-click item was fixed in round 7, and corrected: build #376
had not really proven it (`architecture/ROUND7.md`).

## 2. Known small gaps (real, not fixed)

- Opening an InRibbonGallery's toolbar copy can push `SelectedItem = null` into the app's binding. Needs a redesign of how the copy borrows the items. (`ROUND4.md`, "Not fixed")
- UI Automation Expand/Collapse of the Ribbon (`RibbonAutomationPeer`) can drop an app's OneWay binding on `Ribbon.IsMinimized`. (`ROUND7.md`)

The Spinner and ComboBox toolbar copies, the ApplicationMenu KeyTip and the
StatusBar `Visibility` gaps were fixed in round 7.

## 3. Decisions for the owner

| Question | Details |
|---|---|
| Blurry window and ribbon icons after moving to a screen with different scaling. A fix touches every icon. | `ROUND5.md` |
| The empty strip next to the tabs acts as a title bar (drag, double-click maximizes, right-click shows the window menu); apps can't turn it off. | `ROUND5.md` |
| Contextual group headers follow the order of `ContextualGroups`, not of their tabs. | `ROUND5.md` |
| Gallery filter names `"A, B"` (space after the comma) silently hide B. | `ROUND5.md` |
| The 2012 Walkthrough (`Doc/*.docx`, `.pdf`) is outdated: rewrite, mark outdated, or leave. | `architecture/DOCUMENTATION.md` |

## 4. Suspicions not checked yet (need a test)

All five were tested in round 7 (`architecture/ROUND7.md`): the Backstage reopen
and the moved ribbon were real bugs and are fixed; DismissOnClickOutside, the
RemoveFromQuickAccess command and the StatusBar menu order were ruled out.

Left untested: a ribbon moved from a plain window into a RibbonWindow, and
back-and-forth moves.

## 5. Checks only a person can do (on a Windows PC)

Tests can't use a real mouse, real screens or a screen reader, so these need a
person. Skip the ones already done.

- **Audit checks** (`architecture/AUDIT.md`, "What only a person can check"): KeyTip letters on a collapsed group; colour picker arrow then click; Dark theme text and borders; focus in a drop down that starts with a colour picker; a slider on a touch screen.
- **Two screens or high scaling** (`ROUND5.md`, five steps): sharp icons, dragging to a left-hand screen, maximize on each screen, the Windows 11 snap layout popup, `UseNativeCaptionButtons`.
- **Right-to-left window at 150 % scaling**: ScreenTips line up with their control. (`ROUND6.md`)
- **Showcase "new thread" window**: "More Colors" in both windows, no crash, the colour shows in both. (`ROUND6.md`)
- **High Contrast** (only if an app turns it on): the manual checks in `architecture/HIGH-CONTRAST.md`.

## 6. Not covered on purpose

- High Contrast mode only has basic, opt-in support; the gaps are listed in `architecture/HIGH-CONTRAST.md` ("What is NOT covered").
- Small doc comment wording issues are listed in `architecture/DOCUMENTATION.md` ("Not changed").
