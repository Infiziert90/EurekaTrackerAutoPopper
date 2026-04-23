using System;
using System.Collections.Generic;
using Dalamud.Plugin.Ipc.Exceptions;
using Penumbra.Api.Enums;
using Penumbra.Api.IpcSubscribers;

namespace EurekaTrackerAutoPopper;

public class PenumbraIpc : IDisposable
{
    private const string ModName = "CarrotReplacement [EurekaLinker]";
    private readonly TexEdit TexEdit;

    private bool ActiveReplacement;

    private readonly Dictionary<string, string> Paths;
    private readonly AddTemporaryModAll AddTemporaryModAllFunc = new(Plugin.PluginInterface);
    private readonly RemoveTemporaryModAll RemoveTemporaryModAllFunc = new(Plugin.PluginInterface);

    public Icons GetReplacedIcon => ActiveReplacement ? Icons.CarrotReplaced : Icons.Carrot;

    public PenumbraIpc(TexEdit texEdit)
    {
        TexEdit = texEdit;
        Paths = new Dictionary<string, string> {
            { texEdit.EmptyGamePath, texEdit.ReplacementPath },
            { texEdit.EmptyGamePath.Replace("_hr1", ""), texEdit.ReplacementPath }
        };

        Initialized.Subscriber(Plugin.PluginInterface, RegisterMod);
        Disposed.Subscriber(Plugin.PluginInterface, Unregister);
    }

    public void RegisterMod()
    {
        if (TexEdit.InvalidReplacement)
            return;

        try
        {
            var r = AddTemporaryModAll();
            if (r != PenumbraApiEc.Success)
            {
                Plugin.Log.Error($"Unable to add temporary mod. Result: {r}");
                return;
            }

            ActiveReplacement = true;
        }
        catch (IpcNotReadyError ex)
        {
            Plugin.Log.Debug(ex, "Penumbra not installed, disabling feature.");
        }
    }

    private void Unregister()
    {
        ActiveReplacement = false;
    }

    private PenumbraApiEc AddTemporaryModAll()
        => AddTemporaryModAllFunc.Invoke(ModName, Paths, string.Empty, 99);

    private PenumbraApiEc RemoveTemporaryModAll()
        => RemoveTemporaryModAllFunc.Invoke(ModName, 99);

    public void Dispose()
    {
        if (!ActiveReplacement)
            return;

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