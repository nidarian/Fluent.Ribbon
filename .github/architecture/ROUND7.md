# Round 7: the code items from OPEN-ITEMS.md

Done 2026-10-14. **AI (Claude) was used** for all of it. Nothing was sent to the
original project. Requested by the owner's workstation session: work through
sections 1, 2 and 4 of `OPEN-ITEMS.md` (sections 3 and 5 skipped), merge into
`integration/next` only, and leave `integration/all-fixes`, `develop` and
`master` untouched.

Each fix: a tests-only commit whose Windows build fails in the new tests on
net462, net6.0 and net8.0, then the fix, whose build passes all tests. Each
suspicion: a test first; fixed only if the test proved a real bug.

## Fixed

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| Double-clicking a tab (or clicking a contextual group header while minimized) replaced an app's OneWay binding on `Ribbon.IsMinimized` | The app's "minimized" setting stopped controlling the ribbon after one double-click | `fix/tab-doubleclick-keeps-minimized-binding` | #395 → #402 |
| A Spinner's toolbar copy replaced the original's OneWay `Value` binding | After changing the number in the toolbar copy, the spinner stopped following the app | `fix/spinner-qat-copy-keeps-oneway-value` | #387 → #398 |
| An editable ComboBox's toolbar copy (typing, or opening its list) replaced the original's OneWay `Text` binding | The combo box stopped showing the app's text | `fix/combobox-qat-copy-keeps-oneway-text` | #392 → #404 (#401 didn't compile: analyzer rule WPF0005) |
| The ApplicationMenu's default KeyTip didn't follow a runtime language switch | The shortcut letter stayed in the old language | `fix/applicationmenu-keytip-follows-language` | #389 → #397 |
| Hiding a StatusBarItem through `Visibility` didn't tidy the separators | Doubled or leading separator gaps | `fix/statusbar-separators-on-visibility` | #388 → #399 |
| **Suspicion, proven:** reopening the Backstage during its close animation | The old close animation hid it again while `IsOpen` stayed true | `fix/backstage-reopen-during-close` | #393 → #400 |
| **Suspicion, proven:** a Ribbon moved to another RibbonWindow kept the old window's title bar | The new title bar had no Quick Access Toolbar or contextual headers, and the closed old window stayed in memory | `fix/ribbon-moved-to-other-window` | #394 → #403 |

## Corrected: the tab double-click item

`OPEN-ITEMS.md` said build #376 proved that double-clicking a tab cuts the tabs
off from `Ribbon.IsMinimized`. That was wrong:

- Most of #376's failures came from test pollution. A saved "minimized" state
  (AutomaticStateManagement) leaked from an earlier test.
- The tabs do keep following `Ribbon.IsMinimized`. The style binding is TwoWay,
  so the plain setter writes through it.

The real bug was narrower: the write-through used `SetValue` on the Ribbon,
which dropped an app's OneWay binding. That is what #395 proves and what was
fixed.

## Ruled out (tests kept as guards)

