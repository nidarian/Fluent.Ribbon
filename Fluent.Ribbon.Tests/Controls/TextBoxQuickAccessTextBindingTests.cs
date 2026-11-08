namespace Fluent.Tests.Controls;

using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// The Quick Access Toolbar copy of a text box is kept in sync with the original. Typing in the copy must not replace an app's
/// OneWay binding on the original's Text with a local value (the same bug as the combo box, fixed in round 7).
/// </summary>
[TestFixture]
public class TextBoxQuickAccessTextBindingTests
{
    [Test]
    public void QuickAccessCopy_Typing_Should_Keep_OneWay_Text_Binding()
    {
        var model = new TextModel { Text = "Initial" };
        var textBox = new Fluent.TextBox();
        textBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, new Binding(nameof(TextModel.Text)) { Source = model, Mode = BindingMode.OneWay });
        var quickAccessCopy = (Fluent.TextBox)textBox.CreateQuickAccessItem();

        using (new TestRibbonWindow(new StackPanel { Children = { textBox, quickAccessCopy } }))
        {
            UIHelper.DoEvents();
            Assert.That(quickAccessCopy.Text, Is.EqualTo("Initial"), "Precondition: the copy shows the text of the original");

            // Roughly what typing does: the copy's own Text changes without replacing its binding (the next test simulates a key press through SelectedText).
            quickAccessCopy.SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, "Typed");
            UIHelper.DoEvents();

            Assert.That(textBox.Text, Is.EqualTo("Typed"), "The text typed in the copy must reach the original");
            Assert.That(BindingOperations.GetBindingExpression(textBox, System.Windows.Controls.TextBox.TextProperty), Is.Not.Null, "The app's OneWay binding on Text was lost");

            model.Text = "FromModel";
            UIHelper.DoEvents();

            Assert.That(textBox.Text, Is.EqualTo("FromModel"), "The original must follow the view model again");
            Assert.That(quickAccessCopy.Text, Is.EqualTo("FromModel"), "The copy must show the text of the view model");
        }
    }

    // Typing in the middle of the copy must not replace the copy's whole text when the original's text comes back to it through
    // the OneWay binding, which would reset its caret or selection on every key press.
    [Test]
    public void QuickAccessCopy_Typing_In_The_Middle_Keeps_The_Copys_Selection()
    {
        var model = new TextModel { Text = "Hello" };
        var textBox = new Fluent.TextBox();
        textBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, new Binding(nameof(TextModel.Text)) { Source = model, Mode = BindingMode.OneWay });
        var quickAccessCopy = (Fluent.TextBox)textBox.CreateQuickAccessItem();

        using (new TestRibbonWindow(new StackPanel { Children = { textBox, quickAccessCopy } }))
        {
            UIHelper.DoEvents();
            Assert.That(quickAccessCopy.Text, Is.EqualTo("Hello"), "Precondition: the copy shows the text of the original");

            // Replacing the selection is what a key press does (it changes the text content, not the Text property directly).
            quickAccessCopy.Select(2, 0);
            quickAccessCopy.SelectedText = "X";
            UIHelper.DoEvents();

            Assert.That(quickAccessCopy.Text, Is.EqualTo("HeXllo"), "Precondition: the typed character is in the copy");
            Assert.That(textBox.Text, Is.EqualTo("HeXllo"), "The typed text must reach the original");
            Assert.That(quickAccessCopy.SelectionStart, Is.EqualTo(2), "The copy's selection must stay on the typed character");
            Assert.That(quickAccessCopy.SelectionLength, Is.EqualTo(1), "The copy's selection must stay on the typed character");
        }
    }

    private sealed class TextModel : INotifyPropertyChanged
    {
        private string text;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Text
        {
            get => this.text;
            set
            {
                if (this.text == value)
                {
                    return;
                }

                this.text = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Text)));
            }
        }
    }
}
