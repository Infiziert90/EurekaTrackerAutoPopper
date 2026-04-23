using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace EurekaTrackerAutoPopper;

public class Fate
{
    public readonly uint FateId;
    public readonly Territory Territory;

    public readonly string Position;

    public bool Alive;
    public bool PlayedSound;
    public long SpawnTime;
    public long DeathTime;
    public long LastSeenAlive = -1;

    public long TimeLeft;
    public byte Progress;

    public readonly string Name;
    public readonly uint MapIcon;

    public long StateTimeLeft;
    public DynamicEventState State;

    public readonly Vector3 WorldPos;
    public readonly string MapDataLink;

    public readonly uint[] SpecialRewards = [];

    public readonly int WalkingDistance;
    public readonly OccultAetheryte Aetheryte = OccultAetheryte.None;

    // Only used for Forked Tower
    public int KilledFates;
    public int KilledCEs;
    public long InstanceJoinedTimer;

    // Eureka Bunny Fates
    public Fate(uint id, Territory territory, Vector3 worldPos, string position)
    {
        FateId = id;
        Territory = territory;

        WorldPos = worldPos;
        MapDataLink = Utils.CreateMapDataLink((uint)territory, territory.ToMap(), worldPos.X, worldPos.Z);

        Position = position;

        MapIcon = 60958;

        Name = GetName(id);
    }

    public Fate(uint id, uint mapIcon, Territory territory, Vector3 worldPos, uint[] rewards, OccultAetheryte aetheryte = OccultAetheryte.ExpeditionBaseCamp, int distance = 0, string position = "")
    {
        FateId = id;
        Territory = territory;

        MapIcon = mapIcon;

        WorldPos = worldPos;
        MapDataLink = Utils.CreateMapDataLink((uint)territory, territory.ToMap(), worldPos.X, worldPos.Z);

        Position = position;

        SpecialRewards = rewards;

        Aetheryte = aetheryte;
        WalkingDistance = distance;

        Name = GetName(id);
    }

    public void Update(IFate fate, long currentTime)
    {
        Alive = true;
        LastSeenAlive = currentTime;
        SpawnTime = fate.StartTimeEpoch;

        TimeLeft = fate.TimeRemaining;
        Progress = fate.Progress;
    }

    public void Update(ref DynamicEvent criticalEncounter, long currentTime)
    {
        if (!Alive)
            SpawnTime = currentTime;

        Alive = true;
        LastSeenAlive = currentTime;

        TimeLeft = criticalEncounter.SecondsLeft;
        Progress = criticalEncounter.Progress;

        State = criticalEncounter.State;
        StateTimeLeft = criticalEncounter.StartTimestamp - currentTime;
    }

    public void Reset()
    {
        Alive = false;
        SpawnTime = -1;
        LastSeenAlive = -1;

        KilledFates = 0;
        KilledCEs = 0;
    }

    // Above 1000 are Fates, below is most likely Critical Encounter
    private static string GetName(uint fateId)
        => (fateId > 1000 ? Sheets.FateSheet.GetRow(fateId).Name : Sheets.DynamicEventSheet.GetRow(fateId).Name).ExtractText();
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
        new(1367, Territory.Pagos, new Vector3(-168.20723f, -737.0106f, 304.78036f), " (South)"),
        new(1368, Territory.Pagos, new Vector3(-45.060673f, -542.2534f, -10.444342f), " (North)"),
        new(1407, Territory.Pyros, new Vector3(123.93088f, 706.2543f, 235.71927f), " (South)"),
        new(1408, Territory.Pyros, new Vector3(172.66713f, 679.68823f, -514.0787f), " (North)"),
        new(1425, Territory.Hydatos, new Vector3(-369.96432f, 499.13068f, -477.4539f), ""),

