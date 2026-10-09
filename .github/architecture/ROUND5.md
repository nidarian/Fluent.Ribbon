# Round 5: the window, contextual tabs and galleries

Done 2026-10-09. **AI (Claude) was used** for all of it. Nothing was sent to the
original project.

Three parts of the library that earlier rounds hadn't reviewed: the
`RibbonWindow` (title bar and caption buttons), contextual tabs and tab
selection, and galleries.

## How each one was proven

1. A read-only search per area. It only reports problems it would bet on.
2. For each problem, a tests-only commit whose Windows build fails in the new
   test on net462, net6.0 and net8.0, with the predicted message.
3. Then the fix, whose build passes all tests.

Combined builds: **#337** (the first 10 fixes, 1073/1073 on each framework)
and **#339** (all 11, the pull request's code).

## Fixed

### Window

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| `ShowMinButton` / `ShowMaxRestoreButton` were ignored | The app hid the button, but it was still there, and a double-click on the title still maximized | `fix/window-caption-buttons-show-properties` | #313 → #324 |
| `UseNativeCaptionButtons="True"` showed two sets of buttons | The ribbon's buttons were drawn on top of Windows' own | same | #313 → #324 |
| Right-click on the app's own title bar item opened the Windows window menu | "Restore / Move / Size / Close" instead of the app's menu | `fix/window-commands-rightclick-own-items` | #325 → #334 (#311 was a too-weak first test) |
| Caption button names ended in about 250 invisible NUL characters (since upstream 178ca934, May 2025) | Screen readers and UI tests couldn't find "Close" or "Minimize" by name | `fix/window-commands-names-trailing-nul` | #321 → #333 (#309 didn't compile) |

### Contextual tabs and tab selection

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| `Tabs.Move` and `Tabs[i] = x` weren't mirrored into the tab control | Tabs kept the old order. Clicking a tab could select another one, and replacing the selected tab **crashed the app** (stack overflow). Toolbar and quick access items were affected the same way. | `fix/tabs-sync-move-replace` | #330 → #335 (#320 crashed the test host) |
| `SelectedTabIndex` went stale after a tab was inserted or removed before the selected tab | An app selecting tabs by number couldn't select the new tab | `fix/selectedtabindex-stale-after-insert` | #314 → #323 |
| Removing a contextual tab misplaced its group header | The header moved to the left edge of the title bar, over the toolbar, and an empty group kept its header | `fix/contextual-header-removed-tab` | #336 → #338 (#319, #331: test setup) |
| Contextual tabs got separator lines in a narrow window (a missing `else`) | Thin divider lines on the coloured tabs | `fix/contextual-tab-separator-narrow` | #312 → #322 |
| Ctrl+Tab, Home and End followed the list order, not the screen order | With a normal tab listed after a contextual one, Ctrl+Tab jumped around | `fix/tab-keyboard-order-matches-screen` | #315 → #328 |

### Galleries

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| An in-ribbon gallery stayed a button after the window was widened again (since 82351d86, 2018) | The gallery stayed a big button, or came back with one column fewer | `fix/inribbongallery-uncollapse-on-enlarge` | #318 → #327 |
| `ColorGallery` replaced an app's OneWay `SelectedColor` binding at the first pick | The picker stopped following the app, for example the colour of the text at the cursor | `fix/colorgallery-keeps-oneway-binding` | #317 → #326 |
| An in-ribbon gallery kept an old snapshot after its drop down was closed by a tab switch | Next time it opened, the ribbon showed an outdated picture and could jump in width | `fix/inribbongallery-stale-snapshot` | #316 → #329 |

## Checked and not a bug

- **Title bar hit testing** (`RibbonTitleBar.HitTestCore` returns a hit for any
  point): suspected of taking clicks meant for the window icon or content.
  Tests on the unfixed code passed (run #310), because WPF only asks an element
  about points inside its own area. Branch `fix/titlebar-hittest-bounds` holds
  only those tests and was not merged.
- The search also rejected about 20 other candidates, among them: maximized
  windows cut off (handled by ControlzEx), Alt+Space, the selected contextual
  tab when its group is hidden (correct), gallery selection sync (one control),
  and changing the gallery's items while it is open.

## Left for the owner to decide (not changed)

- **Blurry icons after moving to a screen with different scaling.** Icons are
  picked for the scaling once and not again when the window moves. A fix
  touches every icon.
- **The empty strip next to the tabs acts as a title bar** (drag, double-click
  maximizes, right-click shows the window menu). Intended in the original since
  #207, but apps can't turn it off.
- **Contextual group headers follow the order of `ContextualGroups`, not the
  order of their tabs.** An app that lists them differently gets headers over
  the wrong tabs. Fixing it changes how headers are placed.
- **Gallery filter names split on "," without trimming**: `"A, B"` silently
  hides B. Minor and undocumented.

## For a person with two screens or high scaling

1. Move the window between a 100% and a 150% screen. Are the window and ribbon
   icons sharp on both?
2. With the second screen to the left of the main one, drag the window by its
   title there. Does it jump?
3. Maximize the window on each screen. Is anything cut off? Again with the
   taskbar on auto-hide: can you still reach the taskbar?
4. Windows 11: hover over the maximize button. Does the snap layout popup
   appear?
5. With `UseNativeCaptionButtons="True"`: do Windows' buttons work, and do they
   overlap the ribbon?

## Process notes

Up to 12 helpers worked in parallel, each in its own git worktree. Two problems
came from that and are now ruled out in the helper instructions:

- A shared scratch folder: one helper's log was overwritten by another's
  (noticed before it was misread). Files now carry the branch name.
- `git stash` is shared by all worktrees: one helper popped another's stash and
  pushed it by mistake. It was undone with a revert commit, no history was
  rewritten, the final branch diff is clean, and the other helper confirmed its
  work was intact. `git stash` is no longer allowed.
