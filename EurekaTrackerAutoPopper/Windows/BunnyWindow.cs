using System;
using System.Linq;
using System.Numerics;
using CheapLoc;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using static ImGuiNET.ImGuiWindowFlags;

namespace EurekaTrackerAutoPopper.Windows;

public class BunnyWindow : Window, IDisposable
{
    private const int MinRespawn = 530;
    private const int MaxRespawn = 1000;
    private const int OccultRespawn = 1800;

    private readonly Plugin Plugin;

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

    public override void Draw()
    {
        var isEurekaBunny = Fates.EurekaBunnyTerritories.Contains(Plugin.ClientState.TerritoryType);
        var isOccult = Plugin.ClientState.TerritoryType == (uint)Territory.SouthHorn;
        if (!isEurekaBunny && !isOccult)
            return;

        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        var bunnies = Plugin.Fates.BunnyFates.Where(bnuuuy => (uint)bnuuuy.Territory == Plugin.ClientState.TerritoryType).ToArray();
        if (isEurekaBunny && Plugin.Configuration.OnlyEasyBunny)
            bunnies = bunnies[..1];

        // In Occult the bunny fates spawn after another in a cycle, so we have to sort them based on death time
        if (isOccult)
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
                nextSpawn.LastSeenAlive = lastAlive.LastSeenAlive;
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
                ImGui.TextColored(ImGuiColors.HealerGreen, Loc.Localize("Bunny Window - Status Alive", "Alive"));
            }
            else
            {
                ImGui.TextColored(ImGuiColors.ParsedGold, Loc.Localize("Bunny Window - Respawning", "Respawning in:"));
                ImGui.SameLine();
                if (bunny.LastSeenAlive != -1)
                {
                    var min = TimeSpan.FromSeconds(bunny.LastSeenAlive + (isEurekaBunny ? MinRespawn : OccultRespawn) - currentTime);
                    var max = TimeSpan.FromSeconds(bunny.LastSeenAlive + (isEurekaBunny ? MaxRespawn : OccultRespawn) - currentTime);

                    if (min.TotalSeconds > 0)
                    {
                        ImGui.TextUnformatted(Utils.TimeToFormatted(min));
                    }
                    else
                    {
                        ImGui.TextColored(ImGuiColors.ParsedGold,
                            Loc.Localize("Bunny Window - Respawning soon", " soon "));
                    }

                    ImGui.SameLine();
                    ImGui.TextUnformatted($"(max {Utils.TimeToFormatted(max)})");
                }
                else
                {
                    ImGui.TextColored(ImGuiColors.ParsedBlue, Loc.Localize("Bunny Window - Status Unknown", "Unknown"));
                }
            }

            ImGuiHelpers.ScaledDummy(5);
        }
    }
}