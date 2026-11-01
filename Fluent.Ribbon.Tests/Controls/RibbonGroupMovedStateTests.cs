namespace Fluent.Tests.Controls;

using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// A group that its container had shrunk (here to a single button) kept that state after it moved to another
/// container (another tab) with plenty of room and no ReduceOrder: it stayed a single button. Found by a bug hunt after round 7.
/// </summary>
[TestFixture]
public class RibbonGroupMovedStateTests
{
    [Test]
    public void Group_shrunk_in_one_container_is_large_again_after_moving_to_a_roomy_one()
    {
        var narrow = new RibbonGroupsContainer
        {
            Height = RibbonTabControl.DefaultContentHeight,
            Width = 40,
            ReduceOrder = "(MyGroup),(MyGroup),(MyGroup)"
        };

        var roomy = new RibbonGroupsContainer
        {
            Height = RibbonTabControl.DefaultContentHeight,
            Width = 1000
        };

        var group = new RibbonGroupBox { Name = "MyGroup", Header = "Group" };
        group.Items.Add(new Button { Header = "One" });
        group.Items.Add(new Button { Header = "Two" });
        narrow.Children.Add(group);

        using (new TestRibbonWindow(new StackPanel { Children = { narrow, roomy } }))
        {
            UIHelper.DoEvents();

            Assert.That(group.State, Is.Not.EqualTo(RibbonGroupBoxState.Large), "Precondition: the narrow container shrinks the group");

            narrow.Children.Remove(group);
            roomy.Children.Add(group);
            UIHelper.DoEvents();

            Assert.That(group.State, Is.EqualTo(RibbonGroupBoxState.Large), "In a container with plenty of room and no ReduceOrder the group must be large again");
        }
    }
}
