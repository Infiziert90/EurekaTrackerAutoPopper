using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace EurekaTrackerAutoPopper;

public static class Sheets
{
    public static readonly ExcelSheet<Item> ItemSheet;
    public static readonly ExcelSheet<Treasure> TreasureSheet;

    static Sheets()
    {
        ItemSheet = Plugin.Data.GetExcelSheet<Item>();
        TreasureSheet = Plugin.Data.GetExcelSheet<Treasure>();
    }

    public static Item GetItem(uint itemId) => ItemSheet.GetRow(ItemUtil.GetBaseId(itemId).ItemId);
}
