using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using YukkuriMovieMaker.Project.Items;

namespace ReName.Core;

/// <summary>
/// YMM4 の JSON シリアライズ設定へ IItem 用の仮想プロパティ "CustomName" を追加します。
/// これにより、プロジェクト保存 (.ymmp)・読み込み・複製・分割でカスタム名が自動的に維持されます。
/// </summary>
internal sealed class CustomNameJsonResolver : DefaultContractResolver
{
    private const string JsonPropertyName = "CustomName";

    private static readonly CustomNameJsonResolver Instance = new();
    private static readonly IValueProvider CustomNameProvider = new CustomNameValueProvider();

    private static bool _installed;

    public static void Install()
    {
        if (_installed)
        {
            return;
        }
        _installed = true;

        var jsonType = typeof(YukkuriMovieMaker.Json.Json);
        ApplyToSettingsField(jsonType, "settings");
        ApplyToSettingsField(jsonType, "indentedSettings");
    }

    private static void ApplyToSettingsField(Type jsonType, string fieldName)
    {
        FieldInfo? field = AccessTools.Field(jsonType, fieldName);
        if (field?.GetValue(null) is JsonSerializerSettings settings && settings.ContractResolver == null)
        {
            settings.ContractResolver = Instance;
        }
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

        if (typeof(IItem).IsAssignableFrom(type))
        {
            bool exists = false;
            for (int i = 0; i < properties.Count; i++)
            {
                if (string.Equals(properties[i].PropertyName, JsonPropertyName, StringComparison.Ordinal))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                properties.Add(new JsonProperty
                {
                    PropertyName = JsonPropertyName,
                    PropertyType = typeof(string),
                    DeclaringType = type,
                    Readable = true,
                    Writable = true,
                    ValueProvider = CustomNameProvider,
                    ShouldSerialize = instance =>
                        instance is IItem item && !string.IsNullOrEmpty(CustomNameStore.GetCustomName(item))
                });
            }
        }

        return properties;
    }

    private sealed class CustomNameValueProvider : IValueProvider
    {
        public object? GetValue(object target) =>
            target is IItem item ? CustomNameStore.GetCustomName(item) : null;

        public void SetValue(object target, object? value)
        {
            if (target is IItem item)
            {
                CustomNameStore.SetCustomNameDirect(item, value as string);
            }
        }
    }
}
