namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for the Tab key on an open <see cref="DropDownButton"/>.
/// </summary>
[TestFixture]
public class DropDownButtonTabKeyTests
{
    [Test]
    public void Tab_on_the_button_closes_the_open_drop_down()
    {
        // With only disabled items nothing inside the drop down can take focus,
        // so keyboard input keeps going to the button itself while the drop down is open.
        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            Items = { new MenuItem { Header = "Disabled", IsEnabled = false } }
        };

        using (new TestRibbonWindow(dropDownButton))
        {
            dropDownButton.ApplyTemplate();

            dropDownButton.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(dropDownButton.IsDropDownOpen, Is.True, "Precondition: the drop down should be open");
            Assert.That(dropDownButton.DropDownPopup?.Child?.IsKeyboardFocusWithin, Is.Not.True, "Precondition: focus must not be inside the drop down");

            // Tab moves focus away from the button. The drop down must not stay open, detached from the focus.
            var args = PressKey(dropDownButton, Key.Tab);

            Assert.That(dropDownButton.IsDropDownOpen, Is.False, "Tab should close the drop down");
            Assert.That(args.Handled, Is.False, "Tab must stay unhandled so WPF still moves focus to the next control");
        }
    }

    // Simulates a key press the way WPF delivers it: a KeyDown event on the focused element.
    private static KeyEventArgs PressKey(UIElement target, Key key)
    {
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(target), 0, key)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };

        target.RaiseEvent(args);
        UIHelper.DoEvents();

        return args;
    }
}
