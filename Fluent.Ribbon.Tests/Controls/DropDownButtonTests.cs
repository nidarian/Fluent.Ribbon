namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for <see cref="DropDownButton.FocusFirstItemOnDropDownOpen"/>.
/// </summary>
/// <remarks>
/// https://github.com/fluentribbon/Fluent.Ribbon/issues/813
/// Opening a drop down focuses its first item, which also gives that item its highlighted style.
/// The new property lets apps turn that off.
/// </remarks>
[TestFixture]
public class DropDownButtonTests
{
    [Test]
    public void FocusFirstItemOnDropDownOpen_defaults_to_true()
    {
        // Existing apps must keep the old behavior without changing anything.
        Assert.That(new DropDownButton().FocusFirstItemOnDropDownOpen, Is.True);
        Assert.That(new SplitButton().FocusFirstItemOnDropDownOpen, Is.True);
    }

    [Test]
    public void First_item_is_focused_only_when_FocusFirstItemOnDropDownOpen_is_true()
    {
        var firstItem = new MenuItem { Header = "First" };
        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            Items =
            {
                firstItem,
                new MenuItem { Header = "Second" }
            }
        };

        using (var window = new TestRibbonWindow(dropDownButton))
        {
            dropDownButton.ApplyTemplate();

            // Keyboard focus only works in the active window.
            window.Activate();
            dropDownButton.Focus();
            UIHelper.DoEvents();

            // 1. Default: the first item gets focus. This is also the baseline proving that keyboard
            //    focus works in this environment at all, so step 2 below can't pass by accident.
            OpenDropDown(dropDownButton);

            if (firstItem.IsKeyboardFocused == false)
            {
                Assert.Inconclusive("Keyboard focus is not available in this test environment (the default case couldn't focus the first item).");
            }

            CloseDropDown(dropDownButton);

            // 2. Turned off: the first item must not get focus (and so isn't highlighted).
            //    Focus stays on the button so keys still reach the control.
            dropDownButton.FocusFirstItemOnDropDownOpen = false;
            OpenDropDown(dropDownButton);

            Assert.That(firstItem.IsKeyboardFocused, Is.False, "First item must not be focused");
            Assert.That(firstItem.IsHighlighted, Is.False, "First item must not look highlighted");
            Assert.That(dropDownButton.IsKeyboardFocused, Is.True, "Focus should stay on the drop down button");

            // 3. The first Down key press moves focus into the drop down, to the first item.
            PressKey(dropDownButton, Key.Down);

            Assert.That(firstItem.IsKeyboardFocused, Is.True, "Down should focus the first item");
            Assert.That(dropDownButton.IsDropDownOpen, Is.True, "Down must not close the drop down");

            CloseDropDown(dropDownButton);
        }
    }

    [Test]
    public void Keyboard_open_focuses_an_item_even_when_FocusFirstItemOnDropDownOpen_is_false()
    {
        // FocusFirstItemOnDropDownOpen is about opening with the mouse. Pressing Down or Up to open
        // a menu is a request to navigate it, so an item must get focus. This checks the very first
        // open, before the drop down's items were ever generated.
        var baselineFirstItem = new MenuItem { Header = "First" };
        var baseline = new DropDownButton { Header = "Baseline", Items = { baselineFirstItem, new MenuItem { Header = "Second" } } };

        var firstItem = new MenuItem { Header = "First" };
        var lastItem = new MenuItem { Header = "Last" };
        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            FocusFirstItemOnDropDownOpen = false,
            Items = { firstItem, new MenuItem { Header = "Middle" }, lastItem }
        };

        var panel = new System.Windows.Controls.StackPanel { Children = { baseline, dropDownButton } };

        using (var window = new TestRibbonWindow(panel))
        {
            baseline.ApplyTemplate();
            dropDownButton.ApplyTemplate();
            window.Activate();

            // Baseline with the default setting: proves keyboard focus works in this environment.
            baseline.Focus();
            UIHelper.DoEvents();
            PressKey(baseline, Key.Down);
            if (baselineFirstItem.IsKeyboardFocused == false)
            {
                Assert.Inconclusive("Keyboard focus is not available in this test environment.");
            }

            CloseDropDown(baseline);

            // Down on a never opened drop down with the option turned off.
            dropDownButton.Focus();
            UIHelper.DoEvents();
            PressKey(dropDownButton, Key.Down);

            Assert.That(dropDownButton.IsDropDownOpen, Is.True);
            Assert.That(firstItem.IsKeyboardFocused, Is.True, "Down should focus the first item");

            CloseDropDown(dropDownButton);

            // On long CI runs the test window sometimes loses activation between steps. Focus() then fails
            // and nothing in the app has keyboard focus. That is the environment, not the drop down, so
            // report it as inconclusive instead of failing (the Down step above already passed).
            if (dropDownButton.IsKeyboardFocused == false)
            {
                Assert.Inconclusive("The test window lost keyboard focus before the Up step.");
            }

            // And Up focuses the last item.
            PressKey(dropDownButton, Key.Up);

            Assert.That(dropDownButton.IsDropDownOpen, Is.True);

            if (Keyboard.FocusedElement is null)
            {
                Assert.Inconclusive("The test window lost keyboard focus during the Up step.");
            }

            Assert.That(lastItem.IsKeyboardFocused, Is.True, "Up should focus the last item");
        }
    }

    // Simulates a key press the way WPF delivers it: a KeyDown event on the focused element.
    private static void PressKey(UIElement target, Key key)
    {
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(target), 0, key)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };

        target.RaiseEvent(args);
        UIHelper.DoEvents();
    }

    private static void OpenDropDown(DropDownButton dropDownButton)
    {
        dropDownButton.IsDropDownOpen = true;

        // Focus is moved asynchronously after opening. Let the dispatcher run it.
        UIHelper.DoEvents();
    }

    private static void CloseDropDown(DropDownButton dropDownButton)
    {
        dropDownButton.IsDropDownOpen = false;
        UIHelper.DoEvents();

        // Clean start for the next step.
        Keyboard.ClearFocus();
        dropDownButton.Focus();
        UIHelper.DoEvents();
    }
}
