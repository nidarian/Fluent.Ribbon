namespace Fluent.Tests.Controls;

using System.Collections.Generic;
using System.Windows;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for how <see cref="DropDownButton"/> closes submenus that are still open when the drop down closes.
/// </summary>
[TestFixture]
public class DropDownButtonSubmenuTests
{
    [Test]
    public void Closing_the_drop_down_closes_a_fluent_submenu_after_a_plain_wpf_submenu_was_opened_and_closed()
    {
        // A Fluent MenuItem and a plain WPF MenuItem, both with a submenu.
        var fluentItem = new MenuItem { Header = "Fluent", Items = { new MenuItem { Header = "Fluent child" } } };
        var wpfItem = new System.Windows.Controls.MenuItem { Header = "WPF", Items = { new System.Windows.Controls.MenuItem { Header = "WPF child" } } };

        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            Items = { fluentItem, wpfItem }
        };

        // Records the OriginalSource of every SubmenuOpened/SubmenuClosed event that reaches the drop down button,
        // so we can check that the events really arrive there (that's what the drop down button reacts to).
        var opened = new List<object>();
        var closed = new List<object>();
        dropDownButton.AddHandler(System.Windows.Controls.MenuItem.SubmenuOpenedEvent, new RoutedEventHandler((_, e) => opened.Add(e.OriginalSource)), true);
        dropDownButton.AddHandler(System.Windows.Controls.MenuItem.SubmenuClosedEvent, new RoutedEventHandler((_, e) => closed.Add(e.OriginalSource)), true);

        using (new TestRibbonWindow(dropDownButton))
        {
            dropDownButton.ApplyTemplate();

            dropDownButton.IsDropDownOpen = true;
            UIHelper.DoEvents();

            // WPF only lets a MenuItem open its submenu once it is loaded, which needs the drop down's popup to be shown.
            fluentItem.IsSubmenuOpen = true;
            UIHelper.DoEvents();

            if (fluentItem.IsSubmenuOpen == false)
            {
                Assert.Inconclusive("Submenus can't be opened in this test environment (the menu items weren't loaded).");
            }

            Assert.That(opened, Does.Contain(fluentItem), "Precondition: the drop down button should see the Fluent submenu open");

            // Open and close the plain WPF submenu while the Fluent one stays open.
            wpfItem.IsSubmenuOpen = true;
            UIHelper.DoEvents();
            wpfItem.IsSubmenuOpen = false;
            UIHelper.DoEvents();

            Assert.That(opened, Does.Contain(wpfItem), "Precondition: the drop down button should see the WPF submenu open");
            Assert.That(closed, Does.Contain(wpfItem), "Precondition: the drop down button should see the WPF submenu close");
            Assert.That(fluentItem.IsSubmenuOpen, Is.True, "Precondition: the Fluent submenu should still be open");

            // Closing the drop down must also close every submenu that is still open,
            // otherwise the Fluent submenu's popup stays on screen without its drop down.
            dropDownButton.IsDropDownOpen = false;
            UIHelper.DoEvents();

            Assert.That(fluentItem.IsSubmenuOpen, Is.False, "The Fluent submenu should be closed together with the drop down");
        }
    }
}
