// ReSharper disable once CheckNamespace

namespace Fluent;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Represents a <see cref="ScrollViewer" /> specific to <see cref="RibbonGroupsContainer" />.
/// </summary>
public class RibbonGroupsContainerScrollViewer : ScrollViewer
{
    static RibbonGroupsContainerScrollViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonGroupsContainerScrollViewer), new FrameworkPropertyMetadata(typeof(RibbonGroupsContainerScrollViewer)));
        VerticalScrollBarVisibilityProperty.OverrideMetadata(typeof(RibbonGroupsContainerScrollViewer), new FrameworkPropertyMetadata(ScrollBarVisibility.Disabled));
    }

    /// <inheritdoc />
    /// <remarks>
    /// Touch panning is opt-in (PanningMode, see #1176 and the <see cref="RibbonTabItem" /> constructor).
    /// When the user drags past the first or last group, Windows would by default move the whole window
    /// as "boundary feedback". For a ribbon at the top of the window that looks broken, so it's swallowed here.
    /// </remarks>
    protected override void OnManipulationBoundaryFeedback(ManipulationBoundaryFeedbackEventArgs e)
    {
        e.Handled = true;

        base.OnManipulationBoundaryFeedback(e);
    }

    /// <inheritdoc />
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (this.ScrollInfo is null)
        {
            return;
        }

        // Prevent scrolling when a popup is open
        if (Mouse.Captured is IDropDownControl { IsDropDownOpen: true, DropDownPopup: not null } and not RibbonTabControl)
        {
            return;
        }

        if (e.Delta < 0)
        {
            this.LineRight();
        }
        else
        {
            this.LineLeft();
        }

        e.Handled = true;
    }
}