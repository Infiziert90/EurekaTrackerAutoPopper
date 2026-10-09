using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using EurekaTrackerAutoPopper.Resources;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Dalamud.Bindings.ImGui;
using EurekaTrackerAutoPopper.Data;

namespace EurekaTrackerAutoPopper.Windows.BozjaWindow;

public class BozjaWindow : Window, IDisposable
{
    private const int TowerSpawnTimer = 3600;

    private readonly Plugin Plugin;

    public BozjaWindow(Plugin plugin) : base("Bozja Helper##EurekaLinker")
    {
        Flags = ImGuiWindowFlags.NoScrollbar;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 340),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    public override bool DrawConditions()
    {
        if (!Plugin.Configuration.EngagementsHideInEncounter)
            return true;

        // Do not draw if a player is inside critical encounter
        return !Plugin.IsInCriticalEncounter();
    }

    public override void Draw()
    {
        using var tabBar = ImRaii.TabBar("BozjaTabs");
        if (!tabBar.Success)
            return;

        TabEngagements();

        TabTower();

        TabTracker();
    }

    private void TabEngagements()
    {
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderEngagements}##EngagementTab");
        if (!tabItem.Success)
            return;

        Helper.TextColored(ImGuiColors.DalamudOrange, Language.HeaderActiveCE);
        foreach (var criticalEncounter in Plugin.Fates.GetCEWithoutSpecial().Where(f => f.Alive))
            DrawFateInfo(criticalEncounter, true);

        DrawSeparator();

        Helper.TextColored(ImGuiColors.DalamudOrange, Language.HeaderActiveFate);
        foreach (var fate in Plugin.Fates.GetFatesForTerritory().Where(f => f.Alive))
            DrawFateInfo(fate, true);

        if (ImGui.CollapsingHeader(Language.CollapseablePreviousEngagements))
        {
            using var child = ImRaii.Child("ListChild");
            if (!child.Success)
                return;

            Helper.TextColored(ImGuiColors.DalamudOrange, Language.HeaderCE);
            foreach (var previousCE in Plugin.Fates.GetCEsSkipExtremeForTerritory().Where(f => f.MapIcon != 0))
            {
                DrawFateInfo(previousCE, false);
                DrawSeparator();
            }

            Helper.TextColored(ImGuiColors.DalamudOrange, Language.HeaderFates);
            foreach (var previousFate in Plugin.Fates.GetFatesForTerritory().Where(f => f.MapIcon != 0))
            {
                DrawFateInfo(previousFate, false);
                DrawSeparator();
            }
        }

