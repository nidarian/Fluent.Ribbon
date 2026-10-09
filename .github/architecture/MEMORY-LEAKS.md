# Memory leaks (2026-10-09)

A leak here means: something that lives longer keeps a closed window, a removed
ribbon or a discarded control reachable, so .NET can never free it. Nobody sees
an error; the app just uses more memory the longer it runs. **Nothing here is
posted upstream.**

How it was done:

1. A read-only search of the library for the usual WPF leak patterns
   (subscriptions to longer-lived objects, static fields, window events).
2. For each candidate, a test that creates the object, lets go of it, runs the
   garbage collector and checks it is gone (a `WeakReference`). Each test file
   also has a baseline that is collected, so a pass means something.
3. Tests only must fail on Windows CI on net462, net6.0 and net8.0, then the fix
   must pass.

## Fixed

| Leak | What an app does | Branch | Tests only → with fix |
|---|---|---|---|
| **The shared ribbon context menu keeps a closed window.** All ribbons of a thread share one static context menu. Its items kept the last ribbon as command target, and while open the menu took over values from the right-clicked control (its window, its DataContext) that WPF never reset. | The user right-clicks the ribbon or toolbar in a window, then closes the window. It stayed in memory until another ribbon's menu was opened, possibly until the app exits. | `fix/leak-ribbon-context-menu-keeps-window` | #291/#294 → #297 still failed (clearing the command target was not enough), #299 showed why, #301 passed. |
| **Context menu items showed disabled right after opening** (found while fixing the leak above; it also affected the very first opening before). The items only re-checked their commands on the next input. | Opens the ribbon's context menu. | same branch | #302 → #303 |
| **Quick Access Toolbar copies stay alive as long as the original control.** The GroupStyle sync of a copy was subscribed strongly to the original. Affects DropDownButton, SplitButton, MenuItem with items, ComboBox, InRibbonGallery, RibbonGroupBox, ApplicationMenu. | Adds and removes toolbar items, loads ribbon state or re-templates the ribbon. Every old copy stayed until the window closed. | `fix/leak-qat-copy-groupstyle-sync` | #290 (3 of 7 failed: the test was too weak), #293 (all 7 fail) → #296 |
| **A ribbon removed from a window that stays open stays alive** through the window's `Closed` event. | Swaps or removes the ribbon (`window.Content = ...`). | `fix/leak-removed-ribbon-window-closed` | #289 → #292 |
| **A RibbonWindow's title bar keeps a removed ribbon's toolbar**, and with it the ribbon. | Same, in a `RibbonWindow`. | same branch | #295 → #300 |

## Checked and rejected

KeyTipService (window events, the window hook and its timer are removed on
unload), the Backstage (its window handlers are removed on unload),
`ColorGallery.RecentColors` (only used through bindings, which are weak),
`ScreenTip.HelpPressed` (a static event the library only raises), the ribbon's
own collections (created and owned by the control, so they die together),
PopupService, WindowSteeringHelper and DropDownHelper (no static caches).

## Known and left alone

- SplitButton, InRibbonGallery and ComboBox keep their **latest** toolbar copy
  in a field. That is at most one copy per control, so memory doesn't grow.
- Not tested: a ribbon moved to a different window, and real mouse input.
