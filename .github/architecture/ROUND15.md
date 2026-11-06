# Round 15: three bugs from a new audit

Done 2026-11-06. **AI (Claude) was used** for all of it. Nothing was sent to the
original project. The owner was travelling; the work followed the same rules.

How: a read-only bug hunt looked for new, testable problems. Its three
candidates were each proven by a tests-only build that fails in the new tests on
net462, net6.0 and net8.0, then fixed. Blind reviews (pessimistic and
adversarial) ran until two rounds in a row were clean.

## Fixed

| Problem | What a user saw | Branch | Tests only → with fix |
|---|---|---|---|
| A click, focus or KeyTip on a backstage page, selecting the next page after the selected one is removed, a click on a gallery item, and a non-selectable gallery unselecting the clicked item replaced an app's explicit OneWay binding on `IsSelected` (round 8 had checked only TwoWay bindings set in a style) | After using the page or item once, the app could no longer select it | `fix/items-isselected-oneway` | #579, #589 (non-selectable galleries), RED3 (page removal) → #582, #590, GREEN3 |
| UI Automation Toggle on the checked button of a group (`GroupName`, like Left/Center/Right) unchecked it; a click keeps it checked (issue #481). Same for a checkable menu item in a group | A screen reader user could leave a group with nothing checked | `fix/grouped-toggle-uia` | #580 → #586 |
| Typing in the Quick Access Toolbar copy of a `TextBox` replaced an app's OneWay binding on the original's `Text` (the combo box had the same bug, fixed in round 7) | After typing in the toolbar copy, the text box no longer followed the app | `fix/textbox-qat-oneway-text` | #581 → #585 |

**Default chosen (owner can change it):** UI Automation Toggle on the checked
item of a group keeps its state but, unlike a click, doesn't raise Click or run
the item's command (WPF's Toggle never did).

A review asked whether typing in the toolbar copy now resets the caret (the
original's text comes back to the copy). A test that types in the middle of the
copy showed the selection stays put.

## Deferred (tracked)

- Same pattern, not proven or not reachable by a user here: `BackstageTabControl`
  sets its own `SelectedItem` with the CLR setter in one place (an app's OneWay
  binding on `SelectedItem` would be replaced); `ColorGallery` selects its
  internal items that way; a click on a ribbon tab without a tab control
  (`RibbonTabItem`, already tracked since round 12).
- UI Automation Toggle on a grouped split button raises Click and runs the
  command (its own peer, older); on a grouped toggle button or menu item it now
  doesn't (default above).
- The backstage page's focus path is fixed with the others but has no test of
  its own.
- A value an app sets with a fixed style setter or trigger (not a binding) is no
  longer pinned by a click or by typing in the toolbar copy; if the style is
  applied again, its value comes back.

COMBINED_AND_REVIEWS
