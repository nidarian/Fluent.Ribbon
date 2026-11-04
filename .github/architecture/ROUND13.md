# Round 13: disabled ribbon and groups, and CanMinimize, for screen readers

Done 2026-11-04. **AI (Claude) was used** for all of it. Nothing was sent to the
original project. The disabled ribbon and group were found by the round 12
reviews, the `CanMinimize` gap by a round 13 reviewer.

How: same as rounds 11 and 12. Each fix was proven by a tests-only build that
fails in the new tests on net462, net6.0 and net8.0, then the fix. Blind
reviews (pessimistic and adversarial) ran until two rounds in a row were clean.

## Fixed

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| UI Automation could minimize or restore a **disabled** ribbon, and open or close a **disabled** collapsed group (UI Automation requires `ElementNotEnabledException`, as the other Fluent.Ribbon controls already do, `ACCESSIBILITY.md`) | Assistive technology or UI tests could act on a disabled ribbon or group | `fix/uia-disabled-ribbon-and-group` | #556 → #557 |
| UI Automation could minimize and restore the ribbon while the app had turned `CanMinimize` off; the tab double-click, the display options menu and Ctrl+F1 don't | Assistive technology could change a setting the app had locked | `fix/uia-ribbon-respects-canminimize` | #558 → #559 |

**Default chosen (owner can change it):** with `CanMinimize` off, UI Automation
can neither minimize nor restore, like the keyboard. A screen reader user is
not worse off than a keyboard user: on a minimized ribbon each tab can still be
opened (round 12).

The decision changed twice. First restore was blocked. Round B said that
leaves a screen reader user stuck, because a mouse user can still restore by
clicking a contextual group header, so restore was allowed. Round C showed that
the keyboard can't restore either and that the tabs stay reachable, so restore
was blocked again (with a new commit, history not rewritten). The
contextual header click is left as a decision for the owner.

## Deferred (tracked)

- A mouse click on a contextual group header restores a minimized ribbon even
  with `CanMinimize` off (`RibbonContextualTabGroup`, older). Decision for the
  owner; default: leave it.
- With `CanMinimize` off, the ribbon still offers Expand/Collapse to UI
  Automation and quietly does nothing; it could throw instead. Decision for the
  owner; default: quiet, like the other Fluent.Ribbon peers in a state where the
  action doesn't apply.
- If a collapsed group is disabled while its drop down is open, UI Automation
  can't close it through the group (it refuses, as UI Automation requires).
  Escape probably doesn't close it either (keyboard input doesn't reach a
  disabled element); a click outside probably does. Not checked.
- The disabled tests stop at the first action (the ribbon's Collapse, the
  group's Expand), so the second action's refusal is shown by the passing runs
  only.

Combined build (`integration/round13`, bfc562f8, on top of round 12, built as
`diag/round13`, which only adds the CI step that lists failed tests): **#563
green** on net462, net6.0 and net8.0. Blind reviews (pessimistic and
adversarial each round):

- Round A (disabled ribbon and group): no bugs in the change; one reviewer
  found the `CanMinimize` gap, fixed above.
- Round B (not clean): restore should be allowed; the code was changed.
- Round C (not clean): the opposite view; restore was blocked again.
- Rounds D and E: safe, no bugs. **Two clean rounds in a row.**
