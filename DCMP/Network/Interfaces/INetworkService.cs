using DCMP.Core.Interfaces;
using LiteNetLib;
using LiteNetLib.Utils;

namespace DCMP.Network.Interfaces;

public interface INetworkService : IService
{
    bool IsHost { get; }
    void StartHost(int port);
    void Connect(string address, int port);
    void SendToAll(NetDataWriter writer, DeliveryMethod method);
    void SendToPeer(NetPeer peer, NetDataWriter writer, DeliveryMethod method);
    void RegisterHandler(DCMP.Network.Packets.PacketType type, IPacketHandler handler);
    void Stop();
}
