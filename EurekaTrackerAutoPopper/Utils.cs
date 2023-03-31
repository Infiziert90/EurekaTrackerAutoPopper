using System.Numerics;
using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Text;

using static System.Math;

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

        public static double GetDistance(Vector3 player, Vector3 target)
        {
            var difV = player - target;
            return Sqrt(Pow(difV.X, 2f) + Pow(difV.Y, 2f) + Pow(difV.Z, 2f));
        }
    }
}