        // Occult
        new(1976, 60958, Territory.SouthHorn, new Vector3(204.66835f, 111.81729f, -204.96242f), [47749, 47738], OccultAetheryte.CrystallizedCaverns, 40, " (North)"),
        new(1977, 60958, Territory.SouthHorn, new Vector3(-479.8395f, 75f, 524.78894f), [47745, 47738], OccultAetheryte.Stonemarsh, 18, " (South)")
    ];

    public readonly List<Fate> OccultFates =
    [
        new(1962, 60502, Territory.SouthHorn, new Vector3(151.38765f, 56f, 670.072f), [47744], OccultAetheryte.Eldergrowth, 28), // Rough Waters
        new(1963, 60502, Territory.SouthHorn, new Vector3(364.61816f, 70f, 489.55896f), [47744], OccultAetheryte.Eldergrowth, 14), // The Golden Guardian
        new(1964, 60502, Territory.SouthHorn, new Vector3(-217.46391f, 116.70241f, 265.08792f), [47749], OccultAetheryte.Stonemarsh, 10), // King of the Crescent
        new(1965, 60502, Territory.SouthHorn, new Vector3(-221.13199f, 107f, 40.158627f), [47747], OccultAetheryte.TheWanderersHaven, 27), // The Winged Terror
        new(1966, 60502, Territory.SouthHorn, new Vector3(-221.1495f, 106.99999f, 40.21738f), [47746], OccultAetheryte.CrystallizedCaverns, 26), // An Unending Duty
        new(1967, 60502, Territory.SouthHorn, new Vector3(-40.98325f, 111.68926f, -316.82162f), [47747], OccultAetheryte.CrystallizedCaverns, 24), // Brain Drain
        new(1968, 60502, Territory.SouthHorn, new Vector3(-369.61337f, 75f, 649.92035f), [47745], OccultAetheryte.Stonemarsh, 25), // A Delicate Balance
        new(1969, 60502, Territory.SouthHorn, new Vector3(-589.41364f, 96.2f, 330.6984f), [47745], OccultAetheryte.Stonemarsh, 18), // Sworn to Soil
        new(1970, 60502, Territory.SouthHorn, new Vector3(-57.992046f, 69.50635f, 561.93933f), [47744], OccultAetheryte.Stonemarsh, 29), // A Prying Eye
        new(1971, 60502, Territory.SouthHorn, new Vector3(76.327644f, 96.94907f, 275.7444f), [47749], OccultAetheryte.Eldergrowth, 17), // Fatal Allure
        new(1972, 60502, Territory.SouthHorn, new Vector3(413.7364f, 95.999985f, -14.67076f), [47748], OccultAetheryte.Eldergrowth, 24) // Serving Darkness
    ];

    public readonly List<Fate> OccultCriticalEncounters =
    [
        new(33, 63909, Territory.SouthHorn, new Vector3(299.92032f, 70f, 729.9832f), [47744], OccultAetheryte.Eldergrowth, 30), // Scourge of the Mind
        new(34, 63911, Territory.SouthHorn, new Vector3(450.28986f, 65f, 356.46573f), [47749, 47752, 47732], OccultAetheryte.Eldergrowth, 10), // The Black Regiment
        new(35, 63909, Territory.SouthHorn, new Vector3(620.17365f, 79f, 800.0485f), [47744, 47751, 47730], OccultAetheryte.Eldergrowth, 48), // The Unbridled
        new(36, 63909, Territory.SouthHorn, new Vector3(680.90576f, 74f, 534.0728f), [47744], OccultAetheryte.Eldergrowth, 33), // Crawling Death
        new(37, 63909, Territory.SouthHorn, new Vector3(-340.11813f, 75f, 800.0618f), [47745, 47728, 48008], OccultAetheryte.Stonemarsh, 33), // Calamity Bound
        new(38, 63909, Territory.SouthHorn, new Vector3(-413.43665f, 92f, 74.68839f), [47746], OccultAetheryte.CrystallizedCaverns, 17), // Trial by Claw
        new(39, 63909, Territory.SouthHorn, new Vector3(-799.84845f, 43.99998f, 245.20094f), [47746, 47729], OccultAetheryte.Stonemarsh, 37), // From Times Bygone
        new(40, 63911, Territory.SouthHorn, new Vector3(676.5143f, 96.03f, -254.43198f), [47748], OccultAetheryte.ExpeditionBaseCamp, 36), // Company of Stone
        new(41, 63909, Territory.SouthHorn, new Vector3(-117.018456f, 1f, -850.34644f), [47747, 47731], OccultAetheryte.TheWanderersHaven, 17), // Shark Attack
        new(42, 63909, Territory.SouthHorn, new Vector3(629.3389f, 108f, -52.77268f), [47748, 47757], OccultAetheryte.Eldergrowth, 42), // On the Hunt
        new(43, 63909, Territory.SouthHorn, new Vector3(-353.2408f, 5f, -606.3008f), [47747], OccultAetheryte.TheWanderersHaven, 12), // With Extreme Prejudice
        new(44, 63909, Territory.SouthHorn, new Vector3(457.3497f, 97f, -357.9041f), [47749], OccultAetheryte.ExpeditionBaseCamp, 36), // Noise Complaint
        new(45, 63909, Territory.SouthHorn, new Vector3(72.06891f, 20f, -549.957f), [47747, 47733], OccultAetheryte.TheWanderersHaven, 17), // Cursed Concern
        new(46, 63909, Territory.SouthHorn, new Vector3(870.55774f, 122f, 180.04774f), [47748], OccultAetheryte.Eldergrowth, 57), // Eternal Watch
        new(47, 63909, Territory.SouthHorn, new Vector3(-569.202f, 97f, -158.79793f), [47746], OccultAetheryte.CrystallizedCaverns, 14), // Flame of Dusk

        new(48, 63978, Territory.SouthHorn, new Vector3(63.066174f, 126.499985f, 3.8296576f), [47868, 47734, 47735, 47736, 47737], OccultAetheryte.Eldergrowth, 25) // The Forked Tower: Blood
    ];

    private void ScanOccultFates(IFramework _)
    {
        var local = Plugin.ObjectTable.LocalPlayer;
        if (local == null)
            return;

        var towerEngagement = Plugin.Fates.OccultCriticalEncounters[^1];

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        foreach (var bnuuy in BunnyFates)
        {
            foreach (var fate in Plugin.FateTable)
            {
                if (fate.FateId != bnuuy.FateId)
                    continue;

                bnuuy.Update(fate, currentTime);
                if (bnuuy.PlayedSound || !Plugin.Configuration.PlayBunnyEffect)
                    continue;

                bnuuy.PlayedSound = true;
                UIGlobals.PlaySoundEffect((uint)Plugin.Configuration.BunnySoundEffect);
            }

            if (!bnuuy.Alive || bnuuy.LastSeenAlive == currentTime)
                continue;

            bnuuy.Alive = false;
            bnuuy.PlayedSound = false;
            bnuuy.DeathTime = bnuuy.LastSeenAlive;

            // Only increase if tower is not active
            if (!towerEngagement.Alive)
                towerEngagement.KilledFates += 1;
        }

        foreach (var occultFate in OccultFates)
        {
            foreach (var fate in Plugin.FateTable)
            {
                if (fate.StartTimeEpoch == 0)
                    continue;

                if (fate.FateId != occultFate.FateId)
                    continue;

                // Freshly spawned fate
                if (!occultFate.Alive)
                    Plugin.TrackerHandler.InstanceCheckAsync(fate, local);

                occultFate.Update(fate, currentTime);
                if (occultFate.PlayedSound || !Plugin.Configuration.PlayFateEffect)
                    continue;

                occultFate.PlayedSound = true;
                UIGlobals.PlaySoundEffect((uint)Plugin.Configuration.FateSoundEffect);
            }

            if (!occultFate.Alive || occultFate.LastSeenAlive == currentTime)
                continue;

            occultFate.Alive = false;
            occultFate.PlayedSound = false;
            occultFate.DeathTime = occultFate.LastSeenAlive;

            // Only increase if tower is not active
            if (!towerEngagement.Alive)
                towerEngagement.KilledFates += 1;

            // Fate has died, update our running tracker
            Plugin.TrackerHandler.UpdateRunningTracker();
        }
    }

    private unsafe void ScanOccultCEs(IFramework _)
    {
        var local = Plugin.ObjectTable.LocalPlayer;
        if (local == null)
            return;

        var towerEngagement = Plugin.Fates.OccultCriticalEncounters[^1];

        var publicContent = PublicContentOccultCrescent.GetInstance();
        if (publicContent == null)
        {
            // Reset all states while publicContent is not initialized yet
            // This can happen if people get timeout from an active Encounter
            foreach (var occultCE in OccultCriticalEncounters)
                occultCE.State = DynamicEventState.Inactive;

            return;
        }

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        foreach (var occultCE in OccultCriticalEncounters)
        {
            var isTowerCE = occultCE.FateId == 48;

            foreach (ref var criticalEncounter in publicContent->DynamicEventContainer.Events)
            {
                if (criticalEncounter.State == DynamicEventState.Inactive)
                    continue;

                if (criticalEncounter.DynamicEventId != occultCE.FateId)
                    continue;

                // Freshly spawned CE
                if (!occultCE.Alive)
                    Plugin.TrackerHandler.UpdateRunningTracker();

                occultCE.Update(ref criticalEncounter, currentTime);
                if (occultCE.PlayedSound)
                    continue;
                occultCE.PlayedSound = true;

                // Forked Tower
                if (isTowerCE)
                {
                    if (!Plugin.Configuration.PlayTowerEffect)
                        continue;

                    UIGlobals.PlaySoundEffect((uint)Plugin.Configuration.TowerSoundEffect);
                }
                else
                {
                    if (!Plugin.Configuration.PlayEncounterEffect)
                        continue;

                    UIGlobals.PlaySoundEffect((uint)Plugin.Configuration.EncounterSoundEffect);
                }
            }

            if (!occultCE.Alive || occultCE.LastSeenAlive == currentTime)
                continue;

            occultCE.Alive = false;
            occultCE.PlayedSound = false;
            occultCE.State = DynamicEventState.Inactive;
            occultCE.DeathTime = occultCE.LastSeenAlive;

            // Only increase if tower is not active
            if (!towerEngagement.Alive)
                towerEngagement.KilledCEs += 1;

            if (isTowerCE)
            {
                towerEngagement.KilledFates = 0;
                towerEngagement.KilledCEs = 0;
            }

            // CE has died, update our running tracker
            Plugin.TrackerHandler.UpdateRunningTracker();
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