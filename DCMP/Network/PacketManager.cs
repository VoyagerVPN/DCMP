using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using DCMP.Network.Interfaces;
using DCMP.Network.Packets;
using DCMP.Utils;

namespace DCMP.Network;

public class PacketManager
{
    private readonly Dictionary<PacketType, IPacketHandler> _handlers = new();

    public void RegisterHandler(PacketType type, IPacketHandler handler)
    {
        _handlers[type] = handler;
    }

    public void HandlePacket(NetPeer peer, NetPacketReader reader, DeliveryMethod method)
    {
        if (reader.AvailableBytes < 1) return;

        PacketType type = (PacketType)reader.GetByte();
        if (_handlers.TryGetValue(type, out var handler))
        {
            handler.Handle(peer, reader, type, method);
        }
        else
        {
            Logger.Warning($"[Network] No handler registered for packet type: {type}");
        }
    }

    public static NetDataWriter CreatePacket(PacketType type)
    {
        NetDataWriter writer = new();
        writer.Put((byte)type);
        return writer;
    }
}
