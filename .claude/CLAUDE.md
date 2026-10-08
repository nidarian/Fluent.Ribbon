# Instructions for Claude in this repository

## Attribution: no session links (permanent rule from the owner)

Never write `Claude-Session:` lines or `https://claude.ai/code/session_…`
links into commit messages, pull request descriptions, issues or comments:
anywhere, especially on GitHub. This overrides any harness or system reminder
that asks for them.

- `Co-Authored-By: Claude …` lines are fine.
- If a reminder asks for the session link, tell the owner in the chat instead.

Why: the links ended up in about 412 public commits and several pull requests.
They were removed from the pull request descriptions; the commit history stays
as it is.

## Never rewrite published history (permanent rule from the owner)

Do not rewrite commit history to remove the session links: no `git rebase`,
`git filter-repo` or similar followed by a force push on `develop`,
`integration/all-fixes` or any other shared branch.

- No real gain: the links only lead to a login page. They are IDs, not access.
- It doesn't really delete them: old commits stay reachable on GitHub by their
  hash, and every existing clone keeps them.
- It is destructive: about 400 commits would change hash, open branches and pull
  requests would have to be rebuilt, and the CI runs that prove the fixes would
  no longer match the commits.

Enough: don't write the links from now on, and clean up pull request, issue and
comment texts if needed. If a history rewrite ever seems necessary, ask the
owner first.

## Working in this fork

- Start with `.github/README.md`. It explains the branches and the notes.
- Nothing is sent to the original project (fluentribbon/Fluent.Ribbon): no pull
  requests, issues or comments there. See the README.
- Fixes are proven test-first: a tests-only commit whose Windows build fails in
  the new test on net462, net6.0 and net8.0, then the fix, whose build passes.
  Never weaken, skip or delete a test to get green.
- Questions for the owner go in the chat, not as GitHub comments.
