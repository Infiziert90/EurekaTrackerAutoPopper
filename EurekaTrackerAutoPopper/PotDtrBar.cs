using System;
using System.Linq;
using Dalamud.Game.Gui.Dtr;

namespace EurekaTrackerAutoPopper;

public class PotDtrBar : IDisposable
{
    private const int OccultRespawn = 1800;

    private readonly Plugin Plugin;
    private readonly IDtrBarEntry? DtrEntry;

    public PotDtrBar(Plugin plugin)
    {
        Plugin = plugin;

        DtrEntry = Plugin.DtrBar.Get("Eureka Linker Pot Timer");
        if (DtrEntry != null)
        {
            DtrEntry.OnClick += OnClick;
            DtrEntry.Tooltip = "Eureka Linker\nClick to place flag on map\n\nDisable in: /el - Occult - Pot - Show Timer On Server Info Bar";
        }
    }

    public void Dispose()
    {
        if (DtrEntry != null)
        {
            DtrEntry.OnClick -= OnClick;
            DtrEntry.Remove();
        }
    }

    public void Hide()
    {
        DtrEntry?.Shown = false;
    }

    public void Update()
    {
        if (DtrEntry == null)
            return;

        if (!Plugin.Configuration.ShowPotDtrBar || !TerritoryHelper.PlayerInOccult())
        {
            DtrEntry.Shown = false;
            return;
        }

        var potInfo = GetOccultPotInfo();
        if (potInfo == null)
        {
            DtrEntry.Shown = false;
            return;
        }

        var (displayFate, lastFate) = potInfo.Value;
        if (displayFate.Alive)
        {
            DtrEntry.Text = $"Pot: Active{displayFate.Position}";
            DtrEntry.Shown = true;
            return;
        }

        if (displayFate.LastSeenAlive == -1)
        {
            DtrEntry.Shown = false;
            return;
        }

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var respawnTime = TimeSpan.FromSeconds(lastFate.SpawnTime + OccultRespawn - currentTime);

        if (respawnTime.TotalSeconds < 0)
            respawnTime = TimeSpan.Zero;

        var timeString = Utils.TimeToClockFormat(respawnTime);

        DtrEntry.Text = $"Next pot: {timeString}{displayFate.Position}";
        DtrEntry.Shown = true;
    }

    private void OnClick(DtrInteractionEvent e)
    {
        var potInfo = GetOccultPotInfo();
        if (potInfo == null)
            return;

        Plugin.OpenMap(potInfo.Value.DisplayFate.MapDataLink);
    }

    private (Fate DisplayFate, Fate LastFate)? GetOccultPotInfo()
    {
        var bunnies = Plugin.Fates.GetBunnyForTerritory().ToArray();
        if (bunnies.Length == 0)
            return null;

        var sortedFates = bunnies.OrderBy(bnuuuy => bnuuuy.LastSeenAlive).ToArray();
        var nextSpawn = sortedFates[0];
        var lastAlive = sortedFates[^1];

        // If it is -1 there hasn't been any pop yet
        if (nextSpawn.LastSeenAlive == -1 && lastAlive.LastSeenAlive == -1)
            return (nextSpawn, nextSpawn);

        // If our last alive is still active then show it
        if (lastAlive.Alive)
            return (lastAlive, lastAlive);

        // Apply the time of latest spawn to calculate next respawn
        // Set LastSeenAlive to 30min previously
        if (nextSpawn.LastSeenAlive == -1)
            nextSpawn.LastSeenAlive = lastAlive.SpawnTime - OccultRespawn;

        return (nextSpawn, lastAlive);
    }
}
