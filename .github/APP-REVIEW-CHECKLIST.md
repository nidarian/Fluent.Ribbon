# Reviewing the fixes in your own app

All fixes are merged on the `integration/all-fixes` branch of this fork. Its
Windows build produces a normal Fluent.Ribbon NuGet package you can use in your
app instead of the nuget.org one.

## 1. Get the package

1. Open the fork's **Actions** tab, then **Build (Windows)**, and pick the latest
   run for branch `integration/all-fixes`.
2. At the bottom of the run page, download the **packages** artifact (a zip).
3. Unzip it into a folder, for example `C:\LocalNuGet\Fluent.Ribbon`. It contains
   `Fluent.Ribbon.<version>.nupkg`. The version looks like
   `11.0.3-integration-all-fixes.<n>`.

Artifacts are kept for 90 days. Re-run the build to get a fresh one later.

## 2. Point your app at it

In your app's folder, next to the `.sln`, add a local package source:

```sh
dotnet nuget add source C:\LocalNuGet\Fluent.Ribbon -n fluent-ribbon-local
```

Then change the Fluent.Ribbon version in your app's project file, or in its
`Directory.Packages.props` if you use central package management, to the exact
version from the file name, and rebuild.

**To go back:** set the version back to the nuget.org one you had (for example
`11.0.2`). You can leave the local source, because NuGet still picks the exact
version you ask for.

## 3. What to check

### Things that change behavior by themselves

You don't need to change any code to get these. Just watch for them.

