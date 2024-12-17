using System.Collections.Generic;
using System.Numerics;
using CheapLoc;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace EurekaTrackerAutoPopper;

public static class QuestHelper
{
    public static void Quests(uint territoryId, uint quest)
    {
        if (ImGui.BeginChild("Content", new Vector2(0, -(Aetherytes[territoryId].Length == 3 ? 115 : 90)), false, 0))
        {
            Header(Headers[territoryId][quest]);

            if (ImGui.BeginTable("##Table", 2))
            {
                ImGui.TableSetupColumn("##Quest");
                ImGui.TableSetupColumn("##Flag", 0, 0.4f);

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
                ImGui.EndTable();
            }
        }
        ImGui.EndChild();

        ImGuiHelpers.ScaledDummy(5);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5);

        if (ImGui.BeginChild("BottomBar", new Vector2(0, 0), false, 0))
        {
            ImGui.TextColored(ImGuiColors.HealerGreen, Loc.Localize("QuestLog - Aetheryte Text", "Aetheryte Unlocks:"));
            foreach (var aetheryte in Aetherytes[territoryId])
                AttuneTip(aetheryte);
        }
        ImGui.EndChild();
    }

    #region anemos
    private static void Anemos(uint quest)
    {
        switch (quest)
        {
            case 1:
                WrappedPoint(Loc.Localize("QuestLog - Anemos 1 Step 1", "Talk to Krile and Gerolt"));
                WrappedTips(
                    Loc.Localize("QuestLog - Anemos 1 Tip 1", "You must obtain a Protean Crystal from a normal monster."),
                    Loc.Localize("QuestLog - Anemos 1 Tip 2", "You cannot exchange Anemos Crystals for Protean Crystals until you finish this quest."));
                break;

            case 3:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Anemos 3 Step 2", "Proceed inside the Early Natural History Society Observatory and interact with the Confluence"),
                    Anemos3);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));

                WrappedTips(Loc.Localize("QuestLog - Anemos 3 Tip 1", "Mobs will spawn around and inside the building during nighttime and as such it is recommended to do this during daytime (6am to 6pm ET)."));
                break;

            case 5:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Anemos 5 Step 2", "Proceed to the The Orchard below the cliff and interact with the Confluence"),
                    Anemos5);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
                break;

            case 13:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Anemos 13 Step 2", "Proceed to the The Val River Swale near the waterfall and interact with the Confluence"),
                    Anemos13);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
                break;

            case 17:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Anemos 17 Step 2", "Proceed to the Uncanny Valley and interact with the Confluence"),
                    Anemos17);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Anemos 17 Step 4", "Proceed to the Aethernet Control and interact with the Aethernet Relay"),
                    Anemos172);
                WrappedPoint(Loc.Localize("QuestLog - Anemos 17 Step 5", "Interact with the Aethernet Relay again and trade in 99 Anemos Crystals."));
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
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
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                break;

            case 21:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pagos 21 Step 2", "Locate the Depleted Aetherial Stream at The Cones (upper level on a raised grassy hill)"),
                    Pagos21);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
                break;

            case 23:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pagos 23 Step 2", "Locate the Depleted Aetherial Stream at The Val River Belly"),
                    Pagos23);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
                break;

            case 25:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pagos 25 Step 2", "Locate the magicite at The Eastern Edge"),
                    Pagos251);
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pagos 25 Step 3", "Interact with it again in almost the same location"),
                    Pagos252);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));

                WrappedTips(Loc.Localize("QuestLog - Pagos 25 Tip 1", "You can also speak with Gerolt any time after first speaking to Krile to unlock Eureka Pagos weapon enhancement."));
                break;

            case 29:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pagos 29 Step 2", "Drop down here"),
                    Pagos291);
                WrappedPoint(Loc.Localize("QuestLog - Pagos 29 Step 3", "Talk to Ejika Tsunjika"));
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
                break;

            case 35:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pagos 35 Step 2", "Locate the aether column at The Fumarole"),
                    Pagos35);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
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
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pyros 35 Step 2", "Proceed to The Living Foundry and talk to Krile"),
                    Pyros351);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToGerolt", "Return to Gerolt"));
                WrappedPoint(Loc.Localize("QuestLog - TalkToDrake", "Talk to Drake"));
                WrappedPoint(Loc.Localize("QuestLog - Pyros 35 Step 5", "Use the Logos Manipulator and select the Paralyze L logogram and extract the mneme"));
                WrappedPoint(Loc.Localize("QuestLog - TalkToDrake", "Talk to Drake"));
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pyros 35 Step 7", "Proceed to Northeastern Ice Needles and interact with the confluence"),
                    Pyros352
                );
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
                break;

            case 38:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                WrappedPoint(Loc.Localize("QuestLog - TalkToDrake", "Talk to Drake"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pyros 38 Step 2", "Proceed to The Surgate Town House and interact with the Promising Scrap"),
                    Pyros381);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToDrake", "Return to Drake"));
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pyros 38 Step 4", "Proceed to The Firing Chamber and interact with the confluence"),
                    Pyros382);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));

                WrappedTips(Loc.Localize("QuestLog - Pyros 38 Tip 1", "This quest unlocks additional capacity for the logos manipulator."));
                break;

            case 40:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pyros 40 Step 2", "Proceed to West Flamerock and interact with the confluence"),
                    Pyros40);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
                break;

            case 45:
                WrappedPoint(Loc.Localize("QuestLog - Pyros 45 Step 1", "Talk to Krile (a few cutscenes will play)"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pyros 45 Step 2", "Proceed to The Living Foundry and talk to Krile"),
                    Pyros351);
                break;

            case 50:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pyros 50 Step 2", "Proceed to East Flamerock and interact with the confluence"),
                    Pyros501);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Pyros 50 Step 4", "Proceed to The Cavern of the Second Cant and interact with the confluence"),
                    Pyros502);
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
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
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                WrappedPoint(Loc.Localize("QuestLog - TalkToGerolt", "Talk to Gerolt"));
                WrappedPoint(Loc.Localize("QuestLog - ReturnToKrile", "Return to Krile"));
                break;

            case 51:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 51 Step 2", "Proceed to The Central Columns and talk again to Krile"),
                    Hydatos511,
                    1);
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 51 Step 3", "Proceed south to The Val River Source and find the Pooled Aether"),
                    Hydatos512);
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 51 Step 4", "Return to the The Central Columns and talk to Krile"),
                    Hydatos511,
                    2);
                break;

            case 54:
                WrappedPoint(Loc.Localize("QuestLog - TalkToKrile", "Talk to Krile"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 54 Step 2", "Proceed to The Western Columns and talk again to Krile"),
                    Hydatos541,
                    1);
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 54 Step 3", "Proceed to The West Val River Bank"),
                    Hydatos542);
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 54 Step 4", "Walk up the large column to find the Pooled Aether"),
                    Hydatos543);
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 54 Step 5", "Return to The Western Columns and talk to Krile"),
                    Hydatos541,
                    2);
                break;

            case 57:
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 57 Step 1", "Talk to Krile at the The Western Columns"),
                    Hydatos541);
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 57 Step 2", "Proceed east to The Eastern Columns and talk to Krile again"),
                    Hydatos571,
                    1);
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 57 Step 3", "Head north to The East Val River Bank to find the Pooled Aether"),
                    Hydatos572);
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 57 Step 4", "Return to The Eastern Columns and talk to Krile"),
                    Hydatos571,
                    2);
                break;

            case 60:
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 60 Step 1", "Head to The Eastern Columns and talk to Krile"),
                    Hydatos571);
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 60 Step 2", "Head to The Aetherbridge Foundation and talk again to Krile"),
                    Hydatos60);
                WrappedPoint(Loc.Localize("QuestLog - Hydatos 60 Step 3", "Proceed into the Headquarters Entrance (Teleport next to you)"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 60 Step 4", "Talk again to Krile"),
                    Headquarters);
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 60 Step 5", "Go in the Board Room and talk again to Krile"),
                    BookRoom);
                WrappedPoint(Loc.Localize("QuestLog - Hydatos 60 Step 6", "Interact with the Scattered Tomes"));
                WrappedPoint(Loc.Localize("QuestLog - Hydatos 60 Step 7", "Talk again to Krile (a few cutscenes will play)"));
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 60 Step 8", "Choose between Krile's path (sealing) or that of Ejika Tsunjika (destruction)"),
                    CoreArea);
                TextWithSelectable(
                    Loc.Localize("QuestLog - Hydatos 60 Step 9", "Return to Central Point and talk to Ejika Tsunjika or Krile (opposite you picked)"),
                    HydatosHub);
                WrappedTips(
                    Loc.Localize("QuestLog - Hydatos 60 Tip 1", "This quest unlocks The Baldesion Arsenal."),
                    Loc.Localize("QuestLog - Hydatos 60 Tip 2", "Speak with the Expedition Scholar after finishing this quest."));
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
        ImGui.Indent(5f);
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
        // It is currently not possible in imgui to span multiple columns, so we
        // end the table early and start a new one afterwards
        ImGui.EndTable();

        ImGui.Unindent(5);
        ImGuiHelpers.ScaledDummy(15f);
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
        ImGui.TextUnformatted("Tip:");
        foreach (var tip in tips)
            ImGui.TextWrapped($"{tip}");
        ImGui.PopStyleColor();

        ImGui.BeginTable("##nothing", 2);
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
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetColumnWidth() - length.X - ImGui.GetScrollX()
                            - 2 * ImGui.GetStyle().ItemSpacing.X);
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
        public string Name
        {
            get
            {
                var key = Utils.GetSheet<EurekaAethernet>(Key)!;
                return Utils.FromSeString(Utils.GetSheet<PlaceName>(key.Location.RowId).Name);
            }
        }
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
                new Aetheryte(13, 51, CreateHydatosLink(14.0f, 12.3f)),
                new Aetheryte(14, 55, CreateHydatosLink(30.2f, 20.5f))
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
            732 => Loc.Localize("Area Name - Anemos", "Anemos"),
            763 => Loc.Localize("Area Name - Pagos", "Pagos"),
            795 => Loc.Localize("Area Name - Pyros", "Pyros"),
            827 => Loc.Localize("Area Name - Hydatos", "Hydatos"),
            _ => Loc.Localize("Area Name - Anemos", "Anemos")
        };
    }
}