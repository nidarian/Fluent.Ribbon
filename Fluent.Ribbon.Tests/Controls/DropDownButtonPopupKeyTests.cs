namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for Enter and Space pressed inside the open drop down of a <see cref="DropDownButton"/>.
/// </summary>
[TestFixture]
public class DropDownButtonPopupKeyTests
{
    [Test]
    [TestCase(Key.Space)]
    [TestCase(Key.Enter)]
    public void Key_typed_into_a_text_box_inside_the_drop_down_keeps_it_open(Key key)
    {
        var textBox = new System.Windows.Controls.TextBox();
        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            Items = { textBox }
        };

        using (new TestRibbonWindow(dropDownButton))
        {
            dropDownButton.ApplyTemplate();

            dropDownButton.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(dropDownButton.IsDropDownOpen, Is.True, "Precondition: the drop down should be open");
            Assert.That(PresentationSource.FromVisual(textBox), Is.Not.Null, "Precondition: the text box should be shown in the drop down");

            // The text box doesn't handle the key on KeyDown (it types on TextInput), so the event bubbles up to the button.
            PressKey(textBox, key);

            Assert.That(dropDownButton.IsDropDownOpen, Is.True, $"{key} typed into the text box must not close the drop down");
        }
    }

    [Test]
    [TestCase(Key.Space)]
    [TestCase(Key.Enter)]
    public void Key_on_the_button_still_opens_and_closes_the_drop_down(Key key)
    {
        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            Items = { new System.Windows.Controls.TextBox() }
        };

        using (new TestRibbonWindow(dropDownButton))
        {
            dropDownButton.ApplyTemplate();
            UIHelper.DoEvents();

            Assert.That(dropDownButton.IsDropDownOpen, Is.False, "Precondition: the drop down should be closed");

            var args = PressKey(dropDownButton, key);

            Assert.That(dropDownButton.IsDropDownOpen, Is.True, $"{key} on the button should open the drop down");
            Assert.That(args.Handled, Is.True);

            PressKey(dropDownButton, key);

            Assert.That(dropDownButton.IsDropDownOpen, Is.False, $"{key} on the button should close the drop down again");
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