| Fix | What to check in your app |
|---|---|
| #1251 Quick Access items added from code | If your app adds `QuickAccessMenuItem`s in code with `IsChecked = true`, they should now appear on the Quick Access Toolbar right away, without opening its menu first. |
| #357 KeyTip placement | If you have your own controls implementing `IKeyTipedControl` (not Fluent controls), press Alt: their KeyTips should sit top-left instead of centered. |
| #1176 Touch scrolling | On a touch screen, with the window narrow enough that the ribbon groups scroll: dragging a finger across the groups should scroll them. Dragging past the end must not bounce the whole window. |
| KeyTip row snapping (#572 regression) | Press Alt, then a tab's KeyTip. KeyTips of every control inside a group (small, middle and large buttons) now move up or down onto the nearest of the group's row lines, so they line up. Only the height changes, not the left/right position. **This is the most visible change.** If it looks worse in your app, say so. |
| Up key on drop downs | Focus a `DropDownButton` or `SplitButton` with Tab and press Up: the drop down opens with the **last** item focused. Before, it focused the first. |
| Backstage content replaced | Only if your app swaps `Backstage.Content` at runtime and reuses the old element elsewhere: the old element no longer hides when the backstage closes. |
| Group KeyTips like "ZC" | Press Alt, a tab's KeyTip, then the first letter of an *expanded* group's KeyTip (for example Z). Before, every KeyTip vanished but KeyTip mode stayed on and swallowed the key. Now KeyTips close cleanly, as for any key that matches nothing. |
| Adding a gallery to the Quick Access Toolbar | Right-click a gallery inside a drop down: "Add Gallery to Quick Access Toolbar" is now enabled and adds the drop down. |
| Quick Access items restored at startup | If your app calls `ribbon.AddToQuickAccessToolBar(...)` in the window constructor, those items now appear. |
| Turning KeyTips off while they show | Only if your app sets `IsKeyTipHandlingEnabled = false` at runtime: KeyTips that are showing now close. |
| SplitButton with a disabled main part | Tab to a `SplitButton` with `IsButtonEnabled="False"` and press Enter: the drop down opens, and the main action does *not* run. |
| Backstage open while the ribbon moves | Only if your app moves the ribbon or swaps the window content while the backstage is open: afterwards the Quick Access Toolbar must be back, and Esc must not act on a hidden backstage. |
| Collapsed group drop downs | Hard to see by hand: resizing the window closes popups anyway. It shows when something else widens the ribbon while a collapsed group's drop down is open (a splitter or docking panel, or content that changes inside the drop down). Then no empty drop down may stay open. |
| Starting in a small window | Start your app with a window smaller than 300x250 (or restore one): the ribbon is collapsed right away. |
| Screen reader: File button and groups | With Narrator on (Win+Ctrl+Enter): the File button is read as "File", once. A collapsed group reads "expanded" while its drop down is open. |
| Screen reader / UI tests: disabled controls | Disabled drop downs, a disabled backstage, and a SplitButton's disabled main part can no longer be opened or clicked through UI Automation (UI test tools get an "element not enabled" error). |
| Backstage without animations | With `AreAnimationsEnabled="False"` on the backstage: open it, press Esc. Keyboard focus is back on the File button (Tab and Enter work from there). |
| Clearing Quick Access items | Only if your app calls `QuickAccessItems.Clear()` on a `QuickAccessToolBar`: the entries now also leave the toolbar's customize menu (the small arrow). |
| Temporary ribbon state | Only if your app calls `RibbonStateStorage.SaveTemporary()` / `LoadTemporary()` itself: the restored state is now always complete. |
| Simplified ribbon state | If you use the simplified ribbon and set `SimplifiedStateDefinition` on groups, that setting is now respected. Groups with custom values may look different than before, which is the fix working. |

**Regressions to watch for:** Quick Access Toolbar contents after startup and
after switching between classic and simplified; KeyTips on normal Fluent
buttons (none should end up covering the wrong button); mouse-wheel scrolling of the ribbon; group sizes when resizing the
window.

### Round 4 changes you can see (added 2026-10-01)

The full list of 51 round 4 fixes, with what each changes, is in
`architecture/ROUND4.md`. Most are invisible unless your app hit the bug. These
are the ones you will notice anyway:

| Change | What to check in your app |
|---|---|
| **Theme colors** (contrast) | Text box borders are a mid gray (#7F7F7F) in both themes: darker in Light, lighter in Dark. Checked toggles, text box focus frames, check marks and the backstage selection use a darker shade of the accent. Gallery filter headers have black text and a yellow background on mouse over. The selected gallery item has a 1px outline. Button text on hover/pressed uses the palette's matching foreground. If your app overrides Fluent brushes, check that your colors still win. |
| **ColorGallery keyboard** | Arrow keys now only move between colors. **Enter or Space** picks one and closes the drop down. Before, the first arrow key picked a color. The mouse works as before. |
| **Wrong key inside KeyTips** | Press Alt, a tab's KeyTip, then a key that matches nothing: Windows beeps and the KeyTips stay. At the first level (right after Alt) a wrong key still closes them. |
| **Tab on an open drop down** | Only when keyboard focus stayed on the button while the drop down is open (every item disabled, or `FocusFirstItemOnDropDownOpen="False"` and opened with the mouse): Tab now closes the drop down as focus moves on. Tab inside the drop down still moves between its items. |
| **Enter and Space on galleries** | Opening an `InRibbonGallery` with Enter no longer applies its first item. In a collapsed group, both Enter and Space open it. Space now activates a focused gallery item. |
| **Tab order** | One Tab stop fewer on a ColorGallery, the toolbar below the ribbon, and the backstage tab list. In the backstage, Shift+Tab now goes back. |
| **Bindings on `IsDropDownOpen`** | If your app binds `IsDropDownOpen` OneWay, the binding now keeps working after the user opens or closes the drop down. |
| **Spinner values** | Leaving a Spinner without typing keeps its exact value (0.25 stays 0.25 with format F1). Negative numbers keep their sign under Swedish, Norwegian or Finnish Windows. |
| **Language switch at runtime** | If your app changes `RibbonLocalization.Current.Culture` while running: the ribbon's right-click menu and "Customize Status Bar" now switch language too. |
| **Drop-down height at 150%/200% scaling** | Long drop downs are at most a third of the screen tall at any display scaling. Before, at 200% they could run off the screen. |
| **UI tests and screen readers** | A `DropDownButton` is reported as "button" (SplitButton: "split button") instead of "Custom". UI tests that search for "Custom" need updating. Toolbar buttons and copies now have names. |
| **Compiler warning** | If your code uses `Gallery.IsLastItemPropertyKey`, it now gets an `[Obsolete]` warning. |

### Opt-in additions (nothing changes unless you use them)

| Addition | How to try it |
|---|---|
| #813 `FocusFirstItemOnDropDownOpen` | On a `DropDownButton` or `SplitButton`, set `FocusFirstItemOnDropDownOpen="False"`. Open it with the mouse: no item should be highlighted. Press Down: the first item gets focus. Opening with Down or Up from the keyboard still focuses the first or last item. |
| #1247 `Backstage.Closing` | Handle `Closing` on your `Backstage` and set `e.Cancel = true` while you're saving. Escape, clicking outside and the back button should then leave it open. |
| #647 Spinner accessibility | Screen readers (Narrator) and UI test tools should now see a `Spinner` as a spinner with its value, minimum and maximum, and be able to set the value. |
| #1265 `Fluent.Ribbon.Brushes.BackstageTabItem.Focus.Border` | Override this brush in your resources to recolor the keyboard focus frame of backstage tabs. |
| #1233 "..." overflow group | A last group with `Header="..."` and `SimplifiedStateDefinition="Collapsed"` shows as a single "..." button in the simplified ribbon. See the Showcase's Simplified Ribbon window, tab 1. |
| High Contrast (round 4) | Set `ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithHighContrast` at startup, then turn on a Windows High Contrast theme (Settings, Accessibility, Contrast themes). The ribbon should use the High Contrast colors. Known gaps are in `architecture/HIGH-CONTRAST.md`. |

## 4. Reporting back

For anything that looks wrong, note which row it is, what you did, and what you
saw (a screenshot helps). That's enough for the next round of fixes.
