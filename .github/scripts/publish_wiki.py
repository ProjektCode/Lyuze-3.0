#!/usr/bin/env python3
"""Publish the repository's allowlisted public Markdown files to the GitHub Wiki."""

from __future__ import annotations

import argparse
import posixpath
import re
from pathlib import Path

GENERATED_MARKER = ".lyuze-generated-pages"

PAGE_MAP = {
    "README.md": "Home.md",
    "docs/SETUP.md": "Setup.md",
    "docs/CONFIGURATION.md": "Configuration.md",
    "docs/COMMANDS.md": "Commands.md",
    "docs/MODERATION.md": "Moderation.md",
    "docs/ANIME-IMAGE-SEARCH.md": "Anime-and-image-search.md",
    "docs/ARCHITECTURE.md": "Architecture.md",
    "docs/DEVELOPMENT.md": "Development.md",
    "docs/SPEC.md": "Specification.md",
    "docs/ROADMAP.md": "Roadmap.md",
}

LINK_PATTERN = re.compile(r"(?P<prefix>!?\[[^\]]*\]\()(?P<target>[^)\s]+)(?P<suffix>\))")
SECRET_PATTERNS = (
    re.compile(r"ghp_[A-Za-z0-9_]{20,}"),
    re.compile(r"github_pat_[A-Za-z0-9_]{20,}"),
    re.compile(r"-----BEGIN (?:RSA|OPENSSH|EC|DSA) PRIVATE KEY-----"),
    re.compile(r"mongodb\+srv://(?!YOUR_|<)[^\s\"']+", re.IGNORECASE),
)


def load_allowlist(repo_root: Path) -> list[str]:
    allowlist_path = repo_root / ".github" / "public-docs.txt"
    if not allowlist_path.is_file():
        raise RuntimeError(f"Missing public documentation allowlist: {allowlist_path}")

    entries = []
    for raw_line in allowlist_path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if not line or line.startswith("#"):
            continue
        if line not in PAGE_MAP:
            raise RuntimeError(f"Allowlisted file has no wiki page mapping: {line}")
        if line == "docs/TASKS.md":
            raise RuntimeError("Internal docs/TASKS.md cannot be published")
        entries.append(line)

    if not entries:
        raise RuntimeError("The public documentation allowlist is empty")
    return entries


def page_target(source_path: str) -> str:
    return PAGE_MAP[source_path].removesuffix(".md")


def rewrite_links(markdown: str, source_path: str) -> str:
    source_parent = posixpath.dirname(source_path)

    def replace(match: re.Match[str]) -> str:
        target = match.group("target")
        if target.startswith(("http://", "https://", "mailto:", "#")):
            return match.group(0)

        target_path, separator, fragment = target.partition("#")
        normalized = posixpath.normpath(posixpath.join(source_parent, target_path))
        normalized = normalized.removeprefix("./")

        if normalized not in PAGE_MAP:
            return match.group(0)

        wiki_target = page_target(normalized)
        if separator:
            wiki_target += f"#{fragment}"
        return f"{match.group('prefix')}{wiki_target}{match.group('suffix')}"

    return LINK_PATTERN.sub(replace, markdown)


def scan_public_content(content: str, source_path: str) -> None:
    for pattern in SECRET_PATTERNS:
        if pattern.search(content):
            raise RuntimeError(f"Secret-like content detected in public document: {source_path}")
    if "Task_Sheets/" in content or "H:/Vaults/" in content:
        raise RuntimeError(f"Private vault reference detected in public document: {source_path}")


def sidebar() -> str:
    return """## Lyuze 3.0

- [Home](Home)

### Start here

- [Setup](Setup)
- [Configuration](Configuration)
- [Commands](Commands)
- [Moderation](Moderation)

### Project

- [Architecture](Architecture)
- [Development](Development)
- [Specification](Specification)
- [Roadmap](Roadmap)
- [Anime and image search](Anime-and-image-search)
"""


def read_previous_generated(wiki_root: Path) -> set[str]:
    marker = wiki_root / GENERATED_MARKER
    if not marker.is_file():
        return set()
    return {line.strip() for line in marker.read_text(encoding="utf-8").splitlines() if line.strip()}


def publish(repo_root: Path, wiki_root: Path, dry_run: bool) -> list[str]:
    sources = load_allowlist(repo_root)
    generated_pages = {PAGE_MAP[source] for source in sources} | {"_Sidebar.md", GENERATED_MARKER}

    rendered: dict[str, str] = {}
    for source in sources:
        source_file = repo_root / source
        if not source_file.is_file():
            raise RuntimeError(f"Allowlisted public document does not exist: {source}")
        content = rewrite_links(source_file.read_text(encoding="utf-8"), source)
        scan_public_content(content, source)
        rendered[PAGE_MAP[source]] = content.rstrip() + "\n"

    rendered["_Sidebar.md"] = sidebar()
    rendered[GENERATED_MARKER] = "\n".join(sorted(generated_pages)) + "\n"

    previous = read_previous_generated(wiki_root)
    stale = sorted(previous - generated_pages)
    changed = []

    for page in stale:
        path = wiki_root / page
        if path.is_file() and not dry_run:
            path.unlink()
            changed.append(f"deleted {page}")

    for page, content in rendered.items():
        path = wiki_root / page
        current = path.read_text(encoding="utf-8") if path.is_file() else None
        if current != content:
            changed.append(f"updated {page}")
            if not dry_run:
                path.write_text(content, encoding="utf-8", newline="\n")

    return changed


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--wiki-root", type=Path, required=True)
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    if not args.repo_root.is_dir():
        raise SystemExit(f"Repository root does not exist: {args.repo_root}")
    if not args.wiki_root.is_dir():
        raise SystemExit(f"Wiki root does not exist: {args.wiki_root}")

    changes = publish(args.repo_root.resolve(), args.wiki_root.resolve(), args.dry_run)
    if changes:
        print("\n".join(changes))
    else:
        print("Wiki is already synchronized")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
