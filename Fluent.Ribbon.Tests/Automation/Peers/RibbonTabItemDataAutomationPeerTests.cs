namespace Fluent.Tests.Automation.Peers;

using System.Linq;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using Fluent.Automation.Peers;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Screen readers announce a tab by the name of its <see cref="RibbonTabItemDataAutomationPeer"/>.
/// An explicit AutomationProperties.Name is the app author's deliberate choice and must win over the Header text.
/// </summary>
[TestFixture]
public class RibbonTabItemDataAutomationPeerTests
{
    [Test]
    public void Name_should_be_AutomationProperties_Name_when_set()
    {
        var tab = new RibbonTabItem { Header = "Home" };
        AutomationProperties.SetName(tab, "Home tab");

        Assert.That(GetDataPeerName(tab), Is.EqualTo("Home tab"), "AutomationProperties.Name must take precedence over the Header.");
    }

    [Test]
    public void Name_should_be_Header_when_AutomationProperties_Name_is_not_set()
    {
        var tab = new RibbonTabItem { Header = "Home" };

        Assert.That(GetDataPeerName(tab), Is.EqualTo("Home"), "Without AutomationProperties.Name the string Header must be used.");
    }

    private static string GetDataPeerName(RibbonTabItem tab)
    {
        var ribbon = new Ribbon();
        ribbon.Tabs.Add(tab);

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();
            UIHelper.DoEvents();

            Assert.That(ribbon.TabControl, Is.Not.Null, "Precondition: the ribbon template must create its RibbonTabControl.");

            // The data peer is created by the tab control's peer for each item, so we have to go through its children.
            var tabControlPeer = UIElementAutomationPeer.CreatePeerForElement(ribbon.TabControl);
            Assert.That(tabControlPeer, Is.InstanceOf<RibbonTabControlAutomationPeer>(), "Precondition: RibbonTabControl must create a RibbonTabControlAutomationPeer.");

            var dataPeer = tabControlPeer.GetChildren().OfType<RibbonTabItemDataAutomationPeer>().FirstOrDefault(x => ReferenceEquals(x.Item, tab));
            Assert.That(dataPeer, Is.Not.Null, "Precondition: the tab control peer must expose a RibbonTabItemDataAutomationPeer for the tab.");

            return dataPeer.GetName();
        }
    }
}