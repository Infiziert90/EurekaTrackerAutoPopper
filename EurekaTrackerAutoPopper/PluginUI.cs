using ImGuiNET;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Linq;
using Dalamud.Interface;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using CheapLoc;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;

using static FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;
using static ImGuiNET.ImGuiWindowFlags;

namespace EurekaTrackerAutoPopper
{
    internal class PluginUI : IDisposable
    {
        private const ImGuiWindowFlags ConfigFlags = NoScrollbar | NoScrollWithMouse;
        private const ImGuiWindowFlags BunnyFlags = AlwaysAutoResize;
        private const ImGuiWindowFlags PopFlags = NoDecoration | AlwaysAutoResize;
        private const ImGuiWindowFlags CircleFlags = NoBackground | NoMove | NoTitleBar | NoScrollbar | NoResize | NoInputs;
        private const ImGuiColorEditFlags ColorFlags = ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha;

        private Configuration Configuration { get; init; }
        private Plugin Plugin { get; init; }
        private Library Library { get; init; }

        private string instance = "";
        private string password = "";
        private int soundEffect = 36;
        private int pullTime = 27;
        private int eorzeaTime = 720;

        public uint SoundEffect => (uint)soundEffect;
        public int PullTime => pullTime;
        public int DebugFate = 0;

        private readonly Timer ShoutTimer = new();
        private const int CountdownForShout = 20 * 1000; // Seconds

        public const int MinimalBunnyRespawn = 530;
        private const int MaximumBunnyRespawn = 1000;

        public bool NearToCoffer = false;
        public Vector3 CofferPos = Vector3.Zero;
        private uint imguiCircleColor;
        private readonly Timer PreviewTimer = new(5 * 1000);


        private bool settingsVisible;

        public bool SettingsVisible
        {
            get => settingsVisible;
            set => settingsVisible = value;
        }

        public bool PopVisible { get; set; }

        private bool bunnyVisible;

        public bool BunnyVisible
        {
            get => bunnyVisible;
            set => bunnyVisible = value;
        }

        public string Instance
        {
            get => instance;
            set => instance = value;
        }

        public string Password
        {
            get => password;
            set => password = value;
        }

        public PluginUI(Configuration configuration, Plugin plugin, Library library)
        {
            Configuration = configuration;
            Plugin = plugin;
            Library = library;

            imguiCircleColor = ImGui.GetColorU32(ImGui.ColorConvertFloat4ToU32(Configuration.CircleColor));
            ShoutTimer.AutoReset = false;
            PreviewTimer.AutoReset = false;
        }

        public void Dispose() { }

        public void Reset()
        {
            Instance = "";
            Password = "";
            PopVisible = false;
            BunnyVisible = false;
        }

        public void Draw()
        {
            DrawSettingsWindow();
            DrawFatePopWindow();
            DrawBunnyWindow();

            DrawCloseBunnyChest();
        }

        private void DrawCloseBunnyChest()
        {
            if (!Configuration.BunnyCircleDraw)
                return;

            if (!NearToCoffer)
                return;

            if (CofferPos == Vector3.Zero)
                return;

            Plugin.GameGui.WorldToScreen(CofferPos, out var circlePos);

            var winPos = new Vector2(circlePos.X - 15, circlePos.Y - 15);

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(winPos);
            ImGui.SetNextWindowSize(new Vector2(100, 50));
            if (ImGui.Begin("Pointer##bunny-chest", CircleFlags))
            {
                ImGui.GetWindowDrawList().AddCircleFilled(circlePos, 8.0f, imguiCircleColor);
                ImGui.End();
            }
        }

