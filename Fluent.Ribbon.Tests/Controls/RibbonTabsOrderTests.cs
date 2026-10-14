namespace Fluent.Tests.Controls;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// <see cref="Ribbon"/> shows its <see cref="Ribbon.Tabs"/> by copying them into the items of its tab control.
/// That copy ignored <see cref="System.Collections.ObjectModel.ObservableCollection{T}.Move"/> and put a replaced tab
/// (Tabs[i] = x) at the end instead of at index i. The tab control then showed the tabs in another order than Tabs,
/// and the ribbon translated the selected tab with the index of the wrong collection.
/// </summary>
[TestFixture]
public class RibbonTabsOrderTests
{
    /// <summary>
    /// The selection between the ribbon and its tab control can bounce back and forth when the two lists
    /// differ in order. One change of the tabs or one click on a tab must not select tabs more often than this,
    /// otherwise the test stops the bouncing (instead of letting it end in a stack overflow that kills the test run).
    /// </summary>
    private const int MaxSelectionsPerAction = 10;

    [Test]
    public void Moving_a_tab_should_move_it_in_the_tab_control()
    {
        var tabA = new RibbonTabItem { Header = "A" };
        var tabB = new RibbonTabItem { Header = "B" };
        var ribbon = new Ribbon { Tabs = { tabA, tabB } };

        using var window = new TestRibbonWindow(ribbon);
        UIHelper.DoEvents();

        Act(ribbon, "Tabs.Move(0, 1)", () => ribbon.Tabs.Move(0, 1));

        Assert.That(ribbon.TabControl.Items.Cast<object>().ToList(), Is.EqualTo(ribbon.Tabs.Cast<object>().ToList()), "Tab control items after Tabs.Move(0, 1)");
    }

    [Test]
    public void Replacing_a_tab_should_put_the_new_tab_at_the_same_place_in_the_tab_control()
    {
        var tabA = new RibbonTabItem { Header = "A" };
        var tabB = new RibbonTabItem { Header = "B" };
        var tabC = new RibbonTabItem { Header = "C" };
        var ribbon = new Ribbon { Tabs = { tabA, tabB } };

        using var window = new TestRibbonWindow(ribbon);
        UIHelper.DoEvents();

        Assert.That(ribbon.SelectedTabItem, Is.SameAs(tabA), "Initially selected tab");

        // Replaces the selected tab. Tabs is now C, B
        Act(ribbon, "Tabs[0] = C", () => ribbon.Tabs[0] = tabC);

        Assert.That(ribbon.TabControl.Items.Cast<object>().ToList(), Is.EqualTo(ribbon.Tabs.Cast<object>().ToList()), "Tab control items after Tabs[0] = C");

        // The selected tab is gone, the tab control selects the remaining tab B, and the ribbon has to report that.
        AssertSelected(ribbon, tabB);
    }

    [Test]
    public void Moving_the_selected_tab_should_keep_it_selected()
    {
        var tabA = new RibbonTabItem { Header = "A" };
        var tabB = new RibbonTabItem { Header = "B" };
        var tabC = new RibbonTabItem { Header = "C" };
        var ribbon = new Ribbon { Tabs = { tabA, tabB, tabC } };

        using var window = new TestRibbonWindow(ribbon);
        UIHelper.DoEvents();

        Assert.That(ribbon.SelectedTabItem, Is.SameAs(tabA), "Initially selected tab");

        var selectedTabChangedCount = 0;
        ribbon.SelectedTabChanged += (_, _) => ++selectedTabChangedCount;

        // Tabs is now B, C, A
        Act(ribbon, "Tabs.Move(0, 2)", () => ribbon.Tabs.Move(0, 2));

        Assert.That(ribbon.TabControl.Items.Cast<object>().ToList(), Is.EqualTo(ribbon.Tabs.Cast<object>().ToList()), "Tab control items after Tabs.Move(0, 2)");
        AssertSelected(ribbon, tabA);
        Assert.That(selectedTabChangedCount, Is.EqualTo(0), "SelectedTabChanged raised while moving the selected tab");

        // Moving another tab in front of the selected one changes the index of the selected tab. Tabs is now B, A, C
        Act(ribbon, "Tabs.Move(1, 2)", () => ribbon.Tabs.Move(1, 2));

        Assert.That(ribbon.TabControl.Items.Cast<object>().ToList(), Is.EqualTo(ribbon.Tabs.Cast<object>().ToList()), "Tab control items after Tabs.Move(1, 2)");
        AssertSelected(ribbon, tabA);
        Assert.That(selectedTabChangedCount, Is.EqualTo(0), "SelectedTabChanged raised while moving another tab");
    }

