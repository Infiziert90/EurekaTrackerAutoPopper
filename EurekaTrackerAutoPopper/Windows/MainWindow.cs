using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using EurekaTrackerAutoPopper.Resources;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;

using static ImGuiNET.ImGuiWindowFlags;

namespace EurekaTrackerAutoPopper.Windows;

public class MainWindow : Window, IDisposable
{
    private const float SeparatorPadding = 1.0f;
    private static float GetSeparatorPaddingHeight => SeparatorPadding * ImGuiHelpers.GlobalScale;

    private const ImGuiColorEditFlags ColorFlags = ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha;
    private static readonly int[] SoundEffects = [36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52];

    private readonly Plugin Plugin;

    private int DebugFate;
    public string Instance = string.Empty;
    public string Password = string.Empty;

    public MainWindow(Plugin plugin) : base("Eureka Linker##EurekaLinker")
    {
        Flags = NoScrollbar | NoScrollWithMouse;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 570),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("##SettingTabs"))
        {
            TabGeneral();

            TabChat();

            TabFairy();

            TabBunny();

            TabOccult();

            TabStats();

            TabAbout();

            #if DEBUG
            TabDebug();
            #endif

            ImGui.EndTabBar();
        }
    }

    private void TabGeneral()
    {
        if (ImGui.BeginTabItem($"{Language.TabHeaderGeneral}##general-tab"))
        {
            ImGuiHelpers.ScaledDummy(5);

            ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderPop);
            ImGuiHelpers.ScaledIndent(10.0f);
            var changed = false;
            changed |= ImGui.Checkbox(Language.ConfigOptionEchoNM, ref Plugin.Configuration.EchoNMPop);
            changed |= AddSoundOption(ref Plugin.Configuration.PlaySoundEffect, ref Plugin.Configuration.PopSoundEffect);
            changed |= ImGui.Checkbox(Language.ConfigOptionToastNM, ref Plugin.Configuration.ShowPopToast);
            if (Plugin.Configuration.EchoNMPop || Plugin.Configuration.ShowPopToast)
            {
                ImGuiHelpers.ScaledIndent(10.0f);
                changed |= ImGui.Checkbox(Language.ConfigOptionShortNames, ref Plugin.Configuration.UseShortNames);
                ImGuiHelpers.ScaledIndent(-10.0f);
            }
            ImGuiHelpers.ScaledIndent(-10.0f);

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
                {
                    Task.Run(() =>
                    {
                        (Instance, Password) = EurekaTrackerWrapper.WebRequests.CreateNewTracker(Library.TerritoryToTrackerDictionary[Plugin.ClientState.TerritoryType]).Result;
                        Plugin.ProcessCurrentFates(Plugin.ClientState.TerritoryType);
                    });
                }
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.Button(Language.ConfigButtonStartNew);
                ImGui.EndDisabled();

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

            ImGui.EndTabItem();
        }
    }

    private void TabChat()
    {
        if (ImGui.BeginTabItem($"{Language.TabHeaderChat}##chat-tab"))
        {
            ImGuiHelpers.ScaledDummy(5);

            var changed = false;
            changed |= ImGui.Checkbox(Language.ConfigOptionOpenShoutWindow, ref Plugin.Configuration.ShowPopWindow);
            changed |= ImGui.Checkbox(Language.ConfigOptionCopyShoutMessage, ref Plugin.Configuration.CopyShoutMessage);
            changed |= ImGui.Checkbox(Language.ConfigOptionRandomizeCoords, ref Plugin.Configuration.RandomizeMapCoords);
            changed |= ImGui.Checkbox(Language.ConfigOptionShowPT, ref Plugin.Configuration.ShowPullTimer);
            ImGuiComponents.HelpMarker(Language.ConfigTooltipShowPT);

            if (Plugin.Configuration.ShowPullTimer)
            {
                ImGuiHelpers.ScaledIndent(10.0f);
                changed |= ImGui.Checkbox(Language.ConfigOptionUseET, ref Plugin.Configuration.UseEorzeaTimer);
                if (Plugin.Configuration.UseEorzeaTimer)
                    changed |= ImGui.Checkbox(Language.ConfigOptionUse12h, ref Plugin.Configuration.UseTwelveHourFormat);
                ImGuiHelpers.ScaledIndent(-10.0f);
            }

            if (changed)
            {
                Plugin.Configuration.Save();
                Plugin.Library.Initialize();
            }

            ImGuiHelpers.ScaledDummy(5);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);
            ImGui.TextUnformatted(Language.ConfigHeaderFormat);
            ImGuiHelpers.ScaledDummy(10);

            string chatFormat = Plugin.Configuration.ChatFormat;
            ImGui.InputText("##input-chatformat", ref chatFormat, 64);
            if (chatFormat != Plugin.Configuration.ChatFormat)
            {
                Plugin.Configuration.ChatFormat = chatFormat;
                Plugin.Configuration.Save();
            }

            ImGuiHelpers.ScaledDummy(5);

            ImGui.TextUnformatted("$n = Full Name");
            ImGui.TextUnformatted("$sN = Short Name");
            ImGui.TextUnformatted("$p = MapFlag");
            ImGui.TextUnformatted("$t = Pull Timer - e.g. PT 27 / ET 13:37");

            ImGui.EndTabItem();
        }
    }

    private void TabFairy()
    {
        if (ImGui.BeginTabItem($"{Language.TabHeaderFairy}##fairy-tab"))
        {
            if (ImGui.BeginChild("FairyContent", new Vector2(0, -50 * ImGuiHelpers.GlobalScale)))
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

                if (ImGui.BeginChild("FairyTable"))
                {
                    if (ImGui.BeginTable("##ExistingFairiesTable", 2))
                    {
                        ImGui.TableSetupColumn("##location");
                        ImGui.TableSetupColumn("##trash", 0, 0.1f);

                        foreach (var (fairy, idx) in Plugin.Library.ExistingFairies.Select((val, i) => (val, i)).ToArray())
                        {
                            var map = (MapLinkPayload) fairy.MapLink.Payloads.First();

                            ImGui.TableNextColumn();
                            if (ImGui.Selectable($"Fairy ({map.XCoord:0.0},  {map.YCoord:0.0})##{idx}"))
                                Plugin.OpenMap(map);

                            ImGui.TableNextColumn();
                            if (ImGuiComponents.IconButton(idx, FontAwesomeIcon.Trash))
                                Plugin.Library.ExistingFairies.RemoveAt(idx);
                        }

                        ImGui.EndTable();
                    }
                }
                ImGui.EndChild();
            }
            ImGui.EndChild();

            ImGuiHelpers.ScaledDummy(5);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);

            if (ImGui.BeginChild("FairyBottomBar", new Vector2(0, 0), false, 0))
            {
                if (ImGui.Button(Language.ConfigButtonFairyAll))
                    foreach (var fairy in Plugin.Library.ExistingFairies)
                        Plugin.EchoFairy(fairy);

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.ParsedBlue);
                if (ImGui.Button(Language.ConfigButtonAddMapMarkers))
                    Plugin.PlaceEurekaMarkerSet(false, false);
                ImGui.PopStyleColor();

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DPSRed);
                if (ImGui.Button(Language.ConfigButtonRemoveMapMarkers))
                    Plugin.RemoveMapMarker();
                ImGui.PopStyleColor();
            }
            ImGui.EndChild();

            ImGui.EndTabItem();
        }
    }

    private void TabBunny()
    {
        if (ImGui.BeginTabItem($"{Language.TabHeaderBunny}##bunny-tab"))
        {
            var buttonHeight = ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().WindowPadding.Y + GetSeparatorPaddingHeight;
            if (ImGui.BeginChild("BunnyContent", new Vector2(0, -buttonHeight)))
            {
                ImGuiHelpers.ScaledDummy(5);

                var circleColor = Plugin.Configuration.CircleColor;
                var changed = false;

                ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderBunny);
                ImGuiHelpers.ScaledIndent(10.0f);
                if (ImGui.Checkbox(Language.ConfigOptionFateWindow, ref Plugin.Configuration.ShowBunnyWindow))
                {
                    if (Plugin.Configuration.ShowBunnyWindow && Plugin.PlayerInRelevantTerritory())
                        Plugin.BunnyWindow.IsOpen = true;
                    changed = true;
                }
                ImGuiComponents.HelpMarker(Language.ConfigTooltipBunnyWindow);
                changed |= ImGui.Checkbox(Language.ConfigOptionCofferIcons, ref Plugin.Configuration.AddIconsOnEntry);
                ImGuiComponents.HelpMarker(Language.ConfigTooltipCofferIcons);

                changed |= AddSoundOption(ref Plugin.Configuration.PlayBunnyEffect, ref Plugin.Configuration.BunnySoundEffect);
                changed |= ImGui.Checkbox(Language.ConfigOptionBunnyLowLevelFates, ref Plugin.Configuration.OnlyEasyBunny);
                ImGuiComponents.HelpMarker(Language.ConfigTooltipBunnyLowLevelFates);
                changed |= ImGui.Checkbox(Language.ConfigOptionDrawCircle, ref Plugin.Configuration.BunnyCircleDraw);
                ImGui.SameLine();
                ImGui.ColorEdit4("##circleColorPicker", ref circleColor, ColorFlags);

                if (circleColor != Plugin.Configuration.CircleColor)
                {
                    circleColor.W = 1; // fix alpha

                    Plugin.Configuration.CircleColor = circleColor;
                    changed = true;
                }
                ImGuiComponents.HelpMarker(Language.ConfigTooltipDrawCircle);
                ImGuiHelpers.ScaledIndent(-10.0f);

                if (changed)
                    Plugin.Configuration.Save();
            }
            ImGui.EndChild();

            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(1.0f);

            if (ImGui.BeginChild("BunnyBottomBar", new Vector2(0, 0), false, 0))
            {
                if (ImGui.Button(Language.ConfigButtonCirclePreview))
                    Plugin.EnablePreview();

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.ParsedBlue);
                if (ImGui.Button(Language.ConfigButtonAddMapMarkers))
                    Plugin.PlaceEurekaMarkerSet(true);
                ImGui.PopStyleColor();

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DPSRed);
                if (ImGui.Button(Language.ConfigButtonRemoveMapMarkers))
                    Plugin.RemoveMapMarker();
                ImGui.PopStyleColor();
            }
            ImGui.EndChild();

            ImGui.EndTabItem();
        }
    }

    private void TabOccult()
    {
        if (ImGui.BeginTabItem($"{Language.TabHeaderOccult}##occult-tab"))
        {
            var buttonHeight = ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().WindowPadding.Y + GetSeparatorPaddingHeight;
            if (ImGui.BeginChild("OccultContent", new Vector2(0, -buttonHeight)))
            {
                ImGuiHelpers.ScaledDummy(5);

                var changed = false;
                ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderProximityNotification);
                ImGuiHelpers.ScaledIndent(10.0f);
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
                ImGuiHelpers.ScaledIndent(-10.0f);

                ImGuiHelpers.ScaledDummy(5);

                ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderMarkerSetHeader);
                ImGuiHelpers.ScaledIndent(10.0f);
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
                    ImGuiHelpers.ScaledIndent(10.0f);
                    changed |= ImGui.Checkbox(Language.ConfigOptionSwitcherPosition, ref Plugin.Configuration.SwitcherBelowMap);
                    ImGuiHelpers.ScaledIndent(-10.0f);
                }
                ImGuiHelpers.ScaledIndent(-10.0f);

                ImGuiHelpers.ScaledDummy(5);

                ImGui.TextColored(ImGuiColors.DalamudViolet, Language.ConfigHeaderPotHelperHeader);
                ImGuiHelpers.ScaledIndent(10.0f);
                if (ImGui.Checkbox(Language.ConfigOptionFateWindow, ref Plugin.Configuration.ShowBunnyWindow))
                {
                    if (Plugin.Configuration.ShowBunnyWindow && Plugin.ClientState.TerritoryType == (uint)Territory.SouthHorn)
                        Plugin.BunnyWindow.IsOpen = true;

                    changed = true;
                }
                ImGuiComponents.HelpMarker(Language.ConfigTooltipBunnyWindow);
                changed |= AddSoundOption(ref Plugin.Configuration.PlayBunnyEffect, ref Plugin.Configuration.BunnySoundEffect);

                var circleColor = Plugin.Configuration.CircleColor;
                changed |= ImGui.Checkbox(Language.ConfigOptionDrawCircle, ref Plugin.Configuration.BunnyCircleDraw);
                ImGui.SameLine();
                ImGui.ColorEdit4("##circleColorPicker", ref circleColor, ColorFlags);

                if (circleColor != Plugin.Configuration.CircleColor)
                {
                    circleColor.W = 1; // fix alpha

                    Plugin.Configuration.CircleColor = circleColor;
                    changed = true;
                }
                ImGuiComponents.HelpMarker(Language.ConfigTooltipDrawCircle);

                if (ImGui.Button(Language.ConfigButtonCirclePreview))
                    Plugin.EnablePreview();

                ImGuiHelpers.ScaledIndent(-10.0f);

                if (changed)
                    Plugin.Configuration.Save();
            }
            ImGui.EndChild();

            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(1.0f);

            if (ImGui.BeginChild("OccultBottomBar", new Vector2(0, 0), false, 0))
            {
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted("Switch To:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(175.0f * ImGuiHelpers.GlobalScale);
                using (var combo = ImRaii.Combo("##SwitchMarkersToCombo", Plugin.MarkerSetToPlace.ToOccultSet().ToName()))
                {
                    if (combo.Success)
                    {
                        foreach (var set in EnumExtensions.OccultSetArray)
                        {
                            if (!ImGui.Selectable(set.ToName(), set == Plugin.MarkerSetToPlace.ToOccultSet()))
                                continue;

                            Plugin.PlaceOccultMarkerSet(set, true);
                        }
                    }
                }

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DPSRed);
                if (ImGui.Button(Language.ConfigButtonRemoveMapMarkers))
                    Plugin.RemoveMapMarker();
                ImGui.PopStyleColor();
            }
            ImGui.EndChild();

            ImGui.EndTabItem();
        }
    }

    private void TabStats()
    {
        var secondRow = ImGui.CalcTextSize("Killed Bunnies: ").X + 120.0f;

        if (ImGui.BeginTabItem($"{Language.TabHeaderStats}##stats-tab"))
        {
            ImGuiHelpers.ScaledDummy(5.0f);

            ImGui.TextUnformatted(Language.ConfigHeaderTotalStats);
            ImGui.Indent(10.0f);

            var span = TimeSpan.FromMilliseconds(Plugin.Configuration.TimeInEureka + Plugin.EurekaWatch.ElapsedMilliseconds);
            var text = $"{(int) span.TotalHours:###00}:{span:mm} h";
            ImGui.TextUnformatted(Language.StatsEntryTime);
            ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);

            text = $"{Plugin.Configuration.KilledBunnies}";
            ImGui.TextUnformatted(Language.StatsEntryKilled);
            ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);

            var total = Plugin.Configuration.Stats.Values.Sum(v => v.Values.Sum());
            text = total.ToString();
            ImGui.TextUnformatted(Language.StatsEntryFound);
            ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);

            var bronze = Plugin.Configuration.Stats.Values.Sum(v => v[2009532]);
            TextWithCalculatedSpacing(Language.StatsEntryBronze, bronze, total);

            var silver = Plugin.Configuration.Stats.Values.Sum(v => v[2009531]);
            TextWithCalculatedSpacing(Language.StatsEntrySilver, silver, total);

            var gold = Plugin.Configuration.Stats.Values.Sum(v => v[2009530]);
            TextWithCalculatedSpacing(Language.StatsEntryGold, gold, total);

            ImGui.Unindent(10.0f);
            ImGuiHelpers.ScaledDummy(10.0f);

            ImGui.TextUnformatted(Language.ConfigHeaderMapStats);
            if (ImGui.BeginTabBar("StatsTabBar"))
            {
                foreach (var (key, value) in Plugin.Configuration.Stats)
                {
                    if (ImGui.BeginTabItem($"{QuestHelper.TerritoryToPlaceName(key)}##area-tab"))
                    {
                        ImGui.Indent(10.0f);
                        total = value.Sum(c => c.Value);
                        TextWithCalculatedSpacing(Language.StatsEntryBronze, value[2009532], total);
                        TextWithCalculatedSpacing(Language.StatsEntrySilver, value[2009531], total);
                        TextWithCalculatedSpacing(Language.StatsEntryGold, value[2009530], total);
                        ImGui.Unindent(10.0f);

                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }

            ImGuiHelpers.ScaledDummy(20.0f);
            ImGui.TextColored(ImGuiColors.DalamudOrange, "Hover Me");
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("For more detailed loot tracking take a look at the plugin TrackyTack~");
            ImGui.EndTabItem();
        }
    }

    private void TabAbout()
    {
        if (ImGui.BeginTabItem($"{Language.TabHeaderAbout}##about-tab"))
        {
            if (ImGui.BeginChild("AboutContent", new Vector2(0, -50 *  ImGuiHelpers.GlobalScale)))
            {
                ImGuiHelpers.ScaledDummy(5);

                ImGui.TextUnformatted(Language.AboutEntryAuthor);
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedGold, Plugin.PluginInterface.Manifest.Author);

                ImGui.TextUnformatted(Language.AboutEntryDiscord);
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedGold, "@infi");

                ImGui.TextUnformatted(Language.AboutEntryVersion);
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedOrange, Plugin.PluginInterface.Manifest.AssemblyVersion.ToString());
            }
            ImGui.EndChild();

            ImGuiHelpers.ScaledDummy(5);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);

            if (ImGui.BeginChild("AboutBottomBar", new Vector2(0, 0), false, 0))
            {
                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.ParsedBlue);
                if (ImGui.Button(Language.ConfigButtonDiscordForumThread))
                    Dalamud.Utility.Util.OpenLink("https://canary.discord.com/channels/581875019861328007/1085943921160491050");
                ImGui.PopStyleColor();

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DPSRed);
                if (ImGui.Button(Language.ConfigButtonGithubIssues))
                    Dalamud.Utility.Util.OpenLink("https://github.com/Infiziert90/EurekaTrackerAutoPopper/issues");
                ImGui.PopStyleColor();

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.12549f, 0.74902f, 0.33333f, 0.6f));
                if (ImGui.Button("Ko-Fi Tip"))
                    Dalamud.Utility.Util.OpenLink("https://ko-fi.com/infiii");
                ImGui.PopStyleColor();
            }
            ImGui.EndChild();

            ImGui.EndTabItem();
        }
    }

    private void TabDebug()
    {
        if (ImGui.BeginTabItem("Debug##debug-tab"))
        {
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

                if (ImGui.Button($"Populate last seen fate"))
                {
                    Plugin.LastSeenFate = list[DebugFate];
                }
            }
            else
            {
                ImGui.TextUnformatted($"Current Fate: {Plugin.LastSeenFate.Name}");

                ImGuiHelpers.ScaledDummy(10);

                if (ImGui.Button("Test NMPop"))
                    Plugin.NMPop();

                if (ImGui.Button($"Test EchoNMPop"))
                    Plugin.EchoNMPop();

                if (ImGui.Button($"Open Post Window"))
                {
                    Plugin.ShoutWindow.SetEorzeaTimeWithPullOffset();
                    Plugin.ShoutWindow.IsOpen = true;
                }

                if (ImGui.Button($"SetFlagMarker"))
                    Plugin.SetFlagMarker((MapLinkPayload)Plugin.LastSeenFate.MapLink.Payloads[0]);

                ImGuiHelpers.ScaledDummy(20);

                if (ImGui.Button($"Reset"))
                    Plugin.LastSeenFate = Library.EurekaFate.Empty;
            }

            ImGui.EndTabItem();
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

    private static bool AddSoundOption(ref bool playSound, ref int soundEffect)
    {
        var changed = false;
        changed |= ImGui.Checkbox(Language.ConfigOptionSpawnNotification, ref playSound);

        if (!playSound)
            return changed;

        ImGui.SameLine();

        ImGui.SetNextItemWidth(50.0f * ImGuiHelpers.GlobalScale);
        if (ImGui.BeginCombo("##SoundEffectCombo", soundEffect.ToString()))
        {
            foreach (var value in SoundEffects)
            {
                if (!ImGui.Selectable(value.ToString()))
                    continue;

                soundEffect = value;
                changed = true;
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Play))
            UIGlobals.PlaySoundEffect((uint) soundEffect);

        return changed;
    }

    public void Reset()
    {
        Instance = string.Empty;
        Password = string.Empty;
    }
}