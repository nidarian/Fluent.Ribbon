namespace Fluent.Tests.Automation.Peers;

using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// With <see cref="Ribbon.CanMinimize"/> off, a user can't minimize the ribbon (the tab double-click, the display options menu and
/// Ctrl+F1 ignore it), so UI Automation Collapse must not minimize it either. Restoring stays possible through UI Automation:
/// a mouse user can still restore through a contextual group header, and a screen reader user must not be stuck.
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

    // Default (noted in the round notes): restoring is not blocked.
    [Test]
    public void Expand_still_restores_when_CanMinimize_is_off()
    {
        var ribbon = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            Prepare(ribbon, isMinimized: true);

            GetExpandCollapse(ribbon).Expand();
            UIHelper.DoEvents();

            Assert.That(ribbon.IsMinimized, Is.False, "A screen reader user must still be able to restore a minimized ribbon");
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
