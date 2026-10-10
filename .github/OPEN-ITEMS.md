# Open items: everything not done, not fixed or not tested yet

One page for all loose ends. Status: 2026-10-10, after round 6 (package
build #383, 1179/1179 tests). Each item points to the note with the details.
When an item is done, delete it here and record it in that note.

The older lists in `architecture/FINDINGS.md` ("Confirmed, not fixed") and
`architecture/ACCESSIBILITY.md` ("found, not fixed") are **all fixed** (round
4, `ROUND4.md`). Only the items below are still open.

## 1. Proven bugs without a fix yet

| Bug | Proof | Details |
|---|---|---|
| Double-clicking a tab can cut the tabs off from `Ribbon.IsMinimized`, so the app's "minimized" setting stops working | Tests-only build #376 failed; branch `fix/tab-doubleclick-keeps-minimized-binding` has the test, no fix yet | `ROUND6.md` |

## 2. Known small gaps (real, not fixed)

All the same kind as fixes already made: the control stops following the app's
data (a OneWay binding is replaced) or a value isn't refreshed.

- A Spinner's toolbar copy: its arrows can drop a OneWay `Value` binding on the original. (`ROUND6.md`)
- A ComboBox's toolbar copy can drop a OneWay `Text` binding on the original. (`ROUND6.md`)
- Opening an InRibbonGallery's toolbar copy can push `SelectedItem = null` into the app's binding. Needs a redesign of how the copy borrows the items. (`ROUND4.md`, "Not fixed")
- The ApplicationMenu's default KeyTip doesn't follow a runtime language switch. (`ROUND4.md`, "Not fixed")
- Hiding a status bar item through `Visibility` (not the status bar's menu) doesn't tidy the separators. (`ROUND6.md`)

## 3. Decisions for the owner

| Question | Details |
|---|---|
| Blurry window and ribbon icons after moving to a screen with different scaling. A fix touches every icon. | `ROUND5.md` |
| The empty strip next to the tabs acts as a title bar (drag, double-click maximizes, right-click shows the window menu); apps can't turn it off. | `ROUND5.md` |
| Contextual group headers follow the order of `ContextualGroups`, not of their tabs. | `ROUND5.md` |
| Gallery filter names `"A, B"` (space after the comma) silently hide B. | `ROUND5.md` |
| The 2012 Walkthrough (`Doc/*.docx`, `.pdf`) is outdated: rewrite, mark outdated, or leave. | `architecture/DOCUMENTATION.md` |

## 4. Suspicions not checked yet (need a test)

From code reading only; any of them may turn out fine.

- Reopening the Backstage during its close animation may hide it while `IsOpen` is true. (`FINDINGS.md`, "Still open")
- `DropDownButton.DismissOnClickOutside = false` keeps mouse capture, so clicks elsewhere do nothing until Esc. Maybe intended. (`FINDINGS.md`)
- `RemoveFromQuickAccessCommand` with a parameter that isn't on the toolbar could throw if WPF skipped its can-execute check (it normally doesn't). (`FINDINGS.md`)
- A StatusBar item added at runtime may land one place too early in the "Customize" menu; round 6 reading says the menu rebuild puts it right, but it was never tested. (`ROUND4.md`, `ROUND6.md`)
- A ribbon moved from one window to another (memory and title bar). (`MEMORY-LEAKS.md`)

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
