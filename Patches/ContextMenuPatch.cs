using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using HarmonyLib;
using ReName.UI;
using YukkuriMovieMaker.ViewModels;
using YukkuriMovieMaker.Views;

namespace ReName.Patches;

/// <summary>
/// タイムラインアイテムの右クリックメニュー (BackgroundContextMenu) に
/// 「名前を変更」項目を追加する Harmony パッチ。
/// </summary>
[HarmonyPatch(typeof(TimelineItemView), "EndDragging")]
internal static class ContextMenuPatch
{
    private const string RenameMenuItemTag = "ReName.RenameMenuItem";

    private static readonly FieldInfo? ModeField = AccessTools.Field(typeof(TimelineItemView), "_mode");

    [HarmonyPrefix]
    public static void Prefix(TimelineItemView __instance)
    {
        object? modeObj = ModeField?.GetValue(__instance);
        if (modeObj?.ToString() != "ContextMenu")
        {
            return;
        }

        ContextMenu? menu = __instance.BackgroundContextMenu;
        if (menu == null)
        {
            return;
        }

        EnsureRenameMenuItem(menu);
    }

    private static void EnsureRenameMenuItem(ContextMenu menu)
    {
        foreach (object? item in menu.Items)
        {
            if (item is FrameworkElement fe && Equals(fe.Tag, RenameMenuItemTag))
            {
                return;
            }
        }

        var separator = new Separator
        {
            Tag = "ReName.RenameSeparator"
        };

        var renameItem = new MenuItem
        {
            Header = "名前を変更",
            Tag = RenameMenuItemTag
        };

        renameItem.Click += (_, _) =>
        {
            if (menu.PlacementTarget is TimelineItemView targetView)
            {
                InlineRenameController.BeginRename(targetView);
            }
        };

        menu.Opened += (_, _) =>
        {
            renameItem.IsEnabled =
                menu.PlacementTarget is TimelineItemView view &&
                view.DataContext is TimelineItemViewModel;
        };

        menu.Items.Add(separator);
        menu.Items.Add(renameItem);
    }
}
