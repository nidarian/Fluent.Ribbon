namespace Fluent.Tests.Automation.Peers;

using System.Linq;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Fluent.Automation.Peers;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// A tab's <see cref="RibbonTabItemDataAutomationPeer"/> implements <see cref="IScrollItemProvider"/> (bring the tab into view when
/// the tabs don't fit), but a screen reader or UI test can only use it if GetPattern offers it.
/// </summary>
[TestFixture]
public class RibbonTabItemDataAutomationPeerScrollItemTests
{
    [Test]
    public void ScrollItem_pattern_is_reachable()
    {
        var ribbon = new Ribbon();
        var tab = new RibbonTabItem { Header = "Insert" };
        ribbon.Tabs.Add(new RibbonTabItem { Header = "Home" });
        ribbon.Tabs.Add(tab);

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();
            UIHelper.DoEvents();
            Assert.That(ribbon.TabControl, Is.Not.Null, "Precondition: the ribbon has its tab control");

            var tabControlPeer = UIElementAutomationPeer.CreatePeerForElement(ribbon.TabControl);
            var dataPeer = tabControlPeer.GetChildren().OfType<RibbonTabItemDataAutomationPeer>().FirstOrDefault(x => ReferenceEquals(x.Item, tab));
            Assert.That(dataPeer, Is.Not.Null, "Precondition: the tab control peer exposes a data peer for the tab");

            Assert.That(dataPeer.GetPattern(PatternInterface.ScrollItem), Is.SameAs(dataPeer), "A screen reader must be able to get the ScrollItem pattern of a tab");
        }
    }
}
