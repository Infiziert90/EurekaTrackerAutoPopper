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
            if (!TerritoryHelper.PlayerInOccult() || !TerritoryHelper.IsOccultMap(AgentMap.Instance()->SelectedMapId))
                return;

            var mapBaseNode = Plugin.GameGui.GetAddonByName("AreaMap");
            if (mapBaseNode == nint.Zero)
                return;

            if (!mapBaseNode.IsVisible)
                return;

            Size = CalculateRequiredSize(EnumExtensions.IconArray.Length + 1) / ImGuiHelpers.GlobalScale;

            var posY = mapBaseNode.Y - Size.Value.Y * ImGuiHelpers.GlobalScale;
            if (Plugin.Configuration.SwitcherBelowMap)
                posY = mapBaseNode.Y + mapBaseNode.ScaledHeight;

            if (!Plugin.Configuration.SwitcherMoveable)
            {
                Position = new Vector2(mapBaseNode.X + 5, posY);
                PositionCondition = ImGuiCond.Always;
                Flags |= ImGuiWindowFlags.NoMove;
            }
            else
            {
                PositionCondition = ImGuiCond.Once;
                Flags &= ~ImGuiWindowFlags.NoMove;
            }

            IsOpen = true;
        }
        catch
        {
            // Something went wrong, we don't draw
        }
    }

    public override void Draw()
    {
        var isDisabled = Plugin.MapMarkerController.SavedOccultMarkerSets != null;

        using var disabled = ImRaii.Disabled(isDisabled);
        if (isDisabled && ImGui.IsWindowHovered())
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
        ImGui.SameLine();
        flagsChanged |= Helper.ImageButtonWithState(Icons.SurveyPoint, FlagMarkerSet.OccultSurvey, ref current);
        ImGui.SameLine();

        if (flagsChanged)
        {
            Plugin.MapMarkerController.RevertTempMarkerSet();
            Plugin.MapMarkerController.SetMarkerSet(current);
        }
    }

    private Vector2 CalculateRequiredSize(float numberOfButtons)
    {
        var style = ImGui.GetStyle();
        var buttonSize = (Helper.IconSize * ImGuiHelpers.GlobalScale) + style.FramePadding * 2.0f;
        var spacing = style.ItemSpacing.X * (numberOfButtons - 1);
        var windowPadding = style.WindowPadding * 2.0f;

        var width = buttonSize.X * numberOfButtons + spacing + windowPadding.X;
        var height = buttonSize.Y + windowPadding.Y;

        return new Vector2(width, height);
    }
}
