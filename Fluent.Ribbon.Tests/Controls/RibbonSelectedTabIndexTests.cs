namespace Fluent.Tests.Controls;

using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// <see cref="Ribbon.SelectedTabIndex"/> must keep pointing at the selected tab when tabs are inserted or removed before it.
/// The tab control shifts its own selected index in that case without raising SelectionChanged,
/// so an index that's only updated from SelectionChanged goes stale.
/// </summary>
[TestFixture]
public class RibbonSelectedTabIndexTests
{
    [Test]
    public void SelectedTabIndex_follows_selected_tab_when_tab_is_inserted_before_it()
    {
        var firstTab = new RibbonTabItem { Header = "First" };
        var ribbon = new Ribbon
        {
            Tabs =
            {
                firstTab,
                new RibbonTabItem { Header = "Second" }
            }
        };

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();

            ribbon.SelectedTabItem = firstTab;
            UIHelper.DoEvents();

            Assert.That(ribbon.SelectedTabItem, Is.SameAs(firstTab), "Precondition: the first tab must be selected.");
            Assert.That(ribbon.SelectedTabIndex, Is.EqualTo(0), "Precondition: the selected index must be 0.");

            var newTab = new RibbonTabItem { Header = "New" };
            ribbon.Tabs.Insert(0, newTab);
            UIHelper.DoEvents();

            Assert.That(ribbon.SelectedTabItem, Is.SameAs(firstTab), "Inserting a tab must not change the selected tab.");
            Assert.That(ribbon.SelectedTabIndex, Is.EqualTo(ribbon.Tabs.IndexOf(firstTab)), "SelectedTabIndex must be the index of the selected tab after the insert.");

            // An app bound to SelectedTabIndex selects the new tab by setting the index to 0.
            ribbon.SelectedTabIndex = 0;
            UIHelper.DoEvents();

            Assert.That(ribbon.SelectedTabItem, Is.SameAs(newTab), "Setting SelectedTabIndex to 0 must select the inserted tab.");
            Assert.That(ribbon.TabControl.SelectedItem, Is.SameAs(newTab), "The tab control must show the inserted tab.");
        }
    }

    [Test]
    public void SelectedTabIndex_follows_selected_tab_when_tab_before_it_is_removed()
    {
        var firstTab = new RibbonTabItem { Header = "First" };
        var lastTab = new RibbonTabItem { Header = "Last" };
        var ribbon = new Ribbon
        {
            Tabs =
            {
                firstTab,
                new RibbonTabItem { Header = "Second" },
                lastTab
            }
        };

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();

            ribbon.SelectedTabItem = lastTab;
            UIHelper.DoEvents();

            Assert.That(ribbon.SelectedTabItem, Is.SameAs(lastTab), "Precondition: the last tab must be selected.");
            Assert.That(ribbon.SelectedTabIndex, Is.EqualTo(2), "Precondition: the selected index must be 2.");

            ribbon.Tabs.Remove(firstTab);
            UIHelper.DoEvents();

            Assert.That(ribbon.SelectedTabItem, Is.SameAs(lastTab), "Removing an earlier tab must not change the selected tab.");
            Assert.That(ribbon.SelectedTabIndex, Is.EqualTo(ribbon.Tabs.IndexOf(lastTab)), "SelectedTabIndex must be the index of the selected tab after the removal, not past the end.");
        }
    }
}
