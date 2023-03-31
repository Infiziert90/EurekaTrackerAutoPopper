using System;
using System.Numerics;
using CheapLoc;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace EurekaTrackerAutoPopper.Windows;

public class QuestWindow : Window, IDisposable
{
    public QuestWindow() : base("Quest Guide")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(370, 570),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        var territoryId = Plugin.ClientState.TerritoryType;
        // If territoryId not in eureka, set anemos for preview
        if (!Library.TerritoryToMap.ContainsKey(territoryId))
            territoryId = 732;

        ImGui.TextColored(ImGuiColors.ParsedGold, $"{Loc.Localize("QuestLog - Header", "Current Area")}: ");
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.ParsedOrange, QuestHelper.TerritoryToPlaceName(territoryId));

        ImGuiHelpers.ScaledDummy(5);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5);

        if (ImGui.BeginTabBar("##quest-tabs"))
        {
            foreach (var quest in QuestHelper.TerritoryToQuests(territoryId))
            {
                TabItem(territoryId, quest);
            }

            ImGui.EndTabBar();
        }
    }

    private void TabItem(uint territoryId, uint quest)
    {
        if (ImGui.BeginTabItem($"{Loc.Localize("QuestLog - Tab Item Quest Level", "Level")} {quest}"))
        {
            QuestHelper.Quests(territoryId, quest);
            ImGui.EndTabItem();
        }
    }
}