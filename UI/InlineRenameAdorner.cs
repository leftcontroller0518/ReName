using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using YukkuriMovieMaker.ViewModels;
using YukkuriMovieMaker.Views;

namespace ReName.UI;

/// <summary>
/// TimelineItemView の名前表示位置にインライン TextBox を重ねて表示する Adorner。
/// </summary>
internal sealed class InlineRenameAdorner : Adorner
{
    private const double MarginTop = 4.0;
    private const double MinEditorWidth = 120.0;
    private const double MaxEditorWidth = 420.0;
    private const double MinEditorHeight = 20.0;

    private readonly VisualCollection _visuals;
    private readonly TimelineItemView _itemView;
    private readonly TimelineItemViewModel _viewModel;

    public TextBox Editor { get; }

    public InlineRenameAdorner(TimelineItemView adornedElement, TimelineItemViewModel viewModel, string initialText)
        : base(adornedElement)
    {
        _itemView = adornedElement;
        _viewModel = viewModel;
        _visuals = new VisualCollection(this);

        Editor = new TextBox
        {
            Text = initialText,
            FontFamily = SystemFonts.MessageFontFamily,
            FontSize = SystemFonts.MessageFontSize,
            Padding = new Thickness(3.0, 0.0, 3.0, 0.0),
            Margin = new Thickness(0.0),
            BorderThickness = new Thickness(1.0),
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            AcceptsReturn = false,
            AcceptsTab = false,
            Background = new SolidColorBrush(Color.FromRgb(0x20, 0x22, 0x26)),
            Foreground = Brushes.White,
            CaretBrush = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x4A, 0xA3, 0xFF)),
            SelectionBrush = new SolidColorBrush(Color.FromRgb(0x26, 0x4F, 0x78))
        };

        _visuals.Add(Editor);
    }

    protected override int VisualChildrenCount => _visuals.Count;

    protected override Visual GetVisualChild(int index) => _visuals[index];

    protected override Size MeasureOverride(Size constraint)
    {
        Rect rect = CalculateEditorRect();
        Editor.Measure(rect.Size);
        return _itemView.RenderSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Rect rect = CalculateEditorRect();
        Editor.Arrange(rect);
        return finalSize;
    }

    private Rect CalculateEditorRect()
    {
        double itemWidth = Math.Max(0.0, _itemView.RenderSize.Width);
        double itemHeight = Math.Max(MinEditorHeight, _itemView.RenderSize.Height - MarginTop);

        double originX = Math.Max(2.0, _viewModel.LabelMargin.Left + 4.0);
        double availableWidth = Math.Max(0.0, itemWidth - originX - 4.0);
        double width = Math.Clamp(Math.Max(availableWidth, MinEditorWidth), MinEditorWidth, MaxEditorWidth);

        double height = Math.Max(MinEditorHeight, itemHeight);
        double y = MarginTop + Math.Max(0.0, (itemHeight - height) * 0.5);

        return new Rect(originX, y, width, height);
    }
}
