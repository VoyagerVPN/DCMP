using LiteNetLib.Utils;
using System.Collections.Generic;

namespace DCMP.Network.Packets;

public struct LobbyUpdatePacket : INetSerializable
{
    public List<string> PlayerNames;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PlayerNames.Count);
        foreach (var name in PlayerNames)
        {
            writer.Put(name);
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        int count = reader.GetInt();
        PlayerNames = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            PlayerNames.Add(reader.GetString());
        }
    }
}

public struct GameStartPacket : INetSerializable
{
    public void Serialize(NetDataWriter writer) { }
    public void Deserialize(NetDataReader reader) { }
}
