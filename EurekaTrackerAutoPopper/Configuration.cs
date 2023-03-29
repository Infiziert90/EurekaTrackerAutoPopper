using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Colors;

namespace EurekaTrackerAutoPopper
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public string ChatFormat = "/sh $sN pop: $p - $t";

        public bool EchoNMPop = true;
        public bool PlaySoundEffect = true;
        public bool ShowPopToast = true;
        public bool UseShortNames = true;

        public bool ShowPopWindow = true;
        public bool ShowPullTimer = true;
        public bool UseEorzeaTimer = false;
        public bool UseTwelveHourFormat = false;

        public bool RandomizeMapCoords = true;

        public bool EchoFairies = true;
        public bool ShowFairyToast = true;

        public bool ShowBunnyWindow = true;
        public bool OnlyEasyBunny = true;
        public bool BunnyCircleDraw = true;
        public Vector4 CircleColor = ImGuiColors.HealerGreen;

        public int KilledBunnies = 0;
        public Dictionary<uint, Dictionary<uint, int>> Stats = new()
        {
            { 763, new Dictionary<uint, int>
                {
                    {2009530, 0},
                    {2009531, 0},
                    {2009532, 0}
                }
            },
            { 795, new Dictionary<uint, int>
                {
                    {2009530, 0},
                    {2009531, 0},
                    {2009532, 0}
                }
            },
            { 827, new Dictionary<uint, int>
                {
                    {2009530, 0},
                    {2009531, 0},
                    {2009532, 0}
                }
            }
        };

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            pluginInterface!.SavePluginConfig(this);
        }
    }
}
