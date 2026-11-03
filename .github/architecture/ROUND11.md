# Round 11: accessibility queue and the remaining binding items

Done 2026-11-02. **AI (Claude) was used** for all of it. Nothing was sent to the
original project.

How: the plan was attacked first by a blind pessimistic reviewer, which cut two
ideas (a ColorGallery peer and English colour names: wrong for other languages)
and widened one (tab selection). Each fix was proven by a tests-only build that
fails in the new tests on net462, net6.0 and net8.0, then the fix. A scope check
asked whether any fix went beyond its intent; two blind reviews checked the
combined round.

## Fixed

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| "Use classic ribbon" / "Use simplified ribbon" replaced an app's OneWay binding on `Ribbon.IsSimplified` (same bug as "Minimize ribbon", fixed in round 7's independent review) | After using the menu, the app's setting no longer switched the ribbon | `fix/display-options-keeps-simplified-binding` | #516 → #519 |
| A contextual tab group ("Table Tools") had no automation peer; its header TextBlock is hidden from the control view (WCAG 1.3.1) | Screen readers never announced the context of contextual tabs | `fix/a11y-contextual-group-name` | #517 → #521 |
| The End/Home/Ctrl+Tab keys, the mouse wheel and a contextual header click replaced an explicit OneWay binding on a tab's `IsSelected` | After using them, the app could no longer select that tab | `fix/tab-isselected-keeps-oneway-binding` | #523 → #526 |
| The ColorGallery colour lists had no names (WCAG 1.3.1) | A screen reader said "list" instead of "Theme Colors, list" | `fix/a11y-colorgallery-lists-and-menu-headings` | #518 → #524 |
| A control wrapped in an item DataTemplate (a Border around the button) didn't get the simplified state | The button stayed in the classic look in the simplified ribbon | `fix/simplified-wrapped-template-children` | #528 → #529 |
| UI Automation Toggle on the four display options items only changed the check mark (a regression of the fixes from round 7's independent review and round 11, found by the review) | UI tests or assistive technology that toggle these items didn't change the ribbon | `fix/display-options-uia-toggle` | #531 → #532 |

Along the way: the first tab test (#515) threw before it reached the code (a
mouse wheel event without its RoutedEvent); the corrected test failed for the
right reason in #523. A click on a tab already kept the binding (the test passed
without a fix, kept as a guard), so the fix was narrowed to the proven entry
points. The first version of the wrapped-template fix also walked into Panels;
the scope check called that beyond the intent, and it was removed.

## Deferred or ruled out (tracked)

- The display options headings ("Show Ribbon", "Ribbon Layout") are disabled menu
  items, announced as unavailable: WPF has no `AccessibilityView` (that is
  UWP/WinUI), so hiding them needs a new element type. Not done.
- Expanded groups announce their KeyTip: fixing it contradicts an existing test.
- ColorGallery headings are hard-coded English (visible, not only accessibility);
  in "highlight colours" mode one list is announced as "Standard Colors"; the
  gradient lists have no name.
- Tab `IsSelected`, same pattern but not proven (no failing test): selecting the
  next tab after a removal, `SelectFirstTab`, hiding a tab while minimized,
  UI Automation Select, a click on a tab (passes already).
- Controls added later to a Panel nested in a group aren't simplified: there is no
  safe hook for a nested Panel's children.
- A RibbonTabControl with its own style without the IsSimplified binding: the
  check marks of the simplified items can show the wrong state.

Combined build of the round (`integration/round11`, c247c6ef): **#534 green** on net462, net6.0 and net8.0. Blind reviews (pessimistic and adversarial each round):

- Round A (four fixes): safe, no bugs; one real finding, UI Automation Toggle on
  the display options items, fixed and proven above.
- Round B (all six fixes): safe, no bugs.
- Round C (all six fixes): safe, no bugs. **Two clean rounds in a row.**

Their remaining notes (low likelihood, known): `UpdateTarget` runs inside the
Checked event, which WPF tolerates (the least proven spot); a runtime theme swap
could in theory hand a default value to the menu handlers; the "no ribbon"
simplified path doesn't check CanUseSimplified; Decorator roots such as Viewbox
now also pass the group's size to their child (the intent, but visible for apps
that sized that child themselves); the contextual group name isn't refreshed by
an event when the header changes at runtime; the two gradient colour lists have
no name.
