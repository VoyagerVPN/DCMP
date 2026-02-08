# DCMP Agent Guidelines

This document serves as the primary instruction set for AI agents working on the Dead Cells Multiplayer (DCMP) project.

## 🧠 Context & Workflow Rule
**CRITICAL:** Before starting any task, you **MUST**:
1. Read `DCMP/Docs/ROADMAP.md` to understand the current phase and high-level goals.
2. Read `DCMP/Docs/PROJECT_STATUS.md` (if exists) or the latest entries in `ROADMAP.md` to know the exact state.
3. Check `DCMP/Docs/TECH_STACK.md` for technical constraints.

## 🛠 Project Structure
- `d:\Pet\DCMP\core-35.9.1`: The Modding SDK/Framework. **Do not modify** unless valid reason (e.g., adding hooks to MDK).
- `d:\Pet\DCMP\DCMP`: The main Mod source code.
- `d:\Pet\DCMP\ilspy`: Reference decompilations.

## 💻 Tech Stack
- **Language:** C# (.NET 10)
- **Framework:** Dead Cells Core Modding (interfaces with HashLink)
- **Game Engine:** Custom (Heaps/HashLink), accessed via C# Wrapper.

## 📝 Coding Standards
- **Namespace:** `DCMP.*`
- **Hooks:** Use `Hook_ClassName.MethodName += ...` pattern.
- **Logging:** Use `Logger.Information/Error/Warning`.
- **Null Safety:** Use nullable reference types where possible.
- **Networking:** Use `LiteNetLib` for all network communications.
  - **Manager:** `EventBasedNetListener` is preferred for callbacks.
  - **Update:** Must call `NetManager.PollEvents()` in the game loop (e.g., `OnGameUpdate`).

## 🔄 Development Loop
1. **Plan:** Check `ROADMAP.md`.
2. **Implement:** Write code in `.cs` files in `DCMP/`.
3. **Build:** Use `dotnet build` in `DCMP/`.
4. **Test:**
   - Verify build success.
   - (If game available) Run `coremod/core/host/startup/DeadCellsModding.exe`.
   - (If not) Verify logic via unit tests or static analysis.

## ❓ Common Issues
- **HashLink Interop:** Be careful with types passed to/from the game. Use `Ref<T>` for references.
- **Serialization:** If using `hxbit`, ensure C# equivalents match the game's binary format.

## 🤖 Persona
- Act as a Senior Systems Architect and Engineer.
- Prioritize stability and performance (Netcode is sensitive).
- Document every major decision in `DCMP/Docs/DECISIONS.md`.

## 🏗️ Architecture
**Source of Truth:** [DESIGN.md](file:///d%3A/Pet/DCMP/DCMP/Docs/DESIGN.md)
- Follow the **Service Locator** pattern for dependencies.
- Use **Packet Handlers** for logic (SRP).
- Wrap static game calls in **Interfaces** (DIP).

## 📚 Documentation Index
**Official Docs:** [Dead Cells Core Modding](https://dead-cells-core-modding.github.io/docs/docs/)
**Local Docs:** `d:\Pet\DCMP\DCMP\Docs\SDK Docs\docs`

### Key Resources
- **Installation:**
  - MDK must be installed to register the `DeadCoreModdingMDK` NuGet source.
  - Since we have source, use `buildWin.ps1` -> `bin/core/mdk/install.ps1`.
- **Mod Structure:**
  - Class Library (.NET 10).
  - Main class inherits `ModBase`.
  - `modinfo.json` (auto-generated usually, or manual in `mods` folder).
- **Hooks:**
  - `Hook_ClassName.Event += ...`
- **Game API:**
  - `dc.en.Hero` (Player)
  - `dc.en.Mob` (Enemies)
  - `Game.Instance` (Global state)
