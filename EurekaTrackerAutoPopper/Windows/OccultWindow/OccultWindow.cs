using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
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
using ImGuiNET;

namespace EurekaTrackerAutoPopper.Windows.OccultWindow;

public class OccultWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    public OccultWindow(Plugin plugin) : base("Occult Helper##EurekaLinker")
    {
        Flags = ImGuiWindowFlags.NoScrollbar;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 340),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
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
    }

    private void TabEngagements()
    {
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderEngagements}##EngagementTab");
        if (!tabItem.Success)
            return;

        Helper.TextColored(ImGuiColors.DalamudOrange, Language.HeaderActiveCE);
        if (Plugin.Fates.OccultCriticalEncounters.SkipLast(1).FirstOrDefault(f => f.Alive) is { } criticalEncounter)
            DrawFateInfo(criticalEncounter, true);

        DrawSeparator();

        Helper.TextColored(ImGuiColors.DalamudOrange, Language.HeaderActiveFate);
        if (Plugin.Fates.OccultFates.FirstOrDefault(f => f.Alive) is {} fate)
            DrawFateInfo(fate, true);

        DrawSeparator();

        if (Plugin.Configuration.EngagementsShowPot)
        {
            Helper.TextColored(ImGuiColors.DalamudOrange, Language.HeaderActivePot);
            if (Plugin.Fates.BunnyFates.FirstOrDefault(f => f.Alive) is { } potFate)
                DrawFateInfo(potFate, true);

            DrawSeparator();
        }

        if (ImGui.CollapsingHeader(Language.CollapseablePreviousEngagements))
        {
            using var child = ImRaii.Child("ListChild");
            if (!child.Success)
                return;

            Helper.TextColored(ImGuiColors.DalamudOrange, Language.HeaderCE);
            foreach (var previousCE in Plugin.Fates.OccultCriticalEncounters.Where(f => f.MapIcon != 0))
            {
                DrawFateInfo(previousCE, false);
                DrawSeparator();
            }

            Helper.TextColored(ImGuiColors.DalamudOrange, Language.HeaderFates);
            foreach (var previousFate in Plugin.Fates.OccultFates.Where(f => f.MapIcon != 0))
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

        var towerEngagement = Plugin.Fates.OccultCriticalEncounters[^1];
        if (towerEngagement.SpawnTime > 0)
            DrawFateInfo(towerEngagement, false, true);
        else
            Helper.TextColored(ImGuiColors.DalamudOrange, Language.ForkedTowerNotSeen);

        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        var local = Plugin.ClientState.LocalPlayer;
        if (local == null)
            return;

        if (Utils.Distance(towerEngagement.WorldPos, local.Position) > 20.0f)
        {
            Helper.TextColored(ImGuiColors.DalamudOrange, Language.ForkedTowerNotOnPlatform);
        }
        else
        {
            var playersClose = Plugin.ObjectTable
                .Where(o => o.ObjectKind == ObjectKind.Player)
                .Where(o => Utils.Distance(towerEngagement.WorldPos, o.Position) <= 20.0f)
                .ToArray();

            Helper.TextColored(ImGuiColors.HealerGreen, Language.ForkedTowerInfoPlayerCount.Format(playersClose.Length));
            if (ImGui.CollapsingHeader(Language.ForkedTowerInfoPlayerListCollapseable))
            {
                var length = Math.Clamp(playersClose.Length, 2, 10);
                using var child = ImRaii.Child("PlayerListChild", new Vector2(0, ImGui.GetTextLineHeightWithSpacing() * length), true);
                if (child.Success)
                {
                    foreach (var player in playersClose.Skip(1).Cast<IPlayerCharacter>())
                        ImGui.TextUnformatted($"{player.Name.TextValue}@{player.HomeWorld.Value.Name.ExtractText()}");
                }
            }
        }

        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.CollapsingHeader(Language.ForkedTowerInfoJoinRun))
        {
            Helper.WrappedTextWithColor(ImGuiColors.DalamudOrange, Language.ForkedTowerInfoWarning1);
            ImGuiHelpers.ScaledDummy(5.0f);
            Helper.WrappedTextWithColor(ImGuiColors.DalamudOrange, Language.ForkedTowerInfoWarning2);

            ImGuiHelpers.ScaledDummy(10.0f);

            Helper.WrappedTextWithColor(ImGuiColors.HealerGreen, Language.ForkedTowerInfoJoinDiscordList);
            Helper.WrappedTextWithColor(ImGuiColors.HealerGreen, "EU:");

            Helper.BulletLink("CBT", "https://discord.gg/8HUKDA745x");
            Helper.BulletLink("Savage Slimes", "https://discord.gg/SavageSlimes");
            Helper.BulletLink("Lunar Forays Group", "https://discord.gg/d5gNTMmqbp");
            Helper.BulletLink("Late Night", "https://discord.gg/28SRRADTK3");
            Helper.BulletLink("Chaos Hour", "https://discord.gg/eE7vkX73");

            ImGuiHelpers.ScaledDummy(10.0f);

            Helper.WrappedTextWithColor(ImGuiColors.HealerGreen, "NA:");
            Helper.BulletLink("FOE", "https://discord.gg/foexiv");
            Helper.BulletLink("ABBA", "https://discord.gg/abbaffxiv");
            Helper.BulletLink("CAFE", "https://discord.gg/c-a-f-e");
            Helper.BulletLink("CEM", "https://discord.gg/cem");
            Helper.BulletLink("DFO", "https://discord.gg/vjwYEeubeN");
            Helper.BulletLink("The Help Lines", "https://discord.gg/thehelplines");
        }

    }

    private void DrawFateInfo(Fate fate, bool isCurrent, bool isTower = false, bool showSpawnTimer = false)
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
        ImGui.Image(iconTexture.ImGuiHandle, iconTexture.Size * ImGuiHelpers.GlobalScale);
        var afterPos = ImGui.GetCursorPos();

        var lineHeightWithSpacing = ImGui.GetTextLineHeightWithSpacing();
        var widthOffset = pos.X + iconTexture.Width * ImGuiHelpers.GlobalScale + 5.0f * ImGuiHelpers.GlobalScale;
        var heightOffset = pos.Y + iconTexture.Height * ImGuiHelpers.GlobalScale - (lineHeightWithSpacing * 3);

        DrawOffsetText(new Vector2(widthOffset, heightOffset), ImGuiColors.DalamudWhite, fate.Name);
        ImGui.SameLine();
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            if (ImGui.Selectable($"{FontAwesomeIcon.Flag.ToIconString()}##{fate.FateId}"))
                Plugin.OpenMap(fate.MapLinkPayload);
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

        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
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

        if (showSpawnTimer && fate.PreviousRespawnTimes.Count > 0)
        {
            var convertedTimes = fate.PreviousRespawnTimes.Select(t => Utils.TimeToClockFormat(TimeSpan.FromSeconds(t)));

            heightOffset += lineHeightWithSpacing;
            DrawOffsetText(new Vector2(widthOffset, heightOffset), ImGuiColors.HealerGreen, Language.FateInfoRespawnTimes.Format(string.Join(", ", convertedTimes)));
        }

        ImGui.SetCursorPos(afterPos);

        ImGui.TableNextColumn();
        var lineHeight = ImGui.GetTextLineHeight();
        foreach (var (itemId, idx) in fate.SpecialRewards.Select((val, i) => (val, i)))
        {
            var item = Sheets.GetItem(itemId);
            var itemIcon = Plugin.TextureManager.GetFromGameIcon(new GameIconLookup(item.Icon)).GetWrapOrDefault();
            if (itemIcon == null)
                continue;

            ImGui.Image(itemIcon.ImGuiHandle, new Vector2(lineHeight, lineHeight));
            if (ImGui.IsItemHovered())
                Helper.Tooltip(item.Name.ExtractText());

            if (idx + 1 !=  fate.SpecialRewards.Length)
                ImGui.SameLine();
        }

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

        var towerEncounter = Plugin.Fates.OccultCriticalEncounters[^1];
        if (towerEncounter.State == DynamicEventState.Inactive)
            return string.Empty;

        return Language.OccultTowerActiveIndicator;
    }
}