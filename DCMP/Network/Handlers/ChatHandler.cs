using LiteNetLib;
using LiteNetLib.Utils;
using DCMP.Network.Interfaces;
using DCMP.Network.Packets;
using DCMP.Utils;

namespace DCMP.Network.Handlers;

public class ChatHandler : IPacketHandler
{
    public void Handle(NetPeer peer, NetDataReader reader, PacketType type, DeliveryMethod method)
    {
        if (type != PacketType.Chat) return;
        
        string message = reader.GetString();
        Logger.Information($"[Chat] {peer}: {message}");
    }
}
