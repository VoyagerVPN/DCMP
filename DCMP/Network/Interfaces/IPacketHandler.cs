using LiteNetLib;
using LiteNetLib.Utils;
using DCMP.Network.Packets;

namespace DCMP.Network.Interfaces;

public interface IPacketHandler
{
    void Handle(NetPeer peer, NetDataReader reader, PacketType type, DeliveryMethod method);
}
