# C# Formatting and Layout Reference

Supplementary depth and rationale for the rules in `CODING_STANDARDS.md §3`.

Sources:
- [Microsoft — .NET Coding Conventions (Style Guidelines)](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [DoFactory — C# Coding Standards](https://www.dofactory.com/csharp-coding-standards)
- [CSharpCodingGuidelines.com — Layout Guidelines (AV2400)](https://csharpcodingguidelines.com/layout-guidelines/)

---

## 1. Indentation and spacing

| Rule | Detail |
|---|---|
| 4-space indentation | No tabs; spaces produce consistent rendering across editors |
| Line width ≤ 130 characters | CSharpCodingGuidelines upper bound; Microsoft docs recommends 65 for mobile readability |
| Space after keyword | `if (condition)`, not `if(condition)` |
| Spaces around operators | `x + y`, `a == b`, not `x+y` |
| No spaces inside parentheses | `if (x > 0)`, not `if ( x > 0 )` |

```csharp
// Correct
if ((startX > endX) && (startX > previousX))
{
    // Take appropriate action.
}
```

---

## 2. Braces — Allman style

Opening and closing braces each occupy their own line, vertically aligned with the current indentation level:

```csharp
// Correct: Allman style
public class DataService
{
    public void ProcessData()
    {
        if (condition)
        {
            DoSomething();
        }
    }
}

// Avoid: K&R style
public class DataService {
    public void ProcessData() {
        if (condition) {
            DoSomething();
        }
    }
}
```

Exception: simple auto-properties may stay on one line (AV2400):

```csharp
public string Value { get; set; }
public int Count { get; private set; }
```

Why Allman? Vertically aligned braces make the start and end of every block immediately visible, reducing misread nesting in complex code.

---

## 3. One statement / declaration per line

```csharp
// Correct
int x = 5;
int y = 10;

// Avoid
int x = 5; int y = 10;
string firstName, lastName;  // multiple declarations on one line
```

---

## 4. Member ordering (AV2406)

Within a class, order members as follows:

1. Private fields and constants
2. Public constants
3. Public static read-only fields
4. Factory methods (static creation methods)
5. Constructors and finalizer
6. Events
7. Public properties
8. Other methods and private properties (in call order)

Local functions belong at the bottom of their containing method, after all executable statements.

```csharp
public class QrCodeService : IQrCodeService
{
    // 1. Private fields
    private readonly ILogger<QrCodeService> _logger;

    // 2. Public constants
    public const int MaxPayloadLength = 4296;

    // 5. Constructor
    public QrCodeService(ILogger<QrCodeService> logger) => _logger = logger;

    // 6. Events
    public event EventHandler? Generated;

    // 7. Public properties
    public string Version { get; set; } = "1";

    // 8. Methods
    public byte[] GeneratePng(string payload) { ... }
}
```

---

## 5. Blank lines

| Location | Rule |
|---|---|
| Between methods | At least one blank line |
| Between multi-line properties | At least one blank line |
| After a closing brace before the next code block | At least one blank line |
| Between unrelated logical groups of statements | One blank line |
| Between different `using` namespace groups | One blank line |

---

## 6. Line breaking

### Binary operators: break before the operator

```csharp
// Correct: operator starts the continuation line
bool isEligible = age >= MinimumAge
    && salary >= MinimumSalary
    && !isBlacklisted;

// Avoid: operator ends the line
bool isEligible = age >= MinimumAge &&
    salary >= MinimumSalary &&
    !isBlacklisted;
```

### LINQ clause alignment

```csharp
// Correct: keywords vertically aligned
var result = from customer in Customers
             where customer.City == "Seattle"
             orderby customer.Name
             select customer;
```

---

## 7. Parentheses

Add parentheses to make operator precedence explicit in complex expressions:

```csharp
// Correct: precedence is clear without memorising rules
if ((startX > endX) && (startX > previousX)) { ... }
```

Remove unnecessary parentheses that add no clarity:

```csharp
return (x + y);  // remove: return x + y;
var result = (a * b);  // remove: var result = a * b;
```

---

## 8. Expression-bodied members (AV2410)

Use expression-bodied syntax **only** when the entire member body is a single, self-evident expression:

```csharp
// Correct: one clear expression
public string FullName => $"{FirstName} {LastName}";
public bool IsEmpty => Count == 0;
public int Add(int a, int b) => a + b;

// Avoid: chained ternaries and nested expressions obscure intent
public string GetStatus() =>
    IsActive ? GetActiveStatus() : IsExpired ? "Expired" : GetInactiveStatus();
// Use a full method body here instead
```

---

## 9. No `#region` (AV2407)

`#region` adds visual complexity without improving readability and complicates refactoring. Member ordering conventions (§4) make regions unnecessary.

```csharp
// Avoid
#region Private Methods
private void DoSomething() { ... }
#endregion

// Correct: rely on member ordering — no region needed
```

---

## 10. `using` directive ordering (AV2402)

```csharp
using System;                                       // 1. System namespaces (alphabetical)
using System.Collections.Generic;
using System.Threading.Tasks;
                                                    // blank line
using Microsoft.Extensions.DependencyInjection;    // 2. Other namespaces (alphabetical)
using QrCodeGenerate.Services;
                                                    // blank line
using static System.Console;                        // 3. static and alias directives last
using QrEncoder = QRCoder.QRCodeGenerator;
```

---

## 11. Comment style

| Rule | Detail |
|---|---|
| Use `//` for single-line comments | Avoid `/* */` — doc samples are not localised so explanatory text belongs in the article |
| Public members use XML doc comments | `///` for methods, properties, and classes |
| First letter capitalised | `// The following declaration...` |
| End with a period | `// Sets the maximum value.` |
| One space after `//` | `// Comment`, not `//Comment` |
| Comment on its own line | Do not append to the end of a code line |

```csharp
// The following declaration creates a query. It does not run the query.
var scoreQuery = from student in students
                 where student.Score > 90
                 select student;

/// <summary>
/// Generates a QR code PNG for the specified payload.
/// </summary>
/// <param name="payload">The text to encode.</param>
/// <returns>PNG image bytes.</returns>
public byte[] GeneratePng(string payload) { ... }
```

When to add a comment: only when the **why** is non-obvious — a hidden constraint, a specific bug workaround, or behaviour that would surprise a reader. Good naming makes the what self-documenting.

---

## Formatting checklist

- [ ] 4-space indentation, no tabs
- [ ] Allman-style braces (each on its own line)
- [ ] One statement and one declaration per line
- [ ] Member order: fields → constants → constructor → events → properties → methods
- [ ] At least one blank line between methods and properties
- [ ] Line breaks before binary operators
- [ ] Explicit parentheses in complex expressions
- [ ] Expression-bodied members for single clear expressions only
- [ ] No `#region`
- [ ] `using` order: System first, others alphabetical, static/alias last
- [ ] XML doc comments on all public members
