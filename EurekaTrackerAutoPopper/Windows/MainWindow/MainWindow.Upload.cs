using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace EurekaTrackerAutoPopper.Windows.MainWindow;

public partial class MainWindow
{
    private void UploadCategory()
    {
        using var tabItem = ImRaii.TabItem("Upload");
        if (!tabItem.Success)
            return;

        var changed = false;
        ImGuiHelpers.ScaledDummy(5.0f);

        Helper.WrappedTextWithColor(ImGuiColors.DalamudViolet, "Anonymously provide data for instance history.\nThis data can't be tied to you in any way and everyone benefits!");

        ImGui.TextColored(ImGuiColors.DalamudViolet, "What data?");
        using (ImRaii.PushIndent(10.0f))
        {
            ImGui.TextColored(ImGuiColors.DalamudViolet, "- Critical Encounter Spawns");
            ImGui.TextColored(ImGuiColors.DalamudViolet, "- Pot Fate Spawns");
            ImGui.TextColored(ImGuiColors.DalamudViolet, "- Forked Tower Timers");
        }

        ImGuiHelpers.ScaledDummy(5.0f);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        changed |= ImGui.Checkbox("Upload Permission", ref Plugin.Configuration.UploadPermission);

        if (changed)
            Plugin.Configuration.Save();
    }
}