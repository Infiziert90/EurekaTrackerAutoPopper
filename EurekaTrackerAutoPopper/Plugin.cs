using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using CheapLoc;
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
using EurekaTrackerAutoPopper.Windows;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;

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
    [PluginService] public static IGameInteropProvider Hook { get; private set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    public Configuration Configuration { get; init; }

    private WindowSystem WindowSystem { get; init; } = new("Eureka Linker");
    private MainWindow MainWindow { get; init; }
    private QuestWindow QuestWindow { get; init; }
    private LogWindow LogWindow { get; init; }
    public BunnyWindow BunnyWindow { get; init; }
    public ShoutWindow ShoutWindow { get; init; }

    public readonly Library Library;
    public bool PlayerInEureka;
    public Library.EurekaFate LastSeenFate = Library.EurekaFate.Empty;
    private List<IFate> LastPolledFates = [];

    private static bool GotBunny;
    private readonly Timer CofferTimer = new(20 * 1000);

    public readonly Stopwatch EurekaWatch = new();

    private readonly Localization Localization = new();
    private readonly PluginCommandManager<Plugin> Commands;

    // CircleOverlay
    public bool NearToCoffer;
    public Vector3 CofferPos = Vector3.Zero;
    private readonly Timer PreviewTimer = new(5 * 1000);

    // Bunny Chest Markers
    public bool ShouldPlaceMarkers;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        Library = new Library(Configuration);
        Library.Initialize();

        MainWindow = new MainWindow(this);
        QuestWindow = new QuestWindow();
        LogWindow = new LogWindow();
        BunnyWindow = new BunnyWindow(this);
        ShoutWindow = new ShoutWindow(this);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(QuestWindow);
        WindowSystem.AddWindow(LogWindow);
        WindowSystem.AddWindow(BunnyWindow);
        WindowSystem.AddWindow(ShoutWindow);

        Commands = new PluginCommandManager<Plugin>(this, CommandManager);
        Localization.SetupWithLangCode(PluginInterface.UiLanguage);

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        PluginInterface.LanguageChanged += Localization.SetupWithLangCode;

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

        AddonLifecycle.UnregisterListener(AddonEvent.PostRefresh, "AreaMap", RefreshMapMarker);

        if (EurekaWatch.IsRunning)
        {
            Configuration.TimeInEureka += EurekaWatch.ElapsedMilliseconds;
            Configuration.Save();
            EurekaWatch.Reset();
        }

        Commands.Dispose();
        WindowSystem.RemoveWindow(QuestWindow);
    }

    private unsafe void RefreshMapMarker(AddonEvent type, AddonArgs args)
    {
        if (!ShouldPlaceMarkers)
            return;

        // Check the players current territory type
        if (!Library.BunnyTerritories.Contains(ClientState.TerritoryType))
            return;

        // Check the map selection, important for Hydatos as it has different map IDs with BA
        if (Library.BunnyMapIds.Contains(AgentMap.Instance()->SelectedMapId))
        {
            RemoveMarkerMap();
            AddChestsLocationsMap();
            AddFairyLocationsMap();
        }
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
        if (Library.BunnyTerritories.Contains(ClientState.TerritoryType))
            BunnyWindow.IsOpen ^= true;
        else
            Chat.PrintError(Loc.Localize("Chat - Error Not In Eureka",
                "You are not in Eureka, this command is unavailable."));
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

            if (Configuration.ShowBunnyWindow && Library.BunnyTerritories.Contains(ClientState.TerritoryType))
                BunnyWindow.IsOpen = true;

            if (Configuration.AddIconsOnEntry && Library.BunnyTerritories.Contains(ClientState.TerritoryType))
                Task.Run(async () =>
                {
                    // Delay it by 10s so that map had a chance to fully load
                    await Task.Delay(new TimeSpan(0, 0, 10));
                    await Framework.RunOnFrameworkThread(AddChestsLocationsMap);
                });

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

            GotBunny = false;
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
        return FateTable.SequenceEqual(LastPolledFates);
    }

    private void CheckForRelevantFates(ushort currentTerritory)
    {
        List<ushort> newFateIds = FateTable.Except(LastPolledFates).Select(i => i.FateId).ToList();
        IEnumerable<Library.EurekaFate> relevantFates = Library.TerritoryToFateDictionary(currentTerritory);
        foreach (Library.EurekaFate fate in relevantFates.Where(i => newFateIds.Contains(i.FateId)))
        {
            LastSeenFate = fate;

            ProcessNewFate(fate);
        }
    }

    public void ProcessCurrentFates(ushort currentTerritory)
    {
        var currentFates = FateTable.ToList();
        var relevantFates = Library.TerritoryToFateDictionary(currentTerritory);
        var relevantCurrentFates = relevantFates
            .Where(fate => currentFates.Select(i => i.FateId).Contains(fate.FateId)).ToList();
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
        SetFlagMarker();
        ImGui.SetClipboardText(BuildChatString());
    }

    public void PostChatMessage()
    {
        SetFlagMarker();
        ChatBox.SendMessage(BuildChatString());
    }

    public void EchoFairy(Library.Fairy fairy)
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
    }

    private void BunnyCheck(IFramework framework)
    {
        if (!Library.BunnyTerritories.Contains(ClientState.TerritoryType))
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
                        UIGlobals.PlaySoundEffect((uint)Configuration.BunnySoundEffect);
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

                // TODO Remove after all chests found
                if (!BunnyChests.Exists(ClientState.TerritoryType, coffer.Position))
                {
                    Chat.Print(Loc.Localize("Chat - New Chest Found", "You've found a new chest location"));
                    Chat.Print(Loc.Localize("Chat - New Chest Found Dev Note",
                        "Please consider sending the following information to the developer:"));
                    Chat.Print(Loc.Localize("Chat - New Chest Found Feedback",
                        "(Please do not use the feedback function to send this)"));
                    Chat.Print(
                        $"Terri: {ClientState.TerritoryType} Pos: {coffer.Position.X:000.000000}f, {coffer.Position.Y:000.#########}f, {coffer.Position.Z:000.#########}f");
                    BunnyChests.Positions[ClientState.TerritoryType].Add(coffer.Position);
                }
            }
        }
    }

    private void FairyCheck(IFramework framework)
    {
        foreach (var actor in ObjectTable.OfType<IBattleNpc>()
                     .Where(battleNpc => Library.Fairies.Contains(battleNpc.NameId))
                     .Where(battleNpc => Library.ExistingFairies.All(f => f.ObjectId != battleNpc.EntityId)))
        {
            var fairy = new Library.Fairy(actor.EntityId, actor.NameId, actor.Position);
            Library.ExistingFairies.Add(fairy);
            EchoFairy(fairy);
        }

        var local = ClientState.LocalPlayer;
        if (local == null)
            return;

        foreach (var fairy in Library.ExistingFairies.ToArray())
        {
            if (!(Utils.GetDistance(local.Position, fairy.Pos) < 80.0)
                || ObjectTable.Any(obj => fairy.ObjectId == obj.EntityId))
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
        MainWindow.IsOpen = true;
    }

    public unsafe void AddChestsLocationsMap()
    {
        if (!Library.BunnyTerritories.Contains(ClientState.TerritoryType))
        {
            Chat.PrintError(Loc.Localize("Chat - Error Not In Eureka",
                "You are not in Eureka, this command is unavailable."));
            return;
        }

        ShouldPlaceMarkers = true;
        foreach (var pos in BunnyChests.Positions[ClientState.TerritoryType])
        {
            var orgPos = pos;
            if (ClientState.TerritoryType == 827)
                orgPos.Z += 475;

            if (!AgentMap.Instance()->AddMapMarker(orgPos, 60354))
                Chat.PrintError(Loc.Localize("Chat - Error Map Markers", "Unable to place all markers on map"));
            if (!AgentMap.Instance()->AddMiniMapMarker(pos, 60354))
                Chat.PrintError(Loc.Localize("Chat - Error Minimap Markers",
                    "Unable to place all markers on minimap"));
        }
    }

    public unsafe void AddFairyLocationsMap()
    {
        if (!PlayerInEureka)
        {
            Chat.PrintError(Loc.Localize("Chat - Error Not In Eureka", "You are not in Eureka, this command is unavailable."));
            return;
        }

        ShouldPlaceMarkers = true;
        foreach (var (fairy, idx) in Library.ExistingFairies.Select((val, i) => (val, (uint)i)))
        {
            if (idx == 3)
                Chat.PrintError(Loc.Localize("Chat - Error Fairy Markers", "Tracking for fairies needs to be reset, please go to all listed locations to update."));

            var orgPos = fairy.Pos;
            if (ClientState.TerritoryType == 827)
                orgPos.Z += 475;

            if (!AgentMap.Instance()->AddMapMarker(orgPos, 60474 + idx))
                Chat.PrintError(Loc.Localize("Chat - Error Fairy Map Markers", "Unable to place fairy markers on map"));
            if (!AgentMap.Instance()->AddMiniMapMarker(fairy.Pos, 60474 + idx))
                Chat.PrintError(Loc.Localize("Chat - Error Fairy Minimap Markers", "Unable to place fairy markers on minimap"));
        }
    }

    public unsafe void RemoveMarkerMap()
    {
        if (!Library.BunnyTerritories.Contains(ClientState.TerritoryType))
        {
            Chat.PrintError(Loc.Localize("Chat - Error Not In Eureka", "You are not in Eureka, this command is unavailable."));
            return;
        }

        ShouldPlaceMarkers = false;
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
            var map = (MapLinkPayload)LastSeenFate.MapLink.Payloads.First();
            AgentMap.Instance()->SetFlagMapMarker(
                map.Map.Value.TerritoryType.RowId,
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

    public void EnablePreview()
    {
        if (ClientState.LocalPlayer == null)
            return;

        NearToCoffer = true;
        CofferPos = ClientState.LocalPlayer.Position;

        PreviewTimer.Start();
    }
}