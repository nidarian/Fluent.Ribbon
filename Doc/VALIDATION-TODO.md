# Documentation Validation Checklist

**Status:** NOT YET VALIDATED
**Last Updated:** 2026-01-21

## Purpose

These recovered documents are historical artifacts from 2018. The codebase has evolved significantly since then. Before relying on this documentation, each item should be validated against the current code.

**Do not blindly trust these docs.** They are preserved for reference, not as authoritative guides.

## Validation Tasks

### Foundation.md

- [ ] **RibbonWindow class** - Does it still exist? Same namespace?
- [ ] **Non-DWM style handling** - Is Windows XP support still relevant/present?
- [ ] **Theme setup in App.xaml** - Is the `pack://` URI path still correct?
- [ ] **Theme file locations** - Do `Themes/Office2010/Silver.xaml` etc. still exist?
- [ ] **XAML namespace** - Is `xmlns:Fluent="clr-namespace:Fluent;assembly=Fluent"` still valid?
- [ ] **Backstage syntax** - Is `<Fluent:Ribbon.Menu><Fluent:Backstage>` still the pattern?
- [ ] **Quick Access Toolbar** - Does `IQuickAccessToolbarItem` interface still exist?

### KeyboardAccess.md

- [ ] **KeyTip system** - Is the described keyboard navigation still accurate?
- [ ] **Access key syntax** - Are the XAML attributes the same?

### RibbonResizing.md

- [ ] **Size states** - Are Large/Middle/Small/Collapsed still the states?
- [ ] **RibbonGroupBox behavior** - Does resizing still work as described?

### ScreenTips.md

- [ ] **ScreenTip class** - Does it still exist with same properties?
- [ ] **Attachment syntax** - Is `Fluent:ScreenTip.Attach` still the pattern?

### Screenshots.md

- [ ] **Image paths** - Do referenced screenshots still exist in the repo?
- [ ] **Visual accuracy** - Do the screenshots match current appearance?

## How to Validate

1. Pick an item from the checklist above
2. Search the current codebase for the referenced class/property/pattern
3. Compare what you find against what the doc claims
4. If accurate: check the box and add a note with current file location
5. If outdated: check the box, note what changed, consider updating the doc

## Validation Log

| Date | Validator | Item | Result | Notes |
|------|-----------|------|--------|-------|
| | | | | |

## When Validation is Complete

Once all items are checked:
1. Update the Status at the top of this file to "VALIDATED AS OF [date]"
2. Note any major discrepancies in the RECOVERY-REPORT.md
3. Consider whether validated docs should be proposed upstream or kept fork-only
