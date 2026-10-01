namespace Fluent.Tests.Controls;

using System.Collections.ObjectModel;
using System.Linq;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class StatusBarTests
{
    [Test]
    public void Moving_Item_In_ItemsSource_Should_Move_Context_Menu_Item_Without_Duplicating_It()
    {
        var items = new ObservableCollection<string> { "A", "B", "C" };
        var statusBar = new StatusBar
        {
            ItemsSource = items
        };

        using (new TestRibbonWindow(statusBar))
        {
            // Containers must be generated, otherwise StatusBar ignores collection changes and waits for the generator.
            UIHelper.DoEvents();

            var contextMenu = statusBar.ContextMenu;

            Assert.That(contextMenu, Is.Not.Null, "StatusBar should provide a context menu.");
            Assert.That(contextMenu.Items[0], Is.InstanceOf<GroupSeparatorMenuItem>(), "First context menu item should be the header.");
            Assert.That(GetMenuItemsOrder(statusBar), Is.EqualTo(new[] { "A", "B", "C" }), "Precondition: context menu should mirror the initial items.");

            items.Move(0, 2);

            // Assert directly after the move: this is the state the Move branch of OnItemsChanged produces.
            // A later, dispatcher-queued menu rebuild (triggered by container regeneration) could hide the problem.
            Assert.That(contextMenu.Items.Count, Is.EqualTo(1 + items.Count), "Header plus one menu item per status bar item.");
            Assert.That(GetMenuItemsOrder(statusBar), Is.EqualTo(new[] { "B", "C", "A" }));
        }
    }

    // Maps every menu item (except the header) back to the data item of the status bar item it represents.
    private static object[] GetMenuItemsOrder(StatusBar statusBar)
    {
        return statusBar.ContextMenu.Items
            .Cast<object>()
            .Skip(1)
            .Select(x => x is StatusBarMenuItem menuItem
                ? statusBar.ItemContainerGenerator.ItemFromContainer(menuItem.StatusBarItem)
                : x)
            .ToArray();
    }
}
