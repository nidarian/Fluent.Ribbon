namespace Fluent.Tests.Controls;

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;
using System.Windows.Threading;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// All ribbons of a thread share one static context menu (<see cref="Ribbon.RibbonContextMenu"/>).
/// When it opened, every menu item got the ribbon as its <see cref="System.Windows.Controls.MenuItem.CommandTarget"/>,
/// and nothing cleared that when the menu closed. So after a user right-clicked a control in a window
/// and then closed that window, the static menu kept its ribbon, and through it the whole closed window
/// (with its DataContext), alive until some other ribbon opened the menu: forever if none did.
/// </summary>
[TestFixture]
public class RibbonContextMenuLeakTests
{
    /// <summary>
    /// Control for the test method: a closed window that never opened the menu is collected.
    /// If this fails, the garbage collection check itself is unreliable here.
    /// </summary>
    [Test]
    public void Closed_window_is_collected_when_the_context_menu_was_never_opened()
    {
        var window = CreateUseAndCloseWindow(openContextMenu: false);

        Assert.That(IsAliveAfterGarbageCollection(window), Is.False, "A closed window that never opened the context menu should be collected");
    }

    [Test]
    public void Closed_window_is_collected_after_its_context_menu_was_opened_and_closed()
    {
        var window = CreateUseAndCloseWindow(openContextMenu: true);

        Assert.That(IsAliveAfterGarbageCollection(window), Is.False, "The shared ribbon context menu must not keep a closed window alive");
    }

    /// <summary>
    /// Control: once another ribbon has used the menu, nothing of the first window is left that keeps it alive.
    /// This shows that the stale state of the shared menu is the only thing holding the window.
    /// </summary>
    [Test]
    public void Closed_window_is_collected_after_the_context_menu_was_opened_on_another_ribbon()
    {
        var window = CreateUseAndCloseWindow(openContextMenu: true);

        using (CreateWindow(out _, out var otherButton))
        {
            OpenContextMenu(otherButton);
            CloseContextMenu();

            Assert.That(IsAliveAfterGarbageCollection(window), Is.False, "The first window should be collected once the menu was used by another ribbon");
        }
    }

    /// <summary>
    /// While open, the menu (inside its popup) takes over inherited values from the element it was opened on,
    /// for example the DataContext and the window it belongs to. After closing, none of that may stay.
    /// </summary>
    [Test]
    public void Closed_context_menu_no_longer_refers_to_the_window_it_was_opened_in()
    {
        using (var window = CreateWindow(out _, out var button))
        {
            var menu = Ribbon.RibbonContextMenu;

            OpenContextMenu(button);

            var windowWhileOpen = Window.GetWindow(menu);
            var dataContextWhileOpen = menu.DataContext;

            CloseContextMenu();

            var popup = LogicalTreeHelper.GetParent(menu);

            Assert.Multiple(() =>
            {
                Assert.That(windowWhileOpen, Is.SameAs(window), "While open, the menu belongs to the window");
                Assert.That(dataContextWhileOpen, Is.SameAs(window.DataContext), "While open, the menu has the window's DataContext");

                Assert.That(menu.PlacementTarget, Is.Null, "PlacementTarget after closing");
                Assert.That(GetMenuItems().Select(x => x.CommandTarget), Is.All.Null, "CommandTarget of the items after closing");
                Assert.That(GetMenuItems().Select(x => x.CommandParameter), Is.All.Null, "CommandParameter of the items after closing");
                Assert.That(Window.GetWindow(menu), Is.Null, "Window of the menu after closing");
                Assert.That(menu.DataContext, Is.Null, "DataContext of the menu after closing");
                Assert.That(GetMenuItems().Select(Window.GetWindow), Is.All.Null, "Window of the items after closing");
                Assert.That(popup is null ? null : Window.GetWindow(popup), Is.Null, "Window of the popup after closing");
            });
        }
    }

    [Test]
    public void Context_menu_commands_target_the_ribbon_it_was_opened_on()
    {
        using (CreateWindow(out var firstRibbon, out var firstButton))
        using (CreateWindow(out var secondRibbon, out var secondButton))
        {
            OpenContextMenu(firstButton);

            Assert.That(GetMenuItems().Select(x => x.CommandTarget), Is.All.SameAs(firstRibbon), "Opened on the first ribbon");
            Assert.That(GetAddToQuickAccessToolBarMenuItem().CommandParameter, Is.SameAs(firstButton), "Opened on the first button");

            CloseContextMenu();

            OpenContextMenu(secondButton);

            Assert.That(GetMenuItems().Select(x => x.CommandTarget), Is.All.SameAs(secondRibbon), "Opened on the second ribbon");
            Assert.That(GetAddToQuickAccessToolBarMenuItem().CommandParameter, Is.SameAs(secondButton), "Opened on the second button");

            CloseContextMenu();
        }
    }

