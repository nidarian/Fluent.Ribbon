# Round 12: the last tab selection bindings, and opening a tab with a screen reader

Done 2026-11-03. **AI (Claude) was used** for all of it. Nothing was sent to the
original project.

How: same as round 11. Each fix was proven by a tests-only build that fails in
the new tests on net462, net6.0 and net8.0, then the fix. Blind reviews
(pessimistic and adversarial) ran until two rounds in a row were clean, and a
scope check asked whether any fix went beyond its intent.

## Fixed

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| Removing the selected tab, restoring a minimized ribbon that had no selected tab, and hiding the selected tab while minimized replaced an explicit OneWay binding on a tab's `IsSelected` (the "same pattern, not proven" items of round 11) | After that, the app could no longer select that tab | `fix/tab-isselected-sites-r12` | #41696827415 (stricter tests: #41700836516) → #41699027520, #41699674643 |
| A tab on a minimized ribbon implements Expand/Collapse for UI Automation, but never offered the pattern, so a screen reader could not open it (WCAG 2.1.1, 4.1.2) | With Narrator or NVDA, the tabs of a minimized ribbon could not be opened | `fix/tab-isselected-uia-expand` | #41700673139 → #41702884691 |
| Expand (now reachable) replaced an app's OneWay binding on the tab's `IsSelected` | After a screen reader opened a tab, the app could no longer select it | `fix/tab-isselected-uia-expand` | #41700673139 → #41702884691 |
| Expand and Collapse on a disabled tab acted anyway (found by the review once the pattern was reachable; the other Fluent.Ribbon peers refuse, `ACCESSIBILITY.md`) | Assistive technology could open a disabled tab | `fix/tab-isselected-uia-expand` | #41703458284 → #41703465283 |
| Expand opened a hidden tab, or a contextual tab whose group is hidden; Collapse on a closed tab closed another tab's popup (both found by the reviews) | Assistive technology could open a tab nobody can see, or close the wrong one | `fix/tab-isselected-uia-expand` | #41705833700 → RESULT_HIDDEN |

**Default chosen (owner can change it):** the tab offers Expand/Collapse only
while the ribbon is minimized. On a normal ribbon a tab isn't expanded or
collapsed, and offering it would make screen readers say "expanded" on every
tab. A test guards this default.

Hidden tabs and Expand came up in three review rounds in a row; by the fork's
rule that means re-plan, not track. The tab now uses the tab control's own rule
for which tabs can be picked (visible, and for a contextual tab its group
visible): such a tab doesn't offer the pattern, and Expand ignores it.

Along the way: the first round 12 commit carried a session link, which this fork
forbids. History isn't rewritten here, so the same code went to a new branch
(`fix/tab-isselected-sites-r12`) and the old branch
(`fix/tab-isselected-remaining-sites`) is waiting for the owner to delete it.

## Deferred or ruled out (tracked)

- Still not proven (no failing test): UI Automation **Select** on a tab (goes
  through WPF's own Selector, which keeps bindings) and a click on a tab that has
  no tab control (`RibbonTabItem`, only without a ribbon, still the CLR setter).
- The ribbon's own UI Automation Expand/Collapse (minimize, restore) doesn't
  refuse on a disabled ribbon, unlike the tabs now (older, not tested).
- Nothing tells cached UI Automation clients that the pattern appears or goes
  away when the ribbon is minimized or restored (Narrator and NVDA ask again on
  focus, so they are fine).
- `ExpandCollapseState` still says "Expanded" on a normal ribbon (only readable
  through the pattern, which isn't offered there).
- The same peer implements ScrollItem, which is probably not offered either (not
  tested).

COMBINED_AND_REVIEWS
