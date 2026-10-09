using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace EurekaTrackerAutoPopper.Data;

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

    public Weakness Weakness = Weakness.None;

    // Critical Engagement
    public uint TriggeredBy;
    public string TriggerName = string.Empty;
    public int TriggerKills;

    // Only used for Forked Tower
    public int KilledFates;
    public int KilledCEs;
    public long InstanceJoinedTimer;
    public bool SpecialEngagement;

    // Eureka Bunny Fates
    public Fate(uint id, Territory territory, Vector3 worldPos, string position)
    {
        FateId = id;
        Territory = territory;

        WorldPos = worldPos;
        MapDataLink = Utils.CreateMapDataLink((uint)territory, (uint)territory.ToMap(), worldPos.X, worldPos.Z);

        Position = position;

        Name = GetName(id);
        MapIcon = GetIcon(id);
    }

    public Fate(uint id, Territory territory, Vector3 worldPos, uint[] rewards, OccultAetheryte aetheryte = OccultAetheryte.ExpeditionBaseCamp, int distance = 0, string position = "", uint trigger = 0, Weakness weakness = Weakness.None, bool special = false)
    {
        FateId = id;
        Territory = territory;

        WorldPos = worldPos;
        MapDataLink = Utils.CreateMapDataLink((uint)territory, (uint)territory.ToMap(), worldPos.X, worldPos.Z);

        Position = position;

        SpecialRewards = rewards;

        Aetheryte = aetheryte;
        WalkingDistance = distance;

        Weakness = weakness;

        Name = GetName(id);
        MapIcon = GetIcon(id);

        if (trigger != 0)
        {
            TriggeredBy = trigger;
            TriggerName = GetMonsterName(trigger);
        }

        SpecialEngagement = special;
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
        PlayedSound = false;
        SpawnTime = 0;
        DeathTime = 0;
        LastSeenAlive = -1;

        TimeLeft = 0;
        Progress = 0;

        StateTimeLeft = 0;
        State = DynamicEventState.Inactive;

        KilledFates = 0;
        KilledCEs = 0;

        TriggerKills = 0;

        InstanceJoinedTimer = 0;
    }

    // Above 1000 are Fates, below is most likely Critical Encounter
    private static uint GetIcon(uint fateId)
    {
        if (fateId > 1000)
            return Sheets.FateSheet.GetRow(fateId).Icon;

        var ce = Sheets.DynamicEventSheet.GetRow(fateId);
        return ce.IconObjective0 != 0 ? ce.IconObjective0 : ce.EventType.Value.IconObjective0;
    }

    private static string GetName(uint fateId)
        => (fateId > 1000 ? Sheets.FateSheet.GetRow(fateId).Name : Sheets.DynamicEventSheet.GetRow(fateId).Name).ToString();

    private static string GetMonsterName(uint nameId)
        => Plugin.Evaluator.EvaluateObjStr(ObjectKind.BattleNpc, nameId);
}