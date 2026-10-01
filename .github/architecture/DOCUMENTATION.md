# Documentation audit (XML doc comments, README, Walkthrough)

Done in this fork, 2026-09-30 / 10-01. **AI (Claude) was used** to find and
check these. Every change below was checked by hand against the code on
`develop` (= upstream `develop`). Nothing has been sent to the original project.

Why so careful: the maintainer rejected an earlier AI-written theming guide
(#1259) as "totally wrong". Bad documentation is worse than none, so a change
was only made when **both** of these agreed:

1. **My hand check**: the claim, re-read in the code (coerce/changed callbacks,
   theme XAML, callers, defaults in the `DependencyProperty` registration).
2. **A blind check**: a separate reviewer got the old and the new text shuffled
   into "A" and "B" (seeded random, key kept elsewhere), was told to ignore the
   existing `///` comments, and picked the more accurate one from the code only.
   It also had to flag anything false in either text.

| Batch | Scope | Changes | Blind check agreed |
|---|---|---|---|
| 1 | Controls A-M (galleries, drop downs, menus) | 19 | 19/19 |
| 2 | Controls N-Z, state storage, interfaces, README | 22 | 22/22 |

## Where the fixes are

| Branch | What |
|---|---|
| `docs/xml-doc-corrections` | Both batches, on top of this fork's `develop` |
| `upstream-pr/xml-doc-corrections` | The same 2 commits on upstream `develop` (23 files, doc text only, no code) |
| `integration/all-fixes` | Merged in, so the app package has the corrected tooltips |

Windows builds (they compile the XML docs with warnings as errors, so a broken
`<see cref>` would fail them): batch 1 [#77](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36793793727) passed.
Batch 2 [#78](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36794258178) and
integration [#79](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36794299814): running.

## The worst ones (a developer would be misled)

- `Ribbon.QuickAccessToolBarHeight` said "height used to render the window
  title". It sets the height of the Quick Access Toolbar row **below** the
  ribbon (`Themes/Controls/Ribbon.xaml`, `quickAccessToolBarHolder`).
- `Ribbon.KeyTipKeys` starts **empty**, yet Alt and F10 work (defaults live in
  `KeyTipService`). Any change copies only this collection, so adding one key
  removes the defaults and clearing it leaves **no** keys.
- `RibbonStateStorage.Reset` deletes the saved state of **every** ribbon in the
  isolated store (`*Fluent.Ribbon.State*`), not just this one.
- `AutomaticStateManagement` said it saves the *Quick Access Toolbar's* state.
  It saves the ribbon's: minimized, simplified, toolbar position. No toolbar items.
- `StatusBarItem.IsChecked = false` **hides** the item (coerce on Visibility).
- `DropDownButton.DropDownHeight`, `InRibbonGallery.DropDownHeight/Width` and
  `MaxDropDownWidth` are not used by those drop downs at all.
- Copy-paste docs that described a different member: `SplitButton.Indeterminate`
  ("unchecked"), `UniformGridWithItemSize.MinColumns` ("maximum"),
  `RibbonTabControl.TabsContainer` and `IsToolBarVisible`,
  `Gallery.Min/MaxItemsInRow` ("width").
- README said ".NET SDK 10.0.100 or later". `global.json` pins 10.0.400 with
  `rollForward: feature`, so 10.0.100-10.0.3xx can't build it, and neither can
  an 11.0 SDK alone. Now: "10.0.400 or a later 10.0 SDK".

## Proposals I changed after checking

- The auditor wanted to call `SaveTemporary` unused. It **is** called (when
  `ShowQuickAccessToolBarAboveRibbon` changes). Only `LoadTemporary` has no
  caller, so only that one says "Fluent.Ribbon never calls this itself".
- The auditor wanted to change `Reset` on the **interface**. Other storage
  classes may implement it differently, so only `RibbonStateStorage.Reset`
  got the "deletes every ribbon's state" text.
- The blind reviewer pointed out that "while empty the defaults are used"
  (`KeyTipKeys`) is false after keys were added and then cleared. Reworded.
- The blind reviewer pointed out that "10.0.400 or later" overclaims (11.0
  doesn't satisfy `feature`). Reworded.

## Not changed (cosmetic, left for the maintainer)

These are the auditor's reading; they were not blind-checked.

- "Gets or sets" on read-only members: the `IsSimplified` implementations,
  `RibbonTabItem.IsContextual`, `RibbonContextualTabGroup.InnerVisibility`,
  `RibbonGroupBoxStateDefinition.States`, `IRibbonStateStorage.IsLoaded`.
- Wrong names in summaries: `RibbonGroupBoxWrapPanel.ExcludeFromSharedSizeProperty`,
  `RibbonContextualTabGroup.FirstVisibleAndEnabledItem` (also needs enabled),
  `IToggleButton.IsChecked` ("SplitButton"), `RibbonToolBarControlGroupDefinition.Children`
  ("rows"), `RibbonToolBarRow` / `RibbonToolBarLayoutDefinition` class summaries
  ("size definition for group box").
- Typos that change meaning: "Occurs **then** popup is dismissed"
  (`PopupService`), "Occurs then CanAddToQuickAccessToolBar property changed"
  (it is a method, `RibbonControl`).
- `KeyTip.AutoPlacement` reads as if Margin only applies when it is false.
  The adorner applies Margin either way. *(agent reading, not hand-checked)*
- Showcase comments: a wrong `ReduceOrder` default comment, mislabeled
  Large/Middle size comments, a "GroupedByAdvanced" typo. *(not blind-checked)*

## The Walkthrough (`Doc/*.docx`, `Doc/*.pdf`)

Written 2010-2012 for v2.x. An auditor listed 12 wrong items. I re-checked
these 4 by hand:

- Theme URI `.../Themes/Office2010/Silver.xaml`: no Office2010 themes exist now.
- `<Fluent:Button Text="...">`: `Button` has no `Text`; it uses `Header`.
- `public partial class Window : MetroWindow`: there is no `MetroWindow` in the library.
- "distributed under Microsoft Permissive License (Ms-PL)": the project is
  **MIT** (`License.txt`, `PackageLicenseFile`).

**Recommendation: don't edit it.** It is a 2012 document; fixing it means a
rewrite. Suggest to the maintainer that it is marked outdated or removed.
That is the maintainer's call.

## Code problems found in passing (not fixed)

- `ColorGallery.OnApplyTemplate`: the **old** More Colors button is
  subscribed again with `+=` where it should be unsubscribed with `-=` (the
  No Color button right below does it correctly). Low impact: the discarded
  button keeps a reference to the gallery.
- `Gallery.IsLastItemPropertyKey` is `public`, so any code can set the
  read-only `IsLastItem`.
