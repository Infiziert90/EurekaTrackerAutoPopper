using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using EurekaTrackerAutoPopper.Resources;
using Dalamud.Bindings.ImGui;

namespace EurekaTrackerAutoPopper;

public static class QuestHelper
{
    public static void Quests(uint territoryId, uint quest)
    {
        using (var contentChild = ImRaii.Child("Content", new Vector2(0, -(Aetherytes[territoryId].Length == 3 ? 115 : 90))))
        {
            if (contentChild.Success)
            {
                Header(Headers[territoryId][quest]);
                using var indent = ImRaii.PushIndent(5.0f);

                using var table = ImRaii.Table("QuestTable", 2);
                if (table.Success)
                {
                    ImGui.TableSetupColumn("##Quest");
                    ImGui.TableSetupColumn("##Flag", ImGuiTableColumnFlags.WidthStretch, 0.4f);

                    switch (territoryId)
                    {
                        case 732:
                            Anemos(quest);
                            break;
                        case 763:
                            Pagos(quest);
                            break;
                        case 795:
                            Pyros(quest);
                            break;
                        case 827:
                            Hydatos(quest);
                            break;
                    }
                }
            }
        }

        ImGuiHelpers.ScaledDummy(5);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5);

        using (var bottomChild = ImRaii.Child("BottomBar", Vector2.Zero))
        {
            if (bottomChild.Success)
            {
                ImGui.TextColored(ImGuiColors.HealerGreen, Language.QuestLogAetheryteText);
                foreach (var aetheryte in Aetherytes[territoryId])
                    AttuneTip(aetheryte);
            }
        }
    }

    #region anemos
    private static void Anemos(uint quest)
    {
        switch (quest)
        {
            case 1:
                WrappedPoint(Language.QuestLogAnemos1Step1);
                WrappedTips(Language.QuestLogAnemos1Tip1, Language.QuestLogAnemos1Tip2);
                break;

            case 3:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogAnemos3Step2, Anemos3);
                WrappedPoint(Language.QuestLogReturnToKrile);

                WrappedTips(Language.QuestLogAnemos3Tip1);
                break;

            case 5:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogAnemos5Step2, Anemos5);
                WrappedPoint(Language.QuestLogReturnToKrile);
                break;

            case 13:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogAnemos13Step2, Anemos13);
                WrappedPoint(Language.QuestLogReturnToKrile);
                break;

            case 17:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogAnemos17Step2, Anemos17);
                WrappedPoint(Language.QuestLogReturnToKrile);
                TextWithSelectable(Language.QuestLogAnemos17Step4, Anemos172);
                WrappedPoint(Language.QuestLogAnemos17Step5);
                WrappedPoint(Language.QuestLogReturnToKrile);
                break;
        }
    }
    #endregion

    #region pagos
    private static void Pagos(uint quest)
    {
        switch (quest)
        {
            case 17:
                WrappedPoint(Language.QuestLogTalkToKrile);
                break;

            case 21:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogPagos21Step2, Pagos21);
                WrappedPoint(Language.QuestLogReturnToKrile);
                break;

            case 23:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogPagos23Step2, Pagos23);
                WrappedPoint(Language.QuestLogReturnToKrile);
                break;

            case 25:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogPagos25Step2, Pagos251);
                TextWithSelectable(Language.QuestLogPagos25Step3, Pagos252);
                WrappedPoint(Language.QuestLogReturnToKrile);

                WrappedTips(Language.QuestLogPagos25Tip1);
                break;

            case 29:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogPagos29Step2, Pagos291);
                WrappedPoint(Language.QuestLogPagos29Step3);
                WrappedPoint(Language.QuestLogReturnToKrile);
                break;

            case 35:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogPagos35Step2, Pagos35);
                WrappedPoint(Language.QuestLogReturnToKrile);
                break;
        }
    }
    #endregion

    #region pyros
    private static void Pyros(uint quest)
    {
        switch (quest)
        {
            case 35:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogPyros35Step2, Pyros351);
                WrappedPoint(Language.QuestLogReturnToGerolt);
                WrappedPoint(Language.QuestLogTalkToDrake);
                WrappedPoint(Language.QuestLogPyros35Step5);
                WrappedPoint(Language.QuestLogTalkToDrake);
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogPyros35Step7, Pyros352);
                WrappedPoint(Language.QuestLogReturnToKrile);
                break;

            case 38:
                WrappedPoint(Language.QuestLogTalkToKrile);
                WrappedPoint(Language.QuestLogTalkToDrake);
                TextWithSelectable(Language.QuestLogPyros38Step2, Pyros381);
                WrappedPoint(Language.QuestLogReturnToDrake);
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogPyros38Step4,Pyros382);
                WrappedPoint(Language.QuestLogReturnToKrile);

                WrappedTips(Language.QuestLogPyros38Tip1);
                break;

            case 40:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogPyros40Step2, Pyros40);
                WrappedPoint(Language.QuestLogReturnToKrile);
                break;

            case 45:
                WrappedPoint(Language.QuestLogPyros45Step1);
                TextWithSelectable(Language.QuestLogPyros45Step2, Pyros351);
                break;

            case 50:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogPyros50Step2, Pyros501);
                WrappedPoint(Language.QuestLogReturnToKrile);
                TextWithSelectable(Language.QuestLogPyros50Step4, Pyros502);
                WrappedPoint(Language.QuestLogReturnToKrile);
                break;
        }
    }
    #endregion

    #region hydatos
    private static void Hydatos(uint quest)
    {
        switch (quest)
        {
            case 50:
                WrappedPoint(Language.QuestLogTalkToKrile);
                WrappedPoint(Language.QuestLogTalkToGerolt);
                WrappedPoint(Language.QuestLogReturnToKrile);
                break;

            case 51:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogHydatos51Step2, Hydatos511, 1);
                TextWithSelectable(Language.QuestLogHydatos51Step3, Hydatos512);
                TextWithSelectable(Language.QuestLogHydatos51Step4, Hydatos511, 2);
                break;

            case 54:
                WrappedPoint(Language.QuestLogTalkToKrile);
                TextWithSelectable(Language.QuestLogHydatos54Step2, Hydatos541, 1);
                TextWithSelectable(Language.QuestLogHydatos54Step3, Hydatos542);
                TextWithSelectable(Language.QuestLogHydatos54Step4, Hydatos543);
                TextWithSelectable(Language.QuestLogHydatos54Step5, Hydatos541, 2);
                break;

            case 57:
                TextWithSelectable(Language.QuestLogHydatos57Step1, Hydatos541);
                TextWithSelectable(Language.QuestLogHydatos57Step2, Hydatos571, 1);
                TextWithSelectable(Language.QuestLogHydatos57Step3, Hydatos572);
                TextWithSelectable(Language.QuestLogHydatos57Step4, Hydatos571, 2);
                break;

            case 60:
                TextWithSelectable(Language.QuestLogHydatos60Step1, Hydatos571);
                TextWithSelectable(Language.QuestLogHydatos60Step2, Hydatos60);
                WrappedPoint(Language.QuestLogHydatos60Step3);
                TextWithSelectable(Language.QuestLogHydatos60Step4, Headquarters);
                TextWithSelectable(Language.QuestLogHydatos60Step5,BookRoom);
                WrappedPoint(Language.QuestLogHydatos60Step6);
                WrappedPoint(Language.QuestLogHydatos60Step7);
                TextWithSelectable(Language.QuestLogHydatos60Step8, CoreArea);
                TextWithSelectable(Language.QuestLogHydatos60Step9, HydatosHub);

                WrappedTips(Language.QuestLogHydatos60Tip1, Language.QuestLogHydatos60Tip2);
                break;
        }
    }
    #endregion

    #region LayoutHelper
    private static void Header(string header)
    {
        ImGuiHelpers.ScaledDummy(5f);
        ImGui.TextColored(ImGuiColors.HealerGreen, header);
        ImGuiHelpers.ScaledDummy(5f);
    }

    private static void WrappedPoint(string content)
    {
        ImGui.TableNextColumn();

        ImGui.TextUnformatted("-");
        ImGui.SameLine();
        ImGui.TextWrapped($"{content}");

        ImGui.TableNextRow();
    }

    private static void WrappedTips(params string[] tips)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        // It is currently not possible in imgui to span multiple columns, so we
        // end the table early and start a new one afterwards
        ImGuiHelpers.ScaledDummy(15f);

        using var pushedColor = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
        ImGui.TextUnformatted("Tips:");
        foreach (var tip in tips)
            ImGui.TextWrapped(tip);
    }

    private static void TextWithSelectable(string content, MapLinkPayload map, int index = 0)
    {
        var text = $"({map.XCoord:0.0},  {map.YCoord:0.0})";
        var length = ImGui.CalcTextSize(text);

        ImGui.TableNextColumn();

        ImGui.TextUnformatted("-");
        ImGui.SameLine();
        ImGui.TextWrapped($"{content}");
        ImGui.TableNextColumn();
        // right align coords
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetColumnWidth() - length.X - ImGui.GetScrollX() - 2 * ImGui.GetStyle().ItemSpacing.X);
        if (ImGui.Selectable($"{text}##{index}", false, 0, length))
            Plugin.OpenMap(map);

        ImGui.TableNextRow();
    }

    private static void AttuneTip(Aetheryte aetheryte)
    {
        var text = $"({aetheryte.MapFlag.XCoord:0.0},  {aetheryte.MapFlag.YCoord:0.0})";
        var length = ImGui.CalcTextSize(text);

        ImGui.TextUnformatted($"Level {aetheryte.Unlock}:");
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.ParsedGold, aetheryte.Name);
        ImGui.SameLine();
        if (ImGui.Selectable(text, false, 0, length))
            Plugin.OpenMap(aetheryte.MapFlag);
    }
    #endregion

    private static MapLinkPayload CreateAnemosLink(float x, float y) => CreateMapLinkPayload(732, 414, x, y);
    private static MapLinkPayload CreatePagosLink(float x, float y) => CreateMapLinkPayload(763, 467, x, y);
    private static MapLinkPayload CreatePyrosLink(float x, float y) => CreateMapLinkPayload(795, 484, x, y);
    private static MapLinkPayload CreateHydatosLink(float x, float y) => CreateMapLinkPayload(827, 515, x, y);

    private static MapLinkPayload CreateMapLinkPayload(uint terri, uint mapId, float x, float y)
    {
        return new MapLinkPayload(terri, mapId, x, y);
    }

    private record Aetheryte(uint Key, uint Unlock, MapLinkPayload MapFlag)
    {
        public string Name => Sheets.EurekaAethernetSheet.GetRow(Key).Location.Value.Name.ExtractText();
    }

    #region QuestMapFlags
    private static readonly MapLinkPayload Anemos3 = CreateAnemosLink(13.5f, 20.2f);
    private static readonly MapLinkPayload Anemos5 = CreateAnemosLink(24.4f, 23.1f);
    private static readonly MapLinkPayload Anemos13 = CreateAnemosLink(20.9f, 13.0f);
    private static readonly MapLinkPayload Anemos17 = CreateAnemosLink(32.8f, 18.3f);
    private static readonly MapLinkPayload Anemos172 = CreateAnemosLink(25.5f,16.1f);

    private static readonly MapLinkPayload Pagos21 = CreatePagosLink(16.5f, 28.8f);
    private static readonly MapLinkPayload Pagos23 = CreatePagosLink(26.7f, 30.7f);
    private static readonly MapLinkPayload Pagos251 = CreatePagosLink(30.9f, 20.7f);
    private static readonly MapLinkPayload Pagos252 = CreatePagosLink(30.7f, 21.0f);
    private static readonly MapLinkPayload Pagos291 = CreatePagosLink(9.9f, 21.3f);
    private static readonly MapLinkPayload Pagos35 = CreatePagosLink(21.8f, 12.4f);

    private static readonly MapLinkPayload Pyros351 = CreatePyrosLink(13.7f, 21.0f);
    private static readonly MapLinkPayload Pyros352 = CreatePyrosLink(27.8f, 27.1f);
    private static readonly MapLinkPayload Pyros381 = CreatePyrosLink(18.6f, 30.6f);
    private static readonly MapLinkPayload Pyros382 = CreatePyrosLink(23.8f, 17.0f);
    private static readonly MapLinkPayload Pyros40 = CreatePyrosLink(17.2f, 11.6f);
    private static readonly MapLinkPayload Pyros501 = CreatePyrosLink(34.1f, 7.3f);
    private static readonly MapLinkPayload Pyros502 = CreatePyrosLink(14.5f, 37.6f);

    private static readonly MapLinkPayload Hydatos511 = CreateHydatosLink(20.3f, 24.8f);
    private static readonly MapLinkPayload Hydatos512 = CreateHydatosLink(25.7f, 30.7f);
    private static readonly MapLinkPayload Hydatos541 = CreateHydatosLink(10.6f, 29.6f);
    private static readonly MapLinkPayload Hydatos542 = CreateHydatosLink(7.1f, 17.9f);
    private static readonly MapLinkPayload Hydatos543 = CreateHydatosLink(6.5f, 15.7f);
    private static readonly MapLinkPayload Hydatos571 = CreateHydatosLink(31.3f, 27.2f);
    private static readonly MapLinkPayload Hydatos572 = CreateHydatosLink(30.8f, 14.4f);
    private static readonly MapLinkPayload Hydatos60 = CreateHydatosLink(24.0f, 30.6f);
    private static readonly MapLinkPayload HydatosHub = CreateHydatosLink(20.3f, 13.7f);

    private static readonly MapLinkPayload Headquarters = CreateMapLinkPayload(827, 520, 11.1f, 11.0f);
    private static readonly MapLinkPayload BookRoom = CreateMapLinkPayload(827, 520, 10.0f, 12.7f);
    private static readonly MapLinkPayload ScatteredBooks = CreateMapLinkPayload(827, 520,10.1f, 12.9f);
    private static readonly MapLinkPayload CoreArea = CreateMapLinkPayload(827, 520,11.1f, 11.9f);
    #endregion

    #region UnlockableAetherytes
    private static readonly Dictionary<uint, Aetheryte[]> Aetherytes = new() {
        {
            732,
            [
                new Aetheryte(2, 9, CreateAnemosLink(14.0f, 12.3f)),
                new Aetheryte(3, 9, CreateAnemosLink(30.2f, 20.5f))
            ]
        },
        {
            763,
            [
                new Aetheryte(5, 21, CreatePagosLink(23.3f, 27.5f)),
                new Aetheryte(6, 23, CreatePagosLink(7.4f, 15.0f)),
                new Aetheryte(7, 25, CreatePagosLink(28.4f, 15.6f))
            ]
        },
        {
            795,
            [
                new Aetheryte(11, 37, CreatePyrosLink(24.0f, 37.4f)),
                new Aetheryte(9, 39, CreatePyrosLink(24.0f, 17.5f)),
                new Aetheryte(10, 41, CreatePyrosLink(12.6f, 9.0f))
            ]
        },
        {
            827,
            [
                new Aetheryte(13, 51, CreateHydatosLink(9.7f, 28.0f)),
                new Aetheryte(14, 55, CreateHydatosLink(37.1f, 22.6f))
            ]
        },
    };
    #endregion

    #region Chapters
    private static readonly Dictionary<uint, Dictionary<uint, string>> Headers = new()
    {
        {
            732, new Dictionary<uint, string>
            {
                { 1, "- Welcome to Anemos -" },
                { 3, "- Anemos Chapter II -" },
                { 5, "- Anemos Chapter III -" },
                { 13, "- Anemos Chapter IV -" },
                { 17, "- Anemos Chapter V -" },
            }
        },
        {
            763, new Dictionary<uint, string>
            {
                {17, "- Welcome to Pagos -"},
                {21, "- Pagos Chapter II -"},
                {23, "- Pagos Chapter III -"},
                {25, "- Pagos Chapter IV -"},
                {29, "- Pagos Chapter V -"},
                {35, "- Pagos Chapter VI -"},
            }
        },
        {
            795, new Dictionary<uint, string>
            {
                {35, "- Welcome to Pyros -"},
                {38, "- Pyros Chapter II -"},
                {40, "- Pyros Chapter III -"},
                {45, "- Pyros Chapter IV -"},
                {50, "- Pyros Chapter V -"},
            }
        },
        {
            827, new Dictionary<uint, string>
            {
                {50, "- Welcome to Hydatos -"},
                {51, "- Hydatos Chapter II -"},
                {54, "- Hydatos Chapter III -"},
                {57, "- Hydatos Chapter IV -"},
                {60, "- Eureka Final Chapter -"},
            }
        },
    };
    #endregion

    private static readonly List<uint> AnemosQuests = [1, 3, 5, 13, 17];
    private static readonly List<uint> PagosQuests = [17, 21, 23, 25, 29, 35];
    private static readonly List<uint> PyrosQuests = [35, 38, 40, 45, 50];
    private static readonly List<uint> HydatosQuests = [50, 51, 54, 57, 60];

    public static List<uint> TerritoryToQuests(uint territoryId)
    {
        return territoryId switch
        {
            732 => AnemosQuests,
            763 => PagosQuests,
            795 => PyrosQuests,
            827 => HydatosQuests,
            _ => AnemosQuests,
        };
    }

    public static string TerritoryToPlaceName(uint territoryId)
    {
        return territoryId switch
        {
            732 => Language.AreaNameAnemos,
            763 => Language.AreaNamePagos,
            795 => Language.AreaNamePyros,
            827 => Language.AreaNameHydatos,
            _ => Language.AreaNameAnemos
        };
    }

    public static uint TerritoryTypeToSafeId(uint territoryId)
    {
        return territoryId switch
        {
            732 => 732,
            763 => 763,
            795 => 795,
            827 => 827,
            _ => 732
        };
    }
}