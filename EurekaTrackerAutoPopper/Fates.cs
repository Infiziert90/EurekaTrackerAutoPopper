using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace EurekaTrackerAutoPopper;

public class Fate(uint id, Territory territory, bool easy, string position)
{
    public readonly uint FateId = id;
    public readonly Territory Territory = territory;
    public readonly bool Easy = easy;

    public string Position = position;

    public bool Alive;
    public long LastSeenAlive = -1;
    public bool PlayedSound = false;

    public string Name => Sheets.FateSheet.GetRow(FateId).Name.ExtractText();
}

public class Fates
{
    private readonly Plugin Plugin;

    public Fates(Plugin plugin)
    {
        Plugin = plugin;
    }

    public readonly List<Fate> BunnyFates =
    [
        // Eureka
        new(1367, Territory.Pagos, true, " (South)"),
        new(1368, Territory.Pagos, false, " (North)"),
        new(1407, Territory.Pyros, true, " (South)"),
        new(1408, Territory.Pyros, false, " (North)"),
        new(1425, Territory.Hydatos, true, ""),

        // Occult
        new(1976, Territory.SouthHorn, true, " (North)"),
        new(1977, Territory.SouthHorn, true, " (South)"),
    ];

    public static readonly List<uint> BunnyTerritories =
    [
        (uint)Territory.Pagos,
        (uint)Territory.Pyros,
        (uint)Territory.Hydatos,
        (uint)Territory.SouthHorn
    ];

    public static readonly List<uint> EurekaBunnyTerritories =
    [
        (uint)Territory.Pagos,
        (uint)Territory.Pyros,
        (uint)Territory.Hydatos
    ];

    public static readonly List<uint> BunnyMapIds =
    [
        467,
        484,
        515
    ];

    public void CheckForBunnyFates(IFramework _)
    {
        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        foreach (var bnuuy in BunnyFates)
        {
            if (Plugin.FateTable.Any(fate => fate.FateId == bnuuy.FateId))
            {
                bnuuy.Alive = true;
                bnuuy.LastSeenAlive = currentTime;

                if (!bnuuy.PlayedSound && Plugin.Configuration.PlayBunnyEffect)
                {
                    if (!Plugin.Configuration.OnlyEasyBunny || bnuuy.Easy)
                    {
                        bnuuy.PlayedSound = true;
                        UIGlobals.PlaySoundEffect((uint)Plugin.Configuration.BunnySoundEffect);
                    }
                }
            }

            if (bnuuy.LastSeenAlive != currentTime)
            {
                bnuuy.Alive = false;
                bnuuy.PlayedSound = false;
            }
        }
    }

    public void Reset()
    {
        foreach (var bunny in BunnyFates)
        {
            bunny.Alive = false;
            bunny.LastSeenAlive = -1;
        }
    }
}