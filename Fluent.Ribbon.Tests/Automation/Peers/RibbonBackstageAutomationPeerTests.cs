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
public class RibbonBackstageAutomationPeerTests
{
    /// <summary>
    /// The backstage button ("File") shows its Header as text, but its peer had no name:
    /// Backstage is a plain Control, so WPF finds no text to use. Narrator read only "menu".
    /// </summary>
    [Test]
    public void Name_comes_from_the_header()
    {
        var backstage = new Backstage { Header = "File" };
        var ribbon = new Ribbon { Menu = backstage };

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            var peer = UIElementAutomationPeer.CreatePeerForElement(backstage);
            Assert.That(peer, Is.InstanceOf<RibbonBackstageAutomationPeer>(), "Precondition");

            Assert.That(peer.GetName(), Is.EqualTo("File"));
        }
    }

    [Test]
    public void AutomationProperties_Name_still_wins()
    {
        var backstage = new Backstage { Header = "File" };
        AutomationProperties.SetName(backstage, "Start menu");

        Assert.That(UIElementAutomationPeer.CreatePeerForElement(backstage).GetName(), Is.EqualTo("Start menu"));
    }
}
