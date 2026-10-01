namespace Fluent.Tests.Automation.Peers;

using System.Linq;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Backstage tabs behave like tabs, so UI Automation clients (screen readers, UI tests) expect them to be
/// exposed as tab items that can be selected through the SelectionItem pattern.
/// </summary>
[TestFixture]
public class RibbonBackstageTabControlAutomationPeerTests
{
    [Test]
    public void BackstageTabItem_is_a_selectable_tab_item()
    {
        var control = new BackstageTabControl();
        var newItem = new BackstageTabItem { Header = "New" };
        var openItem = new BackstageTabItem { Header = "Open" };
        control.Items.Add(newItem);
        control.Items.Add(openItem);

        using (new TestRibbonWindow(control))
        {
            UIHelper.DoEvents();

            Assert.That(openItem.IsSelected, Is.False, "Precondition: \"Open\" is not selected yet");

            var peer = UIElementAutomationPeer.CreatePeerForElement(control);
            Assert.That(peer, Is.Not.Null, "Precondition");

            var openItemPeer = peer.GetChildren().OfType<ItemAutomationPeer>().FirstOrDefault(x => ReferenceEquals(x.Item, openItem));
            Assert.That(openItemPeer, Is.Not.Null, "Precondition: the tab control exposes a peer for \"Open\"");

            Assert.That(openItemPeer!.GetAutomationControlType(), Is.EqualTo(AutomationControlType.TabItem), "A backstage tab must be exposed as a tab item");

            var selectionItem = openItemPeer.GetPattern(PatternInterface.SelectionItem) as ISelectionItemProvider;
            Assert.That(selectionItem, Is.Not.Null, "A backstage tab must support the SelectionItem pattern");

            selectionItem!.Select();
            UIHelper.DoEvents();

            Assert.That(openItem.IsSelected, Is.True, "Select() must select the tab");
            Assert.That(control.SelectedItem, Is.SameAs(openItem));
            Assert.That(selectionItem.IsSelected, Is.True);
        }
    }
}
