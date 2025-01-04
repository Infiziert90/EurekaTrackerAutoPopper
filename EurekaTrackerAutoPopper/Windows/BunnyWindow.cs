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

    private readonly Plugin Plugin;

    public BunnyWindow(Plugin plugin) : base("Bunny##EurekaLinker")
    {
        Flags = AlwaysAutoResize;
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(135, 70),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (!Library.BunnyTerritories.Contains(Plugin.ClientState.TerritoryType))
            return;

        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        var bunnies = Plugin.Library.Bunnies.Where(bnuuuy => bnuuuy.TerritoryId == Plugin.ClientState.TerritoryType).ToArray();
        if (Plugin.Configuration.OnlyEasyBunny)
            bunnies = bunnies[..1];

        foreach (var (bunny, idx) in bunnies.Select((val, idx) => (val, idx)))
        {
            if (idx > 0)
            {
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(5);
            }

            ImGui.TextUnformatted($"Fate: {bunny.Name}");
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
                    var min = TimeSpan.FromSeconds(bunny.LastSeenAlive + MinRespawn - currentTime);
                    var max = TimeSpan.FromSeconds(bunny.LastSeenAlive + MaxRespawn - currentTime);

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