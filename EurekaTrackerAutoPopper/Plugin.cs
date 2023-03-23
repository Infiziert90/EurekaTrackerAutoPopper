using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Gui;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using Dalamud.Logging;
using XivCommon;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace EurekaTrackerAutoPopper
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Eureka Linker";

        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        public Library Library;
        public bool PlayerInEureka;
        public Library.EurekaFate LastSeenFate = null!;
        private List<Fate> lastPolledFates = new();

        private static XivCommonBase xivCommon = null!;

        [PluginService] public static ChatGui Chat { get; private set; } = null!;
        [PluginService] public static ToastGui Toast { get; private set; } = null!;
        [PluginService] public static ObjectTable ObjectTable { get; private set; } = null!;
        [PluginService] public static FateTable FateTable { get; private set; } = null!;
        [PluginService] public static Framework Framework { get; private set; } = null!;
        [PluginService] public static ClientState ClientState { get; private set; } = null!;
        [PluginService] public static DalamudPluginInterface DalamudPluginInterface { get; private set; } = null!;
        [PluginService] public static CommandManager CommandManager { get; private set; } = null!;

        public Plugin()
        {
            Configuration = DalamudPluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(DalamudPluginInterface);

            Library = new Library(Configuration);
            Library.Initialize();

            PluginUi = new PluginUI(Configuration, this, Library);

            CommandManager.AddHandler("/xleureka", new CommandInfo(OnEurekaCommand)
            {
                HelpMessage = "Opens the config window",
                ShowInHelp = true
            });

            CommandManager.AddHandler("/xlposition", new CommandInfo(WritePlayerPosition)
            {
                HelpMessage = "Debug",
                ShowInHelp = false
            });

            DalamudPluginInterface.UiBuilder.Draw += DrawUI;
            DalamudPluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            ClientState.TerritoryChanged += TerritoryChangePoll;
            xivCommon = new XivCommonBase();

            if (PlayerInRelevantTerritory())
            {
                PlayerInEureka = true;
                Framework.Update += PollForFateChange;
                Framework.Update += FairyCheck;
                Framework.Update += BunnyCheck;
            }
        }

        private void OnEurekaCommand(string command, string arguments)
        {
            DrawConfigUI();
        }

        private void WritePlayerPosition(string command, string arguments)
        {
            var pos = ClientState.LocalPlayer!.Position;
            PluginLog.Information($"X {pos.X} Y {pos.Y} Z {pos.Z}");
        }

        private void TerritoryChangePoll(object? sender, ushort territoryId)
        {
            if (PlayerInRelevantTerritory())
            {
                PlayerInEureka = true;

                Framework.Update += PollForFateChange;
                Framework.Update += FairyCheck;
                Framework.Update += BunnyCheck;
            }
            else
            {
                PlayerInEureka = false;
                PluginUi.Instance = "";
                PluginUi.Password = "";
                Library.ExistingFairies.Clear();
                Library.ResetBunnies();

                Framework.Update -= PollForFateChange;
                Framework.Update -= FairyCheck;
                Framework.Update -= BunnyCheck;
            }
        }

        private bool PlayerInRelevantTerritory()
        {
            return Library.TerritoryToFateDictionary.ContainsKey(ClientState.TerritoryType);
        }

        private bool NoFatesHaveChangedSinceLastChecked()
        {
            return FateTable.SequenceEqual(lastPolledFates);
        }

        private void CheckForRelevantFates(ushort currentTerritory)
        {
            List<ushort> newFateIds = FateTable.Except(lastPolledFates).Select(i => i.FateId).ToList();
            List<Library.EurekaFate> relevantFates = Library.TerritoryToFateDictionary[currentTerritory];
            foreach (Library.EurekaFate fate in relevantFates.Where(i => newFateIds.Contains(i.FateId)))
            {
                LastSeenFate = fate;

                ProcessNewFate(fate);
            }
        }

        public void ProcessCurrentFates(ushort currentTerritory)
        {
            List<Fate> currentFates = FateTable.ToList();
            List<Library.EurekaFate> relevantFates = Library.TerritoryToFateDictionary[currentTerritory];
            List<Library.EurekaFate> relevantCurrentFates = relevantFates.Where(fate => currentFates.Select(i => i.FateId).Contains(fate.FateId)).ToList();
            foreach (Library.EurekaFate fate in relevantCurrentFates)
            {
                if (fate.TrackerId != 1337 && !string.IsNullOrEmpty(PluginUi.Instance) && !string.IsNullOrEmpty(PluginUi.Password))
                {
                    NMPop(fate);
                }
            }
        }

        private void ProcessNewFate(Library.EurekaFate fate)
        {
            EchoNMPop();
            PlaySoundEffect();

            if (Configuration.ShowPopWindow)
            {
                PluginUi.StartShoutCountdown();
                PluginUi.SetEorzeaTimeWithPullOffset();

                PluginUi.PopVisible = true;
            }

            if (fate.TrackerId != 1337 && !string.IsNullOrEmpty(PluginUi.Instance) && !string.IsNullOrEmpty(PluginUi.Password))
            {
                NMPop(fate);
            }
        }

        public void PlaySoundEffect()
        {
            if (Configuration.PlaySoundEffect)
            {
                Sound.PlayEffect(PluginUi.SoundEffect);
            }
        }

        public void NMPop()
        {
            PluginLog.Debug($"Attempting to pop {LastSeenFate.Name}");
            NMPop(LastSeenFate);
        }

        private void NMPop(Library.EurekaFate fate)
        {
            string instanceID = PluginUi.Instance.Split("/").Last();
            if (fate.TrackerId != 1337)
            {
                PluginLog.Debug("Calling web request with following data:");
                PluginLog.Debug($"     NM ID: {fate.TrackerId}");
                PluginLog.Debug($"     Instance ID: {instanceID}");
                PluginLog.Debug($"     Password: {PluginUi.Password}");
                EurekaTrackerWrapper.WebRequests.PopNM((ushort)fate.TrackerId, instanceID, PluginUi.Password);
            }
            else
            {
                PluginLog.Debug("Tracker ID was Ovni, so not attempting web request");
            }
        }

        public void EchoNMPop()
        {
            SeString payload = new SeStringBuilder()
                .AddUiForeground(540)
                .AddText($"{(Configuration.UseShortNames ? LastSeenFate.ShortName : LastSeenFate.Name)} pop: ")
                .AddUiForegroundOff()
                .BuiltString
                .Append(LastSeenFate.MapLink);

            if (Configuration.EchoNMPop)
            {
                Chat.PrintChat(new XivChatEntry { Message = payload });
            }

            if (Configuration.ShowPopToast)
            {
                Toast.ShowQuest(payload);
            }
        }

        public string BuildChatString()
        {
            string time = !Configuration.UseEorzeaTimer ? $"PT {PluginUi.PullTime}" : $"ET {PluginUi.CurrentEorzeanPullTime()}";
            string output = Configuration.ChatFormat
                .Replace("$n", LastSeenFate.Name)
                .Replace("$sN", LastSeenFate.ShortName)
                .Replace("$t", time)
                .Replace("$p", "<flag>");

            return output;
        }

        public void PostChatMessage()
        {
            SetFlagMarker();
            xivCommon.Functions.Chat.SendMessage(BuildChatString());
        }

        public void EchoFairy(Library.Fairy fairy)
        {
            SeString payload = new SeStringBuilder()
                .AddUiForeground(570)
                .AddText("Fairy: ")
                .AddUiGlowOff()
                .AddUiForegroundOff()
                .BuiltString
                .Append(fairy.MapLink);

            if (Configuration.EchoFairies)
            {
                Chat.PrintChat(new XivChatEntry { Message = payload });
            }

            if (Configuration.ShowFairyToast)
            {
                Toast.ShowQuest(payload);
            }
        }

        private void BunnyCheck(Framework framework)
        {
            if (!Library.BunnyMaps.Contains(ClientState.TerritoryType))
                return;

            var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            foreach (var bnuuy in Library.Bunnies.Where(bunny => FateTable.Any(fate => fate.FateId == bunny.FateId)))
            {
                bnuuy.Alive = true;
                bnuuy.LastSeenAlive = currentTime;
            }

            foreach (var bnuuy in Library.Bunnies.Where(bunny => bunny.Alive))
            {
                if (bnuuy.LastSeenAlive != currentTime)
                    bnuuy.Alive = false;
            }
        }

        private void FairyCheck(Framework framework)
        {
            foreach (BattleNpc actor in ObjectTable.OfType<BattleNpc>()
                         .Where(battleNpc => Library.Fairies.Contains(battleNpc.NameId))
                         .Where(battleNpc => !Library.ExistingFairies.ContainsKey(battleNpc.ObjectId)))
            {
                Library.Fairy fairy = new Library.Fairy(actor.NameId, actor.Position.X, actor.Position.Z); // directX Z = Y
                Library.ExistingFairies.Add(actor.ObjectId, fairy);
                EchoFairy(fairy);
            }
        }

        private void PollForFateChange(Framework framework)
        {
            if (NoFatesHaveChangedSinceLastChecked())
            {
                return;
            }

            CheckForRelevantFates(ClientState.TerritoryType);
            lastPolledFates = FateTable.ToList();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            PluginUi.Dispose();
            Framework.Update -= PollForFateChange;
            Framework.Update -= FairyCheck;
            Framework.Update -= BunnyCheck;
            ClientState.TerritoryChanged -= TerritoryChangePoll;
            xivCommon.Dispose();

            CommandManager.RemoveHandler("/xleureka");
            CommandManager.RemoveHandler("/xlposition");
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
                var map = (MapLinkPayload) LastSeenFate.MapLink.Payloads.First();
                AgentMap.Instance()->SetFlagMapMarker(
                    map.Map.TerritoryType.Row,
                    map.Map.RowId,
                    map.RawX / 1000.0f,
                    map.RawY / 1000.0f);
            }
            catch (Exception)
            {
                PluginLog.Error("Exception during SetFlagMarker");
            }
        }
    }
}
