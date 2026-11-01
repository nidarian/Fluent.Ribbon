# Round 8: a bug hunt and an accessibility re-audit

Done 2026-11-01. **AI (Claude) was used** for all of it. Nothing was sent to the
original project.

How the candidates were found: three hunter agents (one each for lost bindings,
ribbons built from data at runtime, and the simplified ribbon / resizing) and an
accessibility re-audit against ADA / WCAG 2.1 AA read the code and listed
candidates. The session then checked every candidate against the code and proved
it test-first: a tests-only build that fails in the new tests on net462, net6.0
and net8.0, then the fix, whose build passes. Two candidates were not real (their
tests passed without a fix); their tests stay as guards. Finally two independent
reviewer agents (pessimistic and adversarial) reviewed the fixes, and their real
findings were fixed the same way.

## Fixed

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| A group moved from a tab that is out of the ribbon (removed from `Tabs`) to another tab | The app crashed ("already the logical child of another element") | `fix/runtime-tabs-groups-changes` | #460 → #468 |
| All tabs replaced (`Tabs.Clear()`, then `Add`) | No tab selected, the ribbon showed nothing | `fix/runtime-tabs-groups-changes` (#462 broke `SelectionTests`; the selection moved from RibbonTabControl into the Ribbon) | #460 → #468 |
| "Customize Status Bar" entries had no name (WCAG 4.1.2) | A screen reader said "menu item, checked" with no label | `fix/a11y-names-and-toggle` | #464 → #470 |
| The Spinner's text box (which takes the focus) had no name (WCAG 4.1.2) | A screen reader said only "edit" | `fix/a11y-names-and-toggle` | #464 → #470 |
| A checkable SplitButton had no Toggle pattern (WCAG 4.1.2) | A screen reader couldn't tell whether "Bold" was on | `fix/a11y-names-and-toggle` | #464 → #470 |
| Toolbar children ignored `SimplifiedSizeDefinition` in the simplified ribbon | A button meant to be icon-only showed its text | `fix/toolbar-simplified-size-definition` | #466 → #472 |

## Review findings, fixed

| Finding | Fix | Tests only → with fix |
|---|---|---|
| UI Automation Toggle flipped a SplitButton whose button part was disabled, and skipped the app's command and the GroupName rule | Toggle does what a click on the button part does, like Invoke | running (recorded here when done) |
| The Spinner text box was named by its Header object: a non-text header was read as a type name, an app's `AutomationProperties.Name` was ignored | The text box is labeled by the Spinner (`AutomationProperties.LabeledBy`) | running (recorded here when done) |
| Tabs replaced while the ribbon was unloaded left no tab when it came back | The ribbon selects the first tab on Loaded when none is selected | running (recorded here when done) |
| The toolbar size fix changed the look of apps whose toolbar children only set `SizeDefinition` (the default `SimplifiedSizeDefinition` is Large, Middle, Small) | Only a `SimplifiedSizeDefinition` the app set is used | running (recorded here when done) |

(#475/#476: the first review test didn't compile, `Button` meant WPF's button;
corrected.)

## Ruled out (tests kept as guards)

| Suspicion | Result |
|---|---|
| Clicking a backstage page or gallery item (or its KeyTip) puts a local value on top of an `IsSelected` binding from the app's item style, so the view model never learns the selection | Not a bug: the tests passed without a fix (#458). Kept as guards (`ItemIsSelectedStyleBindingTests`) |
| A group added to a tab while the tab is out of the ribbon is not shown when it comes back | Not a bug: passed without a fix (#460). Kept as a guard |

## Known limits and behaviour changes

- After all tabs are replaced, the first tab is selected at Loaded priority. An
  app that picks its own tab later (after an `await`, or at Background priority)
  wins, but `SelectedTabChanged` fires twice. Same timing as the first selection
  on load.
- The custom toolbar layout (with `LayoutDefinitions`) takes sizes from the
  layout definitions, which can be marked `ForSimplified`; that path is unchanged.
- The SplitButton's inner button still reports its own toggle state as well.

## Accessibility re-audit: still open

From the ADA / WCAG 2.1 AA re-audit, not fixed in this round:

- ColorGallery has no automation peer; colours are read as hex codes (1.1.1, 4.1.2).
- KeyTips and help text don't reach UI Automation for `Fluent.MenuItem`,
  `GalleryItem` and collapsed groups (4.1.2).
- Contextual tab text is below 4.5:1 in four themes (Light.Yellow 3.32, Light.Lime
  4.09, Light.Amber 4.33, Dark.Crimson 4.48) (1.4.3). Changes visible colours:
  owner's decision.
- The "..." overflow sample (`SimplifiedRibbonWindow.xaml`, `APP-REVIEW-CHECKLIST.md`)
  has no accessible name (1.1.1).
- Contextual group labels aren't exposed to UI Automation; the display options
  menu uses disabled items as headings (1.3.1).
- Fixed control heights don't grow with the Windows text size (1.4.4-like).
  Design work.
- `Control.Border` is below 3:1 for gallery and combo box borders (1.4.11).
  Changes visible colours.
- The status bar menu may not open from the keyboard when nothing in the status
  bar takes focus (2.1.1). Needs a person to check.
