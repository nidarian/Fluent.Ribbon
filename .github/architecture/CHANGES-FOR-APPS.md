# Changes an app may notice (package from `integration/all-fixes`)

Most fixes only change something when the bug happens. This page lists the
changes that **every** app using the fork's package can notice, so they can
be checked when updating. Status: 2026-10-08. The open choices about some of
these are in `AUDIT-DECISIONS.md`.

## The built-in themes look slightly different (`contrast-wcag`)

These colours were changed to meet WCAG contrast ratios. `ROUND4.md` has the
reasons, and `AUDIT.md` lists the ratios recalculated by the audit.

- **Text box borders** (ComboBox, Spinner, TextBox): `#CCCCCC` → `#7F7F7F` in
  **Light** only (easier to see there). Dark keeps `#CCCCCC`, by decision D2.
- **Light.Blue**: check marks, the text box focus border and the Backstage
  selection bar go from `#0078D7` to `#0048A3`.
- **Dark.Blue**: the checked toggle button border goes from `#1651AA` to
  `#58ACED`, and the check mark from `#0078D7` to `#58ACED`.
- **Light.Yellow palette**: `AccentDark3` goes from `#B19802` to `#9F8902`.
  This also darkens Light.Yellow contextual tab text and the tab border.
- **Button text on hover and press** uses a palette colour: white in
  Light.Crimson and Light.Indigo, black in Dark.Amber, Cyan, Lime, Pink, Teal
  and Yellow. A button with its own `Foreground` (for example red) keeps it
  on hover and press, by decision D1.
- **Gallery filter header**: black text instead of white, with a yellow
  (`#FFD232`) hover background. In opt-in High Contrast mode it follows the
  system colours.
- **Selected gallery item**: a 1px outline.
- **Placeholder text**: opacity 0.5 → 0.6.

## New theme keys

A hand-written theme dictionary (one not generated from Fluent's template)
must add these keys. Otherwise the parts that use them lose their colour; a
missing check mark colour can make check marks disappear.

- `Fluent.Ribbon.Brushes.BackstageTabItem.Focus.Border`
- `Fluent.Ribbon.Brushes.Button.MouseOver.Foreground`
- `Fluent.Ribbon.Brushes.Button.Pressed.Foreground`
- `Fluent.Ribbon.Brushes.CheckBox.CheckMark`
- `Fluent.Ribbon.Brushes.Gallery.Header.Foreground`
- `Fluent.Ribbon.Brushes.Gallery.Header.MouseOver.Background`
- `Fluent.Ribbon.Brushes.Gallery.Header.MouseOver.Foreground`
- `Fluent.Ribbon.Brushes.GalleryItem.Selected.Border`

Themes generated from `Theme.Template.xaml` also need the three
`Gallery.Header.*` values in their parameters file. `GeneratorParameters.json`
has the defaults.

## Automated UI tests (UI Automation) may see something different

These make the library more correct for screen readers, but a UI test that
searches by the old value stops finding the control.

- **DropDownButton** reports control type Button, not Custom. This also
  applies to SplitButton, ApplicationMenu, the gallery filter button, and the
  Quick Access Toolbar's own overflow and customize buttons, because they are
  DropDownButtons too.
- **Backstage tabs**: TabItem instead of ListItem, with a "selected" state.
- **Spinner**: its own control type and value pattern (#647), not Custom.
- **Quick Access Toolbar** in a `RibbonWindow` appears only once in the tree,
  under the title bar. It is no longer also under the Ribbon element.
- **KeyTips** are reported as the access key and **ScreenTips** as help text
  for all ribbon controls, not just Button. Only the last KeyTip letter is
  reported (see `AUDIT-DECISIONS.md`, D8).

## Keyboard behaviour

- **Drop downs**: Tab with focus still on the button closes the drop down.
  Enter or Space pressed *inside* an open drop down (for example in a text
  box) no longer closes it, and in a SplitButton no longer clicks the button.
- **KeyTips**: a wrong key inside a group or menu beeps and keeps the KeyTips.
  At the first level the user sees (the ribbon, or an open Backstage,
  application menu or start screen), a wrong key closes the KeyTips (D3). After opening a drop down with its KeyTip,
  ↓, Tab and other non-letter keys end KeyTips but leave the drop down open,
  as in Office.
- **ColorGallery**: arrow keys browse colours without picking. Enter or Space
  picks, and so does a mouse click.
- **Collapsed groups**: Enter opens them, as Space already did.

## New or changed public API

- `Backstage.Closing`: a cancellable event before the Backstage closes (#1247).
- `DropDownButton.FocusFirstItemOnDropDownOpen` (default `true`) (#813).
- `RibbonSpinnerAutomationPeer` (#647).
- Opt-in High Contrast support for runtime themes (`HIGH-CONTRAST.md`).
- The `IsLastItem` resource key on GalleryItem is marked obsolete, so apps that
  build with warnings as errors get a compile error until they stop using it.
- `RibbonProperties.UsesThemeForeground` (attached, set by the button styles)
  and `Fluent.Converters.IsSameAsResourceConverter`: used to keep an app's own
  button text colour on hover (D1).
- Internal writes to `IsDropDownOpen` and similar properties use
  `SetCurrentValue`, so an app's OneWay binding on them survives.

## Other

- **Touch** (#1176): the ribbon's groups area can scroll sideways by touch,
  **opt-in** (D4): `<Fluent:Ribbon ScrollViewer.PanningMode="HorizontalOnly">`.
  It is off by default, as in the original, because it hasn't been checked on
  a touch screen.
- **Ribbon collapse at start**: the ribbon collapses itself at start if the
  window is too small, not only on the first resize. It never expands a ribbon
  the app set to collapsed (D5).
- **Quick Access Toolbar items changed from code**: checking a
  `QuickAccessMenuItem` from code adds its control to the toolbar right away
  (#1251), and unchecking it removes it right away, even before the user has
  opened the quick access menu.
- **Turning on `AutomaticStateManagement` after load** reads the saved state
  instead of overwriting it.
