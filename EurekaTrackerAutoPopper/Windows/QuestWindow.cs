using System;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using EurekaTrackerAutoPopper.Resources;
using ImGuiNET;

namespace EurekaTrackerAutoPopper.Windows;

public class QuestWindow : Window, IDisposable
{
    public QuestWindow() : base("Quest Guide##EurekaLinker")
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

        if (!Library.TerritoryToMap.ContainsKey(territoryId))
        {
            ImGui.TextColored(ImGuiColors.ParsedGold, $"{Language.QuestLogHeader}: ");
            ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.ParsedOrange, $"{Language.QuestLogOutside}");
            return;
        }

        ImGui.TextColored(ImGuiColors.ParsedGold, $"{Language.QuestLogHeader}: ");
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
        if (ImGui.BeginTabItem($"{Language.QuestLogTabItemQuestLevel} {quest}"))
        {
            QuestHelper.Quests(territoryId, quest);
            ImGui.EndTabItem();
        }
    }
}