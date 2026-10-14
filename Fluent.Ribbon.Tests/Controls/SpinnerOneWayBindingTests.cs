namespace Fluent.Tests.Controls;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Apps often show a value from the view model in a spinner with a OneWay binding and handle ValueChanged themselves.
/// Writing Value with SetValue (or the CLR setter) from inside the control replaces that binding with a local value,
/// so after one click on the up button (or one typed value) the spinner stops following the view model.
/// </summary>
[TestFixture]
public class SpinnerOneWayBindingTests
{
    [Test]
    public void ButtonUp_Should_Keep_OneWay_Binding()
    {
        var model = new SpinnerModel { Zoom = 5 };
        var spinner = CreateSpinner(model, BindingMode.OneWay);

        using (new TestRibbonWindow(spinner))
        {
            UIHelper.DoEvents();

            ClickButton(spinner, "PART_ButtonUp");

            Assert.That(spinner.Value, Is.EqualTo(6), "Precondition: the up button should increment the value.");

            AssertSpinnerFollowsModel(spinner, model);
        }
    }

    [Test]
    public void ButtonDown_Should_Keep_OneWay_Binding()
    {
        var model = new SpinnerModel { Zoom = 5 };
        var spinner = CreateSpinner(model, BindingMode.OneWay);

        using (new TestRibbonWindow(spinner))
        {
            UIHelper.DoEvents();

            ClickButton(spinner, "PART_ButtonDown");

            Assert.That(spinner.Value, Is.EqualTo(4), "Precondition: the down button should decrement the value.");

            AssertSpinnerFollowsModel(spinner, model);
        }
    }

    [Test]
    public void Typed_Text_Should_Keep_OneWay_Binding()
    {
        var model = new SpinnerModel { Zoom = 5 };
        var spinner = CreateSpinner(model, BindingMode.OneWay);

        using (new TestRibbonWindow(spinner))
        {
            UIHelper.DoEvents();

            TypeText(spinner, "7");

            Assert.That(spinner.Value, Is.EqualTo(7), "Precondition: the typed text should be committed to Value.");

            AssertSpinnerFollowsModel(spinner, model);
        }
    }

    [Test]
    public void Maximum_Change_Should_Keep_OneWay_Binding()
    {
        var model = new SpinnerModel { Zoom = 5 };
        var spinner = CreateSpinner(model, BindingMode.OneWay);

        spinner.Maximum = 3;

        Assert.That(spinner.Value, Is.EqualTo(3), "Precondition: Value should be limited to the new Maximum.");

        AssertSpinnerFollowsModel(spinner, model);
    }

    [Test]
    public void Minimum_Change_Should_Keep_OneWay_Binding()
    {
        var model = new SpinnerModel { Zoom = 5 };
        var spinner = CreateSpinner(model, BindingMode.OneWay);

        spinner.Minimum = 8;

        Assert.That(spinner.Value, Is.EqualTo(8), "Precondition: Value should be limited to the new Minimum.");

        // 9 is inside the new range, so the spinner should show it.
        model.Zoom = 9;

        Assert.That(BindingOperations.GetBindingExpression(spinner, Spinner.ValueProperty), Is.Not.Null, "OneWay binding on Value was lost.");
        Assert.That(spinner.Value, Is.EqualTo(9), "Spinner should follow the view model again.");
    }

    [Test]
    public void ButtonUp_Should_Still_Update_TwoWay_Source()
    {
        var model = new SpinnerModel { Zoom = 5 };
        var spinner = CreateSpinner(model, BindingMode.TwoWay);

        using (new TestRibbonWindow(spinner))
        {
            UIHelper.DoEvents();

            ClickButton(spinner, "PART_ButtonUp");

            Assert.That(model.Zoom, Is.EqualTo(6), "The view model should receive the value from the up button.");

            model.Zoom = 2;
            Assert.That(spinner.Value, Is.EqualTo(2), "Spinner should follow the view model.");
        }
    }

    [Test]
    public void Typed_Text_Should_Still_Update_TwoWay_Source()
    {
        var model = new SpinnerModel { Zoom = 5 };
        var spinner = CreateSpinner(model, BindingMode.TwoWay);

        using (new TestRibbonWindow(spinner))
        {
            UIHelper.DoEvents();

            TypeText(spinner, "7");

            Assert.That(model.Zoom, Is.EqualTo(7), "The view model should receive the typed value.");
        }
    }

    [Test]
    public void Maximum_Change_Should_Still_Update_TwoWay_Source()
    {
        var model = new SpinnerModel { Zoom = 5 };
        var spinner = CreateSpinner(model, BindingMode.TwoWay);

        spinner.Maximum = 3;

        Assert.That(model.Zoom, Is.EqualTo(3), "The view model should receive the limited value.");
    }

    private static Spinner CreateSpinner(SpinnerModel model, BindingMode mode)
    {
        var spinner = new Spinner
        {
            Minimum = 0,
            Maximum = 100,
            Increment = 1
        };

        spinner.SetBinding(Spinner.ValueProperty, new Binding(nameof(SpinnerModel.Zoom)) { Source = model, Mode = mode });

        Assert.That(spinner.Value, Is.EqualTo(model.Zoom), "Precondition: Value should come from the bound source.");

        return spinner;
    }

    private static void ClickButton(Spinner spinner, string partName)
    {
        var button = (RepeatButton)spinner.Template.FindName(partName, spinner);

        Assert.That(button, Is.Not.Null, $"Precondition: {partName} must exist in the template.");

        // This is what a mouse click (and the Up/Down keys in the text box) raises on the button.
        button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }

    private static void TypeText(Spinner spinner, string text)
    {
        var textBox = (System.Windows.Controls.TextBox)spinner.Template.FindName("PART_TextBox", spinner);

        Assert.That(textBox, Is.Not.Null, "Precondition: PART_TextBox must exist in the template.");

        textBox.Text = text;

        // Leaving the text box commits the typed text.
        textBox.RaiseEvent(new KeyboardFocusChangedEventArgs(Keyboard.PrimaryDevice, 0, textBox, null)
        {
            RoutedEvent = Keyboard.LostKeyboardFocusEvent
        });
    }

    private static void AssertSpinnerFollowsModel(Spinner spinner, SpinnerModel model)
    {
        // SetValue would have replaced the binding with a local value; SetCurrentValue keeps it.
        Assert.That(BindingOperations.GetBindingExpression(spinner, Spinner.ValueProperty), Is.Not.Null, "OneWay binding on Value was lost.");

        model.Zoom = 2;

        Assert.That(spinner.Value, Is.EqualTo(2), "Spinner should follow the view model again.");
    }

    private sealed class SpinnerModel : INotifyPropertyChanged
    {
        private double zoom;

        public event PropertyChangedEventHandler PropertyChanged;

        public double Zoom
        {
            get => this.zoom;
            set
            {
                if (this.zoom.Equals(value))
                {
                    return;
                }

                this.zoom = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Zoom)));
            }
        }
    }
}
