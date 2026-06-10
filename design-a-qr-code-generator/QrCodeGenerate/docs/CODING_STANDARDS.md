# Coding & Naming Standards

Standards for all C# and Razor code in this solution. The repo-root `.editorconfig`
is the machine-readable companion: it surfaces violations in the IDE and via
`dotnet format`, but style is intentionally **not** enforced at build time.

Each section cites the official source it is derived from. Conflicts between this
document and observed code are resolved per AGENTS.md Rule 7 (surface, don't blend)
and Rule 11 (conformance over taste).

## 1. Scope & precedence

- Applies to every `.cs` and `.razor` file in `QrCodeGenerate/` and `tests/`.
- Precedence: this document → `.editorconfig` → personal preference.
- Test-process rules (TDD layering, Red-Green-Refactor) live in `AGENTS.md`, not here.

## 2. C# naming

Source: [C# identifier naming rules and conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/identifier-names)
and [Framework Design Guidelines — General Naming Conventions](https://learn.microsoft.com/dotnet/standard/design-guidelines/general-naming-conventions).

| Element | Convention | Example |
|---|---|---|
| Namespace, class, struct, record, enum, delegate | PascalCase | `QrCodeGenerate.Services` |
| Interface | `I` + PascalCase | `IQrCodeService` |
| Method, property, event, public field | PascalCase | `GeneratePng()` |
| Async method (Task-returning) | PascalCase + `Async` suffix | `GeneratePngAsync()` |
| Parameter, local variable | camelCase | `payloadText` |
| Private field (instance **and** static) | `_` + camelCase | `_factory` |
| Constant (field or local) | PascalCase | `MaxPayloadLength` |
| Generic type parameter | `T` or `T` + descriptive name | `TResult` |
| Record positional parameters | PascalCase (they become public properties) | `record Size(int Width, int Height)` |

Additional rules:

- Favor readability over brevity; no Hungarian notation, no abbreviations except
  widely accepted ones (`Id`, `Ok`, `Qr` as an accepted domain acronym).
- Enum types: singular noun for non-flags, plural for `[Flags]`.
- Attribute types end with `Attribute`.
- No two consecutive underscores in any identifier (reserved for the compiler).
- Project decision: private static fields use `_camelCase` like instance fields;
  the .NET Runtime `s_`/`t_` prefixes are **not** used (matches existing code).

## 3. C# coding style

Source: [Common C# code conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions).

- File-scoped namespaces (`namespace X;`), one public type per file, file named after the type.
- `using` directives outside the namespace, `System.*` first.
- Always declare accessibility modifiers explicitly.
- `var` only when the type is apparent from the right-hand side (`new`, cast, literal);
  otherwise use the explicit type. Use language keywords (`string`, `int`) over CLR
  type names (`String`, `Int32`).
- Nullable reference types stay enabled; a null-forgiving `!` requires a comment
  stating why it is safe.
- Always use braces, including single-line `if` bodies.
- Prefer object/collection initializers and pattern matching where they simplify code.
- Expression-bodied members only when the whole member is a single clear expression.

## 4. Blazor / Razor conventions

Source: [ASP.NET Core Razor components](https://learn.microsoft.com/aspnet/core/blazor/components/),
[Blazor project structure](https://learn.microsoft.com/aspnet/core/blazor/project-structure),
[Blazor fundamentals](https://learn.microsoft.com/aspnet/core/blazor/fundamentals/).

- Component file names are PascalCase and match the class name: `QrGenerator.razor`.
- Folder layout: routable pages in `Components/Pages`, layouts in `Components/Layout`,
  shared non-page components directly in `Components/` (or a purpose-named subfolder
  registered in `_Imports.razor`).
- Routes are kebab-case versions of the component name: `QrGenerator` → `@page "/qr-generator"`.
- Directive order at the top of a component, with no blank lines between directives
  and one blank line before the first markup line:
  `@page` → `@rendermode` → `@using` → `@attribute` / `@implements` → `@inject`.
- Every routable page sets `<PageTitle>`.
- Component parameters are public auto-properties annotated `[Parameter]`; do not
  mutate a parameter value inside the component.
- Keep logic in `@code` while it is small; move to a code-behind partial class
  (`Component.razor.cs`) when the block exceeds roughly 30 lines.
- Component-specific styles go in a collocated `Component.razor.css`; values must
  use the design tokens defined in `DESIGN.md`.
- Common namespaces are imported once in `_Imports.razor`, not per component.

## 5. Project & service naming

- Services: `XxxService` implementing `IXxxService`, registered in `Program.cs`
  (e.g. `builder.Services.AddSingleton<IQrCodeService, QrCodeService>();`).
- DTOs and domain records live in `Models/`; service implementations in `Services/`.
- Namespaces mirror the folder structure under the root namespace `QrCodeGenerate`.
- Test projects follow `QrCodeGenerate.Tests.{Unit|Component|Integration|Acceptance}`.

## 6. Test naming

Codifies the existing patterns; layer responsibilities are defined by AGENTS.md Rules 14/18.

- Test class: `<Subject>Tests` (e.g. `HomePageTests`, `QrCodeServiceTests`).
- Test method: `<MethodOrPage>_<Scenario>_<ExpectedOutcome>` in PascalCase with
  underscores (e.g. `Home_Page_Should_Return_Ok`,
  `GeneratePng_EmptyPayload_ThrowsArgumentException`).
- bUnit component test classes mirror the component name: `QrGeneratorTests`.
- Reqnroll step methods: `Given/When/Then` prefix followed by the readable step text
  in PascalCase; step classes end with `Steps` and carry `[Binding]`.
- Test methods are exempt from the `Async` suffix rule of §2 — the
  `<Subject>_<Scenario>_<ExpectedOutcome>` pattern takes precedence in test projects
  (the `.editorconfig` disables the rule under `tests/`).
- A test name must make the intent (the WHY of AGENTS.md Rule 9) readable without
  opening the body.

## 7. Known deviations

Registry required by AGENTS.md Rule 7. Currently empty — the former
`qr_code_generate` root-namespace deviation was resolved by the full rename to
`QrCodeGenerate` (June 2026).

| Deviation | Location | Reason kept | Cleanup plan |
|---|---|---|---|
| — | — | — | — |
