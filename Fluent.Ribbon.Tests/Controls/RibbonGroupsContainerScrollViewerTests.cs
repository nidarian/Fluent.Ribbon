namespace Fluent.Tests.Controls;

using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// https://github.com/fluentribbon/Fluent.Ribbon/issues/1176
/// Dragging a finger across the ribbon didn't scroll the groups, only the mouse wheel did.
/// A ScrollViewer ignores touch unless PanningMode is set.
/// Touch panning is opt-in: it's off by default (as upstream), because a horizontal touch drag on a slider
/// inside the ribbon might scroll the ribbon instead. An app turns it on with
/// <c>ScrollViewer.PanningMode="HorizontalOnly"</c> on the <see cref="Ribbon" />.
/// </summary>
[TestFixture]
public class RibbonGroupsContainerScrollViewerTests
{
    [Test]
    public void Ribbon_groups_are_not_scrolled_with_touch_by_default()
    {
        var tabItem = new RibbonTabItem { Header = "Home" };
        tabItem.Groups.Add(new RibbonGroupBox { Header = "Group" });

        var ribbon = new Ribbon();
        ribbon.Tabs.Add(tabItem);

        using (new TestRibbonWindow(ribbon))
        {
            // Make sure the tab (and so its groups container) is shown and has its style applied.
            ribbon.SelectedTabItem = tabItem;
            UIHelper.DoEvents();

            // GroupsContainer is the RibbonGroupsContainerScrollViewer that hosts the groups of the tab.
            var scrollViewer = tabItem.GroupsContainer;

            Assert.That(scrollViewer, Is.InstanceOf<RibbonGroupsContainerScrollViewer>());
            Assert.That(scrollViewer.PanningMode, Is.EqualTo(PanningMode.None), "Touch panning must be off unless the app opts in");
            Assert.That(scrollViewer.IsManipulationEnabled, Is.False, "Touch manipulation must stay off unless the app opts in");
        }
    }

    [Test]
    public void Ribbon_groups_can_be_scrolled_with_touch_when_the_app_opts_in()
    {
        var tabItem = new RibbonTabItem { Header = "Home" };
        tabItem.Groups.Add(new RibbonGroupBox { Header = "Group" });

        var ribbon = new Ribbon();
        ribbon.Tabs.Add(tabItem);

        // The opt-in: the same as ScrollViewer.PanningMode="HorizontalOnly" on the Ribbon in XAML.
        ScrollViewer.SetPanningMode(ribbon, PanningMode.HorizontalOnly);

        using (new TestRibbonWindow(ribbon))
        {
            // Make sure the tab (and so its groups container) is shown and has its style applied.
            ribbon.SelectedTabItem = tabItem;
            UIHelper.DoEvents();

            // GroupsContainer is the RibbonGroupsContainerScrollViewer that hosts the groups of the tab.
            var scrollViewer = tabItem.GroupsContainer;

            Assert.That(scrollViewer, Is.InstanceOf<RibbonGroupsContainerScrollViewer>());
            Assert.That(scrollViewer.PanningMode, Is.EqualTo(PanningMode.HorizontalOnly), "Groups only scroll horizontally, so only horizontal drags should pan");

            // PanningMode alone isn't enough: ScrollViewer has to switch on manipulation (touch) events.
            // It only does that when PanningMode changes.
            Assert.That(scrollViewer.IsManipulationEnabled, Is.True, "Touch manipulation must be enabled for panning to work");
        }
    }
}
