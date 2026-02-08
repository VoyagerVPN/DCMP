using dc;
using dc.ui;
using dc.h2d;
using dc.libs;
using dc.pr;
using dc.hl.types;
using DCMP.Core;
using DCMP.Utils;
using ModCore.Utilities;
using System;
using System.Collections.Generic;

namespace DCMP.UI;

public class LobbyModal : dc.ui.Process
{
    private FlowBox fb;
    private List<dc.ui.Text> texts = new();

    public LobbyModal() : base(Main.Class.ME)
    {
        // 1. Initialize process with Main.ME
        // This is a passive process, so we DON'T block input or pause game.

        // 2. Create root layer at menu depth
        // ROOT_DP_MENU is where game menus and popups usually live
        this.createRootInLayers(Main.Class.ME.root, Const.Class.ROOT_DP_MENU);
        
        // 3. Create styled FlowBox
        // Using validation box style (dark semi-transparent background + biome-aware colors)
        double padHVar = 16.0;
        double padVVar = 20.0;
        var rH = new HaxeProxy.Runtime.Ref<double>(ref padHVar);
        var rV = new HaxeProxy.Runtime.Ref<double>(ref padVVar);
        fb = dc.ui.FlowBox.Class.createBoxValidationWithBiomeParam.Invoke(this.root, rH, rV);
        fb.set_isVertical(true);
        fb.set_horizontalAlign(new FlowAlign.Middle());
        fb.set_verticalAlign(new FlowAlign.Middle());
        
        LobbyState.OnLobbyUpdated += Refresh;
        LobbyState.OnLobbyClosed += ForceClose;

        Refresh();
        onResize();
        
        Logger.Information("[LobbyModal] Reactive passive UI initialized.");
    }

    public void Refresh()
    {
        try 
        {
            if (fb == null) return;
            fb.removeChildren();
            
            texts.Clear();
            
            var players = LobbyState.ConnectedPlayers ?? new List<string>();
            string hostName = players.Count > 0 ? players[0] : (ConfigManager.Current?.PlayerName ?? "Player");

            AddText(LocalizationService.Translate("dcmp_lobby_title", new { name = hostName }), 0x90d2ff, true);
            AddText("----------------".AsHaxeString(), 0x90d2ff, false);

            if (players.Count == 0)
            {
                AddText(LocalizationService.Translate("dcmp_lobby_connecting"), 0x90d2ff, false);
            }
            else
            {
                foreach (var player in players)
                {
                    AddText($"> {player}".AsHaxeString(), 0x90d2ff, false);
                }
            }

            AddText(" ".AsHaxeString(), 0, false);
            AddText(LocalizationService.Translate("dcmp_lobby_players_count", new { count = players.Count }), 0x90d2ff, false);

            fb.reflow();
            onResize();
        }
        catch (Exception ex)
        {
            Logger.Error($"[LobbyModal] Refresh failed: {ex}");
        }
    }

    private void AddText(dc.String str, int color, bool isBig)
    {
        // Replicating ModalPopUp.text() logic:
        // Text(Object parent, bool? big, bool? smooth, Ref<double> font, String text, Font font)
        bool bigFlag = isBig;
        double scale = 1.0;
        var r = new HaxeProxy.Runtime.Ref<double>(ref scale);
        dc.ui.Text t = new dc.ui.Text(fb, (bool?)(object)bigFlag, null, r, null, null);
        t.set_textColor(color);
        t.set_text(str);
        texts.Add(t);
    }

    public override void onResize()
    {
        base.onResize();
        if (fb == null) return;

        double pixelScale = Main.Class.ME.pixelScale;
        fb.set_verticalSpacing((int)(pixelScale * 10.0));
        
        // stage size detection logic from ModalPopUp
        int stageWidth = dc.libs.Process.Class.CUSTOM_STAGE_WIDTH;
        if (stageWidth <= 0) stageWidth = dc.hxd.Window.Class.getInstance.Invoke().get_width();
        
        int stageHeight = dc.libs.Process.Class.CUSTOM_STAGE_HEIGHT;
        if (stageHeight <= 0) stageHeight = dc.hxd.Window.Class.getInstance.Invoke().get_height();

        // proportional sizing from ModalPopUp
        fb.set_maxWidth((int?)(object)(int)(stageWidth * 0.5));
        fb.set_minHeight((int?)(object)(int)(stageHeight * 0.4));
        
        fb.reflow();

        // Center it (shift by 1 bit = divide by 2)
        fb.posChanged = true;
        fb.x = (stageWidth - fb.get_outerWidth()) >> 1;
        fb.y = (stageHeight - fb.get_outerHeight()) >> 1;
    }

    public override void update()
    {
        base.update();
    }

    public void ForceClose()
    {
        LobbyState.OnLobbyUpdated -= Refresh;
        LobbyState.OnLobbyClosed -= ForceClose;
        
        this.destroy(); 
    }
}
