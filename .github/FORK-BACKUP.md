# About this fork

This repository is a backup copy of
[fluentribbon/Fluent.Ribbon](https://github.com/fluentribbon/Fluent.Ribbon),
kept in case the original is ever deleted or made private. Three GitHub
Actions workflows keep it current:

| Workflow | When | What it does |
|---|---|---|
| `sync-upstream.yml` | Mondays, 06:17 UTC | Copies new commits, branches and tags. Never deletes or overwrites. Merges upstream into `develop`, because `develop` holds these fork-only files. |
| `archive-upstream.yml` | 3rd of each month | Saves issues, PRs, comments and the wiki to the `archive` branch. Saves every NuGet package and upstream release file to the `archive-nuget` / `archive-releases` releases. |
| `build.yml` | On push, on PRs, and after a sync | Builds and tests on Windows, which WPF requires. |

## If a run fails

GitHub emails you when a scheduled run fails. The run page's summary lists
what needs attention, for example:

- **merge conflict on develop**: merge `upstream/develop` by hand.
- **branch diverged**: someone committed to that branch here; decide which
  side to keep.
- **tag moved upstream**: the original re-pointed a release tag. Ours is kept
  on purpose. Check what changed before trusting the new one.
- **push rejected for a workflow file**: upstream changed `.github/workflows/`.
  Add a `SYNC_TOKEN` secret (a personal access token with the `workflow`
  scope) and re-run.

## Manual sync from a local clone

```sh
git remote add upstream https://github.com/fluentribbon/Fluent.Ribbon.git
bash .github/scripts/sync-upstream.sh            # DRY_RUN=1 to preview
```
