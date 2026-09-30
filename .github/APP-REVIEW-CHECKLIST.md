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
| Simplified ribbon state | If you use the simplified ribbon and set `SimplifiedStateDefinition` on groups, that setting is now respected. Groups with custom values may look different than before, which is the fix working. |

**Regressions to watch for:** Quick Access Toolbar contents after startup and
after switching between classic and simplified; KeyTips on normal Fluent
buttons; mouse-wheel scrolling of the ribbon; group sizes when resizing the
window.

### Opt-in additions (nothing changes unless you use them)

| Addition | How to try it |
|---|---|
| #813 `FocusFirstItemOnDropDownOpen` | On a `DropDownButton` or `SplitButton`, set `FocusFirstItemOnDropDownOpen="False"`. Open it with the mouse: no item should be highlighted. Press Down: the first item gets focus. |
| #1247 `Backstage.Closing` | Handle `Closing` on your `Backstage` and set `e.Cancel = true` while you're saving. Escape, clicking outside and the back button should then leave it open. |
| #647 Spinner accessibility | Screen readers (Narrator) and UI test tools should now see a `Spinner` as a spinner with its value, minimum and maximum, and be able to set the value. |
| #1265 `Fluent.Ribbon.Brushes.BackstageTabItem.Focus.Border` | Override this brush in your resources to recolor the keyboard focus frame of backstage tabs. |
| #1233 "..." overflow group | A last group with `Header="..."` and `SimplifiedStateDefinition="Collapsed"` shows as a single "..." button in the simplified ribbon. See the Showcase's Simplified Ribbon window, tab 1. |

## 4. Reporting back

For anything that looks wrong, note which row it is, what you did, and what you
saw (a screenshot helps). That's enough for the next round of fixes.
