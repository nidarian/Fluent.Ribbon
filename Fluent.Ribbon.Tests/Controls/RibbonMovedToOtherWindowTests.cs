namespace Fluent.Tests.Controls;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// An app can move a <see cref="Ribbon"/> from one window to another at runtime
/// (remove it from window A, add it to window B, which stays open, then close A).
/// The ribbon re-attaches to its window on Loaded. These tests check that, after the move,
/// the ribbon follows window B (title bar, KeyTips, collapsing) and no longer keeps the closed window A alive.
/// </summary>
[TestFixture]
public class RibbonMovedToOtherWindowTests
{
    /// <summary>
    /// The title bar of the new RibbonWindow must show the quick access toolbar and contextual groups of the moved ribbon,
    /// and the title bar of the old window must no longer show them.
    /// </summary>
    [Test]
    public void Ribbon_moved_to_another_RibbonWindow_shows_its_toolbar_in_the_new_title_bar_only()
    {
        var ribbon = new Ribbon();

        using (var windowA = new TestRibbonWindow(ribbon))
        using (var windowB = new TestRibbonWindow())
        {
            UIHelper.DoEvents();

            Assert.That(windowA.TitleBar, Is.Not.Null, "Precondition: window A provides a title bar");
            Assert.That(windowB.TitleBar, Is.Not.Null, "Precondition: window B provides a title bar");
            Assert.That(windowA.TitleBar!.QuickAccessToolBar, Is.SameAs(ribbon.QuickAccessToolBar), "Precondition: the title bar of window A shows the quick access toolbar of the ribbon");

            MoveRibbon(ribbon, windowA, windowB);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(ribbon.TitleBar, Is.SameAs(windowB.TitleBar), "The moved ribbon must use the title bar of its new window");
                Assert.That(windowB.TitleBar!.QuickAccessToolBar, Is.SameAs(ribbon.QuickAccessToolBar), "The title bar of the new window must show the quick access toolbar of the moved ribbon");
                Assert.That(windowB.TitleBar.ItemsSource, Is.SameAs(ribbon.ContextualGroups), "The title bar of the new window must show the contextual groups of the moved ribbon");
                Assert.That(windowA.TitleBar.QuickAccessToolBar, Is.Null, "The title bar of the old window must no longer show the quick access toolbar of the moved ribbon");
                Assert.That(windowA.TitleBar.ItemsSource, Is.Null, "The title bar of the old window must no longer show the contextual groups of the moved ribbon");
            }
        }
    }

    /// <summary>
    /// KeyTips (Alt) are handled through the window of the ribbon. After the move this must be the new window.
    /// </summary>
    [Test]
    public void KeyTips_of_a_moved_ribbon_are_handled_in_the_new_window()
    {
        var ribbon = new Ribbon();

        var windowA = new TestRibbonWindow(ribbon);

        try
        {
            using (var windowB = new TestRibbonWindow())
            {
                UIHelper.DoEvents();

                var keyTipService = ribbon.GetFieldValue<KeyTipService>("keyTipService");

                Assert.That(keyTipService.GetFieldValue<Window>("window"), Is.SameAs(windowA), "Precondition: KeyTips are handled in window A");

                MoveRibbon(ribbon, windowA, windowB);

                Assert.That(keyTipService.GetFieldValue<Window>("window"), Is.SameAs(windowB), "KeyTips of the moved ribbon must be handled in the new window");

                windowA.Close();
                UIHelper.DoEvents();

                Assert.That(keyTipService.GetFieldValue<Window>("window"), Is.SameAs(windowB), "Closing the old window must not detach the KeyTips from the new window");
            }
        }
        finally
        {
            // Only close window A if the test did not get to close it.
            if (windowA.IsVisible)
            {
                windowA.Close();
            }
        }
    }

    /// <summary>
    /// The moved ribbon must collapse when its new window becomes too small, also after the old window was closed.
    /// </summary>
    [Test]
    public void Ribbon_moved_to_another_window_follows_the_size_of_the_new_window()
    {
        var ribbon = new Ribbon();

        var windowA = new TestRibbonWindow(ribbon);

        try
        {
            using (var windowB = new TestRibbonWindow())
            {
                UIHelper.DoEvents();

                MoveRibbon(ribbon, windowA, windowB);

                windowA.Close();
                UIHelper.DoEvents();

                Assert.That(ribbon.IsCollapsed, Is.False, "Precondition: the ribbon is not collapsed in a large window");

                windowB.Width = Ribbon.MinimalVisibleWidth - 50;
                windowB.Height = Ribbon.MinimalVisibleHeight - 50;
                UIHelper.DoEvents();

                Assert.That(windowB.ActualWidth, Is.LessThan(Ribbon.MinimalVisibleWidth), "Precondition: window B is small now");
                Assert.That(ribbon.IsCollapsed, Is.True, "A moved ribbon must collapse when its new window becomes too small");
            }
        }
        finally
        {
            // Only close window A if the test did not get to close it.
            if (windowA.IsVisible)
            {
                windowA.Close();
            }
        }
    }

    [Test]
    public void Old_Window_is_collected_after_the_ribbon_was_moved_and_the_old_window_closed()
    {
        var windowB = CreatePlainWindow();
        var ribbon = new Ribbon();

        try
        {
            var weakWindowA = ShowMoveAndCloseOld(ribbon, CreatePlainWindow, windowB);

            Assert.That(IsCollected(weakWindowA), Is.True, "A closed window must not be kept alive by a ribbon that was moved from it to another window");
            Assert.That(ribbon.IsLoaded, Is.True, "The ribbon stays loaded in window B");
        }
        finally
        {
            GC.KeepAlive(ribbon);
            windowB.Close();
        }
    }

    [Test]
    public void Old_RibbonWindow_is_collected_after_the_ribbon_was_moved_and_the_old_window_closed()
    {
        var windowB = new TestRibbonWindow();
        var ribbon = new Ribbon();

        try
        {
            var weakWindowA = ShowMoveAndCloseOld(ribbon, () => new TestRibbonWindow(), windowB);

            Assert.That(IsCollected(weakWindowA), Is.True, "A closed RibbonWindow must not be kept alive by a ribbon that was moved from it to another window");
            Assert.That(ribbon.IsLoaded, Is.True, "The ribbon stays loaded in window B");
        }
        finally
        {
            GC.KeepAlive(ribbon);
            windowB.Close();
        }
    }

    private static void MoveRibbon(Ribbon ribbon, Window from, Window to)
    {
        from.Content = null;
        UIHelper.DoEvents();

        Assert.That(ribbon.IsLoaded, Is.False, "Precondition: the ribbon is unloaded after being removed from the old window");

        to.Content = ribbon;
        UIHelper.DoEvents();

        Assert.That(ribbon.IsLoaded, Is.True, "Precondition: the ribbon is loaded in the new window");
        Assert.That(Window.GetWindow(ribbon), Is.SameAs(to), "Precondition: the ribbon is in the new window");
    }

    private static Window CreatePlainWindow()
    {
        var window = new Window
        {
            Width = 800,
            Height = 600,
            ShowActivated = false,
            ShowInTaskbar = false
        };

        if (Debugger.IsAttached == false)
        {
            window.Left = int.MinValue;
            window.Top = int.MinValue;
        }

        FrameworkHelper.SetUseLayoutRounding(window, true);

        window.Show();

        return window;
    }

    // Only a WeakReference to window A may escape this helper, so no local variable keeps it alive.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference ShowMoveAndCloseOld(Ribbon ribbon, Func<Window> createWindowA, Window windowB)
    {
        var windowA = createWindowA();

        windowA.Content = ribbon;
        UIHelper.DoEvents();

        Assert.That(ribbon.IsLoaded, Is.True, "Precondition: the ribbon is loaded in window A");

        MoveRibbon(ribbon, windowA, windowB);

        windowA.Close();
        UIHelper.DoEvents();

        return new WeakReference(windowA);
    }

    private static bool IsCollected(WeakReference weakReference)
    {
        for (var i = 0; i < 3 && weakReference.IsAlive; i++)
        {
            UIHelper.DoEvents();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        return weakReference.IsAlive == false;
    }
}
