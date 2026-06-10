# Project Instructions

This file is the single entry point for all Claude Code instructions in this repository.
Project rules and coding standards are maintained in the file imported below; do not duplicate
them here. Add Claude-specific overrides or notes after the import if needed.

@QrCodeGenerate/AGENTS.md

## Reference documents

Five read-only reference files in `QrCodeGenerate/docs/references/` provide depth and rationale
behind the coding standards. They are derived from official Microsoft documentation and community
guidelines. Consult them whenever the coding standards leave a decision ambiguous:

- `01-naming-conventions.md` — identifier casing rules, abbreviations, forbidden patterns.
- `02-csharp-language-idioms.md` — `var` usage, strings, collections, delegates, LINQ, exceptions, class and member design principles.
- `03-formatting-and-layout.md` — brace style, member ordering, blank lines, comments, expression-bodied members.
- `04-aspnetcore-best-practices.md` — async/await, caching, data access, `HttpContext` safety, request/response I/O, background tasks.
- `05-framework-design-guidelines.md` — class vs struct vs record selection, interface design, property vs method decisions, exception design, `IDisposable`.

When two sources conflict, apply this priority order:
`CODING_STANDARDS.md` > `.editorconfig` > `docs/references/` files > personal preference.

Full usage instructions are in AGENTS.md Rule 20.
