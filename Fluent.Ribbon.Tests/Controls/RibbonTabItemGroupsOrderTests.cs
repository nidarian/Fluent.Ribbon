namespace Fluent.Tests.Controls;

using System.Linq;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// The groups shown in a tab must stay in the order of <see cref="RibbonTabItem.Groups"/>.
/// </summary>
/// <remarks>
/// Before the fix, the tab ignored Move notifications (Groups.Move) and handled Replace (Groups[i] = x)
/// by adding the new group at the end, so the groups were shown in another order than Groups.
/// </remarks>
[TestFixture]
public class RibbonTabItemGroupsOrderTests
{
    [Test]
    public void Move_should_show_the_groups_in_the_new_order()
    {
        var groupA = new RibbonGroupBox { Header = "A" };
        var groupB = new RibbonGroupBox { Header = "B" };
        var groupC = new RibbonGroupBox { Header = "C" };

        var tabItem = new RibbonTabItem { Header = "Home" };
        tabItem.Groups.Add(groupA);
        tabItem.Groups.Add(groupB);
        tabItem.Groups.Add(groupC);

        var ribbon = new Ribbon();
        ribbon.Tabs.Add(tabItem);

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.SelectedTabItem = tabItem;
            UIHelper.DoEvents();

            tabItem.Groups.Move(0, 2);
            UIHelper.DoEvents();

            Assert.That(GetShownGroupHeaders(tabItem), Is.EqualTo(new[] { "B", "C", "A" }), "after Groups.Move(0, 2)");

            tabItem.Groups.Move(2, 1);
            UIHelper.DoEvents();

            Assert.That(GetShownGroupHeaders(tabItem), Is.EqualTo(new[] { "B", "A", "C" }), "after Groups.Move(2, 1)");
        }
    }

    [Test]
    public void Replace_should_show_the_new_group_at_the_same_index()
    {
        var groupA = new RibbonGroupBox { Header = "A" };
        var groupB = new RibbonGroupBox { Header = "B" };
        var groupC = new RibbonGroupBox { Header = "C" };

        var tabItem = new RibbonTabItem { Header = "Home" };
        tabItem.Groups.Add(groupA);
        tabItem.Groups.Add(groupB);
        tabItem.Groups.Add(groupC);

        var ribbon = new Ribbon();
        ribbon.Tabs.Add(tabItem);

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.SelectedTabItem = tabItem;
            UIHelper.DoEvents();

            tabItem.Groups[0] = new RibbonGroupBox { Header = "D" };
            UIHelper.DoEvents();

            Assert.That(GetShownGroupHeaders(tabItem), Is.EqualTo(new[] { "D", "B", "C" }), "after Groups[0] = D");

            tabItem.Groups[1] = new RibbonGroupBox { Header = "E" };
            UIHelper.DoEvents();

            Assert.That(GetShownGroupHeaders(tabItem), Is.EqualTo(new[] { "D", "E", "C" }), "after Groups[1] = E");
        }
    }

    // The panel inside GroupsContainer is the one that shows the groups (and lays them out from left to right).
    private static string[] GetShownGroupHeaders(RibbonTabItem tabItem)
    {
        var panel = (Panel)tabItem.GroupsContainer.Content;

        return panel.Children
            .OfType<RibbonGroupBox>()
            .Select(x => x.Header.ToString())
            .ToArray();
    }
}
