namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// A collapsed <see cref="RibbonGroupBox"/> acts as a drop down button, and buttons are activated with Space and Enter.
/// RibbonGroupBox.OnKeyDown only opened the drop down on Space.
/// </summary>
[TestFixture]
public class RibbonGroupBoxKeyboardTests
{
    [Test]
    public void Enter_opens_collapsed_group()
    {
        var groupBox = new RibbonGroupBox
        {
            Header = "Group",
            Items =
            {
                new Button { Header = "Button" }
            }
        };

        using (new TestRibbonWindow(groupBox))
        {
            groupBox.State = RibbonGroupBoxState.Collapsed;
            UIHelper.DoEvents();

            Assert.That(groupBox.IsInButtonState, Is.True, "precondition: a collapsed group is in button state");

            // Baseline: Space opens the drop down, so opening works in this environment.
            PressKey(groupBox, Key.Space);
            Assert.That(groupBox.IsDropDownOpen, Is.True, "precondition: Space should open the collapsed group");

            groupBox.IsDropDownOpen = false;
            UIHelper.DoEvents();
            Assert.That(groupBox.IsDropDownOpen, Is.False, "precondition: drop down should be closed again");

            PressKey(groupBox, Key.Enter);

            Assert.That(groupBox.IsDropDownOpen, Is.True, "Enter should open the collapsed group like Space does");
        }
    }

    // Keys pressed inside the open drop down of a collapsed group bubble up through the popup to the group box.
    // The group box handled Enter from anywhere, so Enter typed into a text box in the drop down was swallowed:
    // window-level Enter key bindings, KeyDown handlers and default buttons never saw it.
    [Test]
    public void Enter_typed_into_a_text_box_inside_the_drop_down_is_not_swallowed_by_the_group()
    {
        var textBox = new System.Windows.Controls.TextBox();
        var groupBox = CreateGroupBoxWith(textBox);

        using (var window = new TestRibbonWindow(groupBox))
        {
            var entersSeenByWindow = 0;
            window.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    entersSeenByWindow++;
                }
            };

            OpenCollapsedGroup(groupBox, textBox);

            var args = PressKey(textBox, Key.Enter);

