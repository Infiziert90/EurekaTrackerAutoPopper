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
}

public enum OccultMarkerSets
{
    None = 0,
    OccultTreasure = 1,
    OccultPot = 2,
    OccultBunny = 3,
    OccultOnlyBronze = 4,
    OccultOnlySilver = 5,
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
            OccultMarkerSets.OccultTreasure => "All Treasure",
            OccultMarkerSets.OccultPot => "Pot Locations",
            OccultMarkerSets.OccultBunny => "Bunny Carrot Locations",
            OccultMarkerSets.OccultOnlyBronze => "Bronze Treasure",
            OccultMarkerSets.OccultOnlySilver => "Silver Treasure",
            _ => "Unknown",
        };
    }

    public static OccultMarkerSets ToOccultSet(this SharedMarketSet set)
    {
        return set switch
        {
            SharedMarketSet.None or SharedMarketSet.Eureka => OccultMarkerSets.None,
            SharedMarketSet.OccultTreasure => OccultMarkerSets.OccultTreasure,
            SharedMarketSet.OccultPot => OccultMarkerSets.OccultPot,
            SharedMarketSet.OccultBunny => OccultMarkerSets.OccultBunny,
            SharedMarketSet.OccultBronze => OccultMarkerSets.OccultOnlyBronze,
            SharedMarketSet.OccultSilver => OccultMarkerSets.OccultOnlySilver,
            _ =>  OccultMarkerSets.None,
        };
    }
}