        ImGuiHelpers.ScaledDummy(5.0f);
    }

    private void TabTower()
    {
        var towerEngagement = Plugin.Fates.GetNormalTowerForTerritory();
        using var tabItem = ImRaii.TabItem($"{towerEngagement.Name}{CheckTowerActivity()}###TowerTab");
        if (!tabItem.Success)
            return;

        if (towerEngagement.SpawnTime > 0)
            DrawFateInfo(towerEngagement, false, true);
        else
            Helper.TextColored(ImGuiColors.DalamudOrange, Language.ForkedTowerNotSeen);

        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.CollapsingHeader("Spawn Prediction"))
        {
            if (towerEngagement.Alive)
            {
                Helper.TextColored(ImGuiColors.HealerGreen, "Forked Tower is already active.");
            }
            else
            {
                var lastSpawn = towerEngagement.LastSeenAlive;
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var spawnTimer = TowerSpawnTimer - 300 * towerEngagement.KilledCEs - 60 * towerEngagement.KilledFates;
                if (towerEngagement.LastSeenAlive == -1)
                {
                    lastSpawn = towerEngagement.InstanceJoinedTimer;
                    Helper.TextColored(ImGuiColors.DalamudOrange, "This may not be correct!!!");
                }

                var timer = Utils.TimeToClockFormat(TimeSpan.FromSeconds(lastSpawn - currentTime + spawnTimer));
                Helper.TextColored(ImGuiColors.HealerGreen, $"Predicted Respawn: {timer}");

                var activeFate = Plugin.Fates.GetFatesForTerritory().FirstOrDefault(f => f.Alive);
                var activeCE =  Plugin.Fates.GetCEForTerritory().FirstOrDefault(f => f.Alive);
                var activeBunny =  Plugin.Fates.GetBunnyForTerritory().FirstOrDefault(f => f.Alive);

                Helper.TextColored(ImGuiColors.HealerGreen, "Upcoming Reductions:");
                if (activeFate != null)
                    Helper.TextColored(ImGuiColors.TankBlue, $"-1 Minute [{activeFate.Name} - {activeFate.Progress}%]");

                if (activeBunny != null)
                    Helper.TextColored(ImGuiColors.TankBlue, $"-1 Minute [{activeBunny.Name} - {activeBunny.Progress}%]");

                if (activeCE != null)
                    Helper.TextColored(ImGuiColors.TankBlue, $"-5 Minute [{activeCE.Name} - {activeCE.Progress}%]");
            }
        }
    }

    private void TabTracker()
    {
        using var tabItem = ImRaii.TabItem("Tracker###TrackerTab");
        if (!tabItem.Success)
            return;

        if (!Plugin.Configuration.UploadPermission)
        {
            Helper.TextColored(ImGuiColors.DalamudOrange, Language.TrackerError);
            return;
        }

        var width = ImGui.CalcTextSize("Tracker ID: ").X + 20.0f * ImGuiHelpers.GlobalScale;
        ImGui.AlignTextToFramePadding();
        Helper.TextColored(ImGuiColors.HealerGreen, "Server ID: ");
        ImGui.SameLine(width);
        ImGui.Text($"{Plugin.HookManager.ServerId} (Experimental)");

        if (Plugin.TrackerHandler.CurrentTracker == null || !Plugin.TrackerHandler.IsConnected)
        {
            if (Plugin.Fates.GetFatesForTerritory().Any(f => f.Alive))
                Helper.CenterText(Language.TrackerSearch);
            else
                Helper.CenterText(Language.TrackerSearchAgain);

            return;
        }

        ImGui.AlignTextToFramePadding();
        Helper.TextColored(ImGuiColors.HealerGreen, "Tracker ID: ");
        ImGui.SameLine(width);
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        ImGui.InputText("##trackerIdInput", ref Plugin.TrackerHandler.ConnectedTo, 100, ImGuiInputTextFlags.ReadOnly);

        ImGui.SameLine();

        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            if (ImGui.Button(FontAwesomeIcon.Clipboard.ToIconString()))
                ImGui.SetClipboardText(Plugin.TrackerHandler.ConnectedTo);
        }

        if (ImGui.IsItemHovered())
            Helper.Tooltip(Language.TrackerCopy);

        ImGui.SameLine();

        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            if (ImGui.Button(FontAwesomeIcon.Globe.ToIconString()))
                Util.OpenLink($"https://tracker.xivstats.com/{Plugin.TrackerHandler.ConnectedTo}");
        }

        if (ImGui.IsItemHovered())
            Helper.Tooltip(Language.TrackerOpenLink);

        using var table = ImRaii.Table("trackerTable", 6, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerH |
                                                          ImGuiTableFlags.BordersV | ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.ScrollY |
                                                          ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable |
                                                          ImGuiTableFlags.SortTristate);
        if (!table.Success)
            return;

        ImGui.TableSetupColumn(Language.TrackerEncounter, ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn(Language.TrackerTrigger, ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort);
        ImGui.TableSetupColumn(Language.TrackerKills);
        ImGui.TableSetupColumn(Language.TrackerDrops, ImGuiTableColumnFlags.NoSort);
        ImGui.TableSetupColumn(Language.TrackerPop);
        ImGui.TableSetupColumn(Language.TrackerLast);

        ImGui.TableHeadersRow();

        DrawTracker();
    }

    private void DrawFateInfo(Fate fate, bool isCurrent, bool isTower = false)
    {
        var iconTexture = Plugin.TextureManager.GetFromGameIcon(new GameIconLookup(fate.MapIcon)).GetWrapOrDefault();
        if (iconTexture == null)
            return;

        using var table = ImRaii.Table($"FateInfoTable##{fate.FateId}{isCurrent}", 2, ImGuiTableFlags.BordersInnerV);
        if (!table.Success)
            return;

        ImGui.TableSetupColumn("##info", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X / 1.6f);
        ImGui.TableSetupColumn("##extra");

        ImGui.TableNextColumn();

        var pos = ImGui.GetCursorPos();
        ImGui.Image(iconTexture.Handle, iconTexture.Size * ImGuiHelpers.GlobalScale);
        var afterPos = ImGui.GetCursorPos();

        var widthOffset = pos.X + iconTexture.Width * ImGuiHelpers.GlobalScale + 5.0f * ImGuiHelpers.GlobalScale;
        var lineHeightWithSpacing = ImGui.GetTextLineHeightWithSpacing();
        var heightOffset = pos.Y + iconTexture.Height * ImGuiHelpers.GlobalScale - lineHeightWithSpacing * 3;

        DrawOffsetText(new Vector2(widthOffset, heightOffset), ImGuiColors.DalamudWhite, fate.Name);

        if (ImGui.IsItemHovered())
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        if (ImGui.IsItemClicked())
            Plugin.OpenMap(fate.MapDataLink);

        string state, time;
        if (fate.State == DynamicEventState.Inactive)
        {
            time = Utils.TimeToClockFormat(TimeSpan.FromSeconds(fate.TimeLeft));
            state = Language.FateTimeRemaining;
        }
        else
        {
            time = Utils.TimeToClockFormat(TimeSpan.FromSeconds(fate.StateTimeLeft));
            state = fate.State.ToName();
        }

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (isCurrent)
        {
            heightOffset += lineHeightWithSpacing;
            DrawOffsetText(new Vector2(widthOffset, heightOffset), ImGuiColors.HealerGreen, $"{state}: {time}");

            heightOffset += lineHeightWithSpacing;
            DrawOffsetText(new Vector2(widthOffset, heightOffset), ImGuiColors.HealerGreen, $"{Language.FateProgress}: {fate.Progress}%");
        }
        else if (isTower)
        {
            var extraText = string.Empty;
            if (fate.State == DynamicEventState.Register)
                extraText = $": {Utils.TimeToClockFormat(TimeSpan.FromSeconds(fate.SpawnTime + 300 - currentTime))}";

            heightOffset += lineHeightWithSpacing;
            if (fate.State == DynamicEventState.Inactive)
            {
                var text = fate.LastSeenAlive > 0
                    ? Language.FateInfoLastSeen.Format(Utils.TimeToClockFormat(TimeSpan.FromSeconds(currentTime - fate.LastSeenAlive)))
                    : Language.FateInfoLastSeenUnknown;

                DrawOffsetText(new Vector2(widthOffset, heightOffset), ImGuiColors.HealerGreen, text);
            }
            else
            {
                DrawOffsetText(new Vector2(widthOffset, heightOffset), ImGuiColors.HealerGreen, $"{state}{extraText}");
            }
        }
        else
        {
            var text = fate.LastSeenAlive > 0
                ? Language.FateInfoLastSeen.Format(Utils.TimeToClockFormat(TimeSpan.FromSeconds(currentTime - fate.LastSeenAlive)))
                : Language.FateInfoLastSeenUnknown;

            heightOffset += lineHeightWithSpacing;
            DrawOffsetText(new Vector2(widthOffset, heightOffset), ImGuiColors.HealerGreen, text);
        }

        heightOffset += lineHeightWithSpacing;
        ImGui.SetCursorPos(new Vector2(widthOffset, heightOffset));
        foreach (var (itemId, idx) in fate.SpecialRewards.Select((val, i) => (val, i)))
        {
            var item = Sheets.GetItem(itemId);
            var itemIcon = Plugin.TextureManager.GetFromGameIcon(new GameIconLookup(item.Icon)).GetWrapOrDefault();
            if (itemIcon == null)
                continue;

            ImGui.Image(itemIcon.Handle, ImGuiHelpers.ScaledVector2(24, 24));
            if (ImGui.IsItemHovered())
                Helper.Tooltip(item.Name.ToString());

            if (idx + 1 !=  fate.SpecialRewards.Length)
                ImGui.SameLine();
        }

        ImGui.SetCursorPos(afterPos);

        ImGui.TableNextColumn();

        ImGui.Text("");
        Helper.TextColored(ImGuiColors.HealerGreen, fate.Aetheryte.ToName());
        Helper.TextColored(ImGuiColors.HealerGreen, Language.FateInfoWalkingTime.Format(Utils.TimeToClockFormat(TimeSpan.FromSeconds(fate.WalkingDistance))));
    }

    private void DrawOffsetText(Vector2 offset, Vector4 color, string text)
    {
        ImGui.SetCursorPos(offset);
        Helper.WrappedTextWithColor(color, text);
    }

    private void DrawSeparator()
    {
        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);
    }

    private string CheckTowerActivity()
    {
        if (!Plugin.Configuration.TowerChangeHeader)
            return string.Empty;

        var towerEncounter = Plugin.Fates.GetNormalTowerForTerritory();
        if (towerEncounter.State == DynamicEventState.Inactive)
            return string.Empty;

        return Language.OccultTowerActiveIndicator;
    }

    // Inspired by https://github.com/KangasZ/EurekaHelper/blob/main/EurekaHelper/Windows/PluginWindow.cs
    private void DrawTracker()
    {
        var zoneFates = Plugin.Fates.GetCEWithoutSpecial().ToArray();
        var minRowHeight = ImGui.GetContentRegionAvail().Y / zoneFates.Length;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var sortSpecs = ImGui.TableGetSortSpecs();
        if (sortSpecs.SpecsDirty)
        {
            var specsCount = sortSpecs.SpecsCount;
            if (specsCount > 0)
            {
                switch (sortSpecs.Specs.ColumnIndex, sortSpecs.Specs.SortDirection)
                {
                    case (0, ImGuiSortDirection.Ascending):
                        zoneFates = zoneFates.OrderBy(x => x.Name).ToArray();
                        break;
                    case (0, ImGuiSortDirection.Descending):
                        zoneFates = zoneFates.OrderByDescending(x => x.Name).ToArray();
                        break;
                    case (2, ImGuiSortDirection.Ascending):
                        zoneFates = zoneFates.OrderBy(x => x.TriggerKills).ToArray();
                        break;
                    case (2, ImGuiSortDirection.Descending):
                        zoneFates = zoneFates.OrderByDescending(x => x.TriggerKills).ToArray();
                        break;
                    case (4, ImGuiSortDirection.Ascending):
                        zoneFates = zoneFates.OrderBy(x => x.DeathTime + (x.TriggeredBy != 0 ? 3600 : 7200)).ToArray();
                        break;
                    case (4, ImGuiSortDirection.Descending):
                        zoneFates = zoneFates.OrderByDescending(x => x.DeathTime + (x.TriggeredBy != 0 ? 3600 : 7200)).ToArray();
                        break;
                    case (5, ImGuiSortDirection.Ascending):
                        zoneFates = zoneFates.OrderBy(x => x.LastSeenAlive).ToArray();
                        break;
                    case (5, ImGuiSortDirection.Descending):
                        zoneFates = zoneFates.OrderByDescending(x => x.LastSeenAlive).ToArray();
                        break;
                }
            }
        }

        foreach (var fate in zoneFates)
        {
            ImGui.TableNextRow(ImGuiTableRowFlags.None, minRowHeight);
            if (fate.Alive)
            {
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiColors.HealerGreen with {W = 0.3f}));
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, ImGui.GetColorU32(ImGuiColors.HealerGreen with {W = 0.3f}));
            }

            ImGui.TableNextColumn();
            if (fate.Weakness != Weakness.None)
            {
                var weaknessIcon = Plugin.TextureManager.GetFromGameIcon(new GameIconLookup((uint)fate.Weakness)).GetWrapOrEmpty();
                ImGui.Image(weaknessIcon.Handle, ImGuiHelpers.ScaledVector2(14, 20));
                if (ImGui.IsItemHovered())
                    Helper.Tooltip(fate.Weakness.ToName());

                ImGui.SameLine();
            }

            ImGui.Text(fate.Name);

            if (ImGui.IsItemHovered())
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

            if (ImGui.IsItemClicked())
                Plugin.OpenMap(fate.MapDataLink);

            ImGui.TableNextColumn();
            ImGui.Text(fate.TriggerName);

            ImGui.TableNextColumn();
            Helper.RightTextColored(ImGuiColors.TankBlue, fate.TriggerKills > 0 ? fate.TriggerKills.ToString() : " ");

            ImGui.TableNextColumn();
            using (ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero))
            {
                foreach (var (itemId, idx) in fate.SpecialRewards.Select((val, i) => (val, i)))
                {
                    var item = Sheets.GetItem(itemId);
                    var itemIcon = Plugin.TextureManager.GetFromGameIcon(new GameIconLookup(item.Icon)).GetWrapOrDefault();
                    if (itemIcon == null)
                        continue;

                    ImGui.Image(itemIcon.Handle, ImGuiHelpers.ScaledVector2(20, 20));
                    if (ImGui.IsItemHovered())
                        Helper.Tooltip(item.Name.ToString());

                    if (idx + 1 != fate.SpecialRewards.Length)
                        ImGui.SameLine();
                }
            }

            ImGui.TableNextColumn();
            if (fate.Alive)
            {
                if (fate.State == DynamicEventState.Battle)
                    Helper.RightText(Language.TrackerBattle);
                else if (fate.State == DynamicEventState.Warmup)
                    Helper.RightText(Language.TrackerStarting);
                else if (fate.State == DynamicEventState.Register)
                    Helper.RightText(Language.TrackerRecruiting);
            }
            else if (fate.DeathTime == 0)
            {
                Helper.RightTextColored(ImGuiColors.HealerGreen, Language.TrackerCanPop);
            }
            else
            {
                var respawnTimer = fate.TriggeredBy != 0 ? 3600 : 7200;
                if (fate.DeathTime + respawnTimer < currentTime)
                    Helper.RightTextColored(ImGuiColors.HealerGreen,  Language.TrackerCanPop);
                else
                    Helper.RightText(Utils.TimeToClockFormat(TimeSpan.FromSeconds(fate.DeathTime + respawnTimer - currentTime)));
            }

            ImGui.TableNextColumn();
            if (fate.LastSeenAlive > 0)
            {
                if (fate.Alive)
                {
                    if (fate.State == DynamicEventState.Battle)
                        Helper.RightText($"{fate.Progress}%");
                    else
                        Helper.RightText(Utils.TimeToClockFormat(TimeSpan.FromSeconds(fate.StateTimeLeft)));
                }
                else
                {
                    Helper.RightText(Utils.TimeToClockFormat(TimeSpan.FromSeconds(currentTime - fate.LastSeenAlive)));
                }
            }
            else
            {
                Helper.RightText(Language.TrackerNA);
            }
        }
    }
}
