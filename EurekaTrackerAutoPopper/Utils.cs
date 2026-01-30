using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static System.Math;

namespace EurekaTrackerAutoPopper;

public static class Utils
{
    public static double GetDistance(Vector3 player, Vector3 target)
    {
        var difV = player - target;
        return Sqrt(Pow(difV.X, 2f) + Pow(difV.Y, 2f) + Pow(difV.Z, 2f));
    }

    public static string TimeToFormatted(TimeSpan span)
    {
        return span.ToString(span.TotalSeconds > 59 ? @"%m\ \m\i\n" : @"%s\ \s\e\c");
    }

    public static string TimeToClockFormat(TimeSpan span)
    {
        return span.Hours == 0 ? $"{span.Minutes:00}:{span.Seconds:00}" : $"{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}" ;
    }

    public static float Distance(Vector2 worldPos, Vector3 playerPos)
    {
        return Vector2.Distance(worldPos, new Vector2(playerPos.X, playerPos.Z));
    }

    public static SeString SuccessMessage(string success)
    {
        return new SeStringBuilder()
            .AddUiForeground("[Eureka Linker] ", 540)
            .AddUiForeground($"{success}", 43)
            .BuiltString;
    }

    public static unsafe IGameObject[] GetTowerCharacter(Fate towerEngagement)
    {
        return Plugin.ObjectTable
            .Where(o => o.ObjectKind == ObjectKind.Player)
            .Where(o => Distance(towerEngagement.WorldPos, o.Position) <= 20.0f)
            .Where(o => ((BattleChara*)o.Address)->GetForayInfo()->Level >= 20)
            .ToArray();
    }

    /// <summary>
    /// Return the ui path for usage with XIVAPI.
    /// </summary>
    /// <param name="iconId">The items icon id</param>
    /// <returns>Game path to the icon</returns>
    public static string GetIconPath(uint iconId){
        var iconGroup = iconId - (iconId % 1000);
        return $"{iconGroup:D6}/{iconId:D6}";
    }

    /// <summary>
    /// Converts <see cref="Image{Bgra32}"/> to raw pixel array in the same pixel format
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static byte[] ImageToRaw(this Image<Bgra32> image)
    {
        var data = new byte[4 * image.Width * image.Height];
        image.CopyPixelDataTo(data);
        return data;
    }
}