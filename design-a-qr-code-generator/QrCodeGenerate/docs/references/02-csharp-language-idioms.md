# C# Language Idioms Reference

Supplementary depth and rationale for the rules in `CODING_STANDARDS.md §3`.

Sources:
- [Microsoft — .NET Coding Conventions (Language Guidelines)](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [CSharpCodingGuidelines.com — Class Design (AV1000)](https://csharpcodingguidelines.com/class-design-guidelines/)
- [CSharpCodingGuidelines.com — Member Design (AV1100)](https://csharpcodingguidelines.com/member-design-guidelines/)

---

## 1. `var` — when to use and when not to

### Use `var` when the type is unambiguous from the right-hand side

```csharp
var firstExample = new ExampleClass();   // new operator makes it obvious
var message = "This is clearly a string.";
var currentTemperature = 27;
```

### Do not use `var` when the type must be inferred from a method name

```csharp
// Avoid: reader cannot tell the return type without an IDE
var result = ExampleClass.ResultSoFar();

// Correct: explicit type
int currentMaximum = ExampleClass.ResultSoFar();
int numberOfIterations = Convert.ToInt32(Console.ReadLine());
```

### LINQ is the exception: `var` is expected

```csharp
// Correct: anonymous types and nested generics make var the better choice
var seattleCustomers = from customer in Customers
                       where customer.City == "Seattle"
                       select customer.Name;
```

### `for` counters — `var` is fine

```csharp
for (var i = 0; i < 10000; i++) { ... }
```

### `foreach` — prefer explicit type

```csharp
// Correct: element type is not obvious from collection name alone
foreach (char ch in laugh) { ... }
```

Rule of thumb: `var` reduces visual noise but must not reduce readability. Readers should not need an IDE to understand the variable's type.

---

## 2. String handling

### 2.1 String interpolation for short concatenations

```csharp
// Correct
string displayName = $"{nameList[n].LastName}, {nameList[n].FirstName}";
```

### 2.2 `StringBuilder` for loop-level concatenation

```csharp
var manyPhrases = new StringBuilder();
for (var i = 0; i < 10000; i++)
    manyPhrases.Append(phrase);
```

### 2.3 Raw string literals (C# 11+) over verbatim or escaped strings

```csharp
// Correct
var message = """
    This message spans multiple lines.
    It can contain \n and \t without escaping.
    """;
```

---

## 3. Collections and arrays

### Collection expressions (C# 12+)

```csharp
string[] vowels = [ "a", "e", "i", "o", "u" ];
List<int> numbers = [ 1, 2, 3, 4, 5 ];
```

### `IEnumerable<T>` vs `IAsyncEnumerable<T>`

Returning `IEnumerable<T>` from an action causes synchronous serialization, which can starve the thread pool. Prefer async enumeration in ASP.NET Core 3.0+:

```csharp
// Correct: async streaming
public async IAsyncEnumerable<Item> StreamItemsAsync() { ... }

// If IEnumerable<T> is required, materialise first
public async Task<IEnumerable<Item>> GetItemsAsync()
    => await _context.Items.ToListAsync();
```

---

## 4. Delegates and events

### 4.1 Prefer `Func<>` / `Action<>` over custom delegate types

```csharp
// Correct
Action<string> log = x => Console.WriteLine(x);
Func<string, int> parse = x => Convert.ToInt32(x);

// Avoid (unless a named type adds meaningful documentation)
public delegate void MyCallback(string message);  // Action<string> suffices
```

### 4.2 Lambda for event handlers that are never removed

```csharp
// Correct: concise, no unnecessary named method
this.Click += (s, e) =>
{
    MessageBox.Show(((MouseEventArgs)e).Location.ToString());
};
```

### 4.3 Single-method interfaces vs delegates (AV1032)

Prefer a `Func<T>` or delegate when:
- The abstraction has no state requirements.
- No additional members will be added.
- Strong naming is not needed.

Prefer an interface when:
- The implementation needs state.
- The contract requires strong naming or documentation.
- Default interface methods are needed.

---

## 5. Exception handling

### 5.1 `try-catch` — catch only what you can handle

```csharp
static double ComputeDistance(double x1, double y1, double x2, double y2)
{
    try
    {
        return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }
    catch (ArithmeticException ex)
    {
        Console.WriteLine($"Arithmetic overflow: {ex}");
        throw;  // re-throw preserves the original stack trace
    }
}
```

### 5.2 `using` replaces `try-finally` for `IDisposable`

```csharp
// Modern syntax — no braces needed
using Font normalStyle = new Font("Arial", 10.0f);
byte charset = normalStyle.GdiCharSet;
// normalStyle is disposed when it goes out of scope
```

### 5.3 Exceptions must not control normal program flow

```csharp
// Avoid: exception used as a branch condition
try { int result = int.Parse(input); }
catch (FormatException) { ... }

// Correct: use TryParse
if (int.TryParse(input, out var result)) { ... }
```

---

## 6. Short-circuit operators

Always use `&&` and `||` instead of `&` and `|` for conditional evaluation:

```csharp
// Correct: right side is not evaluated when left side is false
if ((divisor != 0) && (dividend / divisor > 10)) { ... }

// Dangerous: & evaluates both sides — throws when divisor is 0
if ((divisor != 0) & (dividend / divisor > 10)) { ... }
```

---

## 7. Object initialisation

### Object initialisers

```csharp
// Correct
var example = new ExampleClass { Name = "Desktop", ID = 37414, Age = 2.3 };
```

### `required` properties (C# 11+)

```csharp
public class LabelledContainer<T>(string label)
{
    public string Label { get; } = label;
    public required T Contents { get; init; }
}

// Compiler error if Contents is not set at construction
var c = new LabelledContainer<string>("box") { Contents = "data" };
```

### Record primary constructor parameters — PascalCase

```csharp
public record Person(string FirstName, string LastName);  // PascalCase: becomes property

public class DataService(IWorkerQueue workerQueue) { }    // camelCase: method parameter
```

---

## 8. LINQ queries

### Use semantic variable names

```csharp
var seattleCustomers = from customer in Customers
                       where customer.City == "Seattle"
                       select customer.Name;
```

### Anonymous type properties in PascalCase

```csharp
var pairs = from customer in Customers
            join dist in Distributors on customer.City equals dist.City
            select new { CustomerName = customer.Name, DistributorName = dist.Name };
```

### `where` before other clauses

```csharp
var result = from customer in Customers
             where customer.City == "Seattle"   // filter first
             orderby customer.Name              // then sort
             select customer;
```

### Multiple `from` for nested collections (not `join`)

```csharp
var highScores = from student in students
                 from score in student.Scores
                 where score > 90
                 select new { Last = student.LastName, score };
```

---

## 9. Namespace declarations

### File-scoped namespace (C# 10+)

```csharp
namespace QrCodeGenerate.Services;  // one level of indentation saved

public class QrCodeService { }
```

### `using` directives outside the namespace block

Placing `using` inside the namespace makes resolution context-sensitive and can silently break when a new dependency introduces a matching nested namespace:

```csharp
// Correct: fully qualified lookup
using Azure;

namespace CoolStuff.AwesomeFeature
{
    public class Awesome
    {
        WaitUntil wait = WaitUntil.Completed;
    }
}

// Dangerous: if CoolStuff.Azure is later introduced, resolution changes
namespace CoolStuff.AwesomeFeature
{
    using Azure;  // ambiguous
}
```

---

## 10. Static member access

Always call static members through the declaring class name, not through a derived class:

```csharp
// Correct
ClassName.StaticMember();

// Avoid: misleading if a same-named static is later added to DerivedClass
DerivedClass.BaseStaticMember();
```

---

## 11. Class design principles (AV1000 highlights)

**Single Responsibility (AV1000):** A class or interface has one purpose. "And" in a class name is a strong violation signal. If naming is difficult, the class is doing too much.

**Constructors must return usable objects (AV1001):** After construction, the object must be in a valid, usable state with no additional setup calls required. More than three constructor parameters usually signals excessive responsibility.

**Avoid bidirectional dependencies (AV1020):** Two classes each holding a reference to the other limits refactoring. Break cycles with interfaces and dependency injection.

**Record vs class (AV1030):**

| Scenario | Recommended type |
|---|---|
| Immutable data, DTOs, value objects | `record` |
| Small, frequently-passed immutable data | `record struct` |
| Mutable, behaviour-rich, lifecycle-managed types | `class` |

---

## 12. Member design principles (AV1100 highlights)

**Method over property (AV1105):** Use a method, not a property, when the operation is expensive, performs a conversion, returns different results on successive calls, or has side-effects.

**Return read-only collection interfaces (AV1130):**

```csharp
// Correct: prevent callers from mutating internal state
public IReadOnlyCollection<Order> GetOrders() => _orders.AsReadOnly();
```

**Never return null for strings, collections, or Tasks (AV1135):**

```csharp
public string GetName() => _name ?? string.Empty;
public IEnumerable<Item> GetItems() => _items ?? [];
public Task DoWorkAsync() => Task.CompletedTask;
```

**Do not hide dependencies behind static members (AV1125):** Global or environment state (`DateTime.UtcNow`, `HttpContext.Current`) makes unit testing hard and parallel execution unreliable. Inject through constructors or method parameters instead.
