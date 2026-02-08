# DRY (Don't Repeat Yourself)

Every piece of knowledge must have a single, unambiguous, authoritative representation within a system.

## Key Areas in DCMP

### 1. Packet Serialization
**Violation:** Manually writing `writer.Write(x); writer.Write(y);` in every packet class.
**Fix:** Use a shared `PacketSerializer` or extension methods for common game types (`Vector2`, `EntityId`, etc.).

```csharp
// Extension method for centralized serialization
public static void Write(this NetDataWriter writer, Vector2 vec) {
    writer.Put(vec.X);
    writer.Put(vec.Y);
}
```

### 2. Entity Lookup
**Violation:** Repeating `Game.Instance.level.entities.FirstOrDefault(e => e.id == id)` in every packet handler.
**Fix:** Create an `IEntityProvider` service that handles safe entity retrieval and caching.

### 3. Log Formatting
**Violation:** `Logger.Log($"[Client] {msg}")` vs `Logger.Log($"[Server] {msg}")` everywhere.
**Fix:** Encapsulate logging with context in a `ScopedLogger` class.
