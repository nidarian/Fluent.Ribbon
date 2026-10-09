namespace Fluent.Tests.Controls;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// An app that removes or replaces a <see cref="Ribbon"/> while its window stays open
/// (for example window.Content = otherView when switching views) must not keep the old ribbon alive.
/// The ribbon subscribed to <see cref="Window.Closed"/> of its window when loaded, but only removed its
/// SizeChanged and KeyDown handlers when unloaded. The window kept the removed ribbon
/// (with its tabs, quick access toolbar and the app's view models) until the window was closed.
/// </summary>
[TestFixture]
public class RibbonRemovedFromWindowTests
{
    /// <summary>
    /// Baseline: proves that the way these tests check for collection works (on CI too).
    /// A plain element removed from the same kind of window is collected.
    /// </summary>
    [Test]
    public void Baseline_plain_element_removed_from_an_open_window_is_collected()
    {
        var window = CreatePlainWindow();

        try
        {
            var weakElement = ShowAndRemove(window, () => new Border());

            Assert.That(IsCollected(weakElement), Is.True, "Baseline: a plain element removed from an open window must be collected");
        }
        finally
        {
            GC.KeepAlive(window);
            window.Close();
        }
    }

    /// <summary>
    /// Baseline: a ribbon that was never put into a window is collected.
    /// </summary>
    [Test]
    public void Baseline_ribbon_that_was_never_shown_is_collected()
    {
        var weakRibbon = CreateRibbonOnly();

        Assert.That(IsCollected(weakRibbon), Is.True, "Baseline: a ribbon that was never shown must be collected");
    }

    [Test]
    public void Ribbon_removed_from_an_open_Window_is_collected()
    {
        var window = CreatePlainWindow();

        try
        {
            var weakRibbon = ShowAndRemove(window, CreateRibbonInHost);

            Assert.That(IsCollected(weakRibbon), Is.True, "A ribbon removed from a window that stays open must not be kept alive by the window");
        }
        finally
        {
            GC.KeepAlive(window);
            window.Close();
        }
    }

    [Test]
    public void Ribbon_removed_from_an_open_RibbonWindow_is_collected()
    {
        var window = new TestRibbonWindow();

        try
        {
            var weakRibbon = ShowAndRemove(window, CreateRibbonInHost);

            Assert.That(IsCollected(weakRibbon), Is.True, "A ribbon removed from a RibbonWindow that stays open must not be kept alive by the window");
        }
        finally
        {
            GC.KeepAlive(window);
            window.Close();
        }
    }

    /// <summary>
    /// A ribbon that is removed, added again and removed again must also be released.
    /// </summary>
    [Test]
    public void Ribbon_removed_re_added_and_removed_again_is_collected()
    {
        var window = CreatePlainWindow();

        try
        {
            var weakRibbon = ShowRemoveReAddAndRemove(window);

            Assert.That(IsCollected(weakRibbon), Is.True, "A ribbon removed again after being re-added must not be kept alive by the window");
        }
        finally
        {
            GC.KeepAlive(window);
            window.Close();
        }
    }

    /// <summary>
    /// A ribbon removed from its window and added again must follow its window again,
    /// here: collapse when the window becomes too small.
    /// </summary>
    [Test]
    public void Ribbon_removed_and_re_added_still_follows_its_window()
    {
        var ribbon = new Ribbon();

        using (var window = new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            window.Content = null;
            UIHelper.DoEvents();

            window.Content = ribbon;
            UIHelper.DoEvents();

            Assert.That(ribbon.IsLoaded, Is.True, "Precondition: the ribbon is loaded again");
            Assert.That(ribbon.IsCollapsed, Is.False, "Precondition: the ribbon is not collapsed in a large window");

            window.Width = Ribbon.MinimalVisibleWidth - 50;
            window.Height = Ribbon.MinimalVisibleHeight - 50;
            UIHelper.DoEvents();

            Assert.That(window.ActualWidth, Is.LessThan(Ribbon.MinimalVisibleWidth), "Precondition: the window is small now");
            Assert.That(ribbon.IsCollapsed, Is.True, "A re-added ribbon must still collapse when its window becomes too small");
        }
    }

    /// <summary>
    /// A ribbon that is still in its window when the window is closed must still be detached from the window then
    /// (its state storage gets saved and disposed). The fix only removes the Closed handler when the ribbon is unloaded.
    /// </summary>
    [Test]
    public void Ribbon_still_in_its_window_is_detached_when_the_window_is_closed()
    {
        var ribbon = new TrackingRibbon();

        var window = new TestRibbonWindow(ribbon);
        UIHelper.DoEvents();

        Assert.That(ribbon.IsLoaded, Is.True, "Precondition: the ribbon is loaded");

        var storage = (TrackingRibbonStateStorage)ribbon.RibbonStateStorage;

        window.Close();
        UIHelper.DoEvents();

        Assert.That(storage.IsDisposed, Is.True, "The state storage of a ribbon that was in the window when it was closed must be disposed");
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

    private static FrameworkElement CreateRibbonInHost()
    {
        var ribbon = new Ribbon();
        ribbon.Tabs.Add(new RibbonTabItem { Header = "Tab" });

        var host = new Grid();
        host.Children.Add(ribbon);

        return host;
    }

    // Only a WeakReference may escape these helpers, so no local variable keeps the element alive.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference ShowAndRemove(Window window, Func<FrameworkElement> createContent)
    {
        var content = createContent();
        var target = content is Grid grid ? grid.Children[0] : content;

        window.Content = content;
        UIHelper.DoEvents();

        Assert.That(((FrameworkElement)target).IsLoaded, Is.True, "Precondition: the element is loaded");

        window.Content = null;
        UIHelper.DoEvents();

        Assert.That(((FrameworkElement)target).IsLoaded, Is.False, "Precondition: the element is unloaded");

        return new WeakReference(target);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference ShowRemoveReAddAndRemove(Window window)
    {
        var ribbon = new Ribbon();

        window.Content = ribbon;
        UIHelper.DoEvents();

        window.Content = null;
        UIHelper.DoEvents();

        window.Content = ribbon;
        UIHelper.DoEvents();

        Assert.That(ribbon.IsLoaded, Is.True, "Precondition: the ribbon is loaded again");

        window.Content = null;
        UIHelper.DoEvents();

        Assert.That(ribbon.IsLoaded, Is.False, "Precondition: the ribbon is unloaded");

        return new WeakReference(ribbon);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateRibbonOnly()
    {
        var ribbon = new Ribbon();
        ribbon.Tabs.Add(new RibbonTabItem { Header = "Tab" });

        return new WeakReference(ribbon);
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

    private sealed class TrackingRibbon : Ribbon
    {
        protected override IRibbonStateStorage CreateRibbonStateStorage()
        {
            return new TrackingRibbonStateStorage(this);
        }
    }

    private sealed class TrackingRibbonStateStorage : RibbonStateStorage
    {
        public TrackingRibbonStateStorage(Ribbon ribbon)
            : base(ribbon)
        {
        }

        public bool IsDisposed => this.Disposed;
    }
}
