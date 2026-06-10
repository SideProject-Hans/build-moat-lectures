# Framework and API Design Guidelines Reference

Guidelines for designing reusable service interfaces, domain types, and API boundaries.

Sources:
- [Microsoft — Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Microsoft — Choosing Between Class and Struct](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct)
- [Microsoft — Interface Design](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/interface)
- [Microsoft — Property Design](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/property)
- [CSharpCodingGuidelines.com — Class Design (AV1000)](https://csharpcodingguidelines.com/class-design-guidelines/)
- [CSharpCodingGuidelines.com — Member Design (AV1100)](https://csharpcodingguidelines.com/member-design-guidelines/)

---

## Recommendation strength markers

| Marker | Meaning |
|---|---|
| ✅ DO | Follow in virtually all cases |
| 🔵 CONSIDER | Follow in most cases; justified deviation is allowed |
| ⚠️ AVOID | Do not use in most cases; justified use is allowed |
| ❌ DO NOT | Never violate this |

---

## 1. Class vs Struct vs Record

### Reference type vs value type fundamentals

| Characteristic | Class (reference type) | Struct (value type) |
|---|---|---|
| Allocation | Heap — GC collects | Stack or inline in container |
| GC pressure | Yes | None (freed with scope) |
| Array layout | Out-of-line (references) | Inline (better locality) |
| Assignment cost | Copies reference (cheap) | Copies entire value (costly for large structs) |
| Boxing | No | Yes — when cast to interface or `object` |

> 🔵 CONSIDER defining a `struct` if instances are small and commonly short-lived or embedded in other objects.

> ❌ AVOID defining a `struct` unless **all** of the following hold:
> - Logically represents a single value (like `int` or `double`)
> - Instance size is **under 16 bytes**
> - The type is **immutable**
> - It will not be boxed frequently

All other types should be `class`.

### Record vs class vs record struct (AV1030)

| Scenario | Recommended type |
|---|---|
| Immutable data, DTOs, value objects (e.g. `QrCodeOptions`) | `record` |
| Small, frequently-passed immutable data | `record struct` |
| Mutable, behaviour-rich, lifecycle-managed (e.g. `QrCodeService`) | `class` |

```csharp
public record QrCodeOptions(string Payload, int Size, ErrorCorrectionLevel Level);

public class QrCodeService : IQrCodeService { /* state + behaviour */ }

public record struct Point(int X, int Y);  // <16 bytes, immutable
```

### Avoid static classes (AV1008)

Static classes resist isolated testing and usually signal poor design. Extension method containers are the one acceptable exception.

```csharp
// Avoid
public static class QrHelper
{
    public static string Encode(string input) => ...;
}

// Correct: injectable, replaceable, testable
public class QrCodeService(IEncoder encoder) : IQrCodeService { }
```

---

## 2. Interface design

### When to define an interface

> ✅ DO define an interface when a common API must be supported by a set of types that includes value types.

> ✅ CONSIDER defining an interface when functionality must be added to types that already have a different base class.

> ❌ AVOID marker interfaces (interfaces with no members). Use a custom attribute instead.

```csharp
// Avoid: marker interface
public interface IValidatable { }

// Correct: attribute
[AttributeUsage(AttributeTargets.Class)]
public class ValidatableAttribute : Attribute { }
```

### Validate the design

> ✅ DO provide at least one implementation of every interface you define — this validates the design.

> ✅ DO provide at least one API that consumes each interface (as a parameter or property type).

### Never add members to a shipped interface

> ❌ DO NOT add members to an interface that has already been published. This breaks all existing implementations.

```csharp
// Shipped
public interface IQrCodeService
{
    byte[] GeneratePng(string payload);
}

// Need new behaviour? Define a new interface
public interface IQrCodeServiceV2 : IQrCodeService
{
    byte[] GenerateSvg(string payload);
}
```

### Interfaces for decoupling (AV1005)

Interfaces break bidirectional dependencies, enable mock/stub testing, and support DI. In general, prefer interfaces over base classes for extension points.

---

## 3. Property design

### Access modifier rules

> ✅ DO create get-only properties when callers must not change the value.

> ❌ DO NOT provide set-only properties, or properties where the setter has broader accessibility than the getter.

```csharp
// Avoid
public string Status { set; }                           // set-only: use a method
public string Name { protected get; public set; }      // setter wider than getter

// Correct
public string Name { get; private set; }
public string Name { get; init; }
```

### Sensible defaults and order independence

> ✅ DO provide sensible default values that do not create security holes or inefficiency.

> ✅ DO allow properties to be set in any order, even if this temporarily produces an invalid state. Validate only when the interrelated properties are actually used together.

> ✅ DO preserve the previous value if a property setter throws an exception.

### Getters must not throw

> ❌ AVOID throwing exceptions from property getters. A getter that can throw is probably better modelled as a method.

```csharp
// Avoid
public string DisplayName
{
    get
    {
        if (_name == null) throw new InvalidOperationException();
        return _name;
    }
}

// Correct
public string DisplayName => _name ?? string.Empty;
```

### Property vs method decision (AV1105)

Use a **method**, not a property, when:

| Situation | Example |
|---|---|
| Cost is much higher than a field access | DB query, network I/O |
| Operation performs a type conversion | `ToString()`, `ToArray()` |
| Successive calls return different results without input change | `Guid.NewGuid()`, `DateTime.Now` |
| Side-effects unrelated to the property | Triggering an event |

```csharp
// Property: fast, stateless, represents "what the object is"
public bool IsEmpty => Count == 0;
public string FullName => $"{FirstName} {LastName}";

// Method: has cost, has side-effect, or varies each call
public async Task<byte[]> GeneratePngAsync(string payload);
public Guid CreateToken();
```

---

## 4. Member design

### Single Responsibility (AV1115)

Each method, property, or local function does exactly one thing. "And" in a method name is a strong violation signal.

```csharp
// Avoid
public void ValidateAndSave(Order order) { ... }

// Correct
public void Validate(Order order) { ... }
public async Task SaveAsync(Order order) { ... }
```

### Return read-only collection interfaces (AV1130)

```csharp
// Avoid: exposes mutable internal state
public List<Order> GetOrders() => _orders;

// Correct
public IReadOnlyCollection<Order> GetOrders() => _orders.AsReadOnly();
public IReadOnlyList<string> GetNames() => _names;
public IReadOnlyDictionary<string, int> GetCountMap() => _countMap;
```

`ImmutableArray<T>`, `FrozenSet<T>`, and `FrozenDictionary<TKey, TValue>` may be returned directly.

### Never return null for strings, collections, or Tasks (AV1135)

```csharp
// Avoid: forces null checks on every caller
public IEnumerable<Item> GetItems() => null;

// Correct
public IEnumerable<Item> GetItems() => [];
public string GetName() => _name ?? string.Empty;
public Task DoWorkAsync() => Task.CompletedTask;
```

### Parameters: narrow and specific (AV1137)

Accept only what is actually needed, not a container object:

```csharp
// Avoid: couples caller to the full configuration object
public void Connect(AppConfiguration config)
    => _conn = config.Database.ConnectionString;

// Correct
public void Connect(string connectionString) { ... }
```

### Prefer domain types over primitives (AV1140)

```csharp
// Avoid: primitive obsession
public void ProcessPayment(string cardNumber, string cvv, decimal amount) { }

// Correct: domain types carry validation logic
public void ProcessPayment(CardNumber cardNumber, Cvv cvv, Money amount) { }
```

### Do not hide dependencies behind static members (AV1125)

```csharp
// Avoid: hard to test, hides implicit dependency
var now = DateTime.UtcNow;

// Correct: inject an abstraction
public class MyService(TimeProvider timeProvider)
{
    public DateTimeOffset GetNow() => timeProvider.GetUtcNow();
}
```

---

## 5. Extensibility design

### Inheritance vs composition vs interface

| Need | Recommendation |
|---|---|
| Polymorphism across unrelated type hierarchies | Interface |
| Shared implementation with inheritance | Abstract base class |
| Adding behaviour to an existing type without modifying it | Extension members (C# 14) or interface |

### Liskov Substitution Principle (AV1011)

Derived types must be fully substitutable for their base types. Callers must not be able to detect the substitution.

```csharp
// Violates LSP: derived class throws NotImplementedException
public class NoOpSave : ISaveService
{
    public Task SaveAsync(Item item) => throw new NotImplementedException();
}

// Correct: honour the contract
public class NoOpSave : ISaveService
{
    public Task SaveAsync(Item item) => Task.CompletedTask;
}
```

### Law of Demeter (AV1014)

Avoid chained access that exposes internal structure:

```csharp
// Violation: caller depends on three levels of internal structure
var name = order.Customer.GetAddress().City.ToUpper();

// Correct: encapsulate behind a meaningful method
var city = order.GetCustomerCity();
```

### Avoid bidirectional dependencies (AV1020)

Two classes that each reference the other are hard to refactor and test. Break the cycle with interfaces and DI.

---

## 6. Exception design

### When to throw

Exceptions represent unexpected, non-recoverable conditions — not expected business scenarios.

```csharp
// Correct: guard against invalid input at the API boundary
public byte[] GeneratePng(string payload)
{
    ArgumentException.ThrowIfNullOrEmpty(payload);
    if (payload.Length > MaxPayloadLength)
        throw new ArgumentOutOfRangeException(nameof(payload), "Payload too long.");
    // ...
}

// Avoid: exception used as a branch condition
try { return int.Parse(input); }
catch (FormatException) { return 0; }  // use int.TryParse instead
```

### Custom exception types

Inherit from the most semantically appropriate base:

```csharp
public class QrCodeGenerationException : InvalidOperationException
{
    public QrCodeGenerationException(string message, Exception? inner = null)
        : base(message, inner) { }
}
```

Document thrown exceptions on public API:

```csharp
/// <exception cref="ArgumentNullException">Thrown when <paramref name="payload"/> is null.</exception>
/// <exception cref="ArgumentOutOfRangeException">Thrown when payload exceeds the maximum length.</exception>
public byte[] GeneratePng(string payload) { ... }
```

---

## 7. Dispose pattern

For types that hold unmanaged resources, implement `IDisposable` with the standard pattern:

```csharp
public class ResourceHolder : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                _managedResource?.Dispose();

            _disposed = true;
        }
    }
}
```

Callers always use `using`:

```csharp
using var holder = new ResourceHolder();
// automatically disposed when the scope exits
```

---

## Quick decision trees

**Choosing a type:**

```
Need polymorphism across unrelated hierarchies? → Interface
Need shared implementation + inheritance?        → Abstract class
Immutable data / DTO / value object?             → record
Size < 16 B, immutable, rarely boxed?            → struct
Otherwise                                        → class
```

**Property or method?**

```
Fast, side-effect-free, represents "what the object is"? → Property (getter)
Has I/O cost, side-effects, or varies per call?           → Method
```
