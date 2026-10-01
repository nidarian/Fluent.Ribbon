namespace Fluent.Tests.Controls;

using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class QuickAccessToolBarTests
{
    [Test]
    public void TestDefaultKeyTips()
    {
        var toolbar = new QuickAccessToolBar();

        for (var i = 0; i < 30; i++)
        {
            toolbar.Items.Add(new UIElement());
        }

        TestDefaultKeyTips(toolbar);
    }

    private static void TestDefaultKeyTips(QuickAccessToolBar toolbar)
    {
        var keyTips = toolbar.Items.Select(KeyTip.GetKeys);
        var expectedKeyTips = new[]
        {
            "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "09", "08", "07", "06", "05", "04", "03", "02", "01",
            "0A", "0B", "0C", "0D", "0E", "0F", "0G", "0H", "0I", "0J", "0K", "0L"
        };

        Assert.That(keyTips, Is.EquivalentTo(expectedKeyTips));
    }

    [Test]
    public void TestCustomKeyTips()
    {
        var toolbar = new QuickAccessToolBar
        {
            UpdateKeyTipsAction = quickAccessToolBar =>
            {
                for (var i = 0; i < quickAccessToolBar.Items.Count; i++)
                {
                    KeyTip.SetKeys(quickAccessToolBar.Items[i], (i + 1).ToString("00", CultureInfo.InvariantCulture));
                }
            }
        };

        for (var i = 0; i < 30; i++)
        {
            toolbar.Items.Add(new UIElement());
        }

        var keyTips = toolbar.Items.Select(KeyTip.GetKeys);
        var expectedKeyTips = new[]
        {
            "01", "02", "03", "04", "05", "06", "07", "08", "09",
            "10", "11", "12", "13", "14", "15", "16", "17", "18",
            "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"
        };

        Assert.That(keyTips, Is.EquivalentTo(expectedKeyTips));

        toolbar.UpdateKeyTipsAction = null;

        TestDefaultKeyTips(toolbar);
    }

    /// <summary>
    /// QuickAccessItems are the entries of the toolbar's customize menu (the small arrow).
    /// Adding, removing and replacing them was mirrored into that menu, but Clear() raises a
    /// "Reset" change, which the handler ignored. The cleared entries stayed in the menu.
    /// </summary>
    [Test]
    public void Clearing_QuickAccessItems_removes_them_from_the_customize_menu()
    {
        var toolbar = new QuickAccessToolBar();

        using (new TestRibbonWindow(toolbar))
        {
            toolbar.ApplyTemplate();

            var first = new QuickAccessMenuItem { Header = "First" };
            var second = new QuickAccessMenuItem { Header = "Second" };
            toolbar.QuickAccessItems.Add(first);
            toolbar.QuickAccessItems.Add(second);

            var menu = toolbar.MenuDownButton;
            Assert.That(menu, Is.Not.Null, "Precondition: the template has the customize menu");
            Assert.That(menu.Items.OfType<QuickAccessMenuItem>(), Is.EqualTo(new[] { first, second }), "Precondition: added entries appear in the menu");

            toolbar.QuickAccessItems.Clear();

            Assert.That(menu.Items.OfType<QuickAccessMenuItem>(), Is.Empty, "Cleared entries must leave the menu");

            // Adding after a clear works as before: the entry goes right after the menu header.
            var third = new QuickAccessMenuItem { Header = "Third" };
            toolbar.QuickAccessItems.Add(third);

            Assert.That(menu.Items.OfType<QuickAccessMenuItem>(), Is.EqualTo(new[] { third }));
            Assert.That(menu.Items.IndexOf(third), Is.EqualTo(1));
        }
    }

    /// <summary>
    /// Clearing the items raises a Reset that doesn't say which items were removed.
    /// The toolbar must still unsubscribe from their SizeChanged, otherwise removed items keep it alive and keep invalidating its title bar.
    /// </summary>
    [Test]
    public void Clearing_items_unsubscribes_from_their_SizeChanged()
    {
        var toolbar = new QuickAccessToolBar();
        var item = new Button();

        toolbar.Items.Add(item);

        Assert.That(GetSizeChangedHandlersOf(item, toolbar), Is.EqualTo(1), "Precondition: the toolbar must listen to SizeChanged of its item.");

        toolbar.Items.Clear();

        Assert.That(GetSizeChangedHandlersOf(item, toolbar), Is.EqualTo(0), "The toolbar must stop listening to SizeChanged of a cleared item.");
    }

    /// <summary>
    /// Counts the <see cref="FrameworkElement.SizeChangedEvent"/> handlers on <paramref name="element"/> that belong to <paramref name="handlerOwner"/>.
    /// WPF has no public API for that, so this reads the internal UIElement.EventHandlersStore (present in .NET Framework and .NET).
    /// </summary>
    private static int GetSizeChangedHandlersOf(UIElement element, object handlerOwner)
    {
        var storeProperty = typeof(UIElement).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(storeProperty, Is.Not.Null, "Precondition: UIElement.EventHandlersStore must exist.");

        var store = storeProperty.GetValue(element, null);
        if (store is null)
        {
            return 0;
        }

        var getHandlers = store.GetType().GetMethod("GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.That(getHandlers, Is.Not.Null, "Precondition: EventHandlersStore.GetRoutedEventHandlers must exist.");

        var handlers = (RoutedEventHandlerInfo[])getHandlers.Invoke(store, new object[] { FrameworkElement.SizeChangedEvent });

        return handlers?.Count(x => ReferenceEquals(x.Handler.Target, handlerOwner)) ?? 0;
    }
}
