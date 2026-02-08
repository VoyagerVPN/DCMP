# Dead Cells Multiplayer (DCMP)

A global multiplayer modification for *Dead Cells* utilizing a custom dedicated server implementation within the client. Built on .NET 10 and the LiteNetLib transport layer, integrated via ModCore.

## Technical Stack

*   **Runtime**: .NET 10.0 (CoreCLR)
*   **Interop**: Hashlink VM (via HaxeProxy)
*   **Networking**: LiteNetLib (UDP, Reliable/Unreliable channels)
*   **Modding API**: ModCore (DeadCellsCoreModding)
*   **Serialization**: Custom binary packet serialization

## Architecture

The project adheres to SOLID and DRY principles.

*   **Core**: `ModEntry` manages the lifecycle (Initialize, Update, Exit) and service registration via `ServiceLocator`.
*   **Networking**:
    *   `INetworkService` abstraction for transport agnosticism.
    *   `PacketManager` handles opcode registration and serialization.
    *   Handler-based packet processing (`ConnectionHandler`, `LobbyHandler`).
*   **UI**:
    *   Direct injection into `TitleScreen` via native hooks.
    *   MVVM pattern: `LobbyUI` (View) <-> `LobbyViewModel` (ViewModel).
    *   Reactive updates based on `LobbyState`.

## Build Instructions

1.  Clone the repository.
2.  Ensure .NET 10 SDK is installed.
3.  Build the solution:

```powershell
dotnet build DCMP/DCMP.csproj -c Debug
```

The build process automatically copies the artifact (`DCMP.dll`) and resources (`res.pak`) to the configured `Dead Cells/coremod/mods/DCMP` directory.

## Project Structure

*   `DCMP/Core`: Core lifecycle and configuration management.
*   `DCMP/Network`: Networking logic, transport implementation, packets, and handlers.
*   `DCMP/UI`: UI logic, hooking into game menus, view models.
*   `DCMP/Utils`: Helper classes (Logging, Steam API interaction).
*   `DCMP/Resources`: Localization files (`json`) and assets.

## Roadmap

### Phase 1: Foundation & Networking (Completed)
- [x] **Transport Layer**: Integrated LiteNetLib UDP transport.
- [x] **Protocol**: OpCode-based packet system with binary serialization.
- [x] **Lobby System**: Host/Join logic with MVVM-based UI.
- [x] **Platform Integration**: Steam Persona Name fetching and localization (EN/RU).

### Phase 2: Entity Synchronization (Current)
- [ ] **Player Concept**: Implementation of `RemotePlayer` entity wrapper.
- [ ] **State Sync**: Synchronization of position, velocity, and `scaleX` (flipping).
- [ ] **Interpolation**: Linear interpolation for movement smoothing.
- [ ] **Animations**: Syncing of animation state machine indices.

### Phase 3: World & Persistence
- [ ] **RNG Logic**: Seed synchronization for world generation consistency.
- [ ] **Transitions**: Loading state synchronization and biome change events.
- [ ] **Identities**: Global `NetworkIdentity` system for shared entity tracking.

### Phase 4: Combat & Mechanics
- [ ] **Hit Registration**: Latency-compensated combat events.
- [ ] **Health/Stats**: Synchronization of player HP, gold, and scrolls.
- [ ] **Equipment**: Weapon and skill setup synchronization.

### Phase 5: AI & Meta
- [ ] **Host Authority**: Server-side AI logic with client-side visual representation.
- [ ] **Overlay**: In-game chat and notification system.
- [ ] **Steam P2P**: NAT punchthrough support via Steamworks.

## License

MIT License.
