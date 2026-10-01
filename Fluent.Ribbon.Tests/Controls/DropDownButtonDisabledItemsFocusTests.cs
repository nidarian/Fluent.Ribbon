namespace Fluent.Tests.Controls;

using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for where keyboard focus goes when a <see cref="DropDownButton"/> opens but none of its items can take focus.
/// </summary>
[TestFixture]
public class DropDownButtonDisabledItemsFocusTests
{
    [Test]
    public void Opening_with_only_disabled_items_moves_focus_to_the_drop_down_button()
    {
        // The only item is disabled, so the "focus the first item" step on open can't do anything.
        var disabledItem = new MenuItem { Header = "Disabled", IsEnabled = false };
        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            Items = { disabledItem }
        };

        // A plain WPF TextBox (Fluent has its own TextBox, hence the full name) that holds focus before the drop down opens.
        var textBox = new System.Windows.Controls.TextBox();

        var panel = new System.Windows.Controls.StackPanel { Children = { textBox, dropDownButton } };

        using (var window = new TestRibbonWindow(panel))
        {
            dropDownButton.ApplyTemplate();

            // Keyboard focus only works in the active window.
            window.Activate();
            Keyboard.Focus(textBox);
            UIHelper.DoEvents();

            if (textBox.IsKeyboardFocused == false)
            {
                Assert.Inconclusive("Keyboard focus is not available in this test environment (the TextBox couldn't be focused).");
            }

            // Opening goes through OnIsDropDownOpenChanged, which tries to focus the first item and,
            // when that fails, falls back to focusing the popup content. Key tips and clicks open it the same way.
            dropDownButton.IsDropDownOpen = true;

            // Focus is moved asynchronously after opening. Let the dispatcher run it.
            UIHelper.DoEvents();

            Assert.That(dropDownButton.IsDropDownOpen, Is.True, "Precondition: the drop down should be open");
            Assert.That(disabledItem.IsKeyboardFocused, Is.False, "Precondition: a disabled item can't take focus");

            // In a long test run the hidden test window can lose keyboard focus altogether; then nothing has focus
            // and the result says nothing about the code. With the bug, focus stays on the TextBox (not null).
            if (Keyboard.FocusedElement is null)
            {
                Assert.Inconclusive("The test window lost keyboard focus while the drop down was opening.");
            }

            // Focus must not be left behind on the TextBox, otherwise keys (Escape, arrows, Tab)
            // go to the TextBox instead of the open drop down.
            Assert.That(dropDownButton.IsKeyboardFocusWithin, Is.True, "Focus should move to the drop down button when no item can take it");
            Assert.That(textBox.IsKeyboardFocused, Is.False, "Focus must not stay on the TextBox");

            dropDownButton.IsDropDownOpen = false;
            UIHelper.DoEvents();
        }
    }
}
