using System;
using System.Numerics;
using System.Timers;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using EurekaTrackerAutoPopper.Resources;
using ImGuiNET;

using static ImGuiNET.ImGuiWindowFlags;
using static FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace EurekaTrackerAutoPopper.Windows.Overlay;

public class ShoutWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    public int PullTime = 27;
    public int EorzeaTime = 720;

    private const int CountdownForShout = 20 * 1000; // Seconds
    private readonly Timer ShoutTimer = new(CountdownForShout);

    public ShoutWindow(Plugin plugin) : base("Shout##EurekaLinker")
    {
        Flags = NoDecoration | AlwaysAutoResize;

        Plugin = plugin;
        ShoutTimer.AutoReset = false;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (Plugin.LastSeenFate == Library.EurekaFate.Empty)
        {
            IsOpen = false;
            return;
        }

        var fate = $"Fate: {Plugin.LastSeenFate.Name}";
        var size = ImGui.CalcTextSize(fate).X;
        var extraSize = 0.0f; // extra spacing for ET time slider

        ImGui.TextUnformatted(fate);

        if (Plugin.Configuration.ShowPullTimer)
        {
            if (!Plugin.Configuration.UseEorzeaTimer)
            {
                ImGui.SameLine(size + 35);
                ImGui.TextUnformatted("PT");

                ImGui.SameLine(size + 55);
                ImGui.SetNextItemWidth(80 * ImGuiHelpers.GlobalScale);
                if (ImGui.InputInt("##pulltimer_input", ref PullTime, 1))
                    PullTime = Math.Clamp(PullTime, 1, 30);
            }
            else
            {
                extraSize = 50;
                ImGui.SameLine(size + 35);
                ImGui.TextUnformatted("ET");

                ImGui.SameLine(size + 55);
                ImGui.SetNextItemWidth(80 * ImGuiHelpers.GlobalScale + extraSize);
                if (ImGui.SliderInt("##eorzeatime_input", ref EorzeaTime, 1, 1440, CurrentEorzeanPullTime()))
                    EorzeaTime = RoundOff(Math.Clamp(EorzeaTime, 1, 1440));
            }
        }

        ImGui.Spacing();
        ImGui.NextColumn();

        ImGui.Columns(1);
        ImGui.Separator();

        ImGui.NewLine();

        ImGui.SameLine(size + 30 + extraSize);
        if (!ShoutTimer.Enabled)
        {
            if (ImGui.Button(Language.ShoutButtonPost, new Vector2(50, 0)))
            {
                Plugin.PostChatMessage();
                IsOpen = false;
            }
        }
        else
        {
            ImGui.BeginDisabled();
            ImGui.Button(Language.ShoutButtonPost, new Vector2(50, 0));
            ImGui.EndDisabled();

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                ImGui.SetTooltip(Language.ShoutWindowLimit);
        }

        ImGui.SameLine(size + 85 + extraSize);
        if (ImGui.Button(Language.ShoutButtonClose, new Vector2(50, 0)))
            IsOpen = false;
    }

    public void StartShoutCountdown()
    {
        ShoutTimer.Stop();
        ShoutTimer.Interval = CountdownForShout;
        ShoutTimer.Start();
    }

    public string CurrentEorzeanPullTime()
    {
        var time = new DateTime().AddMinutes(EorzeaTime);

        return !Plugin.Configuration.UseTwelveHourFormat ? $"{time:HH:mm}" : $"{time:hh:mm tt}";
    }

    public unsafe void SetEorzeaTimeWithPullOffset()
    {
        var et = DateTimeOffset.FromUnixTimeSeconds(Instance()->ClientTime.EorzeaTime);
        EorzeaTime = et.Hour * 60 + et.Minute + 60; // 60 min ET = 3 min our time
        EorzeaTime = RoundOff(EorzeaTime); // Round it to X0

        if (EorzeaTime > 1440)
            EorzeaTime -= 1440;
    }

    private static int RoundOff (int i) => (int) Math.Round(i / 10.0) * 10;
}