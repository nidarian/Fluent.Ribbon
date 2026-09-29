namespace Fluent.Tests.Controls;

using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// https://github.com/fluentribbon/Fluent.Ribbon/issues/1176
/// Dragging a finger across the ribbon didn't scroll the groups, only the mouse wheel did.
/// A ScrollViewer ignores touch unless PanningMode is set.
/// </summary>
[TestFixture]
public class RibbonGroupsContainerScrollViewerTests
{
    [Test]
    public void Ribbon_groups_can_be_scrolled_with_touch()
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
            Assert.That(scrollViewer.PanningMode, Is.EqualTo(PanningMode.HorizontalOnly), "Groups only scroll horizontally, so only horizontal drags should pan");

            // PanningMode alone isn't enough: ScrollViewer has to switch on manipulation (touch) events.
            // It only does that when PanningMode changes, which is why it's set in the style.
            Assert.That(scrollViewer.IsManipulationEnabled, Is.True, "Touch manipulation must be enabled for panning to work");
        }
    }
}
