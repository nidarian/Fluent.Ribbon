# Audit follow-up: decisions for the owner (2026-10-07)

The audit (`AUDIT.md`) found about 15 smaller concerns. Some were plain bugs and
are being fixed test-first. The rest are **choices**: there is no single right
answer, so the owner decides. No coding knowledge is needed: each item says
what a user would notice, the options, and a recommendation.

**How to answer:** write the item numbers and A or B (e.g. "D1 B, D2 A, ...")
and send it to Claude. Anything left blank stays as it is now.

## Choices

### D1. Button text colour on hover

Since `contrast-wcag`, a Fluent button's text switches to a theme colour while
the mouse is over it or it is pressed, so it is readable on the highlight.
But if an app gave the button its own text colour (for example red for
"Delete"), that colour is lost on hover.

- **A.** Keep as it is (always the theme colour on hover).
- **B.** Use the theme colour on hover only for buttons that don't set their own text colour. *(Recommended: the app's choice wins, and default buttons stay readable.)*

### D2. Text box borders in the Dark themes

`contrast-wcag` made text box borders darker in **both** Light and Dark. In
Light they became easier to see (1.5:1 → 3.7:1). In Dark they became
**harder** to see (8.7:1 → 3.5:1), which was never the intent.

- **A.** Keep as it is.
- **B.** Undo the change in the Dark themes only. *(Recommended.)*

### D3. A wrong KeyTip key inside the Backstage, application menu or start screen

Since `keytip-wrong-key-nested-level-keeps-keytips`, a wrong key inside a
group's or menu's KeyTips beeps and keeps the KeyTips (like Office), while a
wrong key at the ribbon's first level closes them. The Backstage ("File"
screen), the application menu and the start screen count as "inside", so
there a wrong key beeps too, and Esc then closes the Backstage.

- **A.** Keep as it is.
- **B.** Treat the first KeyTip level of the Backstage, application menu and start screen like the ribbon's first level: a wrong key closes the KeyTips. *(Recommended: closer to how it behaved before, and to Office.)*

### D4. Touch scrolling of the ribbon

Since `#1176`, the ribbon's groups area can be scrolled sideways by touch,
for every app. A horizontal touch drag on a slider inside the ribbon might
scroll the ribbon instead of moving the slider. This has not been checked on a
touch screen.

- **A.** Keep it on for every app.
- **B.** Make it opt-in (off unless the app turns it on).
- *(Recommended: A if Jay can check it on a touch screen and sliders still work; otherwise B.)*

### D5. An app's own "ribbon collapsed" setting at start

Since `ribbon-collapse-on-load`, the ribbon checks at start whether the window
is wide enough and collapses or expands itself. An app that starts the ribbon
collapsed on purpose (`IsCollapsed="True"`) sees it expand at start in a large
window.

- **A.** Keep as it is.
- **B.** At start, only collapse automatically, never un-collapse what the app set. *(Recommended.)*

### D6. Gallery filter that isn't in the filter list

Since `gallery-filters-reset-and-removed`, if an app selects a gallery filter
that isn't in the gallery's filter list, the gallery switches to the first
filter instead. This is rare.

- **A.** Keep as it is. *(Recommended: rare, and the old behaviour showed a filter that the menu didn't list.)*
- **B.** Go back to the old behaviour.

### D7. Image sizes written with a comma in code

Since `image-converter-invariant-size`, sizes like `16.5` in XAML are read the
same on every PC. A side effect: code that passes "16,5" as text on a German PC
now gets 165.

- **A.** Keep as it is. *(Recommended: XAML is the common case; code can pass a number instead of text.)*
- **B.** Accept both, guessing from the text.

### D8. Screen readers and KeyTips

Screen readers now get the KeyTip as the control's access key, but only the
last letter (for example "B" instead of "Alt, H, B").

- **A.** Keep as it is. *(Recommended for now: still more than before, when they got nothing.)*
- **B.** Look into reporting the full sequence. This is more work and needs testing with a screen reader.

## Already decided: documentation only

These are changes apps may notice. Nothing needs choosing, but they must be
written down where someone updating the package will look. They are listed in
`CHANGES-FOR-APPS.md`.

- Colour changes in the built-in themes.
- Changes that break automated UI tests (UI Automation types and tree).
- New theme keys that hand-written theme dictionaries must add.

## Being fixed now (plain bugs, no choice needed)

| Concern | Branch |
|---|---|
| Esc in KeyTip mode after an app cancelled `Backstage.Closing` shows the ribbon's KeyTips over the open Backstage | `fix/backstage-closing-cancel-keytip-back` |
| The toolbar copy of a TextBox commits an `UpdateSourceTrigger=Explicit` binding | `fix/textbox-qat-copy-explicit-binding` |
| With `AutomaticStateManagement=false`, the first tab may be re-selected on every Loaded | `fix/state-storage-manual-no-reselect` |
| Re-templating re-pins toolbar items in a different order and can drop some | `fix/ribbon-retemplate-repin-order` |

## Already resolved

- KeyTip row snapping using the wrong group's rows: fixed in #6 (issue #5).
- Focus when opening a drop down that starts with a ColorGallery: Jay's manual
  test of build #222 found drop downs and ColorGallery keyboard browsing
  working. This is not proven for every case, but no problem was seen.
