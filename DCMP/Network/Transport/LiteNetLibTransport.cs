using DCMP.Core.Interfaces;
using DCMP.Network.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;
using DCMP.Utils;
using DCMP.Core;
using System.Net;
using System.Net.Sockets;

namespace DCMP.Network.Transport;

public class LiteNetLibTransport : INetworkService
{
    private NetManager _netManager = null!;
    private EventBasedNetListener _listener = null!;
    private readonly PacketManager _packetManager = new();
    
    public bool IsHost { get; private set; }

    public void Initialize()
    {
        _listener = new EventBasedNetListener();
        _netManager = new NetManager(_listener)
        {
            AutoRecycle = true,
            ReconnectDelay = 1000,
            UnconnectedMessagesEnabled = true
        };

        // Subscribe to events
        _listener.NetworkReceiveEvent += OnNetworkReceive;
        _listener.PeerConnectedEvent += OnPeerConnected;
        _listener.PeerDisconnectedEvent += OnPeerDisconnected;
        _listener.ConnectionRequestEvent += OnConnectionRequest;

        Logger.Information("[Network] LiteNetLib Transport Initialized.");
    }

    public void StartHost(int port)
    {
        IsHost = true;
        
        if (_netManager.Start(port))
            Logger.Information($"[Network] Server started on port {port}.");
        else
            Logger.Error($"[Network] Failed to start server on port {port}.");
    }

    public void Connect(string address, int port)
    {
        IsHost = false;
        Logger.Information($"[Network] Connecting to {address}:{port}...");
        _netManager.Start();
        _netManager.Connect(address, port, "DCMP_KEY");
    }

    public void SendToAll(NetDataWriter writer, DeliveryMethod method)
    {
        _netManager.SendToAll(writer, method);
    }

    public void SendToPeer(NetPeer peer, NetDataWriter writer, DeliveryMethod method)
    {
        peer.Send(writer, method);
    }

    public void RegisterHandler(DCMP.Network.Packets.PacketType type, IPacketHandler handler)
    {
        _packetManager.RegisterHandler(type, handler);
    }

    public void Update(double dt)
    {
        _netManager.PollEvents();
    }

    public void Stop()
    {
        if (_netManager.IsRunning)
        {
            _netManager.Stop();
            Logger.Information("[Network] Transport stopped.");
        }
        IsHost = false;
        
        // Clear lobby state and notify UI
        Handlers.ConnectionHandler.Clear();
        LobbyState.Close();
    }

    public void Dispose()
    {
        Stop();
    }

    // --- Events ---

    private void OnConnectionRequest(ConnectionRequest request)
    {
        if (_netManager.ConnectedPeersCount < 10)
            request.AcceptIfKey("DCMP_KEY");
        else
            request.Reject();
    }

    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        _packetManager.HandlePacket(peer, reader, deliveryMethod);
    }

    private void OnPeerConnected(NetPeer peer)
    {
        Logger.Information($"[Network] Peer connected: {peer}");
        
        if (!IsHost)
        {
            Logger.Information("[Network] Sending connection handshake...");
            var packet = new Packets.ConnectionRequestPacket
            {
                ModVersion = "0.1.0",
                PlayerName = ConfigManager.Current.PlayerName
            };

            var writer = PacketManager.CreatePacket(Packets.PacketType.ConnectionRequest);
            packet.Serialize(writer);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Logger.Information($"[Network] Peer disconnected: {peer}. Reason: {disconnectInfo.Reason}");
        
        if (IsHost)
        {
            Handlers.ConnectionHandler.RemovePeer(peer.Id);
        }
        else
        {
            // Client side: if disconnected from server, clear lobby state and notify UI
            Handlers.ConnectionHandler.Clear();
            LobbyState.Close();
        }
    }
}
