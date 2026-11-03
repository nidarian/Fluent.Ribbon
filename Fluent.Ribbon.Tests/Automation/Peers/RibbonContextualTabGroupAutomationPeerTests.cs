namespace Fluent.Tests.Automation.Peers;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using NUnit.Framework;

/// <summary>
/// The header of a contextual tab group ("Table Tools") is a TextBlock in the template, which UI Automation hides from the control view,
/// and the group itself had no automation peer, so a screen reader never announced the context of its tabs (WCAG 1.3.1).
/// Found by the accessibility re-audit of round 8.
/// </summary>
[TestFixture]
public class RibbonContextualTabGroupAutomationPeerTests
{
    [Test]
    public void Contextual_tab_group_is_announced_with_its_header()
    {
        var group = new RibbonContextualTabGroup { Header = "Table Tools" };

        var peer = UIElementAutomationPeer.CreatePeerForElement(group);

        Assert.That(peer, Is.Not.Null, "The contextual tab group must have an automation peer");
        Assert.That(peer.GetType().Namespace, Is.EqualTo("Fluent.Automation.Peers"), "It must be one of our automation peers");
        Assert.That(peer.GetName(), Is.EqualTo("Table Tools"), "It must be announced with its header");
        Assert.That(peer.GetAutomationControlType(), Is.EqualTo(AutomationControlType.Group));
    }

    [Test]
    public void An_explicit_AutomationProperties_Name_wins_over_the_header()
    {
        var group = new RibbonContextualTabGroup { Header = "Table Tools" };
        AutomationProperties.SetName(group, "Table");

        var peer = UIElementAutomationPeer.CreatePeerForElement(group);

        Assert.That(peer, Is.Not.Null, "The contextual tab group must have an automation peer");
        Assert.That(peer.GetName(), Is.EqualTo("Table"));
    }
}
