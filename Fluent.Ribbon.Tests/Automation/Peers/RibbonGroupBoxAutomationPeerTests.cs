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
}
