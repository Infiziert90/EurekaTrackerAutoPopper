using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.Reflection;
using Dalamud.Game.Gui;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState;
using Dalamud.Game;
using System.Threading;
using Dalamud.Game.Gui.Toast;
using ImGuiNET;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using Dalamud.Logging;
using XivCommon;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace EurekaTrackerAutoPopper
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Eureka Tracker Auto Popper";

        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        private List<Fate> lastPolledFates = new();
        public bool PlayerInEureka { get; set; } = false;
        public Library.EurekaFate LastSeenFate = null;
        
        private static XivCommonBase xivCommon;
        
        [PluginService] public static ChatGui Chat { get; private set; } = null!;
        [PluginService] public static GameGui GameGui { get; private set; } = null!;
        [PluginService] public static ToastGui Toast { get; private set; } = null!;
        [PluginService] public static Dalamud.Data.DataManager DataManager { get; private set; } = null!;
        [PluginService] public static FateTable FateTable { get; private set; } = null!;
        [PluginService] public static Framework Framework { get; private set; } = null!;
        [PluginService] public static ClientState ClientState { get; private set; } = null!;
        [PluginService] public static DalamudPluginInterface DalamudPluginInterface { get; private set; } = null!;
        [PluginService] public static CommandManager CommandManager { get; private set; } = null!;
        public Plugin()
        {
            Configuration = DalamudPluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(DalamudPluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            string? assemblyLocation = Assembly.GetExecutingAssembly().Location;
            PluginUi = new PluginUI(Configuration, this);

            _ = CommandManager.AddHandler("/xleureka", new CommandInfo(OnEurekaCommand)
            {
                HelpMessage = "Opens the config window",
                ShowInHelp = true
            });
            
            DalamudPluginInterface.UiBuilder.Draw += DrawUI;
            DalamudPluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            ClientState.TerritoryChanged += TerritoryChangePoll;
            xivCommon = new XivCommonBase();
            
            if (PlayerInRelevantTerritory())
            {
                PlayerInEureka = true;
                Framework.Update += PollForFateChange;
            }
        }

        private void OnEurekaCommand(string command, string arguments)
        {
            DrawConfigUI();
        }
        
        private void TerritoryChangePoll(object? sender, ushort territoryId)
        {
            if (PlayerInRelevantTerritory())
            {
                PlayerInEureka = true;
                _ = Task.Run(async () =>
                  {
                      Task<string?> task = EurekaTrackerWrapper.WebRequests.FindTracker(GetDataCenterId(), Library.TerritoryToTrackerDictionary[ClientState.TerritoryType]);

                      string? instance = await task;

                      if (!string.IsNullOrEmpty(instance))
                      {
                          Chat.PrintChat(new XivChatEntry { Message = $"[{Name}] Join existing tracker: https://ffxiv-eureka.com/{instance}" });
                          PluginUi.Instance = instance;
                          ImGui.SetClipboardText($"https://ffxiv-eureka.com/{instance}");
                      }
                  });
                Framework.Update += PollForFateChange;
            }
            else
            {
                PlayerInEureka = false;
                PluginUi.Instance = "";
                PluginUi.Password = "";
                Framework.Update -= PollForFateChange;
            }
        }

        private static uint GetDataCenterId()
        {
            // keep trying this method until we get the local player
            // maybe a better way, but *shrug* for now
            Thread.Sleep(500);
            return ClientState.LocalPlayer?.CurrentWorld.GameData?.DataCenter.Row ?? GetDataCenterId();
        }

        private static bool PlayerInRelevantTerritory()
        {
            return Library.TerritoryToFateDictionary.Keys.Contains(ClientState.TerritoryType);
        }

        private bool NoFatesHaveChangedSinceLastChecked()
        {
            return FateTable.SequenceEqual(lastPolledFates);
        }

        private void CheckForRelevantFates(ushort currentTerritory)
        {
            List<Fate> newFates = FateTable.Except(lastPolledFates).ToList();
            List<Library.EurekaFate> relevantFates = Library.TerritoryToFateDictionary[currentTerritory];
            List<Library.EurekaFate> newRelevantFates = relevantFates.Where(i => newFates.Select(i => i.FateId).Contains(i.fateId)).ToList();
            foreach (Library.EurekaFate fate in newRelevantFates)
            {
                LastSeenFate = fate;
                PluginUi.SetEorzeaTimeWithPullOffset();
                ProcessNewFate(fate);
            }
        }

        public void ProcessCurrentFates(ushort currentTerritory)
        {
            List<Fate> currentFates = FateTable.ToList();
            List<Library.EurekaFate> relevantFates = Library.TerritoryToFateDictionary[currentTerritory];
            List<Library.EurekaFate> relevantCurrentFates = relevantFates.Where(i => currentFates.Select(i => i.FateId).Contains(i.fateId)).ToList();
            foreach (Library.EurekaFate fate in relevantCurrentFates)
            {
                if (fate.trackerId != null && !string.IsNullOrEmpty(PluginUi.Instance) && !string.IsNullOrEmpty(PluginUi.Password))
                {
                    EurekaTrackerWrapper.WebRequests.PopNM((ushort)fate.trackerId, PluginUi.Instance, PluginUi.Password);
                }
            }
        }

        public void ProcessNewFate(Library.EurekaFate fate)
        {
            EchoNMPop();
            PlaySoundEffect();
            if (PluginUi.ShowPopWindow)
            {
                PluginUi.PopVisible = true;
            }
            
            if (fate.trackerId != null && !string.IsNullOrEmpty(PluginUi.Instance) && !string.IsNullOrEmpty(PluginUi.Password))
            {
                EurekaTrackerWrapper.WebRequests.PopNM((ushort)fate.trackerId, PluginUi.Instance, PluginUi.Password);
            }
        }

        public void PlaySoundEffect()
        {
            if (PluginUi.PlaySoundEffect)
            {
                Sound.PlayEffect(PluginUi.SoundEffect);
            }
        }

        public void EchoNMPop()
        {
            SeString payload = new();
            _ = payload.Append($"{(PluginUi.UseShortNames ? LastSeenFate.shortName : LastSeenFate.name)} pop: ");
            _ = payload.Append(LastSeenFate.mapLink);
            
            if (PluginUi.EchoNMPop)
            {
                Chat.PrintChat(new XivChatEntry { Message = payload });
            }

            if (PluginUi.ShowPopToast)
            {
                Toast.ShowQuest(payload);
            }
        }
        
        public string BuildChatString()
        {
            var time = !PluginUi.UseEorzeaTimer ? $"PT {PluginUi.PullTime}" : $"ET {PluginUi.CurrentTimePullTime()}";
            var output = Configuration.ChatFormat
                .Replace("$n", LastSeenFate.name)
                .Replace("$sN", LastSeenFate.shortName)
                .Replace("$t", time)
                .Replace("$p", "<flag>");

            return output;
        }

        public void PostChatMessage()
        {
            SetFlagMarker();
            xivCommon.Functions.Chat.SendMessage(BuildChatString());
        }
        

        // not going to set data center until I can figure some things out
        //private async void SetDataCenter()
        //{
        //    uint? dataCenterId = ClientState.LocalPlayer?.CurrentWorld.GameData?.DataCenter.Row;
        //    // keep trying this method until we get the local player
        //    if (dataCenterId == null)
        //    {
        //        Thread.Sleep(500);
        //        SetDataCenter();
        //        return;
        //    }
        //    ClientWebSocket socket = new();
        //    await socket.ConnectAsync(new Uri("wss://ffxiv-eureka.com/socket/websocket?vsn=2.0.0"), CancellationToken.None);
        //    await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"1\",\"1\",\"instance:{PluginUi.Instance}\",\"phx_join\",{{\"password\":\"{PluginUi.Password}\"}}]"), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
        //    await socket.SendAsync(Encoding.UTF8.GetBytes($"[\"2\",\"2\",\"instance:{PluginUi.Instance}\",\"set_instance_information\",{{\"data_center_id\":{dataCenterId}}}]"), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
        //    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        //}

        public void PollForFateChange(Framework framework)
        {
            if (NoFatesHaveChangedSinceLastChecked())
            {
                return;
            }

            CheckForRelevantFates(ClientState.TerritoryType);
            lastPolledFates = FateTable.ToList();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Won't change")]
        public void Dispose()
        {
            PluginUi.Dispose();
            Framework.Update -= PollForFateChange;
            ClientState.TerritoryChanged -= TerritoryChangePoll;
            xivCommon.Dispose();
            _ = CommandManager.RemoveHandler("/xleureka");
        }

        private void DrawUI()
        {
            PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            PluginUi.SettingsVisible = true;
        }
        
        public unsafe void SetFlagMarker()
        {
            try
            {
                PluginLog.Debug("SetFlagMarker");
                // removes current flag marker from map
                AgentMap.Instance()->IsFlagMarkerSet = 0;
                // divide by 1000 as raw is too long for CS SetFlagMapMarker
                AgentMap.Instance()->SetFlagMapMarker(
                    LastSeenFate.mapLink.territoryId, 
                    LastSeenFate.mapLink.mapId,
                    LastSeenFate.mapLink.payload.RawX / 1000.0f, 
                    LastSeenFate.mapLink.payload.RawY / 1000.0f);
            } 
            catch (Exception)
            {
                PluginLog.Error("Exception during SetFlagMarker");
            }
        }
    }
}
