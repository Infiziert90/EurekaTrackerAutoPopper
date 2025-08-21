using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using EurekaTrackerAutoPopper.Attributes;
using EurekaTrackerAutoPopper.Resources;
using EurekaTrackerAutoPopper.Windows;
using EurekaTrackerAutoPopper.Windows.MainWindow;
using EurekaTrackerAutoPopper.Windows.OccultWindow;
using EurekaTrackerAutoPopper.Windows.Overlay;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI;
using Dalamud.Bindings.ImGui;
using EurekaTrackerAutoPopper.Audio;

using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace EurekaTrackerAutoPopper;

public class Plugin : IDalamudPlugin
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IChatGui Chat { get; private set; } = null!;
    [PluginService] public static IToastGui Toast { get; private set; } = null!;
    [PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] public static IFateTable FateTable { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IDataManager Data { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static ITextureProvider TextureManager { get; private set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    public Configuration Configuration { get; init; }

    private WindowSystem WindowSystem { get; init; } = new("Eureka Linker");
    private MainWindow MainWindow { get; init; }
    public OccultWindow OccultWindow { get; init; }
    private QuestWindow QuestWindow { get; init; }
    private LogWindow LogWindow { get; init; }
    public BunnyWindow BunnyWindow { get; init; }
    public ShoutWindow ShoutWindow { get; init; }
    public FastSwitchOverlay FastSwitchOverlay { get; init; }

    public readonly Library Library;
    public readonly Fates Fates;
    public readonly TrackerHandler TrackerHandler;
    public readonly AlarmClock AlarmClock;

    public bool PlayerInEureka;
    public Library.EurekaFate LastSeenFate = Library.EurekaFate.Empty;
    private List<IFate> LastPolledFates = [];

    private static bool GotBunny;
    private readonly Timer CofferTimer = new(20 * 1000);

    public readonly Stopwatch EurekaWatch = new();
    private readonly PluginCommandManager<Plugin> Commands;

    // CircleOverlay
    public bool NearToCoffer;
    public Vector3 CofferPos = Vector3.Zero;
    private readonly Timer PreviewTimer = new(5 * 1000);

    // Selected Map Markers
    public SharedMarkerSet MarkerSetToPlace = SharedMarkerSet.None;
    public SharedMarkerSet? SavedOccultMarkerSets;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        Library = new Library(Configuration);
        Library.Initialize();

        Fates = new Fates(this);
        TrackerHandler = new TrackerHandler(this);
        AlarmClock = new AlarmClock();

        MainWindow = new MainWindow(this);
        OccultWindow = new OccultWindow(this);
        QuestWindow = new QuestWindow();
        LogWindow = new LogWindow();
        BunnyWindow = new BunnyWindow(this);
        ShoutWindow = new ShoutWindow(this);
        FastSwitchOverlay = new FastSwitchOverlay(this);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(OccultWindow);
        WindowSystem.AddWindow(QuestWindow);
        WindowSystem.AddWindow(LogWindow);
        WindowSystem.AddWindow(BunnyWindow);
        WindowSystem.AddWindow(ShoutWindow);
        WindowSystem.AddWindow(FastSwitchOverlay);

        Commands = new PluginCommandManager<Plugin>(this, CommandManager);

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        PluginInterface.LanguageChanged += LanguageChanged;

        LanguageChanged(PluginInterface.UiLanguage);

        ClientState.TerritoryChanged += TerritoryChangePoll;
        CofferTimer.AutoReset = false;

        TerritoryChangePoll(ClientState.TerritoryType);

        PreviewTimer.AutoReset = false;
        PreviewTimer.Elapsed += (_, _) =>
        {
            NearToCoffer = false;
            CofferPos = Vector3.Zero;
        };

        AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "AreaMap", RefreshMapMarker);
        AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "AreaMap", RefreshMapMarkerOccult);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Fates.Dispose();
        TrackerHandler.Dispose();

        Framework.Update -= PollForFateChange;
        Framework.Update -= FairyCheck;
        Framework.Update -= BunnyCheck;
        Framework.Update -= OccultCheck;
        Framework.Update -= OccultPotCheck;
        ClientState.TerritoryChanged -= TerritoryChangePoll;

        PluginInterface.UiBuilder.Draw -= DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
        PluginInterface.LanguageChanged -= LanguageChanged;

        AddonLifecycle.UnregisterListener(AddonEvent.PostRefresh, "AreaMap", RefreshMapMarker);
        AddonLifecycle.UnregisterListener(AddonEvent.PostRefresh, "AreaMap", RefreshMapMarkerOccult);

        if (EurekaWatch.IsRunning)
        {
            Configuration.TimeInEureka += EurekaWatch.ElapsedMilliseconds;
            Configuration.Save();
            EurekaWatch.Reset();
        }

        Commands.Dispose();
        WindowSystem.RemoveAllWindows();
    }

    public void StartTrackerAsync()
    {
        Task.Run(async () =>
        {
            try
            {
                (MainWindow.Instance, MainWindow.Password) = await EurekaTrackerWrapper.WebRequests.CreateNewTracker(Library.TerritoryToTrackerDictionary[ClientState.TerritoryType]);

                await Framework.RunOnFrameworkThread(ProcessCurrentFates);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start new Tracker.");
            }
        });
    }

    private void LanguageChanged(string langCode)
    {
        Language.Culture = new CultureInfo(langCode);
    }

    private unsafe void RefreshMapMarker(AddonEvent type, AddonArgs args)
    {
        if (MarkerSetToPlace != SharedMarkerSet.Eureka)
            return;

        // Check the players territory type
        if (!Fates.EurekaTerritories.Contains(ClientState.TerritoryType))
            return;

        // Check the map selection, important for Hydatos as it has different map IDs with BA
        var isBunnyMap = Fates.BunnyMapIds.Contains(AgentMap.Instance()->SelectedMapId);

        PlaceEurekaMarkerSet(isBunnyMap);
    }

    private void RefreshMapMarkerOccult(AddonEvent type, AddonArgs args)
    {
        PlaceOccultMarkerSet(MarkerSetToPlace.ToOccultSet());
    }

    [Command("/el")]
    [HelpMessage("Opens the config window")]
    private void OnEurekaCommand(string command, string args)
    {
        MainWindow.Toggle();
    }

    [Command("/eloccult")]
    [HelpMessage("Opens occult helper window")]
    private void OnOccultCommand(string command, string args)
    {
        OccultWindow.Toggle();
    }

    [Command("/elquest")]
    [HelpMessage("Opens the quest guide")]
    private void OnQuestCommand(string command, string args)
    {
        QuestWindow.Toggle();
    }

    [Command("/elbunny")]
    [HelpMessage("Opens the bunny window")]
    private void OnBunnyCommand(string command, string args)
    {
        if (Fates.BunnyTerritories.Contains(ClientState.TerritoryType))
            BunnyWindow.Toggle();
        else
            Chat.PrintError(Language.ChatErrorNotInEureka);
    }

    [Command("/ellog")]
    [HelpMessage("Opens the log window")]
    private void OnLogCommand(string command, string args)
    {
        LogWindow.Toggle();
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
        RemoveMapMarker();
    }

    private void TerritoryChangePoll(ushort territoryId)
    {
        // Notify the user once about upload opt out
        if (Configuration.UploadNotification)
        {
            // User received the notice, so we schedule the first upload 1h after
            Configuration.UploadNotification = false;
            Configuration.Save();

            Chat.Print(Utils.SuccessMessage("Important"));
            Chat.Print(Utils.SuccessMessage("This plugin uploads anonymized instance data. " +
                                            "For more information on the exact data collected please see the upload tab in the configuration menu. " +
                                            "You can opt out of any and all forms of data collection."));
        }

        if (PlayerInRelevantTerritory())
        {
            PlayerInEureka = true;

            if (Configuration.ShowBunnyWindow && Fates.BunnyTerritories.Contains(ClientState.TerritoryType))
                BunnyWindow.IsOpen = true;

            if (Configuration.AddIconsOnEntry)
            {
                Task.Run(async () =>
                {
                    // Delay it by 10s so that map had a chance to fully load
                    await Task.Delay(new TimeSpan(0, 0, 10));
                    await Framework.RunOnFrameworkThread(() => { PlaceEurekaMarkerSet(true); });
                });
            }

            EurekaWatch.Restart();

            Framework.Update += PollForFateChange;
            Framework.Update += FairyCheck;
            Framework.Update += BunnyCheck;
            Fates.RegisterEvents();
        }
        else if (ClientState.TerritoryType == (uint)Territory.SouthHorn)
        {
            if (Configuration.ShowBunnyWindow)
                BunnyWindow.IsOpen = true;

            if (Configuration.PlaceDefaultOccult)
            {
                Task.Run(async () =>
                {
                    // Delay it by 10s so that map had a chance to fully load
                    await Task.Delay(new TimeSpan(0, 0, 10));
                    await Framework.RunOnFrameworkThread(() => { PlaceOccultMarkerSet(Configuration.DefaultOccultMarkerSets); });
                });
            }

            Framework.Update += OccultCheck;
            Framework.Update += OccultPotCheck;

            Fates.RegisterEvents();

            // Set forked tower timer to when the client joined south horn
            Fates.OccultCriticalEncounters[^1].InstanceJoinedTimer = DateTimeOffset.Now.ToUnixTimeSeconds();
        }
        else
        {
            PlayerInEureka = false;

            MainWindow.Reset();
            BunnyWindow.IsOpen = false;
            ShoutWindow.IsOpen = false;
            LastSeenFate = Library.EurekaFate.Empty;
            Library.CleanCaches();

            Fates.Reset();
            TrackerHandler.Reset();

            GotBunny = false;
            NearToCoffer = false;
            CofferPos = Vector3.Zero;
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
            Framework.Update -= OccultCheck;
            Framework.Update -= OccultPotCheck;

            Fates.RemoveEvents();
        }
    }

    public static bool PlayerInRelevantTerritory()
    {
        return Library.TerritoryToMap.ContainsKey(ClientState.TerritoryType);
    }

    public static bool PlayerInOccultTerritory()
    {
        return ClientState.TerritoryType == (uint)Territory.SouthHorn;
    }

    private bool NoFatesHaveChangedSinceLastChecked()
    {
        return FateTable.SequenceEqual(LastPolledFates);
    }

    private void CheckForRelevantFates(ushort currentTerritory)
    {
        var newFateIds = FateTable.Except(LastPolledFates).Select(i => i.FateId).ToList();
        var relevantFates = Library.TerritoryToFateDictionary(currentTerritory);
        foreach (var fate in relevantFates.Where(i => newFateIds.Contains(i.FateId)))
        {
            LastSeenFate = fate;

            ProcessNewFate(fate);
        }
    }

    public void ProcessCurrentFates()
    {
        var currentFates = FateTable.ToList();
        var relevantFates = Library.TerritoryToFateDictionary(ClientState.TerritoryType);
        var relevantCurrentFates = relevantFates.Where(fate => currentFates.Select(i => i.FateId).Contains(fate.FateId)).ToList();
        foreach (var fate in relevantCurrentFates)
        {
            if (fate.TrackerId != 0 && !string.IsNullOrEmpty(MainWindow.Instance) &&
                !string.IsNullOrEmpty(MainWindow.Password))
                NMPop(fate);
        }
    }

    private void ProcessNewFate(Library.EurekaFate fate)
    {
        EchoNMPop();

        if (Configuration.PlaySoundEffect)
            UIGlobals.PlaySoundEffect((uint)Configuration.PopSoundEffect);

        if (Configuration.ShowPopWindow)
        {
            ShoutWindow.StartShoutCountdown();
            ShoutWindow.SetEorzeaTimeWithPullOffset();

            ShoutWindow.IsOpen = true;
        }

        if (Configuration.CopyShoutMessage)
            CopyChatMessage();

        if (fate.TrackerId != 0 && !string.IsNullOrEmpty(MainWindow.Instance) &&
            !string.IsNullOrEmpty(MainWindow.Password))
            NMPop(fate);
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
        var payload = new SeStringBuilder()
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
        var time = !Configuration.UseEorzeaTimer
            ? $"PT {ShoutWindow.PullTime}"
            : $"ET {ShoutWindow.CurrentEorzeanPullTime()}";
        var output = Configuration.ChatFormat
            .Replace("$n", LastSeenFate.Name)
            .Replace("$sN", LastSeenFate.ShortName)
            .Replace("$t", Configuration.ShowPullTimer ? time : "")
            .Replace("$p", "<flag>");

        return output;
    }

    public void CopyChatMessage()
    {
        SetFlagMarker((MapLinkPayload)LastSeenFate.MapLink.Payloads[0]);
        ImGui.SetClipboardText(BuildChatString());
    }

    public void PostChatMessage()
    {
        SetFlagMarker((MapLinkPayload)LastSeenFate.MapLink.Payloads[0]);
        ChatBox.SendMessage(BuildChatString());
    }

    public void EchoFairy(Library.LocationMemory fairy)
    {
        var payload = new SeStringBuilder()
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

        if (Configuration.PlaceFairyFlag)
            SetFlagMarker((MapLinkPayload)fairy.MapLink.Payloads[0]);
    }

    private void EchoTreasure(Library.LocationMemory treasure)
    {
        var payload = new SeStringBuilder()
            .AddUiForeground(570)
            .AddText($"Treasure ({treasure.Type}): ")
            .AddUiGlowOff()
            .AddUiForegroundOff()
            .BuiltString
            .Append(treasure.MapLink);

        if (Configuration.EchoTreasure)
            Chat.Print(new XivChatEntry { Message = payload });

        if (Configuration.ShowTreasureToast)
            Toast.ShowQuest(payload);

        if (Configuration.PlaceTreasureFlag)
            SetFlagMarker((MapLinkPayload)treasure.MapLink.Payloads[0]);
    }

    private void EchoBunnyCarrot(Library.LocationMemory carrot)
    {
        var payload = new SeStringBuilder()
            .AddUiForeground(570)
            .AddText("Carrot spotted: ")
            .AddUiGlowOff()
            .AddUiForegroundOff()
            .BuiltString
            .Append(carrot.MapLink);

        if (Configuration.EchoBunnyCarrot)
            Chat.Print(new XivChatEntry { Message = payload });

        if (Configuration.ShowBunnyCarrotToast)
            Toast.ShowQuest(payload);

        if (Configuration.PlaceBunnyCarrotFlag)
            SetFlagMarker((MapLinkPayload)carrot.MapLink.Payloads[0]);
    }

    private void BunnyCheck(IFramework framework)
    {
        if (!Fates.BunnyTerritories.Contains(ClientState.TerritoryType))
            return;

        var local = ClientState.LocalPlayer;
        if (local == null)
            return;

        if (local.StatusList.Any(status => status.StatusId == 1531))
        {
            if (!GotBunny)
            {
                Configuration.KilledBunnies += 1;
                Configuration.Save();

                GotBunny = true;
            }

            var pos = BunnyChests.CalculateDistance(ClientState.TerritoryType, local.Position);
            if (pos != Vector3.Zero)
            {
                NearToCoffer = true;
                CofferPos = pos;
            }
            else
            {
                NearToCoffer = false;
            }

            // refresh timer until buff is gone
            CofferTimer.Stop();
            CofferTimer.Interval = 20 * 1000;
            CofferTimer.Start();
        }
        else
        {
            NearToCoffer = false;
            GotBunny = false;

            // return if timer isn't running
            if (!CofferTimer.Enabled)
                return;

            if (local.TargetObject == null)
                return;

            foreach (var coffer in ObjectTable.OfType<IEventObj>()
                         .Where(a => BunnyChests.Coffers.Contains(a.DataId))
                         .Where(a => !BunnyChests.ExistingCoffers.Contains(a.EntityId)))
            {
                if (coffer.EntityId != local.TargetObject.EntityId)
                    continue;

                BunnyChests.ExistingCoffers.Add(coffer.EntityId);
                CofferTimer.Stop();

                Configuration.Stats[ClientState.TerritoryType][coffer.DataId] += 1;
                Configuration.KilledBunnies -= 1;
                Configuration.Save();
            }
        }
    }

    private void FairyCheck(IFramework framework)
    {
        var local = ClientState.LocalPlayer;
        if (local == null)
            return;

        foreach (var actor in ObjectTable.OfType<IBattleNpc>().Where(npc => Library.Fairies.Contains(npc.NameId)))
        {
            if (Library.ExistingFairies.Any(f => f.ObjectId == actor.EntityId))
                continue;

            var fairy = new Library.LocationMemory(actor.EntityId, actor.Position);
            Library.ExistingFairies.Add(fairy);
            EchoFairy(fairy);
        }

        foreach (var fairy in Library.ExistingFairies.ToArray())
        {
            if (!(Utils.GetDistance(local.Position, fairy.Pos) < 80.0)
                || ObjectTable.Any(obj => fairy.ObjectId == obj.EntityId))
                continue;

            Library.ExistingFairies.Remove(fairy);
            Chat.Print(Language.ChatDeadFairyNote);
        }
    }

    private unsafe void OccultCheck(IFramework _)
    {
        var local = ClientState.LocalPlayer;
        if (local == null)
            return;

        // Player is in a Critical Engagement, disable all scans
        if (IsInCriticalEncounter())
            return;

        // Player is in a fade, disable all scans
        if (IsInFate())
            return;

        foreach (var actor in ObjectTable.Where(gameObject => gameObject.ObjectKind == ObjectKind.Treasure))
        {
            // This range should include all random coffer
            if (actor.DataId is > 1856 or < 1789)
                return;

            var treasureObject = (Treasure*)actor.Address;
            if (treasureObject->RenderFlags > 0)
                return;

            if (!Sheets.TreasureSheet.TryGetRow(actor.DataId, out var treasureRow))
                return;

            if (Library.ExistingOccultLocations.Any(f => f.ObjectId == actor.EntityId))
                continue;

            var treasure = new Library.LocationMemory(actor.EntityId, actor.Position, treasureRow.SGB.RowId);
            Library.ExistingOccultLocations.Add(treasure);
            EchoTreasure(treasure);
        }

        foreach (var actor in ObjectTable.Where(gameObject => gameObject.ObjectKind == ObjectKind.EventObj))
        {
            if (actor.DataId != 2010139)
                continue;

            if (Library.ExistingOccultLocations.Any(f => f.ObjectId == actor.EntityId))
                continue;

            var bunnyCarrot = new Library.LocationMemory(actor.EntityId, actor.Position);
            Library.ExistingOccultLocations.Add(bunnyCarrot);
            EchoBunnyCarrot(bunnyCarrot);
        }

        if (!Configuration.ClearMemory)
            return;

        var deleteIndex = -1;
        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        foreach (var (locationObject, idx) in Library.ExistingOccultLocations.Select((val, idx) => (val, idx)))
        {
            if (!CheckForObjectRemoval(locationObject, local, currentTime))
                continue;

            deleteIndex = idx;
            break;
        }

        if (deleteIndex != -1)
            Library.ExistingOccultLocations.RemoveAt(deleteIndex);
    }

    private bool CheckForObjectRemoval(Library.LocationMemory locationObject, IPlayerCharacter local, long currentTime)
    {
        // Treasure still in reach of the object table?
        if (Utils.GetDistance(local.Position, locationObject.Pos) > 120.0)
            return locationObject.LastSeen + Configuration.ClearAfterSeconds < currentTime;

        // Treasure is still around the player
        if (ObjectTable.Any(obj => locationObject.ObjectId == obj.EntityId))
        {
            locationObject.LastSeen = currentTime;
            return false;
        }

        // Treasure may be in reach but isn't in the object table anymore, so we check timer for removal
        return locationObject.LastSeen + Configuration.ClearAfterSeconds < currentTime;
    }

    private void OccultPotCheck(IFramework _)
    {
        if (ClientState.TerritoryType != 1252)
            return;

        var local = ClientState.LocalPlayer;
        if (local == null)
            return;

        if (local.StatusList.Any(status => status.StatusId == 1531))
        {
            var pos = OccultChests.CalculateDistance(ClientState.TerritoryType, local.Position);
            if (pos != Vector3.Zero)
            {
                NearToCoffer = true;
                CofferPos = pos;
            }
            else
            {
                NearToCoffer = false;
            }

            // refresh timer until buff is gone
            CofferTimer.Stop();
            CofferTimer.Interval = 20 * 1000;
            CofferTimer.Start();

            if (Configuration.AutoSwitchToOccultPots && SavedOccultMarkerSets is null)
            {
                SavedOccultMarkerSets = MarkerSetToPlace;
                PlaceOccultMarkerSet(OccultMarkerSets.PotAndReroll, true);
            }
        }
        else
        {
            NearToCoffer = false;
            if (SavedOccultMarkerSets is { } set)
            {
                PlaceOccultMarkerSet(set.ToOccultSet(), true);
                SavedOccultMarkerSets = null;
            }
        }
    }

    private void PollForFateChange(IFramework framework)
    {
        if (NoFatesHaveChangedSinceLastChecked())
            return;

        CheckForRelevantFates(ClientState.TerritoryType);
        LastPolledFates = FateTable.ToList();
    }

    private void DrawUI()
    {
        WindowSystem.Draw();

        // Draw Circle Overlay
        if (!Configuration.BunnyCircleDraw)
            return;

        if ((!NearToCoffer && !PreviewTimer.Enabled) || CofferPos == Vector3.Zero)
            return;

        // Only draw if the returned pos is valid
        if (GameGui.WorldToScreen(CofferPos, out var circlePos))
            ImGui.GetForegroundDrawList().AddCircleFilled(circlePos, 8.0f, ImGui.ColorConvertFloat4ToU32(Configuration.CircleColor));
    }

    private void DrawConfigUI()
    {
        MainWindow.Toggle();
    }

    public void PlaceEurekaMarkerSet(bool placeBunny, bool clear = true)
    {
        if (!Library.TerritoryToMap.ContainsKey(ClientState.TerritoryType))
            return;

        if (clear)
            RemoveMapMarker();

        if (placeBunny && Fates.BunnyTerritories.Contains(ClientState.TerritoryType))
            AddChestsLocationsMap();

        AddFairyLocationsMap();
    }

    public unsafe void PlaceOccultMarkerSet(OccultMarkerSets set, bool clearForNone = false)
    {
        // Check the players current territory type
        if (ClientState.TerritoryType != 1252 || AgentMap.Instance()->SelectedMapId != 967)
            return;

        if (set == OccultMarkerSets.None)
        {
            if (clearForNone)
                RemoveMapMarker();

            return;
        }

        RemoveMapMarker();

        switch (set)
        {
            case OccultMarkerSets.Treasure:
                AddOccultTreasureLocations();
                break;
            case OccultMarkerSets.PotNorth:
                AddOccultPotNorthLocations();
                break;
            case OccultMarkerSets.PotSouth:
                AddOccultPotSouthLocations();
                break;
            case OccultMarkerSets.Pot:
                AddOccultPotNorthLocations();
                AddOccultPotSouthLocations();
                MarkerSetToPlace = SharedMarkerSet.OccultPot;
                break;
            case OccultMarkerSets.Reroll:
                AddOccultRerollLocations();
                break;
            case OccultMarkerSets.PotAndReroll:
                AddOccultPotNorthLocations();
                AddOccultPotSouthLocations();
                AddOccultRerollLocations();
                MarkerSetToPlace = SharedMarkerSet.OccultPotReroll;
                break;
            case OccultMarkerSets.Bunny:
                AddOccultBunnyPositions();
                break;
            case OccultMarkerSets.OnlyBronze:
                AddOccultBronzeLocations();
                break;
            case OccultMarkerSets.OnlySilver:
                AddOccultSilverLocations();
                break;
            case OccultMarkerSets.TreasureAndCarrots:
                AddOccultTreasureLocations();
                AddOccultBunnyPositions();
                MarkerSetToPlace = SharedMarkerSet.OccultTreasureCarrots;
                break;
        }
    }

    private void AddChestsLocationsMap()
    {
        MarkerSetToPlace = SharedMarkerSet.Eureka;
        foreach (var worldPos in BunnyChests.Positions[ClientState.TerritoryType])
        {
            var mapPos = worldPos;
            if (ClientState.TerritoryType == 827)
                mapPos.Z += 475;

            SetMarkers(worldPos, mapPos, 60354);
        }
    }

    private void AddFairyLocationsMap()
    {
        MarkerSetToPlace = SharedMarkerSet.Eureka;
        foreach (var (fairy, idx) in Library.ExistingFairies.Select((val, i) => (val, i)))
        {
            if (idx == 3)
                Chat.PrintError(Language.ChatErrorFairyMarkers);

            var mapPos = fairy.Pos;
            if (ClientState.TerritoryType == 827)
                mapPos.Z += 475;

            Log.Information($"Adding Fairy Marker: {fairy.Pos}");
            SetMarkers(fairy.Pos, mapPos, 60474 + (uint)idx);
        }
    }

    private void AddOccultTreasureLocations()
    {
        MarkerSetToPlace = SharedMarkerSet.OccultTreasure;
        foreach (var (worldPos, iconType) in OccultChests.TreasurePosition[ClientState.TerritoryType])
        {
            var icon = iconType switch
            {
                1596 => 60356u,
                1597 => 60355u,
                _ => 60354u
            };

            SetMarkers(worldPos, worldPos, icon);
        }
    }

    private void AddOccultPotNorthLocations()
    {
        MarkerSetToPlace = SharedMarkerSet.OccultPotNorth;
        foreach (var worldPos in OccultChests.PotNorthPosition[ClientState.TerritoryType])
            SetMarkers(worldPos, worldPos, 60354);
    }

    private void AddOccultPotSouthLocations()
    {
        MarkerSetToPlace = SharedMarkerSet.OccultPotSouth;
        foreach (var worldPos in OccultChests.PotSouthPosition[ClientState.TerritoryType])
            SetMarkers(worldPos, worldPos, 60354);
    }

    private void AddOccultRerollLocations()
    {
        MarkerSetToPlace = SharedMarkerSet.OccultReroll;
        foreach (var worldPos in OccultChests.RerollPosition[ClientState.TerritoryType])
            SetMarkers(worldPos, worldPos, 61473);
    }

    private void AddOccultBunnyPositions()
    {
        MarkerSetToPlace = SharedMarkerSet.OccultBunny;
        foreach (var worldPos in OccultChests.BunnyPosition[ClientState.TerritoryType])
            SetMarkers(worldPos, worldPos, 25207);
    }

    private void AddOccultBronzeLocations()
    {
        MarkerSetToPlace = SharedMarkerSet.OccultBronze;
        foreach (var (worldPos, _) in OccultChests.TreasurePosition[ClientState.TerritoryType].Where(pair => pair.Item2 == 1596))
            SetMarkers(worldPos, worldPos, 60356u);
    }

    private void AddOccultSilverLocations()
    {
        MarkerSetToPlace = SharedMarkerSet.OccultSilver;
        foreach (var (worldPos, _) in OccultChests.TreasurePosition[ClientState.TerritoryType].Where(pair => pair.Item2 == 1597))
            SetMarkers(worldPos, worldPos, 60355u);
    }


    public unsafe void RemoveMapMarker()
    {
        MarkerSetToPlace = SharedMarkerSet.None;
        AgentMap.Instance()->ResetMapMarkers();
        AgentMap.Instance()->ResetMiniMapMarkers();
    }

    public unsafe void SetFlagMarker(MapLinkPayload map)
    {
        try
        {
            var agent = AgentMap.Instance();
            // removes current flag marker from map
            agent->FlagMarkerCount = 0;

            // divide by 1000 as raw is too long for CS SetFlagMapMarker
            agent->SetFlagMapMarker(map.Map.Value.TerritoryType.RowId, map.Map.RowId, map.RawX / 1000.0f, map.RawY / 1000.0f);
        }
        catch (Exception)
        {
            Log.Error("Exception during SetFlagMarker");
        }
    }

    public static void OpenMap(MapLinkPayload map) => GameGui.OpenMapWithMapLink(map);

    public unsafe bool IsInCriticalEncounter()
    {
        var publicContent = PublicContentOccultCrescent.GetInstance();
        if (publicContent == null)
            return false;

        return publicContent->DynamicEventContainer.CurrentEventIndex != -1;
    }

    public unsafe bool IsInFate()
    {
        var fateManager = FateManager.Instance();
        if (fateManager == null)
            return false;

        return fateManager->FateJoined > 0;
    }

    public void EnablePreview()
    {
        if (ClientState.LocalPlayer == null)
            return;

        NearToCoffer = true;
        CofferPos = ClientState.LocalPlayer.Position;

        PreviewTimer.Start();
    }

    private unsafe void SetMarkers(Vector3 worldPos, Vector3 mapPos, uint iconId, int scale = 0)
    {
        if (!AgentMap.Instance()->AddMapMarker(mapPos, iconId, scale: scale))
            Chat.PrintError(Language.ChatErrorMapMarkers);

        if (!AgentMap.Instance()->AddMiniMapMarker(worldPos, iconId, scale: scale))
            Chat.PrintError(Language.ChatErrorMinimapMarkers);
    }
}