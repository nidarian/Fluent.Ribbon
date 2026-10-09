namespace Fluent.Tests.Controls;

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for the Windows system menu (Restore, Move, Size, ..., Close) that <see cref="WindowCommands"/> shows on a right click.
/// </summary>
[TestFixture]
public class WindowCommandsSystemMenuTests
{
    private const int WmEnterMenuLoop = 0x0211;
    private const int WmExitMenuLoop = 0x0212;
    private const int WmEnterIdle = 0x0121;
    private const int WmCancelMode = 0x001F;
    private const int WmKeyDown = 0x0100;
    private const int VkEscape = 0x1B;

    [Test]
    public void Right_click_on_an_own_item_does_not_open_the_system_menu()
    {
        // An app adds its own button (for example "Help" or "Account") with its own context menu to the window commands.
        var ownItem = new System.Windows.Controls.Button
        {
            Content = "Help",
            ContextMenu = new System.Windows.Controls.ContextMenu { Items = { new System.Windows.Controls.MenuItem { Header = "App menu" } } }
        };

        using (var window = new TestRibbonWindow())
        {
            var windowCommands = window.WindowCommands;
            windowCommands.Items.Add(ownItem);
            UIHelper.DoEvents();

            Assert.That(IsVisualAncestor(windowCommands.ItemsControl, ownItem), Is.True, "Precondition: the own item should be shown inside the items host of the window commands");

            var menuLoops = CountSystemMenuLoops(window, () => RightButtonDown(ownItem));

            // The system menu runs a modal menu loop which swallows the button up,
            // so the item's own context menu would never open.
            Assert.That(menuLoops, Is.EqualTo(0), "A right click on the app's own item must not open the window's system menu");
        }
    }

    [Test]
    public void Right_click_on_the_window_commands_themselves_still_opens_the_system_menu()
    {
        using (var window = new TestRibbonWindow())
        {
            var windowCommands = window.WindowCommands;
            windowCommands.Items.Add(new System.Windows.Controls.Button { Content = "Help" });
            UIHelper.DoEvents();

            var menuLoops = CountSystemMenuLoops(window, () => RightButtonDown(windowCommands));

            Assert.That(menuLoops, Is.EqualTo(1), "A right click on the window commands outside of the app's own items should still open the system menu");
        }
    }

    private static void RightButtonDown(UIElement target)
    {
        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
        {
            RoutedEvent = UIElement.MouseRightButtonDownEvent
        };

        target.RaiseEvent(args);
        UIHelper.DoEvents();
    }

    // Runs the action and counts how often a menu loop was entered for the window.
    // The system menu is shown modally (the call only returns once the menu is closed),
    // so every menu loop that is entered is closed again right away and the test can't hang.
    private static int CountSystemMenuLoops(Window window, Action action)
    {
        var handle = new WindowInteropHelper(window).Handle;
        var hwndSource = HwndSource.FromHwnd(handle);
        Assert.That(hwndSource, Is.Not.Null, "Precondition: the window should have a handle");

        var menuLoops = 0;
        var insideMenuLoop = false;

        IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WmEnterMenuLoop:
                    menuLoops++;
                    insideMenuLoop = true;

                    // The menu loop has not started yet, so ask it to close as soon as it runs.
                    _ = NativeMethods.PostMessage(hwnd, WmCancelMode, IntPtr.Zero, IntPtr.Zero);
                    break;

                case WmCancelMode:
                case WmEnterIdle:
                    if (insideMenuLoop)
                    {
                        _ = NativeMethods.EndMenu();
                    }

                    break;

                case WmExitMenuLoop:
                    insideMenuLoop = false;
                    break;
            }

            return IntPtr.Zero;
        }

        // Last resort if the menu could not be closed from inside the hook: cancel it from another thread.
        void CancelMenuFromOtherThread(object state)
        {
            _ = NativeMethods.PostMessage(handle, WmCancelMode, IntPtr.Zero, IntPtr.Zero);
            _ = NativeMethods.PostMessage(handle, WmKeyDown, new IntPtr(VkEscape), IntPtr.Zero);
        }

        HwndSourceHook hook = Hook;

        using (new Timer(CancelMenuFromOtherThread, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1)))
        {
            hwndSource.AddHook(hook);

            try
            {
                action();
            }
            finally
            {
                hwndSource.RemoveHook(hook);
            }
        }

        return menuLoops;
    }

    private static bool IsVisualAncestor(DependencyObject ancestor, DependencyObject element)
    {
        if (ancestor is null)
        {
            return false;
        }

        for (var current = VisualTreeHelper.GetParent(element); current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }
        }

        return false;
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EndMenu();
    }
}
