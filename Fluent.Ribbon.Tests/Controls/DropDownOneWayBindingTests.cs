namespace Fluent.Tests.Controls;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Apps commonly drive a drop down from a view model with a OneWay binding on IsDropDownOpen.
/// Writing that property with SetValue (or the CLR setter) from inside the control replaces the binding with a local value,
/// so after the first user close the view model can no longer open the drop down.
/// These tests close the drop down through internal paths (key tips, popup service) and check the binding survives.
/// </summary>
[TestFixture]
public class DropDownOneWayBindingTests
{
    [Test]
    public void DropDownButton_KeyTipBack_Should_Keep_OneWay_Binding()
    {
        var control = new DropDownButton
        {
            Header = "Test"
        };
        control.Items.Add(new MenuItem { Header = "Item" });

        using (new TestRibbonWindow(control))
        {
            // OnKeyTipBack is what KeyTipService calls when the user backs out of the drop down with Esc.
            AssertOneWayBindingSurvivesInternalClose(control, DropDownButton.IsDropDownOpenProperty, () => ((IKeyTipedControl)control).OnKeyTipBack());
        }
    }

    [Test]
    public void DropDownButton_PopupService_Dismiss_Should_Keep_OneWay_Binding()
    {
        var control = new DropDownButton
        {
            Header = "Test"
        };
        control.Items.Add(new MenuItem { Header = "Item" });

        using (new TestRibbonWindow(control))
        {
            // PopupService only knows the control as IDropDownControl, so it has to write through the interface.
            AssertOneWayBindingSurvivesInternalClose(control, DropDownButton.IsDropDownOpenProperty, () => PopupService.RaiseDismissPopupEvent(control, DismissPopupMode.Always));
        }
    }

    [Test]
    public void RibbonGroupBox_Collapsed_KeyTipBack_Should_Keep_OneWay_Binding()
    {
        // Only a collapsed (or QuickAccess) group box has a drop down; in other states IsDropDownOpen is coerced to false.
        var control = new RibbonGroupBox
        {
            Header = "Test"
        };
        control.Items.Add(new Fluent.Button { Header = "Item" });

        using (new TestRibbonWindow(control))
        {
            UIHelper.DoEvents();

            // Set the state only after loading: a group box resets its state to Large when its content loads.
            control.State = RibbonGroupBoxState.Collapsed;
            UIHelper.DoEvents();

            Assert.That(control.State, Is.EqualTo(RibbonGroupBoxState.Collapsed), "Precondition: the group box must be collapsed.");

            AssertOneWayBindingSurvivesInternalClose(control, RibbonGroupBox.IsDropDownOpenProperty, () => ((IKeyTipedControl)control).OnKeyTipBack());
        }
    }

    [Test]
    public void InRibbonGallery_KeyTipBack_Should_Keep_OneWay_Binding()
    {
        var control = new InRibbonGallery
        {
            Width = 10,
            Height = 30
        };

        using (new TestRibbonWindow(control))
        {
            AssertOneWayBindingSurvivesInternalClose(control, InRibbonGallery.IsDropDownOpenProperty, () => ((IKeyTipedControl)control).OnKeyTipBack());
        }
    }

    private static void AssertOneWayBindingSurvivesInternalClose(FrameworkElement control, DependencyProperty isDropDownOpenProperty, Action closeThroughInternalPath)
    {
        var state = new DropDownState();
        control.SetBinding(isDropDownOpenProperty, new Binding(nameof(DropDownState.IsOpen)) { Source = state, Mode = BindingMode.OneWay });

        // Open through the view model, like an app would.
        state.IsOpen = true;
        Assert.That((bool)control.GetValue(isDropDownOpenProperty), Is.True, "Drop down should open from the bound source.");

        // Close through a path the user triggers (key tip back, popup dismiss, ...).
        closeThroughInternalPath();
        UIHelper.DoEvents();

        Assert.That((bool)control.GetValue(isDropDownOpenProperty), Is.False, "Internal path should close the drop down.");

        // SetValue would have replaced the binding with a local value; SetCurrentValue keeps it.
        Assert.That(BindingOperations.GetBindingExpression(control, isDropDownOpenProperty), Is.Not.Null, "OneWay binding on IsDropDownOpen was lost after an internal close.");

        // The source still says "open" (OneWay never wrote back), so toggle it to get a change notification through.
        state.IsOpen = false;
        state.IsOpen = true;
        Assert.That((bool)control.GetValue(isDropDownOpenProperty), Is.True, "Bound source should be able to reopen the drop down after an internal close.");

        state.IsOpen = false;
        Assert.That((bool)control.GetValue(isDropDownOpenProperty), Is.False, "Bound source should be able to close the drop down.");
    }

    private sealed class DropDownState : INotifyPropertyChanged
    {
        private bool isOpen;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsOpen
        {
            get => this.isOpen;
            set
            {
                if (this.isOpen == value)
                {
                    return;
                }

                this.isOpen = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsOpen)));
            }
        }
    }
}
