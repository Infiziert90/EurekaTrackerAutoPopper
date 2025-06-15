using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace EurekaTrackerAutoPopper;

public static class Sheets
{
    public static readonly ExcelSheet<Item> ItemSheet;
    public static readonly ExcelSheet<Treasure> TreasureSheet;
    public static readonly ExcelSheet<PlaceName> PlaceNameSheet;
    public static readonly ExcelSheet<ContentsNote> ContentsSheet;
    public static readonly ExcelSheet<DynamicEvent> DynamicEventSheet;
    public static readonly ExcelSheet<Lumina.Excel.Sheets.Fate> FateSheet;
    public static readonly ExcelSheet<EurekaAethernet> EurekaAethernetSheet;

    static Sheets()
    {
        ItemSheet = Plugin.Data.GetExcelSheet<Item>();
        TreasureSheet = Plugin.Data.GetExcelSheet<Treasure>();
        PlaceNameSheet = Plugin.Data.GetExcelSheet<PlaceName>();
        ContentsSheet = Plugin.Data.GetExcelSheet<ContentsNote>();
        DynamicEventSheet = Plugin.Data.GetExcelSheet<DynamicEvent>();
        FateSheet = Plugin.Data.GetExcelSheet<Lumina.Excel.Sheets.Fate>();
        EurekaAethernetSheet = Plugin.Data.GetExcelSheet<EurekaAethernet>();
    }

    public static Item GetItem(uint itemId) => ItemSheet.GetRow(ItemUtil.GetBaseId(itemId).ItemId);
}
