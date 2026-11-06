# Round 14: a tab's ScrollItem for screen readers

Done 2026-11-06. **AI (Claude) was used** for all of it. Nothing was sent to the
original project.

How: the fix was proven by a tests-only build that fails in the new test on
net462, net6.0 and net8.0, then the fix. Blind reviews (pessimistic and
adversarial) ran until two rounds in a row were clean.

## Fixed

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| A ribbon tab implements UI Automation ScrollItem (bring the tab into view when the tabs don't fit) but never offered it, like Expand before round 12 (WCAG 4.1.2) | A screen reader or UI test could not scroll a tab into view | `fix/tab-uia-scrollitem` | #566 → #567 |

ScrollItem only scrolls: the tab strip, and an app's own scroll area around the
ribbon if it has one (as with any WPF control). No tab is selected, focus
doesn't move, nothing opens. It is offered for every tab, like the gallery items in this
library and WPF's own list items.

## Deferred (tracked)

- For a hidden tab, or a contextual tab whose group is hidden, ScrollItem is
  still offered; scrolling to it is harmless but meaningless (the tab strip can
  jump). Not tested.
- The test checks that the pattern is offered, not that the tab strip actually
  scrolls (the scrolling code was there before).

`integration/round14` has only this fix on top of round 13, so #567 is also
its combined build (adaf0e2b, built as `diag/tab-scrollitem`, which only adds
the CI step that lists failed tests). Blind reviews:

- Round A (pessimistic and adversarial): safe, no bugs.
- Round B: safe, no bugs. **Two clean rounds in a row.**
