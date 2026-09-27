using System;
using System.Linq;
using Dalamud.Plugin.Ipc;

namespace EurekaTrackerAutoPopper;

/// <summary>
/// Read-only CallGate providers for Occult pot timers (e.g. BOCCHI).
/// </summary>
public sealed class PotIpc : IDisposable
{
    public const int ApiVersionValue = 1;

    private readonly Plugin plugin;
    private readonly ICallGateProvider<int> apiVersion;
    private readonly ICallGateProvider<(uint FateId, long SpawnUnix, long LastSeenUnix, bool Alive)[]> getTimers;
    private readonly ICallGateProvider<string> getInstanceKey;

    public PotIpc(Plugin plugin)
    {
        this.plugin = plugin;

        apiVersion = Plugin.PluginInterface.GetIpcProvider<int>("EurekaLinker.ApiVersion");
        getTimers = Plugin.PluginInterface.GetIpcProvider<(uint, long, long, bool)[]>("EurekaLinker.Pot.GetTimers");
        getInstanceKey = Plugin.PluginInterface.GetIpcProvider<string>("EurekaLinker.Pot.GetInstanceKey");

        apiVersion.RegisterFunc(() => ApiVersionValue);
        getTimers.RegisterFunc(ProvideTimers);
        getInstanceKey.RegisterFunc(ProvideInstanceKey);
    }

    public void Dispose()
    {
        apiVersion.UnregisterFunc();
        getTimers.UnregisterFunc();
        getInstanceKey.UnregisterFunc();
    }

    private (uint FateId, long SpawnUnix, long LastSeenUnix, bool Alive)[] ProvideTimers()
    {
        if (!TerritoryHelper.PlayerInOccult())
            return [];

        return plugin.Fates.GetBunnyForTerritory()
            .Select(f => (f.FateId, f.SpawnTime, f.LastSeenAlive, f.Alive))
            .ToArray();
    }

    private string ProvideInstanceKey()
    {
        if (plugin.TrackerHandler.CurrentTracker is { LastFateHash: { Length: > 0 } current })
            return current;

        if (plugin.TrackerHandler.UpcomingTracker is { LastFateHash: { Length: > 0 } upcoming })
            return upcoming;

        return string.Empty;
    }
}
