namespace Fluent.Tests.Automation.Peers;

using System.Windows.Automation.Peers;
using Fluent.Automation.Peers;
using NUnit.Framework;

/// <summary>
/// Screen readers announce the (localized) control type of an element, e.g. "button".
/// A Custom control type with the class name as "localized" type made DropDownButton read as "DropDownButton" in every language.
/// </summary>
[TestFixture]
public class RibbonDropDownButtonAutomationPeerTests
{
    [Test]
    public void DropDownButton_should_report_Button_control_type()
    {
        var control = new DropDownButton { Header = "DropDown" };

        var peer = UIElementAutomationPeer.CreatePeerForElement(control);

        Assert.That(peer, Is.InstanceOf<RibbonDropDownButtonAutomationPeer>(), "Precondition: DropDownButton must create a RibbonDropDownButtonAutomationPeer.");

        Assert.That(peer.GetAutomationControlType(), Is.EqualTo(AutomationControlType.Button), "DropDownButton must be reported as a Button.");
        Assert.That(peer.GetLocalizedControlType(), Is.Not.EqualTo("DropDownButton").And.Not.Empty, "The localized control type must come from WPF, not be the class name.");
    }

    // SplitButton's peer derives from the DropDownButton peer, overrides the control type itself, but inherited the class name as localized control type.
    [Test]
    public void SplitButton_should_keep_SplitButton_control_type_with_localized_name()
    {
        var control = new SplitButton { Header = "Split" };

        var peer = UIElementAutomationPeer.CreatePeerForElement(control);

        Assert.That(peer, Is.InstanceOf<Fluent.Automation.Peers.RibbonSplitButtonAutomationPeer>(), "Precondition: SplitButton must create a RibbonSplitButtonAutomationPeer.");

        Assert.That(peer.GetAutomationControlType(), Is.EqualTo(AutomationControlType.SplitButton), "SplitButton must still be reported as a SplitButton.");
        Assert.That(peer.GetLocalizedControlType(), Is.Not.EqualTo("SplitButton").And.Not.Empty, "The localized control type must come from WPF, not be the class name.");
    }
}