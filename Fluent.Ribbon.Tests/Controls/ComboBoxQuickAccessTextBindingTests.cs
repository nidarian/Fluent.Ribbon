namespace Fluent.Tests.Controls;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Apps often show a text from the view model in an editable combo box with a OneWay binding on Text.
/// The Quick Access Toolbar copy of the combo box is kept in sync with the original.
/// Typing in the copy, or opening and closing its drop down, must not replace that binding with a local value.
/// Otherwise the original (and with it the copy) stops following the view model.
/// </summary>
[TestFixture]
public class ComboBoxQuickAccessTextBindingTests
{
    [Test]
    public void QuickAccessCopy_Typing_Should_Keep_OneWay_Text_Binding()
    {
        var model = new TextModel { Text = "Initial" };
        var comboBox = CreateComboBox(model, BindingMode.OneWay);
        var quickAccessCopy = (Fluent.ComboBox)comboBox.CreateQuickAccessItem();

        using (new TestRibbonWindow(new StackPanel { Children = { comboBox, quickAccessCopy } }))
        {
            UIHelper.DoEvents();

            Assert.That(quickAccessCopy.Text, Is.EqualTo("Initial"), "Precondition: the copy should show the text of the original.");

            TypeInQuickAccessCopy(quickAccessCopy, "Typed");

            Assert.That(comboBox.Text, Is.EqualTo("Typed"), "The text typed in the copy should reach the original.");

            AssertFollowsModel(comboBox, quickAccessCopy, model);
        }
    }

    [Test]
    public void QuickAccessCopy_DropDown_Should_Keep_OneWay_Text_Binding()
    {
        var model = new TextModel { Text = "Initial" };
        var comboBox = CreateComboBox(model, BindingMode.OneWay);
        var quickAccessCopy = (Fluent.ComboBox)comboBox.CreateQuickAccessItem();

        using (new TestRibbonWindow(new StackPanel { Children = { comboBox, quickAccessCopy } }))
        {
            UIHelper.DoEvents();

            // Opening the copy moves the items from the original to the copy ("freeze").
            quickAccessCopy.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(quickAccessCopy.Items.Count, Is.EqualTo(3), "Precondition: opening the copy should move the items to it.");

            // Closing it moves them back and copies the text of the copy to the original ("unfreeze").
            quickAccessCopy.IsDropDownOpen = false;
            UIHelper.DoEvents();

            Assert.That(comboBox.Items.Count, Is.EqualTo(3), "Precondition: closing the copy should move the items back.");
            Assert.That(comboBox.Text, Is.EqualTo("Initial"), "Precondition: the original should keep its text.");

            AssertFollowsModel(comboBox, quickAccessCopy, model);
        }
    }

    [Test]
    public void QuickAccessCopy_Typing_Should_Still_Update_TwoWay_Source()
    {
        var model = new TextModel { Text = "Initial" };
        var comboBox = CreateComboBox(model, BindingMode.TwoWay);
        var quickAccessCopy = (Fluent.ComboBox)comboBox.CreateQuickAccessItem();

        using (new TestRibbonWindow(new StackPanel { Children = { comboBox, quickAccessCopy } }))
        {
            UIHelper.DoEvents();

            TypeInQuickAccessCopy(quickAccessCopy, "Typed");

            Assert.That(comboBox.Text, Is.EqualTo("Typed"), "The text typed in the copy should reach the original.");
            Assert.That(model.Text, Is.EqualTo("Typed"), "The view model should receive the text typed in the copy.");

            model.Text = "FromModel";
            UIHelper.DoEvents();

            Assert.That(comboBox.Text, Is.EqualTo("FromModel"), "The original should follow the view model.");
            Assert.That(quickAccessCopy.Text, Is.EqualTo("FromModel"), "The copy should follow the original.");
        }
    }

    private static Fluent.ComboBox CreateComboBox(TextModel model, BindingMode mode)
    {
        var comboBox = new Fluent.ComboBox
        {
            IsEditable = true,
            Items = { "A", "B", "C" }
        };

        comboBox.SetBinding(System.Windows.Controls.ComboBox.TextProperty, new Binding(nameof(TextModel.Text)) { Source = model, Mode = mode });

        Assert.That(comboBox.Text, Is.EqualTo(model.Text), "Precondition: Text should come from the bound source.");

        return comboBox;
    }

    /// <summary>
    /// Does what a user typing in the copy does: the copy gets focus (which "freezes" the original),
    /// the text of its text box changes (which the combo box turns into a new Text), and then it loses focus ("unfreeze").
    /// </summary>
    private static void TypeInQuickAccessCopy(Fluent.ComboBox quickAccessCopy, string text)
    {
        quickAccessCopy.RaiseEvent(new RoutedEventArgs(UIElement.GotFocusEvent, quickAccessCopy));
        UIHelper.DoEvents();

        var textBox = quickAccessCopy.Template.FindName("PART_EditableTextBox", quickAccessCopy) as TextBox;

        Assert.That(textBox, Is.Not.Null, "Precondition: the copy should have an editable text box.");

        // A key press changes the text of the text box, the combo box listens to TextChanged and sets its Text from it.
        textBox.Text = text;
        UIHelper.DoEvents();

        Assert.That(quickAccessCopy.Text, Is.EqualTo(text), "Precondition: the copy should take the text of its text box.");

        quickAccessCopy.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, quickAccessCopy));
        UIHelper.DoEvents();
    }

    private static void AssertFollowsModel(Fluent.ComboBox comboBox, Fluent.ComboBox quickAccessCopy, TextModel model)
    {
        // SetValue (also from a TwoWay binding writing to its source) would have replaced the binding with a local value; SetCurrentValue keeps it.
        Assert.That(BindingOperations.GetBindingExpression(comboBox, System.Windows.Controls.ComboBox.TextProperty), Is.Not.Null, "OneWay binding on Text was lost.");

        model.Text = "FromModel";
        UIHelper.DoEvents();

        Assert.That(comboBox.Text, Is.EqualTo("FromModel"), "The original should follow the view model again.");
        Assert.That(quickAccessCopy.Text, Is.EqualTo("FromModel"), "The copy should show the text of the view model.");
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
