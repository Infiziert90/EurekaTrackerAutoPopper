using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
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

namespace EurekaTrackerAutoPopper.Windows.OccultWindow;

public class OccultWindow : Window, IDisposable
{
    private const int TowerSpawnTimer = 3600;

    private readonly Plugin Plugin;

    public OccultWindow(Plugin plugin) : base("Occult Helper##EurekaLinker")
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
        using var tabBar = ImRaii.TabBar("OccultTabs");
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
        if (Plugin.Fates.GetCEsWithoutTowerForTerritory().FirstOrDefault(f => f.Alive) is { } criticalEncounter)
            DrawFateInfo(criticalEncounter, true);

        DrawSeparator();

        Helper.TextColored(ImGuiColors.DalamudOrange, Language.HeaderActiveFate);
        if (Plugin.Fates.GetFatesForTerritory().FirstOrDefault(f => f.Alive) is {} fate)
            DrawFateInfo(fate, true);

        DrawSeparator();

        if (Plugin.Configuration.EngagementsShowPot)
        {
            Helper.TextColored(ImGuiColors.DalamudOrange, Language.HeaderActivePot);
            if (Plugin.Fates.GetBunnyForTerritory().FirstOrDefault(f => f.Alive) is { } potFate)
                DrawFateInfo(potFate, true);

            DrawSeparator();
        }

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
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderTower}{CheckTowerActivity()}###TowerTab");
        if (!tabItem.Success)
            return;

        var towerEngagement = Plugin.Fates.GetNormalTowerForTerritory();
        var isSouthTower = towerEngagement.FateId == 48;
        if (towerEngagement.SpawnTime > 0)
            DrawFateInfo(towerEngagement, false, true);
        else
            Helper.TextColored(ImGuiColors.DalamudOrange, Language.ForkedTowerNotSeen);

        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        var local = Plugin.ObjectTable.LocalPlayer;
        if (local == null)
            return;

        if (Utils.Distance(towerEngagement.WorldPos, local.Position) > 20.0f)
        {
            Helper.TextColored(ImGuiColors.DalamudOrange, Language.ForkedTowerNotOnPlatform);
        }
        else
        {
            var playersClose = Utils.GetTowerCharacter(towerEngagement, isSouthTower ? 20 : 40);

            Helper.TextColored(ImGuiColors.HealerGreen, Language.ForkedTowerInfoPlayerCount.Format(isSouthTower ? 20 : 40, playersClose.Length));
            if (ImGui.CollapsingHeader(Language.ForkedTowerInfoPlayerListCollapseable))
            {
                var length = Math.Clamp(playersClose.Length, 2, 10);
                using var child = ImRaii.Child("PlayerListChild", new Vector2(0, ImGui.GetTextLineHeightWithSpacing() * length), true);
                if (child.Success)
                {
                    foreach (var player in playersClose.Skip(1).Cast<IPlayerCharacter>())
                        ImGui.TextUnformatted($"{player.Name.TextValue}@{player.HomeWorld.Value.Name.ToString()}");
                }
            }
        }

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
                var activeCE =  Plugin.Fates.GetCriticalEngagementForTerritory().FirstOrDefault(f => f.Alive);
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

        if (ImGui.CollapsingHeader(Language.ForkedTowerInfoJoinRun))
        {
            Helper.WrappedTextWithColor(ImGuiColors.AttentionForeground, Language.ForkedTowerInfoNote1);
            Helper.WrappedTextWithColor(ImGuiColors.AttentionForeground, Language.ForkedTowerInfoNote2);

            ImGuiHelpers.ScaledDummy(5.0f);

            Helper.WrappedTextWithColor(ImGuiColors.DalamudOrange, Language.ForkedTowerInfoWarning1);
            ImGuiHelpers.ScaledDummy(5.0f);
            Helper.WrappedTextWithColor(ImGuiColors.DalamudOrange, Language.ForkedTowerInfoWarning2);

            ImGuiHelpers.ScaledDummy(10.0f);

            Helper.WrappedTextWithColor(ImGuiColors.HealerGreen, Language.ForkedTowerInfoJoinDiscordList);
            Helper.WrappedTextWithColor(ImGuiColors.HealerGreen, "EU:");
            Helper.BulletLink("Lunar Forays Group", "https://discord.gg/d5gNTMmqbp");
            Helper.BulletLink("Savage Slimes", "https://discord.gg/SavageSlimes");
            Helper.BulletLink("Late Night", "https://discord.gg/28SRRADTK3");
            Helper.BulletLink("A Late Night Reborn", "https://discord.gg/psuzsjEZWR");
            Helper.BulletLink("CBT", "https://discord.gg/8HUKDA745x");
            Helper.BulletLink("Students of Baldesion", "https://discord.gg/students-of-baldesion");
            Helper.BulletLink("Occult Crescent Chaos!", "https://discord.gg/k5wV3GWKzW");
            Helper.BulletLink("Double Edge", "https://discord.gg/doubleedge");
            Helper.BulletLink("Forked Tower Enjoyer Light", "https://discord.gg/forkedtower");
            Helper.BulletLink("Light Savage Lemmings (German)", "https://discord.gg/p3QwFREXJP");
            Helper.BulletLink("Conclave d'Exploration (French)", "https://discord.gg/CgSRvTEHh8");

            ImGuiHelpers.ScaledDummy(10.0f);

            Helper.WrappedTextWithColor(ImGuiColors.HealerGreen, "NA:");
            Helper.BulletLink("Field Op Enjoyer", "https://discord.gg/foexiv");
            Helper.BulletLink("ABBA+", "https://discord.gg/abbaffxiv");
            Helper.BulletLink("CAFE", "https://discord.gg/c-a-f-e");
            Helper.BulletLink("CEM", "https://discord.gg/cem");
            Helper.BulletLink("DFO", "https://discord.gg/vjwYEeubeN");
            Helper.BulletLink("The Help Lines", "https://discord.gg/thehelplines");

            ImGuiHelpers.ScaledDummy(10.0f);

            Helper.WrappedTextWithColor(ImGuiColors.HealerGreen, "OCE/JP:");
            Helper.BulletLink("Content Achievers [OCE + JP]", "https://discord.gg/FJFxr2U");
            Helper.BulletLink("Murder of Geese [OCE]", "https://discord.gg/zpGRYsZpRA");
            Helper.BulletLink("Once Upon a Fork [Elemental DC]", "https://discord.gg/GJxnnYKVHQ");
        }
    }

    private void TabTracker()
    {
        using var tabItem = ImRaii.TabItem("Tracker###TrackerTab");
        if (!tabItem.Success)
            return;

        if (!Plugin.Configuration.UploadPermission)
        {
            Helper.TextColored(ImGuiColors.DalamudOrange, "No Upload Permission Granted.");
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
                Helper.CenterText("Searching active tracker...");
            else
                Helper.CenterText("Awaiting next fate before searching again ...");

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
            Helper.Tooltip("Copy tracker instance id to clipboard.");

        ImGui.SameLine();

        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            if (ImGui.Button(FontAwesomeIcon.Globe.ToIconString()))
                Util.OpenLink($"https://tracker.xivstats.com/{Plugin.TrackerHandler.ConnectedTo}");
        }

        if (ImGui.IsItemHovered())
            Helper.Tooltip("Open tracker website.");

        using var table = ImRaii.Table("trackerTable", 7, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerH |
                                                          ImGuiTableFlags.BordersV | ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.ScrollY |
                                                          ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable |
                                                          ImGuiTableFlags.SortTristate);
        if (!table.Success)
            return;

        ImGui.TableSetupColumn("##Weakness", ImGuiTableColumnFlags.NoSort);
        ImGui.TableSetupColumn("Encounter", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn("Trigger", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort);
        ImGui.TableSetupColumn("Kills");
        ImGui.TableSetupColumn("Drops", ImGuiTableColumnFlags.NoSort);
        ImGui.TableSetupColumn("Pop Timer");
        ImGui.TableSetupColumn("Last Seen");

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
        ImGui.SameLine();
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            if (ImGui.Selectable($"{FontAwesomeIcon.Flag.ToIconString()}##{fate.FateId}"))
                Plugin.OpenMap(fate.MapDataLink);
        }

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
        var zoneFates = Plugin.Fates.GetCEsWithoutTowerForTerritory().ToArray();
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
                    case (1, ImGuiSortDirection.Ascending):
                        zoneFates = zoneFates.OrderBy(x => x.Name).ToArray();
                        break;
                    case (1, ImGuiSortDirection.Descending):
                        zoneFates = zoneFates.OrderByDescending(x => x.Name).ToArray();
                        break;
                    case (3, ImGuiSortDirection.Ascending):
                        zoneFates = zoneFates.OrderBy(x => x.TriggerKills).ToArray();
                        break;
                    case (3, ImGuiSortDirection.Descending):
                        zoneFates = zoneFates.OrderByDescending(x => x.TriggerKills).ToArray();
                        break;
                    case (5, ImGuiSortDirection.Ascending):
                        zoneFates = zoneFates.OrderBy(x => x.DeathTime + (x.TriggeredBy != 0 ? 3600 : 7200)).ToArray();
                        break;
                    case (5, ImGuiSortDirection.Descending):
                        zoneFates = zoneFates.OrderByDescending(x => x.DeathTime + (x.TriggeredBy != 0 ? 3600 : 7200)).ToArray();
                        break;
                    case (6, ImGuiSortDirection.Ascending):
                        zoneFates = zoneFates.OrderBy(x => x.LastSeenAlive).ToArray();
                        break;
                    case (6, ImGuiSortDirection.Descending):
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
            }

            ImGui.TableNextColumn();
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
                    Helper.RightText("Battle");
                else if (fate.State == DynamicEventState.Warmup)
                    Helper.RightText("Starting");
                else if (fate.State == DynamicEventState.Register)
                    Helper.RightText("Recruiting");
            }
            else if (fate.DeathTime == 0)
            {
                Helper.RightTextColored(ImGuiColors.HealerGreen, "Can pop");
            }
            else
            {
                var respawnTimer = fate.TriggeredBy != 0 ? 3600 : 7200;
                if (fate.DeathTime + respawnTimer < currentTime)
                    Helper.RightTextColored(ImGuiColors.HealerGreen, "Can pop");
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
                Helper.RightText("N/A");
            }
        }
    }
}