            Assert.That(groupBox.IsDropDownOpen, Is.True, "Enter typed into the text box must not close the drop down");
            Assert.That(args.Handled, Is.False, "Enter typed into the text box must not be handled by the group");
            Assert.That(entersSeenByWindow, Is.EqualTo(1), "Enter typed into the text box should reach the window");
        }
    }

    // Space is checked too, because the group box also handles Space. A text box may handle Space itself,
    // so only check that the drop down stays open.
    [Test]
    public void Space_typed_into_a_text_box_inside_the_drop_down_keeps_it_open()
    {
        var textBox = new System.Windows.Controls.TextBox();
        var groupBox = CreateGroupBoxWith(textBox);

        using (new TestRibbonWindow(groupBox))
        {
            OpenCollapsedGroup(groupBox, textBox);

            PressKey(textBox, Key.Space);

            Assert.That(groupBox.IsDropDownOpen, Is.True, "Space typed into the text box must not close the drop down");
        }
    }

    // Showcase, Insert tab, group "FG" collapsed: Alt, I, F, G opens the group's drop down and shows the KeyTips
    // of its items. Pressing Down to move into the items closed the drop down instead. KeyTipService ends KeyTips
    // on a key which is no KeyTip input (like Down), and ending them that way also closed every open popup,
    // including the drop down the user just opened with the KeyTip.
    [Test]
    public void Down_after_opening_a_collapsed_group_with_its_KeyTip_keeps_the_drop_down_open()
    {
        var firstItem = new Button { Header = "First", KeyTip = "A" };
        var secondItem = new Button { Header = "Second", KeyTip = "B" };

        var groupBox = new RibbonGroupBox
        {
            Header = "Group",
            KeyTip = "FG",

            // Only the collapsed state, so the group stays collapsed whatever the window size.
            StateDefinition = new RibbonGroupBoxStateDefinition("Collapsed"),
            Items =
            {
                firstItem,
                secondItem
            }
        };

        var tabItem = new RibbonTabItem { Header = "Insert", KeyTip = "I" };
        tabItem.Groups.Add(groupBox);

        var ribbon = new Ribbon
        {
            // Don't load a persisted (maybe minimized) state, a minimized ribbon would open the tab in a popup instead.
            AutomaticStateManagement = false
        };
        ribbon.Tabs.Add(tabItem);

        using (var window = new TestRibbonWindow(ribbon))
        {
            // KeyTipService ignores all keys while the window is not active.
            window.Activate();
            UIHelper.DoEvents();

            if (window.IsActive == false)
            {
                Assert.Inconclusive("The test window could not be activated, so KeyTipService would ignore all keys.");
            }

            // IsActive only tells the state at the moment it is checked. If another test window takes activation for a moment
            // between two keys, KeyTipService terminates the KeyTips (and closes popups) and the window can be active again
            // by the time IsActive is checked. So remember any deactivation during the key sequence.
            var wasDeactivated = false;
            window.Deactivated += (_, _) => wasDeactivated = true;

            Assert.That(groupBox.State, Is.EqualTo(RibbonGroupBoxState.Collapsed), "precondition: the group is collapsed");

            var keyTipService = ribbon.GetFieldValue<KeyTipService>("keyTipService");

            // Alt shows the KeyTips, I selects the tab, F G presses the KeyTip of the collapsed group.
            PressKeyOnFocusedElement(window, Key.LeftAlt);
            InconclusiveIfWindowDeactivated(window, wasDeactivated, "after Alt");
            Assert.That(keyTipService.AreAnyKeyTipsVisible, Is.True, "precondition: Alt should show the KeyTips");

            PressKeyOnFocusedElement(window, Key.I);
            InconclusiveIfWindowDeactivated(window, wasDeactivated, "after I");
            Assert.That(keyTipService.AreAnyKeyTipsVisible, Is.True, "precondition: I should show the KeyTips of the tab");

            PressKeyOnFocusedElement(window, Key.F);
            InconclusiveIfWindowDeactivated(window, wasDeactivated, "after F");
            Assert.That(keyTipService.AreAnyKeyTipsVisible, Is.True, "precondition: F should keep the KeyTips (FG is not complete yet)");

            PressKeyOnFocusedElement(window, Key.G);
            InconclusiveIfWindowDeactivated(window, wasDeactivated, "after F G");

            Assert.That(groupBox.IsDropDownOpen, Is.True, "precondition: F G should open the drop down of the collapsed group");
            Assert.That(keyTipService.GetFieldValue<KeyTipAdorner>("activeAdornerChain"), Is.Not.Null, "precondition: KeyTips should still be active for the items in the drop down");

            UIHelper.InconclusiveIfKeyboardFocusLost("after opening the drop down");
            Assert.That(firstItem.IsKeyboardFocused, Is.True, "precondition: opening the drop down should focus its first item");

            PressKeyOnFocusedElement(window, Key.Down);
            InconclusiveIfWindowDeactivated(window, wasDeactivated, "after Down");

            Assert.That(groupBox.IsDropDownOpen, Is.True, "Down should move inside the drop down, not close it");
            UIHelper.InconclusiveIfKeyboardFocusLost("after Down");
            Assert.That(firstItem.IsKeyboardFocused || secondItem.IsKeyboardFocused, Is.True, "Keyboard focus should stay on the items of the drop down");
        }
    }

    private static RibbonGroupBox CreateGroupBoxWith(UIElement content)
    {
        return new RibbonGroupBox
        {
            Header = "Group",
            Items =
            {
                content
            }
        };
    }

    private static void OpenCollapsedGroup(RibbonGroupBox groupBox, UIElement content)
    {
        groupBox.State = RibbonGroupBoxState.Collapsed;
        UIHelper.DoEvents();

        groupBox.IsDropDownOpen = true;
        UIHelper.DoEvents();

        Assert.That(groupBox.IsDropDownOpen, Is.True, "precondition: the drop down should be open");
        Assert.That(PresentationSource.FromVisual(content), Is.Not.Null, "precondition: the content should be shown in the drop down");
    }

    // The tests of the three target frameworks run at the same time, so another test window can take activation.
    // KeyTipService then ignores keys, and deactivating the window closes all popups (KeyTipService.WindowProc).
    // That is an environment problem, not the bug, so it ends the test as inconclusive.
    private static void InconclusiveIfWindowDeactivated(Window window, bool wasDeactivated, string step)
    {
        if (wasDeactivated
            || window.IsActive == false)
        {
            Assert.Inconclusive($"The test window was deactivated ({step}).");
        }
    }

    // Like PressKey, but on the element which has keyboard focus (where WPF delivers keys), followed by the key up.
    // The window's PreviewKeyDown and KeyUp handlers are how KeyTipService sees keys.
    private static void PressKeyOnFocusedElement(Window window, Key key)
    {
        var target = Keyboard.FocusedElement as UIElement ?? window;

        // Taken before the key down: if the key closes a drop down, an item in it has no presentation source anymore.
        var inputSource = PresentationSource.FromVisual(target);

        PressKey(target, key);

        var args = new KeyEventArgs(Keyboard.PrimaryDevice, inputSource, 0, key)
        {
            RoutedEvent = Keyboard.PreviewKeyUpEvent
        };

        target.RaiseEvent(args);

        args.RoutedEvent = Keyboard.KeyUpEvent;
        target.RaiseEvent(args);

        UIHelper.DoEvents();
    }

    // Simulates a key press the way WPF input delivers it: PreviewKeyDown (tunnel) and then KeyDown (bubble)
    // sharing one args object.
    private static KeyEventArgs PressKey(UIElement target, Key key)
    {
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(target), 0, key)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        };

        target.RaiseEvent(args);

        args.RoutedEvent = Keyboard.KeyDownEvent;
        target.RaiseEvent(args);

        UIHelper.DoEvents();

        return args;
    }
}
