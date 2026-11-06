namespace Fluent.Tests.Automation.Peers;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// A checked toggle button or checkable menu item in a group (GroupName set) stays checked when it is clicked again, so a group
/// like Left/Center/Right keeps its checked item (issue #481). UI Automation Toggle must keep the state like a click does
/// (it doesn't raise Click or run the command, as WPF's Toggle never did).
/// </summary>
[TestFixture]
public class GroupedToggleAutomationPeerTests
{
    [Test]
    public void Toggle_on_a_checked_grouped_ToggleButton_keeps_it_checked()
    {
        var left = new Fluent.ToggleButton { Header = "Left", GroupName = "Alignment", IsChecked = true };
        var right = new Fluent.ToggleButton { Header = "Right", GroupName = "Alignment" };

        using (new TestRibbonWindow(new StackPanel { Children = { left, right } }))
        {
            UIHelper.DoEvents();

            var toggle = UIElementAutomationPeer.CreatePeerForElement(left).GetPattern(PatternInterface.Toggle) as IToggleProvider;
            Assert.That(toggle, Is.Not.Null, "Precondition: the toggle button offers the Toggle pattern");

            toggle.Toggle();
            UIHelper.DoEvents();

            Assert.That(left.IsChecked, Is.True, "Like a click, UI Automation Toggle must keep a checked grouped button checked");
            Assert.That(toggle.ToggleState, Is.EqualTo(ToggleState.On), "The reported state must match");
        }
    }

    [Test]
    public void Toggle_on_a_disabled_checked_grouped_ToggleButton_refuses()
    {
        var left = new Fluent.ToggleButton { Header = "Left", GroupName = "Alignment", IsChecked = true, IsEnabled = false };

        using (new TestRibbonWindow(new StackPanel { Children = { left } }))
        {
            UIHelper.DoEvents();

            var toggle = UIElementAutomationPeer.CreatePeerForElement(left).GetPattern(PatternInterface.Toggle) as IToggleProvider;
            Assert.That(toggle, Is.Not.Null, "Precondition: the toggle button offers the Toggle pattern");

            Assert.That(() => toggle.Toggle(), Throws.InstanceOf<ElementNotEnabledException>(), "A disabled element must refuse");
            Assert.That(left.IsChecked, Is.True);
        }
    }

    // Guard: a toggle button without a group still toggles off through UI Automation (WPF's own Toggle).
    [Test]
    public void Toggle_on_a_checked_ungrouped_ToggleButton_still_unchecks_it()
    {
        var bold = new Fluent.ToggleButton { Header = "Bold", IsChecked = true };

        using (new TestRibbonWindow(new StackPanel { Children = { bold } }))
        {
            UIHelper.DoEvents();

            var toggle = UIElementAutomationPeer.CreatePeerForElement(bold).GetPattern(PatternInterface.Toggle) as IToggleProvider;
            Assert.That(toggle, Is.Not.Null, "Precondition: the toggle button offers the Toggle pattern");

            toggle.Toggle();
            UIHelper.DoEvents();

            Assert.That(bold.IsChecked, Is.False, "An ungrouped toggle button must still toggle off");
        }
    }

    [Test]
    public void Toggle_on_a_checked_grouped_MenuItem_keeps_it_checked()
    {
        var left = new Fluent.MenuItem { Header = "Left", IsCheckable = true, GroupName = "Alignment", IsChecked = true };
        var right = new Fluent.MenuItem { Header = "Right", IsCheckable = true, GroupName = "Alignment" };

        using (new TestRibbonWindow(new StackPanel { Children = { left, right } }))
        {
            UIHelper.DoEvents();

            var toggle = UIElementAutomationPeer.CreatePeerForElement(left).GetPattern(PatternInterface.Toggle) as IToggleProvider;
            Assert.That(toggle, Is.Not.Null, "Precondition: the checkable menu item offers the Toggle pattern");

            toggle.Toggle();
            UIHelper.DoEvents();

            Assert.That(left.IsChecked, Is.True, "Like a click, UI Automation Toggle must keep a checked grouped menu item checked");
        }
    }
}
