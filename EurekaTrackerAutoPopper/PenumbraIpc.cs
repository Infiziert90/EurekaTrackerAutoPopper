using System;
using Penumbra.Api.Enums;
using Penumbra.Api.IpcSubscribers;

namespace EurekaTrackerAutoPopper;

public class PenumbraIpc : IDisposable
{
    public const string ModName = "CarrotReplacement [EurekaLinker]";

    public bool ActiveReplacement = false;

    private readonly AddTemporaryModAll AddTemporaryModAllFunc = new AddTemporaryModAll(Plugin.PluginInterface);
    private readonly RemoveTemporaryModAll RemoveTemporaryModAllFunc = new RemoveTemporaryModAll(Plugin.PluginInterface);

    public PenumbraApiEc AddTemporaryModAll(string gamePath, string replacedPath)
        => AddTemporaryModAllFunc.Invoke(ModName, new() { { gamePath, replacedPath } }, string.Empty, 99);

    public PenumbraApiEc RemoveTemporaryModAll()
        => RemoveTemporaryModAllFunc.Invoke(ModName, 99);

    public void Dispose()
    {
        try
        {
            var r = RemoveTemporaryModAll();
            if (r != PenumbraApiEc.Success)
                Plugin.Log.Error($"Unable to remove temporary mod.\nReason: {r}");
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "Failed to remove temporary mod.");
        }

    }
}