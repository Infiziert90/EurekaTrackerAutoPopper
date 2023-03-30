using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CheapLoc;
using Dalamud;
using Dalamud.Logging;

namespace EurekaTrackerAutoPopper;

public class Localization
{
    public static readonly string[] ApplicableLangCodes = { "de", "ja", "fr" };

    private const string FallbackLangCode = "en";
    private const string LocResourceDirectory = "loc";

    private readonly Assembly assembly;

    public Localization()
    {
        assembly = Assembly.GetCallingAssembly();
    }

    public void ExportLocalizable() => Loc.ExportLocalizableForAssembly(assembly);
    public void SetupWithFallbacks() => Loc.SetupWithFallbacks(assembly);

    public void SetupWithLangCode(string langCode)
    {
        if (langCode.ToLower() == FallbackLangCode || !ApplicableLangCodes.Contains(langCode.ToLower()))
        {
            SetupWithFallbacks();
            return;
        }

        try
        {
            Loc.Setup(ReadLocData(langCode), assembly);
        }
        catch (Exception)
        {
            PluginLog.Warning($"Could not load loc {langCode}. Setting up fallbacks.");
            SetupWithFallbacks();
        }
    }

    private string ReadLocData(string langCode)
    {
        return File.ReadAllText(Path.Combine(Plugin.DalamudPluginInterface.AssemblyLocation.DirectoryName!, LocResourceDirectory, $"{langCode}.json"));
    }

    public static ClientLanguage LangCodeToClientLanguage(string langCode)
    {
        return langCode switch
        {
            "en" => ClientLanguage.English,
            "de" => ClientLanguage.German,
            "fr" => ClientLanguage.French,
            "ja" => ClientLanguage.Japanese,
            _ => ClientLanguage.English
        };
    }
}