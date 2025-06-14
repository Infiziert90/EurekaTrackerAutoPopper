using System;
using System.Numerics;
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
}