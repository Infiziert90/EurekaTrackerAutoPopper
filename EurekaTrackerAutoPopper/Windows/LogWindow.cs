using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using CheapLoc;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel;

namespace EurekaTrackerAutoPopper.Windows;

public class LogWindow : Window, IDisposable
{
    private Plugin Plugin;

    private ExcelSheet<Lumina.Excel.GeneratedSheets.ContentsNote> ContentsSheet = null!;

    private Timer Cooldown = null!;
    private bool OnCooldown = false;

    public LogWindow(Plugin plugin) : base("Log")
    {
        Flags = ImGuiWindowFlags.NoResize;
        Size = new Vector2(370, 220);

        Plugin = plugin;
        ContentsSheet = Plugin.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.ContentsNote>()!;

        Cooldown = new Timer();
        Cooldown.AutoReset = false;
        Cooldown.Interval = 5 * 1000;
        Cooldown.Elapsed += (_, __) => OnCooldown = false;
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
            ImGui.TextColored(ImGuiColors.ParsedOrange, Loc.Localize("Log - Wrong Tab","Please open your challenge log in the 'Other' tab for a second."));
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
                var ptr = Plugin.GameGui.GetAddonByName("ContentsNote", 1);
                if (ptr != nint.Zero)
                    ((AtkUnitBase*) ptr)->Close(true);

                Plugin.XivCommon.Functions.Chat.SendMessage("/challengelog");

                ptr = Plugin.GameGui.GetAddonByName("ContentsNote", 1);
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
        try
        {
            return ContentsNote.Instance()->SelectedTab == 11;
        }
        catch (Exception)
        {
            // ignored
        }

        return false;
    }

    private static unsafe List<(int Id, int Killed)> GetContentProgress()
    {
        try
        {
            var note = ContentsNote.Instance();
            var list = new List<(int Id, int Killed)>();
            for (var i = 0; i < ContentsNote.Instance()->DisplayCount; i++)
                list.Add((note->DisplayID[i], note->DisplayStatus[i]));

            return list;
        }
        catch (Exception)
        {
            // ignored
        }

        return new List<(int Id, int Killed)>();
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