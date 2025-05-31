using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CheapLoc;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;

using static ImGuiNET.ImGuiWindowFlags;

namespace EurekaTrackerAutoPopper.Windows;

public class MainWindow : Window, IDisposable
{
    private const ImGuiColorEditFlags ColorFlags = ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha;
    private static readonly int[] SoundEffects = [36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52];

    private readonly Plugin Plugin;

    private int DebugFate;
    public string Instance = string.Empty;
    public string Password = string.Empty;

    public MainWindow(Plugin plugin) : base("Eureka Linker##EurekaLinker")
    {
        Flags = NoScrollbar | NoScrollWithMouse;
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(450, 400),
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
        if (ImGui.BeginTabItem($"{Loc.Localize("Tab Header - General", "General")}##general-tab"))
        {
            ImGuiHelpers.ScaledDummy(5);

            ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config Header - Pop", "Pop"));
            ImGuiHelpers.ScaledIndent(10.0f);
            var changed = false;
            changed |= ImGui.Checkbox(Loc.Localize("Config Option - Echo NM", "Echo"), ref Plugin.Configuration.EchoNMPop);
            changed |= AddSoundOption(ref Plugin.Configuration.PlaySoundEffect, ref Plugin.Configuration.PopSoundEffect);
            changed |= ImGui.Checkbox(Loc.Localize("Config Option - Toast NM", "Show Toast"), ref Plugin.Configuration.ShowPopToast);
            if (Plugin.Configuration.EchoNMPop || Plugin.Configuration.ShowPopToast)
            {
                ImGuiHelpers.ScaledIndent(10.0f);
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Short Names", "Use Short Names"), ref Plugin.Configuration.UseShortNames);
                ImGuiHelpers.ScaledIndent(-10.0f);
            }
            ImGuiHelpers.ScaledIndent(-10.0f);

            if (changed)
                Plugin.Configuration.Save();

            ImGuiHelpers.ScaledDummy(10);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);
            ImGui.TextUnformatted(Loc.Localize("Config Header - Tracker", "Tracker:"));
            ImGuiHelpers.ScaledDummy(10);

            if (ImGui.InputText(Loc.Localize("Config Input - URL", "Instance URL"), ref Instance, 31))
                Instance = Instance.Length == 6 ? $"https://ffxiv-eureka.com/{Instance}" : Instance;


            if (!string.IsNullOrEmpty(Instance))
            {
                ImGui.SameLine();
                if (ImGuiComponents.IconButton(1, FontAwesomeIcon.Clipboard))
                    ImGui.SetClipboardText(Instance);
            }

            ImGui.InputText(Loc.Localize("Config Input - PW", "Password"), ref Password, 50);
            if (!string.IsNullOrEmpty(Password))
            {
                ImGui.SameLine();
                if (ImGuiComponents.IconButton(2, FontAwesomeIcon.Clipboard))
                    ImGui.SetClipboardText(Password);
            }

            if (Plugin.PlayerInEureka && string.IsNullOrEmpty(Instance))
            {
                if (ImGui.Button(Loc.Localize("Config Button - Start New", "Start New Tracker")))
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
                ImGui.Button(Loc.Localize("Config Button - Start New", "Start New Tracker"));
                ImGui.EndDisabled();

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    ImGui.SetTooltip(Loc.Localize("Config Tooltip - Start New", "Only available while in eureka"));
            }

            if (Instance.Length > 0)
            {
                if (ImGuiComponents.IconButton(3, FontAwesomeIcon.Globe))
                    Dalamud.Utility.Util.OpenLink(Instance);

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(Loc.Localize("Config Tooltip - Open Tracker", "Open Tracker in Browser"));
            }

            ImGui.EndTabItem();
        }
    }

    private void TabChat()
    {
        if (ImGui.BeginTabItem($"{Loc.Localize("Tab Header - Chat", "Chat")}##chat-tab"))
        {
            ImGuiHelpers.ScaledDummy(5);

            var changed = false;
            changed |= ImGui.Checkbox(Loc.Localize("Config Option - Open Shout Window", "Show Shout Window"), ref Plugin.Configuration.ShowPopWindow);
            changed |= ImGui.Checkbox(Loc.Localize("Config Option - Copy Shout Message", "Copy Shout Message"), ref Plugin.Configuration.CopyShoutMessage);
            changed |= ImGui.Checkbox(Loc.Localize("Config Option - Randomize Coords", "Randomize Map Coords"), ref Plugin.Configuration.RandomizeMapCoords);
            changed |= ImGui.Checkbox(Loc.Localize("Config Option - Show PT", "Show PT in Shout Window"), ref Plugin.Configuration.ShowPullTimer);
            ImGuiComponents.HelpMarker(Loc.Localize("Config Tooltip - Show PT", "Disabling this option will remove time element from the shout message."));

            if (Plugin.Configuration.ShowPullTimer)
            {
                ImGuiHelpers.ScaledIndent(10.0f);
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Use ET", "Use Eorzea Time instead"), ref Plugin.Configuration.UseEorzeaTimer);
                if (Plugin.Configuration.UseEorzeaTimer)
                    changed |= ImGui.Checkbox(Loc.Localize("Config Option - Use 12h", "Use 12-hour Format"), ref Plugin.Configuration.UseTwelveHourFormat);
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
            ImGui.TextUnformatted(Loc.Localize("Config Header - Format", "Format:"));
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
        if (ImGui.BeginTabItem($"{Loc.Localize("Tab Header - Fairy", "Fairy")}##fairy-tab"))
        {
            if (ImGui.BeginChild("FairyContent", new Vector2(0, -50 * ImGuiHelpers.GlobalScale)))
            {
                ImGuiHelpers.ScaledDummy(5);

                var changed = false;
                ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config Header - Fairy", "Fairy / Elemental"));
                ImGuiHelpers.ScaledIndent(10.0f);
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Echo Fairy", "Echo"), ref Plugin.Configuration.EchoFairies);
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Toast Fairy", "Show Toast"), ref Plugin.Configuration.ShowFairyToast);
                ImGuiHelpers.ScaledIndent(-10.0f);

                if (changed)
                    Plugin.Configuration.Save();

                ImGuiHelpers.ScaledDummy(5);
                ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config Header - Known Locations", "Known Locations"));
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
                if (ImGui.Button(Loc.Localize("Config Button - Fairy All", "Echo All")))
                    foreach (var fairy in Plugin.Library.ExistingFairies)
                        Plugin.EchoFairy(fairy);

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.ParsedBlue);
                if (ImGui.Button(Loc.Localize("Config Button - Add Map Markers", "Add Markers")))
                    Plugin.AddFairyLocationsMap();
                ImGui.PopStyleColor();

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DPSRed);
                if (ImGui.Button(Loc.Localize("Config Button - Remove Map Markers", "Remove Markers")))
                    Plugin.RemoveMarkerMap();
                ImGui.PopStyleColor();
            }
            ImGui.EndChild();

            ImGui.EndTabItem();
        }
    }

    private void TabBunny()
    {
        if (ImGui.BeginTabItem($"{Loc.Localize("Tab Header - Bunny", "Bunny")}##bunny-tab"))
        {
            if (ImGui.BeginChild("BunnyContent", new Vector2(0, -50 * ImGuiHelpers.GlobalScale)))
            {
                ImGuiHelpers.ScaledDummy(5);

                var circleColor = Plugin.Configuration.CircleColor;
                var changed = false;

                ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config Header - Bunny", "Bunny"));
                ImGuiHelpers.ScaledIndent(10.0f);
                if (ImGui.Checkbox(Loc.Localize("Config Option - Bunny Window", "Show Window on Entry"), ref Plugin.Configuration.ShowBunnyWindow))
                {
                    if (Plugin.Configuration.ShowBunnyWindow && Plugin.PlayerInRelevantTerritory())
                        Plugin.BunnyWindow.IsOpen = true;
                    changed = true;
                }
                ImGuiComponents.HelpMarker(Loc.Localize("Config Tooltip - Bunny Window", "Or use the command '/elbunny'."));
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Coffer Icons", "Add Coffer Icons on Entry"), ref Plugin.Configuration.AddIconsOnEntry);
                ImGuiComponents.HelpMarker(Loc.Localize("Config Tooltip - Coffer Icons", "Adds the coffer locations on entry of eureka."));

                changed |= AddSoundOption(ref Plugin.Configuration.PlayBunnyEffect, ref Plugin.Configuration.BunnySoundEffect);
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Bunny Low Level Fates", "Show Only Low Level"), ref Plugin.Configuration.OnlyEasyBunny);
                ImGuiComponents.HelpMarker(Loc.Localize("Config Tooltip - Bunny Low Level Fates", "Only shows the low level bunny fates for Pagos and Pyros."));
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Draw Circle", "Draw Nearby Coffer Circle"), ref Plugin.Configuration.BunnyCircleDraw);
                ImGui.SameLine();
                ImGui.ColorEdit4("##circleColorPicker", ref circleColor, ColorFlags);

                if (circleColor != Plugin.Configuration.CircleColor)
                {
                    circleColor.W = 1; // fix alpha

                    Plugin.Configuration.CircleColor = circleColor;
                    changed = true;
                }
                ImGuiComponents.HelpMarker(Loc.Localize("Config Tooltip - Draw Circle", "Draws a circle if the player is near a coffer location."));
                ImGuiHelpers.ScaledIndent(-10.0f);

                if (changed)
                    Plugin.Configuration.Save();
            }
            ImGui.EndChild();

            ImGuiHelpers.ScaledDummy(5);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);

            if (ImGui.BeginChild("BunnyBottomBar", new Vector2(0, 0), false, 0))
            {
                if (ImGui.Button(Loc.Localize("Config Button - Circle Preview", "Preview")))
                    Plugin.EnablePreview();

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.ParsedBlue);
                if (ImGui.Button(Loc.Localize("Config Button - Add Map Markers", "Add Markers")))
                    Plugin.AddChestsLocationsMap();
                ImGui.PopStyleColor();

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DPSRed);
                if (ImGui.Button(Loc.Localize("Config Button - Remove Map Markers", "Remove Markers")))
                    Plugin.RemoveMarkerMap();
                ImGui.PopStyleColor();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.ParsedBlue);
                if (ImGui.Button("Add Occult Markers"))
                    Plugin.AddOccultTreasureLocations();
                ImGui.PopStyleColor();
            }
            ImGui.EndChild();

            ImGui.EndTabItem();
        }
    }

    private void TabOccult()
    {
        if (ImGui.BeginTabItem($"{Loc.Localize("Tab Header - Occult", "Occult")}##occult-tab"))
        {
            if (ImGui.BeginChild("OccultContent", new Vector2(0, -50 * ImGuiHelpers.GlobalScale)))
            {
                ImGuiHelpers.ScaledDummy(5);

                var changed = false;
                ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config Header - Treasure Proximity", "Random Treasure Proximity"));
                ImGuiHelpers.ScaledIndent(10.0f);
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Echo Treasure", "Echo"), ref Plugin.Configuration.EchoTreasure);
                ImGui.SameLine();
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Toast Treasure", "Show Toast"), ref Plugin.Configuration.ShowTreasureToast);
                ImGuiHelpers.ScaledIndent(-10.0f);

                ImGuiHelpers.ScaledDummy(5);

                ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config Header - Carrot Proximity", "Bunny Carrot Proximity"));
                ImGuiHelpers.ScaledIndent(10.0f);
                changed |= ImGui.Checkbox($"{Loc.Localize("Config Option - Echo Bunny Carrot", "Echo")}##EchoBunnyCarrot", ref Plugin.Configuration.EchoBunnyCarrot);
                ImGui.SameLine();
                changed |= ImGui.Checkbox($"{Loc.Localize("Config Option - Toast Treasure", "Show Toast")}##ToastBunnyCarrot", ref Plugin.Configuration.ShowBunnyCarrotToast);
                ImGuiHelpers.ScaledIndent(-10.0f);

                ImGuiHelpers.ScaledDummy(5);

                ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config Header - Marker Set Header", "Map Marker"));
                ImGuiHelpers.ScaledIndent(10.0f);
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Place Default", "Place After Entering"), ref Plugin.Configuration.PlaceDefaultOccult);
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
                ImGuiHelpers.ScaledIndent(-10.0f);

                ImGuiHelpers.ScaledDummy(5);

                var circleColor = Plugin.Configuration.CircleColor;
                ImGui.TextColored(ImGuiColors.DalamudViolet, Loc.Localize("Config Header - Pot Location Helper", "Pot Location Helper"));
                ImGuiHelpers.ScaledIndent(10.0f);
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Draw Circle", "Draw Nearby Coffer Circle"), ref Plugin.Configuration.BunnyCircleDraw);
                ImGui.SameLine();
                ImGui.ColorEdit4("##circleColorPicker", ref circleColor, ColorFlags);

                if (circleColor != Plugin.Configuration.CircleColor)
                {
                    circleColor.W = 1; // fix alpha

                    Plugin.Configuration.CircleColor = circleColor;
                    changed = true;
                }
                ImGuiComponents.HelpMarker(Loc.Localize("Config Tooltip - Draw Circle", "Draws a circle if the player is near a coffer location."));

                if (ImGui.Button(Loc.Localize("Config Button - Circle Preview", "Preview")))
                    Plugin.EnablePreview();

                ImGuiHelpers.ScaledIndent(-10.0f);

                if (changed)
                    Plugin.Configuration.Save();
            }
            ImGui.EndChild();

            ImGuiHelpers.ScaledDummy(5);
            ImGui.Separator();
            ImGuiHelpers.ScaledDummy(5);

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
                if (ImGui.Button(Loc.Localize("Config Button - Remove Map Markers", "Remove Markers")))
                    Plugin.RemoveMarkerMap();
                ImGui.PopStyleColor();
            }
            ImGui.EndChild();

            ImGui.EndTabItem();
        }
    }

    private void TabStats()
    {
        var secondRow = ImGui.CalcTextSize("Killed Bunnies: ").X + 120.0f;

        if (ImGui.BeginTabItem($"{Loc.Localize("Tab Header - Stats", "Stats")}##stats-tab"))
        {
            ImGuiHelpers.ScaledDummy(5.0f);

            ImGui.TextUnformatted(Loc.Localize("Config Header - Total Stats", "Total Stats:"));
            ImGui.Indent(10.0f);

            var span = TimeSpan.FromMilliseconds(Plugin.Configuration.TimeInEureka + Plugin.EurekaWatch.ElapsedMilliseconds);
            var text = $"{(int) span.TotalHours:###00}:{span:mm} h";
            ImGui.TextUnformatted(Loc.Localize("Stats Entry - Time", "Time in Eureka:"));
            ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);

            text = $"{Plugin.Configuration.KilledBunnies}";
            ImGui.TextUnformatted(Loc.Localize("Stats Entry - Killed", "Killed Bunnies:"));
            ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);

            var total = Plugin.Configuration.Stats.Values.Sum(v => v.Values.Sum());
            text = total.ToString();
            ImGui.TextUnformatted(Loc.Localize("Stats Entry - Found", "Coffers Found:"));
            ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);

            var bronze = Plugin.Configuration.Stats.Values.Sum(v => v[2009532]);
            TextWithCalculatedSpacing(Loc.Localize("Stats Entry - Bronze", "Bronze:"), bronze, total);

            var silver = Plugin.Configuration.Stats.Values.Sum(v => v[2009531]);
            TextWithCalculatedSpacing(Loc.Localize("Stats Entry - Silver", "Silver:"), silver, total);

            var gold = Plugin.Configuration.Stats.Values.Sum(v => v[2009530]);
            TextWithCalculatedSpacing(Loc.Localize("Stats Entry - Gold", "Gold:"), gold, total);

            ImGui.Unindent(10.0f);
            ImGuiHelpers.ScaledDummy(10.0f);

            ImGui.TextUnformatted(Loc.Localize("Config Header - Map Stats", "Map Stats:"));
            if (ImGui.BeginTabBar("StatsTabBar"))
            {
                foreach (var (key, value) in Plugin.Configuration.Stats)
                {
                    if (ImGui.BeginTabItem($"{QuestHelper.TerritoryToPlaceName(key)}##area-tab"))
                    {
                        ImGui.Indent(10.0f);
                        total = value.Sum(c => c.Value);
                        TextWithCalculatedSpacing(Loc.Localize("Stats Entry - Bronze", "Bronze:"), value[2009532], total);
                        TextWithCalculatedSpacing(Loc.Localize("Stats Entry - Silver", "Silver:"), value[2009531], total);
                        TextWithCalculatedSpacing(Loc.Localize("Stats Entry - Gold", "Gold:"), value[2009530], total);
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
        if (ImGui.BeginTabItem($"{Loc.Localize("Tab Header - About", "About")}##about-tab"))
        {
            if (ImGui.BeginChild("AboutContent", new Vector2(0, -50 *  ImGuiHelpers.GlobalScale)))
            {
                ImGuiHelpers.ScaledDummy(5);

                ImGui.TextUnformatted(Loc.Localize("About Entry - Author", "Author:"));
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedGold, Plugin.PluginInterface.Manifest.Author);

                ImGui.TextUnformatted(Loc.Localize("About Entry - Discord", "Discord:"));
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.ParsedGold, "@infi");

                ImGui.TextUnformatted(Loc.Localize("About Entry - Version", "Version:"));
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
                if (ImGui.Button(Loc.Localize("Config Button - Discord Forum Thread", "Discord Thread")))
                    Dalamud.Utility.Util.OpenLink("https://canary.discord.com/channels/581875019861328007/1085943921160491050");
                ImGui.PopStyleColor();

                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DPSRed);
                if (ImGui.Button(Loc.Localize("Config Button - Github Issues", "Issues")))
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
                    Plugin.SetFlagMarker();

                if(ImGui.Button("Export Loc"))
                {
                    var pwd = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(Plugin.PluginInterface.AssemblyLocation.DirectoryName!);
                    Loc.ExportLocalizable();
                    Directory.SetCurrentDirectory(pwd);
                }

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
        changed |= ImGui.Checkbox(Loc.Localize("Config Option - Sound NM", "Play Sound"), ref playSound);

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