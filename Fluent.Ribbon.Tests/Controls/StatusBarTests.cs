namespace Fluent.Tests.Controls;

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class StatusBarTests
{
    [Test]
    public void Moving_Item_In_ItemsSource_Should_Move_Context_Menu_Item_Without_Duplicating_It()
    {
        var items = new ObservableCollection<string> { "A", "B", "C" };
        var statusBar = new StatusBar
        {
            ItemsSource = items
        };

        using (new TestRibbonWindow(statusBar))
        {
            // Containers must be generated, otherwise StatusBar ignores collection changes and waits for the generator.
            UIHelper.DoEvents();

            var contextMenu = statusBar.ContextMenu;

            Assert.That(contextMenu, Is.Not.Null, "StatusBar should provide a context menu.");
            Assert.That(contextMenu.Items[0], Is.InstanceOf<GroupSeparatorMenuItem>(), "First context menu item should be the header.");
            Assert.That(GetMenuItemsOrder(statusBar), Is.EqualTo(new[] { "A", "B", "C" }), "Precondition: context menu should mirror the initial items.");

            items.Move(0, 2);

            // Assert directly after the move: this is the state the Move branch of OnItemsChanged produces.
            // A later, dispatcher-queued menu rebuild (triggered by container regeneration) could hide the problem.
            Assert.That(contextMenu.Items.Count, Is.EqualTo(1 + items.Count), "Header plus one menu item per status bar item.");
            Assert.That(GetMenuItemsOrder(statusBar), Is.EqualTo(new[] { "B", "C", "A" }));
        }
    }

    // Issue #708: an app wants a ToolTip ("Zoom") and a ContextMenu (zoom presets) on the zoom slider in the status bar.
    // WPF shows a ContextMenu when a right click (MouseUp, right button) stays unhandled; it then uses the ContextMenu
    // of the nearest element up from the clicked one. So neither the ZoomSlider template parts nor the StatusBar
    // (which has its own "Customize Status Bar" menu) may handle the right click, put a menu or tooltip of their own
    // on the parts, or replace the menu and tooltip the app set on the slider.
    [Test]
    public void ZoomSlider_In_StatusBar_Should_Keep_ToolTip_And_ContextMenu_Set_By_App_And_Not_Handle_Right_Click()
    {
        var zoomMenu = new System.Windows.Controls.ContextMenu();
        zoomMenu.Items.Add(new System.Windows.Controls.MenuItem { Header = "100%" });

        var slider = new System.Windows.Controls.Slider
        {
            ToolTip = "Zoom",
            ContextMenu = zoomMenu
        };
        slider.SetResourceReference(FrameworkElement.StyleProperty, "Fluent.Ribbon.Styles.ZoomSlider");

        var statusBar = new StatusBar();
        statusBar.Items.Add(new StatusBarItem
        {
            Title = "Zoom Slider",
            IsCheckable = false,
            Content = slider
        });

        using (new TestRibbonWindow(statusBar))
        {
            UIHelper.DoEvents();

            Assert.That(slider.Style, Is.SameAs(Application.Current.FindResource("Fluent.Ribbon.Styles.ZoomSlider")), "Precondition: the slider should use the ZoomSlider style.");
            Assert.That(slider.Template, Is.SameAs(Application.Current.FindResource("Fluent.Ribbon.Templates.ZoomSlider")), "Precondition: the ZoomSlider template should be applied.");

            Assert.That(slider.ContextMenu, Is.SameAs(zoomMenu), "The slider should keep the ContextMenu set by the app.");
            Assert.That(slider.ToolTip, Is.EqualTo("Zoom"), "The slider should keep the ToolTip set by the app.");
            Assert.That(statusBar.ContextMenu, Is.Not.SameAs(zoomMenu), "The StatusBar keeps its own menu, separate from the slider's.");
            Assert.That(System.Windows.Controls.ContextMenuService.GetIsEnabled(slider), Is.True, "ContextMenuService should be enabled on the slider.");
            Assert.That(System.Windows.Controls.ToolTipService.GetIsEnabled(slider), Is.True, "ToolTipService should be enabled on the slider.");

            foreach (var partName in new[] { "leftButton", "thumb", "rightButton", "repeatButton", "repeatButton_Copy", "rectangle" })
            {
                var part = slider.Template.FindName(partName, slider) as FrameworkElement;

                Assert.That(part, Is.Not.Null, $"Precondition: template part {partName} should exist.");
                Assert.That(part!.ContextMenu, Is.Null, $"{partName} should not have a ContextMenu of its own that would hide the slider's.");
                Assert.That(part.ToolTip, Is.Null, $"{partName} should not have a ToolTip of its own that would hide the slider's.");

                var mouseDown = RightButtonArgs(Mouse.MouseDownEvent);
                part.RaiseEvent(mouseDown);
                UIHelper.DoEvents();

                var mouseUp = RightButtonArgs(Mouse.MouseUpEvent);
                part.RaiseEvent(mouseUp);
                UIHelper.DoEvents();

                Assert.That(mouseDown.Handled, Is.False, $"Right mouse down on {partName} should not be handled.");
                Assert.That(mouseUp.Handled, Is.False, $"Right mouse up on {partName} should not be handled, otherwise WPF does not open the slider's ContextMenu.");
            }
        }
    }

    // StatusBar hides a Separator when nothing visible stands before it (leading) or when the
    // visible element before it is another Separator (doubled gap). That rule ran only when an
    // item was checked/unchecked or the context menu was rebuilt, not when items were removed,
    // so removing an item left a doubled or leading separator gap visible.
    [Test]
    public void Removing_Item_Between_Separators_Should_Collapse_Doubled_Separator()
    {
        var (statusBar, _, s1, b, s2, _) = CreateStatusBarWithSeparators();

        using (new TestRibbonWindow(statusBar))
        {
            UIHelper.DoEvents();

            Assert.That(GetSeparatorVisibilities(statusBar), Is.EqualTo(new[] { Visibility.Visible, Visibility.Visible }), "Precondition: both separators are visible.");

            statusBar.Items.Remove(b);
            UIHelper.DoEvents();

            AssertSeparatorsFollowRule(statusBar);
            Assert.That(s2.Visibility, Is.EqualTo(Visibility.Collapsed), "The second separator directly follows the first one now and should be collapsed.");
            Assert.That(s1.Visibility, Is.EqualTo(Visibility.Visible));
        }
    }

    [Test]
    public void Removing_First_Item_Should_Collapse_Leading_Separator()
    {
        var (statusBar, a, s1, _, _, _) = CreateStatusBarWithSeparators();

        using (new TestRibbonWindow(statusBar))
        {
            UIHelper.DoEvents();

            Assert.That(GetSeparatorVisibilities(statusBar), Is.EqualTo(new[] { Visibility.Visible, Visibility.Visible }), "Precondition: both separators are visible.");

            statusBar.Items.Remove(a);
            UIHelper.DoEvents();

            AssertSeparatorsFollowRule(statusBar);
            Assert.That(s1.Visibility, Is.EqualTo(Visibility.Collapsed), "The first separator is the leading element now and should be collapsed.");
        }
    }

    [Test]
    public void Removing_Separator_Should_Show_Separator_That_Was_Collapsed_As_Doubled()
    {
        var (statusBar, _, s1, b, s2, _) = CreateStatusBarWithSeparators();
        b.IsChecked = false;

        using (new TestRibbonWindow(statusBar))
        {
            UIHelper.DoEvents();

            Assert.That(GetSeparatorVisibilities(statusBar), Is.EqualTo(new[] { Visibility.Visible, Visibility.Collapsed }), "Precondition: the second separator is collapsed because B is hidden.");

            statusBar.Items.Remove(s1);
            UIHelper.DoEvents();

            AssertSeparatorsFollowRule(statusBar);
            Assert.That(s2.Visibility, Is.EqualTo(Visibility.Visible), "The remaining separator now stands between A and C and should be visible.");
        }
    }

    [Test]
    public void Replacing_Item_With_Hidden_Item_Should_Collapse_Doubled_Separator()
    {
        var (statusBar, _, _, _, s2, _) = CreateStatusBarWithSeparators();

        using (new TestRibbonWindow(statusBar))
        {
            UIHelper.DoEvents();

            Assert.That(GetSeparatorVisibilities(statusBar), Is.EqualTo(new[] { Visibility.Visible, Visibility.Visible }), "Precondition: both separators are visible.");

            statusBar.Items[2] = new StatusBarItem { Title = "Hidden", Value = "Hidden", IsChecked = false };
            UIHelper.DoEvents();

            AssertSeparatorsFollowRule(statusBar);
            Assert.That(s2.Visibility, Is.EqualTo(Visibility.Collapsed), "Only a hidden item stands between the separators now, so the second one should be collapsed.");
        }
    }

    // Guard for the existing behaviour: unchecking/checking an item updates the separators.
    [Test]
    public void Unchecking_And_Checking_Item_Should_Update_Separators()
    {
        var (statusBar, _, _, b, _, _) = CreateStatusBarWithSeparators();

        using (new TestRibbonWindow(statusBar))
        {
            UIHelper.DoEvents();

            b.IsChecked = false;
            UIHelper.DoEvents();

            AssertSeparatorsFollowRule(statusBar);
            Assert.That(GetSeparatorVisibilities(statusBar), Is.EqualTo(new[] { Visibility.Visible, Visibility.Collapsed }));

            b.IsChecked = true;
            UIHelper.DoEvents();

            AssertSeparatorsFollowRule(statusBar);
            Assert.That(GetSeparatorVisibilities(statusBar), Is.EqualTo(new[] { Visibility.Visible, Visibility.Visible }));
        }
    }

    private static (StatusBar StatusBar, StatusBarItem A, System.Windows.Controls.Separator S1, StatusBarItem B, System.Windows.Controls.Separator S2, StatusBarItem C) CreateStatusBarWithSeparators()
    {
        var a = new StatusBarItem { Title = "A", Value = "A" };
        var s1 = new System.Windows.Controls.Separator();
        var b = new StatusBarItem { Title = "B", Value = "B" };
        var s2 = new System.Windows.Controls.Separator();
        var c = new StatusBarItem { Title = "C", Value = "C" };

        var statusBar = new StatusBar();
        statusBar.Items.Add(a);
        statusBar.Items.Add(s1);
        statusBar.Items.Add(b);
        statusBar.Items.Add(s2);
        statusBar.Items.Add(c);

        return (statusBar, a, s1, b, s2, c);
    }

    private static Visibility[] GetSeparatorVisibilities(StatusBar statusBar)
    {
        return statusBar.Items
            .OfType<System.Windows.Controls.Separator>()
            .Select(x => x.Visibility)
            .ToArray();
    }

    // The rule StatusBar applies: a separator is collapsed when no visible item stands before it,
    // either at the start or since the previous separator; otherwise it is visible.
    private static void AssertSeparatorsFollowRule(StatusBar statusBar)
    {
        var visibleItemSinceLastSeparator = false;

        for (var i = 0; i < statusBar.Items.Count; i++)
        {
            if (statusBar.Items[i] is System.Windows.Controls.Separator separator)
            {
                var expected = visibleItemSinceLastSeparator
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                Assert.That(separator.Visibility, Is.EqualTo(expected), $"Separator at index {i} should be {expected}.");

                visibleItemSinceLastSeparator = false;
            }
            else if (statusBar.Items[i] is StatusBarItem { Visibility: Visibility.Visible })
            {
                visibleItemSinceLastSeparator = true;
            }
        }
    }

    private static MouseButtonEventArgs RightButtonArgs(RoutedEvent routedEvent)
    {
        return new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
        {
            RoutedEvent = routedEvent
        };
    }

    // Maps every menu item (except the header) back to the data item of the status bar item it represents.
    private static object[] GetMenuItemsOrder(StatusBar statusBar)
    {
        return statusBar.ContextMenu.Items
            .Cast<object>()
            .Skip(1)
            .Select(x => x is StatusBarMenuItem menuItem
                ? statusBar.ItemContainerGenerator.ItemFromContainer(menuItem.StatusBarItem)
                : x)
            .ToArray();
    }
}
