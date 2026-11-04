namespace Fluent.Tests.Automation.Peers;

using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// With <see cref="Ribbon.CanMinimize"/> off, a user can't minimize or restore the ribbon: the tab double-click and the display
/// options menu ignore it. UI Automation Expand/Collapse on the ribbon must follow the same rule.
/// </summary>
[TestFixture]
public class RibbonAutomationPeerCanMinimizeTests
{
    [Test]
    public void Collapse_does_not_minimize_when_CanMinimize_is_off()
    {
        var ribbon = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            Prepare(ribbon, isMinimized: false);

            GetExpandCollapse(ribbon).Collapse();
            UIHelper.DoEvents();

            Assert.That(ribbon.IsMinimized, Is.False, "UI Automation must not minimize a ribbon whose CanMinimize is off");
        }
    }

    [Test]
    public void Expand_does_not_restore_when_CanMinimize_is_off()
    {
        var ribbon = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            Prepare(ribbon, isMinimized: true);

            GetExpandCollapse(ribbon).Expand();
            UIHelper.DoEvents();

            Assert.That(ribbon.IsMinimized, Is.True, "UI Automation must not restore a ribbon whose CanMinimize is off, like the tab double-click");
        }
    }

    private static Ribbon CreateRibbon()
    {
        var ribbon = new Ribbon { AutomaticStateManagement = false };
        ribbon.Tabs.Add(new RibbonTabItem { Header = "Home" });
        return ribbon;
    }

    // The app minimizes (or not) while CanMinimize is on, then turns CanMinimize off.
    private static void Prepare(Ribbon ribbon, bool isMinimized)
    {
        ribbon.ApplyTemplate();
        UIHelper.DoEvents();
        ribbon.IsMinimized = isMinimized;
        ribbon.CanMinimize = false;
        UIHelper.DoEvents();
        Assert.That(ribbon.IsMinimized, Is.EqualTo(isMinimized), "Precondition: the ribbon's starting state");
        Assert.That(ribbon.CanMinimize, Is.False, "Precondition: CanMinimize is off");
    }

    private static IExpandCollapseProvider GetExpandCollapse(Ribbon ribbon)
    {
        var expandCollapse = UIElementAutomationPeer.CreatePeerForElement(ribbon).GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
        Assert.That(expandCollapse, Is.Not.Null, "Precondition: the ribbon offers ExpandCollapse");
        return expandCollapse;
    }
}
