#!/usr/bin/env bash
#
# sync-upstream.sh
#
# Copies new commits, branches and tags from the original project ("upstream")
# into this fork ("origin") WITHOUT ever deleting or overwriting anything here.
#
# Why this exists: this fork is a safety copy in case the original repository
# disappears. A copy is only useful if it stays current, so this script is run
# on a schedule by .github/workflows/sync-upstream.yml.
#
# The rules, per upstream branch:
#   * branch missing in the fork          -> create it
#   * fork branch is behind upstream      -> fast-forward it (no merge commit)
#   * fork branch is ahead of upstream    -> leave it (we already have everything)
#   * both sides have unique commits:
#       - branch is in MERGE_BRANCHES     -> merge upstream in (merge commit)
#       - any other branch                -> leave it and report a warning
#   * branch deleted upstream             -> keep ours (that's the whole point)
#
# Why "develop" gets merged instead of fast-forwarded: develop is this fork's
# default branch, and it carries the fork's own .github/workflows files that
# upstream doesn't have. So develop can never be an exact copy of upstream
# again; merging keeps both our files and theirs.
#
# Tags: new upstream tags are copied. A tag that exists on both sides but
# points at different commits is NEVER overwritten. Tags are supposed to be
# permanent, so a moved tag is suspicious and gets reported instead.
#
# Environment variables (all optional):
#   UPSTREAM_URL    git URL of the original project
#   MERGE_BRANCHES  space-separated branches allowed to receive merge commits
#   DRY_RUN=1       print what would be pushed, push nothing
#
# Exit code is non-zero if anything needed a human (conflict, divergence,
# moved tag). On GitHub that marks the run as failed, and GitHub emails the
# repo owner about failed scheduled runs, which works as the alert.

set -euo pipefail

UPSTREAM_URL="${UPSTREAM_URL:-https://github.com/fluentribbon/Fluent.Ribbon.git}"
MERGE_BRANCHES="${MERGE_BRANCHES:-develop}"
DRY_RUN="${DRY_RUN:-0}"

# Collected results, printed at the end (and into the GitHub run summary).
changes=()
problems=()

log()  { echo "[sync] $*"; }
note() { changes+=("$*"); log "$*"; }
warn() { problems+=("$*"); echo "::warning::$*"; }

# Pushes one ref, or just prints it when DRY_RUN=1. Pushes go one ref at a
# time so a single rejected ref can't block all the others.
push_ref() {
  local src="$1" dst="$2"
  if [[ "$DRY_RUN" == "1" ]]; then
    log "DRY RUN: would push $src -> $dst"
    return 0
  fi
  git push origin "$src:$dst"
}

is_merge_branch() {
  local b="$1" m
  for m in $MERGE_BRANCHES; do [[ "$m" == "$b" ]] && return 0; done
  return 1
}

# --- 1. Fetch both sides ------------------------------------------------------

# (Re)point the "upstream" remote, so the script works in a fresh CI checkout
# and in a local clone that already has the remote.
if git remote get-url upstream >/dev/null 2>&1; then
  git remote set-url upstream "$UPSTREAM_URL"
else
  git remote add upstream "$UPSTREAM_URL"
fi

# Fetch the fork's own branches so we compare against what is really on GitHub,
# not a stale local copy.
git fetch --quiet --prune origin '+refs/heads/*:refs/remotes/origin/*'

# Upstream tags go into a private namespace (refs/upstream-tags/*) instead of
# refs/tags/*. A plain `git fetch --tags` would fail or clobber if a tag differs
# between the two repos. Keeping them apart lets us compare them safely.
git fetch --quiet --prune upstream \
  '+refs/heads/*:refs/remotes/upstream/*' \
  '+refs/tags/*:refs/upstream-tags/*'

# The fork's current tags, read straight from GitHub. `^{}` "peels" annotated
# tags down to the commit they point at, so we compare commits, not tag objects.
declare -A origin_tags=()
while read -r sha ref; do
  name="${ref#refs/tags/}"
  if [[ "$name" == *'^{}' ]]; then
    origin_tags["${name%'^{}'}"]="$sha"          # peeled value wins
  elif [[ -z "${origin_tags[$name]:-}" ]]; then
    origin_tags["$name"]="$sha"
  fi
