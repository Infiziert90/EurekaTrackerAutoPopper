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
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
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
using KamiToolKit;

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
    [PluginService] public static IDtrBar DtrBar { get; private set; } = null!;

    public readonly Configuration Configuration;

    private readonly WindowSystem WindowSystem = new("Eureka Linker");
    private readonly MainWindow MainWindow;
    public readonly OccultWindow OccultWindow;
    private readonly QuestWindow QuestWindow;
    private readonly LogWindow LogWindow;
    public readonly BunnyWindow BunnyWindow;
    public readonly ShoutWindow ShoutWindow;
    private readonly FastSwitchOverlay FastSwitchOverlay;

    public readonly Library Library;
    public readonly Fates Fates;
    public readonly TrackerHandler TrackerHandler;
    public readonly TexEdit TexEdit;
    public readonly PenumbraIpc PenumbraIpc;
    public readonly PotDtrBar PotDtrBar;
    public readonly MapMarkerController MapMarkerController;

    public Library.EurekaFate LastSeenFate = Library.EurekaFate.Empty;
    private List<IFate> LastPolledFates = [];

    private static bool GotBunny;
    private readonly Timer CofferTimer = new(20 * 1000);

    public readonly Stopwatch EurekaWatch = new();
    private readonly PluginCommandManager<Plugin> Commands;

    // CircleOverlay
    private bool NearToCoffer;
    private Vector3 CofferPos = Vector3.Zero;
    private readonly Timer PreviewTimer = new(5 * 1000);

    public Plugin()
    {
        KamiToolKitLibrary.Initialize(PluginInterface, "Eureka Linker");

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        TexEdit = new TexEdit();
        TexEdit.EditIcon();
        PenumbraIpc = new PenumbraIpc(TexEdit);
        PenumbraIpc.RegisterMod();

        Library = new Library(Configuration);
        Library.Initialize();

        Fates = new Fates(this);
        TrackerHandler = new TrackerHandler(this);
        PotDtrBar = new PotDtrBar(this);
        MapMarkerController = new MapMarkerController(this);

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
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        PenumbraIpc.Dispose();
        Fates.Dispose();
        TrackerHandler.Dispose();
        PotDtrBar.Dispose();

        Framework.Update -= PollForFateChange;
        Framework.Update -= FairyCheck;
        Framework.Update -= EurekaBunnyCheck;
        Framework.Update -= OccultCheck;
        Framework.Update -= OccultPotCheck;
        Framework.Update -= UpdateDtr;
        ClientState.TerritoryChanged -= TerritoryChangePoll;

        PluginInterface.UiBuilder.Draw -= DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
        PluginInterface.LanguageChanged -= LanguageChanged;

        if (EurekaWatch.IsRunning)
        {
            Configuration.TimeInEureka += EurekaWatch.ElapsedMilliseconds;
            Configuration.Save();
            EurekaWatch.Reset();
        }

        Commands.Dispose();

        MainWindow.Dispose();
        OccultWindow.Dispose();
        QuestWindow.Dispose();
        LogWindow.Dispose();
        BunnyWindow.Dispose();
        ShoutWindow.Dispose();
        FastSwitchOverlay.Dispose();
        WindowSystem.RemoveAllWindows();

        MapMarkerController.Dispose();
        KamiToolKitLibrary.Dispose();
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
        if (TerritoryHelper.HasBunnies())
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
    [HelpMessage("Adds eureka bunny coffer locations to the map and minimap")]
    private void OnAddCommand(string command, string args)
    {
        MapMarkerController.SetMarkerSet(FlagMarkerSet.Eureka);
    }

    [Command("/elremove")]
    [HelpMessage("Removes all the placed markers")]
    private void OnRemoveCommand(string command, string args)
    {
        MapMarkerController.RemoveMapMarker();
    }

    [Command("/ccc")]
    [HelpMessage("Coordinatess")]
    private void OnCoordCommand(string command, string args)
    {
        var local = ObjectTable.LocalPlayer;
        if (local == null)
            return;

        var pos = local.Position;
        ImGui.SetClipboardText($"new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f)");
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

        if (TerritoryHelper.PlayerInEureka())
        {
            if (Configuration.ShowBunnyWindow && TerritoryHelper.HasBunnies())
                BunnyWindow.IsOpen = true;

            if (Configuration.AddIconsOnEntry)
                Framework.RunOnTick(() => { MapMarkerController.SetMarkerSet(FlagMarkerSet.Eureka); }, delay: TimeSpan.FromSeconds(10));

            EurekaWatch.Restart();

            Framework.Update += PollForFateChange;
            Framework.Update += FairyCheck;
            Framework.Update += EurekaBunnyCheck;
            Fates.RegisterEvents();
        }
        else if (ClientState.TerritoryType == (uint)Territory.SouthHorn)
        {
            if (Configuration.ShowBunnyWindow)
                BunnyWindow.IsOpen = true;

            if (Configuration.PlaceDefaultOccult)
                Framework.RunOnTick(() => { MapMarkerController.SetMarkerSet(Configuration.DefaultOccultFlags); }, delay: TimeSpan.FromSeconds(10));

            Framework.Update += OccultCheck;
            Framework.Update += OccultPotCheck;
            Framework.Update += UpdateDtr;

            Fates.RegisterEvents();

            // Set forked tower timer to when the client joined south horn
            Fates.OccultCriticalEncounters[^1].InstanceJoinedTimer = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        else
        {
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
            Framework.Update -= EurekaBunnyCheck;
            Framework.Update -= OccultCheck;
            Framework.Update -= OccultPotCheck;
            Framework.Update -= UpdateDtr;

            PotDtrBar.Hide();

            Fates.RemoveEvents();
        }
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
            .AddUiForeground($"{(Configuration.UseShortNames ? LastSeenFate.ShortName : LastSeenFate.Name)} pop: ", 540)
            .Append(LastSeenFate.MapLink)
            .BuiltString;

        if (Configuration.EchoNMPop)
            Chat.Print(payload);

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

    private void CopyChatMessage()
    {
        SetFlagMarker(LastSeenFate.WorldPos);
        ImGui.SetClipboardText(BuildChatString());
    }

    public void PostChatMessage()
    {
        var worldPos = LastSeenFate.WorldPos;
        if (Configuration.RandomizeMapCoords)
            worldPos = worldPos with { X = Utils.Randomize(worldPos.X), Y = Utils.Randomize(worldPos.Y) };

        SetFlagMarker(worldPos);
        ChatBox.SendMessage(BuildChatString());
    }

    public unsafe void SetFlagMarker(Vector3 worldPos)
    {
        try
        {
            var agent = AgentMap.Instance();

            // removes current flag marker from map
            agent->FlagMarkerCount = 0;
            agent->SetFlagMapMarker(ClientState.TerritoryType, ClientState.MapId, worldPos);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unable to set the flag marker.");
        }
    }

    public void EchoFairy(Library.LocationMemory fairy)
    {
        var payload = new SeStringBuilder()
            .AddUiForeground("Fairy: ", 570)
            .Append(fairy.MapLink)
            .BuiltString;

        if (Configuration.EchoFairies)
            Chat.Print(payload);

        if (Configuration.ShowFairyToast)
            Toast.ShowQuest(payload);

        if (Configuration.PlaceFairyFlag)
            SetFlagMarker(fairy.WorldPos);
    }

    private void EchoTreasure(Library.LocationMemory treasure)
    {
        var payload = new SeStringBuilder()
            .AddUiForeground($"Treasure ({treasure.Type}): ", 570)
            .Append(treasure.MapLink)
            .BuiltString;

        if (Configuration.EchoTreasure)
            Chat.Print(payload);

        if (Configuration.ShowTreasureToast)
            Toast.ShowQuest(payload);

        if (Configuration.PlaceTreasureFlag)
            SetFlagMarker(treasure.WorldPos);
    }

    private void EchoBunnyCarrot(Library.LocationMemory carrot)
    {
        var payload = new SeStringBuilder()
            .AddUiForeground("Carrot spotted: ", 570)
            .Append(carrot.MapLink)
            .BuiltString;

        if (Configuration.EchoBunnyCarrot)
            Chat.Print(payload);

        if (Configuration.ShowBunnyCarrotToast)
            Toast.ShowQuest(payload);

        if (Configuration.PlaceBunnyCarrotFlag)
            SetFlagMarker(carrot.WorldPos);
    }

    private void EurekaBunnyCheck(IFramework framework)
    {
        if (!TerritoryHelper.HasEurekaBunnies())
            return;

        var local = ObjectTable.LocalPlayer;
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
                         .Where(a => BunnyChests.Coffers.Contains(a.BaseId))
                         .Where(a => !BunnyChests.ExistingCoffers.Contains(a.EntityId)))
            {
                if (coffer.EntityId != local.TargetObject.EntityId)
                    continue;

                BunnyChests.ExistingCoffers.Add(coffer.EntityId);
                CofferTimer.Stop();

                Configuration.Stats[ClientState.TerritoryType][coffer.BaseId] += 1;
                Configuration.KilledBunnies -= 1;
                Configuration.Save();
            }
        }
    }

    private void FairyCheck(IFramework framework)
    {
        var local = ObjectTable.LocalPlayer;
        if (local == null)
            return;

        foreach (var actor in ObjectTable.OfType<IBattleNpc>().Where(npc => Library.Fairies.Contains(npc.NameId)))
        {
            if (Library.ExistingFairies.Any(f => f.EntityId == actor.EntityId))
                continue;

            var fairy = new Library.LocationMemory(actor.EntityId, actor.Position);
            Library.ExistingFairies.Add(fairy);
            EchoFairy(fairy);

            MapMarkerController.RefreshMarkers();
        }

        foreach (var fairy in Library.ExistingFairies.ToArray())
        {
            if (!(Utils.GetDistance(local.Position, fairy.WorldPos) < 80.0)
                || ObjectTable.Any(obj => fairy.EntityId == obj.EntityId))
                continue;

            Library.ExistingFairies.Remove(fairy);
            Chat.Print(Language.ChatDeadFairyNote);
        }
    }

    private unsafe void OccultCheck(IFramework _)
    {
        var local = ObjectTable.LocalPlayer;
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
            if (actor.BaseId is > 1856 or < 1789)
                return;

            var treasureObject = (Treasure*)actor.Address;
            if (treasureObject->RenderFlags > 0)
                return;

            if (!Sheets.TreasureSheet.TryGetRow(actor.BaseId, out var treasureRow))
                return;

            if (Library.ExistingOccultLocations.Any(f => f.EntityId == actor.EntityId))
                continue;

            var treasure = new Library.LocationMemory(actor.EntityId, actor.Position, treasureRow.SGB.RowId);
            Library.ExistingOccultLocations.Add(treasure);
            EchoTreasure(treasure);
        }

        foreach (var actor in ObjectTable.Where(gameObject => gameObject.ObjectKind == ObjectKind.EventObj))
        {
            if (actor.BaseId != 2010139)
                continue;

            if (Library.ExistingOccultLocations.Any(f => f.EntityId == actor.EntityId))
                continue;

            var bunnyCarrot = new Library.LocationMemory(actor.EntityId, actor.Position);
            Library.ExistingOccultLocations.Add(bunnyCarrot);
            EchoBunnyCarrot(bunnyCarrot);
        }

        if (!Configuration.ClearMemory)
            return;

        var deleteIndex = -1;
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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
        if (Utils.GetDistance(local.Position, locationObject.WorldPos) > 120.0)
            return locationObject.LastSeen + Configuration.ClearAfterSeconds < currentTime;

        // Treasure is still around the player
        if (ObjectTable.Any(obj => locationObject.EntityId == obj.EntityId))
        {
            locationObject.LastSeen = currentTime;
            return false;
        }

        // Treasure may be in reach but isn't in the object table anymore, so we check timer for removal
        return locationObject.LastSeen + Configuration.ClearAfterSeconds < currentTime;
    }

    private void OccultPotCheck(IFramework _)
    {
        if (!TerritoryHelper.PlayerInOccult())
            return;

        var local = ObjectTable.LocalPlayer;
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
            CofferTimer.Start();

            if (Configuration.AutoSwitchToOccultPots)
                MapMarkerController.SetTempMarkerSet(Configuration.AutoSwitchFlags);
        }
        else
        {
            NearToCoffer = false;
            MapMarkerController.RevertTempMarkerSet();
        }
    }

    private void UpdateDtr(IFramework _)
    {
        PotDtrBar.Update();
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

    public static unsafe void OpenMap(string mapDataLink) =>
        RaptureAtkModule.Instance()->OpenMapWithMapLink(mapDataLink);

    public unsafe bool IsInCriticalEncounter()
    {
        var publicContent = PublicContentOccultCrescent.GetInstance();
        if (publicContent == null)
            return false;

        return publicContent->DynamicEventContainer.CurrentEventIndex != -1;
    }

    private static unsafe bool IsInFate()
    {
        var fateManager = FateManager.Instance();
        return fateManager != null && fateManager->FateJoined > 0;
    }

    public void EnablePreview()
    {
        if (ObjectTable.LocalPlayer == null)
            return;

        NearToCoffer = true;
        CofferPos = ObjectTable.LocalPlayer.Position;

        PreviewTimer.Start();
    }
}