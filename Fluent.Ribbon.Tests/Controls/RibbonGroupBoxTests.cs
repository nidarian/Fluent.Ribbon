namespace Fluent.Tests.Controls;

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class RibbonGroupBoxTests
{
    [Test]
    public void Size_Should_Change_On_Group_State_Change_When_Items_Are_Bound()
    {
        var items = new List<ItemViewModel>
        {
            new()
        };

        var ribbonGroupBox = new RibbonGroupBox
        {
            ItemsSource = items,
            ItemTemplate = CreateDataTemplateForItemViewModel()
        };

        using (new TestRibbonWindow(ribbonGroupBox))
        {
            {
                {
                    ribbonGroupBox.State = RibbonGroupBoxState.Small;
                    UIHelper.DoEvents();
                }

                Assert.That(items.First().ControlSize, Is.EqualTo(RibbonControlSize.Small));
            }

            {
                {
                    ribbonGroupBox.State = RibbonGroupBoxState.Middle;
                    UIHelper.DoEvents();
                }

                Assert.That(items.First().ControlSize, Is.EqualTo(RibbonControlSize.Middle));
            }

            {
                {
                    ribbonGroupBox.State = RibbonGroupBoxState.Large;
                    UIHelper.DoEvents();
                }

                Assert.That(items.First().ControlSize, Is.EqualTo(RibbonControlSize.Large));
            }
        }
    }

    [Test]
    public void Size_Should_Change_On_Group_State_Change_When_Items_Are_Ribbon_Controls()
    {
        var ribbonGroupBox = new RibbonGroupBox();

        ribbonGroupBox.Items.Add(new Fluent.Button());

        using (new TestRibbonWindow(ribbonGroupBox))
        {
            {
                {
                    ribbonGroupBox.State = RibbonGroupBoxState.Small;
                    UIHelper.DoEvents();
                }

                Assert.That(ribbonGroupBox.Items.OfType<Fluent.Button>().First().Size, Is.EqualTo(RibbonControlSize.Small));
            }

            {
                {
                    ribbonGroupBox.State = RibbonGroupBoxState.Middle;
                    UIHelper.DoEvents();
                }

                Assert.That(ribbonGroupBox.Items.OfType<Fluent.Button>().First().Size, Is.EqualTo(RibbonControlSize.Middle));
            }

            {
                {
                    ribbonGroupBox.State = RibbonGroupBoxState.Large;
                    UIHelper.DoEvents();
                }

                Assert.That(ribbonGroupBox.Items.OfType<Fluent.Button>().First().Size, Is.EqualTo(RibbonControlSize.Large));
            }
        }
    }

    [Test]
    public void TestStateDefinition()
    {
        var panel = new RibbonGroupsContainer();

        var ribbonGroupBox = new RibbonGroupBox { Name = "MyGroup" };

        panel.Children.Add(ribbonGroupBox);

        ribbonGroupBox.Items.Add(new Fluent.Button() { Width = 200 });

        using (var testWindow = new TestRibbonWindow(panel))
        {
            {
                Assert.That(ribbonGroupBox.State, Is.EqualTo(RibbonGroupBoxState.Large));
                Assert.That(ribbonGroupBox.Items.OfType<Fluent.Button>().First().Size, Is.EqualTo(RibbonControlSize.Large));
            }

            {
                {
                    ribbonGroupBox.StateDefinition = RibbonGroupBoxStateDefinition.FromString("Middle,Collapsed");
                    UIHelper.DoEvents();
                }

                Assert.That(ribbonGroupBox.State, Is.EqualTo(RibbonGroupBoxState.Middle));
                Assert.That(ribbonGroupBox.Items.OfType<Fluent.Button>().First().Size, Is.EqualTo(RibbonControlSize.Middle));
            }

            {
                {
                    ribbonGroupBox.State = RibbonGroupBoxState.Large;
                    UIHelper.DoEvents();
                }

                Assert.That(ribbonGroupBox.State, Is.EqualTo(RibbonGroupBoxState.Middle));
                Assert.That(ribbonGroupBox.Items.OfType<Fluent.Button>().First().Size, Is.EqualTo(RibbonControlSize.Middle));
            }

            {
                {
                    testWindow.Width = 10;
                    UIHelper.DoEvents();
                }

                Assert.That(ribbonGroupBox.State, Is.EqualTo(RibbonGroupBoxState.Middle));
                Assert.That(ribbonGroupBox.Items.OfType<Fluent.Button>().First().Size, Is.EqualTo(RibbonControlSize.Middle));
            }

            {
                {
                    panel.ReduceOrder = "MyGroup";
                    UIHelper.DoEvents();
                }

                Assert.That(ribbonGroupBox.State, Is.EqualTo(RibbonGroupBoxState.Collapsed));
                Assert.That(ribbonGroupBox.Items.OfType<Fluent.Button>().First().Size, Is.EqualTo(RibbonControlSize.Large));
            }
        }
    }

    /// <summary>
    /// Changing <see cref="RibbonGroupsContainer.ReduceOrder"/> first undoes the reductions of the old order.
    /// Only the entries that were really applied may be undone, otherwise the group ends up larger than its default.
    /// </summary>
    [Test]
    public void Changing_ReduceOrder_only_undoes_applied_reductions()
    {
        var panel = new RibbonGroupsContainer();

        var ribbonGroupBox = new RibbonGroupBox { Name = "MyGroup" };
        ribbonGroupBox.Items.Add(new Fluent.Button());

        panel.Children.Add(ribbonGroupBox);

        // Never measured, so like in a wide window nothing of this order gets applied.
        panel.ReduceOrder = "(MyGroup)";

        Assert.That(ribbonGroupBox.ScaleIntermediate, Is.EqualTo(0), "Precondition: the group must not be scaled.");

        panel.ReduceOrder = "MyGroup";

        Assert.That(ribbonGroupBox.ScaleIntermediate, Is.EqualTo(0), "Nothing was reduced, so changing the order must not enlarge the group.");
    }

    private static DataTemplate CreateDataTemplateForItemViewModel()
    {
        var dataTemplate = new DataTemplate(typeof(ItemViewModel));

        var factory = new FrameworkElementFactory(typeof(Fluent.Button));
        factory.SetBinding(RibbonProperties.SizeProperty, new Binding(nameof(ItemViewModel.ControlSize)) { Mode = BindingMode.TwoWay });

        //set the visual tree of the data template
        dataTemplate.VisualTree = factory;

        return dataTemplate;
    }
}

