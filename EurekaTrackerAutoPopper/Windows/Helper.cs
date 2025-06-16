using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using EurekaTrackerAutoPopper.Resources;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;

namespace EurekaTrackerAutoPopper.Windows;

public static class Helper
{
    public const float SeparatorPadding = 1.0f;
    public static float GetSeparatorPaddingHeight => SeparatorPadding * ImGuiHelpers.GlobalScale;

    public static float CalculateChildHeight()
    {
        return ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().WindowPadding.Y + GetSeparatorPaddingHeight;
    }

    private static readonly int[] SoundEffects = [36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52];
    public static bool AddSoundOption(int id, string text, ref bool playSound, ref int soundEffect)
    {
        var changed = false;

        using var pushedId = ImRaii.PushId(id);
        changed |= ImGui.Checkbox(text, ref playSound);
        if (!playSound)
            return changed;

        ImGui.SameLine();

        ImGui.SetNextItemWidth(50.0f * ImGuiHelpers.GlobalScale);
        using (var combo = ImRaii.Combo("##SoundEffectCombo", soundEffect.ToString()))
        {
            if (combo.Success)
            {
                foreach (var value in SoundEffects)
                {
                    if (!ImGui.Selectable(value.ToString()))
                        continue;

                    changed = true;
                    soundEffect = value;
                }
            }
        }

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Play))
            UIGlobals.PlaySoundEffect((uint) soundEffect);

        return changed;
    }

    /// <summary>
    /// An unformatted version for ImGui.TextColored
    /// </summary>
    /// <param name="color">color to be used</param>
    /// <param name="text">text to display</param>
    public static void TextColored(Vector4 color, string text)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(text);
    }

    /// <summary>
    /// An unformatted version for ImGui.Tooltip
    /// </summary>
    /// <param name="tooltip">tooltip to display</param>
    public static void Tooltip(string tooltip)
    {
        using (ImRaii.Tooltip())
        using (ImRaii.TextWrapPos(ImGui.GetFontSize() * 35.0f))
            ImGui.TextUnformatted(tooltip);
    }

    /// <summary>
    /// An unformatted version for ImGui.TextWrapped
    /// </summary>
    /// <param name="text">text to display</param>
    public static void TextWrapped(string text)
    {
        using (ImRaii.TextWrapPos(0.0f))
            ImGui.TextUnformatted(text);
    }

    /// <summary>
    /// An unformatted version for ImGui.TextWrapped with color
    /// </summary>
    /// <param name="color">color to be used</param>
    /// <param name="text">text to display</param>
    public static void WrappedTextWithColor(Vector4 color, string text)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            TextWrapped(text);
    }

    /// <summary>
    /// Bullet with clickable link
    /// </summary>
    /// <param name="text">text to display</param>
    /// <param name="url">url to open</param>
    public static void BulletLink(string text, string url)
    {
        ImGui.Bullet();
        ImGui.SameLine();
        if (ImGui.Selectable(text))
            Dalamud.Utility.Util.OpenLink(url);
    }
}