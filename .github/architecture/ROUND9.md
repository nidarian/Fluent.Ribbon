# Round 9: queued items from round 8

Done 2026-11-01. **AI (Claude) was used** for all of it. Nothing was sent to the
original project. Same method: each fix proven by a tests-only build that fails in
the new tests on net462, net6.0 and net8.0, then the fix, whose build passes; two
blind reviews (pessimistic and adversarial) of the combined round.

## Fixed

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| KeyTips and ScreenTips didn't reach UI Automation for `Fluent.MenuItem` (no automation peer of its own), `GalleryItem` and `RibbonGroupBox` (WCAG 4.1.2) | A screen reader didn't announce the keyboard shortcut or the help text of menu items, gallery items and collapsed groups | `fix/a11y-keytips-menuitem-galleryitem-groupbox` | #485 → #488 |
| The gallery filter menu replaced an app's OneWay binding on `SelectedFilter` (Gallery and InRibbonGallery; the property isn't TwoWay by default) | After picking a filter, the app could no longer choose the filter | `fix/gallery-selectedfilter-keeps-oneway-binding` | #495 → #497 |
| The Showcase "..." overflow group had no accessible name (WCAG 1.1.1) | A screen reader announced "dot dot dot"; apps copying the sample did the same | `docs/overflow-sample-accessible-name` | sample only |

Combined on `integration/round9` (on top of round 8): running (recorded here when done).

## Blind reviews

Both reviews: **safe to merge, no bugs.** Their notes, not changed in this round:

- An expanded group (not collapsed) also announces its KeyTip, which only works
  while the group is collapsed or on the Quick Access Toolbar. Slightly misleading.
- When a menu item's header has an access key ("_Save"), that key is announced, not
  the KeyTip (same as before).
- If the app changes `SelectedFilter` while the Quick Access copy's drop down is
  open, closing it writes the copy's filter back (same as before).
- One assertion in `GallerySelectedFilterBindingTests` would also pass through
  coercion; the test still fails without the fix through its binding check.
