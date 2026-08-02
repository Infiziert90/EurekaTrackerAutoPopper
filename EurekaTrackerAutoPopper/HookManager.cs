using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Network;
using FFXIVClientStructs.FFXIV.Client.Network;

namespace EurekaTrackerAutoPopper;

public unsafe class HookManager
{
    private readonly Plugin Plugin;

    private Hook<PacketDispatcher.Delegates.HandleZoneInitPacket>? ZoneInitHook { get; init; }

    public uint ServerId;

    public HookManager(Plugin plugin)
    {
        Plugin = plugin;

        ZoneInitHook = Plugin.Hook.HookFromAddress<PacketDispatcher.Delegates.HandleZoneInitPacket>(PacketDispatcher.MemberFunctionPointers.HandleZoneInitPacket, ZoneInitDetour);
        ZoneInitHook.Enable();
    }

    private void ZoneInitDetour(uint entityId, ZoneInitPacket* packet, byte a3)
    {
        try
        {
            ZoneInitHook!.Original(entityId, packet, a3);

            ServerId = packet->ServerId;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "ZoneInitDetour failed.");
        }
    }

    public void Dispose()
    {
        ZoneInitHook?.Dispose();
    }
}
