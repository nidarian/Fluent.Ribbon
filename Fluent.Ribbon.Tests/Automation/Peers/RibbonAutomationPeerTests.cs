namespace Fluent.Tests.Automation.Peers;

using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation.Peers;
using Fluent.Automation.Peers;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// When the quick access toolbar is shown above the ribbon it lives in the window's title bar.
/// Screen readers walk the automation tree, so the toolbar must appear there exactly once,
/// otherwise users hear (and navigate through) every quick access item twice.
/// </summary>
[TestFixture]
public class RibbonAutomationPeerTests
{
    [Test]
    public void QuickAccessToolBar_shown_above_ribbon_is_listed_once_in_the_automation_tree()
    {
        var ribbon = new Ribbon
        {
            ShowQuickAccessToolBarAboveRibbon = true
        };

        using (var window = new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();
            window.UpdateLayout();
            UIHelper.DoEvents();

            Assert.That(ribbon.QuickAccessToolBar, Is.Not.Null, "Precondition: the ribbon template provides a quick access toolbar");
            Assert.That(window.TitleBar, Is.Not.Null, "Precondition: the window template provides a title bar");
            Assert.That(window.TitleBar!.QuickAccessToolBar, Is.SameAs(ribbon.QuickAccessToolBar), "Precondition: the quick access toolbar is moved to the title bar");
            Assert.That(ribbon.QuickAccessToolBar!.IsDescendantOf(window.TitleBar), Is.True, "Precondition: the title bar hosts the quick access toolbar visually");

            var windowPeer = UIElementAutomationPeer.CreatePeerForElement(window);
            Assert.That(windowPeer, Is.Not.Null, "Precondition");

            var quickAccessToolBarPeers = GetDescendants(windowPeer).OfType<Fluent.Automation.Peers.RibbonQuickAccessToolBarAutomationPeer>().ToList();

            Assert.That(quickAccessToolBarPeers, Has.Count.EqualTo(1), "The quick access toolbar must be listed exactly once in the automation tree");
        }
    }

    // Walks the raw automation tree (all peers, not just control/content view) depth first.
    private static IEnumerable<AutomationPeer> GetDescendants(AutomationPeer peer)
    {
        var children = peer.GetChildren();

        if (children is null)
        {
            yield break;
        }

        foreach (var child in children)
        {
            yield return child;

            foreach (var descendant in GetDescendants(child))
            {
                yield return descendant;
            }
        }
    }
}
