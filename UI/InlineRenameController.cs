using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HarmonyLib;
using ReName.Core;
using YukkuriMovieMaker.UndoRedo;
using YukkuriMovieMaker.ViewModels;
using YukkuriMovieMaker.Views;

namespace ReName.UI;

/// <summary>
/// タイムライン上のインライン名前編集セッションを管理します。
/// </summary>
internal static class InlineRenameController
{
    private static readonly FieldInfo? UndoRedoManagerField =
        AccessTools.Field(typeof(TimelineItemViewModel), "undoRedoManager");

    private static ActiveSession? _currentSession;

    public static void BeginRename(TimelineItemView itemView)
    {
        // 既に別のアイテムを編集中なら確定して閉じる
        _currentSession?.Finish(commit: true);

        if (itemView.DataContext is not TimelineItemViewModel viewModel)
        {
            return;
        }

        AdornerLayer? layer = AdornerLayer.GetAdornerLayer(itemView);
        if (layer == null)
        {
            return;
        }

        var undoRedoManager = UndoRedoManagerField?.GetValue(viewModel) as UndoRedoManager;
        string initialText = CustomNameStore.GetCustomName(viewModel.Item) ?? viewModel.Label ?? string.Empty;

        var session = new ActiveSession(itemView, viewModel, layer, undoRedoManager, initialText);
        _currentSession = session;
        session.Start();
    }

    private sealed class ActiveSession
    {
        private readonly TimelineItemView _itemView;
        private readonly TimelineItemViewModel _viewModel;
        private readonly AdornerLayer _adornerLayer;
        private readonly UndoRedoManager? _undoRedoManager;
        private readonly InlineRenameAdorner _adorner;
        private readonly Window? _ownerWindow;
        private readonly MouseButtonEventHandler _windowMouseDownHandler;

        private bool _isReady;
        private bool _isFinished;

        public ActiveSession(
            TimelineItemView itemView,
            TimelineItemViewModel viewModel,
            AdornerLayer adornerLayer,
            UndoRedoManager? undoRedoManager,
            string initialText)
        {
            _itemView = itemView;
            _viewModel = viewModel;
            _adornerLayer = adornerLayer;
            _undoRedoManager = undoRedoManager;
            _adorner = new InlineRenameAdorner(itemView, viewModel, initialText);
            _ownerWindow = Window.GetWindow(itemView);
            _windowMouseDownHandler = OnWindowPreviewMouseDown;
        }

        public void Start()
        {
            TextBox editor = _adorner.Editor;
            editor.PreviewKeyDown += OnEditorPreviewKeyDown;
            editor.LostKeyboardFocus += OnEditorLostKeyboardFocus;
            _itemView.Unloaded += OnItemViewUnloaded;
            _itemView.DataContextChanged += OnItemViewDataContextChanged;

            if (_ownerWindow != null)
            {
                _ownerWindow.AddHandler(UIElement.PreviewMouseDownEvent, _windowMouseDownHandler, handledEventsToo: true);
            }

            _adornerLayer.Add(_adorner);

            // ContextMenu が完全に閉じたタイミングで確実にフォーカスと全選択を行う
            _itemView.Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
            {
                if (_isFinished)
                {
                    return;
                }

                editor.Focus();
                Keyboard.Focus(editor);
                editor.SelectAll();
                _isReady = true;
            });
        }

        public void Finish(bool commit)
        {
            if (_isFinished)
            {
                return;
            }
            _isFinished = true;

            if (ReferenceEquals(_currentSession, this))
            {
                _currentSession = null;
            }

            TextBox editor = _adorner.Editor;
            string resultText = editor.Text;

            editor.PreviewKeyDown -= OnEditorPreviewKeyDown;
            editor.LostKeyboardFocus -= OnEditorLostKeyboardFocus;
            _itemView.Unloaded -= OnItemViewUnloaded;
            _itemView.DataContextChanged -= OnItemViewDataContextChanged;

            if (_ownerWindow != null)
            {
                _ownerWindow.RemoveHandler(UIElement.PreviewMouseDownEvent, _windowMouseDownHandler);
            }

            _adornerLayer.Remove(_adorner);

            if (commit)
            {
                CustomNameStore.SetCustomName(_viewModel.Item, resultText, _undoRedoManager);
                _itemView.InvalidateVisual();
            }
        }

        private void OnEditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // IME 変換中の Enter/Escape は Key.ImeProcessed になるためここでは反応しない
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Finish(commit: true);
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Finish(commit: false);
            }
        }

        private void OnEditorLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!_isReady)
            {
                return;
            }

            Finish(commit: true);
        }

        private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isReady)
            {
                return;
            }

            if (e.OriginalSource is DependencyObject source && IsDescendantOf(source, _adorner.Editor))
            {
                return;
            }

            Finish(commit: true);
        }

        private void OnItemViewUnloaded(object sender, RoutedEventArgs e)
        {
            Finish(commit: true);
        }

        private void OnItemViewDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Finish(commit: true);
        }

        private static bool IsDescendantOf(DependencyObject? node, DependencyObject ancestor)
        {
            while (node != null)
            {
                if (ReferenceEquals(node, ancestor))
                {
                    return true;
                }

                node = VisualTreeHelper.GetParent(node) ?? LogicalTreeHelper.GetParent(node);
            }

            return false;
        }
    }
}
