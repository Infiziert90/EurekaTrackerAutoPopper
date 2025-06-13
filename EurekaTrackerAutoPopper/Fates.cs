using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Text.ReadOnly;

namespace EurekaTrackerAutoPopper;

public class Fate
{
    public readonly uint FateId;
    public readonly Territory Territory;
    public readonly bool Easy;

    public string Position = string.Empty;

    public bool Alive;
    public long LastSeenAlive = -1;
    public bool PlayedSound;
    public readonly List<long> PreviousRespawnTimes = [];

    public long TimeLeft;
    public byte Progress;

    public uint MapIcon;
    public string Name = "Unknown";

    public DynamicEventState State;
    public long SpawnTime;
    public long StateTimeLeft;

    public SeString? MapLink;
    public Vector2 WorldPos = Vector2.Zero;
    public uint[] SpecialRewards = [];

    public OccultAetheryte Aetheryte = OccultAetheryte.None;
    public int WalkingDistance;

    public Fate(uint id, Territory territory, bool easy, string position)
    {
        FateId = id;
        Territory = territory;
        Easy = easy;
        Position = position;
    }

    public Fate(uint id, Territory territory, Vector2 worldPosition, uint[] rewards, OccultAetheryte aetheryte = OccultAetheryte.ExpeditionBaseCamp, int distance = 0)
    {
        FateId = id;
        Territory = territory;

        WorldPos = worldPosition;
        MapLink = Fates.CreateOccultLink(worldPosition.X, worldPosition.Y);
        SpecialRewards = rewards;

        Aetheryte = aetheryte;
        WalkingDistance = distance;
    }

    public string GetName()
    {
        if (Name != string.Empty)
            return Name;

        Name = Sheets.FateSheet.GetRow(FateId).Name.ExtractText();
        return Name;
    }

    public void Update(IFate fate, long currentTime)
    {
        if (!Alive && LastSeenAlive > -1)
            PreviousRespawnTimes.Add(currentTime - LastSeenAlive);

        Alive = true;
        LastSeenAlive = currentTime;

        TimeLeft = fate.TimeRemaining;
        Progress = fate.Progress;

        if (Name == string.Empty || MapIcon == 0)
        {
            Name = fate.Name.TextValue;
            MapIcon = fate.MapIconId;
        }
    }

    public void Update(ref DynamicEvent criticalEncounter, long currentTime)
    {
        if (!Alive)
        {
            SpawnTime = currentTime;

            if (LastSeenAlive > -1)
                PreviousRespawnTimes.Add(currentTime - LastSeenAlive);
        }

        Alive = true;
        LastSeenAlive = currentTime;

        TimeLeft = criticalEncounter.SecondsLeft;
        Progress = criticalEncounter.Progress;

        State = criticalEncounter.State;
        StateTimeLeft = criticalEncounter.StartTimestamp - currentTime;

        if (Name == string.Empty || MapIcon == 0)
        {
            Name = new ReadOnlySeString(criticalEncounter.Name).ExtractText();
            MapIcon = criticalEncounter.IconObjective0;
        }
    }

    public void Reset()
    {
        Alive = false;
        LastSeenAlive = -1;
        MapIcon = 0;
    }
}

public class Fates
{
    private readonly Plugin Plugin;

    public Fates(Plugin plugin)
    {
        Plugin = plugin;
    }

    public void Dispose()
    {
        RemoveEvents();
    }

    public void RegisterEvents()
    {
        Plugin.Framework.Update += ScanOccultCEs;
        Plugin.Framework.Update += ScanOccultFates;
    }

