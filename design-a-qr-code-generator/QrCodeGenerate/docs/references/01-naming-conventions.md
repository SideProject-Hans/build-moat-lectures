# C# Naming Conventions Reference

Supplementary depth and rationale for the rules in `CODING_STANDARDS.md §2`.

Sources:
- [Microsoft — C# Identifier Naming Rules and Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)
- [Microsoft — .NET Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [DoFactory — C# Coding Standards](https://www.dofactory.com/csharp-coding-standards)
- [CSharpCodingGuidelines.com — Naming Guidelines (AV1700)](https://csharpcodingguidelines.com/naming-guidelines/)
- [Microsoft — Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)

---

## 1. Language rules (compiler-enforced)

Before conventions: identifiers must start with a letter or `_`, and may contain Unicode letters, decimal digits, connecting characters, combining characters, or formatting characters. To use a C# keyword as an identifier, prefix it with `@` (e.g. `@if`).

---

## 2. Casing quick-reference

| Element | Convention | Example |
|---|---|---|
| Namespace, class, struct, record, enum, delegate | PascalCase | `QrCodeGenerate.Services` |
| Interface | `I` + PascalCase | `IQrCodeService` |
| Method, property, event, public field | PascalCase | `GeneratePng()` |
| Local function | PascalCase | `CountQueueItems()` |
| Async method (Task-returning) | PascalCase + `Async` | `GeneratePngAsync()` |
| Method / constructor parameter | camelCase | `payloadText` |
| Local variable | camelCase | `itemCount` |
| Private / internal instance field | `_` + camelCase | `_workerQueue` |
| Private / internal static field | `s_` + camelCase (.NET runtime convention) | `s_workerQueue` |
| Thread-static field | `t_` + camelCase | `t_timeSpan` |
| Constant (field or local) | PascalCase | `MaxPayloadLength` |
| Generic type parameter | `T` or `T` + descriptive name | `T`, `TResult`, `TSession` |
| Record positional parameter | PascalCase (becomes a public property) | `record Size(int Width, int Height)` |
| Tuple element name | PascalCase | `(int Min, int Max) range` |
| Tuple variable | camelCase | `var range = (min, max)` |

Why PascalCase vs camelCase? The visual distinction immediately separates public API surface (PascalCase) from local/internal state (camelCase), reducing cognitive load when reading unfamiliar code.

---

## 3. Type naming

### 3.1 Classes — nouns or noun phrases

```csharp
// Correct
public class DataService { }
public class BusinessLocation { }

// Avoid
public class DataHelper { }   // "Helper" conveys no semantics
public class CommonUtils { }  // "Utils"/"Common" are anti-patterns
```

### 3.2 Interfaces — role-based names with `I` prefix

```csharp
// Correct: capability-oriented names
public interface IQrCodeService { }
public interface IDisposable { }
public interface IGroupable { }  // adjective form

// Avoid: naming the interface after a class
public interface IMyClass { }    // no semantic value
```

### 3.3 Enums

- Non-`[Flags]` enums: **singular** noun.
- `[Flags]` enums: **plural** noun.
- Do **not** append `Enum` suffix.
- Do **not** repeat the type name in member names.

```csharp
// Correct
public enum Direction { North, East, South, West }

[Flags]
public enum Dockings { None = 0, Top = 1, Right = 2, Bottom = 4, Left = 8 }

// Avoid
public enum DirectionEnum { ... }             // redundant suffix
public enum Color { ColorRed, ColorGreen }   // repeated type name in members
```

### 3.4 Attribute types

Must end with `Attribute`.

```csharp
public class SerializableAttribute : Attribute { }
public class ValidateInputAttribute : Attribute { }
```

### 3.5 Generic type parameters

- Prefix descriptive names with `T`.
- Use plain `T` only when a single parameter is completely self-explanatory.
- Consider reflecting constraints in the name (e.g. `TSession` for `where TSession : ISession`).

```csharp
public interface ISessionChannel<TSession> { }
public delegate TOutput Converter<TInput, TOutput>(TInput from);
public class List<T> { }  // single, self-evident parameter
```

---

## 4. Abbreviations and acronyms

| Length | Rule | Examples |
|---|---|---|
| 2 characters | All caps | `IO`, `UI`, `ID` |
| 3+ characters | First letter only | `Html`, `Xml`, `Http`, `Json` |

Widely accepted abbreviations: `Id`, `Ok`, `IO`, `UI`, `Uri`, `Xml`, `Html`, `Ftp`, `Http`. Domain-specific short forms (e.g. `Qr` in this project) are acceptable with team consensus.

```csharp
// Avoid
UsrGrp       // should be UserGroup
ChangePwd    // should be ChangePassword
GetInt       // should be GetLength (semantic, not type-based)

// Correct
UserGroup
ChangePassword
GetLength
```

Hungarian notation (`strName`, `iCounter`) is forbidden. IDEs provide hover-type tooltips; identifier names should convey semantics, not types.

---

## 5. Namespaces

Use a hierarchical structure that reflects organisation and modules:

```
Company.Product.Module.SubModule
```

Examples from this project:

```csharp
QrCodeGenerate.Services
QrCodeGenerate.Models
QrCodeGenerate.Components.Pages
```

Use nouns, layers, and features. Plural nouns (`Collections`, `Services`) are acceptable. Avoid verbs and type names in namespace segments.

---

## 6. Source file naming

| Rule | Detail |
|---|---|
| Named after the primary public type | `DataService.cs` contains `public class DataService` |
| Partial classes use a functional suffix | `DataService.cs` + `DataService.Generated.cs` |
| PascalCase | `QrGenerator.razor`, not `qr_generator.razor` |

---

## 7. Forbidden patterns

| Anti-pattern | Reason | Replacement |
|---|---|---|
| Hungarian notation (`strName`, `iCount`) | IDEs provide type info; names should carry semantics | `name`, `count` |
| All-caps constants (`MAX_LENGTH`) | Not aligned with .NET core style; visually heavy | `MaxLength` |
| Two consecutive underscores (`__`) | Reserved for compiler-generated identifiers | Rename |
| Numbers in identifiers (`item1`, `button2`) | Usually indicates lazy naming without semantics | Use intent-expressing names |
| Non-descriptive generic params (`T2`, `TX`) | Conveys no meaning about the type's role | `TInput`, `TOutput` |

---

## 8. Events

Use verbs or verb phrases that reflect the action:

```csharp
public event EventHandler Click;
public event EventHandler Deleted;
public event EventHandler<StatusEventArgs> StatusChanged;
public event EventHandler Closing;  // before the action
public event EventHandler Closed;   // after the action
```

---

## 9. Discarded lambda parameters

Mark unused lambda parameters with `_` (C# 9+) to signal they are irrelevant:

```csharp
button.Click += (_, e) => HandleClick(e);
someAction = (_, __) => DoWork();  // pre-C#9: use __ for a second discard
```

---

## 10. Extension method classes

Static classes that hold extension methods must end with `Extensions`:

```csharp
public static class StringExtensions { ... }
public static class EnumerableExtensions { ... }
```

---

## Common mistakes at a glance

| Wrong | Correct |
|---|---|
| `iCounter` | `counter` |
| `strUserName` | `userName` |
| `SHIPPINGTYPE` (constant) | `ShippingType` |
| `ColorEnum` | `Color` |
| `GetEmployee()` inside class `Employee` | `Get()` |
| `client_Appointment` | `clientAppointment` |
| `l0` (letter l + digit 0) | `logIndex` |
| `IMyClass` | Semantic role name e.g. `IWorkerQueue` |
