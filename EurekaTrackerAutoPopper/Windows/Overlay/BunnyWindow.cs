using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using EurekaTrackerAutoPopper.Resources;
using ImGuiNET;

using static ImGuiNET.ImGuiWindowFlags;

namespace EurekaTrackerAutoPopper.Windows.Overlay;

public class BunnyWindow : Window, IDisposable
{
    private const int MinRespawn = 530;
    private const int MaxRespawn = 1000;
    private const int OccultRespawn = 1800;

    private readonly Plugin Plugin;

    private bool InEureka;
    private bool InOccult;

    public BunnyWindow(Plugin plugin) : base("Bunny##EurekaLinker")
    {
        Flags = AlwaysAutoResize;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(135, 70),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    public override bool DrawConditions()
    {
        InEureka = Fates.EurekaBunnyTerritories.Contains(Plugin.ClientState.TerritoryType);
        InOccult = Plugin.PlayerInOccultTerritory();

        return InEureka || InOccult;
    }

    public override void Draw()
    {
        var bunnies = Plugin.Fates.BunnyFates.Where(bnuuuy => (uint)bnuuuy.Territory == Plugin.ClientState.TerritoryType).ToArray();
        if (InEureka && Plugin.Configuration.OnlyEasyBunny)
            bunnies = bunnies[..1];

        // In Occult the bunny fates spawn after another in a cycle, so we have to sort them based on death time
        if (InOccult)
        {
            var sortedFates = bunnies.OrderBy(bnuuuy => bnuuuy.LastSeenAlive).ToArray();

            var nextSpawn = sortedFates[0];
            var lastAlive = sortedFates[^1];

            // If it is -1 there hasn't been any pop yet
            if (nextSpawn.LastSeenAlive == -1 && lastAlive.LastSeenAlive == -1)
            {
                bunnies = [nextSpawn];
            }
            // If our last alive is still active then show it
            else if (lastAlive.Alive)
            {
                bunnies = [lastAlive];
            }
            // Apply the time of latest kill to calculate next respawn
            else
            {
                nextSpawn.SpawnTime = lastAlive.SpawnTime;
                bunnies = [nextSpawn];
            }
        }

        foreach (var (bunny, idx) in bunnies.Select((val, idx) => (val, idx)))
        {
            if (idx > 0)
            {
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(5);
            }

            ImGui.TextUnformatted($"Fate: {bunny.Name}{bunny.Position}");
            if (bunny.Alive)
            {
                ImGui.TextColored(ImGuiColors.HealerGreen, Language.BunnyWindowStatusAlive);
            }
            else
            {
                ImGui.TextColored(ImGuiColors.ParsedGold, Language.BunnyWindowRespawning);
                ImGui.SameLine();
                if (bunny.LastSeenAlive != -1)
                {
                    var (min, max) = InEureka
                        ? CalculateEurekaRespawn(bunny)
                        : CalculateOccultRespawn(bunny);

                    if (min.TotalSeconds > 0)
                        ImGui.TextUnformatted(Utils.TimeToFormatted(min));
                    else
                        ImGui.TextColored(ImGuiColors.ParsedGold, Language.BunnyWindowRespawningsoon);

                    ImGui.SameLine();
                    ImGui.TextUnformatted($"(max {Utils.TimeToFormatted(max)})");
                }
                else
                {
                    ImGui.TextColored(ImGuiColors.ParsedBlue, Language.BunnyWindowStatusUnknown);
                }
            }

            ImGuiHelpers.ScaledDummy(5);
        }
    }

    /// <summary>
    /// Calculates the respawn time of bunny fates in eureka, which depends on if people keep their bunnies out long.
    /// </summary>
    /// <param name="bunny">fate</param>
    /// <returns>min and max respawn times</returns>
    private (TimeSpan Min, TimeSpan Max) CalculateEurekaRespawn(Fate bunny)
    {
        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();

        var min = TimeSpan.FromSeconds(bunny.LastSeenAlive + MinRespawn - currentTime);
        var max = TimeSpan.FromSeconds(bunny.LastSeenAlive + MaxRespawn - currentTime);

        return (min, max);
    }

    /// <summary>
    /// Calculates the respawn time of pot fates in occult crescent, which is exactly every 30 minutes after the last one spawned.
    /// </summary>
    /// <param name="pot"></param>
    /// <returns>fixed respawn time</returns>
    private (TimeSpan Min, TimeSpan Max) CalculateOccultRespawn(Fate pot)
    {
        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();

        var min = TimeSpan.FromSeconds(pot.SpawnTime + OccultRespawn - currentTime);
        return (min, min);
    }
}