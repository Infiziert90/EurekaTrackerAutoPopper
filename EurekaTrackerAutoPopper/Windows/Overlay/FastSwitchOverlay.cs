using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using EurekaTrackerAutoPopper.Resources;

namespace EurekaTrackerAutoPopper.Windows.Overlay;

public class FastSwitchOverlay : Window, IDisposable
{
    private readonly Plugin Plugin;
    private readonly Vector2 OriginalSize = new(295, 55);

    public FastSwitchOverlay(Plugin plugin) : base("Linker: Fast Switch##EurekaLinker")
    {
        Size = OriginalSize;

        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        RespectCloseHotkey = false;
        DisableWindowSounds = true;
        ForceMainWindow = true;

        Plugin = plugin;
    }

    public void Dispose() { }

    public override unsafe void PreOpenCheck()
    {
        IsOpen = false;
        if (!Plugin.Configuration.ShowFastSwitcher)
            return;

        try
        {
            if (Plugin.ClientState.TerritoryType != 1252 || AgentMap.Instance()->SelectedMapId != 967)
                return;

            var mapBaseNode = Plugin.GameGui.GetAddonByName("AreaMap");
            if (mapBaseNode == nint.Zero)
                return;

            if (!mapBaseNode.IsVisible)
                return;

            var posY = mapBaseNode.Y - OriginalSize.Y * ImGuiHelpers.GlobalScale;
            if (Plugin.Configuration.SwitcherBelowMap)
                posY = mapBaseNode.Y + mapBaseNode.ScaledHeight;

            Position = new Vector2(mapBaseNode.X + 5, posY);
            PositionCondition = ImGuiCond.Always;

            IsOpen = true;
        }
        catch
        {
            // Something went wrong, we don't draw
        }
    }

    public override void Draw()
    {
        using var disabled = ImRaii.Disabled(Plugin.MapMarkerController.SavedOccultMarkerSets != null);
        if (disabled.Success && ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(Language.AutoMarkerActiveWarning);

        var flagsChanged = false;
        var current = Plugin.MapMarkerController.MarkerSetToPlace;
        flagsChanged |= Helper.ImageButtonWithState(Icons.BronzeTreasure, FlagMarkerSet.OccultBronzeTreasure, ref current);
        ImGui.SameLine();
        flagsChanged |= Helper.ImageButtonWithState(Icons.SilverTreasure, FlagMarkerSet.OccultSilverTreasure, ref current);
        ImGui.SameLine();
        flagsChanged |= Helper.ImageButtonWithState(Icons.GoldChest, FlagMarkerSet.OccultNorthPot, ref current);
        ImGui.SameLine();
        flagsChanged |= Helper.ImageButtonWithState(Icons.GoldChest, FlagMarkerSet.OccultSouthPot, ref current);
        ImGui.SameLine();
        flagsChanged |= Helper.ImageButtonWithState(Icons.Reroll, FlagMarkerSet.OccultReroll, ref current);
        ImGui.SameLine();
        flagsChanged |= Helper.ImageButtonWithState(Plugin.PenumbraIpc.GetReplacedIcon, FlagMarkerSet.OccultBunny, ref current);

        if (flagsChanged)
        {
            Plugin.MapMarkerController.RevertTempMarkerSet();
            Plugin.MapMarkerController.SetMarkerSet(current);
        }
    }
}
