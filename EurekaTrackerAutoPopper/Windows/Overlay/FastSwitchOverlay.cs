using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Dalamud.Bindings.ImGui;

namespace EurekaTrackerAutoPopper.Windows.Overlay;

public class FastSwitchOverlay : Window, IDisposable
{
    private readonly Plugin Plugin;
    private readonly Vector2 OriginalSize = new(260, 40);

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
        DrawMapSetSwitcher();
    }

    public void DrawMapSetSwitcher()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("Switch To:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(175.0f * ImGuiHelpers.GlobalScale);
        using var combo = ImRaii.Combo("##SwitchMarkersToCombo", Plugin.MarkerSetToPlace.ToOccultSet().ToName());
        if (!combo.Success)
            return;

        foreach (var set in EnumExtensions.OccultSetArray)
        {
            if (!ImGui.Selectable(set.ToName(), set == Plugin.MarkerSetToPlace.ToOccultSet()))
                continue;

            Plugin.SavedOccultMarkerSets = null;
            Plugin.PlaceOccultMarkerSet(set, true);
        }
    }
}