    /// <summary>
    /// WPF runs a context menu item's command after it started closing the menu.
    /// Whatever is cleared when the menu closes must not be missing when the command runs.
    /// </summary>
    [Test]
    public void Clicking_add_to_quick_access_toolbar_in_the_context_menu_adds_the_control()
    {
        using (CreateWindow(out var ribbon, out var button))
        {
            OpenContextMenu(button);

            var menuItem = GetAddToQuickAccessToolBarMenuItem();
            Assert.That(menuItem.Visibility, Is.EqualTo(Visibility.Visible), "Precondition: the menu offers adding the button");

            var closed = false;
            RoutedEventHandler onClosed = (_, _) => closed = true;
            Ribbon.RibbonContextMenu.Closed += onClosed;
            try
            {
                var invoke = (IInvokeProvider)UIElementAutomationPeer.CreatePeerForElement(menuItem).GetPattern(PatternInterface.Invoke);
                invoke.Invoke();

                PumpUntil(() => closed);
            }
            finally
            {
                Ribbon.RibbonContextMenu.Closed -= onClosed;
            }

            Assert.That(closed, Is.True, "Precondition: clicking the item closes the menu");
            Assert.That(ribbon.IsInQuickAccessToolBar(button), Is.True, "The button should be on the quick access toolbar");
        }
    }

    // Creates a window with a ribbon, optionally opens and closes the ribbon context menu on a control of it,
    // closes the window and returns a weak reference to it.
    // Not inlined, so no local of the caller keeps the window alive.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateUseAndCloseWindow(bool openContextMenu)
    {
        var window = CreateWindow(out _, out var button);

        if (openContextMenu)
        {
            OpenContextMenu(button);
            CloseContextMenu();
        }

        window.Close();
        UIHelper.DoEvents();

        return new WeakReference(window);
    }

    private static TestRibbonWindow CreateWindow(out Ribbon ribbon, out Button button)
    {
        button = new Button { Header = "Button" };
        ribbon = new Ribbon
        {
            Tabs =
            {
                new RibbonTabItem
                {
                    Header = "Tab",
                    Groups = { new RibbonGroupBox { Header = "Group", Items = { button } } }
                }
            }
        };

        var window = new TestRibbonWindow(ribbon)
        {
            DataContext = new object()
        };

        UIHelper.DoEvents();

        Assert.That(PresentationSource.FromVisual(button), Is.Not.Null, "Precondition: the button is shown");

        return window;
    }

    // Opens the context menu the way WPF does on a right click or the context menu key:
    // PopupControlService raises ContextMenuOpening on the element, then opens the element's context menu
    // with the element as its owner (which makes the element the menu's PlacementTarget).
    private static void OpenContextMenu(UIElement element)
    {
        var serviceType = typeof(FrameworkElement).Assembly.GetType("System.Windows.Controls.PopupControlService", true);
        var currentProperty = serviceType.GetProperty("Current", BindingFlags.Static | BindingFlags.NonPublic);
        var raiseMethod = serviceType.GetMethod("RaiseContextMenuOpeningEvent", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(IInputElement), typeof(double), typeof(double), typeof(bool) }, null);

        Assert.That(currentProperty, Is.Not.Null, "Precondition: PopupControlService.Current exists");
        Assert.That(raiseMethod, Is.Not.Null, "Precondition: PopupControlService.RaiseContextMenuOpeningEvent exists");

        raiseMethod.Invoke(currentProperty.GetValue(null, null), new object[] { element, -1.0, -1.0, false });

        // A real right click or key press is input, and after input WPF asks all commands again whether they can execute,
        // which enables or disables the menu items. Nothing here is real input, so ask for that explicitly.
        CommandManager.InvalidateRequerySuggested();
        UIHelper.DoEvents();

        Assert.That(Ribbon.RibbonContextMenu.IsOpen, Is.True, "Precondition: the ribbon context menu is open");
        Assert.That(Ribbon.RibbonContextMenu.PlacementTarget, Is.SameAs(element), "Precondition: the menu was opened on the element");
    }

    // Closes the menu like WPF does (for example on a click outside of it) and waits until it is really closed.
    private static void CloseContextMenu()
    {
        var closed = false;
        RoutedEventHandler onClosed = (_, _) => closed = true;
        Ribbon.RibbonContextMenu.Closed += onClosed;
        try
        {
            Ribbon.RibbonContextMenu.SetCurrentValue(System.Windows.Controls.ContextMenu.IsOpenProperty, false);
            PumpUntil(() => closed);
        }
        finally
        {
            Ribbon.RibbonContextMenu.Closed -= onClosed;
        }

        Assert.That(closed, Is.True, "Precondition: the ribbon context menu closed");
    }

    private static System.Windows.Controls.MenuItem[] GetMenuItems()
    {
        return Ribbon.RibbonContextMenu.Items.OfType<System.Windows.Controls.MenuItem>().ToArray();
    }

    private static System.Windows.Controls.MenuItem GetAddToQuickAccessToolBarMenuItem()
    {
        // The first item is "Add to quick access toolbar" (see Ribbon.InitRibbonContextMenuItems)
        var menuItem = GetMenuItems().First();

        Assert.That(menuItem.Command, Is.SameAs(Ribbon.AddToQuickAccessCommand), "Precondition: found the add item");

        return menuItem;
    }

    // Popups close asynchronously (Popup destroys its window on a dispatcher timer), so pump until the condition holds.
    private static void PumpUntil(Func<bool> condition)
    {
        var stopwatch = Stopwatch.StartNew();

        while (condition() == false
               && stopwatch.Elapsed < TimeSpan.FromSeconds(5))
        {
            UIHelper.DoEvents();
            Thread.Sleep(10);
        }
    }

    private static bool IsAliveAfterGarbageCollection(WeakReference reference)
    {
        for (var i = 0; i < 5 && reference.IsAlive; i++)
        {
            // Let pending dispatcher work (layout, deferred cleanup of the closed window) finish first.
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
            Thread.Sleep(50);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        return reference.IsAlive;
    }
}
