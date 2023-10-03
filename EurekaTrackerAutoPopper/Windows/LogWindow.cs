using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using CheapLoc;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel;

namespace EurekaTrackerAutoPopper.Windows;

public class LogWindow : Window, IDisposable
{
    private readonly ExcelSheet<Lumina.Excel.GeneratedSheets.ContentsNote> ContentsSheet;

    private readonly Timer Cooldown = new(5 * 1000);
    private bool OnCooldown;

    public LogWindow() : base("Log##EurekaLinker")
    {
        Flags = ImGuiWindowFlags.NoResize;
        Size = new Vector2(370, 220);

        ContentsSheet = Plugin.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.ContentsNote>()!;

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
            ImGui.TextColored(ImGuiColors.ParsedOrange, Loc.Localize("Log - Wrong Tab Note","Unable to detect eureka progress."));
            ImGui.TextColored(ImGuiColors.ParsedOrange, Loc.Localize("Log - Wrong Tab","Please open your challenge log in the 'Other' category."));
            return;
        }

        var log = GetContentProgress();
        if (!log.Any())
        {
            ImGui.TextColored(ImGuiColors.ParsedOrange, Loc.Localize("Log - Error","Unable to read log."));
            return;
        }

        if (ImGui.BeginTable("##monsterTable", 3))
        {
            ImGui.TableSetupColumn(Loc.Localize("Table Label: Monster", "Monster"));
            ImGui.TableSetupColumn(Loc.Localize("Table Label: Done", "Done"), ImGuiTableColumnFlags.None, 0.4f);
            ImGui.TableSetupColumn(Loc.Localize("Table Label: Enemy Lvl", "Enemy Level"), ImGuiTableColumnFlags.None, 0.4f);

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
                var elementalLevel = ((BattleChara*) local.Address)->GetForayInfo->ElementalLevel;
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
        ImGuiComponents.HelpMarker("Refreshing is necessary as the game has to request updated progress from the server.\nCooldown of 5s is applied after each usage.");
    }

    private static unsafe bool IsCorrectTab()
    {
        var instance = ContentsNote.Instance();
        if (instance == null)
            return false;
        return instance->SelectedTab == 11;
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
                var (id, progress) = (note->DisplayID[i], note->DisplayStatus[i]);
                if (id is < 56 or > 65)
                    continue;

                list.Add((id, progress));
            }
        }
        catch (Exception)
        {
            // ignored
        }

        return list;
    }

    private static string IndexToName(int index)
    {
        return index switch
        {
            0 => Loc.Localize("Log - Category IL","Ice and Lighting"),
            2 => Loc.Localize("Log - Category FE","Fire and Earth"),
            4 => Loc.Localize("Log - Category WW","Water and Wind"),
            6 => Loc.Localize("Log - Category Night","Night"),
            8 => Loc.Localize("Log - Category Sprites","Sprites"),
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