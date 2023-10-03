using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Timers;
using CheapLoc;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using XivCommon;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using EurekaTrackerAutoPopper.Attributes;
using EurekaTrackerAutoPopper.Windows;

namespace EurekaTrackerAutoPopper
{
    public class Plugin : IDalamudPlugin
    {
        [PluginService] public static IChatGui Chat { get; private set; } = null!;
        [PluginService] public static IToastGui Toast { get; private set; } = null!;
        [PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
        [PluginService] public static IFateTable FateTable { get; private set; } = null!;
        [PluginService] public static IFramework Framework { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static IGameGui GameGui { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;
        [PluginService] public static IGameInteropProvider Hook { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;

        public Configuration Configuration { get; init; }

        private MainWindow MainWindow { get; init; }
        private QuestWindow QuestWindow { get; init; }
        private LogWindow LogWindow { get; init; }
        public BunnyWindow BunnyWindow { get; init; }
        public ShoutWindow ShoutWindow { get; init; }
        public CircleOverlay CircleOverlay { get; init; }
        private WindowSystem WindowSystem { get; init; } = new("Eureka Linker");

        public readonly Library Library;
        public bool PlayerInEureka;
        public Library.EurekaFate LastSeenFate = Library.EurekaFate.Empty;
        private List<Fate> lastPolledFates = new();
        public static XivCommonBase XivCommon = null!;

        public static string Authors = "Infi, electr0sheep";
        public static string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";

        private static bool gotBunny;
        private readonly Timer cofferTimer = new(20 * 1000);

        public readonly Stopwatch EurekaWatch = new();

        private Localization Localization = new();
        private readonly PluginCommandManager<Plugin> commandManager;

        public Plugin()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            Library = new Library(Configuration);
            Library.Initialize();

            MainWindow = new MainWindow(this);
            QuestWindow = new QuestWindow();
            LogWindow = new LogWindow();
            BunnyWindow = new BunnyWindow(this);
            ShoutWindow = new ShoutWindow(this);
            CircleOverlay = new CircleOverlay(this);
            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(QuestWindow);
            WindowSystem.AddWindow(LogWindow);
            WindowSystem.AddWindow(BunnyWindow);
            WindowSystem.AddWindow(ShoutWindow);
            WindowSystem.AddWindow(CircleOverlay);

            commandManager = new PluginCommandManager<Plugin>(this, CommandManager);
            Localization.SetupWithLangCode(PluginInterface.UiLanguage);

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            PluginInterface.LanguageChanged += Localization.SetupWithLangCode;

            ClientState.TerritoryChanged += TerritoryChangePoll;
            XivCommon = new XivCommonBase(PluginInterface);
            cofferTimer.AutoReset = false;

            TerritoryChangePoll(ClientState.TerritoryType);
        }

        [Command("/el")]
        [HelpMessage("Opens the config window")]
        private void OnEurekaCommand(string command, string args)
        {
            MainWindow.IsOpen ^= true;
        }

        [Command("/elquest")]
        [HelpMessage("Opens the quest guide")]
        private void OnQuestCommand(string command, string args)
        {
            QuestWindow.IsOpen ^= true;
        }

        [Command("/elbunny")]
        [HelpMessage("Opens the bunny window")]
        private void OnBunnyCommand(string command, string args)
        {
            if (Library.BunnyMaps.Contains(ClientState.TerritoryType))
                BunnyWindow.IsOpen ^= true;
            else
                Chat.PrintError(Loc.Localize("Chat - Error Not In Eureka", "You are not in Eureka, this command is unavailable."));
        }

        [Command("/ellog")]
        [HelpMessage("Opens the log window")]
        private void OnLogCommand(string command, string args)
        {
            LogWindow.IsOpen ^= true;
        }

        [Command("/elmarkers")]
        [HelpMessage("Adds all known coffer locations to the map and minimap")]
        private void OnAddCommand(string command, string args)
        {
            AddChestsLocationsMap();
        }

        [Command("/elremove")]
        [HelpMessage("Removes all the placed markers")]
        private void OnRemoveCommand(string command, string args)
        {
            RemoveMarkerMap();
        }

        private void TerritoryChangePoll(ushort territoryId)
        {
            if (PlayerInRelevantTerritory())
            {
                PlayerInEureka = true;

                if (Configuration.ShowBunnyWindow && Library.BunnyMaps.Contains(ClientState.TerritoryType))
                    BunnyWindow.IsOpen = true;

                EurekaWatch.Restart();

                Framework.Update += PollForFateChange;
                Framework.Update += FairyCheck;
                Framework.Update += BunnyCheck;
            }
            else
            {
                PlayerInEureka = false;

                MainWindow.Reset();
                BunnyWindow.IsOpen = false;
                ShoutWindow.IsOpen = false;
                LastSeenFate = Library.EurekaFate.Empty;
                Library.ExistingFairies.Clear();
                Library.ResetBunnies();

                gotBunny = false;
                BunnyChests.ExistingCoffers.Clear();

                if (EurekaWatch.IsRunning)
                {
                    Configuration.TimeInEureka += EurekaWatch.ElapsedMilliseconds;
                    Configuration.Save();
                    EurekaWatch.Reset();
                }

                Framework.Update -= PollForFateChange;
                Framework.Update -= FairyCheck;
                Framework.Update -= BunnyCheck;
            }
        }

        public static bool PlayerInRelevantTerritory()
        {
            return Library.TerritoryToMap.ContainsKey(ClientState.TerritoryType);
        }

        private bool NoFatesHaveChangedSinceLastChecked()
        {
            return FateTable.SequenceEqual(lastPolledFates);
        }

        private void CheckForRelevantFates(ushort currentTerritory)
        {
            List<ushort> newFateIds = FateTable.Except(lastPolledFates).Select(i => i.FateId).ToList();
            IEnumerable<Library.EurekaFate> relevantFates = Library.TerritoryToFateDictionary(currentTerritory);
            foreach (Library.EurekaFate fate in relevantFates.Where(i => newFateIds.Contains(i.FateId)))
            {
                LastSeenFate = fate;

                ProcessNewFate(fate);
            }
        }

        public void ProcessCurrentFates(ushort currentTerritory)
        {
            List<Fate> currentFates = FateTable.ToList();
            IEnumerable<Library.EurekaFate> relevantFates = Library.TerritoryToFateDictionary(currentTerritory);
            List<Library.EurekaFate> relevantCurrentFates = relevantFates.Where(fate => currentFates.Select(i => i.FateId).Contains(fate.FateId)).ToList();
            foreach (Library.EurekaFate fate in relevantCurrentFates)
            {
                if (fate.TrackerId != 0 && !string.IsNullOrEmpty(MainWindow.Instance) && !string.IsNullOrEmpty(MainWindow.Password))
                {
                    NMPop(fate);
                }
            }
        }

        private void ProcessNewFate(Library.EurekaFate fate)
        {
            EchoNMPop();

            if (Configuration.PlaySoundEffect)
                Sound.PlayEffect(Configuration.PopSoundEffect);

            if (Configuration.ShowPopWindow)
            {
                ShoutWindow.StartShoutCountdown();
                ShoutWindow.SetEorzeaTimeWithPullOffset();

                ShoutWindow.IsOpen = true;
            }

            if (fate.TrackerId != 0 && !string.IsNullOrEmpty(MainWindow.Instance) && !string.IsNullOrEmpty(MainWindow.Password))
            {
                NMPop(fate);
            }
        }

        public void NMPop()
        {
            Log.Debug($"Attempting to pop {LastSeenFate.Name}");
            NMPop(LastSeenFate);
        }

        private void NMPop(Library.EurekaFate fate)
        {
            var instanceID = MainWindow.Instance.Split("/").Last();
            if (fate.TrackerId != 0)
            {
                Log.Debug("Calling web request with following data:");
                Log.Debug($"     NM ID: {fate.TrackerId}");
                Log.Debug($"     Instance ID: {instanceID}");
                Log.Debug($"     Password: {MainWindow.Password}");
                EurekaTrackerWrapper.WebRequests.PopNM(fate.TrackerId, instanceID, MainWindow.Password);
            }
            else
            {
                Log.Debug("Tracker ID was Ovni, so not attempting web request");
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
                Chat.Print(new XivChatEntry { Message = payload });

            if (Configuration.ShowPopToast)
                Toast.ShowQuest(payload);
        }

        private string BuildChatString()
        {
            string time = !Configuration.UseEorzeaTimer ? $"PT {ShoutWindow.PullTime}" : $"ET {ShoutWindow.CurrentEorzeanPullTime()}";
            string output = Configuration.ChatFormat
                .Replace("$n", LastSeenFate.Name)
                .Replace("$sN", LastSeenFate.ShortName)
                .Replace("$t", Configuration.ShowPullTimer ? time : "")
                .Replace("$p", "<flag>");

            return output;
        }

        public void PostChatMessage()
        {
            SetFlagMarker();
            XivCommon.Functions.Chat.SendMessage(BuildChatString());
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
                Chat.Print(new XivChatEntry { Message = payload });

            if (Configuration.ShowFairyToast)
                Toast.ShowQuest(payload);
        }

        private void BunnyCheck(IFramework framework)
        {
            if (!Library.BunnyMaps.Contains(ClientState.TerritoryType))
                return;

            var local = ClientState.LocalPlayer;
            if (local == null)
                return;

            var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            foreach (var bnuuy in Library.Bunnies)
            {
                if (FateTable.Any(fate => fate.FateId == bnuuy.FateId))
                {
                    bnuuy.Alive = true;
                    bnuuy.LastSeenAlive = currentTime;

                    if (!bnuuy.PlayedSound && Configuration.PlayBunnyEffect)
                    {
                        if (!Configuration.OnlyEasyBunny || bnuuy.Easy)
                        {
                            bnuuy.PlayedSound = true;
                            Sound.PlayEffect(Configuration.BunnySoundEffect);
                        }
                    }
                }

                if (bnuuy.LastSeenAlive != currentTime)
                {
                    bnuuy.Alive = false;
                    bnuuy.PlayedSound = false;
                }
            }

            if (local.StatusList.Any(status => status.StatusId == 1531))
            {
                if (!gotBunny)
                {
                    Configuration.KilledBunnies += 1;
                    Configuration.Save();

                    gotBunny = true;
                }

                var pos = BunnyChests.CalculateDistance(ClientState.TerritoryType, local.Position);
                if (pos != Vector3.Zero)
                {
                    CircleOverlay.NearToCoffer = true;
                    CircleOverlay.CofferPos = pos;
                }
                else
                {
                    CircleOverlay.NearToCoffer = false;
                }

                // refresh timer until buff is gone
                cofferTimer.Stop();
                cofferTimer.Interval = 20 * 1000;
                cofferTimer.Start();
            }
            else
            {
                CircleOverlay.NearToCoffer = false;
                gotBunny = false;

                // return if timer isn't running
                if (!cofferTimer.Enabled)
                    return;

                if (local.TargetObject == null)
                    return;

                foreach (var coffer in ObjectTable.OfType<EventObj>()
                             .Where(a => BunnyChests.Coffers.Contains(a.DataId))
                             .Where(a => !BunnyChests.ExistingCoffers.Contains(a.ObjectId)))
                {
                    if (coffer.ObjectId != local.TargetObject.ObjectId)
                        continue;

                    BunnyChests.ExistingCoffers.Add(coffer.ObjectId);
                    cofferTimer.Stop();

                    Configuration.Stats[ClientState.TerritoryType][coffer.DataId] += 1;
                    Configuration.KilledBunnies -= 1;
                    Configuration.Save();

                    // TODO Remove after all chests found
                    if (!BunnyChests.Exists(ClientState.TerritoryType, coffer.Position))
                    {
                        Chat.Print(Loc.Localize("Chat - New Chest Found", "You've found a new chest location"));
                        Chat.Print(Loc.Localize("Chat - New Chest Found Dev Note", "Please consider sending the following information to the developer:"));
                        Chat.Print(Loc.Localize("Chat - New Chest Found Feedback", "(Please do not use the feedback function to send this)"));
                        Chat.Print($"Terri: {ClientState.TerritoryType} Pos: {coffer.Position.X:000.000000}f, {coffer.Position.Y:000.#########}f, {coffer.Position.Z:000.#########}f");
                        BunnyChests.Positions[ClientState.TerritoryType].Add(coffer.Position);
                    }
                }
            }
        }

        private void FairyCheck(IFramework framework)
        {
            foreach (BattleNpc actor in ObjectTable.OfType<BattleNpc>()
                         .Where(battleNpc => Library.Fairies.Contains(battleNpc.NameId))
                         .Where(battleNpc => Library.ExistingFairies.All(f => f.ObjectId != battleNpc.ObjectId)))
            {
                var fairy = new Library.Fairy(actor.ObjectId, actor.NameId, actor.Position);
                Library.ExistingFairies.Add(fairy);
                EchoFairy(fairy);
            }

            var local = ClientState.LocalPlayer;
            if (local == null)
                return;

            foreach (var fairy in Library.ExistingFairies.ToArray())
            {
                if (!(Utils.GetDistance(local.Position, fairy.Pos) < 80.0)
                    || ObjectTable.Any(obj => fairy.ObjectId == obj.ObjectId))
                    continue;

                Library.ExistingFairies.Remove(fairy);
                Chat.Print(Loc.Localize("Chat - Dead Fairy Note", "Removing inactive fairies from tracking."));
            }
        }

        private void PollForFateChange(IFramework framework)
        {
            if (NoFatesHaveChangedSinceLastChecked())
                return;

            CheckForRelevantFates(ClientState.TerritoryType);
            lastPolledFates = FateTable.ToList();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Framework.Update -= PollForFateChange;
            Framework.Update -= FairyCheck;
            Framework.Update -= BunnyCheck;
            ClientState.TerritoryChanged -= TerritoryChangePoll;

            PluginInterface.UiBuilder.Draw -= DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
            PluginInterface.LanguageChanged -= Localization.SetupWithLangCode;

            if (EurekaWatch.IsRunning)
            {
                Configuration.TimeInEureka += EurekaWatch.ElapsedMilliseconds;
                Configuration.Save();
                EurekaWatch.Reset();
            }

            XivCommon.Dispose();
            commandManager.Dispose();
            WindowSystem.RemoveWindow(QuestWindow);
        }

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        private void DrawConfigUI()
        {
            MainWindow.IsOpen = true;
        }

        public static unsafe void AddChestsLocationsMap()
        {
            if (!Library.BunnyMaps.Contains(ClientState.TerritoryType))
            {
                Chat.PrintError(Loc.Localize("Chat - Error Not In Eureka", "You are not in Eureka, this command is unavailable."));
                return;
            }

            AgentMap.Instance()->ResetMapMarkers();
            AgentMap.Instance()->ResetMiniMapMarkers();

            foreach (var pos in BunnyChests.Positions[ClientState.TerritoryType])
            {
                var orgPos = pos;
                if (ClientState.TerritoryType == 827)
                    orgPos.Z += 475;

                if(!AgentMap.Instance()->AddMapMarker(orgPos, 60354))
                    Chat.PrintError(Loc.Localize("Chat - Error Map Markers", "Unable to place all markers on map"));
                if (!AgentMap.Instance()->AddMiniMapMarker(pos, 60354))
                    Chat.PrintError(Loc.Localize("Chat - Error Minimap Markers", "Unable to place all markers on minimap"));
            }
        }

        public unsafe void AddFairyLocationsMap()
        {
            if (!Library.BunnyMaps.Contains(ClientState.TerritoryType))
            {
                Chat.PrintError(Loc.Localize("Chat - Error Not In Eureka", "You are not in Eureka, this command is unavailable."));
                return;
            }

            AgentMap.Instance()->ResetMapMarkers();
            AgentMap.Instance()->ResetMiniMapMarkers();

            foreach (var (fairy, idx) in Library.ExistingFairies.Select((val, i) => (val, (uint) i)))
            {
                if (idx == 3)
                    Chat.PrintError(Loc.Localize("Chat - Error Fairy Markers", "Tracking for fairies needs to be reset, please go to all listed locations to update."));

                var orgPos = fairy.Pos;
                if (ClientState.TerritoryType == 827)
                    orgPos.Z += 475;

                if(!AgentMap.Instance()->AddMapMarker(orgPos, 60474 + idx))
                    Chat.PrintError(Loc.Localize("Chat - Error Fairy Map Markers", "Unable to place fairy markers on map"));
                if (!AgentMap.Instance()->AddMiniMapMarker(fairy.Pos, 60474 + idx))
                    Chat.PrintError(Loc.Localize("Chat - Error Fairy Minimap Markers", "Unable to place fairy markers on minimap"));
            }
        }

        public static unsafe void RemoveMarkerMap()
        {
            if (!Library.BunnyMaps.Contains(ClientState.TerritoryType))
            {
                Chat.PrintError(Loc.Localize("Chat - Error Not In Eureka", "You are not in Eureka, this command is unavailable."));
                return;
            }

            AgentMap.Instance()->ResetMapMarkers();
            AgentMap.Instance()->ResetMiniMapMarkers();
        }

        public unsafe void SetFlagMarker()
        {
            try
            {
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
                Log.Error("Exception during SetFlagMarker");
            }
        }

        public static void OpenMap(MapLinkPayload map) => GameGui.OpenMapWithMapLink(map);
    }
}
