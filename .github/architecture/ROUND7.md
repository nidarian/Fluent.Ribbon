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
`integration/all-fixes` b0470d30) at 6714e6c7. Combined build: pending.

## Still open after this round

- `RibbonAutomationPeer` Expand/Collapse (screen readers and UI automation) sets `Ribbon.IsMinimized` with the plain setter, so it can drop an app's OneWay binding (found while fixing the tab double-click).
- Opening an InRibbonGallery's toolbar copy can push `SelectedItem = null` into the app's binding. It needs a redesign; skipped on purpose.
- A ribbon moved from a plain window into a RibbonWindow, and back-and-forth moves, are not tested.
- Cheap hardening, not a bug: `First(...)` in the RemoveFromQuickAccess handler could become `FirstOrDefault` + null check.
- StatusBar: the menu is briefly one place off right after an Add (index + 1 would fix even that instant).
