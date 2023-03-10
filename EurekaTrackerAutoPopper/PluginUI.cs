using ImGuiNET;
using System;
using System.Numerics;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Dalamud.Interface;
using System.Collections.Generic;

namespace EurekaTrackerAutoPopper
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    internal class PluginUI : IDisposable
    {
        private const ImGuiWindowFlags flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse |
                                               ImGuiWindowFlags.NoCollapse;
        private const ImGuiWindowFlags popFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize;

        private Configuration Configuration { get; init; }
        private Plugin Plugin { get; init; }
        private Library Library { get; init; }

        private bool settingsVisible = false;
        private string instance = "";
        private string password = "";
        private readonly int soundEffect = 36;
        private int pullTime = 27;
        private int eorzeaTime = 720;

        public uint SoundEffect => (uint)soundEffect;
        public int PullTime => pullTime;
        public int DebugFate = 0;

        public bool SettingsVisible
        {
            get => settingsVisible;
            set => settingsVisible = value;
        }

        public bool PopVisible { get; set; } = false;

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
        }

        public void Dispose()
        {
            Configuration.Save();
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.
            DrawSettingsWindow();
            DrawFatePopWindow();
        }

        public void DrawFatePopWindow()
        {
            if (!PopVisible)
            {
                return;
            }

            if (Plugin.LastSeenFate == null)
            {
                PopVisible = false;
                return;
            }

            if (ImGui.Begin("Eureka NM Pop", popFlags))
            {
                string fate = $"Fate: {Plugin.LastSeenFate.name}";
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
                if (ImGui.Button("Post", new Vector2(50, 0)))
                {
                    Plugin.PostChatMessage();
                    PopVisible = false;
                }

                ImGui.SameLine(size + 85 + extraSize);
                if (ImGui.Button("Close", new Vector2(50, 0)))
                {
                    PopVisible = false;
                }

                ImGui.End();
            }
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 385), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 385), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Eureka Tracker Auto Popper", ref settingsVisible, flags))
            {
                if (ImGui.BeginTabBar("##setting-tabs"))
                {
                    // Renders General Settings Tab
                    TabGeneral();

                    // Renders Chat Tab
                    TabChat();
                    
                    // Renders Fairy Tab
                    TabFairy();

#if DEBUG
                    //Renders Debug Tab
                    TabDebug();
#endif

                    ImGui.EndTabBar();
                }
#if DEBUG
                //ImGui.InputInt("Sound Effect Number", ref soundEffect);
                //if (ImGui.Button("Test Sound"))
                //{
                //    Plugin.PlaySoundEffect((uint)soundEffect);
                //}
                //if (Library.TerritoryToFateDictionary.ContainsKey(Plugin.ClientState.TerritoryType))
                //{
                //    List<Library.EurekaFate> fates = Library.TerritoryToFateDictionary[Plugin.ClientState.TerritoryType];
                //    foreach (Library.EurekaFate fate in fates)
                //    {
                //        if (ImGui.Button($"Test Pop Echo for {fate.name}"))
                //        {
                //            Plugin.EchoNMPop(fate);
                //        }
                //    }
                //}
#endif
                ImGui.End();
            }
        }

        public void TabGeneral()
        {
            if (ImGui.BeginTabItem("General###general-tab"))
            {
                _ = ImGui.Checkbox("Echo NM pops", ref Configuration.EchoNMPop);
                _ = ImGui.Checkbox("Play Sound when NM pops", ref Configuration.PlaySoundEffect);
                _ = ImGui.Checkbox("Show Toast when NM pops", ref Configuration.ShowPopToast);
                if (Configuration.EchoNMPop || Configuration.ShowPopToast)
                {
                    ImGuiHelpers.ScaledDummy(20, 0);
                    ImGui.SameLine();
                    _ = ImGui.Checkbox("Use Short Names", ref Configuration.UseShortNames);
                }

                ImGuiHelpers.ScaledDummy(10);
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(5);
                ImGui.TextUnformatted("Tracker:");
                ImGuiHelpers.ScaledDummy(10);

                _ = ImGui.InputText("Instance URL", ref instance, 31);
                if (!string.IsNullOrEmpty(instance))
                {
                    ImGui.SameLine();
                    if (Dalamud.Interface.Components.ImGuiComponents.IconButton(1, FontAwesomeIcon.Clipboard))
                    {
                        ImGui.SetClipboardText(instance);
                    }
                }
                _ = ImGui.InputText("Password", ref password, 50);
                if (!string.IsNullOrEmpty(password))
                {
                    ImGui.SameLine();
                    if (Dalamud.Interface.Components.ImGuiComponents.IconButton(2, FontAwesomeIcon.Clipboard))
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
                    if (Dalamud.Interface.Components.ImGuiComponents.IconButton(3, FontAwesomeIcon.Globe))
                    {
                        _ = Process.Start(new ProcessStartInfo()
                        {
                            FileName = instance,
                            UseShellExecute = true
                        });
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Open Tracker in Browser");
                    }
                }

                ImGui.EndTabItem();
            }
        }

        public void TabChat()
        {
            if (ImGui.BeginTabItem("Chat###chat-tab"))
            {
                _ = ImGui.Checkbox("Show Post Window", ref Configuration.ShowPopWindow);
                bool randomize = Configuration.RandomizeMapCoords;
                if (ImGui.Checkbox("Randomize Map Coords", ref randomize))
                {
                    Configuration.RandomizeMapCoords = randomize;
                    Configuration.Save();
                    
                    Library.Initialize();
                }
                _ = ImGui.Checkbox("Show PT in Post Window", ref Configuration.ShowPullTimer);

                if (Configuration.ShowPullTimer)
                {
                    ImGuiHelpers.ScaledDummy(20, 0);
                    ImGui.SameLine();
                    _ = ImGui.Checkbox("Use Eorzea Time instead", ref Configuration.UseEorzeaTimer);
                    if (Configuration.UseEorzeaTimer)
                    {
                        ImGuiHelpers.ScaledDummy(20, 0);
                        ImGui.SameLine();
                        _ = ImGui.Checkbox("Use 12-hour Format", ref Configuration.UseTwelveHourFormat);
                    }
                }

                ImGuiHelpers.ScaledDummy(5);
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(5);
                ImGui.TextUnformatted("Format:");
                ImGuiHelpers.ScaledDummy(10);

                string chatFormat = Configuration.ChatFormat;
                _ = ImGui.InputText("##input-chatformat", ref chatFormat, 30);
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
        
        public void TabFairy()
        {
            if (ImGui.BeginTabItem("Fairy###fairy-tab"))
            {
                ImGui.TextUnformatted("Fairy / Elemental");
                _ = ImGui.Checkbox("Echo Fairies", ref Configuration.EchoFairies);
                _ = ImGui.Checkbox("Show Toast for Fairies", ref Configuration.ShowFairyToast);
                
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

        public void TabDebug()
        {
            if (ImGui.BeginTabItem("Debug###debug-tab"))
            {
                if (Plugin.LastSeenFate == null)
                {
                    List<Library.EurekaFate> list = Plugin.ClientState.TerritoryType switch
                    {
                        732 => Library.AnemosFates,
                        763 => Library.PagosFates,
                        795 => Library.PyrosFates,
                        827 => Library.HydatosFates,
                        _ => Library.AnemosFates
                    };

                    string[] stringList = list.Select(x => x.name).ToArray();
                    _ = ImGui.Combo("##fateSelector", ref DebugFate, stringList, stringList.Length);

                    if (ImGui.Button($"Populate last seen fate"))
                    {
                        Plugin.LastSeenFate = list[DebugFate];
                    }
                }
                else
                {
                    ImGui.TextUnformatted($"Current Fate: {Plugin.LastSeenFate.name}");

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

                    if (ImGui.Button($"Reset"))
                    {
                        Plugin.LastSeenFate = null;
                    }
                }

                ImGui.EndTabItem();
            }
        }

        public string CurrentEorzeanPullTime()
        {
            DateTime time = new DateTime().AddMinutes(eorzeaTime);

            return !Configuration.UseTwelveHourFormat ? $"{time:HH:mm}" : $"{time:hh:mm tt}";
        }

        public void SetEorzeaTimeWithPullOffset()
        {
            unsafe
            {
                var et = DateTimeOffset.FromUnixTimeSeconds(FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->EorzeaTime);
                eorzeaTime = et.Hour * 60 + et.Minute + 60; // 60 min ET = 3 min our time
                eorzeaTime = RoundOff(eorzeaTime); // Round it to X0
                
                if (eorzeaTime > 1440)
                {
                    eorzeaTime -= 1440;
                }
            }
        }
        
        private static int RoundOff (int i) => (int) Math.Round(i / 10.0) * 10;
    }
}
