using ImGuiNET;
using System;
using System.Numerics;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        public bool EchoNMPop => echoNMPop;
        public bool PlaySoundEffect => playSoundEffect;
        public uint SoundEffect => (uint)soundEffect;

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
                ImGui.Checkbox("Echo NM pops", ref echoNMPop);
                ImGui.Checkbox("Play Sound when NM pops", ref playSoundEffect);
                _ = ImGui.InputText("Instance", ref instance, 6);
                if (!string.IsNullOrEmpty(instance))
                {
                    ImGui.SameLine();
                    if (Dalamud.Interface.Components.ImGuiComponents.IconButton(1, Dalamud.Interface.FontAwesomeIcon.Clipboard))
                    {
                        ImGui.SetClipboardText(instance);
                    }
                }
                _ = ImGui.InputText("Password", ref password, 50);
                if (!string.IsNullOrEmpty(password))
                {
                    ImGui.SameLine();
                    if (Dalamud.Interface.Components.ImGuiComponents.IconButton(2, Dalamud.Interface.FontAwesomeIcon.Clipboard))
                    {
                        ImGui.SetClipboardText(password);
                    }
                }
                if (Plugin.PlayerInEureka && string.IsNullOrEmpty(instance) && ImGui.Button("Start New Tracker"))
                {
                    Task.Run(() =>
                    {
                        bool previousSoundEffectSetting = playSoundEffect;
                        bool previousEchoNMPopSetting = echoNMPop;
                        (instance, password) = EurekaTrackerWrapper.WebRequests.CreateNewTracker(Library.TerritoryToTrackerDictionary[Plugin.ClientState.TerritoryType]);
                        playSoundEffect = false;
                        echoNMPop = false;
                        Plugin.ProcessCurrentFates(Plugin.ClientState.TerritoryType);
                        playSoundEffect = previousSoundEffectSetting;
                        echoNMPop = previousEchoNMPopSetting;
                    });
                }
                if (Instance.Length > 0)
                {
                    if (Dalamud.Interface.Components.ImGuiComponents.IconButton(Dalamud.Interface.FontAwesomeIcon.Globe))
                    {
                        _ = Process.Start(new ProcessStartInfo()
                        {
                            FileName = $"https://ffxiv-eureka.com/{instance}",
                            UseShellExecute = true
                        });
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Open Tracker in Browser");
                    }
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
    }
}
