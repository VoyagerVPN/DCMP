using dc;
using dc.pr;
using dc.ui;
using Hashlink.Virtuals;
using HaxeProxy.Runtime;
using DCMP.Core;
using DCMP.Network.Interfaces;
using DCMP.Utils;
using ModCore.Utilities;
using Hashlink;

namespace DCMP.UI;

public class LobbyUI
{
    private static bool _isHubMode = false;
    private static bool _isProcessingInjection = false;
    private static readonly LobbyViewModel _viewModel = new();
    private static LobbyModal? _activeModal;

    public static void Initialize()
    {
        Hook_TitleScreen.addMenu += OnAddMenu;
        Logger.Information("[LobbyUI] Hooked.");
    }

    private static virtual_cb_help_inter_isEnable_t_<bool> OnAddMenu(
        Hook_TitleScreen.orig_addMenu orig,
        TitleScreen self,
        dc.String str,
        HlAction cb,
        dc.String help,
        bool? isEnable,
        Ref<int> color)
    {
        // 1. If we are building our own menu, pass through to original implementation
        if (_isProcessingInjection) 
            return orig(self, str, cb, help, isEnable, color);

        // 2. If we are in Hub Mode, BLOCK all standard game menu items (like "About", "Options")
        // preventing them from mixing with our UI
        if (_isHubMode)
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.

        // 3. Normal behavior: draw the game menu item
        var result = orig(self, str, cb, help, isEnable, color);
        
        // 4. Inject our "Play Online" entry point if we see the Main Menu
        if (!_isHubMode && self.isMainMenu && self.menuItems.length == 1)
        {
            _isProcessingInjection = true;
            Logger.Information("[LobbyUI] Injecting Play Online button...");
            
            // Use specific color for our button (Blue-ish)
            int colorVal = 0x90d2ff;
            var r = new HaxeProxy.Runtime.Ref<int>(ref colorVal);
            
            orig(self, LocalizationService.Translate("dcmp_menu_play_online"), new HlAction(() => 
            {
                Logger.Information("[LobbyUI] Play Online clicked.");
                _isHubMode = true;
                _viewModel.Reset(); 
                self.clearMenu();
                RefreshMenu(self, orig);
            }), LocalizationService.Translate("dcmp_menu_multiplayer"), null, r);
            _isProcessingInjection = false;
        }
        return result;
    }

