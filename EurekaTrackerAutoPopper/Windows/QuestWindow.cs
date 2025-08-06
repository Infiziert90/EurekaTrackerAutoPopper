using System;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using EurekaTrackerAutoPopper.Resources;
using Dalamud.Bindings.ImGui;

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
        var territoryId = QuestHelper.TerritoryTypeToSafeId(Plugin.ClientState.TerritoryType);

        ImGui.TextColored(ImGuiColors.ParsedGold, $"{Language.QuestLogHeader}: ");
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.ParsedOrange, QuestHelper.TerritoryToPlaceName(territoryId));

        ImGuiHelpers.ScaledDummy(5);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5);

        using var tabBar = ImRaii.TabBar("QuestTabs");
        if (!tabBar.Success)
            return;

        foreach (var quest in QuestHelper.TerritoryToQuests(territoryId))
        {
            using var tabItem = ImRaii.TabItem($"{Language.QuestLogTabItemQuestLevel} {quest}");
            if (!tabItem.Success)
                continue;

            QuestHelper.Quests(territoryId, quest);
        }
    }
}