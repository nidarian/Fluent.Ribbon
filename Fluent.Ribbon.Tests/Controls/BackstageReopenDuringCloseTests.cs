namespace Fluent.Tests.Controls;

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class BackstageReopenDuringCloseTests
{
    /// <summary>
    /// Closing the backstage with animations enabled starts a close animation and hides the backstage
    /// (and gives the ribbon back its normal state) when that animation has completed.
    /// When the user opens the backstage again while the close animation is still running
    /// (for example a quick double click on the File button), the backstage must stay open and visible:
    /// the "completed" step of the old, replaced close animation must not hide it afterwards.
    /// </summary>
    [Test]
    public void Reopening_during_the_close_animation_keeps_the_backstage_displayed()
    {
        var backstage = new Backstage { Content = new System.Windows.Controls.Button { Content = "Inside" }, AreAnimationsEnabled = true };
        var ribbon = new Ribbon { Menu = backstage };

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            // A normal animated open and close, to measure how long a close takes in this environment.
            backstage.IsOpen = true;
            PumpDispatcherFor(TimeSpan.FromSeconds(1));
            Assert.That(GetAdorner(backstage).Visibility, Is.EqualTo(Visibility.Visible), "Precondition: the backstage is displayed after opening");

            backstage.IsOpen = false;
            var closeDuration = PumpDispatcherUntil(() => GetAdorner(backstage).Visibility == Visibility.Collapsed, TimeSpan.FromSeconds(10));

            if (closeDuration is null)
            {
                Assert.Inconclusive("The close animation did not complete within 10 seconds: animations don't run in this test environment.");
            }

            Assert.That(ribbon.IsBackstageOrStartScreenOpen, Is.False, "Precondition: a normal close restores the ribbon");

            backstage.IsOpen = true;
            PumpDispatcherFor(TimeSpan.FromSeconds(1));
            Assert.That(GetAdorner(backstage).Visibility, Is.EqualTo(Visibility.Visible), "Precondition: the backstage is displayed after opening again");

            // Close and open again right away, while the close animation is still running.
            backstage.IsOpen = false;
            UIHelper.DoEvents();

            if (GetAdorner(backstage).Visibility != Visibility.Visible)
            {
                Assert.Inconclusive("The close animation already completed before the backstage could be reopened.");
            }

            backstage.IsOpen = true;

            // Let much more time pass than the old close animation needs (and than the open animation needs).
            var wait = TimeSpan.FromTicks(Math.Max(TimeSpan.FromSeconds(2).Ticks, closeDuration.Value.Ticks * 4));
            PumpDispatcherFor(wait);

            Assert.That(backstage.IsOpen, Is.True);
            Assert.That(GetAdorner(backstage).Visibility, Is.EqualTo(Visibility.Visible), "The reopened backstage must stay displayed after the old close animation has ended");
            Assert.That(ribbon.IsBackstageOrStartScreenOpen, Is.True, "The ribbon must stay in backstage mode while the backstage is open");

            // And a normal close still works afterwards.
            backstage.IsOpen = false;
            var finalCloseDuration = PumpDispatcherUntil(() => GetAdorner(backstage).Visibility == Visibility.Collapsed, TimeSpan.FromSeconds(10));

            Assert.That(finalCloseDuration, Is.Not.Null, "A normal close after the reopen must hide the backstage");
            Assert.That(ribbon.IsBackstageOrStartScreenOpen, Is.False, "A normal close after the reopen must restore the ribbon");
        }
    }

    private static UIElement GetAdorner(Backstage backstage)
    {
        return backstage.GetFieldValue<UIElement>("adorner");
    }

    // WPF animation clocks run in real time, so the dispatcher has to keep running while time passes.
    private static void PumpDispatcherFor(TimeSpan duration)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < duration)
        {
            UIHelper.DoEvents();
            Thread.Sleep(10);
        }

        UIHelper.DoEvents();
    }

    // Returns how long it took until the condition was met, or null when the timeout was reached first.
    private static TimeSpan? PumpDispatcherUntil(Func<bool> condition, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            UIHelper.DoEvents();

            if (condition())
            {
                return stopwatch.Elapsed;
            }

            Thread.Sleep(10);
        }

        return null;
    }
}
