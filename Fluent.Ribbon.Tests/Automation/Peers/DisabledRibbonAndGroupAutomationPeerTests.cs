namespace Fluent.Tests.Automation.Peers;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// UI Automation says a disabled element must refuse actions with ElementNotEnabledException, as the drop down, backstage and
/// SplitButton peers do (see DisabledControlsAutomationPeerTests). The ribbon's own Expand/Collapse (restore, minimize) and a
/// collapsed group's Expand/Collapse (its drop down) acted anyway.
/// </summary>
[TestFixture]
public class DisabledRibbonAndGroupAutomationPeerTests
{
    [Test]
    public void Ribbon_ExpandCollapse_throws_when_disabled()
    {
        var ribbon = new Ribbon { CanMinimize = true, AutomaticStateManagement = false };
        ribbon.Tabs.Add(new RibbonTabItem { Header = "Home" });

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();
            UIHelper.DoEvents();
            ribbon.IsEnabled = false;
            UIHelper.DoEvents();
            Assert.That(ribbon.IsMinimized, Is.False, "Precondition: the ribbon is not minimized");

            var expandCollapse = UIElementAutomationPeer.CreatePeerForElement(ribbon).GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
            Assert.That(expandCollapse, Is.Not.Null, "Precondition");

            Assert.That(() => expandCollapse.Collapse(), Throws.InstanceOf<ElementNotEnabledException>());
            Assert.That(ribbon.IsMinimized, Is.False, "A disabled ribbon must not be minimized");
            Assert.That(() => expandCollapse.Expand(), Throws.InstanceOf<ElementNotEnabledException>());
        }
    }

    [Test]
    public void Collapsed_group_ExpandCollapse_throws_when_disabled()
    {
        var group = new RibbonGroupBox { Header = "Group", State = RibbonGroupBoxState.Collapsed, IsEnabled = false };

        var expandCollapse = UIElementAutomationPeer.CreatePeerForElement(group).GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
        Assert.That(expandCollapse, Is.Not.Null, "Precondition: a collapsed group offers ExpandCollapse");

        Assert.That(() => expandCollapse.Expand(), Throws.InstanceOf<ElementNotEnabledException>());
        Assert.That(group.IsDropDownOpen, Is.False, "A disabled group must not open its drop down");
        Assert.That(() => expandCollapse.Collapse(), Throws.InstanceOf<ElementNotEnabledException>());
    }
}
