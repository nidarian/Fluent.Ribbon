namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Opening a drop down with the keyboard: Down should focus the first item, Up the last one.
/// DropDownButton.OnKeyDown focuses the last item for Up, but opening also queues a callback
/// that focuses the first item, and that callback runs later and wins.
/// </summary>
[TestFixture]
public class DropDownButtonKeyboardTests
{
    [Test]
    public void Down_opens_and_focuses_the_first_item()
    {
        var (dropDownButton, firstItem, lastItem) = CreateDropDownButton();

        using (var window = new TestRibbonWindow(dropDownButton))
        {
            var activation = PrepareFocus(window, dropDownButton);

            PressKey(dropDownButton, Key.Down);
            activation.InconclusiveIfDeactivated("Down step");

            Assert.That(dropDownButton.IsDropDownOpen, Is.True);
            UIHelper.InconclusiveIfKeyboardFocusLost("Down step");
            Assert.That(firstItem.IsKeyboardFocused, Is.True, $"Down should focus the first item, but focus was on {DescribeFocusedElement()}");
            Assert.That(lastItem.IsKeyboardFocused, Is.False);
        }
    }

    [Test]
    public void Up_opens_and_focuses_the_last_item()
    {
        var (dropDownButton, firstItem, lastItem) = CreateDropDownButton();

        using (var window = new TestRibbonWindow(dropDownButton))
        {
            var activation = PrepareFocus(window, dropDownButton);

            // Baseline: prove keyboard focus works in this environment, so the real check can't pass by accident.
            PressKey(dropDownButton, Key.Down);
            activation.InconclusiveIfDeactivated("baseline Down step");
            if (firstItem.IsKeyboardFocused == false)
            {
                Assert.Inconclusive("Keyboard focus is not available in this test environment.");
            }

            dropDownButton.IsDropDownOpen = false;
            UIHelper.DoEvents();
            dropDownButton.Focus();
            UIHelper.DoEvents();

            PressKey(dropDownButton, Key.Up);
            activation.InconclusiveIfDeactivated("Up step");

            Assert.That(dropDownButton.IsDropDownOpen, Is.True);
            UIHelper.InconclusiveIfKeyboardFocusLost("Up step");
            Assert.That(lastItem.IsKeyboardFocused, Is.True, $"Up should focus the last item, but focus was on {DescribeFocusedElement()}");
            Assert.That(firstItem.IsKeyboardFocused, Is.False);
        }
    }

    private static (DropDownButton DropDownButton, MenuItem FirstItem, MenuItem LastItem) CreateDropDownButton()
    {
        var firstItem = new MenuItem { Header = "First" };
        var lastItem = new MenuItem { Header = "Last" };

        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            Items =
            {
                firstItem,
                new MenuItem { Header = "Middle" },
                lastItem
            }
        };

        return (dropDownButton, firstItem, lastItem);
    }

    private static ActivationWatch PrepareFocus(Window window, DropDownButton dropDownButton)
    {
        dropDownButton.ApplyTemplate();

        // Keyboard focus only works in the active window.
        window.Activate();
        dropDownButton.Focus();
        UIHelper.DoEvents();

        if (window.IsActive == false)
        {
            Assert.Inconclusive("The test window could not be activated, so it can't get keyboard focus.");
        }

        return new ActivationWatch(window);
    }

    private static string DescribeFocusedElement()
    {
        return Keyboard.FocusedElement?.ToString() ?? "nothing";
    }

    // Simulates a key press the way WPF delivers it: a KeyDown event on the focused element.
    // DoEvents lets the focus callbacks queued by opening the drop down run.
    private static void PressKey(UIElement target, Key key)
    {
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(target), 0, key)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };

        target.RaiseEvent(args);
        UIHelper.DoEvents();
    }

    // The tests of the three target frameworks run at the same time, so another test window can take activation.
    // Keyboard focus then moves to that window (and deactivating closes the popup), which is an environment problem,
    // not a wrong focus. IsActive only tells the state at the moment it is checked and the window can be active again
    // by then, so this remembers any deactivation since the window was prepared.
    // A window that stayed active with focus on the wrong element still fails the test.
    private sealed class ActivationWatch
    {
        private readonly Window window;
        private bool wasDeactivated;

        public ActivationWatch(Window window)
        {
            this.window = window;
            this.window.Deactivated += (_, _) => this.wasDeactivated = true;
        }

        public void InconclusiveIfDeactivated(string step)
        {
            if (this.wasDeactivated
                || this.window.IsActive == false)
            {
                Assert.Inconclusive($"The test window was deactivated ({step}), focus is on {DescribeFocusedElement()}.");
            }
        }
    }
}
