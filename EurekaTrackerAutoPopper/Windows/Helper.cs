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
    public static bool AddSoundOption(ref bool playSound, ref int soundEffect)
    {
        var changed = false;

        changed |= ImGui.Checkbox(Language.ConfigOptionSpawnNotification, ref playSound);
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
}