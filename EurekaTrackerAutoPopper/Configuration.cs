using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Colors;

namespace EurekaTrackerAutoPopper;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public string ChatFormat = "/sh $sN pop: $p - $t";

    public bool EchoNMPop = true;
    public bool PlaySoundEffect = true;
    public int PopSoundEffect = 36;
    public bool ShowPopToast = true;
    public bool UseShortNames = true;

    public bool ShowPopWindow = true;
    public bool CopyShoutMessage = true;
    public bool ShowPullTimer = true;
    public bool UseEorzeaTimer = false;
    public bool UseTwelveHourFormat = false;

    public bool RandomizeMapCoords = true;

    public bool EchoFairies = true;
    public bool ShowFairyToast = true;
    public bool PlaceFairyFlag = false;

    public bool ShowBunnyWindow = true;
    public bool AddIconsOnEntry = true;
    public bool PlayBunnyEffect = true;
    public int BunnySoundEffect = 36;
    public bool OnlyEasyBunny = true;
    public bool BunnyCircleDraw = true;
    public Vector4 CircleColor = ImGuiColors.HealerGreen;

    public bool ClearMemory = true;
    public int ClearAfterSeconds = 60;
    public bool EchoTreasure = true;
    public bool ShowTreasureToast = true;
    public bool PlaceTreasureFlag = false;
    public bool EchoBunnyCarrot = true;
    public bool ShowBunnyCarrotToast = true;
    public bool PlaceBunnyCarrotFlag = false;
    public bool PlaceDefaultOccult = true;
    public bool ShowFastSwitcher = true;
    public bool SwitcherBelowMap = false;
    public OccultMarkerSets DefaultOccultMarkerSets = OccultMarkerSets.OccultTreasure;

    public long TimeInEureka = 0;  // in milliseconds
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

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}