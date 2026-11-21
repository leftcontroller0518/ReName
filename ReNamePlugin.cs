using System;
using System.Reflection;
using System.Windows;
using HarmonyLib;
using ReName.Core;
using YukkuriMovieMaker.Plugin;

namespace ReName;

/// <summary>
/// タイムライン上のアイテム名を右クリックメニューからインライン編集できるYMM4プラグイン。
/// </summary>
[PluginDetails(AuthorName = "ReName")]
public sealed class ReNamePlugin : IPlugin
{
    private const string HarmonyId = "com.ymm4.plugin.rename";
    private static bool _initialized;

    public string Name => "ReName";

    public PluginDetailsAttribute Details =>
        (PluginDetailsAttribute)Attribute.GetCustomAttribute(GetType(), typeof(PluginDetailsAttribute))!;

    static ReNamePlugin()
    {
        InitializeOnce();
    }

    public ReNamePlugin()
    {
        InitializeOnce();
    }

    private static void InitializeOnce()
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;

        try
        {
            CustomNameJsonResolver.Install();

            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"ReName プラグインの初期化に失敗しました:\n{ex.Message}\n\n{ex.StackTrace}",
                "ReName Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
