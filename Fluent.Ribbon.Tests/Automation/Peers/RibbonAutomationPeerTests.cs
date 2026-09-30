namespace Fluent.Tests.Automation.Peers;

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
}
