# Round 6: input controls, simplified ribbon, status bar, tooltips, robustness

Done 2026-10-09. **AI (Claude) was used** for all of it. Nothing was sent to the
original project.

Each fix: a tests-only commit whose Windows build fails in the new tests on
net462, net6.0 and net8.0, then the fix, whose build passes all tests. The 16
fixes are combined in `integration/round6` (pull request #24). The first build
of all 16 together runs on that pull request: merge it only when it is green.

## Fixed

| Problem | Branch | Tests only → with fix |
|---|---|---|
| OneWay `IsChecked` lost after a click (checkable SplitButton; toolbar copies of ToggleButton, CheckBox, RadioButton, MenuItem, SplitButton) | `fix/ischecked-oneway-binding-kept` | #346 → #366 |
| Opening a toolbar copy's drop down cut the original's `ItemsSource` binding | `fix/qat-copy-dropdown-keeps-itemssource-binding` | #344 → #360 |
| Clicking the selected toolbar copy of a grouped button left nothing selected | `fix/qat-copy-groupname-keeps-selection` | #343 → #358 |
| Spinner `Value` / ComboBox `SelectedItem` lost OneWay bindings | `fix/spinner-combobox-keep-oneway-bindings` | #345 → #361 |
| Percent spinner read "60 %" as 60 (shown as 6,000 %); exponent formats broke | `fix/spinner-percent-exponent-formats` | #342 → #359 |
| Custom ribbon texts with the same language were ignored | `fix/localization-same-culture-replace` | #347 → #362 |
| Controls added to a RibbonToolBar at runtime stayed classic in the simplified ribbon | `fix/toolbar-runtime-children-simplified` | #348 → #369 |
| Loading saved state and the ribbon's menu cut OneWay bindings (IsSimplified, IsMinimized, toolbar position) | `fix/state-load-keeps-oneway-bindings` | #354 → #365 |
| `Groups.Move` / `Groups[i] = x` showed the wrong order | `fix/tab-groups-move-replace` | #350 → #367 |
| "More Colors" crashed apps with windows on several UI threads | `fix/colorgallery-recentcolors-threads` | #357 → #373 |
| Status bar separators not recalculated after removing items | `fix/statusbar-separators-on-remove` | #353 → #372 |
| Right-to-left ScreenTips half their width off at >100 % scaling | `fix/screentip-rtl-dpi-offset` | #349 → #364 |
| Finished UI threads kept in memory by the ribbon context menu; unsafe shared table | `fix/ribbon-context-menu-per-dispatcher` | #363 → #374 (#355 didn't compile) |
| An empty, missing or non-image icon path crashed the app (now: no icon, debug output) | `fix/icon-converter-bad-path-no-crash` | #371 → #375 (#352: test mistakes) |
| The first window with an ApplicationMenu was never freed | `fix/applicationmenu-header-keeps-first-menu` | #356 → #370 |
| `ToolTipService.IsEnabled="False"` ignored by drop downs and collapsed groups | `fix/tooltip-isenabled-respected-dropdowns` | #351 → #368 |

## Corrected during the round

- The search reported a right-click **crash** on recycled UI thread ids. The
  helper showed it can't happen with today's code (the leaked thread is never
  freed, so its id is never reused). What was real: the leak and the unsafe
  table, both fixed.
- The icon fix follows the owner's preference: a missing icon must not crash
  the app. The original library fails fast instead; easy to reverse.

## Open (not fixed)

- **Double-clicking a tab can cut the tabs off from `Ribbon.IsMinimized`.**
  Proven by the tests-only build #376 (branch
  `fix/tab-doubleclick-keeps-minimized-binding`); the fix wasn't written before
  work stopped.
- A spinner's toolbar copy: its arrows can still drop a OneWay `Value` binding
  on the original (same kind as the IsChecked fix).
- A ComboBox toolbar copy's `Text` can still drop a OneWay `Text` binding.
- Hiding a status bar item through `Visibility` doesn't recalculate separators.
- Human check: a right-to-left window at 150 % scaling (ScreenTip position).
- Human check: the Showcase's "new thread" window, "More Colors" in both.
