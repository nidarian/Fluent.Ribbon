# Accessibility review (screen readers, keyboard, contrast)

Done on the fork, 2026-09-30. **Nothing here is posted upstream.**

Three AI agents each reviewed one area and were told to try to *disprove* every
problem they found. Then:

- **Fixed** rows were re-read in the code by hand, and proven on Windows CI: a
  tests-only run fails for the predicted reason, then the fix passes on
  net462, net6.0 and net8.0.
- Rows marked *(agent reading)* were only read by an agent. Treat them as
  likely, not proven.
- Contrast ratios are WCAG 2.x values computed by the agent from the theme
  colors (`GeneratorParameters.json`) and the templates that use them. Two were
  recomputed by hand, and are marked.

Standards referred to: WCAG 2.1.1 (keyboard), 2.4.7 (focus visible),
1.4.3 (text contrast 4.5:1), 1.4.11 (contrast of controls and focus 3:1),
4.1.2 (name, role, value for assistive technology).

## Fixed

| Problem | Who is affected | Branch | Proof |
|---|---|---|---|
| A collapsed group always reported "collapsed" to UI Automation, even while its drop down was open. | Screen reader users, UI tests | `fix/uia-groupbox-expand-state` | Tests only, [run #72](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36776740350). With fix, [run #73](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36776785882): 306/306 |
| UI Automation could open a **disabled** drop down or backstage, and click a SplitButton's **disabled** button part. UIA requires `ElementNotEnabledException` there (the InRibbonGallery peer already did it). | Screen reader users, UI tests | `fix/uia-respect-disabled` | Tests only, [run #63](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775121538): 3 failures. With fix, [run #67](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775298146): 308/308 |
| The backstage button ("File") had no automation name. Narrator said only "menu". | Screen reader users | `fix/uia-backstage-name` | Tests only, [run #64](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775125648). With fix, [run #68](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775301012): 307/307 |
| The ribbon listed its menu ("File") twice to UI Automation. | Screen reader users | `fix/uia-ribbon-menu-once` | Tests only, [run #65](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775128710). With fix, [run #69](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775303570): 306/306 |
| With animations off, closing the backstage left keyboard focus on the hidden content: nothing visible had focus. | Keyboard users, and users who turn animations off | `fix/backstage-focus-no-animation` | Tests only, [run #70](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775488430). With fix, [run #71](https://github.com/nidarian/Fluent.Ribbon/actions/runs/36775561237) (second attempt: in the first, the CI machine had no keyboard focus on net8.0 and the test was inconclusive there): 306/306 |

Two of these tests were wrong at first, and CI caught both: the group test set
the state before loading (loading resets it), and failed on its own setup line
(runs #62 and #66, both unusable as proof). The focus test uses a keyboard-focus
baseline and is marked inconclusive when the CI machine can't focus the window;
NUnit's report shows that as "NotExecuted", which the fork's result checker now
flags.

## Screen readers: found, not fixed

> **Update 2026-10-01 (round 4):** all of these are fixed and proven in this fork (the
> ScreenTip "disabled because" row is documented instead). See `ROUND4.md`.

| Finding | Why not fixed |
|---|---|
| The Quick Access Toolbar's "Customize" and "More controls" buttons have no automation name, and "More controls" is missing from the automation tree. *(agent reading)* | Needs localized names in the template. Small, but not proven yet. |
| The Quick Access Toolbar appears twice in the automation tree of a `RibbonWindow` (under the title bar and under the ribbon). *(agent reading)* | Which parent should own it is a design choice. |
| Backstage tabs report as plain list items with no "selected" state (no SelectionItem pattern). *(agent reading)* | Changing the peer type changes what existing UI tests see. |
| `InRibbonGallery` advertises a Scroll pattern it doesn't implement. *(agent reading)* | Small, not proven yet. |
| A tab's `AutomationProperties.Name` is overridden by its text header (inverted check). *(agent reading)* | Small, not proven yet. |
| KeyTips (as access keys) and ScreenTip text (as help text) reach UI Automation only for `Button`, not for the other ribbon controls. *(agent reading)* | Touches every peer; ask the maintainer how it should be shared. |
| Quick Access Toolbar copies drop `AutomationProperties.Name`/`AutomationId`. *(agent reading)* | Needs a naming rule for the copies' ids. |
| `DropDownButton` reports control type "Custom" with its class name as the type ("DropDownButton", not localized). *(agent reading)* | Changing it breaks existing UI tests. Maintainer's call. |

## Keyboard: found, not fixed

> **Update 2026-10-01 (round 4):** all of these are fixed and proven in this fork (the
> ScreenTip "disabled because" row is documented instead). See `ROUND4.md`.

| Finding | Why not fixed |
|---|---|
| **ColorGallery: the first arrow key picks a color and closes the drop down**, so colors can't be browsed with the keyboard. Checked by hand: every selection change commits and dismisses (`ColorGallery.cs`, `RaiseDismissPopupEvent` after `SelectedColor`). | How a keyboard user should commit a color (Enter), and how that fits mouse clicks, is a design decision. |
| Opening an `InRibbonGallery` with Enter may immediately apply the first item (Enter opens on key down, focus moves to the first item, the item acts on key up). *(agent reading)* | Timing-sensitive; needs a runtime check first. |
| In the backstage, Tab ignores Shift (Shift+Tab moves forward into the content). *(agent reading)* | Small; needs real Shift input to test. |
| The backstage tab control may be an invisible tab stop. Tab may leave an open drop down while it stays open. *(agent reading, unclear)* | Needs a runtime check. |
| The Quick Access Toolbar and ColorGallery are tab stops that do nothing (a dotted frame around the whole control). *(agent reading)* | Small style change. |
| A collapsed group opens on Space but not on Enter. Space doesn't activate a gallery item. *(agent reading)* | Consistency; small. |
| The simplified `Spinner` shows no keyboard-focus highlight (the normal one does). *(agent reading)* | Small template change. |
| A ScreenTip's "disabled because ..." text is only shown for disabled controls, which can't take keyboard focus. *(agent reading)* | WPF limitation; document it. |

## Contrast

With the default **Light.Blue** and **Dark.Blue** themes, the main text passes
easily: tabs, group headers, buttons, KeyTips, ScreenTips, title and backstage
text are between 12:1 and 21:1. Focus rectangles are 14:1 or more.

Failures (all *agent-measured* unless marked):

| Where | Theme | Colors | Ratio | Needs | Smallest fix |
|---|---|---|---|---|---|
| Text box border | Light | `#CCCCCC` on `#F7F7F7` | **1.50:1** (re-checked by hand) | 3:1 | `TextBox.Border` (and `Control.Border`) to Gray2 `#7F7F7F`: 3.7:1 |
| Placeholder text in a simplified text box | Light | black at 0.5 opacity on white (`#808080`) | **3.95:1** (re-checked by hand) | 4.5:1 | Opacity 0.6: 5.74:1 (`RibbonTextBox.xaml`) |
| Checked toggle button border | Dark | `#1651AA` on `#2C2C2C` | 1.86:1 | 3:1 | Use AccentDark3 in Dark (fails for 19 of 23 accents today) |
| Gallery filter header text | Light | white on `#9D9D9D` (hover: `#FFD232`) | 2.71:1 (1.87:1) | 4.5:1 | Black text, or a darker header |
| "Colorful" title bar text | Light | white on Accent80 over `#FAFAFA` | 3.33:1 | 4.5:1 | Brand decision (20 of 23 accents fail in Light) |
| Button hover/pressed text, some accents | Dark: Yellow, Lime, Amber, Pink, Cyan, Teal. Light: Crimson, Indigo | e.g. Dark.Yellow hover | down to 2.16:1 | 4.5:1 | Use the palette's existing `AccentLight2/3.Foreground` colors on hover/pressed |
| Accent used as the only indicator (backstage selected bar, text box focus border, check box tick), some accents | Light: Amber, Cyan, Green, Lime, Orange, Pink, Teal, Yellow. Dark: 11 darker accents | e.g. Light.Yellow | down to 1.25:1 | 3:1 | Use AccentDark3 |
| Selected gallery item | Light.Blue | `#3B9BE5` on white | 2.997:1 | 3:1 | AccentLight1 |

Also by color alone: the backstage's selected vs. hovered tab differ only by
the color of the side bar (1.12:1 between the two), and a gallery's selected
vs. hovered item only by fill color. *(agent reading)*

Under the default Mica window backdrop, Windows draws the window background,
so the window-background numbers above hold when there is no backdrop (for
example Windows 10). Needs a runtime check.

Nearly all contrast fixes change theme colors, which is the maintainer's
decision. The placeholder opacity is the only one that doesn't touch a color.

> **Update 2026-10-01 (round 4):** all rows except the "Colorful" title bar are fixed in
> this fork (`fix/contrast-wcag`), checked in all 46 Light/Dark themes. This changes
> colors in the app. See `ROUND4.md`.

## High Contrast mode: not supported

Confirmed in the code: nothing reads `SystemParameters.HighContrast`, no High
Contrast theme is generated (no color scheme sets `IsHighContrast`), and the
only use of system colors is the text selection color, read once when the
theme loads. A user who turns on a Windows High Contrast theme still sees the
app's normal Fluent colors. The maintainer said the same in
[#1018](https://github.com/fluentribbon/Fluent.Ribbon/issues/1018) ("Fluent.Ribbon
does not ship any high contrast themes"). Adding it is design work: a High
Contrast color scheme based on system colors, or `SystemParameters.HighContrast`
triggers in the templates.

> **Update 2026-10-01 (round 4):** basic, opt-in support exists in this fork
> (`feature/high-contrast-basic`). What it covers and what it doesn't: `HIGH-CONTRAST.md`.
