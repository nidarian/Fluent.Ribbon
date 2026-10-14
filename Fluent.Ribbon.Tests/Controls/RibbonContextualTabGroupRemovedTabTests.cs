namespace Fluent.Tests.Controls;

using System;
using System.Windows;
using System.Windows.Media;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// An app shows the contextual group headers (<see cref="RibbonTitleBar.HideContextTabs"/> = false) and removes contextual tabs
/// from <see cref="Ribbon.Tabs"/> while their <see cref="RibbonContextualTabGroup"/> stays visible.
/// The removed tab kept its <see cref="RibbonTabItem.Group"/>, so it stayed in <see cref="RibbonContextualTabGroup.Items"/> and was still counted as a visible tab of the group.
/// The title bar then measured the header from the removed tab: its position relative to the title bar is (0, 0) because it is no longer in the window,
/// so the contextual headers were drawn from the left edge of the title bar (over the quick access toolbar) instead of above their tabs,
/// and a group whose tabs were all removed still showed its header.
/// </summary>
[TestFixture]
public class RibbonContextualTabGroupRemovedTabTests
{
    private const double Tolerance = 1;

    [Test]
    public void Header_stays_above_the_remaining_tab_when_the_first_tab_of_the_group_is_removed()
    {
        var setup = new Setup();

        using (var window = new TestRibbonWindow(setup.Ribbon))
        {
            ShowContextualHeaders(setup);

            Assert.That(GetX(setup.GroupA, window), Is.EqualTo(GetX(setup.TabA1, window)).Within(Tolerance), "Precondition: the header of group A starts above its first tab. " + Describe(setup, window));
            Assert.That(GetX(setup.TabA2, window) - GetX(setup.TabA1, window), Is.GreaterThan(10), "Precondition: the tabs of group A are laid out side by side");

            setup.Ribbon.Tabs.Remove(setup.TabA1);
            UIHelper.DoEvents();
            UIHelper.DoEvents();

            Assert.That(setup.GroupA.InnerVisibility, Is.EqualTo(Visibility.Visible), "Group A still has a tab in the ribbon, so its header stays visible");
            Assert.That(GetX(setup.GroupA, window), Is.EqualTo(GetX(setup.TabA2, window)).Within(Tolerance), "The header of group A must start above its remaining tab, not at the left edge of the title bar");
            Assert.That(setup.GroupA.ActualWidth, Is.EqualTo(setup.TabA2.ActualWidth).Within(Tolerance), "The header of group A must be as wide as its remaining tab, not include the removed tab");
        }
    }

    [Test]
    public void Header_is_hidden_when_all_tabs_of_the_group_are_removed()
    {
        var setup = new Setup();

        using (var window = new TestRibbonWindow(setup.Ribbon))
        {
            ShowContextualHeaders(setup);

            Assert.That(setup.GroupB.InnerVisibility, Is.EqualTo(Visibility.Visible), "Precondition: the header of group B is shown");
            Assert.That(GetX(setup.GroupB, window), Is.EqualTo(GetX(setup.TabB1, window)).Within(Tolerance), "Precondition: the header of group B starts above its tab. " + Describe(setup, window));

            setup.Ribbon.Tabs.Remove(setup.TabB1);
            UIHelper.DoEvents();
            UIHelper.DoEvents();

            Assert.That(setup.GroupB.InnerVisibility, Is.EqualTo(Visibility.Collapsed), "Group B has no tab left in the ribbon, so its header must not be shown");
            Assert.That(GetX(setup.GroupA, window), Is.EqualTo(GetX(setup.TabA1, window)).Within(Tolerance), "The header of group A must stay above its first tab");
        }
    }

    [Test]
    public void Headers_come_back_when_the_removed_tabs_are_added_again()
    {
        var setup = new Setup();

        using (var window = new TestRibbonWindow(setup.Ribbon))
        {
            ShowContextualHeaders(setup);

            setup.Ribbon.Tabs.Remove(setup.TabA1);
            setup.Ribbon.Tabs.Remove(setup.TabB1);
            UIHelper.DoEvents();
            UIHelper.DoEvents();

            setup.Ribbon.Tabs.Insert(1, setup.TabA1);
            setup.Ribbon.Tabs.Add(setup.TabB1);
            UIHelper.DoEvents();
            UIHelper.DoEvents();

            Assert.That(setup.GroupA.InnerVisibility, Is.EqualTo(Visibility.Visible), "The header of group A must be shown again");
            Assert.That(setup.GroupB.InnerVisibility, Is.EqualTo(Visibility.Visible), "The header of group B must be shown again");
            Assert.That(GetX(setup.GroupA, window), Is.EqualTo(GetX(setup.TabA1, window)).Within(Tolerance), "The header of group A must start above its re-added first tab");
            Assert.That(GetX(setup.GroupB, window), Is.EqualTo(GetX(setup.TabB1, window)).Within(Tolerance), "The header of group B must start above its re-added tab");
        }
    }

