using System;

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

public static class EnumExtensions
{
    public static readonly OccultMarkerSets[] OccultSetArray = Enum.GetValues<OccultMarkerSets>();

    public static string ToName(this OccultMarkerSets set)
    {
        return set switch
        {
            OccultMarkerSets.None => "Show None",
            OccultMarkerSets.Treasure => "All Treasure",
            OccultMarkerSets.Pot => "Pot Locations",
            OccultMarkerSets.Bunny => "Bunny Carrot Locations",
            OccultMarkerSets.OnlyBronze => "Bronze Treasure",
            OccultMarkerSets.OnlySilver => "Silver Treasure",
            OccultMarkerSets.TreasureAndCarrots => "Treasure And Carrots",
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
}

