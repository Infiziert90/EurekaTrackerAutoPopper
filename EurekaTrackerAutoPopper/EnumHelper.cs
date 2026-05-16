using System;
using System.Collections.Generic;
using System.Linq;
using EurekaTrackerAutoPopper.Resources;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace EurekaTrackerAutoPopper;

[Flags]
public enum FlagMarkerSet : uint
{
    None = 0,

    Eureka = 1 << 0,

    OccultBronzeTreasure = 1 << 1,
    OccultSilverTreasure = 1 << 2,
    OccultNorthPot = 1 << 3,
    OccultSouthPot = 1 << 4,
    OccultReroll = 1 << 5,
    OccultBunny = 1 << 6,
}

public enum Icons : uint
{
    Fairy = 60474u,

    GoldChest = 60354u,

    BronzeTreasure = 60356u,
    SilverTreasure = 60355u,
    Reroll = 61473u,

    Carrot = 25207u,
    CarrotReplaced = 199201u,
}

public enum Territory : uint
{
    Anemos = 732,
    Pagos = 763,
    Pyros = 795,
    Hydatos = 827,

    SouthHorn = 1252,
}

public enum Map : uint
{
    Anemos = 414,
    Pagos = 467,
    Pyros = 484,
    Hydatos = 515,

    SouthHorn = 967,
}

public enum OccultAetheryte : uint
{
    None = 0,
    ExpeditionBaseCamp = 4944,
    TheWanderersHaven = 4936,
    CrystallizedCaverns = 4939,
    Eldergrowth = 4940,
    Stonemarsh = 4947,
}

public static class TerritoryHelper
{
    private static readonly HashSet<Territory> SupportedTerritories = Enum.GetValues<Territory>().ToHashSet();
    private static readonly HashSet<Territory> EurekaTerritories = [Territory.Anemos, Territory.Pagos, Territory.Pyros, Territory.Hydatos];
    private static readonly HashSet<Territory> EurekaBunnyTerritories = [Territory.Pagos, Territory.Pyros, Territory.Hydatos];
    private static readonly HashSet<Territory> BunnyTerritories = [Territory.Pagos, Territory.Pyros, Territory.Hydatos, Territory.SouthHorn];
    private static readonly HashSet<Territory> OccultTerritories = [Territory.SouthHorn];

    private static Territory CurrentTerritory
        => (Territory)Plugin.ClientState.TerritoryType;

    public static bool PlayerInSupportedTerritory()
        => SupportedTerritories.Contains(CurrentTerritory);

    public static bool PlayerInEureka()
        => EurekaTerritories.Contains(CurrentTerritory);

    public static bool HasBunnies()
        => BunnyTerritories.Contains(CurrentTerritory);

    public static bool HasEurekaBunnies()
        => EurekaBunnyTerritories.Contains(CurrentTerritory);

    public static bool PlayerInOccult()
        => OccultTerritories.Contains(CurrentTerritory);

    public static bool IsCorrectMap()
        => CurrentTerritory.ToMap() == (Map)Plugin.ClientState.MapId;
}

public static class EnumExtensions
{
    public static readonly Icons[] IconArray = [Icons.BronzeTreasure, Icons.SilverTreasure, Icons.GoldChest, Icons.Reroll, Icons.Carrot];

    public static string ToName(this FlagMarkerSet flag)
    {
        return flag switch
        {
            FlagMarkerSet.None => Language.MarkerSetNone,
            FlagMarkerSet.Eureka => Language.MarkerSetEureka,
            FlagMarkerSet.OccultBronzeTreasure => Language.MarkerSetOnlyBronze,
            FlagMarkerSet.OccultSilverTreasure => Language.MarkerSetOnlySilver,
            FlagMarkerSet.OccultNorthPot => Language.MarkerSetPotNorth,
            FlagMarkerSet.OccultSouthPot => Language.MarkerSetPotSouth,
            FlagMarkerSet.OccultReroll => Language.MarketSetReroll,
            FlagMarkerSet.OccultBunny => Language.MarkerSetBunny,
            _ => "Unknown"
        };
    }

    public static string ToOccultName(this Icons set)
    {
        return set switch
        {
            Icons.BronzeTreasure => Language.MarkerSetOnlyBronze,
            Icons.SilverTreasure => Language.MarkerSetOnlySilver,
            Icons.GoldChest => Language.MarkerSetPot,
            Icons.Reroll => Language.MarketSetReroll,
            Icons.Carrot or Icons.CarrotReplaced=> Language.MarkerSetBunny,
            _ => "Unknown",
        };
    }

    public static string ToName(this DynamicEventState state)
    {
        return state switch
        {
            DynamicEventState.Inactive => Language.CEStateInactive,
            DynamicEventState.Register => Language.CEStateRegister,
            DynamicEventState.Warmup => Language.CEStateWarmup,
            DynamicEventState.Battle => Language.CEStateBattle,
            _ => "Unknown State",
        };
    }

    public static string ToName(this OccultAetheryte id)
    {
        if (id == OccultAetheryte.None)
            return "None";

        return !Sheets.PlaceNameSheet.TryGetRow((uint) id, out var placeNameRow) ? "Unknown" : placeNameRow.Name.ToString();
    }

    public static Map ToMap(this Territory territory)
    {
        return territory switch
        {
            Territory.Anemos => Map.Anemos,
            Territory.Pagos => Map.Pagos,
            Territory.Pyros => Map.Pyros,
            Territory.Hydatos => Map.Hydatos,
            Territory.SouthHorn => Map.SouthHorn,
            _ => throw new ArgumentOutOfRangeException(nameof(territory), territory, null),
        };
    }
}

