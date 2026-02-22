using System;
using System.Linq;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;

namespace EurekaTrackerAutoPopper;

public class PotDtrBar : IDisposable
{
    private const int OccultRespawn = 1805;

    private readonly Plugin Plugin;
    private readonly IDtrBar DtrBar;
    private readonly IDtrBarEntry? DtrEntry;

    public PotDtrBar(Plugin plugin, IDtrBar dtrBar)
    {
        Plugin = plugin;
        DtrBar = dtrBar;

        DtrEntry = DtrBar.Get("Eureka Linker Pot Timer");
        if (DtrEntry != null)
        {
            DtrEntry.OnClick += OnClick;
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
        if (DtrEntry != null)
            DtrEntry.Shown = false;
    }

    public void Update()
    {
        if (DtrEntry == null)
            return;

        if (!Plugin.Configuration.ShowPotDtrBar || !Plugin.PlayerInOccultTerritory())
        {
            DtrEntry.Shown = false;
            return;
        }

        var potInfo = Plugin.BunnyWindow.GetOccultPotInfo();
        if (potInfo == null)
        {
            DtrEntry.Shown = false;
            return;
        }

        var (displayFate, lastFate) = potInfo.Value;

        if (displayFate.Alive)
        {
            var activeFateDirection = displayFate.FateId == 1976 ? "North" : "South";
            DtrEntry.Text = new SeString(new Dalamud.Game.Text.SeStringHandling.Payloads.TextPayload($"Pot: Active ({activeFateDirection})"));
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

        var direction = displayFate.FateId == 1976 ? "North" : "South";
        var timeString = Utils.TimeToClockFormat(respawnTime);

        DtrEntry.Text = new SeString(new Dalamud.Game.Text.SeStringHandling.Payloads.TextPayload($"Next pot: {timeString} ({direction})"));
        DtrEntry.Shown = true;
    }

    private void OnClick(DtrInteractionEvent e)
    {
        var potInfo = Plugin.BunnyWindow.GetOccultPotInfo();
        if (potInfo == null)
            return;

        var (displayFate, _) = potInfo.Value;
        Plugin.OpenMap(displayFate.MapLinkPayload);
    }
}
