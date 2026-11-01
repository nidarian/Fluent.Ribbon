namespace Fluent.Tests.Automation.Peers;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Gaps found by an accessibility (WCAG 2.1 AA, 4.1.2 name/role/value and 1.1.1) re-audit after round 7:
/// what a screen reader announces for controls that had no name or no state.
/// </summary>
[TestFixture]
public class AccessibilityNameAndStateTests
{
    /// <summary>
    /// The "Customize Status Bar" menu draws each entry's title in its template, but the menu item itself had no Header,
    /// so a screen reader announced every entry as "menu item, checked" with no label.
    /// </summary>
    [Test]
    public void StatusBar_menu_entries_are_named_after_their_item()
    {
        var item = new StatusBarItem { Title = "Word count", Value = "120" };
        var menuItem = new StatusBarMenuItem(item);

        var peer = UIElementAutomationPeer.CreatePeerForElement(menuItem);
        Assert.That(peer, Is.Not.Null, "Precondition: the menu item has an automation peer");
        Assert.That(peer.GetName(), Is.EqualTo("Word count"), "The entry must be announced with its item's title");

        item.Title = "Words";
        Assert.That(peer.GetName(), Is.EqualTo("Words"), "The name must follow the item's title");
    }

    /// <summary>
    /// A Spinner takes no focus itself; its text box does. The text box had no name, so a screen reader announced only "edit".
    /// </summary>
    [Test]
    public void Spinner_text_box_is_named_after_the_spinner()
    {
        var spinner = new Spinner { Header = "Zoom" };

        using (new TestRibbonWindow(spinner))
        {
            spinner.ApplyTemplate();
            UIHelper.DoEvents();

            var textBox = spinner.Template.FindName("PART_TextBox", spinner) as TextBox;
            Assert.That(textBox, Is.Not.Null, "Precondition: the template has PART_TextBox");

            var peer = UIElementAutomationPeer.CreatePeerForElement(textBox!);
            Assert.That(peer.GetName(), Is.EqualTo("Zoom"), "The focused text box must be announced with the spinner's header");
        }
    }

    /// <summary>
    /// A checkable SplitButton (like "Bold" with a drop down) didn't expose the Toggle pattern,
    /// so a screen reader couldn't tell whether it was on.
    /// </summary>
    [Test]
    public void Checkable_SplitButton_reports_its_checked_state()
    {
        var splitButton = new SplitButton { Header = "Bold", IsCheckable = true, IsChecked = true };

        using (new TestRibbonWindow(splitButton))
        {
            UIHelper.DoEvents();

            var peer = UIElementAutomationPeer.CreatePeerForElement(splitButton);
            var toggle = peer.GetPattern(PatternInterface.Toggle) as IToggleProvider;

            Assert.That(toggle, Is.Not.Null, "A checkable split button must expose the Toggle pattern");
            Assert.That(toggle!.ToggleState, Is.EqualTo(ToggleState.On), "Its state must be On while checked");

            toggle.Toggle();
            UIHelper.DoEvents();

            Assert.That(splitButton.IsChecked, Is.False, "Toggle must uncheck it");
            Assert.That(toggle.ToggleState, Is.EqualTo(ToggleState.Off), "Its state must be Off while unchecked");
        }
    }

    /// <summary>
    /// Guard: a split button that isn't checkable must not claim to be a toggle.
    /// </summary>
    [Test]
    public void Plain_SplitButton_is_not_a_toggle()
    {
        var splitButton = new SplitButton { Header = "Paste" };

        using (new TestRibbonWindow(splitButton))
        {
            UIHelper.DoEvents();

            var peer = UIElementAutomationPeer.CreatePeerForElement(splitButton);
            Assert.That(peer.GetPattern(PatternInterface.Toggle), Is.Null);
        }
    }
}
