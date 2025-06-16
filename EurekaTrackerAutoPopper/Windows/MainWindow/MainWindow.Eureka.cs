using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using EurekaTrackerAutoPopper.Resources;
using ImGuiNET;

namespace EurekaTrackerAutoPopper.Windows.MainWindow;

public partial class MainWindow
{
    private int DebugFate;
    public string Instance = string.Empty;
    public string Password = string.Empty;

    public void Reset()
    {
        Instance = string.Empty;
        Password = string.Empty;
    }

    private void EurekaCategory()
    {
        using var tabItem = ImRaii.TabItem("Eureka##EurekaCategory");
        if (!tabItem.Success)
            return;

        using var tabBar = ImRaii.TabBar("EurekaTabs");
        if (!tabBar.Success)
            return;

        TabGeneral();

        TabChat();

        TabFairy();

        TabBunny();

        TabStats();

#if DEBUG
        TabDebug();
#endif
    }

    private void TabGeneral()
    {
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderTracker}##TrackerTab");
        if (!tabItem.Success)
            return;

        var changed = false;

        ImGuiHelpers.ScaledDummy(5);

        ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderPop);
        using (ImRaii.PushIndent(10.0f))
        {
            changed |= ImGui.Checkbox(Language.ConfigOptionEchoNM, ref Plugin.Configuration.EchoNMPop);
            changed |= Helper.AddSoundOption(0, Language.ConfigOptionSpawnNotification, ref Plugin.Configuration.PlaySoundEffect, ref Plugin.Configuration.PopSoundEffect);
            changed |= ImGui.Checkbox(Language.ConfigOptionToastNM, ref Plugin.Configuration.ShowPopToast);
            if (Plugin.Configuration.EchoNMPop || Plugin.Configuration.ShowPopToast)
            {
                using var indent = ImRaii.PushIndent(10.0f);
                changed |= ImGui.Checkbox(Language.ConfigOptionShortNames, ref Plugin.Configuration.UseShortNames);
            }
        }

        if (changed)
            Plugin.Configuration.Save();

        ImGuiHelpers.ScaledDummy(10);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5);
        ImGui.TextUnformatted(Language.ConfigHeaderTracker);
        ImGuiHelpers.ScaledDummy(10);

        if (ImGui.InputText(Language.ConfigInputURL, ref Instance, 31))
            Instance = Instance.Length == 6 ? $"https://ffxiv-eureka.com/{Instance}" : Instance;


        if (!string.IsNullOrEmpty(Instance))
        {
            ImGui.SameLine();
            if (ImGuiComponents.IconButton(1, FontAwesomeIcon.Clipboard))
                ImGui.SetClipboardText(Instance);
        }

        ImGui.InputText(Language.ConfigInputPW, ref Password, 50);
        if (!string.IsNullOrEmpty(Password))
        {
            ImGui.SameLine();
            if (ImGuiComponents.IconButton(2, FontAwesomeIcon.Clipboard))
                ImGui.SetClipboardText(Password);
        }

        if (Plugin.PlayerInEureka && string.IsNullOrEmpty(Instance))
        {
            if (ImGui.Button(Language.ConfigButtonStartNew))
                Plugin.StartTrackerAsync();
        }
        else
        {
            using (ImRaii.Disabled())
                ImGui.Button(Language.ConfigButtonStartNew);

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                ImGui.SetTooltip(Language.ConfigTooltipStartNew);
        }

        if (Instance.Length > 0)
        {
            if (ImGuiComponents.IconButton(3, FontAwesomeIcon.Globe))
                Dalamud.Utility.Util.OpenLink(Instance);

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(Language.ConfigTooltipOpenTracker);
        }
    }

    private void TabChat()
    {
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderChat}##ChatTab");
        if (!tabItem.Success)
            return;

        var changed = false;

        ImGuiHelpers.ScaledDummy(5);

        changed |= ImGui.Checkbox(Language.ConfigOptionOpenShoutWindow, ref Plugin.Configuration.ShowPopWindow);
        changed |= ImGui.Checkbox(Language.ConfigOptionCopyShoutMessage, ref Plugin.Configuration.CopyShoutMessage);
        changed |= ImGui.Checkbox(Language.ConfigOptionRandomizeCoords, ref Plugin.Configuration.RandomizeMapCoords);
        changed |= ImGui.Checkbox(Language.ConfigOptionShowPT, ref Plugin.Configuration.ShowPullTimer);
        ImGuiComponents.HelpMarker(Language.ConfigTooltipShowPT);

        if (Plugin.Configuration.ShowPullTimer)
        {
            using var indent = ImRaii.PushIndent(10.0f);
            changed |= ImGui.Checkbox(Language.ConfigOptionUseET, ref Plugin.Configuration.UseEorzeaTimer);
            if (Plugin.Configuration.UseEorzeaTimer)
                changed |= ImGui.Checkbox(Language.ConfigOptionUse12h, ref Plugin.Configuration.UseTwelveHourFormat);
        }

        ImGuiHelpers.ScaledDummy(5);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5);
        ImGui.TextUnformatted(Language.ConfigHeaderFormat);
        ImGuiHelpers.ScaledDummy(10);

        changed |= ImGui.InputText("##input-chatformat", ref Plugin.Configuration.ChatFormat, 64);

        ImGuiHelpers.ScaledDummy(5);

        ImGui.TextUnformatted("$n = Full Name");
        ImGui.TextUnformatted("$sN = Short Name");
        ImGui.TextUnformatted("$p = MapFlag");
        ImGui.TextUnformatted("$t = Pull Timer - e.g. PT 27 / ET 13:37");

        if (changed)
        {
            Plugin.Configuration.Save();
            Plugin.Library.Initialize();
        }
    }

    private void TabFairy()
    {
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderFairy}##FairyTab");
        if (!tabItem.Success)
            return;

        var buttonHeight = Helper.CalculateChildHeight();
        using (var contentChild = ImRaii.Child("Content", new Vector2(0, -buttonHeight)))
        {
            if (contentChild.Success)
            {
                ImGuiHelpers.ScaledDummy(5);

                var changed = false;
                ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderFairy);
                ImGuiHelpers.ScaledIndent(10.0f);
                changed |= ImGui.Checkbox(Language.ConfigOptionEcho, ref Plugin.Configuration.EchoFairies);
                changed |= ImGui.Checkbox(Language.ConfigOptionToast, ref Plugin.Configuration.ShowFairyToast);
                changed |= ImGui.Checkbox(Language.ConfigOptionFlag, ref Plugin.Configuration.PlaceFairyFlag);
                ImGuiHelpers.ScaledIndent(-10.0f);

                if (changed)
                    Plugin.Configuration.Save();

                ImGuiHelpers.ScaledDummy(5);
                ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderKnownLocations);
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(5);

                using var fairyChild = ImRaii.Child("FairyTable");
                if (fairyChild.Success)
                {
                    using var table = ImRaii.Table("##ExistingFairiesTable", 2);
                    if (table.Success)
                    {
                        ImGui.TableSetupColumn("##location");
                        ImGui.TableSetupColumn("##trash", ImGuiTableColumnFlags.WidthFixed, 0.1f);

                        var delIdx = -1;
                        foreach (var (fairy, idx) in Plugin.Library.ExistingFairies.Select((val, i) => (val, i)))
                        {
                            var map = (MapLinkPayload)fairy.MapLink.Payloads.First();

                            ImGui.TableNextColumn();
                            if (ImGui.Selectable($"Fairy ({map.XCoord:0.0},  {map.YCoord:0.0})##{idx}"))
                                Plugin.OpenMap(map);

                            ImGui.TableNextColumn();
                            if (ImGuiComponents.IconButton(idx, FontAwesomeIcon.Trash))
                                delIdx = idx;
                        }

                        if (delIdx != -1)
                            Plugin.Library.ExistingFairies.RemoveAt(delIdx);
                    }
                }
            }
        }

        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(Helper.SeparatorPadding);

        using (var bottomChild = ImRaii.Child("Bottom", Vector2.Zero))
        {
            if (bottomChild.Success)
            {
                if (ImGui.Button(Language.ConfigButtonFairyAll))
                {
                    foreach (var fairy in Plugin.Library.ExistingFairies)
                        Plugin.EchoFairy(fairy);
                }

                ImGui.SameLine();

                using (ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.ParsedBlue))
                {
                    if (ImGui.Button(Language.ConfigButtonAddMapMarkers))
                        Plugin.PlaceEurekaMarkerSet(false, false);
                }

                ImGui.SameLine();

                using (ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.DPSRed))
                {
                    if (ImGui.Button(Language.ConfigButtonRemoveMapMarkers))
                        Plugin.RemoveMapMarker();
                }
            }
        }
    }

    private void TabBunny()
    {
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderBunny}##BunnyTab");
        if (!tabItem.Success)
            return;

        var buttonHeight = Helper.CalculateChildHeight();
        using (var contentChild = ImRaii.Child("Content", new Vector2(0, -buttonHeight)))
        {
            if (contentChild.Success)
            {
                var changed = false;

                ImGuiHelpers.ScaledDummy(5);

                ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderBunny);

                using var indent = ImRaii.PushIndent(10.0f);
                if (ImGui.Checkbox(Language.ConfigOptionFateWindow, ref Plugin.Configuration.ShowBunnyWindow))
                {
                    changed = true;
                    if (Plugin.Configuration.ShowBunnyWindow && Plugin.PlayerInRelevantTerritory())
                        Plugin.BunnyWindow.IsOpen = true;
                }
                ImGuiComponents.HelpMarker(Language.ConfigTooltipBunnyWindow);

                changed |= ImGui.Checkbox(Language.ConfigOptionCofferIcons, ref Plugin.Configuration.AddIconsOnEntry);
                ImGuiComponents.HelpMarker(Language.ConfigTooltipCofferIcons);

                changed |= Helper.AddSoundOption(0, Language.ConfigOptionSpawnNotification, ref Plugin.Configuration.PlayBunnyEffect, ref Plugin.Configuration.BunnySoundEffect);
                changed |= ImGui.Checkbox(Language.ConfigOptionBunnyLowLevelFates, ref Plugin.Configuration.OnlyEasyBunny);
                ImGuiComponents.HelpMarker(Language.ConfigTooltipBunnyLowLevelFates);

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

                ImGui.SameLine();

                using (ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.ParsedBlue))
                {
                    if (ImGui.Button(Language.ConfigButtonAddMapMarkers))
                        Plugin.PlaceEurekaMarkerSet(true);
                }

                ImGui.SameLine();

                using (ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.DPSRed))
                {
                    if (ImGui.Button(Language.ConfigButtonRemoveMapMarkers))
                        Plugin.RemoveMapMarker();
                }
            }
        }
    }

    private void TabStats()
    {
        using var tabItem = ImRaii.TabItem($"{Language.TabHeaderStats}##StatsTab");
        if (!tabItem.Success)
            return;

        var secondRow = ImGui.CalcTextSize("Killed Bunnies: ").X + 120.0f;

        ImGuiHelpers.ScaledDummy(5.0f);

        ImGui.TextUnformatted(Language.ConfigHeaderTotalStats);
        using (ImRaii.PushIndent(10.0f))
        {
            var span = TimeSpan.FromMilliseconds(Plugin.Configuration.TimeInEureka + Plugin.EurekaWatch.ElapsedMilliseconds);
            var text = $"{(int)span.TotalHours:###00}:{span:mm} h";
            ImGui.TextUnformatted(Language.StatsEntryTime);
            ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);

            text = $"{Plugin.Configuration.KilledBunnies}";
            ImGui.TextUnformatted(Language.StatsEntryKilled);
            ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);

            var statValues = Plugin.Configuration.Stats.Values;
            var total = statValues.Sum(v => v.Values.Sum());
            text = total.ToString();
            ImGui.TextUnformatted(Language.StatsEntryFound);
            ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);

            var bronze = statValues.Sum(v => v[2009532]);
            TextWithCalculatedSpacing(Language.StatsEntryBronze, bronze, total);

            var silver = statValues.Sum(v => v[2009531]);
            TextWithCalculatedSpacing(Language.StatsEntrySilver, silver, total);

            var gold = statValues.Sum(v => v[2009530]);
            TextWithCalculatedSpacing(Language.StatsEntryGold, gold, total);
        }

        ImGuiHelpers.ScaledDummy(10.0f);

        ImGui.TextUnformatted(Language.ConfigHeaderMapStats);
        using (var tabBar = ImRaii.TabBar("StatsTabBar"))
        {
            if (tabBar.Success)
            {
                foreach (var (key, value) in Plugin.Configuration.Stats)
                {
                    using var tabItemInner = ImRaii.TabItem($"{QuestHelper.TerritoryToPlaceName(key)}##AreaTab");
                    if (!tabItemInner.Success)
                        continue;

                    using var indent = ImRaii.PushIndent(10.0f);
                    var total = value.Sum(c => c.Value);
                    TextWithCalculatedSpacing(Language.StatsEntryBronze, value[2009532], total);
                    TextWithCalculatedSpacing(Language.StatsEntrySilver, value[2009531], total);
                    TextWithCalculatedSpacing(Language.StatsEntryGold, value[2009530], total);
                }
            }
        }

        ImGuiHelpers.ScaledDummy(20.0f);

        ImGui.TextColored(ImGuiColors.DalamudOrange, "Hover Me");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("For more detailed loot tracking take a look at the plugin TrackyTack~");
    }

    private void TabDebug()
    {
        using var tabItem = ImRaii.TabItem($"Debug##DebugTab");
        if (!tabItem.Success)
            return;

        if (Plugin.LastSeenFate == Library.EurekaFate.Empty)
        {
            var list = Plugin.ClientState.TerritoryType switch
            {
                732 => Plugin.Library.AnemosFates,
                763 => Plugin.Library.PagosFates,
                795 => Plugin.Library.PyrosFates,
                827 => Plugin.Library.HydatosFates,
                _ => Plugin.Library.AnemosFates
            };

            var stringList = list.Select(x => x.Name).ToArray();
            ImGui.Combo("##fateSelector", ref DebugFate, stringList, stringList.Length);

            if (ImGui.Button("Populate last seen fate"))
                Plugin.LastSeenFate = list[DebugFate];
        }
        else
        {
            ImGui.TextUnformatted($"Current Fate: {Plugin.LastSeenFate.Name}");

            ImGuiHelpers.ScaledDummy(10);

            if (ImGui.Button("Test NMPop"))
                Plugin.NMPop();

            if (ImGui.Button("Test EchoNMPop"))
                Plugin.EchoNMPop();

            if (ImGui.Button("Open Post Window"))
            {
                Plugin.ShoutWindow.SetEorzeaTimeWithPullOffset();
                Plugin.ShoutWindow.IsOpen = true;
            }

            if (ImGui.Button("SetFlagMarker"))
                Plugin.SetFlagMarker((MapLinkPayload)Plugin.LastSeenFate.MapLink.Payloads[0]);

            ImGuiHelpers.ScaledDummy(20);

            if (ImGui.Button("Reset"))
                Plugin.LastSeenFate = Library.EurekaFate.Empty;
        }
    }

    private static void TextWithCalculatedSpacing(string header, int count, int total)
    {
        if (total == 0)
            total = 1;

        var text = count.ToString();
        var perc = $"{(double) count / total * 100.0:##0.00} %%";

        var secondRow = ImGui.CalcTextSize("Killed Bunnies: ").X + 120.0f;
        var thirdRow = secondRow + 100.0f;

        ImGui.TextUnformatted(header);
        ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
        ImGui.TextColored(ImGuiColors.DalamudOrange, text);
        ImGui.SameLine(thirdRow - ImGui.CalcTextSize(perc).X);
        ImGui.TextColored(ImGuiColors.TankBlue, perc);
    }
}