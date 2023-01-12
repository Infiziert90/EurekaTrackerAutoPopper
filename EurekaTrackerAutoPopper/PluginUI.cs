using ImGuiNET;
using System;
using System.Numerics;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Dalamud.Interface;

namespace EurekaTrackerAutoPopper
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    internal class PluginUI : IDisposable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Won't change")]
        private Configuration Configuration { get; init; }
        private Plugin Plugin { get; init; }

        private bool settingsVisible = false;
        private string instance = "";
        private string password = "";
        private bool echoNMPop = true;
        private bool playSoundEffect = true;
        private int soundEffect = 36;
        private bool showPopToast = true;
        private bool copyChatFormat = true;
        private bool useShortNames = true;

        public bool EchoNMPop => echoNMPop;
        public bool PlaySoundEffect => playSoundEffect;
        public uint SoundEffect => (uint)soundEffect;

        public bool ShowPopToast => showPopToast;
        public bool CopyChatFormat => copyChatFormat;
        public bool UseShortNames => useShortNames;

        public bool SettingsVisible
        {
            get => settingsVisible;
            set => settingsVisible = value;
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

        public PluginUI(Configuration configuration, Plugin plugin)
        {
            Configuration = configuration;
            Plugin = plugin;
        }

        public void Dispose()
        {
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
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Eureka Tracker Auto Popper", ref settingsVisible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse))
            {
                if (ImGui.BeginTabBar("##setting-tabs"))
                {
                    // Renders General Settings Tab
                    TabGeneral();

                    // Renders Chat Tab
                    TabChat();

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
                ImGui.Checkbox("Echo NM pops", ref echoNMPop);
                ImGui.Checkbox("Play Sound when NM pops", ref playSoundEffect);
                ImGui.Checkbox("Show Toast when NM pops", ref showPopToast);
                if (echoNMPop || showPopToast)
                {
                    ImGui.Checkbox("Show Short Names", ref useShortNames);
                }

                ImGuiHelpers.ScaledDummy(10);
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(5);
                ImGui.TextUnformatted("Tracker:");
                ImGuiHelpers.ScaledDummy(10);
                
                _ = ImGui.InputText("Instance", ref instance, 6);
                if (!string.IsNullOrEmpty(instance))
                {
                    ImGui.SameLine();
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Clipboard.ToIconString()}##instance_copy"))
                    {
                        ImGui.SetClipboardText(instance);
                    }
                    ImGui.PopFont();
                }
                _ = ImGui.InputText("Password", ref password, 50);
                if (!string.IsNullOrEmpty(password))
                {
                    ImGui.SameLine();
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Clipboard.ToIconString()}##password_copy"))
                    {
                        ImGui.SetClipboardText(password);
                    }
                    ImGui.PopFont();
                }
                if (Plugin.PlayerInEureka && string.IsNullOrEmpty(instance) && ImGui.Button("Start New Tracker"))
                {
                    Task.Run(async () =>
                    {
                        bool previousSoundEffectSetting = playSoundEffect;
                        bool previousEchoNMPopSetting = echoNMPop;
                        (instance, password) = await EurekaTrackerWrapper.WebRequests.CreateNewTracker(Library.TerritoryToTrackerDictionary[Plugin.ClientState.TerritoryType]);
                        playSoundEffect = false;
                        echoNMPop = false;
                        Plugin.ProcessCurrentFates(Plugin.ClientState.TerritoryType);
                        playSoundEffect = previousSoundEffectSetting;
                        echoNMPop = previousEchoNMPopSetting;
                    });
                }
                if (Instance.Length > 0)
                {
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Globe.ToIconString()}##globe_btn"))
                    {
                        _ = Process.Start(new ProcessStartInfo()
                        {
                            FileName = $"https://ffxiv-eureka.com/{instance}",
                            UseShellExecute = true
                        });
                    }
                    ImGui.PopFont();
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
                ImGui.Checkbox("Copy when NM pops", ref copyChatFormat);
                
                ImGuiHelpers.ScaledDummy(5);
                ImGui.TextUnformatted("Format:");
                ImGuiHelpers.ScaledDummy(10);
                
                var chatFormat = Configuration.ChatFormat;
                ImGui.InputText("##input-chatformat", ref chatFormat, 30);
                if (chatFormat != Configuration.ChatFormat)
                {
                    Configuration.ChatFormat = chatFormat;
                    Configuration.Save();
                }
                
                ImGuiHelpers.ScaledDummy(5);
                
                ImGui.TextUnformatted("$n = Full Name");
                ImGui.TextUnformatted("$sN = Short Name");
                ImGui.TextDisabled("$p = MapFlag (disabled)");
                
                ImGuiHelpers.ScaledDummy(10);
                
                if (Plugin.LastSeenFate != null)
                {
                    ImGui.TextUnformatted("Last seen fate:");
                    ImGui.TextUnformatted(Plugin.LastSeenFate.name);
                    ImGui.SameLine();
                    ImGui.PushFont(UiBuilder.IconFont);
                    if (ImGui.Button($"{FontAwesomeIcon.Clipboard.ToIconString()}##chat_copy"))
                    {
                        ImGui.SetClipboardText(Plugin.BuildChatString());
                    }
                    ImGui.PopFont();
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
                    if (ImGui.Button($"Populate last seen fate"))
                    {
                        Plugin.LastSeenFate = Library.AnemosFates.Last();
                    }
                }
                else
                {
                    if (ImGui.Button($"Test EchoNMPop"))
                    {
                        Plugin.EchoNMPop();
                    }
                    
                    if (ImGui.Button($"Test PlaySoundEffect"))
                    {
                        Plugin.PlaySoundEffect();
                    }
                    
                    if (ImGui.Button($"Test CopyNMPop"))
                    {
                        Plugin.CopyNMPop();
                    }
                }
                
                ImGui.EndTabItem();
            }
        }
    }
}
