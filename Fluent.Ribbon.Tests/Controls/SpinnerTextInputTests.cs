namespace Fluent.Tests.Controls;

using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class SpinnerTextInputTests
{
    [Test]
    public void Losing_Keyboard_Focus_Without_Editing_Keeps_Precise_Value()
    {
        // 0.25 can't be shown with one decimal, so the text box shows a rounded value.
        var spinner = new Spinner
        {
            Format = "F1",
            Value = 0.25
        };

        using (new TestRibbonWindow(spinner))
        {
            UIHelper.DoEvents();

            var textBox = (System.Windows.Controls.TextBox)spinner.Template.FindName("PART_TextBox", spinner);

            Assert.That(textBox, Is.Not.Null, "Precondition: PART_TextBox must exist in the template.");
            Assert.That(textBox.Text, Is.EqualTo(spinner.Text), "Precondition: text box shows the formatted value.");
            Assert.That(spinner.Value, Is.EqualTo(0.25), "Precondition: value is not rounded by formatting.");

            // Simulate the user tabbing through the spinner without typing anything.
            textBox.RaiseEvent(new KeyboardFocusChangedEventArgs(Keyboard.PrimaryDevice, 0, textBox, null)
            {
                RoutedEvent = Keyboard.LostKeyboardFocusEvent
            });

            Assert.That(spinner.Value, Is.EqualTo(0.25));
        }
    }
}
