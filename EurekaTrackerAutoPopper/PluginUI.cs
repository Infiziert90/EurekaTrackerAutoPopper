﻿using ImGuiNET;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Linq;
using Dalamud.Interface;
using System.Collections.Generic;
using System.Timers;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;

using static FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;
using static ImGuiNET.ImGuiWindowFlags;

namespace EurekaTrackerAutoPopper
{
    internal class PluginUI : IDisposable
    {
        private const ImGuiWindowFlags ConfigFlags = NoScrollbar | NoScrollWithMouse | NoCollapse;
        private const ImGuiWindowFlags BunnyFlags = AlwaysAutoResize;
        private const ImGuiWindowFlags PopFlags = NoDecoration | AlwaysAutoResize;
        private const ImGuiWindowFlags CircleFlags = NoBackground | NoMove | NoTitleBar | NoScrollbar | NoResize | NoInputs;

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

        private const int MinimalBunnyRespawn = 530;
        private const int MaximumBunnyRespawn = 1000;

        public bool NearToBunnyChest = false;
        public Vector3 ChestPos = Vector3.Zero;
        private uint green = ImGui.GetColorU32(ImGui.ColorConvertFloat4ToU32(ImGuiColors.HealerGreen));

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

            BunnyVisible = Configuration.ShowBunnyWindow;
            ShoutTimer.AutoReset = false;
        }

        public void Dispose() { }

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

            if (!NearToBunnyChest)
                return;

            if (ChestPos == Vector3.Zero)
                return;

            Plugin.GameGui.WorldToScreen(ChestPos, out var circlePos);

