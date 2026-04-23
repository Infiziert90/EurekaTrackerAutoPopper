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
    private static readonly Random Rand = new();
    private const float MaxValue = 50.0f;
    private const float MinValue = -50.0f;

    public static double GetDistance(Vector3 player, Vector3 target)
    {
        var difV = player - target;
        return Sqrt(Pow(difV.X, 2f) + Pow(difV.Y, 2f) + Pow(difV.Z, 2f));
    }

    public static string TimeToClockFormat(TimeSpan span)
    {
        return span.Hours == 0 ? $"{span.Minutes:00}:{span.Seconds:00}" : $"{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}" ;
    }

    public static float Distance(Vector3 worldPos, Vector3 playerPos)
    {
        return Vector3.Distance(worldPos, playerPos);
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
    /// Creates a Map data link for the Atk function to open the map on this point.
    /// </summary>
    /// <param name="territoryId">Territory to use</param>
    /// <param name="mapId">Map to use</param>
    /// <param name="xRaw">X world position</param>
    /// <param name="zRaw">Z world position, here used as Y</param>
    /// <returns></returns>
    public static string CreateMapDataLink(uint territoryId, uint mapId, double xRaw, double zRaw)
        => $"m:{territoryId},{mapId},{(int)xRaw * 1000},{(int)zRaw * 1000}";

    /// <summary>
    /// Creates a SeString map payload for a given world position.
    /// </summary>
    /// <param name="territoryId">Territory to use</param>
    /// <param name="mapId">Map to use</param>
    /// <param name="xRaw">X world position</param>
    /// <param name="zRaw">Z world position, here used as Y</param>
    /// <returns></returns>
    public static SeString CreateMapLink(uint territoryId, uint mapId, double xRaw, double zRaw)
        => SeString.CreateMapLink(territoryId, mapId, (int)xRaw * 1000, (int)zRaw * 1000);

    /// <summary>
    /// Return the ui path for usage with XIVAPI.
    /// </summary>
    /// <param name="iconId">The items icon id</param>
    /// <returns>Game path to the icon</returns>
    public static string GetIconPath(uint iconId){
        var iconGroup = iconId - (iconId % 1000);
        return $"{iconGroup:D6}/{iconId:D6}";
    }

    /// <inheritdoc cref="GetIconPath(uint)"/>
    public static string GetIconPath(Icons icon)
        => GetIconPath((uint)icon);

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

    /// Randomize a coordinate in a range of +0.5 and -0.5
    /// <param name="coord">The coordinate to randomize</param>
    /// <returns>The randomized coordinate</returns>
    public static float Randomize(float coord) =>
        coord + (Rand.NextSingle() * (MaxValue - MinValue) + MinValue);
}