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
            PrepareFocus(window, dropDownButton);

            PressKey(dropDownButton, Key.Down);

            Assert.That(dropDownButton.IsDropDownOpen, Is.True);
            Assert.That(firstItem.IsKeyboardFocused, Is.True, "Down should focus the first item");
            Assert.That(lastItem.IsKeyboardFocused, Is.False);
        }
    }

    [Test]
    public void Up_opens_and_focuses_the_last_item()
    {
        var (dropDownButton, firstItem, lastItem) = CreateDropDownButton();

        using (var window = new TestRibbonWindow(dropDownButton))
        {
            PrepareFocus(window, dropDownButton);

            // Baseline: prove keyboard focus works in this environment, so the real check can't pass by accident.
            PressKey(dropDownButton, Key.Down);
            if (firstItem.IsKeyboardFocused == false)
            {
                Assert.Inconclusive("Keyboard focus is not available in this test environment.");
            }

            dropDownButton.IsDropDownOpen = false;
            UIHelper.DoEvents();
            dropDownButton.Focus();
            UIHelper.DoEvents();

            PressKey(dropDownButton, Key.Up);

            Assert.That(dropDownButton.IsDropDownOpen, Is.True);
            Assert.That(lastItem.IsKeyboardFocused, Is.True, "Up should focus the last item");
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

    private static void PrepareFocus(Window window, DropDownButton dropDownButton)
    {
        dropDownButton.ApplyTemplate();

        // Keyboard focus only works in the active window.
        window.Activate();
        dropDownButton.Focus();
        UIHelper.DoEvents();
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
}