    [Test]
    public void Selecting_a_tab_after_moving_tabs_should_select_that_tab()
    {
        var tabA = new RibbonTabItem { Header = "A" };
        var tabB = new RibbonTabItem { Header = "B" };
        var ribbon = new Ribbon { Tabs = { tabA, tabB } };

        using var window = new TestRibbonWindow(ribbon);
        UIHelper.DoEvents();

        Assert.That(ribbon.SelectedTabItem, Is.SameAs(tabA), "Initially selected tab");

        // Tabs is now B, A
        Act(ribbon, "Tabs.Move(0, 1)", () => ribbon.Tabs.Move(0, 1));

        Act(ribbon, "Selecting tab B", () => SelectLikeAClick(tabB));

        AssertSelected(ribbon, tabB);
    }

    [Test]
    public void Selecting_a_tab_after_replacing_a_tab_should_select_that_tab()
    {
        var tabA = new RibbonTabItem { Header = "A" };
        var tabB = new RibbonTabItem { Header = "B" };
        var tabC = new RibbonTabItem { Header = "C" };
        var ribbon = new Ribbon { Tabs = { tabA, tabB } };

        using var window = new TestRibbonWindow(ribbon);
        UIHelper.DoEvents();

        // Select B, so the replaced tab A is not the selected one.
        Act(ribbon, "Selecting tab B", () => SelectLikeAClick(tabB));
        AssertSelected(ribbon, tabB);

        // Tabs is now C, B
        Act(ribbon, "Tabs[0] = C", () => ribbon.Tabs[0] = tabC);

        Act(ribbon, "Selecting tab C", () => SelectLikeAClick(tabC));

        AssertSelected(ribbon, tabC);
    }

    /// <summary>
    /// Selects <paramref name="tab"/> the way a click or keyboard focus on the tab header does it (RibbonTabItem.OnGotKeyboardFocus).
    /// </summary>
    private static void SelectLikeAClick(RibbonTabItem tab)
    {
        tab.SetCurrentValue(RibbonTabItem.IsSelectedProperty, true);
    }

    /// <summary>
    /// Runs <paramref name="action"/> and fails the test if that makes the selection bounce between tabs.
    /// Without this guard the bouncing ends in a stack overflow, which kills the whole test run without a test result.
    /// </summary>
    private static void Act(Ribbon ribbon, string what, Action action)
    {
        var selections = 0;
        var bounced = false;

        // Selector.SelectedEvent is raised by each tab as soon as it becomes selected,
        // before the ribbon reacts to the selection, so this also counts nested (bouncing) selections.
        RoutedEventHandler onSelected = (_, _) =>
        {
            ++selections;

            if (selections > MaxSelectionsPerAction)
            {
                bounced = true;
                throw new InvalidOperationException($"{what} selected tabs {selections} times: the selection bounces between tabs (without this guard: stack overflow).");
            }
        };

        ribbon.AddHandler(Selector.SelectedEvent, onSelected, handledEventsToo: true);

        try
        {
            action();
            UIHelper.DoEvents();
        }
        finally
        {
            // After bouncing the guard stays, so closing the window can't end in a stack overflow either.
            if (bounced == false)
            {
                ribbon.RemoveHandler(Selector.SelectedEvent, onSelected);
            }
        }
    }

    private static void AssertSelected(Ribbon ribbon, RibbonTabItem tab)
    {
        Assert.That(ribbon.SelectedTabItem?.Header, Is.EqualTo(tab.Header), "Ribbon.SelectedTabItem");
        Assert.That(ribbon.SelectedTabIndex, Is.EqualTo(ribbon.Tabs.IndexOf(tab)), "Ribbon.SelectedTabIndex");
        Assert.That((ribbon.TabControl.SelectedItem as RibbonTabItem)?.Header, Is.EqualTo(tab.Header), "TabControl.SelectedItem");
        Assert.That(tab.IsSelected, Is.True, $"IsSelected of tab \"{tab.Header}\"");
    }
}
