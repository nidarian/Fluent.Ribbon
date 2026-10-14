namespace Fluent.Tests.Controls;

using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Apps can turn ScreenTips off with <see cref="System.Windows.Controls.ToolTipService.IsEnabledProperty"/>
/// (a style setter, a local value or a binding to a setting like Office's "Don't show ScreenTips").
/// Drop downs hide their own tooltip while they are open, but they used to do that by ignoring the app's value
/// (the coerce callback always returned "not open") and by overwriting it with SetValue on every open/close.
/// So DropDownButton, SplitButton and ApplicationMenu always showed their ScreenTip, and a collapsed
/// RibbonGroupBox showed its tooltip again after its drop down was opened once.
/// </summary>
[TestFixture]
public class ToolTipIsEnabledTests
{
    [Test]
    public void DropDownButton_keeps_tooltips_disabled_by_the_app()
    {
        var dropDownButton = new DropDownButton();

        System.Windows.Controls.ToolTipService.SetIsEnabled(dropDownButton, false);

        Assert.That(System.Windows.Controls.ToolTipService.GetIsEnabled(dropDownButton), Is.False);
    }

    [Test]
    public void SplitButton_keeps_tooltips_disabled_by_the_app()
    {
        var splitButton = new SplitButton();

        System.Windows.Controls.ToolTipService.SetIsEnabled(splitButton, false);

        Assert.That(System.Windows.Controls.ToolTipService.GetIsEnabled(splitButton), Is.False);
    }

    [Test]
    public void DropDownButton_keeps_tooltip_binding_after_opening_and_closing()
    {
        // Stands in for an app setting like "Show ScreenTips".
        var showScreenTips = new System.Windows.Controls.CheckBox { IsChecked = false };

        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            Items = { new MenuItem { Header = "Item" } }
        };

        BindingOperations.SetBinding(dropDownButton, System.Windows.Controls.ToolTipService.IsEnabledProperty, new Binding(nameof(System.Windows.Controls.CheckBox.IsChecked)) { Source = showScreenTips });

        using (new TestRibbonWindow(dropDownButton))
        {
            dropDownButton.ApplyTemplate();

            dropDownButton.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(System.Windows.Controls.ToolTipService.GetIsEnabled(dropDownButton), Is.False, "No tooltip while the drop down is open");

            dropDownButton.IsDropDownOpen = false;
            UIHelper.DoEvents();

            Assert.That(BindingOperations.GetBindingExpression(dropDownButton, System.Windows.Controls.ToolTipService.IsEnabledProperty), Is.Not.Null, "The app's binding must survive opening and closing the drop down");
            Assert.That(System.Windows.Controls.ToolTipService.GetIsEnabled(dropDownButton), Is.False, "The app turned tooltips off, closing the drop down must not turn them on");

            // The binding still works: turning the setting on enables the tooltip again.
            showScreenTips.IsChecked = true;

            Assert.That(System.Windows.Controls.ToolTipService.GetIsEnabled(dropDownButton), Is.True, "Turning the setting on must enable the tooltip");
        }
    }

    [Test]
    public void DropDownButton_disables_tooltip_while_open_even_if_the_app_enables_it()
    {
        // Guards the intended behavior: the tooltip must not cover the open drop down.
        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            Items = { new MenuItem { Header = "Item" } }
        };

        System.Windows.Controls.ToolTipService.SetIsEnabled(dropDownButton, true);

        using (new TestRibbonWindow(dropDownButton))
        {
            dropDownButton.ApplyTemplate();

            Assert.That(System.Windows.Controls.ToolTipService.GetIsEnabled(dropDownButton), Is.True, "Closed: the app's value applies");

            dropDownButton.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(System.Windows.Controls.ToolTipService.GetIsEnabled(dropDownButton), Is.False, "Open: no tooltip");

            dropDownButton.IsDropDownOpen = false;
            UIHelper.DoEvents();

            Assert.That(System.Windows.Controls.ToolTipService.GetIsEnabled(dropDownButton), Is.True, "Closed again: the app's value applies again");
        }
    }

    [Test]
    public void Collapsed_RibbonGroupBox_keeps_tooltips_disabled_by_the_app_after_its_drop_down_was_opened()
    {
        var ribbonGroupBox = new RibbonGroupBox
        {
            Header = "Group",
            Items = { new Fluent.Button { Header = "Button" } }
        };

        System.Windows.Controls.ToolTipService.SetIsEnabled(ribbonGroupBox, false);

        using (new TestRibbonWindow(ribbonGroupBox))
        {
            ribbonGroupBox.State = RibbonGroupBoxState.Collapsed;
            UIHelper.DoEvents();

            ribbonGroupBox.IsDropDownOpen = true;
            UIHelper.DoEvents();
            Assert.That(ribbonGroupBox.IsDropDownOpen, Is.True, "Precondition: a collapsed group's drop down can open");
            Assert.That(System.Windows.Controls.ToolTipService.GetIsEnabled(ribbonGroupBox), Is.False, "No tooltip while the drop down is open");

            ribbonGroupBox.IsDropDownOpen = false;
            UIHelper.DoEvents();

            Assert.That(System.Windows.Controls.ToolTipService.GetIsEnabled(ribbonGroupBox), Is.False, "The app turned tooltips off, closing the drop down must not turn them on");
        }
    }
}
