# Round 12: the remaining tab selection bindings, and opening a tab with a screen reader

Done 2026-11-03. **AI (Claude) was used** for all of it. Nothing was sent to the
original project.

How: same as round 11. Each fix was proven by a tests-only build that fails in
the new tests on net462, net6.0 and net8.0, then the fix. Blind reviews
(pessimistic and adversarial) ran until two rounds in a row were clean, and a
scope check asked whether any fix went beyond its intent.

## Fixed

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| Removing the selected tab, restoring a minimized ribbon that had no selected tab, and hiding the selected tab while minimized replaced an explicit OneWay binding on a tab's `IsSelected` (the "same pattern, not proven" items of round 11) | After that, the app could no longer select the tab the ribbon chose (the one selected instead, the first tab, or the hidden one) | `fix/tab-isselected-sites-r12` | #537 (stricter tests: #543) → #540, #541 |
| A tab on a minimized ribbon implements Expand/Collapse for UI Automation, but never offered the pattern, so a screen reader could not open it (WCAG 2.1.1, 4.1.2) | With Narrator or NVDA, the tabs of a minimized ribbon could not be opened | `fix/tab-isselected-uia-expand` | #542 → #544 |
| Expand (now reachable) replaced an app's OneWay binding on the tab's `IsSelected` | After a screen reader opened a tab, the app could no longer select it | `fix/tab-isselected-uia-expand` | #542 → #544 |
| Expand and Collapse on a disabled tab acted anyway (found by the review once the pattern was reachable; the other Fluent.Ribbon peers refuse, `ACCESSIBILITY.md`) | Assistive technology could open a disabled tab | `fix/tab-isselected-uia-expand` | #545 → #546 |
| Expand opened a hidden tab, a contextual tab whose group is hidden, or a tab of a collapsed ribbon (narrow window); Collapse on a closed tab closed another tab's popup (both found by the reviews) | Assistive technology could open a tab nobody can see, or close the wrong one | `fix/tab-isselected-uia-expand` | #548, #550 (Expand step), #552 (collapsed ribbon) → #549, #553 |

**Default chosen (owner can change it):** the tab offers Expand/Collapse only
while the ribbon is minimized. On a normal ribbon a tab isn't expanded or
collapsed, and offering it would make screen readers say "expanded" on every
tab. A test guards this default.

Hidden tabs and Expand came up in three review rounds in a row; by the fork's
rule that means re-plan, not track. The tab now uses the tab control's own rule
for which tabs can be picked (on screen, and for a contextual tab its group
visible): such a tab doesn't offer the pattern, and Expand ignores it.

Along the way: the first round 12 commit carried a session link, which this fork
forbids. History isn't rewritten here, so the same code went to a new branch
(`fix/tab-isselected-sites-r12`) and the old branch
(`fix/tab-isselected-remaining-sites`) is waiting for the owner to delete it.

## Deferred or ruled out (tracked)

- Still not proven (no failing test): UI Automation **Select** on a tab (goes
  through WPF's own Selector, which keeps bindings), `SelectFirstTab` (sets the
  tab control's `SelectedItem`, so it goes through the Selector too) and a click
  on a tab that has no tab control (`RibbonTabItem`, only without a ribbon, still
  the CLR setter).
- The ribbon's own UI Automation Expand/Collapse (minimize, restore) doesn't
  refuse on a disabled ribbon, unlike the tabs now (older, not tested).
- Nothing tells cached UI Automation clients that the pattern appears or goes
  away when the ribbon is minimized or restored (Narrator and NVDA ask again on
  focus, so they are fine).
- Only the Expand half of the disabled-tab fix was seen failing: the test stops
  at Expand, so the Collapse refusal is shown by the passing run only.
- Expand on a tab that isn't shown, or on a ribbon that isn't minimized, quietly
  does nothing (for a client that kept the pattern from earlier).
- If the open tab becomes disabled, Collapse through that tab refuses (UI
  Automation's rule); Escape still closes the popup.
- A contextual tab whose group is `Hidden` (not `Collapsed`) is still drawn but
  doesn't offer the pattern, like the tab control's own rule (rare).
- When Expand moves the open popup from one tab to another, no state change
  event is raised for either tab (the popup stays open); a client that caches
  the state shows it stale until it asks again (older).
- `ExpandCollapseState` still says "Expanded" on a normal ribbon (only readable
  through the pattern, which isn't offered there).
- The same peer implements ScrollItem, which is probably not offered either (not
  tested).

Combined build of the round (`integration/round12`, as `diag/round12`):
**#553 green** on net462, net6.0 and net8.0. Blind reviews (pessimistic and
adversarial each round):

- Round A (tab sites): one real finding, the tests only checked that the binding
  exists; they now also check that the app can still select the tab.
- Rounds B and C (tab sites): safe, no bugs.
- Round A of the screen reader fix: a disabled tab could be opened (fixed above).
- Rounds B to D (whole round): hidden tabs, Collapse on the wrong tab and a
  collapsed ribbon (fixed above; the hidden-tab finding came three times, so the
  fix was re-planned).
- Rounds E and F (whole round): safe, no bugs. **Two clean rounds in a row.** A
  last review of these notes found missing app notes and wrong run numbers, now
  corrected.
