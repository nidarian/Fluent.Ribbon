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
}
