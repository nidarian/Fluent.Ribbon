namespace Fluent.Tests.Integration;

using System.Windows.Media;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class InRibbonGalleryIntegrationTests
{
    [Test]
    public void Opening_And_Closing_DropDown_Should_Not_Change_Size_For_Fixed_Item_Width()
    {
        var ribbonGroupsContainer = new RibbonGroupsContainer
        {
            Height = RibbonTabControl.DefaultContentHeight,
            ReduceOrder = "(MyGroup),(MyGroup),(MyGroup),(MyGroup),(MyGroup),(MyGroup),(MyGroup),(MyGroup),(MyGroup),(MyGroup)"
        };

        var groupBox = new RibbonGroupBox
        {
            Name = "MyGroup",
            BorderBrush = Brushes.Red
        };

        ribbonGroupsContainer.Children.Add(groupBox);

        var firstInRibbonGallery = new InRibbonGallery
        {
            MinItemsInRow = 1,
            MaxItemsInRow = 5,
            ItemWidth = 50,
            ItemHeight = 18,
            GroupBy = "Group",
            ResizeMode = ContextMenuResizeMode.Both,
            ItemsSource = this.sampleDataItemsForFixedWidth
        };

        groupBox.Items.Add(firstInRibbonGallery);

        var secondInRibbonGallery = new InRibbonGallery
        {
            MinItemsInRow = 1,
            MaxItemsInRow = 5,
            ItemWidth = 50,
            ItemHeight = 18,
            GroupBy = "Group",
            ResizeMode = ContextMenuResizeMode.Both,
            ItemsSource = this.sampleDataItemsForFixedWidth
        };

        groupBox.Items.Add(secondInRibbonGallery);

        using (new TestRibbonWindow(ribbonGroupsContainer))
        {
            UIHelper.DoEvents();

            ribbonGroupsContainer.Width = 520;

            UIHelper.DoEvents();

            Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(219));
            Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(219));
            Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));
            Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));

            Assert.That(groupBox.ActualWidth, Is.EqualTo(450));

            for (var i = 0; i < 5; i++)
            {
                UIHelper.DoEvents();

                Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(219));
                Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(219));
                Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));
                Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));

                Assert.That(groupBox.ActualWidth, Is.EqualTo(450));

                // open and close first
                {
                    firstInRibbonGallery.IsDropDownOpen = true;
                    UIHelper.DoEvents();

                    Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(219));
                    Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(219));
                    Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(0));
                    Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));

                    Assert.That(groupBox.ActualWidth, Is.EqualTo(450));

                    firstInRibbonGallery.IsDropDownOpen = false;
                    UIHelper.DoEvents();

                    Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(219));
                    Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(219));
                    Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));
                    Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));

                    Assert.That(groupBox.ActualWidth, Is.EqualTo(450));
                }

                // open and close second
                {
                    secondInRibbonGallery.IsDropDownOpen = true;
                    UIHelper.DoEvents();

                    Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(219));
                    Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(219));
                    Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));
                    Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(0));

                    Assert.That(groupBox.ActualWidth, Is.EqualTo(450));

                    secondInRibbonGallery.IsDropDownOpen = false;
                    UIHelper.DoEvents();

                    Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(219));
                    Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(219));
                    Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));
                    Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));

                    Assert.That(groupBox.ActualWidth, Is.EqualTo(450));
                }

                ++ribbonGroupsContainer.Width;
            }

            UIHelper.DoEvents();

            Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(219));
            Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(219));
            Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));
            Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));

            Assert.That(groupBox.ActualWidth, Is.EqualTo(450));

            ribbonGroupsContainer.Width = 560;

            UIHelper.DoEvents();

            Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(269));
            Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(269));
            Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(5));
            Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(5));
            Assert.That(groupBox.ActualWidth, Is.EqualTo(550));
        }
    }

    [Test]
    public void Opening_And_Closing_DropDown_Should_Not_Change_Size_For_Dynamic_Item_Width()
    {
        var ribbonGroupsContainer = new RibbonGroupsContainer
        {
            Height = RibbonTabControl.DefaultContentHeight,
            ReduceOrder = "(MyGroup),(MyGroup),(MyGroup),(MyGroup),(MyGroup),(MyGroup),(MyGroup),(MyGroup),(MyGroup),(MyGroup)"
        };

        var groupBox = new RibbonGroupBox
        {
            Name = "MyGroup",
            BorderBrush = Brushes.Red
        };

        ribbonGroupsContainer.Children.Add(groupBox);

        var firstInRibbonGallery = new InRibbonGallery
        {
            MinItemsInRow = 1,
            MaxItemsInRow = 5,
            ItemHeight = 18,
            GroupBy = "Group",
            ResizeMode = ContextMenuResizeMode.Both,
            ItemsSource = this.sampleDataItemsForDynamicWidth
        };

        groupBox.Items.Add(firstInRibbonGallery);

        var secondInRibbonGallery = new InRibbonGallery
        {
            MinItemsInRow = 1,
            MaxItemsInRow = 5,
            ItemHeight = 18,
            GroupBy = "Group",
            ResizeMode = ContextMenuResizeMode.Both,
            ItemsSource = this.sampleDataItemsForDynamicWidth
        };

        groupBox.Items.Add(secondInRibbonGallery);

        using (new TestRibbonWindow(ribbonGroupsContainer))
        {
            UIHelper.DoEvents();

            ribbonGroupsContainer.Width = 620;

            UIHelper.DoEvents();

            Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(247));
            Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(247));
            Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));
            Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));

            Assert.That(groupBox.ActualWidth, Is.EqualTo(506));

            for (var i = 0; i < 5; i++)
            {
                UIHelper.DoEvents();

                Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(247));
                Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(247));
                Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));
                Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));

                Assert.That(groupBox.ActualWidth, Is.EqualTo(506));

                // open and close first
                {
                    firstInRibbonGallery.IsDropDownOpen = true;
                    UIHelper.DoEvents();

                    Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(247));
                    Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(247));
                    Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(0));
                    Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));

                    Assert.That(groupBox.ActualWidth, Is.EqualTo(506));

                    firstInRibbonGallery.IsDropDownOpen = false;
                    UIHelper.DoEvents();

                    Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(247));
                    Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(247));
                    Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));
                    Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));

                    Assert.That(groupBox.ActualWidth, Is.EqualTo(506));
                }

                // open and close second
                {
                    secondInRibbonGallery.IsDropDownOpen = true;
                    UIHelper.DoEvents();

                    Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(247));
                    Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(247));
                    Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));
                    Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(0));

                    Assert.That(groupBox.ActualWidth, Is.EqualTo(506));

                    secondInRibbonGallery.IsDropDownOpen = false;
                    UIHelper.DoEvents();

                    Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(247));
                    Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(247));
                    Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));
                    Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));

                    Assert.That(groupBox.ActualWidth, Is.EqualTo(506));
                }

                ++ribbonGroupsContainer.Width;
            }

            UIHelper.DoEvents();

            Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(247));
            Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(247));
            Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));
            Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));

            Assert.That(groupBox.ActualWidth, Is.EqualTo(506));

            ribbonGroupsContainer.Width = 670;

            UIHelper.DoEvents();

            Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(323));
            Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(323));
            Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));
            Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(4));
            Assert.That(groupBox.ActualWidth, Is.EqualTo(658));

            ribbonGroupsContainer.Width = 900;

            UIHelper.DoEvents();

            Assert.That(firstInRibbonGallery.ActualWidth, Is.EqualTo(399));
            Assert.That(secondInRibbonGallery.ActualWidth, Is.EqualTo(399));
            Assert.That(firstInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(5));
            Assert.That(secondInRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(5));
            Assert.That(groupBox.ActualWidth, Is.EqualTo(810));
        }
    }

    [Test]
    public void Gallery_Should_Expand_Again_When_Group_Goes_Back_To_Large()
    {
        var ribbonGroupsContainer = new RibbonGroupsContainer
        {
            Height = RibbonTabControl.DefaultContentHeight,
            ReduceOrder = "MyGroup"
        };

        var groupBox = new RibbonGroupBox
        {
            Name = "MyGroup"
        };

        ribbonGroupsContainer.Children.Add(groupBox);

        var inRibbonGallery = new InRibbonGallery
        {
            MinItemsInRow = 1,
            MaxItemsInRow = 3,
            ItemWidth = 50,
            ItemHeight = 18,
            ItemsSource = this.sampleDataItemsForFixedWidth
        };

        groupBox.Items.Add(inRibbonGallery);

        using (new TestRibbonWindow(ribbonGroupsContainer))
        {
            UIHelper.DoEvents();

            Assert.That(groupBox.State, Is.EqualTo(RibbonGroupBoxState.Large));
            Assert.That(inRibbonGallery.IsCollapsed, Is.False);

            // Too narrow: the group goes to Middle, the gallery is shown as a button
            ribbonGroupsContainer.Width = 60;

            UIHelper.DoEvents();

            Assert.That(groupBox.State, Is.EqualTo(RibbonGroupBoxState.Middle));
            Assert.That(inRibbonGallery.IsCollapsed, Is.True);

            // Wide again: the group is Large again, so the gallery must show its items again
            ribbonGroupsContainer.Width = 1000;

            UIHelper.DoEvents();

            Assert.That(groupBox.State, Is.EqualTo(RibbonGroupBoxState.Large));
            Assert.That(inRibbonGallery.IsCollapsed, Is.False, "Gallery stayed collapsed to a button although its group is Large again.");
        }
    }

    [Test]
    public void Gallery_Should_Get_All_Columns_Back_After_Narrowing_And_Widening()
    {
        var ribbonGroupsContainer = new RibbonGroupsContainer
        {
            Height = RibbonTabControl.DefaultContentHeight,
            ReduceOrder = "MyGroup,(MyGroup)"
        };

        var groupBox = new RibbonGroupBox
        {
            Name = "MyGroup"
        };

        ribbonGroupsContainer.Children.Add(groupBox);

        var inRibbonGallery = new InRibbonGallery
        {
            MinItemsInRow = 1,
            MaxItemsInRow = 3,
            ItemWidth = 50,
            ItemHeight = 18,
            ItemsSource = this.sampleDataItemsForFixedWidth
        };

        groupBox.Items.Add(inRibbonGallery);

        using (new TestRibbonWindow(ribbonGroupsContainer))
        {
            UIHelper.DoEvents();

            Assert.That(inRibbonGallery.IsCollapsed, Is.False);
            Assert.That(inRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3));

            ribbonGroupsContainer.Width = 60;

            UIHelper.DoEvents();

            Assert.That(groupBox.State, Is.EqualTo(RibbonGroupBoxState.Middle));
            Assert.That(inRibbonGallery.IsCollapsed, Is.True);

            ribbonGroupsContainer.Width = 1000;

            UIHelper.DoEvents();

            Assert.That(groupBox.State, Is.EqualTo(RibbonGroupBoxState.Large));
            Assert.That(inRibbonGallery.IsCollapsed, Is.False);
            Assert.That(inRibbonGallery.CurrentGalleryPanelState.GalleryPanel.MaxItemsInRow, Is.EqualTo(3), "Gallery lost a column after narrowing and widening the ribbon.");
        }
    }

    private readonly SampleDataItem[] sampleDataItemsForFixedWidth =
    {
        new("A", "Blue"),
        new("A", "Brown"),
        new("A", "Gray"),
        new("A", "Green"),
        new("A", "Orange"),

        new("B", "Pink"),
        new("B", "Red"),
        new("B", "Yellow")
    };

    private readonly SampleDataItem[] sampleDataItemsForDynamicWidth =
    {
        new("A", "Blue"),
        new("A", "Brown"),
        new("A", "Hallo text text"),
        new("A", "Green"),
        new("A", "Orange"),

        new("B", "Pink"),
        new("B", "Red"),
        new("B", "Yellow")
    };

    private class SampleDataItem
    {
        public SampleDataItem(string group, string text)
        {
            this.Group = group;
            this.Text = text;
        }

        public string Group { get; }

        public string Text { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Text;
        }
    }
}