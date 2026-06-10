# ASP.NET Core Best Practices Reference

Performance and reliability guidelines for this Blazor Server application.

Source:
- [Microsoft — ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)

---

## 1. Cache aggressively

Caching is the most direct lever for improving throughput. Identify data that is:
- Frequently read, rarely written.
- Acceptable to serve slightly stale (within a defined TTL).

```csharp
if (!_memoryCache.TryGetValue(cacheKey, out var result))
{
    result = await _repository.GetExpensiveDataAsync();
    _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
}
```

Use `MemoryCache` for single-machine deployments and `DistributedCache` (e.g. Redis) when multiple instances share state. Caching large objects also keeps them out of the LOH — see §4.

---

## 2. Async first — no blocking calls

ASP.NET Core's thread pool is a shared, finite resource. Blocking calls cause **thread pool starvation**, degrading response times for all concurrent requests.

```csharp
// Forbidden: synchronous wait on a Task
var result = someTask.Result;   // deadlock risk
someTask.Wait();                // thread pool starvation

// Forbidden: wrapping a synchronous method in Task.Run does not help
var result = await Task.Run(() => SynchronousMethod());
// Task.Run only moves the work to another pool thread — still blocking

// Correct: truly async I/O throughout the call stack
[HttpGet("/data")]
public async Task<ActionResult<Data>> Get()
{
    var result = await _service.GetDataAsync();
    return Ok(result);
}

// Correct: async request body deserialisation
public async Task<ActionResult<MyData>> Post()
    => await JsonSerializer.DeserializeAsync<MyData>(Request.Body);
```

Summary:

| Rule | Reason |
|---|---|
| Never `.Result` or `.Wait()` | Deadlock + thread pool starvation |
| Never `Task.Run` to wrap sync code | Just shifts the block to another thread |
| All controller/page actions must be `async Task` | Full async call stack is required |
| Long-running work → background service or queue | Keeps request threads free |

---

## 3. Paginate large collections

Loading large data sets in a single response risks OOM and slow GC:

```csharp
// Avoid: may return tens of thousands of rows
public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    => await _context.Orders.ToListAsync();

// Correct: paginated
public async Task<PagedResult<Order>> GetOrdersAsync(int pageIndex, int pageSize)
{
    var items = await _context.Orders
        .Skip(pageIndex * pageSize)
        .Take(pageSize)
        .ToListAsync();
    return new PagedResult<Order>(items, pageIndex, pageSize);
}

// Alternative: async streaming with IAsyncEnumerable (ASP.NET Core 3.0+)
public async IAsyncEnumerable<Order> StreamOrdersAsync()
{
    await foreach (var order in _context.Orders.AsAsyncEnumerable())
        yield return order;
}
```

---

## 4. Minimise large object allocations

Objects ≥ 85,000 bytes enter the **Large Object Heap (LOH)**. LOH collection requires a Full GC (Gen 2), which temporarily pauses application execution.

```csharp
// Avoid: frequent large byte[] allocation on hot paths
byte[] buffer = new byte[1024 * 1024];  // 1 MB — enters LOH

// Correct: pool buffers with ArrayPool<T>
byte[] buffer = ArrayPool<byte>.Shared.Rent(1024 * 1024);
try { /* use buffer */ }
finally { ArrayPool<byte>.Shared.Return(buffer); }
```

Avoid reading large request or response bodies into a single `byte[]` or `string`:

```csharp
// Dangerous: may trigger OOM on large payloads
var json = await new StreamReader(Request.Body).ReadToEndAsync();

// Correct: stream directly into the deserialiser
return await JsonSerializer.DeserializeAsync<MyData>(Request.Body);
```

---

## 5. Optimise data access

```csharp
// Always async
var orders = await _context.Orders.ToListAsync();

// Project only needed columns
var names = await _context.Users
    .Where(u => u.IsActive)
    .Select(u => new { u.Id, u.Name })
    .ToListAsync();

// No-tracking for read-only queries
var orders = await _context.Orders
    .AsNoTracking()
    .Where(o => o.Status == OrderStatus.Active)
    .ToListAsync();

// Push filtering to the database — never materialise first
var filtered = await _context.Products
    .Where(p => p.Price > 100)   // translated to SQL WHERE
    .ToListAsync();

// Avoid N+1: use Include instead of per-row queries
var ordersWithCustomers = await _context.Orders
    .Include(o => o.Customer)
    .ToListAsync();
```

Reduce network round trips: retrieve all required data in one query rather than several sequential ones.

---

## 6. `HttpClientFactory` — never new/Dispose `HttpClient` directly

Disposing `HttpClient` leaves the underlying socket in `TIME_WAIT` for a short period. Frequent create/dispose cycles exhaust available sockets (**socket exhaustion**).

```csharp
// Avoid
using var client = new HttpClient();  // socket exhaustion risk

// Correct: retrieve a pooled instance from the factory
public class MyService(IHttpClientFactory factory)
{
    public async Task<string> FetchAsync()
    {
        var client = factory.CreateClient("myClient");
        return await client.GetStringAsync(url);
    }
}

// Program.cs
builder.Services.AddHttpClient("myClient", c =>
    c.BaseAddress = new Uri("https://api.example.com"));
```

