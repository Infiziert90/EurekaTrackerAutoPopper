using System;
using System.IO;
using System.Runtime.InteropServices;
using Dalamud.Utility;
using Lumina.Data.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace EurekaTrackerAutoPopper;

public class TexEdit
{
    public string GamePath = string.Empty;
    public string EmptyGamePath = string.Empty;
    public string ReplacementPath = string.Empty;

    public void EditIcon(uint iconId, uint emptySlotId)
    {
        GamePath = $"ui/icon/{Utils.GetIconPath(iconId)}_hr1.tex";
        EmptyGamePath = $"ui/icon/{Utils.GetIconPath(emptySlotId)}_hr1.tex";
        if (Plugin.Data.FileExists(EmptyGamePath))
        {
            Plugin.Log.Error($"Empty icon file {EmptyGamePath} already exists!");
            return;
        }

        var tex = Plugin.Data.GetFile<TexFile>(GamePath)!;

        var width = tex.Header.Width;
        var height = tex.Header.Height;

        var threshold = 0.004f;

        var sourceColor = Color.FromRgb(110, 108, 99);
        var targetColor = Color.Transparent;
        var brushLightGray = new RecolorBrush(sourceColor, targetColor, threshold);

        sourceColor = Color.FromRgb(77, 78, 74);
        targetColor = Color.Transparent;
        var brushDarkGrey = new RecolorBrush(sourceColor, targetColor, threshold);

        sourceColor = Color.FromRgb(82, 65, 49);
        targetColor = Color.Transparent;
        var brushEdgeBrown = new RecolorBrush(sourceColor, targetColor, threshold);

        sourceColor = Color.FromRgb(93, 94, 85);
        targetColor = Color.Transparent;
        var brushEdgeGray = new RecolorBrush(sourceColor, targetColor, threshold);

        using var iconTex = Image.LoadPixelData<Rgba32>(tex.GetRgbaImageData(), width, height);
        iconTex.Mutate(d => d.Clear(brushLightGray).Clear(brushDarkGrey).Clear(brushEdgeBrown).Clear(brushEdgeGray));
        using var iconBgra = iconTex.CloneAs<Bgra32>();

        var outputPath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "icon.tex");
        WriteTexFile(outputPath, 80, 80, iconBgra.ImageToRaw());
        ReplacementPath = outputPath;
    }

    private static unsafe void WriteTexFile(string filename, uint width, uint height, ReadOnlySpan<byte> pixelData)
    {
        var header = new TexFile.TexHeader
        {
            Type = TexFile.Attribute.TextureType2D,
            Format = TexFile.TextureFormat.B8G8R8A8,
            Width = (ushort)width,
            Height = (ushort)height,
            Depth = 1,
            MipCount = 1,
            MipUnknownFlag = false,
            ArraySize = 0,
        };

        header.LodOffset[0] = 0;
        header.LodOffset[1] = 0;
        header.LodOffset[2] = 0;

        header.OffsetToSurface[0] = (uint)Marshal.SizeOf<TexFile.TexHeader>();
        for (var mip = 1; mip < 13; mip++)
        {
            header.OffsetToSurface[mip] = 0;
        }

        using var stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
        stream.Write(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref header, 1)));
        stream.Write(pixelData);
    }
}