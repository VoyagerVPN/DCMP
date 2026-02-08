using System.Reflection;
using System.IO;
using DCMP.Core;
using DCMP.Network.Interfaces;
using DCMP.Network.Transport;
using DCMP.Utils;
using ModCore.Events.Interfaces.Game;
using ModCore.Mods;

namespace DCMP;

public class ModEntry(ModInfo info) : ModBase(info),
    IOnFrameUpdate,
    IOnGameInit,
    IOnGameExit
{
    public override void Initialize()
    {
        Logger.Information("DCMP Initializing...");
        
        // 0. Load Config
        string? modDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        ConfigManager.Initialize(modDir);

        // -- New Localization System --
        Logger.Information($"[DCMP] Mod Directory for localization: {modDir}");
        if (string.IsNullOrEmpty(modDir))
        {
             Logger.Error("[DCMP] Failed to get mod directory!");
        }
        else
        {
            LocalizationService.Initialize(modDir);
        }
        
        // Nickname Logic moved to OnGameInit because Steam is not initialized yet here.
        
        Logger.Information($"[DCMP] Initial Player Name (Config): {ConfigManager.Current.PlayerName}");

        // 1. Register Services
        var network = new LiteNetLibTransport();
        ServiceLocator.Register<INetworkService>(network);

        // 2. Register Packet Handlers
        var connHandler = new DCMP.Network.Handlers.ConnectionHandler();
        var lobbyHandler = new DCMP.Network.Handlers.LobbyHandler();
        network.RegisterHandler(DCMP.Network.Packets.PacketType.ConnectionRequest, connHandler);
        network.RegisterHandler(DCMP.Network.Packets.PacketType.ConnectionApproval, connHandler);
        network.RegisterHandler(DCMP.Network.Packets.PacketType.Chat, new DCMP.Network.Handlers.ChatHandler());

        network.RegisterHandler(DCMP.Network.Packets.PacketType.LobbyUpdate, lobbyHandler);
        network.RegisterHandler(DCMP.Network.Packets.PacketType.GameStart, lobbyHandler);

        // 3. Initialize UI Hooks
        DCMP.UI.LobbyUI.Initialize();

        Logger.Information("DCMP Systems Ready.");
    }

    public void OnGameInit()
    {
        Logger.Information("[DCMP] Game Initialized. Attempting to fetch Steam Name...");
        // Nickname Logic:
        // 1. Try to get Steam Name
        // 2. If Steam Name is valid (not "GuestPlayer"), use it and UPDATE config for this session
        
        string steamName = SteamHelper.GetPlayerName();
        if (steamName != "GuestPlayer")
        {
            ConfigManager.Current.PlayerName = steamName;
            Logger.Information($"[DCMP] Steam Name fetched: {steamName}");
        }
        else
        {
            Logger.Warning("[DCMP] Could not fetch Steam Name (or user is guest). Keeping config name.");
        }
    }

    public void OnFrameUpdate(double dt)
    {
        ServiceLocator.UpdateAll(dt);
    }

    public void OnGameExit()
    {
        ConfigManager.Save();
        ServiceLocator.DisposeAll();
        Logger.Information("DCMP Shutdown.");
    }
}
