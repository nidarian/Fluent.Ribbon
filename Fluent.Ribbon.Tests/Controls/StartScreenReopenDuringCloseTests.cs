namespace Fluent.Tests.Controls;

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Round 7 made the end of a close animation skip hiding the backstage when it is open again
/// (<see cref="BackstageReopenDuringCloseTests"/>), assuming that the reopen started a new open animation.
/// A <see cref="StartScreen"/> is only shown once: setting IsOpen again after that shows nothing.
/// So a StartScreen closed and set open again during its close animation was left half drawn over the window,
/// with the ribbon still in start screen mode. Found by a review of round 7. The first test reproduces the same mechanism
/// with a Backstage; the StartScreen itself goes through the same code but is not tested here.
/// </summary>
[TestFixture]
public class StartScreenReopenDuringCloseTests
{
    /// <summary>
    /// The same mechanism without a StartScreen (whose setup isn't reliable in these tests): IsOpen becomes true again during the
    /// close animation, but Show returns early and starts no new open animation, here because the backstage has no content any more.
    /// The end of the close animation must still hide the backstage and give the ribbon back its normal state.
    /// </summary>
    [Test]
    public void Backstage_set_open_again_during_its_close_animation_without_a_new_show_is_still_hidden()
    {
        var backstage = new Backstage { Content = new System.Windows.Controls.Button { Content = "Inside" }, AreAnimationsEnabled = true };
        var ribbon = new Ribbon { Menu = backstage };

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            backstage.IsOpen = true;
            PumpDispatcherFor(TimeSpan.FromSeconds(1));
            Assert.That(GetAdorner(backstage), Is.Not.Null, "Precondition: the backstage has its adorner after opening");
            Assert.That(GetAdorner(backstage).Visibility, Is.EqualTo(Visibility.Visible), "Precondition: the backstage is displayed after opening");

            backstage.IsOpen = false;
            UIHelper.DoEvents();

            if (GetAdorner(backstage).Visibility != Visibility.Visible)
            {
                Assert.Inconclusive("The close animation already completed before the backstage could be set open again.");
            }

            // Show returns early without content (like a StartScreen that was already shown), so nothing new is shown.
            backstage.Content = null;
            backstage.IsOpen = true;

            PumpDispatcherFor(TimeSpan.FromSeconds(2));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(GetAdorner(backstage).Visibility, Is.EqualTo(Visibility.Collapsed), "The backstage must not stay drawn over the window after its close animation");
                Assert.That(ribbon.IsBackstageOrStartScreenOpen, Is.False, "The ribbon must leave backstage mode");
            }
        }
    }

    /// <summary>
    /// Open, close, open and close again, all faster than the close animation: the first close animation must not hide
    /// the backstage while the second one is still running, and the backstage ends up hidden with the ribbon restored.
    /// </summary>
    [Test]
    public void Backstage_closed_twice_during_close_animations_ends_hidden_and_restored()
    {
        var backstage = new Backstage { Content = new System.Windows.Controls.Button { Content = "Inside" }, AreAnimationsEnabled = true };
        var ribbon = new Ribbon { Menu = backstage };

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            backstage.IsOpen = true;
            PumpDispatcherFor(TimeSpan.FromSeconds(1));
            Assert.That(GetAdorner(backstage).Visibility, Is.EqualTo(Visibility.Visible), "Precondition: the backstage is displayed after opening");

            backstage.IsOpen = false;
            UIHelper.DoEvents();
            backstage.IsOpen = true;
            UIHelper.DoEvents();
            backstage.IsOpen = false;

            PumpDispatcherFor(TimeSpan.FromSeconds(2));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(GetAdorner(backstage).Visibility, Is.EqualTo(Visibility.Collapsed), "The backstage must end up hidden");
                Assert.That(ribbon.IsBackstageOrStartScreenOpen, Is.False, "The ribbon must be restored");
            }
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
}
