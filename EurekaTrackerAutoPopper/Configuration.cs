using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

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
        
        // the below exist just to make saving less cumbersome

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