    public void RemoveEvents()
    {
        Plugin.Framework.Update -= ScanOccultCEs;
        Plugin.Framework.Update -= ScanOccultFates;
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

    public static readonly HashSet<uint> BunnyTerritories =
    [
        (uint)Territory.Pagos,
        (uint)Territory.Pyros,
        (uint)Territory.Hydatos,
        (uint)Territory.SouthHorn
    ];

    public static readonly HashSet<uint> EurekaBunnyTerritories =
    [
        (uint)Territory.Pagos,
        (uint)Territory.Pyros,
        (uint)Territory.Hydatos
    ];

    public static readonly HashSet<uint> BunnyMapIds =
    [
        467,
        484,
        515
    ];

    public readonly List<Fate> OccultFates =
    [
        new(1962, Territory.SouthHorn, new Vector2(153.12436f, 669.49994f), [47744], OccultAetheryte.Eldergrowth, 28), // Rough Waters
        new(1963, Territory.SouthHorn, new Vector2(366.70068f, 489.8156f), [47744], OccultAetheryte.Eldergrowth, 14), // The Golden Guardian
        new(1964, Territory.SouthHorn, new Vector2(-216.79004f, 265.99548f), [47749], OccultAetheryte.Stonemarsh, 10), // King of the Crescent
        new(1965, Territory.SouthHorn, new Vector2(-546.3211f, -594.8453f), [47747], OccultAetheryte.TheWanderersHaven, 27), // The Winged Terror
        new(1966, Territory.SouthHorn, new Vector2(-220.38368f, 40.531372f), [47746], OccultAetheryte.CrystallizedCaverns, 26), // An Unending Duty
        new(1967, Territory.SouthHorn, new Vector2(-41.95025f, -318.47665f), [47747], OccultAetheryte.CrystallizedCaverns, 24), // Brain Drain
        new(1968, Territory.SouthHorn, new Vector2(-369.75647f, 655.7229f), [47745], OccultAetheryte.Stonemarsh, 25), // A Delicate Balance
        new(1969, Territory.SouthHorn, new Vector2(-589.3044f, 331.68253f), [47745], OccultAetheryte.Stonemarsh, 18), // Sworn to Soil
        new(1970, Territory.SouthHorn, new Vector2(-57.268074f, 562.66583f), [47744], OccultAetheryte.Stonemarsh, 29), // A Prying Eye
        new(1971, Territory.SouthHorn, new Vector2(77.73725f, 275.27878f), [47749], OccultAetheryte.Eldergrowth, 17), // Fatal Allure
        new(1972, Territory.SouthHorn, new Vector2(414.7572f, -15.474099f), [47748], OccultAetheryte.Eldergrowth, 24), // Serving Darkness
    ];

    public readonly List<Fate> OccultCriticalEncounters =
    [
        new(33, Territory.SouthHorn, new Vector2(302.1504f, 733.8478f), [47744], OccultAetheryte.Eldergrowth, 30), // Scourge of the Mind
        new(34, Territory.SouthHorn, new Vector2(450.04425f, 355.49652f), [47749, 47752, 47732], OccultAetheryte.Eldergrowth, 10), // The Black Regiment
        new(35, Territory.SouthHorn, new Vector2(620.46155f, 799.9018f), [47744, 47751, 47730], OccultAetheryte.Eldergrowth, 48), // The Unbridled
        new(36, Territory.SouthHorn, new Vector2(680.983f, 533.4121f), [47744], OccultAetheryte.Eldergrowth, 33), // Crawling Death
        new(37, Territory.SouthHorn, new Vector2(-338.75793f, 799.1263f), [47745, 47728, 48008], OccultAetheryte.Stonemarsh, 33), // Calamity Bound
        new(38, Territory.SouthHorn, new Vector2(-413.84256f, 74.792145f), [47746], OccultAetheryte.CrystallizedCaverns, 17), // Trial by Claw
        new(39, Territory.SouthHorn, new Vector2(-799.7762f, 245.09113f), [47746, 47729], OccultAetheryte.Stonemarsh, 37), // From Times Bygone
        new(40, Territory.SouthHorn, new Vector2(676.0466f, -255.98221f), [47748], OccultAetheryte.ExpeditionBaseCamp, 36), // Company of Stone
        new(41, Territory.SouthHorn, new Vector2(-117.4481f, -848.1179f), [47747, 47731], OccultAetheryte.TheWanderersHaven, 17), // Shark Attack
        new(42, Territory.SouthHorn, new Vector2(629.22296f, -53.54043f), [47748, 47757], OccultAetheryte.Eldergrowth, 42), // On the Hunt
        new(43, Territory.SouthHorn, new Vector2(-350.47372f, -606.685f), [47747], OccultAetheryte.TheWanderersHaven, 12), // With Extreme Prejudice
        new(44, Territory.SouthHorn, new Vector2(458.65436f, -360.27054f), [47749], OccultAetheryte.ExpeditionBaseCamp, 36), // Noise Complaint
        new(45, Territory.SouthHorn, new Vector2(72.1046f, -551.4241f), [47747, 47733], OccultAetheryte.TheWanderersHaven, 17), // Cursed Concern
        new(46, Territory.SouthHorn, new Vector2(868.556f, 178.51392f), [47748], OccultAetheryte.Eldergrowth, 57), // Eternal Watch
        new(47, Territory.SouthHorn, new Vector2(-568.61255f, -160.21591f), [47746], OccultAetheryte.CrystallizedCaverns, 14), // Flame of Dusk

        new(48, Territory.SouthHorn, new Vector2(62.95297f, 4.004886f), [47868, 47734, 47735, 47736, 47737], OccultAetheryte.Eldergrowth, 25), // The Forked Tower: Blood
    ];

    private static SeString CreateMapLink(uint territoryId, uint mapId, double xRaw, double yRaw)
    {
        return SeString.CreateMapLink(territoryId, mapId, (int) xRaw * 1000, (int) yRaw * 1000);
    }

    public static SeString CreateOccultLink(double x, double y) => CreateMapLink(1252, 967, x, y);

    public void ScanOccultFates(IFramework _)
    {
        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        foreach (var bnuuy in BunnyFates)
        {
            foreach (var fate in Plugin.FateTable)
            {
                if (fate.FateId != bnuuy.FateId)
                    continue;

                bnuuy.Update(fate, currentTime);
                if (bnuuy.PlayedSound || !Plugin.Configuration.PlayBunnyEffect)
                    continue;

                if (Plugin.Configuration.OnlyEasyBunny && !bnuuy.Easy)
                    continue;

                bnuuy.PlayedSound = true;
                UIGlobals.PlaySoundEffect((uint)Plugin.Configuration.BunnySoundEffect);
            }

            if (!bnuuy.Alive)
                continue;

            if (bnuuy.LastSeenAlive == currentTime)
                continue;

            bnuuy.Alive = false;
            bnuuy.PlayedSound = false;
        }

        foreach (var occultFate in OccultFates)
        {
            foreach (var fate in Plugin.FateTable)
            {
                if (fate.FateId != occultFate.FateId)
                    continue;

                occultFate.Update(fate, currentTime);
            }

            if (!occultFate.Alive)
                continue;

            if (occultFate.LastSeenAlive == currentTime)
                continue;

            occultFate.Alive = false;
            occultFate.PlayedSound = false;
        }
    }

    public unsafe void ScanOccultCEs(IFramework _)
    {
        var publicContent = PublicContentOccultCrescent.GetInstance();
        if (publicContent == null)
            return;

        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        foreach (var occultCE in OccultCriticalEncounters)
        {
            var dynamicEvents = publicContent->DynamicEventContainer.Events;
            for (var idx = 0; idx < dynamicEvents.Length; idx++)
            {
                ref var criticalEncounter = ref dynamicEvents[idx];
                if (criticalEncounter.State == DynamicEventState.Inactive)
                    continue;

                if (criticalEncounter.DynamicEventId != occultCE.FateId)
                    continue;

                occultCE.Update(ref criticalEncounter, currentTime);
            }

            if (!occultCE.Alive)
                continue;

            if (occultCE.LastSeenAlive == currentTime)
                continue;

            occultCE.Alive = false;
            occultCE.PlayedSound = false;
            occultCE.State = DynamicEventState.Inactive;
        }
    }

    public void Reset()
    {
        foreach (var fate in BunnyFates)
            fate.Reset();

        foreach (var fate in OccultFates)
            fate.Reset();

        foreach (var fate in OccultCriticalEncounters)
            fate.Reset();
    }
}