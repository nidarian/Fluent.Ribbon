# Documentation Recovery Report

**Date:** 2026-01-21
**Recovered by:** Jay + Claude (nidarian fork)
**Branch:** `docs-archive`

## Summary

This branch preserves documentation that was deleted from the upstream repository. These files contain valuable getting-started tutorials and reference material that is no longer available in the main codebase or fully replicated on the external documentation website.

## What Was Deleted

| File | Lines | Content |
|------|-------|---------|
| `Doc/Foundation.md` | 136 | Getting started guide: RibbonWindow setup, themes, basic XAML examples |
| `Doc/KeyboardAccess.md` | 55 | Keyboard navigation and access key documentation |
| `Doc/RibbonResizing.md` | 49 | How ribbon controls resize at different widths |
| `Doc/ScreenTips.md` | 71 | Tooltip/screentip implementation guide |
| `Doc/Screenshots.md` | 56 | Screenshot gallery with descriptions |

**Total: 367 lines of documentation**

## When and Why It Was Deleted

**Commit:** `192208f82626e09705986c455ffcb273e5e460d2`
**Date:** January 17, 2018
**Author:** Bastian Schmidt (batzen@gmx.org)
**Message:** "Removing documentation from this repo - Documentation is available at the website repo https://github.com/fluentribbon/fluentribbon.github.io"

## Why This Matters

The external website (https://fluentribbon.github.io/documentation/) exists but does not contain all the content that was deleted:

- No dedicated "Foundation" getting-started guide
- No RibbonWindow-specific setup tutorial
- The detailed XAML code examples from Foundation.md appear to be missing
- Website structure focuses on concepts/controls rather than step-by-step tutorials

### Knowledge at Risk

The deleted `Foundation.md` contained:

1. **Non-DWM style handling** - How to support Windows XP and basic Windows themes (legacy but valuable for understanding the codebase)
2. **Complete working XAML examples** - Copy-paste-ready code for basic ribbon setup
3. **Theme setup in App.xaml** - The critical "BEWARE" note about including themes
4. **Quick Access Toolbar explanation** - How items get added via context menu

This is the kind of "getting started" content that helps new users understand the library quickly.

## Investigation Details

This recovery was triggered by an investigation into upstream maintenance patterns. The concern: when maintainers "clean up" documentation, they sometimes remove institutional knowledge that isn't fully captured elsewhere.

### Commands Used to Investigate

```bash
# Find commits with large doc deletions
git log --all --numstat --oneline -- "*.md" | awk '...'

# Find the deletion commit
git log --all --oneline --diff-filter=D -- "Doc/*.md"

# View deleted content
git show 192208f8^:Doc/Foundation.md
```

### Verification

The external website was checked on 2026-01-21. While it has documentation sections for Getting Started, Concepts, Controls, and Styling, it lacks the specific tutorial-style content that was in these deleted files.

## Preservation Strategy

This `docs-archive` branch:
1. Restores all 5 deleted markdown files to their original location
2. Contains this report for future reference
3. Should NOT be merged to develop/main - it exists for reference only
4. Can be consulted when the external docs are unclear or incomplete

## Related Upstream Changes (As of 2026-01-21)

The upstream `develop` branch is 5 commits ahead of this fork's `develop`:
- Dependency version bumps (ControlzEx, CsWin32, XAMLTools)
- Version number correction (11.1.0 -> 11.0.0)
- Changelog markdown formatting

These are maintenance commits with no documentation deletions.

## Fork Remotes

| Remote | URL | Purpose |
|--------|-----|---------|
| origin | https://github.com/fluentribbon/Fluent.Ribbon.git | Upstream (currently misconfigured as origin) |
| gitea | https://git.itcraft-support.com/ClaudeCode/Fluent.Ribbon.git | Backup |
| (missing) | https://github.com/nidarian/Fluent.Ribbon | Jay's GitHub fork (should be origin) |

**Note:** The remotes on `develop` should be reconfigured so `origin` points to nidarian's fork and `upstream` points to fluentribbon.
