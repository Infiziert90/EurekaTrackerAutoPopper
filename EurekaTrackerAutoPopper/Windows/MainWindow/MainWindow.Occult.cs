using System;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using EurekaTrackerAutoPopper.Resources;
using ImGuiNET;

namespace EurekaTrackerAutoPopper.Windows.MainWindow;

public partial class MainWindow
{
    private void OccultCategory()
    {
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderOccult}##OccultCategory");
        if (!tabItem.Success)
            return;

        using var tabBar = ImRaii.TabBar("OccultTabs");
        if (!tabBar.Success)
            return;

        TabNotification();

        TabMap();

        TabPot();

        TabHelper();
    }

    private void TabNotification()
    {
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderNotification}##NotificationTab");
        if (!tabItem.Success)
            return;

        var changed = false;

        ImGuiHelpers.ScaledDummy(5);

        ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderProximityNotification);

        using var indent = ImRaii.PushIndent(10.0f);
        changed |= ImGui.Checkbox(Language.ConfigOptionClearMemory, ref Plugin.Configuration.ClearMemory);
        ImGuiComponents.HelpMarker(Language.ConfigTooltipClearMemory);

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 4);
        if (ImGui.InputInt(Language.ConfigOptionMemoryTimer, ref Plugin.Configuration.ClearAfterSeconds, 10))
        {
            Plugin.Configuration.ClearAfterSeconds = Math.Clamp(Plugin.Configuration.ClearAfterSeconds, 60, 600);
            changed = true;
        }

        ImGui.Columns(2, "ProximityColumns", false);

        ImGui.TextColored(ImGuiColors.DalamudOrange, Language.ConfigHeaderTreasure);
        changed |= ImGui.Checkbox($"{Language.ConfigOptionEcho}##EchoTreasure", ref Plugin.Configuration.EchoTreasure);
        changed |= ImGui.Checkbox($"{Language.ConfigOptionToast}##ToastTreasure", ref Plugin.Configuration.ShowTreasureToast);
        changed |= ImGui.Checkbox($"{Language.ConfigOptionFlag}##FlagTreasure", ref Plugin.Configuration.PlaceTreasureFlag);

        ImGui.NextColumn();

        ImGui.TextColored(ImGuiColors.DalamudOrange, Language.ConfigHeaderBunnyCarrot);
        changed |= ImGui.Checkbox($"{Language.ConfigOptionEcho}##EchoBunnyCarrot", ref Plugin.Configuration.EchoBunnyCarrot);
        changed |= ImGui.Checkbox($"{Language.ConfigOptionToast}##ToastBunnyCarrot", ref Plugin.Configuration.ShowBunnyCarrotToast);
        changed |= ImGui.Checkbox($"{Language.ConfigOptionFlag}##FlagBunnyCarrot", ref Plugin.Configuration.PlaceBunnyCarrotFlag);

        ImGui.Columns(1);

        if (changed)
            Plugin.Configuration.Save();
    }

    private void TabMap()
    {
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderMap}##MapTab");
        if (!tabItem.Success)
            return;

        var buttonHeight = Helper.CalculateChildHeight();
        using (var contentChild = ImRaii.Child("Content", new Vector2(0, -buttonHeight)))
        {
            if (contentChild.Success)
            {
                var changed = false;

                ImGuiHelpers.ScaledDummy(5);

                ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderMarkerSetHeader);

                using var indent = ImRaii.PushIndent(10.0f);
                changed |= ImGui.Checkbox(Language.ConfigOptionPlaceDefault, ref Plugin.Configuration.PlaceDefaultOccult);
                if (Plugin.Configuration.PlaceDefaultOccult)
                {
                    using var combo = ImRaii.Combo("##DefaultMarkerSetCombo", Plugin.Configuration.DefaultOccultMarkerSets.ToName());
                    if (combo.Success)
                    {
                        foreach (var set in EnumExtensions.OccultSetArray)
                        {
                            if (!ImGui.Selectable(set.ToName(), set == Plugin.Configuration.DefaultOccultMarkerSets))
                                continue;

                            changed = true;
                            Plugin.Configuration.DefaultOccultMarkerSets = set;
                        }
                    }
                }

                changed |= ImGui.Checkbox(Language.ConfigOptionFastSwitcher, ref Plugin.Configuration.ShowFastSwitcher);
                if (Plugin.Configuration.ShowFastSwitcher)
                {
                    using (ImRaii.PushIndent(10.0f))
                        changed |= ImGui.Checkbox(Language.ConfigOptionSwitcherPosition, ref Plugin.Configuration.SwitcherBelowMap);
                }

                if (changed)
                    Plugin.Configuration.Save();
            }
        }

        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(Helper.SeparatorPadding);

        using (var bottomChild = ImRaii.Child("Bottom", Vector2.Zero))
        {
            if (bottomChild.Success)
            {
                Plugin.FastSwitchOverlay.DrawMapSetSwitcher();

                ImGui.SameLine();

                using (ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.DPSRed))
                {
                    if (ImGui.Button(Language.ConfigButtonRemoveMapMarkers))
                        Plugin.RemoveMapMarker();
                }
            }
        }
    }

    private void TabPot()
    {
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderPot}##PotTab");
        if (!tabItem.Success)
            return;

        var buttonHeight = Helper.CalculateChildHeight();
        using (var contentChild = ImRaii.Child("Content", new Vector2(0, -buttonHeight)))
        {
            if (contentChild.Success)
            {
                var changed = false;

                ImGuiHelpers.ScaledDummy(5);

                ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderPotHelperHeader);

                using var indent = ImRaii.PushIndent(10.0f);
                if (ImGui.Checkbox(Language.ConfigOptionFateWindow, ref Plugin.Configuration.ShowBunnyWindow))
                {
                    changed = true;
                    if (Plugin.Configuration.ShowBunnyWindow && Plugin.PlayerInOccultTerritory())
                        Plugin.BunnyWindow.IsOpen = true;
                }
                ImGuiComponents.HelpMarker(Language.ConfigTooltipBunnyWindow);
                changed |= Helper.AddSoundOption(Language.ConfigOptionSpawnNotification, ref Plugin.Configuration.PlayBunnyEffect, ref Plugin.Configuration.BunnySoundEffect);

                changed |= ImGui.Checkbox(Language.ConfigOptionDrawCircle, ref Plugin.Configuration.BunnyCircleDraw);
                ImGui.SameLine();
                var circleColor = Plugin.Configuration.CircleColor;
                ImGui.ColorEdit4("##circleColorPicker", ref circleColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha);
                ImGuiComponents.HelpMarker(Language.ConfigTooltipDrawCircle);
                if (circleColor != Plugin.Configuration.CircleColor)
                {
                    changed = true;
                    Plugin.Configuration.CircleColor = circleColor with {W = 1}; // fix alpha
                }

                if (changed)
                    Plugin.Configuration.Save();
            }
        }

        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(Helper.SeparatorPadding);

        using (var bottomChild = ImRaii.Child("Bottom", Vector2.Zero))
        {
            if (bottomChild.Success)
            {
                if (ImGui.Button(Language.ConfigButtonCirclePreview))
                    Plugin.EnablePreview();
            }
        }
    }

    private void TabHelper()
    {
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderHelper}##HelperTab");
        if (!tabItem.Success)
            return;

        var changed = false;

        ImGuiHelpers.ScaledDummy(5);

        ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderGeneral);
        using (ImRaii.PushIndent(10.0f))
            changed |= ImGui.Checkbox(Language.ConfigOptionEngagementsHide, ref Plugin.Configuration.EngagementsHideInEncounter);

        ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderEngagements);
        using (ImRaii.PushIndent(10.0f))
        {
            changed |= ImGui.Checkbox(Language.ConfigOptionEngagementsPot, ref Plugin.Configuration.EngagementsShowPot);

            changed |= Helper.AddSoundOption(Language.ConfigOptionSoundFateSpawn, ref Plugin.Configuration.PlayFateEffect, ref Plugin.Configuration.FateSoundEffect);
            changed |= Helper.AddSoundOption(Language.ConfigOptionSoundCESpawn, ref Plugin.Configuration.PlayEncounterEffect, ref Plugin.Configuration.EncounterSoundEffect);
        }

        ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderTower);
        using (ImRaii.PushIndent(10.0f))
        {
            changed |= ImGui.Checkbox(Language.ConfigOptionTowerTabName, ref Plugin.Configuration.TowerChangeHeader);
            ImGuiComponents.HelpMarker(Language.ConfigOptionTowerTabTooltip);

            changed |= Helper.AddSoundOption(Language.ConfigOptionSpawnNotification, ref Plugin.Configuration.PlayTowerEffect, ref Plugin.Configuration.TowerSoundEffect);
        }


        if (changed)
            Plugin.Configuration.Save();
    }
}