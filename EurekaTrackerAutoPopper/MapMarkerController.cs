using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using EurekaTrackerAutoPopper.Resources;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiToolKit.Overlay.MapOverlay;

using MapMarkerInfo = KamiToolKit.Classes.MapMarkerInfo;

namespace EurekaTrackerAutoPopper;

public unsafe class MapMarkerController : IDisposable
{
    private const float MiniMapMarkerRadius = 300.0f;
    private const float RefreshRadius = 150.0f;

    private readonly Plugin Plugin;
    private readonly MapOverlayController MapOverlayController;

    public FlagMarkerSet MarkerSetToPlace = FlagMarkerSet.None;
    public FlagMarkerSet? SavedOccultMarkerSets;

    private bool NeedsRefresh;
    private bool HasMarkersToRemove;

    private Vector3 LastPlayerPos = Vector3.Zero;

    public MapMarkerController(Plugin plugin)
    {
        Plugin = plugin;

        MapOverlayController = new MapOverlayController();
        Plugin.Framework.Update += CheckPlayerRadius;

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

        RemoveMapMarker();
        MapOverlayController.Dispose();
    }

    private void CheckPlayerRadius(IFramework _)
    {
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

        NeedsRefresh |= Utils.GetDistance(local.Position, LastPlayerPos) > RefreshRadius;
        if (!NeedsRefresh)
            return;

        RemoveMapMarker(false);

        NeedsRefresh = false;
        HasMarkersToRemove = true;
        LastPlayerPos = local.Position;

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
        foreach (var (worldPos, _) in OccultChests.TreasurePosition[territory].Where(pair => pair.Item2 == 1596))
            SetMarkers(worldPos, Icons.BronzeTreasure);
    }

    private void AddOccultSilverLocations(uint territory)
    {
        foreach (var (worldPos, _) in OccultChests.TreasurePosition[territory].Where(pair => pair.Item2 == 1597))
            SetMarkers(worldPos, Icons.SilverTreasure);
    }

    private void AddOccultPotNorthLocations(uint territory)
    {
        foreach (var worldPos in OccultChests.PotNorthPosition[territory])
            SetMarkers(worldPos, Icons.GoldChest);
    }

    private void AddOccultPotSouthLocations(uint territory)
    {
        foreach (var worldPos in OccultChests.PotSouthPosition[territory])
            SetMarkers(worldPos, Icons.GoldChest);
    }

    private void AddOccultRerollLocations(uint territory)
    {
        foreach (var worldPos in OccultChests.RerollPosition[territory])
            SetMarkers(worldPos, Icons.Reroll);
    }

    private void AddOccultBunnyPositions(uint territory)
    {
        foreach (var worldPos in OccultChests.BunnyPosition[territory])
            SetMarkers(worldPos, Plugin.PenumbraIpc.GetReplacedIcon);
    }

    private void SetMarkers(Vector3 worldPos, Icons icon)
    {
        var mapPos = worldPos;
        var agentMap = AgentMap.Instance();
        if (Utils.GetDistance(worldPos, LastPlayerPos) > MiniMapMarkerRadius)
        {
            // TODO: Remove once KTK offset bug is fixed
            if ((Territory)Plugin.ClientState.TerritoryType == Territory.Hydatos)
                mapPos.Z -= 475;

            MapOverlayController.AddMarker(new MapMarkerInfo
            {
                AllowAnyMap = false,
                MapId = agentMap->CurrentMapId,
                Position = new Vector2(mapPos.X, mapPos.Z),
                IconId = (uint)icon
            });
        }
        else
        {
            if ((Territory)Plugin.ClientState.TerritoryType == Territory.Hydatos)
                mapPos.Z += 475;

            agentMap->AddMapMarker(mapPos, (uint)icon);
            agentMap->AddMiniMapMarker(worldPos, (uint)icon);
        }
    }
}