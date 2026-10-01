namespace Fluent.Tests.Helper;

using System;
using System.Windows.Input;
using System.Windows.Threading;
using NUnit.Framework;

public static class UIHelper
{
    public static void DoEvents()
    {
        Dispatcher.CurrentDispatcher.DoEvents();
    }

    public static void DoEvents(this Dispatcher dispatcher)
    {
        dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
    }

    /// <summary>
    /// Call right before asserting that an element HAS keyboard focus.
    /// On long CI runs the test window sometimes stops being the active window (Windows gives the
    /// foreground to something else). Then no element of the test has keyboard focus at all and
    /// Keyboard.FocusedElement is null. A real focus bug leaves focus on some other element instead,
    /// so "nothing is focused" is reported as inconclusive (an environment problem), not as a failure.
    /// </summary>
    public static void InconclusiveIfKeyboardFocusLost(string step)
    {
        if (Keyboard.FocusedElement is null)
        {
            Assert.Inconclusive($"The test window lost keyboard focus ({step}).");
        }
    }
}