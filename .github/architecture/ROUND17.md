# Round 17: the ribbon tab switch follows "Animation effects" in Windows

Done 2026-11-18. **AI (Claude) was used** for all of it. Nothing was sent to the
original project.

How: the second item of the read-only audit for accessibility gaps (WCAG 2.3.3,
animation from interactions). Tests were written first and seen failing, then
the fix was written. Blind reviews (pessimistic and adversarial) ran until two
rounds in a row were clean, followed by the two bias-proof checks described at
the end.

## Fixed

| Problem | Who is affected | Branch | Failing build → passing build |
|---|---|---|---|
| Switching ribbon tabs always slid and faded the new tab's content in, even when "Animation effects" is turned off in Windows (Settings, Accessibility, Visual effects). The backstage already followed that setting | People who turn animations off, often because of motion sensitivity | `fix/tab-transition-reduced-motion-r17` | #633 → #634 |

With the setting off, the new tab's content is shown at once and the old content
is gone at once. With it on, nothing changes. The tab switch follows the setting
also when it is changed while the app runs.

How: `TransitioningControl` (the part of the ribbon that shows the selected tab's
content) has a new property `AreAnimationsEnabled`, on by default. The ribbon's
tab control template links it to the Windows setting, the same way the backstage
links its own `AreAnimationsEnabled`.

Proof: build **#633** runs the tests on the code from before the round (tests
only). On each of net462, net6.0 and net8.0 the same three tests fail and
nothing else: the transition follows the Windows setting, it follows a change of
the setting while the app runs, and with animations off the new content is shown
at once with nothing animated. A fourth test is a guard: with animations on the
transition still runs (it passes before and after). Build **#634** is the fix
with the same tests; everything passes.

One test was corrected after a build: its first version checked how WPF stores
the link to the setting, which WPF does differently inside a control template, so
it failed even with the fix (#632) although the value was right. It was replaced
by a test that changes the setting while the app runs and checks that the tab
switch follows. Both builds (#633, #634) were run again with the corrected test.

## Deferred (tracked)

- Turning animations off while a tab switch is playing: the old content is gone
  at once, but the new content finishes its slide and fade (at most about 0.2
  seconds). This was so before the round (the transition can't be stopped
  half-way; the line that would stop it was already turned off). Not tested.
- An app that uses `TransitioningControl` on its own, or has its own copy of the
  ribbon's tab control template, keeps the old behaviour (always animated) unless
  it links `AreAnimationsEnabled` to the setting itself, for example
  `AreAnimationsEnabled="{DynamicResource {x:Static SystemParameters.ClientAreaAnimationKey}}"`.
- The ribbon's own template sets the property, so a style can't override it. An
  app that wants a different value for the ribbon puts its own value for
  `SystemParameters.ClientAreaAnimationKey` into the resources of the ribbon or
  window.
- In a remote desktop session Windows may report animations as off; the tab
  switch then has no animation there, like the backstage. Not tested.
- Not tested: turning the setting off mid-transition, and an animated switch
  followed by a switch with animations off.

## Checks

Blind reviews: round 1 (pessimistic) and round 2 (adversarial), one reviewer
each: no bugs. **Two clean rounds in a row.** Their notes are in "Deferred" above
and in `CHANGES-FOR-APPS.md`.

Broken copies: six copies of the fix were each broken on purpose in one
realistic way and built with the tests. Five were caught on each of the three
.NET versions: the link to the setting removed from the template (#635), the
setting ignored for tab content (#636), the new content not shown with
animations off (#637; 16 tests failed: the animations-off test, the guard test
and 14 other ribbon tests, because the build machine has animations turned off
in Windows, so with this mistake every ribbon there showed no tab content), the link pointing to the wrong Windows setting (#639), and the
old content kept with animations off (#640). One was not caught: removing the
clean-up when the setting is turned off during a running transition (#638). That
is the untested case listed under "Deferred".

Blind comparison: a reviewer got the old and the new version of the two changed
files under neutral names in random order, with only the requirement, and had to
describe both in seven situations. It chose the new version and found no break
for existing apps; its remaining points are the ones under "Deferred".
