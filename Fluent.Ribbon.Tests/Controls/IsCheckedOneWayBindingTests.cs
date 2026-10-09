namespace Fluent.Tests.Controls;

using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;
using ButtonBase = System.Windows.Controls.Primitives.ButtonBase;

/// <summary>
/// Apps commonly show a state with a OneWay binding on IsChecked and change it through a command,
/// for example <c>IsChecked="{Binding IsBold, Mode=OneWay}" Command="{Binding ToggleBoldCommand}"</c>.
/// Clicking the inner button of a checkable <see cref="SplitButton"/>, or a Quick Access Toolbar copy of a control,
/// must not replace that binding with a local value. Otherwise the control no longer follows the view model after one click.
/// </summary>
[TestFixture]
public class IsCheckedOneWayBindingTests
{
    public enum Scenario
    {
        ToggleButtonCopy,
        CheckBoxCopy,
        RadioButtonCopy,
        MenuItemCopy,
        SplitButtonInnerButton,
        SplitButtonCopy,
        SplitMenuItemCopy
    }

    [TestCase(Scenario.ToggleButtonCopy)]
    [TestCase(Scenario.CheckBoxCopy)]
    [TestCase(Scenario.RadioButtonCopy)]
    [TestCase(Scenario.MenuItemCopy)]
    [TestCase(Scenario.SplitButtonInnerButton)]
    [TestCase(Scenario.SplitButtonCopy)]
    [TestCase(Scenario.SplitMenuItemCopy)]
    public void Click_Should_Keep_OneWay_IsChecked_Binding(Scenario scenario)
    {
        var state = new CheckState();

        using (var setup = Setup.Create(scenario))
        {
            setup.Original.SetBinding(setup.IsCheckedProperty, new Binding(nameof(CheckState.IsBold)) { Source = state, Mode = BindingMode.OneWay });
            UIHelper.DoEvents();

            Assert.That(setup.Original.GetValue(setup.IsCheckedProperty), Is.EqualTo(false), "Precondition: the original is unchecked.");

            setup.Click();
            UIHelper.DoEvents();

            Assert.That(setup.Original.GetValue(setup.IsCheckedProperty), Is.EqualTo(true), "Clicking should check the original.");

            // SetValue would have replaced the binding with a local value; SetCurrentValue keeps it.
            Assert.That(BindingOperations.IsDataBound(setup.Original, setup.IsCheckedProperty), Is.True, "OneWay binding on IsChecked was lost after a click.");

            // The view model never got the click (OneWay), so change it twice to get a notification through.
            state.IsBold = true;
            state.IsBold = false;
            UIHelper.DoEvents();

            Assert.That(setup.Original.GetValue(setup.IsCheckedProperty), Is.EqualTo(false), "The view model should be able to uncheck the original after a click.");
            Assert.That(setup.Clicked.GetValue(setup.ClickedIsCheckedProperty), Is.EqualTo(false), "The clicked element should follow the original.");
        }
    }

    [TestCase(Scenario.ToggleButtonCopy)]
    [TestCase(Scenario.CheckBoxCopy)]
    [TestCase(Scenario.RadioButtonCopy)]
    [TestCase(Scenario.MenuItemCopy)]
    [TestCase(Scenario.SplitButtonInnerButton)]
    [TestCase(Scenario.SplitButtonCopy)]
    [TestCase(Scenario.SplitMenuItemCopy)]
    public void Click_Should_Write_TwoWay_IsChecked_Binding(Scenario scenario)
    {
        var state = new CheckState();

        using (var setup = Setup.Create(scenario))
        {
            setup.Original.SetBinding(setup.IsCheckedProperty, new Binding(nameof(CheckState.IsBold)) { Source = state, Mode = BindingMode.TwoWay });
            UIHelper.DoEvents();

            setup.Click();
            UIHelper.DoEvents();

            Assert.That(state.IsBold, Is.True, "Clicking should write the TwoWay bound view model.");
            Assert.That(BindingOperations.IsDataBound(setup.Original, setup.IsCheckedProperty), Is.True, "TwoWay binding on IsChecked was lost after a click.");

            state.IsBold = false;
            UIHelper.DoEvents();

            Assert.That(setup.Original.GetValue(setup.IsCheckedProperty), Is.EqualTo(false), "The view model should be able to uncheck the original.");
            Assert.That(setup.Clicked.GetValue(setup.ClickedIsCheckedProperty), Is.EqualTo(false), "The clicked element should follow the original.");
        }
    }

