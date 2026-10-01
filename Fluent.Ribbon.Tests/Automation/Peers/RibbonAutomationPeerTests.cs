namespace Fluent.Tests.Automation.Peers;

using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Fluent.Automation.Peers;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class RibbonAutomationPeerTests
{
    /// <summary>
    /// The ribbon's peer adds its menu (the backstage) and then all children of the tab control's peer,
    /// which already start with the same menu. Screen readers listed "File" twice.
    /// </summary>
    [Test]
    public void The_menu_is_listed_once()
    {
        var ribbon = new Ribbon
        {
            Menu = new Backstage { Header = "File" },
            Tabs = { new RibbonTabItem { Header = "Home" } }
        };

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            var children = UIElementAutomationPeer.CreatePeerForElement(ribbon).GetChildren();
            Assert.That(children, Is.Not.Null.And.Not.Empty, "Precondition");

            Assert.That(children.Count(x => x is RibbonBackstageAutomationPeer), Is.EqualTo(1), "The backstage must appear once");
            Assert.That(children.Count, Is.EqualTo(children.Distinct().Count()), "No peer may be listed twice");
        }
    }

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