    private static void RefreshMenu(TitleScreen self, Hook_TitleScreen.orig_addMenu orig)
    {
        _isProcessingInjection = true;
        Logger.Information($"[LobbyUI] Refreshing menu. Current State: {_viewModel.CurrentState}");

        // Create color reference once (Blue-ish)
        int colorVal = 0x90d2ff;
        var r = new HaxeProxy.Runtime.Ref<int>(ref colorVal);
        
        // HACK: Temporarily disable isMainMenu to prevent ModCore (and others) 
        // from injecting their "About Core Modding" button into our lobby menu.
        // ModCore checks: if (((ArrayBase)self.menuItems).length == 3 && self.isMainMenu)
        bool wasMainMenu = self.isMainMenu;
        self.isMainMenu = false;

        try 
        {
            switch (_viewModel.CurrentState)
            {
                case LobbyViewModel.MenuState.Lobby:
                    RenderLobbyMenu(self, orig, r);
                    break;
                case LobbyViewModel.MenuState.HostRoom:
                    RenderHostRoom(self, orig, r);
                    EnsureModal();
                    break;
                case LobbyViewModel.MenuState.ClientRoom:
                    RenderClientRoom(self, orig, r);
                    EnsureModal();
                    break;
            }

            // ... selecting first item ...
            bool dummy = false;
            var rBool = new HaxeProxy.Runtime.Ref<bool>(ref dummy);
            if (self.menuItems != null && self.menuItems.length > 0)
            {
                self.select(0, rBool);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[LobbyUI] RefreshMenu crash: {ex}");
        }
        finally 
        {
            // Restore isMainMenu state
            self.isMainMenu = wasMainMenu;
            _isProcessingInjection = false;
        }
    }

    private static void OpenInput(string title, string currentVal, Action<string> onCommit)
    {
        try 
        {
            Logger.Information($"[LobbyUI] Opening Input: {title}");

            // Create Haxe Action wrapper
            var hAction = new HlAction<dc.String>((s) => 
            {
                string res = s.ToString();
                Logger.Information($"[LobbyUI] Input Validated: {res}");
                onCommit?.Invoke(res);
            });

            var textInput = new dc.ui.TextInput(
                null, 
                title.AsHaxeString(),      
                "".AsHaxeString(),       
                currentVal.AsHaxeString(), 
                hAction, 
                LocalizationService.Translate("dcmp_menu_save"), 
                null, 
                null
            );
            
            Logger.Information("[LobbyUI] TextInput created.");
        }
        catch(Exception ex)
        {
            Logger.Error($"[LobbyUI] Failed to open input: {ex}");
        }
    }

    private static void RenderLobbyMenu(TitleScreen self, Hook_TitleScreen.orig_addMenu orig, Ref<int> colorRef)
    {   
        int whiteVal = 0xFFFFFF;
        var whiteColor = new HaxeProxy.Runtime.Ref<int>(ref whiteVal);

        // 1. Host Game
        orig(self, LocalizationService.Translate("dcmp_menu_host"), new HlAction(() => 
        {
            _viewModel.HostGame();
            self.clearMenu();
            RefreshMenu(self, orig);
        }), LocalizationService.Translate("dcmp_help_host"), null, whiteColor);

        // 2. Connect
        orig(self, LocalizationService.Translate("dcmp_menu_connect"), new HlAction(() => 
        {
            _viewModel.Connect();
            self.clearMenu();
            RefreshMenu(self, orig);
        }), LocalizationService.Translate("dcmp_help_connect"), null, whiteColor);

        // 3. IP
        orig(self, LocalizationService.Translate("dcmp_menu_target_ip", new { ip = ConfigManager.Current.LastServerIP }), new HlAction(() => 
        {
            OpenInput("Enter IP", ConfigManager.Current.LastServerIP, (res) => 
            {
                if (!string.IsNullOrWhiteSpace(res)) 
                {
                    ConfigManager.Current.LastServerIP = res.Trim();
                    ConfigManager.Save();
                    self.clearMenu();
                    RefreshMenu(self, orig);
                }
            });
        }), LocalizationService.Translate("dcmp_help_ip"), null, whiteColor);

        // 4. Port
        orig(self, LocalizationService.Translate("dcmp_menu_change_port", new { port = ConfigManager.Current.ServerPort }), new HlAction(() => 
        {
            OpenInput("Enter Port", ConfigManager.Current.ServerPort.ToString(), (res) => 
            {
                if (int.TryParse(res, out int p)) 
                {
                    ConfigManager.Current.ServerPort = p;
                    ConfigManager.Save();
                    self.clearMenu(); // Refresh
                    RefreshMenu(self, orig);
                }
            });
        }), LocalizationService.Translate("dcmp_help_port"), null, whiteColor);

        // 5. Nickname
        orig(self, LocalizationService.Translate("dcmp_menu_change_name", new { name = ConfigManager.Current.PlayerName }), new HlAction(() => 
        {
            OpenInput("Enter Name", ConfigManager.Current.PlayerName, (res) => 
            {
                if (!string.IsNullOrWhiteSpace(res)) 
                {
                    ConfigManager.Current.PlayerName = res.Trim();
                    ConfigManager.Save();
                    self.clearMenu();
                    RefreshMenu(self, orig);
                }
            });
        }), LocalizationService.Translate("dcmp_help_nick"), null, whiteColor);

        // 6. Back
        orig(self, LocalizationService.Translate("dcmp_menu_back"), new HlAction(() => 
        {
            _isHubMode = false;
            CleanupModal();
            self.mainMenu();
        }), LocalizationService.Translate("dcmp_help_back"), null, whiteColor);
    }

    private static void RenderHostRoom(TitleScreen self, Hook_TitleScreen.orig_addMenu orig, Ref<int> colorRef)
    {
        int whiteVal = 0xFFFFFF;
        var whiteColor = new HaxeProxy.Runtime.Ref<int>(ref whiteVal);

        orig(self, LocalizationService.Translate("dcmp_menu_start_game"), new HlAction(() => 
        {
            CleanupModal(); 
            _viewModel.StartGame();
        }), LocalizationService.Translate("dcmp_help_start"), null, whiteColor);

        orig(self, LocalizationService.Translate("dcmp_menu_stop_server"), new HlAction(() => 
        {
            _viewModel.StopOrDisconnect();
            CleanupModal();
            self.clearMenu();
            RefreshMenu(self, orig);
        }), LocalizationService.Translate("dcmp_help_stop"), null, whiteColor);
    }

    private static void RenderClientRoom(TitleScreen self, Hook_TitleScreen.orig_addMenu orig, Ref<int> colorRef)
    {
        int whiteVal = 0xFFFFFF;
        var whiteColor = new HaxeProxy.Runtime.Ref<int>(ref whiteVal);

        orig(self, LocalizationService.Translate("dcmp_menu_disconnect"), new HlAction(() => 
        {
            _viewModel.StopOrDisconnect();
            CleanupModal();
            self.clearMenu();
            RefreshMenu(self, orig);
        }), LocalizationService.Translate("dcmp_help_disconnect"), null, whiteColor);
    }

    private static void EnsureModal()
    {
        try 
        {
            if (_activeModal == null || _activeModal.destroyed)
            {
                Logger.Information("[LobbyUI] Creating LobbyModal...");
                _activeModal = new LobbyModal();
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[LobbyUI] EnsureModal crash: {ex}");
        }
    }

    private static void CleanupModal()
    {
        if (_activeModal != null)
        {
            _activeModal.ForceClose();
            _activeModal = null;
        }
    }
}
