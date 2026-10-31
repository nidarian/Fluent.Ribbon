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
/// with the ribbon still in start screen mode. Found by a review of round 7.
/// </summary>
[TestFixture]
public class StartScreenReopenDuringCloseTests
{
    [Test]
    public void StartScreen_set_open_again_during_its_close_animation_is_still_hidden()
    {
        var startScreen = new StartScreen
        {
            Content = new StartScreenTabControl(),
            AreAnimationsEnabled = true,
            IsOpen = true
        };

        var ribbon = new Ribbon { StartScreen = startScreen };

        using (new TestRibbonWindow(ribbon))
        {
            PumpDispatcherFor(TimeSpan.FromSeconds(1));

            Assert.That(startScreen.Shown, Is.True, "Precondition: the start screen was shown");
            Assert.That(GetAdorner(startScreen).Visibility, Is.EqualTo(Visibility.Visible), "Precondition: the start screen is displayed");

            startScreen.IsOpen = false;
            UIHelper.DoEvents();

            if (GetAdorner(startScreen).Visibility != Visibility.Visible)
            {
                Assert.Inconclusive("The close animation already completed before the start screen could be set open again.");
            }

            // A start screen is shown only once, so this shows nothing.
            startScreen.IsOpen = true;

            PumpDispatcherFor(TimeSpan.FromSeconds(2));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(GetAdorner(startScreen).Visibility, Is.EqualTo(Visibility.Collapsed), "The start screen must not stay drawn over the window after its close animation");
                Assert.That(ribbon.IsBackstageOrStartScreenOpen, Is.False, "The ribbon must leave start screen mode");
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
