using System;
using EurekaTrackerAutoPopper.Resources;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace EurekaTrackerAutoPopper;

public enum SharedMarketSet
{
    None = 0,
    Eureka = 1,
    OccultTreasure = 2,
    OccultPot = 3,
    OccultBunny = 4,
    OccultBronze = 5,
    OccultSilver = 6,
    OccultTreasureCarrots = 7,
}

public enum OccultMarkerSets
{
    None = 0,
    Treasure = 1,
    Pot = 2,
    Bunny = 3,
    OnlyBronze = 4,
    OnlySilver = 5,
    TreasureAndCarrots = 6,
}

public enum Territory : uint
{
    Anemos = 732,
    Pagos = 763,
    Pyros = 795,
    Hydatos = 827,
    SouthHorn = 1252,
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

public static class EnumExtensions
{
    public static readonly OccultMarkerSets[] OccultSetArray = Enum.GetValues<OccultMarkerSets>();

    public static string ToName(this OccultMarkerSets set)
    {
        return set switch
        {
            OccultMarkerSets.None => Language.MarkerSetNone,
            OccultMarkerSets.Treasure => Language.MarkerSetTreasure,
            OccultMarkerSets.Pot => Language.MarkerSetPot,
            OccultMarkerSets.Bunny => Language.MarkerSetBunny,
            OccultMarkerSets.OnlyBronze => Language.MarkerSetOnlyBronze,
            OccultMarkerSets.OnlySilver => Language.MarkerSetOnlySilver,
            OccultMarkerSets.TreasureAndCarrots => Language.MarkerSetCombined,
            _ => "Unknown",
        };
    }

    public static OccultMarkerSets ToOccultSet(this SharedMarketSet set)
    {
        return set switch
        {
            SharedMarketSet.None or SharedMarketSet.Eureka => OccultMarkerSets.None,
            SharedMarketSet.OccultTreasure => OccultMarkerSets.Treasure,
            SharedMarketSet.OccultPot => OccultMarkerSets.Pot,
            SharedMarketSet.OccultBunny => OccultMarkerSets.Bunny,
            SharedMarketSet.OccultBronze => OccultMarkerSets.OnlyBronze,
            SharedMarketSet.OccultSilver => OccultMarkerSets.OnlySilver,
            SharedMarketSet.OccultTreasureCarrots => OccultMarkerSets.TreasureAndCarrots,
            _ =>  OccultMarkerSets.None,
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

        return !Sheets.PlaceNameSheet.TryGetRow((uint) id, out var placeNameRow) ? "Unknown" : placeNameRow.Name.ExtractText();
    }
}

