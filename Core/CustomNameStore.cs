using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;

namespace ReName.Core;

/// <summary>
/// 各タイムラインアイテムに紐づくカスタム表示名を管理するストア。
/// Undo/Redo およびプロパティ変更通知（Label再描画）を統合します。
/// </summary>
internal static class CustomNameStore
{
    private sealed class NameHolder
    {
        public string? Name { get; set; }
    }

    private static readonly ConditionalWeakTable<IItem, NameHolder> Names = new();

    private static readonly MethodInfo? BindableOnPropertyChanged =
        AccessTools.Method(typeof(Bindable), "OnPropertyChanged", [typeof(string)]);

    /// <summary>
    /// アイテムに設定されているカスタム名を取得します。未設定の場合は null を返します。
    /// </summary>
    public static string? GetCustomName(IItem? item)
    {
        if (item == null)
        {
            return null;
        }

        return Names.TryGetValue(item, out var holder) && !string.IsNullOrEmpty(holder.Name)
            ? holder.Name
            : null;
    }

    /// <summary>
    /// JSONデシリアライズ時など、通知やUndo登録を行わずに直接カスタム名を設定します。
    /// </summary>
    public static void SetCustomNameDirect(IItem? item, string? name)
    {
        if (item == null)
        {
            return;
        }

        string? normalized = Normalize(name);
        if (normalized == null)
        {
            Names.Remove(item);
        }
        else
        {
            Names.GetOrCreateValue(item).Name = normalized;
        }
    }

    /// <summary>
    /// カスタム名を変更し、必要に応じて UndoRedoManager へ記録した上で UI を更新します。
    /// 空文字・空白のみの場合はカスタム名を解除して元のデフォルト名に戻します。
    /// </summary>
    public static void SetCustomName(IItem item, string? newName, UndoRedoManager? undoRedoManager = null)
    {
        string? oldName = GetCustomName(item);
        string? normalizedNew = Normalize(newName);

        if (string.Equals(oldName, normalizedNew, System.StringComparison.Ordinal))
        {
            return;
        }

        ApplyAndNotify(item, normalizedNew);

        if (undoRedoManager != null)
        {
            undoRedoManager.AddCommand(new RenameUndoRedoCommand(item, oldName, normalizedNew));
            undoRedoManager.Record();
        }
    }

    private static void ApplyAndNotify(IItem item, string? name)
    {
        SetCustomNameDirect(item, name);

        if (item is Bindable bindable && BindableOnPropertyChanged != null)
        {
            BindableOnPropertyChanged.Invoke(bindable, ["Label"]);
        }
    }

    private static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return raw.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
    }

    private sealed class RenameUndoRedoCommand(IItem item, string? oldName, string? newName) : IUndoRedoCommand
    {
        public bool IsAction => true;

        public bool IsEmpty => false;

        public bool CanConnect(IUndoRedoCommandBase command) => false;

        public IUndoRedoCommandBase Connect(IUndoRedoCommandBase command) =>
            throw new System.NotSupportedException();

        public void Undo() => ApplyAndNotify(item, oldName);

        public void Redo() => ApplyAndNotify(item, newName);
    }
}