done < <(git ls-remote --tags origin)

# --- 2. Branches --------------------------------------------------------------

# for-each-ref lists every upstream branch; lstrip=3 turns
# refs/remotes/upstream/<name> into just <name> (names containing "/" stay whole).
while read -r branch; do
  [[ "$branch" == "HEAD" ]] && continue

  up_sha=$(git rev-parse "refs/remotes/upstream/$branch")

  if ! git show-ref --verify --quiet "refs/remotes/origin/$branch"; then
    note "new branch '$branch' -> ${up_sha:0:8}"
    push_ref "$up_sha" "refs/heads/$branch"
    continue
  fi

  our_sha=$(git rev-parse "refs/remotes/origin/$branch")

  if [[ "$our_sha" == "$up_sha" ]]; then
    continue                                            # already identical
  fi

  # merge-base --is-ancestor A B asks "is A already contained in B's history?"
  if git merge-base --is-ancestor "$our_sha" "$up_sha"; then
    note "fast-forward '$branch' ${our_sha:0:8} -> ${up_sha:0:8}"
    push_ref "$up_sha" "refs/heads/$branch"
  elif git merge-base --is-ancestor "$up_sha" "$our_sha"; then
    log "'$branch' is ahead of upstream, nothing to take"
  elif is_merge_branch "$branch"; then
    # Merge in a temporary detached checkout of the fork's branch. --no-ff
    # always records a merge commit, so the history shows when syncs happened.
    git checkout --quiet --detach "$our_sha"
    if git merge --no-ff --no-edit -m "Merge upstream '$branch' (${up_sha:0:8}) into fork" "$up_sha"; then
      note "merged upstream into '$branch' -> $(git rev-parse --short HEAD)"
      push_ref "HEAD" "refs/heads/$branch"
    else
      git merge --abort
      warn "'$branch': merge conflict with upstream ${up_sha:0:8}, needs a manual merge"
    fi
  else
    warn "'$branch' has diverged from upstream (fork ${our_sha:0:8}, upstream ${up_sha:0:8}); left untouched"
  fi
done < <(git for-each-ref --format='%(refname:lstrip=3)' refs/remotes/upstream/)

# --- 3. Tags ------------------------------------------------------------------

while read -r tag; do
  # Peel to the commit, the same way as for origin_tags above.
  up_commit=$(git rev-parse "refs/upstream-tags/$tag^{}")
  ours="${origin_tags[$tag]:-}"

  if [[ -z "$ours" ]]; then
    note "new tag '$tag'"
    push_ref "refs/upstream-tags/$tag" "refs/tags/$tag"
  elif [[ "$ours" != "$up_commit" ]]; then
    warn "tag '$tag' points to ${up_commit:0:8} upstream but ${ours:0:8} here; kept ours"
  fi
done < <(git for-each-ref --format='%(refname:lstrip=2)' refs/upstream-tags/)

# --- 4. Report ----------------------------------------------------------------

summary="## Upstream sync"$'\n'
if (( ${#changes[@]} == 0 )); then
  summary+=$'\n'"Nothing new upstream."$'\n'
else
  summary+=$'\n'"### Copied"$'\n'
  for c in "${changes[@]}"; do summary+="- $c"$'\n'; done
fi
if (( ${#problems[@]} > 0 )); then
  summary+=$'\n'"### Needs attention"$'\n'
  for p in "${problems[@]}"; do summary+="- $p"$'\n'; done
fi

echo "$summary"
# GITHUB_STEP_SUMMARY only exists inside GitHub Actions; it renders on the run page.
[[ -n "${GITHUB_STEP_SUMMARY:-}" ]] && echo "$summary" >> "$GITHUB_STEP_SUMMARY"

# Tell the workflow whether develop moved, so it can start a build.
if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
  if printf '%s\n' "${changes[@]:-}" | grep -q "'develop'"; then
    echo "develop_changed=true" >> "$GITHUB_OUTPUT"
  else
    echo "develop_changed=false" >> "$GITHUB_OUTPUT"
  fi
fi

(( ${#problems[@]} == 0 ))
