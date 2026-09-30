# Start here: this is nidarian's fork of Fluent.Ribbon

This repository is a **backup copy** of
[fluentribbon/Fluent.Ribbon](https://github.com/fluentribbon/Fluent.Ribbon)
(the original, maintained by batzen), plus **fixes developed here** for its
open issues.

This page replaces the project's normal front page in this fork only. The
original project README is still here: [README.md](../README.md).

## The branches, and what each is for

| Branch | What's in it | Changes by itself? |
|---|---|---|
| `develop` (default) | The original project's `develop`, plus this fork's workflows and notes (`.github/`). **No code fixes.** | Yes, the weekly sync merges upstream in |
| `master` | Exact copy of the original's `master` | Yes, the weekly sync |
| `integration/all-fixes` | **All fixes together.** The NuGet package for my own app is built from this. | No, see "Keeping the fixes current" |
| `fix/...`, `feature/...` | One fix each, with tests | No |
| `upstream-pr/...` | The same fixes, cleaned up for sending to the original project | No |
| `archive` | Copies of the original's issues, pull requests and wiki | Yes, monthly |
| `fix/issue-357-keytip-placement` | My own fix from Feb 2026, fork PR #1 | No |

Why the fixes aren't in `develop`: it stays a clean copy of the original, so
the backup sync never hits merge conflicts. That matters most once the
original project takes the fixes in its own form.

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

## Status of the fixes (as of 2026-09-30)

- **Not sent to the original project.** I asked the maintainer for permission in
  [#1284](https://github.com/fluentribbon/Fluent.Ribbon/issues/1284). Nothing is
  posted there until he agrees.
- The full list, the evidence (test runs), and what's still to check by hand are
  in `.github/UPSTREAM-FIXES.md`.
- Before sending anything: check whether the original project already fixed it
  in the meantime (see its Changelog.md).

## Files in `.github/`

| File | Read it when |
|---|---|
| `README.md` | This page. Start here. |
| `FORK-BACKUP.md` | A sync or archive run failed |
| `UPSTREAM-FIXES.md` | Sending fixes to the original project |
| `APP-REVIEW-CHECKLIST.md` | Trying the fixes in my app |
| `architecture/` | Before changing the library: state diagrams of the 7 main parts, each claim tied to a code line. Run `python3 .github/scripts/check-citations.py` to check they still match the code. |
| `architecture/FINDINGS.md` | What the diagrams turned up: bugs fixed, rejected, and still unverified |
