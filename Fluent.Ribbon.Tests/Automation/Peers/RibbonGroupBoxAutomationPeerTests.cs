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
public class RibbonGroupBoxAutomationPeerTests
{
    /// <summary>
    /// A collapsed group is shown as a button with a drop down, and its peer offers the ExpandCollapse pattern.
    /// The reported state only looked at whether the group is collapsed, never at its drop down,
    /// so a screen reader said "collapsed" even while the drop down was open (and after Expand()).
    /// </summary>
    [Test]
    public void ExpandCollapseState_follows_the_drop_down()
    {
        var groupBox = new RibbonGroupBox
        {
            Header = "Group",
            Items = { new Fluent.Button { Header = "Button" } }
        };

        using (new TestRibbonWindow(groupBox))
        {
            UIHelper.DoEvents();

            // Set after loading: loading the content resets the state to the first one (Large).
            groupBox.State = RibbonGroupBoxState.Collapsed;
            UIHelper.DoEvents();
            Assert.That(groupBox.State, Is.EqualTo(RibbonGroupBoxState.Collapsed), "Precondition: the group is collapsed");

            var peer = UIElementAutomationPeer.CreatePeerForElement(groupBox);
            var expandCollapse = peer.GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
            Assert.That(expandCollapse, Is.Not.Null, "Precondition: a collapsed group offers ExpandCollapse");
            Assert.That(expandCollapse.ExpandCollapseState, Is.EqualTo(ExpandCollapseState.Collapsed));

            expandCollapse.Expand();
            UIHelper.DoEvents();
            Assert.That(groupBox.IsDropDownOpen, Is.True, "Precondition: Expand() opened the drop down");

            Assert.That(expandCollapse.ExpandCollapseState, Is.EqualTo(ExpandCollapseState.Expanded), "The state must say the drop down is open");

            expandCollapse.Collapse();
            UIHelper.DoEvents();
            Assert.That(groupBox.IsDropDownOpen, Is.False);
            Assert.That(expandCollapse.ExpandCollapseState, Is.EqualTo(ExpandCollapseState.Collapsed));
        }
    }

    /// <summary>
    /// The base walk listed the header TextBlock as a direct child of the group, and the header peer listed the same peer
    /// as its own child. From the second walk on the TextBlock's parent was the header peer, so UI Automation's next-sibling
    /// navigation stopped at it and the dialog launcher and the header dropped out of the tree: a screen reader saw them
    /// once, then never again.
    /// </summary>
    [Test]
    public void Every_walk_gives_each_child_one_parent_and_keeps_the_launcher()
    {
        var groupBox = new RibbonGroupBox
        {
            Header = "Group",
            IsLauncherVisible = true,
            LauncherToolTip = new ScreenTip { Title = "Launcher" },
            Items = { new Fluent.Button { Header = "Button" } }
        };

        using (new TestRibbonWindow(groupBox))
        {
            UIHelper.DoEvents();

            var peer = UIElementAutomationPeer.CreatePeerForElement(groupBox);

            for (var walk = 1; walk <= 3; walk++)
            {
                var all = new List<AutomationPeer>();
                CollectAndCheckParents(peer, all, walk);

                Assert.That(all.Count(x => x.GetAutomationId() == "PART_DialogLauncherButton"), Is.EqualTo(1), $"Walk {walk}: the launcher is in the tree once");
                Assert.That(all.Count(x => x is Fluent.Automation.Peers.RibbonGroupHeaderAutomationPeer), Is.EqualTo(1), $"Walk {walk}: the header is in the tree once");
                var twice = all.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => $"{g.Key.GetType().Name} '{g.Key.GetName()}' ({(g.Key as UIElementAutomationPeer)?.Owner.GetType().Name})");
                Assert.That(twice, Is.Empty, $"Walk {walk}: no peer is listed twice");
            }
        }
    }

    private static void CollectAndCheckParents(AutomationPeer parent, List<AutomationPeer> all, int walk)
    {
        foreach (var child in parent.GetChildren() ?? new List<AutomationPeer>())
        {
            Assert.That(child.GetParent(), Is.SameAs(parent), $"Walk {walk}: {child.GetType().Name} '{child.GetName()}' must have the peer that lists it as its parent");
            all.Add(child);
            CollectAndCheckParents(child, all, walk);
        }
    }
}
