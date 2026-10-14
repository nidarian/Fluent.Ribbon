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
}