        private void DrawFatePopWindow()
        {
            if (!PopVisible)
                return;


            if (Plugin.LastSeenFate == Library.EurekaFate.Empty)
            {
                PopVisible = false;
                return;
            }

            if (ImGui.Begin("Eureka NM Pop", PopFlags))
            {
                string fate = $"Fate: {Plugin.LastSeenFate.Name}";
                float size = ImGui.CalcTextSize(fate).X;
                float extraSize = 0.0f; // extra spacing for ET time slider

                ImGui.TextUnformatted(fate);

                if (Configuration.ShowPullTimer)
                {
                    if (!Configuration.UseEorzeaTimer)
                    {
                        ImGui.SameLine(size + 35);
                        ImGui.TextUnformatted("PT");

                        ImGui.SameLine(size + 55);
                        ImGui.SetNextItemWidth(80);
                        if (ImGui.InputInt("##pulltimer_input", ref pullTime, 1))
                        {
                            pullTime = Math.Clamp(pullTime, 1, 30);
                        }
                    }
                    else
                    {
                        extraSize = 50;
                        ImGui.SameLine(size + 35);
                        ImGui.TextUnformatted("ET");

                        ImGui.SameLine(size + 55);
                        ImGui.SetNextItemWidth(80 + extraSize);
                        if (ImGui.SliderInt("##eorzeatime_input", ref eorzeaTime, 1, 1440, CurrentEorzeanPullTime()))
                        {
                            eorzeaTime = RoundOff(Math.Clamp(eorzeaTime, 1, 1440));
                        }
                    }
                }

                ImGui.Spacing();
                ImGui.NextColumn();

                ImGui.Columns(1);
                ImGui.Separator();

                ImGui.NewLine();

                ImGui.SameLine(size + 30 + extraSize);
                if (!ShoutTimer.Enabled)
                {
                    if (ImGui.Button(Loc.Localize("Shout Button - Post", "Post"), new Vector2(50, 0)))
                    {
                        Plugin.PostChatMessage();
                        PopVisible = false;
                    }
                }
                else
                {
                    ImGui.BeginDisabled();
                    ImGui.Button(Loc.Localize("Shout Button - Post", "Post"), new Vector2(50, 0));
                    ImGui.EndDisabled();

                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip(Loc.Localize("Shout Window - Limit", "Shout will be available 20s after spawn."));
                    }
                }

                ImGui.SameLine(size + 85 + extraSize);
                if (ImGui.Button(Loc.Localize("Shout Button - Close", "Close"), new Vector2(50, 0)))
                {
                    PopVisible = false;
                }

                ImGui.End();
            }
        }

