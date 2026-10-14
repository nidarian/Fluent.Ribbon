namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for <see cref="DropDownButton.DismissOnClickOutside"/>.
/// </summary>
/// <remarks>
/// An open drop down holds the mouse capture (<see cref="CaptureMode.SubTree"/>). While it does, WPF delivers a click
/// that lands outside the drop down button and its popup to the drop down button, and first raises
/// <see cref="Mouse.PreviewMouseDownOutsideCapturedElementEvent"/> on it. These tests raise that event the way WPF does.
/// Real mouse input can't be produced in the test run (the test window is off screen and not active),
/// so they check the capture state instead of a real click on the other button.
/// </remarks>
[TestFixture]
public class DropDownButtonDismissOnClickOutsideTests
{
    [Test]
    public void Outside_click_closes_the_drop_down_and_releases_capture_by_default()
    {
        var (dropDownButton, otherButton, panel) = CreateContent(dismissOnClickOutside: true);

        using (new TestRibbonWindow(panel))
        {
            Open(dropDownButton);

            Assert.That(Mouse.Captured, Is.SameAs(dropDownButton), "Precondition: the open drop down should hold the mouse capture");
            Assert.That(dropDownButton.IsAncestorOf(otherButton), Is.False, "Precondition: the other button must be outside the drop down button");

            RaiseClickOutside(dropDownButton);

            Assert.That(dropDownButton.IsDropDownOpen, Is.False, "A click outside should close the drop down");
            Assert.That(Mouse.Captured, Is.Null, "Closing should release the mouse capture");
        }
    }

    [Test]
    public void Outside_click_keeps_the_drop_down_open_and_keeps_capture_when_DismissOnClickOutside_is_false()
    {
        var (dropDownButton, otherButton, panel) = CreateContent(dismissOnClickOutside: false);

        using (new TestRibbonWindow(panel))
        {
            Open(dropDownButton);

            Assert.That(Mouse.Captured, Is.SameAs(dropDownButton), "Precondition: the open drop down should hold the mouse capture");
            Assert.That(dropDownButton.IsAncestorOf(otherButton), Is.False, "Precondition: the other button must be outside the drop down button");

            RaiseClickOutside(dropDownButton);

            Assert.That(dropDownButton.IsDropDownOpen, Is.True, "A click outside must not close the drop down when DismissOnClickOutside is false");

            // The drop down still holds the capture, so further clicks elsewhere in the window keep going to it
            // (like a dialog) until it is closed by its button, Escape, or the application losing focus.
            Assert.That(Mouse.Captured, Is.SameAs(dropDownButton), "The drop down should keep the mouse capture while it stays open");

            // Escape still closes it and gives the mouse back to the rest of the window.
            var args = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(dropDownButton), 0, Key.Escape)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            };

            dropDownButton.RaiseEvent(args);
            UIHelper.DoEvents();

            Assert.That(dropDownButton.IsDropDownOpen, Is.False, "Escape should close the drop down");
            Assert.That(Mouse.Captured, Is.Null, "Closing should release the mouse capture");
        }
    }

    private static (DropDownButton DropDownButton, System.Windows.Controls.Button OtherButton, StackPanel Panel) CreateContent(bool dismissOnClickOutside)
    {
        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            DismissOnClickOutside = dismissOnClickOutside,
            Items = { new Fluent.MenuItem { Header = "Item" } }
        };

        var otherButton = new System.Windows.Controls.Button
        {
            Content = "Other"
        };

        var panel = new StackPanel
        {
            Children =
            {
                dropDownButton,
                otherButton
            }
        };

        return (dropDownButton, otherButton, panel);
    }

    private static void Open(DropDownButton dropDownButton)
    {
        dropDownButton.ApplyTemplate();
        UIHelper.DoEvents();

        dropDownButton.IsDropDownOpen = true;
        UIHelper.DoEvents();

        Assert.That(dropDownButton.IsDropDownOpen, Is.True, "Precondition: the drop down should be open");
    }

    // What WPF raises on the captured element when the user presses a mouse button outside of it.
    private static void RaiseClickOutside(DropDownButton dropDownButton)
    {
        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = Mouse.PreviewMouseDownOutsideCapturedElementEvent
        };

        dropDownButton.RaiseEvent(args);
        UIHelper.DoEvents();
    }
}
