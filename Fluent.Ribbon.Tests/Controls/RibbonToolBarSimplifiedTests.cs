namespace Fluent.Tests.Controls;

using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Controls added to a <see cref="RibbonToolBar"/> at runtime must take over the toolbar's simplified state.
/// </summary>
/// <remarks>
/// Before the fix, the toolbar passed its state on to its children only when its own IsSimplified changed.
/// A child added later to a toolbar in a simplified ribbon stayed in the classic style.
/// </remarks>
[TestFixture]
public class RibbonToolBarSimplifiedTests
{
    [Test]
    public void Child_added_at_runtime_in_simplified_ribbon_is_simplified()
    {
        var toolBar = new RibbonToolBar();

        var groupBox = new RibbonGroupBox { Header = "Group" };
        groupBox.Items.Add(toolBar);

        var tabItem = new RibbonTabItem { Header = "Home" };
        tabItem.Groups.Add(groupBox);

        var ribbon = new Ribbon
        {
            CanUseSimplified = true
        };
        ribbon.Tabs.Add(tabItem);

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.SelectedTabItem = tabItem;
            ribbon.IsSimplified = true;
            UIHelper.DoEvents();

            Assert.That(toolBar.IsSimplified, Is.True, "Precondition: the toolbar should be in simplified mode");

            var button = new Button { Header = "Added later" };
            toolBar.Children.Add(button);
            UIHelper.DoEvents();

            Assert.That(button.IsSimplified, Is.True, "A button added to the toolbar in the simplified ribbon should be simplified");

            // Guard: switching back to the classic ribbon must update the added button too.
            ribbon.IsSimplified = false;
            UIHelper.DoEvents();

            Assert.That(toolBar.IsSimplified, Is.False, "Precondition: the toolbar should be in classic mode");
            Assert.That(button.IsSimplified, Is.False, "The added button should follow the ribbon back to classic mode");
        }
    }

    /// <summary>
    /// Without a layout definition for the current mode the toolbar stacks its children and sizes them itself.
    /// It always used their SizeDefinition, also in the simplified ribbon, so SimplifiedSizeDefinition was ignored
    /// (a button meant to be icon-only showed its text). Found by a bug hunt after round 7.
    /// </summary>
    [Test]
    public void Toolbar_children_use_SimplifiedSizeDefinition_in_the_simplified_ribbon()
    {
        var button = new Button
        {
            Header = "Paste",
            SizeDefinition = new RibbonControlSizeDefinition(RibbonControlSize.Large, RibbonControlSize.Large, RibbonControlSize.Large),
            SimplifiedSizeDefinition = new RibbonControlSizeDefinition(RibbonControlSize.Middle, RibbonControlSize.Middle, RibbonControlSize.Middle)
        };

        var toolBar = new RibbonToolBar();
        toolBar.Children.Add(button);

        var groupBox = new RibbonGroupBox { Header = "Group" };
        groupBox.Items.Add(toolBar);

        var tabItem = new RibbonTabItem { Header = "Home" };
        tabItem.Groups.Add(groupBox);

        var ribbon = new Ribbon { CanUseSimplified = true, AutomaticStateManagement = false };
        ribbon.Tabs.Add(tabItem);

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.SelectedTabItem = tabItem;
            UIHelper.DoEvents();

            Assert.That(button.Size, Is.EqualTo(RibbonControlSize.Large), "Precondition: in the classic ribbon the button follows its SizeDefinition");

            ribbon.IsSimplified = true;
            UIHelper.DoEvents();

            Assert.That(toolBar.IsSimplified, Is.True, "Precondition: the toolbar should be in simplified mode");
            Assert.That(button.Size, Is.EqualTo(RibbonControlSize.Middle), "In the simplified ribbon the button must follow its SimplifiedSizeDefinition");

            ribbon.IsSimplified = false;
            UIHelper.DoEvents();

            Assert.That(button.Size, Is.EqualTo(RibbonControlSize.Large), "Back in the classic ribbon the button must follow its SizeDefinition again");
        }
    }
}
