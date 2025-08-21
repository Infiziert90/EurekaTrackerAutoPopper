using System;
using EurekaTrackerAutoPopper.Resources;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace EurekaTrackerAutoPopper;

public enum SharedMarkerSet
{
    None = 0,
    Eureka = 1,
    OccultTreasure = 2,
    OccultPot = 3,
    OccultBunny = 4,
    OccultBronze = 5,
    OccultSilver = 6,
    OccultTreasureCarrots = 7,
    OccultReroll = 8,
    OccultPotReroll = 9,
    OccultPotNorth = 10,
    OccultPotSouth = 11,
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
    Reroll = 7,
    PotAndReroll = 8,
    PotNorth = 9,
    PotSouth = 10,
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

public enum LegendIcons
{
    PotIcon = 60354,
    BronzeIcon = 60356,
    SilverIcon = 60355,
    RerollIcon = 61473,
    CarrotIcon = 25207
}

public static class EnumExtensions
{
    public static readonly LegendIcons[] IconArray = Enum.GetValues<LegendIcons>();
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
            OccultMarkerSets.Reroll => Language.MarketSetReroll,
            OccultMarkerSets.PotAndReroll => Language.MarkerSetPotReroll,
            OccultMarkerSets.PotNorth => Language.MarkerSetPotNorth,
            OccultMarkerSets.PotSouth => Language.MarkerSetPotSouth,
            _ => "Unknown",
        };
    }

    public static string ToName(this LegendIcons set)
    {
        return set switch
        {
            LegendIcons.PotIcon => Language.MarkerSetPot,
            LegendIcons.CarrotIcon => Language.MarkerSetBunny,
            LegendIcons.BronzeIcon => Language.MarkerSetOnlyBronze,
            LegendIcons.SilverIcon => Language.MarkerSetOnlySilver,
            LegendIcons.RerollIcon => Language.MarketSetReroll,
            _ => "Unknown",
        };
    }

    public static OccultMarkerSets ToOccultSet(this SharedMarkerSet set)
    {
        return set switch
        {
            SharedMarkerSet.None or SharedMarkerSet.Eureka => OccultMarkerSets.None,
            SharedMarkerSet.OccultTreasure => OccultMarkerSets.Treasure,
            SharedMarkerSet.OccultPot => OccultMarkerSets.Pot,
            SharedMarkerSet.OccultBunny => OccultMarkerSets.Bunny,
            SharedMarkerSet.OccultBronze => OccultMarkerSets.OnlyBronze,
            SharedMarkerSet.OccultSilver => OccultMarkerSets.OnlySilver,
            SharedMarkerSet.OccultTreasureCarrots => OccultMarkerSets.TreasureAndCarrots,
            SharedMarkerSet.OccultReroll => OccultMarkerSets.Reroll,
            SharedMarkerSet.OccultPotReroll => OccultMarkerSets.PotAndReroll,
            SharedMarkerSet.OccultPotNorth => OccultMarkerSets.PotNorth,
            SharedMarkerSet.OccultPotSouth => OccultMarkerSets.PotSouth,
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

