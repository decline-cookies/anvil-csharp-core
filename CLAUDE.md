# Anvil C# Core — Claude Code Rules

These rules apply when working with code that uses the `anvil-csharp-core` framework.

## Non-MonoBehaviour Classes (inherit from `AbstractAnvilBase`)

All non-MonoBehaviour classes should inherit from `AbstractAnvilBase`. This provides:
- `IAnvilDisposable` implementation (IsDisposed, IsDisposing, Dispose())
- Lazy-initialized `Logger` property

```csharp
public class MyClass : AbstractAnvilBase
{
    // 1. Constants / static readonly
    private const string SOME_CONSTANT = "value";

    // 2. Static variables
    private static int s_InstanceCount;

    // 3. Static functions (public, then private)

    // 4-6. Instance variables (public, protected, private - readonly first)
    public readonly string ID;
    private readonly int m_Value;
    private string m_Name;

    // 7. Properties (use sparingly)

    // 8. Constructor (setup)
    public MyClass()
    {
    }

    // 9. DisposeSelf (teardown) - immediately after constructor
    protected override void DisposeSelf()
    {
        // Cleanup resources here
        base.DisposeSelf();
    }

    // 10. Other methods - grouped by feature, public first
}
```

### Method Ordering
Setup methods (Constructor/Init) and `DisposeSelf` should be placed together (first methods after fields) so setup and teardown logic are easy to see together.

## Data Classes (inherit from `AbstractAnvilVO`)

Data classes represent data that is serialized/deserialized (read from or written to disk). They hold data only and should not have methods unless absolutely necessary. Use the `VO` suffix in the filename and class name.

Note: Classes that simply hold return values from functions or transient data should NOT use the `VO` suffix or inherit from `AbstractAnvilVO`.

**Deserialization:** VOs are deserialized from disk using the TinyJSON library embedded in Anvil. TinyJSON requires a parameterless constructor to instantiate the object before populating fields. Mark it with `[UsedImplicitly]` and add a remarks comment explaining it's for deserialization. Use `JSON.Decode<T>()` from `Anvil.CSharp.Data` for deserialization — do not import `TinyJSON` directly.

**Inline Initialization:** For readonly reference types (collections like `List<T>`, `Dictionary<K,V>`, `HashSet<T>`, etc.), initialize them inline with their declaration rather than in a constructor. This keeps the code concise and ensures they are always initialized.

```csharp
public class MyDataVO : AbstractAnvilVO
{
    /// <summary>
    /// Description of the field.
    /// </summary>
    public readonly string SomeValue;

    /// <summary>
    /// A collection of items.
    /// </summary>
    public readonly List<string> Items = new List<string>();

    /// <remarks>
    /// Called during deserialization.
    /// </remarks>
    [UsedImplicitly]
    public MyDataVO()
    {
    }
}
```

## Logging

Use the `Logger` property (lazy-initialized on `AbstractAnvilBase`):
- `Logger.Debug()` — default logging, equivalent to Debug.Log
- `Logger.Warning()` — equivalent to Debug.LogWarning
- `Logger.Error()` — equivalent to Debug.LogError

Do not use `Debug.Log`, `Debug.LogWarning`, or `Debug.LogError` directly — always use the `Logger` property.

## Disposal Pattern

All classes that hold resources should override `DisposeSelf()`:
- Place `DisposeSelf` immediately after the constructor
- Call `base.DisposeSelf()` at the end
- Clean up subscriptions, references, and owned disposable objects

```csharp
protected override void DisposeSelf()
{
    // Unsubscribe from events
    // Dispose owned objects
    // Null out references
    base.DisposeSelf();
}
```
