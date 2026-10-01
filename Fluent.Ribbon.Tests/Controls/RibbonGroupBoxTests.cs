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
}