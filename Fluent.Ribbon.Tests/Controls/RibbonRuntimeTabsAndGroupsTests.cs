namespace Fluent.Tests.Controls;

using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Apps that build their ribbon from data change tabs and groups while running: contextual tabs are taken out and put back,
/// their groups rebuilt in between, and all tabs are replaced when the document type changes. Found by a bug hunt after round 7.
/// The ribbons here turn <see cref="Ribbon.AutomaticStateManagement"/> off, so they don't load a state saved by another test.
/// </summary>
[TestFixture]
public class RibbonRuntimeTabsAndGroupsTests
{
    /// <summary>
    /// A tab stopped listening to its Groups while it was out of the ribbon (unloaded) and didn't catch up when it came back,
    /// so a group added in between was in Groups but never shown.
    /// </summary>
    [Test]
    public void Group_added_while_the_tab_is_out_of_the_ribbon_is_shown_when_it_comes_back()
    {
        var tab = new RibbonTabItem { Header = "Picture" };
        var ribbon = new Ribbon { AutomaticStateManagement = false };
        ribbon.Tabs.Add(new RibbonTabItem { Header = "Home" });
        ribbon.Tabs.Add(tab);

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            ribbon.Tabs.Remove(tab);
            UIHelper.DoEvents();
            Assert.That(tab.IsLoaded, Is.False, "Precondition: the removed tab is unloaded");

            var group = new RibbonGroupBox { Header = "Adjust" };
            tab.Groups.Add(group);

            ribbon.Tabs.Add(tab);
            UIHelper.DoEvents();

            Assert.That(GroupsPanel(tab).Children.Contains(group), Is.True, "The group added while the tab was out of the ribbon must be shown");
        }
    }

    /// <summary>
    /// The same while a group moves from a tab that is out of the ribbon to another tab: the old tab still held the group
    /// in its panel, so adding it to the other tab threw ("already the logical child of another element").
    /// </summary>
    [Test]
    public void Group_moved_from_a_tab_that_is_out_of_the_ribbon_to_another_tab()
    {
        var group = new RibbonGroupBox { Header = "Shared" };
        var tabA = new RibbonTabItem { Header = "A", Groups = { group } };
        var tabB = new RibbonTabItem { Header = "B" };
        var ribbon = new Ribbon { AutomaticStateManagement = false };
        ribbon.Tabs.Add(tabA);
        ribbon.Tabs.Add(tabB);

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            ribbon.Tabs.Remove(tabA);
            UIHelper.DoEvents();
            Assert.That(tabA.IsLoaded, Is.False, "Precondition: the removed tab is unloaded");

            tabA.Groups.Remove(group);

            Assert.That(() => tabB.Groups.Add(group), Throws.Nothing, "Moving a group to another tab must not throw");
            UIHelper.DoEvents();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(GroupsPanel(tabA).Children.Contains(group), Is.False, "The old tab must no longer hold the group");
                Assert.That(GroupsPanel(tabB).Children.Contains(group), Is.True, "The new tab must show the group");
            }
        }
    }

    /// <summary>
    /// Replacing all tabs (Clear, then Add) left no tab selected and the ribbon's content empty:
    /// only removing a single tab selected another one.
    /// </summary>
    [Test]
    public void Replacing_all_tabs_selects_a_new_tab()
    {
        var ribbon = new Ribbon { AutomaticStateManagement = false };
        ribbon.Tabs.Add(new RibbonTabItem { Header = "A" });
        ribbon.Tabs.Add(new RibbonTabItem { Header = "B" });

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();
            Assert.That(ribbon.SelectedTabItem, Is.Not.Null, "Precondition: a tab is selected");

            ribbon.Tabs.Clear();
            UIHelper.DoEvents();

            var c = new RibbonTabItem { Header = "C" };
            ribbon.Tabs.Add(c);
            ribbon.Tabs.Add(new RibbonTabItem { Header = "D" });
            UIHelper.DoEvents();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(ribbon.SelectedTabItem, Is.SameAs(c), "The first new tab must be selected");
                Assert.That(ribbon.TabControl!.SelectedContent, Is.Not.Null, "The ribbon must show the selected tab's groups");
            }
        }
    }

    private static Panel GroupsPanel(RibbonTabItem tab)
    {
        var panel = tab.GroupsContainer.Content as Panel;
        Assert.That(panel, Is.Not.Null, "Precondition: the tab's groups container holds its groups panel");
        return panel!;
    }
}
