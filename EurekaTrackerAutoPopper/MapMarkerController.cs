using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using EurekaTrackerAutoPopper.Resources;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiToolKit.MapOverlay;

using MapMarkerInfo = KamiToolKit.Classes.MapMarkerInfo;

namespace EurekaTrackerAutoPopper;

public unsafe class MapMarkerController : IDisposable
{
    private const float MiniMapMarkerRadius = 300.0f;
    private const float RefreshRadius = 150.0f;
    private const float FlagMarkerRadius = 20.0f;

    private readonly Plugin Plugin;

    private bool EnableController = true;
    private readonly MapOverlayController MapOverlayController;

    public FlagMarkerSet MarkerSetToPlace = FlagMarkerSet.None;
    public FlagMarkerSet? SavedOccultMarkerSets;

    private bool NeedsRefresh;
    private bool HasMarkersToRemove;

    private Vector3 LastPlayerPos = Vector3.Zero;
    private Vector3 LastFlagPos = Vector3.Zero;

    public MapMarkerController(Plugin plugin)
    {
        Plugin = plugin;

        MapOverlayController = new MapOverlayController();
        Plugin.Framework.Update += CheckPlayerRadius;
        Plugin.ClientState.Logout += OnLogout;
        Plugin.ClientState.Login += OnLogin;

        Plugin.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "AreaMap", AddonMapRefresh);
    }
    private void AddonMapRefresh(AddonEvent type, AddonArgs args)
    {
        NeedsRefresh = true;
    }

    public void Dispose()
    {
        Plugin.AddonLifecycle.UnregisterListener(AddonEvent.PostRefresh, "AreaMap", AddonMapRefresh);

        Plugin.Framework.Update -= CheckPlayerRadius;
        Plugin.ClientState.Logout -= OnLogout;
        Plugin.ClientState.Login -= OnLogin;

        RemoveMapMarker();
        Plugin.Framework.RunOnFrameworkThread(() =>
        {
            try
            {
                MapOverlayController.Dispose();
            }
            catch
            {
                // Ignore
            }
        });
    }

    private void OnLogin()
    {
        EnableController = true;
    }

    private void OnLogout(int type, int code)
    {
        EnableController = false;
        MapOverlayController.RemoveAllMarkers();
        MapOverlayController.Disable();
    }

    private void CheckPlayerRadius(IFramework _)
    {
        if (EnableController)
        {
            EnableController = false;
            MapOverlayController.Enable();
        }

        if (!TerritoryHelper.PlayerInSupportedTerritory())
            return;

        var local = Plugin.ObjectTable.LocalPlayer;
        if (local == null)
            return;

        var agentMap = AgentMap.Instance();
        if (agentMap == null)
            return;

        if (MarkerSetToPlace == FlagMarkerSet.None)
        {
            if (HasMarkersToRemove)
                RemoveMapMarker();

            NeedsRefresh = false;
            return;
        }

        var flagPos = Vector3.Zero;
        if (agentMap->FlagMarkerCount > 0)
        {
            var flag = agentMap->FlagMapMarkers[0];
            flagPos = new Vector3(flag.XFloat, 0, flag.YFloat);
        }

        NeedsRefresh |= Utils.GetDistance(local.Position, LastPlayerPos) > RefreshRadius;
        NeedsRefresh |= flagPos != LastFlagPos;
        if (!NeedsRefresh)
            return;

        RemoveMapMarker(false);

        NeedsRefresh = false;
        HasMarkersToRemove = true;
        LastPlayerPos = local.Position;
        LastFlagPos = flagPos;

        var territory = Plugin.ClientState.TerritoryType;
        if (TerritoryHelper.PlayerInEureka())
        {
            if (!MarkerSetToPlace.HasFlag(FlagMarkerSet.Eureka))
                return;

            if (TerritoryHelper.HasBunnies())
                AddChestsLocationsMap(territory);

            AddFairyLocationsMap();
        }
        else
        {
            PlaceOccultMarkerSet(territory);
        }
    }

    public void RefreshMarkers()
    {
        NeedsRefresh = true;
    }

    public void SetMarkerSet(FlagMarkerSet set)
    {
        MarkerSetToPlace = set;
        NeedsRefresh = true;
    }

    public void SetTempMarkerSet(FlagMarkerSet set)
    {
        if (SavedOccultMarkerSets != null)
            return;

        NeedsRefresh = true;

        SavedOccultMarkerSets = MarkerSetToPlace;
        MarkerSetToPlace = set;
    }

    public void RevertTempMarkerSet()
    {
        if (SavedOccultMarkerSets == null)
            return;

        NeedsRefresh = true;

        MarkerSetToPlace = SavedOccultMarkerSets.Value;
        SavedOccultMarkerSets = null;
    }

    public void RemoveMapMarker(bool removeSet = true)
    {
        if (removeSet)
            MarkerSetToPlace = FlagMarkerSet.None;

        MapOverlayController.RemoveAllMarkers();
        AgentMap.Instance()->ResetMapMarkers();
        AgentMap.Instance()->ResetMiniMapMarkers();

        HasMarkersToRemove = false;
    }

    private void PlaceOccultMarkerSet(uint territory)
    {
        if (MarkerSetToPlace.HasFlag(FlagMarkerSet.OccultBronzeTreasure))
            AddOccultBronzeLocations(territory);

        if (MarkerSetToPlace.HasFlag(FlagMarkerSet.OccultSilverTreasure))
            AddOccultSilverLocations(territory);

        if (MarkerSetToPlace.HasFlag(FlagMarkerSet.OccultNorthPot))
            AddOccultPotNorthLocations(territory);

        if (MarkerSetToPlace.HasFlag(FlagMarkerSet.OccultSouthPot))
            AddOccultPotSouthLocations(territory);

        if (MarkerSetToPlace.HasFlag(FlagMarkerSet.OccultReroll))
            AddOccultRerollLocations(territory);

        if (MarkerSetToPlace.HasFlag(FlagMarkerSet.OccultBunny))
            AddOccultBunnyPositions(territory);
    }

    private void AddChestsLocationsMap(uint territory)
    {
        foreach (var worldPos in BunnyChests.Positions[territory])
            SetMarkers(worldPos, Icons.GoldChest);
    }

    private void AddFairyLocationsMap()
    {
        foreach (var (idx, fairy) in Plugin.Library.ExistingFairies.Index())
        {
            if (idx == 3)
                Plugin.Chat.PrintError(Language.ChatErrorFairyMarkers);

            SetMarkers(fairy.WorldPos, Icons.Fairy + (uint)idx);
        }
    }

    private void AddOccultBronzeLocations(uint territory)
    {
        foreach (var (worldPos, _, map) in OccultChests.TreasurePosition[(Territory)territory].Where(pair => pair.Rarity == TreasureRarity.Bronze))
            SetMarkers(worldPos, Icons.BronzeTreasure, (uint)map);
    }

    private void AddOccultSilverLocations(uint territory)
    {
        foreach (var (worldPos, _, map) in OccultChests.TreasurePosition[(Territory)territory].Where(pair => pair.Rarity == TreasureRarity.Silver))
            SetMarkers(worldPos, Icons.SilverTreasure, (uint)map);
    }

    private void AddOccultPotNorthLocations(uint territory)
    {
        foreach (var worldPos in OccultChests.PotNorthPosition[(Territory)territory])
            SetMarkers(worldPos, Icons.GoldChest);
    }

    private void AddOccultPotSouthLocations(uint territory)
    {
        foreach (var worldPos in OccultChests.PotSouthPosition[(Territory)territory])
            SetMarkers(worldPos, Icons.GoldChest);
    }

    private void AddOccultRerollLocations(uint territory)
    {
        foreach (var worldPos in OccultChests.RerollPosition[(Territory)territory])
            SetMarkers(worldPos, Icons.Reroll);
    }

    private void AddOccultBunnyPositions(uint territory)
    {
        foreach (var worldPos in OccultChests.BunnyPosition[(Territory)territory])
            SetMarkers(worldPos, Plugin.PenumbraIpc.GetReplacedIcon);
    }

    private void SetMarkers(Vector3 worldPos, Icons icon, uint map = 0)
    {
        var agentMap = AgentMap.Instance();

        // Only place distant markers if correct map is set
        if (!TerritoryHelper.IsCorrectMap(agentMap->CurrentMapId))
            return;

        var mapPos = worldPos;

        var useKtk = Utils.GetDistance(worldPos, LastPlayerPos) > MiniMapMarkerRadius &&
                     Utils.GetDistance(worldPos with { Y = 0 }, LastFlagPos) > FlagMarkerRadius;
        if (useKtk || agentMap->CurrentMapId != map)
        {
            MapOverlayController.AddMarker(new MapMarkerInfo
            {
                AllowAnyMap = false,
                MapId = map == 0 ? agentMap->CurrentMapId : map,
                Position = new Vector2(mapPos.X, mapPos.Z),
                IconId = (uint)icon,
            });
        }
        else
        {
            if (map != 0 && agentMap->SelectedMapId != map)
                return;

            if ((Territory)Plugin.ClientState.TerritoryType == Territory.Hydatos)
                mapPos.Z += 475;

            agentMap->AddMapMarker(mapPos, (uint)icon);
            agentMap->AddMiniMapMarker(worldPos, (uint)icon);
        }
    }
}