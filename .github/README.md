# Start here: this is nidarian's fork of Fluent.Ribbon

This repository is a **backup copy** of
[fluentribbon/Fluent.Ribbon](https://github.com/fluentribbon/Fluent.Ribbon)
(the original, maintained by batzen), plus **fixes developed here** for its
open issues.

> **About this fork.** This is an independent, unofficial fork. It is not
> affiliated with or endorsed by the original Fluent.Ribbon project or its
> maintainer, Bastian Schmidt. The fixes here were made with AI help (Claude),
> each one proven by a test that fails before the fix and passes after it, and
> the work was audited on 2026-10-06, problems included
> (`architecture/AUDIT.md`). The fixes are not offered to the original project.
> The original's MIT license and copyright notices are kept.

This page replaces the project's normal front page in this fork only. The
original project README is still here: [README.md](../README.md).

## The branches, and what each is for

| Branch | What's in it | Changes by itself? |
|---|---|---|
| `develop` (default) | The original project's `develop`, plus this fork's workflows and notes (`.github/`). **No code fixes.** | Yes, the weekly sync merges upstream in |
| `master` | Exact copy of the original's `master` | Yes, the weekly sync |
| `integration/all-fixes` | **All fixes together.** The NuGet package for my own app is built from this. | No, see "Keeping the fixes current" |
| `integration/next` | Where a new round of fixes is merged and built first. When its build passes, `integration/all-fixes` is moved up to it. Round 4 (2026-10-01) went this way. | No |
| `fix/...`, `feature/...`, `docs/...` | One fix each, with tests (`docs/` = doc comment fixes, no code) | No |
| `upstream-pr/...` | **Retired (2026-10-06).** The same fixes, cleaned up for sending to the original project, which won't take them. Kept as a record, not deleted. Nothing new goes here. | No |
| `archive` | Copies of the original's issues, pull requests and wiki | Yes, monthly |
| `fix/issue-357-keytip-placement` | My own fix from Feb 2026 (its fork PR #1 was closed on 2026-09-30 without merging: the fix is in `integration/all-fixes` and `upstream-pr/357-keytip-placement`) | No |

Why the fixes aren't in `develop`: it stays a clean copy of the original, so
the backup sync never hits merge conflicts. The fixes live only on the
`fix/...` branches and `integration/all-fixes`.

## What runs automatically (GitHub Actions, $0 on a public repo)

- **Sync from upstream**, Mondays: copies the original's new commits, branches
  and tags. It never deletes or overwrites anything.
- **Archive upstream**, the 3rd of each month: saves issues, the wiki and every
  NuGet package (Releases `archive-nuget`, `archive-releases`).
- **Build (Windows)**: builds and tests on every push to `develop` or `master`.

GitHub emails me if a run fails. `.github/FORK-BACKUP.md` explains what to do.

## Using the fixes in my app

1. Actions, **Build (Windows)**, latest run for `integration/all-fixes`, then
   download the **packages** artifact. Artifacts expire after 90 days, so re-run
   the build if it's gone.
2. Follow `.github/APP-REVIEW-CHECKLIST.md` (local NuGet source, version, what to test).
3. To go back: set Fluent.Ribbon back to the nuget.org version.

## Keeping the fixes current

`integration/all-fixes` does not follow upstream by itself. When the original
project has moved on, merge `develop` into `integration/all-fixes` and re-run
Build (Windows) on it. Or ask Claude to do it.

When merging, check the original's `Changelog.md` for bugs it fixed in its own
way. If it fixed one the fork also fixed, keep the original's version and drop
the fork's (revert that fix on `integration/all-fixes`), so the two don't
conflict later.

## Status of the fixes (as of 2026-10-07)

- **Not sent to the original project, and they won't be.** I asked for
  permission in [#1284](https://github.com/fluentribbon/Fluent.Ribbon/issues/1284)
  (closed 2026-09-30). The maintainer doesn't want AI-assisted contributions,
  so the fixes stay in this fork. Please respect that: don't open pull requests
  or post these fixes on the original project.
- This fork maintains its own fixes from now on. The library is MIT-licensed
  (`License.txt`): keep that file and its copyright lines in anything built
  from this fork.
- The full list, the evidence (test runs), and what's still to check by hand are
  in `.github/UPSTREAM-FIXES.md`. Round 4 (51 more fixes, including the items
  that used to be "maintainer's call") is in `.github/architecture/ROUND4.md`.

## Files in `.github/`

| File | Read it when |
|---|---|
| `README.md` | This page. Start here. |
| `FORK-BACKUP.md` | A sync or archive run failed |
| `UPSTREAM-FIXES.md` | What each early fix does and its proof. Written for sending upstream, which is no longer planned |
| `OPEN-ITEMS.md` | **What is still open:** bugs not fixed yet, small gaps, decisions, unchecked suspicions and checks for a person, on one page |
| `APP-REVIEW-CHECKLIST.md` | Trying the fixes in my app |
| `MEMORY-CHECK.md` | A program seems to eat memory over time: how to check for a leak with Task Manager, no coding needed |
| `architecture/` | Before changing the library: state diagrams of the 7 main parts, each claim tied to a code line. Run `python3 .github/scripts/check-citations.py` to check they still match the code. |
| `architecture/FINDINGS.md` | What the diagrams turned up: bugs fixed, rejected, and still unverified |
| `architecture/ACCESSIBILITY.md` | Screen reader, keyboard and contrast review: what's fixed, what's left, contrast ratios, High Contrast status |
| `architecture/DOCUMENTATION.md` | Doc comment audit: 41 corrected tooltips (blind-checked), the README SDK line, the outdated 2012 Walkthrough, what's left |
| `architecture/AUDIT.md` | **Read before relying on the fixes.** The 2026-10-06 audit: the evidence checked against GitHub, 4 problems and the concerns found, and what only a person can check |
| `architecture/AUDIT-DECISIONS.md` | The audit's open choices, in plain words, with a recommendation each: answer them to finish the follow-up |
| `architecture/CHANGES-FOR-APPS.md` | **Updating the package in an app:** colour changes, new theme keys, UI Automation and keyboard changes, new API |
| `architecture/ROUND4.md` | Round 4: the 51 fixes of 2026-10-01 (the former "maintainer's call" items, keyboard, contrast, screen readers, a new bug hunt), each with its fail and pass build |
| `architecture/HIGH-CONTRAST.md` | Turning on Windows High Contrast support, and what it doesn't cover yet |
| `architecture/ROUND5.md` | Round 5: the window, contextual tabs and galleries (11 fixes, each with its fail and pass build), what was rejected, and what's left for the owner |
| `architecture/ROUND6.md` | Round 6: input controls, simplified ribbon, status bar, tooltips, robustness (16 fixes), and what's still open |
| `architecture/MEMORY-LEAKS.md` | Memory leaks found and fixed (closed windows, removed ribbons, toolbar copies), and what was ruled out |
