#!/usr/bin/env python3
"""
check-citations.py

Checks that the architecture notes in .github/architecture/ only claim what
the code actually says.

Every note has an "Evidence" table:

    | ID | Claim | Location | Code |
    | E1 | ...   | `Fluent.Ribbon/Controls/Ribbon.cs:650` | `private static void OnIsSimplifiedChanged(` |

For each row this script opens the file, looks at the cited line and the 3
lines on either side, and checks that the quoted code is really there.
Whitespace is ignored, so re-indenting doesn't break a citation.

It also checks that every evidence id used in a diagram, like "[E7]", has a
row in the table, so no diagram arrow is left unsupported.

Why: an earlier AI-written analysis of this library was wrong in ways anyone
could see by opening the code. This makes "look at the code" automatic.

Usage:
    python3 .github/scripts/check-citations.py            # all notes
    python3 .github/scripts/check-citations.py FILE.md    # one note

Exit code 1 if any citation fails, so it can run in CI.
"""

import re
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
NOTES = REPO / ".github" / "architecture"
TOLERANCE = 3  # lines of drift allowed around the cited line

# A table row: | E12 | claim | `path:line` | `code` |
ROW = re.compile(r"^\|\s*(E\d+)\s*\|(.*?)\|\s*`([^`]+?):(\d+)`\s*\|\s*`(.+?)`\s*\|\s*$")
# An id referenced anywhere else in the text, e.g. "[E3]" or "[E3, E4]".
REF = re.compile(r"\bE\d+\b")


def squash(text: str) -> str:
    # Remove all whitespace, so indentation and spacing differences don't matter.
    return re.sub(r"\s+", "", text)


def check_note(note: Path) -> list[str]:
    problems = []
    lines = note.read_text(encoding="utf-8").splitlines()

    defined = {}
    for number, line in enumerate(lines, 1):
        match = ROW.match(line)
        if not match:
            continue

        eid, _claim, path, line_no, code = match.groups()
        if eid in defined:
            problems.append(f"{note.name}:{number}: {eid} is defined twice")
        defined[eid] = number

        source = REPO / path
        if not source.is_file():
            problems.append(f"{note.name}:{number}: {eid} cites a missing file: {path}")
            continue

        source_lines = source.read_text(encoding="utf-8-sig", errors="replace").splitlines()
        target = int(line_no)
        start = max(0, target - 1 - TOLERANCE)
        end = min(len(source_lines), target + TOLERANCE)
        window = squash("".join(source_lines[start:end]))

        if squash(code) not in window:
            actual = source_lines[target - 1].strip() if 0 < target <= len(source_lines) else "<past end of file>"
            problems.append(
                f"{note.name}:{number}: {eid} quote not found near {path}:{target}\n"
                f"      quoted: {code}\n"
                f"      actual: {actual}"
            )

    # Every id used outside the evidence table must exist in it.
    for number, line in enumerate(lines, 1):
        if ROW.match(line):
            continue
        for eid in REF.findall(line):
            if eid not in defined:
                problems.append(f"{note.name}:{number}: {eid} is referenced but has no evidence row")

    if not defined:
        problems.append(f"{note.name}: no evidence rows found (table format wrong?)")

    return problems, len(defined)


def main() -> int:
    notes = [Path(a) for a in sys.argv[1:]] or sorted(NOTES.glob("*.md"))
    failed = 0
    for note in notes:
        problems, count = check_note(note)
        status = "OK  " if not problems else "FAIL"
        print(f"{status} {note.name}: {count} evidence rows, {len(problems)} problem(s)")
        for problem in problems:
            print("   ", problem)
        failed += bool(problems)
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