---

## 7. Long-running work outside HTTP requests

HTTP requests should complete quickly. Move CPU-intensive or slow I/O work to a background service:

```csharp
// Avoid: request hangs for the duration of the work
[HttpPost("/process")]
public async Task<IActionResult> Process()
{
    await _service.LongRunningOperationAsync();
    return Ok();
}

// Correct: accept immediately, process in background
[HttpPost("/process")]
public IActionResult Process([FromServices] IServiceScopeFactory scopeFactory)
{
    _ = Task.Run(async () =>
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var svc = scope.ServiceProvider.GetRequiredService<IProcessingService>();
        await svc.ProcessAsync();
    });
    return Accepted();  // 202
}
```

Preferred: implement as an `IHostedService`, Azure Function, or queue consumer.

---

## 8. Safe `HttpContext` usage

### Do not store `HttpContext` in a field

```csharp
// Dangerous: constructor-captured HttpContext may be null or stale
public class BadService(IHttpContextAccessor accessor)
{
    private readonly HttpContext _context = accessor.HttpContext;  // wrong
}

// Correct: resolve HttpContext on each use
public class GoodService(IHttpContextAccessor accessor)
{
    public void CheckAdmin()
    {
        var context = accessor.HttpContext;
        if (context != null && !context.User.IsInRole("admin"))
            throw new UnauthorizedAccessException();
    }
}
```

### Do not access `HttpContext` from multiple threads

```csharp
// Dangerous: concurrent access to HttpContext
var q1 = SearchAsync(HttpContext.Request.Path);
var q2 = SearchAsync(HttpContext.Request.Path);

// Correct: copy values before spawning parallel work
string path = HttpContext.Request.Path;
var q1 = SearchAsync(path);
var q2 = SearchAsync(path);
```

### Never use `async void` in controllers

```csharp
// Dangerous: HTTP request completes at the first await, then Response is invalid
[HttpGet("/async")]
public async void Get() { ... }

// Correct
[HttpGet("/async")]
public async Task Get() { ... }
```

### Do not capture `HttpContext` or scoped services in background threads

```csharp
// Dangerous: DbContext may already be disposed when the lambda runs
_ = Task.Run(async () => await context.SaveChangesAsync());

// Correct: create a new scope for the background work
_ = Task.Run(async () =>
{
    await using var scope = scopeFactory.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.SaveChangesAsync();
});
```

---

## 9. Async request/response I/O

```csharp
// Avoid: Kestrel does not support synchronous reads
var json = new StreamReader(Request.Body).ReadToEnd();

// Correct: async read
var json = await new StreamReader(Request.Body).ReadToEndAsync();

// Preferred: stream directly
return await JsonSerializer.DeserializeAsync<MyData>(Request.Body);

// Form body: use ReadFormAsync, not Request.Form
var form = await HttpContext.Request.ReadFormAsync();
```

---

## 10. Response header management

ASP.NET Core does not buffer the response body. Once writing starts, headers are sent and cannot be changed:

```csharp
// Dangerous: exception if next() has already written to response
app.Use(async (context, next) =>
{
    await next();
    context.Response.Headers["custom"] = "value";  // may throw
});

// Correct option 1: check HasStarted
app.Use(async (context, next) =>
{
    await next();
    if (!context.Response.HasStarted)
        context.Response.Headers["custom"] = "value";
});

// Correct option 2: OnStarting callback — runs just before headers flush
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["custom"] = "value";
        return Task.CompletedTask;
    });
    await next();
});
```

---

## 11. Additional recommendations

**Exceptions are not flow control.** Throwing and catching exceptions is expensive relative to conditional checks. Reserve them for unexpected, non-recoverable conditions.

**Always use the latest ASP.NET Core version.** Each release includes measurable performance improvements in serialisation, memory use, and hot-path execution.

**Enable response compression.** Gzip and Brotli can dramatically reduce payload size.

**Bundle and minify client assets.** Fewer HTTP requests and smaller files reduce initial load time.

**IIS: prefer in-process hosting.** In-process hosting avoids the loopback proxy overhead of out-of-process. ASP.NET Core 3.0+ defaults to in-process.

---

## Best-practices checklist

- [ ] Frequently-read data is cached; TTL is defined
- [ ] All I/O uses `async`/`await` — no `.Result` or `.Wait()`
- [ ] Large collections are paginated
- [ ] Hot paths avoid allocations ≥ 85 KB; use `ArrayPool<T>` for buffers
- [ ] Queries use `AsNoTracking`, project only needed columns, filter in the database
- [ ] `HttpClient` obtained from `HttpClientFactory`
- [ ] Long-running work is off-loaded to a background service or queue
- [ ] `HttpContext` is not stored in a field and is not accessed across threads
- [ ] Request and response bodies are read asynchronously
- [ ] Exceptions are reserved for unexpected, non-recoverable conditions