    // The title bar hides the contextual headers by default (HideContextTabs is true), apps that want them set it to false.
    private static void ShowContextualHeaders(Setup setup)
    {
        UIHelper.DoEvents();

        Assert.That(setup.Ribbon.TitleBar, Is.Not.Null, "Precondition: the ribbon uses the title bar of the window");

        setup.Ribbon.TitleBar!.HideContextTabs = false;

        UIHelper.DoEvents();
        UIHelper.DoEvents();
    }

    // Layout details for the failure message, so a failure on CI shows where the title bar put the headers.
    private static string Describe(Setup setup, TestRibbonWindow window)
    {
        var titleBar = setup.Ribbon.TitleBar;

        if (titleBar is null)
        {
            return "Ribbon.TitleBar is null";
        }

        return FormattableString.Invariant($"TitleBar X={GetX(titleBar, window)} W={titleBar.ActualWidth} IsCollapsed={titleBar.IsCollapsed} HideContextTabs={titleBar.HideContextTabs} Items={titleBar.Items.Count} itemsRect={titleBar.GetFieldValue<Rect>("itemsRect")} qatRect={titleBar.GetFieldValue<Rect>("quickAccessToolbarRect")} headerRect={titleBar.GetFieldValue<Rect>("headerRect")}; ")
               + FormattableString.Invariant($"Ribbon IsCollapsed={setup.Ribbon.IsCollapsed} CanScroll={setup.Ribbon.TabControl?.CanScroll}; ")
               + DescribeGroup("A", setup.GroupA, window)
               + DescribeGroup("B", setup.GroupB, window)
               + DescribeTab("A1", setup.TabA1, window)
               + DescribeTab("A2", setup.TabA2, window)
               + DescribeTab("B1", setup.TabB1, window);
    }

    private static string DescribeGroup(string name, RibbonContextualTabGroup group, TestRibbonWindow window)
    {
        return FormattableString.Invariant($"Group {name} X={GetX(group, window)} W={group.ActualWidth} Inner={group.InnerVisibility} IsVisible={group.IsVisible} Parent={VisualTreeHelper.GetParent(group)?.GetType().Name}; ");
    }

    private static string DescribeTab(string name, RibbonTabItem tab, TestRibbonWindow window)
    {
        return FormattableString.Invariant($"Tab {name} X={GetX(tab, window)} W={tab.ActualWidth} Desired={tab.DesiredSize.Width}; ");
    }

    private static double GetX(UIElement element, UIElement window)
    {
        return element.TranslatePoint(default, window).X;
    }

    private sealed class Setup
    {
        public Setup()
        {
            this.GroupA = new RibbonContextualTabGroup { Header = "Group A", Visibility = Visibility.Visible };
            this.GroupB = new RibbonContextualTabGroup { Header = "Group B", Visibility = Visibility.Visible };

            this.TabA1 = new RibbonTabItem { Header = "A one", Group = this.GroupA };
            this.TabA2 = new RibbonTabItem { Header = "A two", Group = this.GroupA };
            this.TabB1 = new RibbonTabItem { Header = "B one", Group = this.GroupB };

            this.Ribbon = new Ribbon();
            this.Ribbon.ContextualGroups.Add(this.GroupA);
            this.Ribbon.ContextualGroups.Add(this.GroupB);
            this.Ribbon.Tabs.Add(new RibbonTabItem { Header = "Home" });
            this.Ribbon.Tabs.Add(this.TabA1);
            this.Ribbon.Tabs.Add(this.TabA2);
            this.Ribbon.Tabs.Add(this.TabB1);
        }

        public Ribbon Ribbon { get; }

        public RibbonContextualTabGroup GroupA { get; }

        public RibbonContextualTabGroup GroupB { get; }

        public RibbonTabItem TabA1 { get; }

        public RibbonTabItem TabA2 { get; }

        public RibbonTabItem TabB1 { get; }
    }
}
