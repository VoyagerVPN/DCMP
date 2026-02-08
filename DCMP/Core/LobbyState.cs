using System.Collections.Generic;
using DCMP.Network.Packets;
using DCMP.Utils;

namespace DCMP.Core;

public class LobbyState
{
    public static List<string> ConnectedPlayers { get; } = new();
    
    public static event System.Action? OnLobbyUpdated;
    public static event System.Action? OnLobbyClosed;

    public static void UpdatePlayers(List<string> players)
    {
        Logger.Information($"[LobbyState] Updating players. Count: {players?.Count ?? 0}. Callback has subscribers: {OnLobbyUpdated != null}");
        ConnectedPlayers.Clear();
        if (players != null)
            ConnectedPlayers.AddRange(players);
        OnLobbyUpdated?.Invoke();
    }

    public static void Close()
    {
        ConnectedPlayers.Clear();
        OnLobbyClosed?.Invoke();
    }
}
