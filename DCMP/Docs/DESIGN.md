# DCMP Architecture Design

This document outlines the software architecture for the Dead Cells Multiplayer (DCMP) mod, adhering to SOLID and DRY principles.

## Core Architecture

### Dependency Injection (DI)
We use a lightweight **Service Locator** pattern to manage dependencies.
- **`ServiceLocator`**: Singleton container for services.
- **`IService`**: Marker interface for all services.

```csharp
public static class ServiceLocator {
    public static void Register<T>(T service) where T : IService { ... }
    public static T Get<T>() where T : IService { ... }
}
```

## Networking Layer (Low-Level)

### Transport Abstraction
- **`INetworkTransport`**: Wraps `LiteNetLib`. Handles `PollEvents()`, `Send()`, `Connect()`.
- **`LiteNetLibTransport`**: Concrete implementation.

### Packet System
- **`IPacket`**: Interface for all network packets. Must implement `Serialize(NetDataWriter)` and `Deserialize(NetDataReader)`.
- **`PacketType` enum**: Unique ID for each packet.
- **`IPacketHandler<T>`**: Handles logic for a specific packet type (SRP).

```csharp
public interface IPacketHandler<T> where T : IPacket {
    void Handle(T packet, NetPeer peer);
}
```

## Game Integration Layer (High-Level)

### Game Interface (DIP)
To avoid tight coupling with static game classes (`Game.Instance`, `Hero`), we wrap them.
- **`IGameStateProvider`**: Provides access to `Hero`, `Level`, `Entities`.
- **`GameWrapper`**: Concrete implementation calling actual game code.

### Entity Synchronization
- **`IEntityManager`**: Manages mapping between `NetworkId` (our ID) and `Entity` (game object).
- **`NetworkedEntity`**: Component attached to game entities to track state.

## Implementation Flow

1. **`ModEntry`** (Composition Root):
   - Creates `ServiceLocator`.
   - Registers `LiteNetLibTransport`, `PacketManager`, `EntityManager`.
   - Hooks into `Game.Update` to call `ServiceLocator.Get<INetworkTransport>().Poll()`.

2. **Packet Flow**:
   - `LiteNetLibTransport` receives generic data ->
   - `PacketManager` reads `PacketType` ->
   - Deserializes to `IPacket` ->
   - Dispatches to `IPacketHandler<T>`.

## Folder Structure
```
DCMP/
  ├── Core/
  │   ├── ServiceLocator.cs
  │   └── Interfaces/
  ├── Network/
  │   ├── Packets/ (Definitions)
  │   ├── Handlers/ (Logic)
  │   └── Transport/ (LiteNetLib wrapper)
  ├── Game/
  │   ├── Entities/
  │   └── Wrappers/
  └── Utils/
```