    /// <summary>
    /// Runs the real click handler of a button (the method a mouse click, the space key or a key tip ends in).
    /// </summary>
    private static void ClickButton(ButtonBase button)
    {
        typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(button, null);
    }

    private sealed class Setup : IDisposable
    {
        private TestRibbonWindow window;

        public FrameworkElement Original { get; private set; }

        public DependencyProperty IsCheckedProperty { get; private set; }

        public DependencyObject Clicked { get; private set; }

        public DependencyProperty ClickedIsCheckedProperty { get; private set; }

        public Action Click { get; private set; }

        public static Setup Create(Scenario scenario)
        {
            var setup = new Setup();

            switch (scenario)
            {
                case Scenario.ToggleButtonCopy:
                    setup.UseButtonCopy(new ToggleButton { Header = "Test" });
                    break;

                case Scenario.CheckBoxCopy:
                    setup.UseButtonCopy(new CheckBox { Header = "Test" });
                    break;

                case Scenario.RadioButtonCopy:
                    setup.UseButtonCopy(new RadioButton { Header = "Test" });
                    break;

                case Scenario.MenuItemCopy:
                {
                    var menuItem = new MenuItem { Header = "Test", IsCheckable = true };
                    var copy = (ToggleButton)menuItem.CreateQuickAccessItem();

                    setup.Original = menuItem;
                    setup.IsCheckedProperty = System.Windows.Controls.MenuItem.IsCheckedProperty;
                    setup.Clicked = copy;
                    setup.ClickedIsCheckedProperty = System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty;
                    setup.Click = () => ClickButton(copy);
                    break;
                }

                case Scenario.SplitButtonInnerButton:
                {
                    var splitButton = new SplitButton { Header = "Test", IsCheckable = true };
                    setup.window = new TestRibbonWindow(splitButton);
                    UIHelper.DoEvents();

                    Assert.That(splitButton.Button, Is.Not.Null, "Precondition: the split button must have its inner button.");

                    setup.Original = splitButton;
                    setup.IsCheckedProperty = SplitButton.IsCheckedProperty;
                    setup.Clicked = splitButton.Button;
                    setup.ClickedIsCheckedProperty = System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty;
                    setup.Click = () => ClickButton(splitButton.Button);
                    break;
                }

                case Scenario.SplitButtonCopy:
                {
                    var splitButton = new SplitButton { Header = "Test", IsCheckable = true };
                    splitButton.Items.Add(new MenuItem { Header = "Item" });
                    setup.UseSplitButtonCopy(splitButton, SplitButton.IsCheckedProperty, (SplitButton)splitButton.CreateQuickAccessItem());
                    break;
                }

                case Scenario.SplitMenuItemCopy:
                {
                    var menuItem = new MenuItem { Header = "Test", IsCheckable = true, IsSplit = true };
                    menuItem.Items.Add(new MenuItem { Header = "Item" });
                    setup.UseSplitButtonCopy(menuItem, System.Windows.Controls.MenuItem.IsCheckedProperty, (SplitButton)menuItem.CreateQuickAccessItem());
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
            }

            return setup;
        }

        public void Dispose()
        {
            this.window?.Dispose();
        }

        private void UseButtonCopy(System.Windows.Controls.Primitives.ToggleButton original)
        {
            var copy = (System.Windows.Controls.Primitives.ToggleButton)((IQuickAccessItemProvider)original).CreateQuickAccessItem();

            this.Original = original;
            this.IsCheckedProperty = System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty;
            this.Clicked = copy;
            this.ClickedIsCheckedProperty = System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty;
            this.Click = () => ClickButton(copy);
        }

        private void UseSplitButtonCopy(FrameworkElement original, DependencyProperty isCheckedProperty, SplitButton copy)
        {
            // The copy needs its template (and with it its inner button), so it has to be shown.
            this.window = new TestRibbonWindow(copy);
            UIHelper.DoEvents();

            Assert.That(copy.Button, Is.Not.Null, "Precondition: the split button copy must have its inner button.");

            this.Original = original;
            this.IsCheckedProperty = isCheckedProperty;
            this.Clicked = copy;
            this.ClickedIsCheckedProperty = SplitButton.IsCheckedProperty;
            this.Click = () => ClickButton(copy.Button);
        }
    }

    private sealed class CheckState : INotifyPropertyChanged
    {
        private bool isBold;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsBold
        {
            get => this.isBold;
            set
            {
                if (this.isBold == value)
                {
                    return;
                }

                this.isBold = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsBold)));
            }
        }
    }
}
