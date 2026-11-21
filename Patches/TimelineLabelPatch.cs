using System.Reflection;
using HarmonyLib;
using ReName.Core;
using YukkuriMovieMaker.ViewModels;

namespace ReName.Patches;

/// <summary>
/// TimelineItemViewModel.LoadIItemProperty の後にカスタム名を Label へ反映します。
/// </summary>
[HarmonyPatch(typeof(TimelineItemViewModel), "LoadIItemProperty", [typeof(string)])]
internal static class TimelineLabelPatch
{
    private static readonly MethodInfo? LabelSetter =
        AccessTools.PropertySetter(typeof(TimelineItemViewModel), nameof(TimelineItemViewModel.Label));

    [HarmonyPostfix]
    public static void Postfix(TimelineItemViewModel __instance, string? propertyName)
    {
        if (propertyName != "Label")
        {
            return;
        }

        string? customName = CustomNameStore.GetCustomName(__instance.Item);
        if (!string.IsNullOrEmpty(customName))
        {
            LabelSetter?.Invoke(__instance, [customName]);
        }
    }
}
