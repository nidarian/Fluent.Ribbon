namespace Fluent.Tests.Controls;

using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// A group's starting state must come from <see cref="RibbonGroupBox.SimplifiedStateDefinition"/> in the
/// simplified ribbon, and from <see cref="RibbonGroupBox.StateDefinition"/> in the classic ribbon.
/// </summary>
/// <remarks>
/// Before the fix, the reset always used StateDefinition. Groups only change state when they're listed in
/// the tab's ReduceOrder (most apps don't set it), so a SimplifiedStateDefinition like "Collapsed" had no effect.
/// That also broke the workaround suggested in https://github.com/fluentribbon/Fluent.Ribbon/issues/1233:
/// an always collapsed group at the end of the tab, acting as a "..." overflow button.
/// </remarks>
[TestFixture]
public class RibbonGroupBoxSimplifiedStateTests
{
    [TestCase("Collapsed", RibbonGroupBoxState.Collapsed)]
    [TestCase("Middle,Collapsed", RibbonGroupBoxState.Middle)]
    [TestCase("Large,Middle,Collapsed", RibbonGroupBoxState.Large)] // the default, must stay unchanged
    public void Simplified_group_starts_in_first_state_of_SimplifiedStateDefinition(string simplifiedStateDefinition, RibbonGroupBoxState expectedState)
    {
        var groupBox = new RibbonGroupBox
        {
            Header = "More",
            SimplifiedStateDefinition = simplifiedStateDefinition
        };

        var tabItem = new RibbonTabItem { Header = "Home" };
        tabItem.Groups.Add(groupBox);

        var ribbon = new Ribbon();
        ribbon.Tabs.Add(tabItem);

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.SelectedTabItem = tabItem;
            ribbon.IsSimplified = true;
            UIHelper.DoEvents();

            Assert.That(groupBox.IsSimplified, Is.True, "Precondition: the group should be in simplified mode");
            Assert.That(groupBox.State, Is.EqualTo(expectedState));
        }
    }

    [Test]
    public void Switching_back_to_classic_uses_StateDefinition_again()
    {
        var groupBox = new RibbonGroupBox
        {
            Header = "More",
            SimplifiedStateDefinition = "Collapsed"
        };

        var tabItem = new RibbonTabItem { Header = "Home" };
        tabItem.Groups.Add(groupBox);

        var ribbon = new Ribbon();
        ribbon.Tabs.Add(tabItem);

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.SelectedTabItem = tabItem;

            ribbon.IsSimplified = true;
            UIHelper.DoEvents();
            Assert.That(groupBox.State, Is.EqualTo(RibbonGroupBoxState.Collapsed));

            // The classic ribbon ignores SimplifiedStateDefinition. The default StateDefinition starts with Large.
            ribbon.IsSimplified = false;
            UIHelper.DoEvents();
            Assert.That(groupBox.IsSimplified, Is.False);
            Assert.That(groupBox.State, Is.EqualTo(RibbonGroupBoxState.Large));
        }
    }
}
