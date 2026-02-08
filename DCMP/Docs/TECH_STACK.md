# DCMP Tech Stack

## Core
- **Framework:** .NET 10 (Preview/Latest)
- **Modding API:** `DeadCellsCoreModding.MDK`
- **Game Engine:** Custom (Heaps/HashLink)

## Network Architecture
- **Topology:** Host-Authoritative (LAN Emulation)
- **Transport:** `LiteNetLib` (UDP)
  - Source: `d:\Pet\DCMP\LiteNetLib-2.0.0`
- **Serialization:** Custom Binary (BinaryWriter/Reader) for packets.
  - *Reasoning:* Manual control is simpler and faster than interoping with `hxbit` for C#->C# communication. We only marshal to game types at the API boundary.

## Tools
- `ilspy` for decompilation
- `dotnet` CLI for build
- **Documentation:**
  - [Dead Cells Core Modding Docs](https://dead-cells-core-modding.github.io/docs/docs/)
  - Local: `d:\Pet\DCMP\DCMP\Docs\SDK Docs\docs`
