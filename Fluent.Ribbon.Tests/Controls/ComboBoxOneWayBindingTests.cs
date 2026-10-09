namespace Fluent.Tests.Controls;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Apps often show the current choice from the view model in a combo box with a OneWay binding on SelectedItem.
/// Writing SelectedItem with SetValue (or the CLR setter) from inside the control replaces that binding with a local value,
/// so after the user opens the Quick Access Toolbar copy (or moves through the items of an editable combo box)
/// the combo box stops following the view model.
/// </summary>
[TestFixture]
public class ComboBoxOneWayBindingTests
{
    [Test]
    public void QuickAccessCopy_DropDown_Should_Keep_OneWay_SelectedItem_Binding()
    {
        var model = new SelectionModel { Selected = "A" };
        var comboBox = CreateComboBox(model, BindingMode.OneWay, isEditable: false);
        var quickAccessCopy = (Fluent.ComboBox)comboBox.CreateQuickAccessItem();

        using (new TestRibbonWindow(new StackPanel { Children = { comboBox, quickAccessCopy } }))
        {
            UIHelper.DoEvents();

            // Opening the copy moves the items (and the selection) from the original to the copy ("freeze").
            quickAccessCopy.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(quickAccessCopy.Items.Count, Is.EqualTo(3), "Precondition: opening the copy should move the items to it.");
            Assert.That(quickAccessCopy.SelectedItem, Is.EqualTo("A"), "Precondition: opening the copy should move the selection to it.");

            // Closing it moves them back ("unfreeze").
            quickAccessCopy.IsDropDownOpen = false;
            UIHelper.DoEvents();

            Assert.That(comboBox.Items.Count, Is.EqualTo(3), "Precondition: closing the copy should move the items back.");

            AssertComboBoxFollowsModel(comboBox, model);
        }
    }

    [Test]
    public void QuickAccessCopy_Focus_Should_Keep_OneWay_SelectedItem_Binding()
    {
        var model = new SelectionModel { Selected = "A" };
        var comboBox = CreateComboBox(model, BindingMode.OneWay, isEditable: true);
        var quickAccessCopy = (Fluent.ComboBox)comboBox.CreateQuickAccessItem();

        using (new TestRibbonWindow(new StackPanel { Children = { comboBox, quickAccessCopy } }))
        {
            UIHelper.DoEvents();

            // Focusing the text box of an editable copy freezes the original, losing focus unfreezes it.
            quickAccessCopy.RaiseEvent(new RoutedEventArgs(UIElement.GotFocusEvent, quickAccessCopy));
            UIHelper.DoEvents();

            Assert.That(quickAccessCopy.Items.Count, Is.EqualTo(3), "Precondition: focusing the copy should move the items to it.");

            quickAccessCopy.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, quickAccessCopy));
            UIHelper.DoEvents();

            Assert.That(comboBox.Items.Count, Is.EqualTo(3), "Precondition: leaving the copy should move the items back.");

            AssertComboBoxFollowsModel(comboBox, model);
        }
    }

    [Test]
    public void Editable_Navigation_Should_Keep_OneWay_SelectedItem_Binding()
    {
        var model = new SelectionModel { Selected = "A" };
        var comboBox = CreateComboBox(model, BindingMode.OneWay, isEditable: true);

        using (var window = new TestRibbonWindow(comboBox))
        {
            MoveFocusToItemInOpenDropDown(window, comboBox, "B");

            Assert.That(comboBox.SelectedItem, Is.EqualTo("B"), "Precondition: moving through the items of an editable combo box selects them.");

            AssertComboBoxFollowsModel(comboBox, model);
        }
    }

    [Test]
    public void Editable_Navigation_Should_Still_Update_TwoWay_Source()
    {
        var model = new SelectionModel { Selected = "A" };
        var comboBox = CreateComboBox(model, BindingMode.TwoWay, isEditable: true);

        using (var window = new TestRibbonWindow(comboBox))
        {
            MoveFocusToItemInOpenDropDown(window, comboBox, "B");

            Assert.That(model.Selected, Is.EqualTo("B"), "The view model should receive the item the user moved to.");
        }
    }

    private static Fluent.ComboBox CreateComboBox(SelectionModel model, BindingMode mode, bool isEditable)
    {
        var comboBox = new Fluent.ComboBox
        {
            IsEditable = isEditable,
            Items = { "A", "B", "C" }
        };

        comboBox.SetBinding(Selector.SelectedItemProperty, new Binding(nameof(SelectionModel.Selected)) { Source = model, Mode = mode });

        Assert.That(comboBox.SelectedItem, Is.EqualTo(model.Selected), "Precondition: SelectedItem should come from the bound source.");

        return comboBox;
    }

    private static void MoveFocusToItemInOpenDropDown(Window window, Fluent.ComboBox comboBox, string item)
    {
        window.Activate();
        UIHelper.DoEvents();

        comboBox.IsDropDownOpen = true;
        UIHelper.DoEvents();

        var container = (IInputElement)comboBox.ItemContainerGenerator.ContainerFromItem(item);

        Assert.That(container, Is.Not.Null, "Precondition: the item container must exist while the drop down is open.");

        // This is what the arrow keys do while the list is open.
        Keyboard.Focus(container);
        UIHelper.DoEvents();

        UIHelper.InconclusiveIfKeyboardFocusLost("moving focus to an item");
        Assert.That(Keyboard.FocusedElement, Is.SameAs(container), "Precondition: the item container should have keyboard focus.");
    }

    private static void AssertComboBoxFollowsModel(Fluent.ComboBox comboBox, SelectionModel model)
    {
        // SetValue would have replaced the binding with a local value; SetCurrentValue keeps it.
        Assert.That(BindingOperations.GetBindingExpression(comboBox, Selector.SelectedItemProperty), Is.Not.Null, "OneWay binding on SelectedItem was lost.");

        model.Selected = "C";

        Assert.That(comboBox.SelectedItem, Is.EqualTo("C"), "Combo box should follow the view model again.");
    }

    private sealed class SelectionModel : INotifyPropertyChanged
    {
        private string selected;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Selected
        {
            get => this.selected;
            set
            {
                if (this.selected == value)
                {
                    return;
                }

                this.selected = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Selected)));
            }
        }
    }
}
