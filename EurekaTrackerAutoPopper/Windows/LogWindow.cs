using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using EurekaTrackerAutoPopper.Resources;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel;

namespace EurekaTrackerAutoPopper.Windows;

public class LogWindow : Window, IDisposable
{
    private readonly ExcelSheet<Lumina.Excel.Sheets.ContentsNote> ContentsSheet;

    private readonly Timer Cooldown = new(5 * 1000);
    private bool OnCooldown;

    public LogWindow() : base("Log##EurekaLinker")
    {
        Flags = ImGuiWindowFlags.NoResize;
        Size = new Vector2(370, 220);

        ContentsSheet = Plugin.Data.GetExcelSheet<Lumina.Excel.Sheets.ContentsNote>()!;

        Cooldown.AutoReset = false;
        Cooldown.Elapsed += (_, _) => OnCooldown = false;
    }

    public void Dispose() { }

    public override unsafe void Draw()
    {
        var local = Plugin.ClientState.LocalPlayer;
        if (local == null)
            return;

        ImGuiHelpers.ScaledDummy(5.0f);
        if (!IsCorrectTab())
        {
            ImGui.TextColored(ImGuiColors.ParsedOrange, Language.LogWrongTabNote);
            ImGui.TextColored(ImGuiColors.ParsedOrange, Language.LogWrongTab);
            return;
        }

        var log = GetContentProgress();
        if (!log.Any())
        {
            ImGui.TextColored(ImGuiColors.ParsedOrange, Language.LogError);
            return;
        }

        if (ImGui.BeginTable("##monsterTable", 3))
        {
            ImGui.TableSetupColumn(Language.TableLabelMonster);
            ImGui.TableSetupColumn(Language.TableLabelDone, ImGuiTableColumnFlags.None, 0.4f);
            ImGui.TableSetupColumn(Language.TableLabelEnemyLvl, ImGuiTableColumnFlags.None, 0.4f);

            ImGui.TableHeadersRow();
            for (var i = 0; i < log.Count; i += 2)
            {
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(IndexToName(i));

                ImGui.TableNextColumn();
                var progress1 = log[i];
                var progress2 = log[i + 1];

                var required1 = ContentsSheet.GetRow((uint) progress1.Id)!.RequiredAmount;
                var required2 = ContentsSheet.GetRow((uint) progress2.Id)!.RequiredAmount;

                if (progress1.Killed < required1)
                {
                    ImGui.TextUnformatted($"{progress1.Killed} / {required1}");
                }
                else if (progress2.Killed < required2)
                {
                    ImGui.TextUnformatted($"{progress2.Killed} / {required2}");
                }
                else
                {
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGui.TextUnformatted(FontAwesomeIcon.Check.ToIconString());
                    ImGui.PopFont();
                }

                ImGui.TableNextColumn();
                var elementalLevel = ((BattleChara*) local.Address)->GetForayInfo()->Level;
                ImGui.TextUnformatted($"{elementalLevel + IndexToRequirement(i)}");

                ImGui.TableNextRow();
            }
        }
        ImGui.EndTable();

        ImGuiHelpers.ScaledDummy(5.0f);

        var buttonText = "Refresh";
        var textSize = ImGui.CalcTextSize(buttonText);
        var buttonSize = new Vector2(textSize.X + 20.0f, textSize.Y + 7.0f);

        if (OnCooldown)
        {
            ImGui.BeginDisabled();
            ImGui.Button(buttonText, buttonSize);
            ImGui.EndDisabled();
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.ParsedBlue);
            if (ImGui.Button(buttonText, buttonSize))
            {
                // closing if it is already open
                var ptr = Plugin.GameGui.GetAddonByName("ContentsNote");
                if (ptr != nint.Zero)
                    ((AtkUnitBase*) ptr)->Close(true);

                var ui = (UIModule*) Plugin.GameGui.GetUIModule();
                if (ui != null && ui->IsMainCommandUnlocked(60))
                    ui->ExecuteMainCommand(60);

                ptr = Plugin.GameGui.GetAddonByName("ContentsNote");
                if (ptr != nint.Zero)
                    ((AtkUnitBase*) ptr)->Close(true);

                OnCooldown = true;
                Cooldown.Start();
            }
            ImGui.PopStyleColor();
        }
        ImGuiComponents.HelpMarker("Refreshing is necessary as the game has to request updated progress from the server.\nCooldown of 5s applies after each use.");
    }

    private static unsafe bool IsCorrectTab()
    {
        var instance = ContentsNote.Instance();
        if (instance == null)
            return false;

        return instance->SelectedTab == 13;
    }

    private static unsafe List<(int Id, int Killed)> GetContentProgress()
    {
        var list = new List<(int Id, int Killed)>();
        var note = ContentsNote.Instance();
        if (note == null)
            return list;

        try
        {
            for (var i = 0; i < ContentsNote.Instance()->DisplayCount; i++)
            {
                var (id, progress) = (note->DisplayIds[i], note->DisplayStatuses[i]);
                if (id is < 56 or > 65)
                    continue;

                list.Add((id, progress));
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "Unable to read ContentNote progress.");
        }

        return list;
    }

    private static string IndexToName(int index)
    {
        return index switch
        {
            0 => Language.LogCategoryIL,
            2 => Language.LogCategoryFE,
            4 => Language.LogCategoryWW,
            6 => Language.LogCategoryNight,
            8 => Language.LogCategorySprites,
            _ => "Unknown"
        };
    }

    private static int IndexToRequirement(int index)
    {
        return index switch
        {
            0 => -2,
            2 => -2,
            4 => -2,
            6 => 0,
            8 => 0,
            _ => 0
        };
    }
}