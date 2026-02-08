# DCMP Roadmap

## Phase 0: Initialization & Analysis (Completed 100%)
- [x] Analyze project structure and tools (`core-35.9.1`, `ilspy`, etc.)
- [x] Setup Development Environment
    - [x] Build MDK from source
    - [x] Create `DCMP` project (.NET 10)
    - [x] Configure dependencies (`DeadCellsCoreModding.MDK`)
    - [x] Verify successful build
    - [x] Configure auto-deployment to Game and Test folders

## Phase 1: Networking Foundation & Lobby (Completed 90%)
- [x] Design Network Architecture
    - [x] Transport: `LiteNetLib` (Source integrated).
    - [x] Packet Structure: OpCode (byte) + Payload (`PacketManager`).
    - [x] Serialization: `INetworkPacket` interface and helpers.
- [x] Implement Basic Connection
    - [x] Server (Host) logic.
    - [x] Client (Join) logic.
    - [x] Validation (Protocol version check).
- [x] Lobby UI (SOLID Implementation)
    - [x] Menu Hook (`TitleScreen.addMenu`).
    - [x] Host/Connect/Disconnect/Stop Logic (`LobbyViewModel`).
    - [x] Input Modals (IP, Port, Nickname) (`LobbyUI.OpenInput`).
    - [x] Localization (EN/RU).
- [x] Steam Integration
    - [x] Fetch Steam Persona Name.
    - [x] Fallback to Config Name.

## Phase 2: Player Synchronization (Visuals) (Current Focus)
- [ ] **Player Concept**
    - [ ] Create `RemotePlayer` entity/class.
    - [ ] Spawn logic on `ConnectionApproval`.
    - [ ] Despawn logic on `Disconnect`.
- [ ] **Movement Sync**
    - [ ] Position interpolation (Lerp/Dead Reckoning).
    - [ ] Velocity & State (Jumping, Rolling, Climbing).
    - [ ] Direction/Flip sync (`scaleX`).
- [ ] **Animation Sync**
    - [ ] Sync current animation state (Idle, Run, Jump, Attack).

## Phase 3: World & State Synchronization
- [ ] **World Generation**
    - [ ] Sync RNG seeds (`WorldSeed`, `LevelSeed`).
    - [ ] Ensure same layout for all clients.
- [ ] **Level Transitions**
    - [ ] Sync biome loading events.
    - [ ] "Waiting for players" screen during transitions.
- [ ] **Entity Management**
    - [ ] Shared Entity ID system (NetworkIdentity).

## Phase 4: Gameplay & Combat
- [ ] **Combat System**
    - [ ] PvP / Co-op damage toggles.
    - [ ] Health synchronization.
    - [ ] Death and Revive mechanics.
- [ ] **Inventory**
    - [ ] Sync Equipment (Weapons, Skills).
    - [ ] Sync Stats (Scrolls).

## Phase 5: Enemies & AI (Complex)
- [ ] **Host Authority**
    - [ ] Host controls all AI behavior.
    - [ ] Clients receive position/state updates.
- [ ] **Combat Interactions**
    - [ ] Client attacks -> Server validation -> Damage application.

## Phase 6: Polish & Meta
- [ ] Chat System UI (Overlay).
- [ ] Error Handling (Disconnect reasons, Timeout messages).
- [ ] Steam P2P (Nat Punchthrough alternative).
- [ ] Discord Rich Presence Integration.
