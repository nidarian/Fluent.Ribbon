#!/usr/bin/env python3
"""
export-issues.py

Turns the raw GitHub API dumps made by archive-upstream.yml into Markdown
files people can read without any tools:

    <out>/issues/00042.md   one file per issue or pull request, with all comments
    <out>/INDEX.md          a table of every issue: number, state, title

Why: raw JSON is the complete, lossless backup, but it's unreadable. If the
original project ever vanishes, these Markdown files are what you'll actually
browse on GitHub to find "how did someone fix X in 2019?".

Usage:
    export-issues.py <dump-dir> <out-dir>

<dump-dir> must contain (written by the workflow via `gh api --paginate`):
    issues.json           every issue AND pull request (GitHub treats PRs as issues)
    issue-comments.json   every conversation comment, on issues and PRs
    review-comments.json  every inline code-review comment on PRs
"""

import json
import sys
from collections import defaultdict
from pathlib import Path


def load(path: Path):
    # A missing file just means "nothing of that kind", not an error.
    if not path.exists():
        return []
    return json.loads(path.read_text(encoding="utf-8"))


def number_from_url(url: str) -> int:
    # Comments don't carry the issue number directly, only a URL ending in it,
    # e.g. https://api.github.com/repos/o/r/issues/42 -> 42
    return int(url.rstrip("/").rsplit("/", 1)[-1])


def who(obj) -> str:
    # "user" can be null for comments from deleted accounts ("ghost").
    return (obj.get("user") or {}).get("login", "ghost")


def main() -> None:
    dump, out = Path(sys.argv[1]), Path(sys.argv[2])
    issues = load(dump / "issues.json")

    # Group every comment under the issue it belongs to.
    # defaultdict(list) creates the empty list on first access.
    comments = defaultdict(list)
    for c in load(dump / "issue-comments.json"):
        comments[number_from_url(c["issue_url"])].append(("comment", c))
    for c in load(dump / "review-comments.json"):
        comments[number_from_url(c["pull_request_url"])].append(("review", c))

    issue_dir = out / "issues"
    issue_dir.mkdir(parents=True, exist_ok=True)

    index_rows = []
    for issue in sorted(issues, key=lambda i: i["number"]):
        n = issue["number"]
        kind = "PR" if "pull_request" in issue else "Issue"
        labels = ", ".join(l["name"] for l in issue.get("labels", []))

        lines = [
            f"# {kind} #{n}: {issue['title']}",
            "",
            f"- **State:** {issue['state']}",
            f"- **Author:** {who(issue)}",
            f"- **Created:** {issue['created_at']}",
            f"- **Closed:** {issue.get('closed_at') or '-'}",
            f"- **Labels:** {labels or '-'}",
            f"- **Original:** {issue['html_url']}",
            "",
            issue.get("body") or "_No description._",
        ]

        # ISO-8601 timestamps sort correctly as plain strings.
        for kind_of_comment, c in sorted(comments[n], key=lambda kc: kc[1]["created_at"]):
            header = f"## {who(c)} commented on {c['created_at']}"
            if kind_of_comment == "review":
                # Inline review comments are attached to a line of a file.
                header += f" (code review on `{c.get('path', '?')}`)"
            lines += ["", "---", "", header, "", c.get("body") or ""]

        # Zero-padded names so the files list in numeric order.
        (issue_dir / f"{n:05d}.md").write_text("\n".join(lines) + "\n", encoding="utf-8")

        # "|" would break the Markdown table, so escape it in titles.
        title = issue["title"].replace("|", "\\|")
        index_rows.append(f"| [{n}](issues/{n:05d}.md) | {kind} | {issue['state']} | {title} |")

    index = [
        "# Fluent.Ribbon issue archive",
        "",
        f"{len(issues)} issues and pull requests copied from the original project.",
        "",
        "| # | Type | State | Title |",
        "|---|------|-------|-------|",
        *index_rows,
    ]
    (out / "INDEX.md").write_text("\n".join(index) + "\n", encoding="utf-8")
    print(f"wrote {len(issues)} issue files to {issue_dir}")


if __name__ == "__main__":
    main()
