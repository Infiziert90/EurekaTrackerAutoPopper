using System;
using System.Collections.Generic;
using System.Numerics;

namespace EurekaTrackerAutoPopper
{
    public static class BunnyChests
    {
        private const float InRange = 80.0f;

        private static readonly Dictionary<uint, List<Vector3>> Positions = new()
        {
            { 763, new List<Vector3> // Pagos
                {
                    new(-606.53420f, -700.6216f, 143.02383f),
                    new(-386.33823f, -677.493f, 468.21628f),
                    new(-259.90164f, -602.8868f, -139.11661f),
                    new(-147.99794f, -563.5579f, -282.92133f),
                    new(543.909000f, -730.7676f, 355.7726f),
                }
            },
            { 795, new List<Vector3> // Pyros
                {
                    new(-189.479080f, 671.6885f, 323.32883f),
                    new(-038.375523f, 675.38214f, 354.2082f),
                    new(002.4903228f, 764.1788f, 411.15732f),
                    new(032.1309000f, 754.25977f, 689.94055f),
                    new(280.3536700f, 746.5175f, 754.38336f),
                    new(460.4148000f, 723.1206f, 311.0332f),
                }
            },
            { 827, new List<Vector3> // Hydatos
                {
                    new(-933.61840f, 523.23627f, -736.6334f),
                    new(-863.59470f, 513.31824f, -440.51706f),
                    new(-812.44620f, 515.9799f, -936.0333f),
                    new(-799.73020f, 505.52838f, -109.44379f),
                    new(-678.84076f, 504.2658f, -366.4915f),
                    new(-665.98303f, 509.72754f, -589.6073f),
                    new(-614.09150f, 511.94556f, -872.6113f),
                    new(-562.17500f, 496.6946f, -13.687779f),
                    new(-438.47156f, 505.1122f, -682.6366f),
                    new(-350.35544f, 500.44604f, -108.943886f),
                    new(-322.95847f, 500.96313f, -730.7684f),
                    new(-182.39871f, 501.35736f, -591.2992f),
                    new(-159.06465f, 495.79843f, -44.438065f),
                    new(008.371242f, 504.53586f, -368.8599f),
                    new(122.102020f, 499.99933f, -616.178f),
                    new(140.437470f, 496.05386f, 12.449963f),
                    new(172.528400f, 503.0239f, -349.57162f),
                    new(226.805160f, 505.52136f, -886.2001f),
                    new(287.751900f, 505.17682f, -519.0092f),
                    new(344.529880f, 495.0796f, -212.5871f),
                    new(368.694950f, 505.93457f, -400.701f),
                    new(467.720280f, 505.08575f, -710.6624f),
                    new(580.401500f, 497.0735f, -80.38016f),
                    new(659.867600f, 502.9848f, -732.67096f),
                    new(671.426450f, 508.26538f, -341.5508f),
                    new(832.605500f, 513.5617f, -520.9632f),
                    new(843.430500f, 494.44394f, -7.3525977f),
                    new(851.303200f, 517.4547f, -883.74927f), // 28
                }
            }
        };

        public static Vector3 CalculateDistance(uint territoryId, Vector3 player)
        {
            if (!Positions.TryGetValue(territoryId, out var positions))
                return Vector3.Zero;

            foreach (var pos in positions)
            {
                var difV = player - pos;
                if (Math.Sqrt(Math.Pow(difV.X, 2f) + Math.Pow(difV.Y, 2f) + Math.Pow(difV.Z, 2f)) < InRange)
                    return pos;
            }

            return Vector3.Zero;
        }
    }
}