            var winPos = new Vector2(circlePos.X - 15, circlePos.Y - 15);

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(winPos);
            ImGui.SetNextWindowSize(new Vector2(100, 50));
            if (ImGui.Begin("Pointer##bunny-chest", CircleFlags))
            {
                ImGui.GetWindowDrawList().AddCircleFilled(circlePos, 5.0f, green);
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
                    if (ImGui.Button("Post", new Vector2(50, 0)))
                    {
                        Plugin.PostChatMessage();
                        PopVisible = false;
                    }
                }
                else
                {
                    ImGui.BeginDisabled();
                    _ = ImGui.Button($"Post", new Vector2(50, 0));
                    ImGui.EndDisabled();

                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip($"Shout will be available {CountdownForShout / 1000}s after spawn.\nPlease don't shout if it was already shouted.");
                    }
                }

                ImGui.SameLine(size + 85 + extraSize);
                if (ImGui.Button("Close", new Vector2(50, 0)))
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
                        ImGui.TextColored(ImGuiColors.HealerGreen, "Alive");
                    }
                    else
                    {
                        ImGui.TextColored(ImGuiColors.ParsedGold, "Respawning in:");
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
                                ImGui.TextColored(ImGuiColors.ParsedGold, " soon ");
                            }

                            ImGui.SameLine();
                            ImGui.TextUnformatted($"(max {TimeToFormatted(max)})");
                        }
                        else
                        {
                            ImGui.TextColored(ImGuiColors.ParsedBlue, "Unknown");
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
            if (ImGui.BeginTabItem("General###general-tab"))
            {
                var changed = false;
                changed |= ImGui.Checkbox("Echo NM pops", ref Configuration.EchoNMPop);
                changed |= ImGui.Checkbox("Play Sound when NM pops", ref Configuration.PlaySoundEffect);
                changed |= ImGui.Checkbox("Show Toast when NM pops", ref Configuration.ShowPopToast);
                if (Configuration.EchoNMPop || Configuration.ShowPopToast)
                {
                    ImGuiHelpers.ScaledDummy(20, 0);
                    ImGui.SameLine();
                    changed |= ImGui.Checkbox("Use Short Names", ref Configuration.UseShortNames);
                }

                if (changed)
                    Configuration.Save();

                ImGuiHelpers.ScaledDummy(10);
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(5);
                ImGui.TextUnformatted("Tracker:");
                ImGuiHelpers.ScaledDummy(10);

                _ = ImGui.InputText("Instance URL", ref instance, 31);
                if (!string.IsNullOrEmpty(instance))
                {
                    ImGui.SameLine();
                    if (ImGuiComponents.IconButton(1, FontAwesomeIcon.Clipboard))
                    {
                        ImGui.SetClipboardText(instance);
                    }
                }
                _ = ImGui.InputText("Password", ref password, 50);
                if (!string.IsNullOrEmpty(password))
                {
                    ImGui.SameLine();
                    if (ImGuiComponents.IconButton(2, FontAwesomeIcon.Clipboard))
                    {
                        ImGui.SetClipboardText(password);
                    }
                }
                if (Plugin.PlayerInEureka && string.IsNullOrEmpty(instance) && ImGui.Button("Start New Tracker"))
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
                        ImGui.SetTooltip("Open Tracker in Browser");
                    }
                }

                ImGui.EndTabItem();
            }
        }

        private void TabChat()
        {
            if (ImGui.BeginTabItem("Chat###chat-tab"))
            {
                var changed = false;
                changed |= ImGui.Checkbox("Show Shout Window", ref Configuration.ShowPopWindow);
                changed |= ImGui.Checkbox("Randomize Map Coords", ref Configuration.RandomizeMapCoords);
                changed |= ImGui.Checkbox("Show PT in Shout Window", ref Configuration.ShowPullTimer);

                if (Configuration.ShowPullTimer)
                {
                    ImGuiHelpers.ScaledDummy(20, 0);
                    ImGui.SameLine();
                    changed |= ImGui.Checkbox("Use Eorzea Time instead", ref Configuration.UseEorzeaTimer);
                    if (Configuration.UseEorzeaTimer)
                    {
                        ImGuiHelpers.ScaledDummy(20, 0);
                        ImGui.SameLine();
                        changed |= ImGui.Checkbox("Use 12-hour Format", ref Configuration.UseTwelveHourFormat);
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
                ImGui.TextUnformatted("Format:");
                ImGuiHelpers.ScaledDummy(10);

                string chatFormat = Configuration.ChatFormat;
                ImGui.InputText("##input-chatformat", ref chatFormat, 30);
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
            if (ImGui.BeginTabItem("Fairy###fairy-tab"))
            {
                var changed = false;
                ImGui.TextUnformatted("Fairy / Elemental");
                changed |= ImGui.Checkbox("Echo Fairies", ref Configuration.EchoFairies);
                changed |= ImGui.Checkbox("Show Toast for Fairies", ref Configuration.ShowFairyToast);

                if (changed)
                    Configuration.Save();

                ImGuiHelpers.ScaledDummy(5);
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(5);

                if (ImGui.Button("Echo All"))
                {
                    foreach (var fairy in Library.ExistingFairies.Values)
                    {
                        Plugin.EchoFairy(fairy);
                    }
                }

                ImGui.EndTabItem();
            }
        }

        private void TabBunny()
        {
            if (ImGui.BeginTabItem("Bunny###bunny-tab"))
            {
                var changed = false;
                ImGui.TextUnformatted("Bunny");
                if (ImGui.Checkbox("Show Bunny Window", ref Configuration.ShowBunnyWindow))
                {
                    if (Configuration.ShowBunnyWindow)
                        bunnyVisible = true;
                    Configuration.Save();
                }

                changed |= ImGui.Checkbox("Only Easy Bunny", ref Configuration.OnlyEasyBunny);
                ImGuiComponents.HelpMarker("Only shows the low level bunny fate for Pagos and Pyros.");
                changed |= ImGui.Checkbox("Draw Nearby Circle", ref Configuration.BunnyCircleDraw);
                ImGuiComponents.HelpMarker("Draws a green circle if the player is near a possible chest location." +
                                           "\nSupports currently: Hydatos, small of of low level Pyros and Pagos");

                if (changed)
                    Configuration.Save();

                ImGui.EndTabItem();
            }
        }

        private void TabDebug()
        {
            if (ImGui.BeginTabItem("Debug###debug-tab"))
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

                    ImGuiHelpers.ScaledDummy(20);

                    if (ImGui.Button($"Reset"))
                    {
                        Plugin.LastSeenFate = Library.EurekaFate.Empty;
                    }
                }

                ImGui.EndTabItem();
            }
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
