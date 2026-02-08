using System;
using DCMP.Core;
using DCMP.Network.Interfaces;
using DCMP.Utils;
using ModCore.Utilities;
using DCMP.Network;

namespace DCMP.UI;

public class LobbyViewModel
{
    public enum MenuState
    {
        Lobby,
        HostRoom,
        ClientRoom
    }

    public MenuState CurrentState { get; private set; } = MenuState.Lobby;
    public event Action? OnStateChanged;

    private readonly INetworkService _network;

    public LobbyViewModel()
    {
        _network = ServiceLocator.Get<INetworkService>();
    }

    public void HostGame()
    {
        Logger.Information("[ViewModel] Hosting game...");
        _network.StartHost(ConfigManager.Current.ServerPort);
        DCMP.Network.Handlers.ConnectionHandler.BroadcastLobbyUpdate();
        SetState(MenuState.HostRoom);
    }

    public void Connect()
    {
        Logger.Information("[ViewModel] Connecting to game...");
        _network.Connect(ConfigManager.Current.LastServerIP, ConfigManager.Current.ServerPort);
        SetState(MenuState.ClientRoom);
    }

    public void StopOrDisconnect()
    {
        Logger.Information("[ViewModel] Stopping/Disconnecting...");
        _network.Stop();
        SetState(MenuState.Lobby);
    }

    public void StartGame()
    {
        if (!_network.IsHost) return;

        Logger.Information("[ViewModel] Starting game session...");
        
        var packet = new Network.Packets.GameStartPacket();
        var writer = PacketManager.CreatePacket(Network.Packets.PacketType.GameStart);
        packet.Serialize(writer);
        _network.SendToAll(writer, LiteNetLib.DeliveryMethod.ReliableOrdered);
        
        try
        {
            var launchMode = new dc.LaunchMode.LoadSave(); // Fixed namespace
            dc.Main.Class.ME.launchGame(launchMode, null, null);
        }
        catch (Exception ex)
        {
            Logger.Error($"[ViewModel] Failed to launch host game: {ex.Message}");
        }
    }

    public void Reset()
    {
        StopOrDisconnect();
    }

    private void SetState(MenuState newState)
    {
        if (CurrentState != newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke();
        }
    }
}
