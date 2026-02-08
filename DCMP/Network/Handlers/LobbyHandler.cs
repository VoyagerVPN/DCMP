using LiteNetLib;
using LiteNetLib.Utils;
using DCMP.Network.Interfaces;
using DCMP.Network.Packets;
using DCMP.Core;
using DCMP.Utils;
using dc;

namespace DCMP.Network.Handlers;

public class LobbyHandler : IPacketHandler
{
    public void Handle(NetPeer peer, NetDataReader reader, PacketType type, DeliveryMethod method)
    {
        switch (type)
        {
            case PacketType.LobbyUpdate:
                HandleLobbyUpdate(peer, reader);
                break;
            case PacketType.GameStart:
                HandleGameStart(peer, reader);
                break;
        }
    }

    private void HandleLobbyUpdate(NetPeer peer, NetDataReader reader)
    {
        var packet = new LobbyUpdatePacket();
        packet.Deserialize(reader);
        LobbyState.UpdatePlayers(packet.PlayerNames ?? new List<string>());
        Logger.Information("[Lobby] Received lobby update.");
    }

    private void HandleGameStart(NetPeer peer, NetDataReader reader)
    {
        Logger.Information("[Lobby] Host started the game! Launching...");
        
        // Use LaunchMode.LoadSave to load current save/continue
        // This corresponds to "Continue" behavior
        var launchMode = new LaunchMode.LoadSave();
        
        try
        {
            // Close any open modals/menus?
            // Usually TitleScreen handles this transitions but we might have LobbyModal open.
            // LobbyModal should probably listen to GameStart event or just be forcefully closed.
            // But Main.launchGame triggers loading screen which should cover everything.
            
            Main.Class.ME.launchGame(launchMode, null, null);
        }
        catch (System.Exception ex)
        {
            Logger.Error($"[Lobby] Failed to launch game: {ex}");
        }
    }
}
