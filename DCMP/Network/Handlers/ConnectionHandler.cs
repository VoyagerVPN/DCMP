using LiteNetLib;
using LiteNetLib.Utils;
using DCMP.Network.Interfaces;
using DCMP.Network.Packets;
using DCMP.Utils;
using DCMP.Core;
using System.Collections.Generic;
using System.Linq;

namespace DCMP.Network.Handlers;

public class ConnectionHandler : IPacketHandler
{
    private const string MOD_VERSION = "0.1.0";
    
    // Server-side mapping of Peer ID to Player Name
    private static readonly Dictionary<int, string> _peerToName = new();

    public void Handle(NetPeer peer, NetDataReader reader, PacketType type, DeliveryMethod method)
    {
        var networkService = ServiceLocator.Get<INetworkService>();

        if (networkService.IsHost)
        {
            if (type == PacketType.ConnectionRequest)
                HandleRequest(peer, reader);
        }
        else
        {
            if (type == PacketType.ConnectionApproval)
                HandleApproval(peer, reader);
        }
    }

    private void HandleRequest(NetPeer peer, NetDataReader reader)
    {
        var packet = new ConnectionRequestPacket();
        packet.Deserialize(reader);

        Logger.Information($"[Network] Connection request from {packet.PlayerName} (v{packet.ModVersion})");
        Logger.Information($"[ConnectionHandler] Peer count before add: {_peerToName.Count}");

        var response = new ConnectionApprovalPacket();
        if (packet.ModVersion != MOD_VERSION)
        {
            response.Success = false;
            response.Message = $"Version mismatch! Server: {MOD_VERSION}, Client: {packet.ModVersion}";
        }
        else
        {
            response.Success = true;
            response.Message = "Welcome to DCMP!";
            
            // Add to session
            _peerToName[peer.Id] = packet.PlayerName ?? "Unknown";
            Logger.Information($"[ConnectionHandler] Added peer {peer.Id}. Total peers: {_peerToName.Count}");
            
            // Broadcast lobby update
            Logger.Information("[ConnectionHandler] Broadcasting lobby update...");
            BroadcastLobbyUpdate();
        }

        var writer = PacketManager.CreatePacket(PacketType.ConnectionApproval);
        response.Serialize(writer);
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void HandleApproval(NetPeer peer, NetDataReader reader)
    {
        var packet = new ConnectionApprovalPacket();
        packet.Deserialize(reader);

        if (packet.Success)
        {
            Logger.Information($"[Network] Connected successfully: {packet.Message}");
        }
        else
        {
            Logger.Warning($"[Network] Connection failed: {packet.Message}");
            peer.Disconnect();
        }
    }

    public static void BroadcastLobbyUpdate()
    {
        var names = new List<string> { ConfigManager.Current.PlayerName }; // Host name
        names.AddRange(_peerToName.Values);
        
        Logger.Information($"[ConnectionHandler] Updating lobby state with players: {string.Join(", ", names)}");
        LobbyState.UpdatePlayers(names);

        var packet = new LobbyUpdatePacket { PlayerNames = names };
        var writer = PacketManager.CreatePacket(PacketType.LobbyUpdate);
        packet.Serialize(writer);
        
        ServiceLocator.Get<INetworkService>().SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }

    public static void RemovePeer(int peerId)
    {
        if (_peerToName.Remove(peerId))
        {
            BroadcastLobbyUpdate();
        }
    }

    public static void Clear()
    {
        _peerToName.Clear();
        LobbyState.UpdatePlayers(new List<string>());
    }
}
