# Round 10: a group stuck shrunk after moving to another tab

Done 2026-11-02. **AI (Claude) was used.** Nothing was sent to the original project.

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| A group that one tab had shrunk kept that state after it moved to another tab with plenty of room (nothing reset it there) | The group stayed a single button or small, although there was room | `fix/group-state-reset-when-moved` | #509 → #510 |

The fix: `RibbonGroupsContainer` resets a `RibbonGroupBox` that is added to it
(the existing `TryClearCacheAndResetStateAndScale`, as after a size change).

The test needed three corrections before it proved anything (#505: `Button`
meant WPF's button; #507 and #508: the narrow container didn't shrink the group,
because the container must be narrowed after it is shown, and `(MyGroup)` in a
ReduceOrder scales galleries while `MyGroup` reduces the group's state). #509 ran
the final test without the fix (on `diag/group-moved`) and failed for the right
reason: the group stayed `Small`.

Two blind reviews: both "safe to merge", no bugs. Their notes (existing
behaviour, not changed): a group `State` set by the app is reset when the group is
added; an open drop down of a collapsed group closes when the app adds or moves a
group in that tab; groups left in the old tab keep their reductions until that
tab is measured again.
