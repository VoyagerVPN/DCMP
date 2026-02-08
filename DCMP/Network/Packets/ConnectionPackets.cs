using LiteNetLib.Utils;

namespace DCMP.Network.Packets;

public struct ConnectionRequestPacket : INetSerializable
{
    public string ModVersion;
    public string PlayerName;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ModVersion);
        writer.Put(PlayerName);
    }

    public void Deserialize(NetDataReader reader)
    {
        ModVersion = reader.GetString();
        PlayerName = reader.GetString();
    }
}

public struct ConnectionApprovalPacket : INetSerializable
{
    public bool Success;
    public string Message;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Success);
        writer.Put(Message);
    }

    public void Deserialize(NetDataReader reader)
    {
        Success = reader.GetBool();
        Message = reader.GetString();
    }
}