| Suspicion | Why it's fine | Branch | Build |
|---|---|---|---|
| `DropDownButton.DismissOnClickOutside = false` blocks clicks elsewhere | Intended: it was added upstream (#1048) for dialogs shown in a drop down, which act like modal windows. The default setting also swallows the outside click (it only closes the drop down). | `fix/dropdown-dismissonclickoutside-capture` | #396 green |
| `RemoveFromQuickAccessCommand` throws for a parameter not on the toolbar | Its CanExecute checks exactly what `First(...)` needs, and WPF checks CanExecute first, also for a direct `Execute`. No other toolbar command uses `First`/`Single`. | `fix/remove-from-qat-command-param` | #390 green |
| A StatusBar item added at runtime lands one place too early in "Customize Status Bar" | For an instant it does, but the queued `RecreateMenu` rebuilds the menu in the right order before a user can open it | `fix/statusbar-add-menu-index` | #391 green |

## Combined

All 10 branches are merged into `integration/next` (on top of
`integration/all-fixes` b0470d30) at 6714e6c7. Combined build **#405: 1207/1207 on net462, net6.0 and net8.0**.

## Follow-up (2026-10-31)

**AI (Claude) was used** for all of it. Nothing was sent to the original
project. Same rules: a tests-only build that fails in the new tests on net462,
net6.0 and net8.0, then the fix, whose build passes. Merged into
`integration/next` only; `integration/all-fixes`, `develop` and `master` were
not touched.

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| Collapsing or expanding the ribbon through UI Automation (`RibbonAutomationPeer`, used by screen readers) replaced an app's OneWay binding on `Ribbon.IsMinimized` | After a screen reader collapsed or expanded the ribbon, the app's "minimized" setting stopped controlling it | `fix/uia-expandcollapse-keeps-minimized-binding` | #406 → #410 |
| A ribbon moved from a plain `Window` into a `RibbonWindow` never found the new title bar | The new window's title bar showed neither the Quick Access Toolbar nor the contextual group headers | `fix/ribbon-moved-from-plain-window` | #408 → #413 |
| A StatusBar item inserted at runtime got its "Customize Status Bar" entry one place too early until the queued menu rebuild ran | Nothing a user could see (the rebuild runs first); code reading the menu right after the insert saw the wrong order. Now consistent with Move, Remove and Replace | `fix/statusbar-add-menu-index-immediate` | #409 → #411 |
| Opening an InRibbonGallery's toolbar copy pushed `null` into the app's `SelectedItem` binding (`-1` into `SelectedIndex`) and back, and replaced a OneWay binding | Each time the copy opened, the app was told "nothing selected" (an app applying the selection, a font or a style, applied nothing for a moment); with a OneWay binding the gallery stopped following the app | `fix/inribbongallery-qat-copy-keeps-selection-binding` | #418 → #422 (#420: first try didn't fix TwoWay bindings) |
| `Ribbon.SelectedTabIndex` and `SelectedTabItem` replaced an app's OneWay binding (their default: neither binds TwoWay by default) | An app with `SelectedTabIndex="{Binding CurrentTab}"` could never select tabs from its view model: the binding was already gone once the ribbon had loaded | `fix/selectedtab-keeps-oneway-binding` | #428 → #430 |

Tests that already passed and stay as guards (in
`RibbonMovedToOtherWindowTests`): a ribbon moved from a `RibbonWindow` into a
plain `Window` leaves the old title bar, and a ribbon moved to another
`RibbonWindow` and back uses the first title bar again. #412 (a diagnostic
copy of #408) named the two failing tests on all three frameworks; the other
two passed.

Re-checked: `fix/spinner-qat-copy-keeps-oneway-value` built green again (#407).

Combined: the first three branches are merged into `integration/next` at 1a601e90,
combined build **#414 green** on net462, net6.0 and net8.0. The gallery fix
followed at 2fe5e15b, combined build **#424 green**; the selected-tab fix at
e959bfb4, combined build running (result recorded here when done).

### The InRibbonGallery fix: no redesign needed

Round 4 left this item open because "a safe fix needs a redesign of how the
copy borrows the items". It didn't: the gallery already keeps the app's
`ItemsSource` binding aside while the items are lent (`ItemsControlHelper`).
The fix does the same for the bindings on `SelectedItem`, `SelectedValue` and
`SelectedIndex`: while the copy is open each is replaced by a OneTime binding
to its current value, and it is set again when the items come back. A selection
made in the copy is then passed on with `SetCurrentValue`. Galleries without a
binding behave exactly as before.

The first try (#420) used `SetValue` with the current value instead. On a TwoWay
binding `SetValue` writes through the binding instead of replacing it, so the
view model still got the old value, `null` and the old value again (the same
effect round 7 described for the tab double-click).

Five tests (`InRibbonGalleryQuickAccessCopySelectionTests`): TwoWay
`SelectedItem`, OneWay `SelectedItem`, TwoWay `SelectedIndex` (all three failed
in #418 on every framework), and two guards (a selection made in the copy
reaches the app; no binding at all), which passed before and after.

### Build logs: a CI change for review

From the cloud session, the build logs and the test-results artifact could not
be downloaded (the session's network blocks the storage host GitHub serves
them from). The run page only said "Test Run Failed" three times. So
`ci/show-failed-tests` (based on `develop`) adds a step that lists each failed
test, with its message, as an error annotation on the run page. It only runs
when the build failed. Pull request for review: [#28](https://github.com/nidarian/Fluent.Ribbon/pull/28).

## Still open after this round

- Cheap hardening, not a bug: `First(...)` in the RemoveFromQuickAccess handler could become `FirstOrDefault` + null check. Not done: no test can fail on it (WPF checks CanExecute first).
