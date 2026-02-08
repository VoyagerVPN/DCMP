namespace DCMP.Network.Packets;

public enum PacketType : byte
{
    ConnectionRequest = 0,
    ConnectionApproval = 1,
    LobbyUpdate = 3,
    GameStart = 4,
    
    // Future expansion
    Chat = 10,
    PlayerUpdate = 20,
    SpawnEntity = 30
}