        private void DrawBunnyWindow()
        {
            if (!BunnyVisible)
                return;

            if (!Library.BunnyMaps.Contains(Plugin.ClientState.TerritoryType))
                return;

            ImGui.SetNextWindowSizeConstraints(new Vector2(135, 70), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Eureka Bunnies", ref bunnyVisible, BunnyFlags))
            {
                var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                var bunnies = Library.Bunnies.Where(bnuuy => bnuuy.TerritoryId == Plugin.ClientState.TerritoryType).ToArray();
                if (Configuration.OnlyEasyBunny)
                    bunnies = bunnies[..1];

                foreach (var bunny in bunnies)
                {
                    if (bunnies.First() != bunny)
                    {
                        ImGui.Separator();
                        ImGuiHelpers.ScaledDummy(5);
                    }

                    ImGui.TextUnformatted($"Fate: {bunny.Name}");
                    if (bunny.Alive)
                    {
                        ImGui.TextColored(ImGuiColors.HealerGreen, Loc.Localize("Bunny Window - Status Alive", "Alive"));
                    }
                    else
                    {
                        ImGui.TextColored(ImGuiColors.ParsedGold, Loc.Localize("Bunny Window - Respawning", "Respawning in:"));
                        ImGui.SameLine();
                        if (bunny.LastSeenAlive != -1)
                        {
                            var min = TimeSpan.FromSeconds(bunny.LastSeenAlive + MinimalBunnyRespawn - currentTime);
                            var max = TimeSpan.FromSeconds(bunny.LastSeenAlive + MaximumBunnyRespawn - currentTime);

                            if (min.TotalSeconds > 0)
                            {
                                ImGui.TextUnformatted(TimeToFormatted(min));
                            }
                            else
                            {
                                ImGui.TextColored(ImGuiColors.ParsedGold, Loc.Localize("Bunny Window - Respawning soon", " soon "));
                            }

                            ImGui.SameLine();
                            ImGui.TextUnformatted($"(max {TimeToFormatted(max)})");
                        }
                        else
                        {
                            ImGui.TextColored(ImGuiColors.ParsedBlue, Loc.Localize("Bunny Window - Status Unknown", "Unknown"));
                        }
                    }
                    ImGuiHelpers.ScaledDummy(5);
                }

                ImGui.End();
            }
        }

        private void DrawSettingsWindow()
        {
            if (!SettingsVisible)
                return;

            ImGui.SetNextWindowSize(new Vector2(375, 385), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 385), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Eureka Linker", ref settingsVisible, ConfigFlags))
            {
                if (ImGui.BeginTabBar("##setting-tabs"))
                {
                    // Renders General Settings Tab
                    TabGeneral();

                    // Renders Chat Tab
                    TabChat();

                    // Renders Fairy Tab
                    TabFairy();

                    // Renders Bunny Tab
                    TabBunny();

                    // Renders Stats Tab
                    TabStats();

                    // Renders About Tab
                    TabAbout();
#if DEBUG
                    //Renders Debug Tab
                    TabDebug();
#endif

                    ImGui.EndTabBar();
                }
                ImGui.End();
            }
        }

        private void TabGeneral()
        {
            if (ImGui.BeginTabItem($"{Loc.Localize("Tab Header - General", "General")}###general-tab"))
            {
                var changed = false;
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Echo NM", "Echo NM pops"), ref Configuration.EchoNMPop);
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Sound NM", "Play Sound when NM pops"), ref Configuration.PlaySoundEffect);
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Toast NM", "Show Toast when NM pops"), ref Configuration.ShowPopToast);
                if (Configuration.EchoNMPop || Configuration.ShowPopToast)
                {
                    ImGuiHelpers.ScaledDummy(20, 0);
                    ImGui.SameLine();
                    changed |= ImGui.Checkbox(Loc.Localize("Config Option - Short Names", "Use Short Names"), ref Configuration.UseShortNames);
                }

                if (changed)
                    Configuration.Save();

                ImGuiHelpers.ScaledDummy(10);
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(5);
                ImGui.TextUnformatted(Loc.Localize("Config Header - Tracker", "Tracker:"));
                ImGuiHelpers.ScaledDummy(10);

                var ins = instance;
                ImGui.InputText(Loc.Localize("Config Input - URL", "Instance URL"), ref ins, 31);
                if (ins != instance)
                    instance = ins.Length == 6 ? $"https://ffxiv-eureka.com/{ins}" : ins;


                if (!string.IsNullOrEmpty(instance))
                {
                    ImGui.SameLine();
                    if (ImGuiComponents.IconButton(1, FontAwesomeIcon.Clipboard))
                    {
                        ImGui.SetClipboardText(instance);
                    }
                }

                _ = ImGui.InputText(Loc.Localize("Config Input - PW", "Password"), ref password, 50);
                if (!string.IsNullOrEmpty(password))
                {
                    ImGui.SameLine();
                    if (ImGuiComponents.IconButton(2, FontAwesomeIcon.Clipboard))
                    {
                        ImGui.SetClipboardText(password);
                    }
                }

                if (Plugin.PlayerInEureka && string.IsNullOrEmpty(instance) && ImGui.Button(Loc.Localize("Config Button - Start New", "Start New Tracker")))
                {
                    _ = Task.Run(async () =>
                    {
                        (instance, password) = await EurekaTrackerWrapper.WebRequests.CreateNewTracker(Library.TerritoryToTrackerDictionary[Plugin.ClientState.TerritoryType]);
                        Plugin.ProcessCurrentFates(Plugin.ClientState.TerritoryType);
                    });
                }

                if (Instance.Length > 0)
                {
                    if (ImGuiComponents.IconButton(3, FontAwesomeIcon.Globe))
                    {
                        Dalamud.Utility.Util.OpenLink(instance);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(Loc.Localize("Config Tooltip - Open Tracker", "Open Tracker in Browser"));
                    }
                }

                ImGui.EndTabItem();
            }
        }

        private void TabChat()
        {
            if (ImGui.BeginTabItem($"{Loc.Localize("Tab Header - Chat", "Chat")}###chat-tab"))
            {
                var changed = false;
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Open Shout Window", "Show Shout Window"), ref Configuration.ShowPopWindow);
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Randomize Coords", "Randomize Map Coords"), ref Configuration.RandomizeMapCoords);
                changed |= ImGui.Checkbox(Loc.Localize("Config Option - Show PT", "Show PT in Shout Window"), ref Configuration.ShowPullTimer);
                ImGuiComponents.HelpMarker(Loc.Localize("Config Tooltip - Show PT", "Disabling this option will remove time element from the shout message."));

                if (Configuration.ShowPullTimer)
                {
                    ImGuiHelpers.ScaledDummy(20, 0);
                    ImGui.SameLine();
                    changed |= ImGui.Checkbox(Loc.Localize("Config Option - Use ET", "Use Eorzea Time instead"), ref Configuration.UseEorzeaTimer);
                    if (Configuration.UseEorzeaTimer)
                    {
                        ImGuiHelpers.ScaledDummy(20, 0);
                        ImGui.SameLine();
                        changed |= ImGui.Checkbox(Loc.Localize("Config Option - Use 12h", "Use 12-hour Format"), ref Configuration.UseTwelveHourFormat);
                    }
                }

                if (changed)
                {
                    Configuration.Save();
                    Library.Initialize();
                }

                ImGuiHelpers.ScaledDummy(5);
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(5);
                ImGui.TextUnformatted(Loc.Localize("Config Header - Format", "Format:"));
                ImGuiHelpers.ScaledDummy(10);

                string chatFormat = Configuration.ChatFormat;
                ImGui.InputText("##input-chatformat", ref chatFormat, 64);
                if (chatFormat != Configuration.ChatFormat)
                {
                    Configuration.ChatFormat = chatFormat;
                    Configuration.Save();
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
                var deletion = -1;
                if (ImGui.BeginChild("FairyContent", new Vector2(0, -50)))
                {
                    var changed = false;
                    ImGui.TextUnformatted(Loc.Localize("Config Header - Fairy", "Fairy / Elemental"));
                    changed |= ImGui.Checkbox(Loc.Localize("Config Option - Echo Fairy", "Echo Fairy"), ref Configuration.EchoFairies);
                    changed |= ImGui.Checkbox(Loc.Localize("Config Option - Toast Fairy", "Show Toast for Fairy"), ref Configuration.ShowFairyToast);

                    if (changed)
                        Configuration.Save();

                    ImGuiHelpers.ScaledDummy(5);
                    ImGui.Separator();
                    ImGuiHelpers.ScaledDummy(5);

                    if (ImGui.Button(Loc.Localize("Config Button - Fairy All", "Echo All")))
                    {
                        foreach (var fairy in Library.ExistingFairies)
                        {
                            Plugin.EchoFairy(fairy);
                        }
                    }

                    if (ImGui.BeginChild("FairyTable"))
                    {
                        if (ImGui.BeginTable("##ExistingFairiesTable", 2))
                        {
                            ImGui.TableSetupColumn("##location");
                            ImGui.TableSetupColumn("##trash", 0, 0.1f);

                            foreach (var (fairy, idx) in Library.ExistingFairies.Select((val, i) => (val, i)))
                            {
                                var map = (MapLinkPayload) fairy.MapLink.Payloads.First();

                                ImGui.TableNextColumn();
                                if (ImGui.Selectable($"Fairy ({map.XCoord:0.0},  {map.YCoord:0.0})##{idx}"))
                                    Plugin.OpenMap(map);

                                ImGui.TableNextColumn();
                                if (ImGuiComponents.IconButton(idx, FontAwesomeIcon.Trash))
                                    deletion = idx;
                            }
                        }
                        ImGui.EndTable();
                    }
                    ImGui.EndChild();
                }
                ImGui.EndChild();

                if (deletion != -1)
                    Library.ExistingFairies.RemoveAt(deletion);

                ImGuiHelpers.ScaledDummy(5);
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(5);

                if (ImGui.BeginChild("FairyBottomBar", new Vector2(0, 0), false, 0))
                {
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
                if (ImGui.BeginChild("BunnyContent", new Vector2(0, -50)))
                {
                    var circleColor = Configuration.CircleColor;
                    var changed = false;

                    ImGui.TextUnformatted(Loc.Localize("Config Header - Bunny", "Bunny"));
                    if (ImGui.Checkbox(Loc.Localize("Config Option - Bunny Window", "Show Bunny Window on Entry"), ref Configuration.ShowBunnyWindow))
                    {
                        if (Configuration.ShowBunnyWindow && Plugin.PlayerInRelevantTerritory())
                            BunnyVisible = true;
                        changed = true;
                    }
                    ImGuiComponents.HelpMarker(Loc.Localize("Config Tooltip - Bunny Window", "Or use the command '/elbunny'."));

                    changed |= ImGui.Checkbox(Loc.Localize("Config Option - Bunny Low Level Fates", "Only show low level Bunny"), ref Configuration.OnlyEasyBunny);
                    ImGuiComponents.HelpMarker(Loc.Localize("Config Tooltip - Bunny Low Level Fates", "Only shows the low level bunny fate for Pagos and Pyros."));
                    changed |= ImGui.Checkbox(Loc.Localize("Config Option - Draw Circle", "Draw Nearby Circle"), ref Configuration.BunnyCircleDraw);
                    ImGui.SameLine();
                    ImGui.ColorEdit4("##circleColorPicker", ref circleColor, ColorFlags);

                    if (circleColor != Configuration.CircleColor)
                    {
                        circleColor.W = 1; // fix alpha

                        Configuration.CircleColor = circleColor;
                        imguiCircleColor = ImGui.GetColorU32(ImGui.ColorConvertFloat4ToU32(circleColor));
                        changed = true;
                    }
                    ImGuiComponents.HelpMarker(Loc.Localize("Config Tooltip - Draw Circle",
                        "Draws a circle if the player is near a possible chest location.\nSupports currently: Hydatos, low level Pyros and Pagos"));

                    if (changed)
                        Configuration.Save();

                    ImGuiHelpers.ScaledDummy(5);
                    ImGui.Separator();
                    ImGuiHelpers.ScaledDummy(5);

                    if (ImGui.Button(Loc.Localize("Config Button - Circle Preview", "Preview")))
                        PreviewTimer.Start();

                    if (PreviewTimer.Enabled)
                        PreviewCircle();
                }
                ImGui.EndChild();

                ImGuiHelpers.ScaledDummy(5);
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(5);

                if (ImGui.BeginChild("BunnyBottomBar", new Vector2(0, 0), false, 0))
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.ParsedBlue);
                    if (ImGui.Button(Loc.Localize("Config Button - Add Map Markers", "Add Markers")))
                        Plugin.AddChestsLocationsMap();
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

        private void TabStats()
        {
            var secondRow = ImGui.CalcTextSize("Killed Bunnies: ").X + 120.0f;

            if (ImGui.BeginTabItem($"{Loc.Localize("Tab Header - Stats", "Stats")}##stats-tab"))
            {
                ImGuiHelpers.ScaledDummy(5.0f);
                ImGui.TextUnformatted(Loc.Localize("Config Header - Total Stats", "Total Stats:"));
                ImGui.Indent(10.0f);

                var span = TimeSpan.FromMilliseconds(Configuration.TimeInEureka + Plugin.EurekaWatch.ElapsedMilliseconds);
                var text = $"{(int) span.TotalHours:###00}:{span:mm} h";
                ImGui.TextUnformatted(Loc.Localize("Stats Entry - Time", "Time in Eureka:"));
                ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
                ImGui.TextColored(ImGuiColors.DalamudOrange, text);

                text = $"{Configuration.KilledBunnies}";
                ImGui.TextUnformatted(Loc.Localize("Stats Entry - Killed", "Killed Bunnies:"));
                ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
                ImGui.TextColored(ImGuiColors.DalamudOrange, text);

                var total = Configuration.Stats.Values.Sum(v => v.Values.Sum());
                text = total.ToString();
                ImGui.TextUnformatted(Loc.Localize("Stats Entry - Found", "Coffers Found:"));
                ImGui.SameLine(secondRow - ImGui.CalcTextSize(text).X);
                ImGui.TextColored(ImGuiColors.DalamudOrange, text);

                var bronze = Configuration.Stats.Values.Sum(v => v[2009532]);
                TextWithCalculatedSpacing(Loc.Localize("Stats Entry - Bronze", "Bronze:"), bronze, total);

                var silver = Configuration.Stats.Values.Sum(v => v[2009531]);
                TextWithCalculatedSpacing(Loc.Localize("Stats Entry - Silver", "Silver:"), silver, total);

                var gold = Configuration.Stats.Values.Sum(v => v[2009530]);
                TextWithCalculatedSpacing(Loc.Localize("Stats Entry - Gold", "Gold:"), gold, total);

                ImGui.Unindent(10.0f);
                ImGuiHelpers.ScaledDummy(10.0f);

                ImGui.TextUnformatted(Loc.Localize("Config Header - Map Stats", "Map Stats:"));
                if (ImGui.BeginTabBar("StatsTabBar"))
                {
                    foreach (var (key, value) in Configuration.Stats)
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

                ImGui.EndTabItem();
            }
        }

        private void TabAbout()
        {
            if (ImGui.BeginTabItem($"{Loc.Localize("Tab Header - About", "About")}##about-tab"))
            {
                if (ImGui.BeginChild("AboutContent", new Vector2(0, -50)))
                {
                    ImGuiHelpers.ScaledDummy(10);

                    ImGui.TextUnformatted(Loc.Localize("About Entry - Author", "Author:"));
                    ImGui.SameLine();
                    ImGui.TextColored(ImGuiColors.ParsedGold, Plugin.Authors);

                    ImGui.TextUnformatted(Loc.Localize("About Entry - Discord", "Discord:"));
                    ImGui.SameLine();
                    ImGui.TextColored(ImGuiColors.ParsedGold, "@infi");

                    ImGui.TextUnformatted(Loc.Localize("About Entry - Version", "Version:"));
                    ImGui.SameLine();
                    ImGui.TextColored(ImGuiColors.ParsedOrange, Plugin.Version);
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
                    List<Library.EurekaFate> list = Plugin.ClientState.TerritoryType switch
                    {
                        732 => Library.AnemosFates,
                        763 => Library.PagosFates,
                        795 => Library.PyrosFates,
                        827 => Library.HydatosFates,
                        _ => Library.AnemosFates
                    };

                    string[] stringList = list.Select(x => x.Name).ToArray();
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
                    {
                        Plugin.NMPop();
                    }

                    if (ImGui.Button($"Test EchoNMPop"))
                    {
                        Plugin.EchoNMPop();
                    }

                    if (ImGui.Button($"Test PlaySoundEffect"))
                    {
                        Plugin.PlaySoundEffect();
                    }

                    if (ImGui.Button($"Open Post Window"))
                    {
                        SetEorzeaTimeWithPullOffset();
                        PopVisible = true;
                    }

                    if (ImGui.Button($"SetFlagMarker"))
                    {
                        Plugin.SetFlagMarker();
                    }

                    ImGui.InputInt("Sound Effect Number", ref soundEffect);
                    if (ImGui.Button("Test Sound"))
                    {
                        Sound.PlayEffect((uint) soundEffect);
                    }

                    if(ImGui.Button("Export Loc"))
                    {
                        var pwd = Directory.GetCurrentDirectory();
                        Directory.SetCurrentDirectory(Plugin.DalamudPluginInterface.AssemblyLocation.DirectoryName!);
                        Loc.ExportLocalizable();
                        Directory.SetCurrentDirectory(pwd);
                    }

                    ImGuiHelpers.ScaledDummy(20);

                    if (ImGui.Button($"Reset"))
                    {
                        Plugin.LastSeenFate = Library.EurekaFate.Empty;
                    }
                }

                ImGui.EndTabItem();
            }
        }

        private void PreviewCircle()
        {
            if (Plugin.ClientState.LocalPlayer == null)
                return;

            Plugin.GameGui.WorldToScreen(Plugin.ClientState.LocalPlayer.Position, out var circlePos);

            var winPos = new Vector2(circlePos.X - 15, circlePos.Y - 15);

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(winPos);
            ImGui.SetNextWindowSize(new Vector2(100, 50));
            if (ImGui.Begin("PreviewPointer##preview", CircleFlags))
            {
                ImGui.GetWindowDrawList().AddCircleFilled(circlePos, 8.0f, imguiCircleColor);
                ImGui.End();
            }
        }

        private void TextWithCalculatedSpacing(string header, int count, int total)
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

        public void StartShoutCountdown()
        {
            ShoutTimer.Stop();
            ShoutTimer.Interval = CountdownForShout;
            ShoutTimer.Start();
        }

        public string CurrentEorzeanPullTime()
        {
            DateTime time = new DateTime().AddMinutes(eorzeaTime);

            return !Configuration.UseTwelveHourFormat ? $"{time:HH:mm}" : $"{time:hh:mm tt}";
        }

        public unsafe void SetEorzeaTimeWithPullOffset()
        {
            var et = DateTimeOffset.FromUnixTimeSeconds(Instance()->EorzeaTime);
            eorzeaTime = et.Hour * 60 + et.Minute + 60; // 60 min ET = 3 min our time
            eorzeaTime = RoundOff(eorzeaTime); // Round it to X0

            if (eorzeaTime > 1440)
            {
                eorzeaTime -= 1440;
            }
        }

        private static int RoundOff (int i) => (int) Math.Round(i / 10.0) * 10;

        private string TimeToFormatted(TimeSpan span)
        {
            return span.ToString(span.TotalSeconds > 59 ? @"%m\ \m\i\n" : @"%s\ \s\e\c");
        }
    }
}
