using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Text;

namespace EurekaTrackerAutoPopper
{
    public static class Utils
    {
        public static T? GetSheet<T>(uint row) where T : ExcelRow
        {
            return Plugin.DataManager
                .GetExcelSheet<T>(Localization.LangCodeToClientLanguage(Plugin.DalamudPluginInterface.UiLanguage))!
                .GetRow(row);
        }

        public static string FromSeString(SeString text) => text.ToDalamudString().ToString();
    }
}