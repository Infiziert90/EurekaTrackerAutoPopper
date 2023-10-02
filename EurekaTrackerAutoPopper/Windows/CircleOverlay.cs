using System;
using System.Numerics;
using System.Timers;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using static ImGuiNET.ImGuiWindowFlags;

namespace EurekaTrackerAutoPopper.Windows;

public class CircleOverlay : Window, IDisposable
{
    private readonly Plugin Plugin;

    public bool NearToCoffer;
    public Vector3 CofferPos = Vector3.Zero;
    private readonly Timer PreviewTimer = new(5 * 1000);

    public CircleOverlay(Plugin plugin) : base("Circle###EurekaLinker")
    {
        Flags = NoBackground | NoMove | NoTitleBar | NoScrollbar | NoResize | NoInputs;
        Size = new Vector2(100, 50);
        ForceMainWindow = true;
        IsOpen = true;

        Plugin = plugin;
        PreviewTimer.AutoReset = false;
        PreviewTimer.Elapsed += (_, _) =>
        {
            NearToCoffer = false;
            CofferPos = Vector3.Zero;
        };
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        Plugin.GameGui.WorldToScreen(CofferPos, out var circlePos);
        var winPos = new Vector2(circlePos.X - 15, circlePos.Y - 15);

        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(winPos);
    }

    public override void Draw()
    {
        if (!Plugin.Configuration.BunnyCircleDraw)
            return;

        if (!NearToCoffer || CofferPos == Vector3.Zero)
            return;

        Plugin.GameGui.WorldToScreen(CofferPos, out var circlePos);
        ImGui.GetWindowDrawList().AddCircleFilled(circlePos, 8.0f, ImGui.GetColorU32(ImGui.ColorConvertFloat4ToU32(Plugin.Configuration.CircleColor)));
    }

    public void EnablePreview()
    {
        if (Plugin.ClientState.LocalPlayer == null)
            return;

        NearToCoffer = true;
        CofferPos = Plugin.ClientState.LocalPlayer.Position;

        PreviewTimer.Start();
    }
}