public class ItemViewModel
{
    public RibbonControlSize ControlSize { get; set; }

    /// <summary>
    /// A collapsed group shows its controls in a drop down. The drop down can only be open while the
    /// group is collapsed (see CoerceIsDropDownOpen), but that rule was only checked when IsDropDownOpen
    /// itself was set. When the group went back to a normal state while its drop down was open (for
    /// example because the ribbon got wider), an empty drop down stayed open under the group.
    /// </summary>
    [Test]
    public void Leaving_the_collapsed_state_closes_the_drop_down()
    {
        var ribbonGroupBox = new RibbonGroupBox
        {
            Header = "Group",
            Items = { new Fluent.Button { Header = "Button" } }
        };

        using (new TestRibbonWindow(ribbonGroupBox))
        {
            ribbonGroupBox.State = RibbonGroupBoxState.Collapsed;
            UIHelper.DoEvents();

            ribbonGroupBox.IsDropDownOpen = true;
            UIHelper.DoEvents();
            Assert.That(ribbonGroupBox.IsDropDownOpen, Is.True, "Precondition: a collapsed group's drop down can open");

            ribbonGroupBox.State = RibbonGroupBoxState.Large;
            UIHelper.DoEvents();

            Assert.That(ribbonGroupBox.IsDropDownOpen, Is.False, "An expanded group has no drop down to show");

            // Collapsing again later must not bring the old drop down back by itself.
            ribbonGroupBox.State = RibbonGroupBoxState.Collapsed;
            UIHelper.DoEvents();

            Assert.That(ribbonGroupBox.IsDropDownOpen, Is.False, "The drop down must not reopen by itself");
        }
    }

    /// <summary>
    /// A state definition with more than four parts (here because of a duplicate) must still be read part by part.
    /// The last part must not swallow the rest of the text, because "Small,Collapsed" would be parsed as one combined enum value.
    /// </summary>
    [Test]
    public void StateDefinition_with_more_than_four_parts_keeps_all_states()
    {
        var stateDefinition = RibbonGroupBoxStateDefinition.FromString("Large,Large,Middle,Small,Collapsed");

        Assert.That(stateDefinition.States, Is.EqualTo(new[]
        {
            RibbonGroupBoxState.Large,
            RibbonGroupBoxState.Middle,
            RibbonGroupBoxState.Small,
            RibbonGroupBoxState.Collapsed
        }));
    }

    /// <summary>
    /// <see cref="RibbonGroupsContainer"/> skips measuring when available and desired size match the previous measure.
    /// After <see cref="RibbonGroupsContainer.ReduceOrder"/> changed it must measure again, otherwise the new order is never applied.
    /// </summary>
    [Test]
    public void Changing_ReduceOrder_applies_the_new_order_for_the_same_size()
    {
        var panel = new RibbonGroupsContainer
        {
            // Only scales the (non existing) scalable items, so the group size doesn't change.
            ReduceOrder = "(MyGroup)"
        };

        var ribbonGroupBox = new RibbonGroupBox { Name = "MyGroup" };
        ribbonGroupBox.Items.Add(new Fluent.Button { Width = 200 });

        panel.Children.Add(ribbonGroupBox);

        using (new TestRibbonWindow(panel))
        {
            // ReduceOrder only invalidates the measure of a loaded panel.
            Assert.That(panel.IsLoaded, Is.True, "Precondition: the panel must be loaded.");

            // Measure directly (no DoEvents) so only these measure passes, with exactly this size, happen.
            var narrowSize = new Size(10, 100);

            panel.Measure(narrowSize);

            Assert.That(ribbonGroupBox.DesiredSize.Width, Is.GreaterThan(narrowSize.Width), "Precondition: the group must not fit.");
            Assert.That(ribbonGroupBox.State, Is.EqualTo(RibbonGroupBoxState.Large), "Precondition: the old order must not change the group state.");

            panel.ReduceOrder = "MyGroup";
            panel.Measure(narrowSize);

            Assert.That(ribbonGroupBox.State, Is.EqualTo(RibbonGroupBoxState.Middle), "The new order must reduce the group.");
        }
    }
}
