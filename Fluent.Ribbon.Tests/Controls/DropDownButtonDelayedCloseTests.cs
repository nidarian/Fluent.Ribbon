namespace Fluent.Tests.Controls;

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for <see cref="DropDownButton.ClosePopupOnMouseDown"/> together with <see cref="DropDownButton.ClosePopupOnMouseDownDelay"/>.
/// </summary>
[TestFixture]
public class DropDownButtonDelayedCloseTests
{
    private const int CloseDelay = 100;

    [Test]
    public void Delayed_close_does_not_close_a_drop_down_that_was_reopened_in_the_meantime()
    {
        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            ClosePopupOnMouseDown = true,
            ClosePopupOnMouseDownDelay = CloseDelay,
            Items = { new MenuItem { Header = "Item" } }
        };

        using (new TestRibbonWindow(dropDownButton))
        {
            dropDownButton.ApplyTemplate();
            UIHelper.DoEvents();

            Assert.That(dropDownButton.DropDownPopup, Is.Not.Null, "Precondition: the template must provide the popup");

            // 1. Baseline: a mouse down inside the popup closes the drop down after the delay.
            //    This proves the test really triggers the delayed close, so step 2 can't pass by accident.
            Open(dropDownButton);
            RaiseMouseDownInPopup(dropDownButton);
            PumpDispatcherFor(TimeSpan.FromSeconds(1));

            Assert.That(dropDownButton.IsDropDownOpen, Is.False, "Precondition: the delayed close should close the drop down when nothing else happens");

            // 2. Mouse down schedules a close, but the user closes and reopens the drop down before the delay ran out.
            Open(dropDownButton);
            RaiseMouseDownInPopup(dropDownButton);

            dropDownButton.IsDropDownOpen = false;
            UIHelper.DoEvents();
            Open(dropDownButton);

            Assert.That(dropDownButton.IsDropDownOpen, Is.True, "Precondition: the drop down should be open again");

            // Give the scheduled close plenty of time to run (it waits at least 100 ms).
            PumpDispatcherFor(TimeSpan.FromSeconds(1));

            // The close was meant for the drop down that is gone. It must not close the new one.
            Assert.That(dropDownButton.IsDropDownOpen, Is.True, "A delayed close from an earlier open must not close the reopened drop down");

            dropDownButton.IsDropDownOpen = false;
            UIHelper.DoEvents();
        }
    }

    private static void Open(DropDownButton dropDownButton)
    {
        dropDownButton.IsDropDownOpen = true;
        UIHelper.DoEvents();
    }

    // DropDownButton listens for MouseDown on its popup (including already handled events),
    // so raising the event on the popup is enough to schedule the delayed close.
    private static void RaiseMouseDownInPopup(DropDownButton dropDownButton)
    {
        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = Mouse.MouseDownEvent
        };

        dropDownButton.DropDownPopup!.RaiseEvent(args);
    }

    // The close is scheduled from a background task and then posted to the dispatcher,
    // so we have to keep the dispatcher running while real time passes.
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
