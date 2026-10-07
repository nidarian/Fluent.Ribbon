namespace Fluent.Tests.Adorners;

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class KeyTipAdornerTests
{
    [Test]
    public void Adorner_Should_Properly_Grab_Keys_From_KeyTipInformationProvider()
    {
        {
            var splitButton = new SplitButton();
            var panel = new Grid();
            panel.Children.Add(splitButton);
            using (var window = new TestRibbonWindow(panel))
            {
                var adorner = new KeyTipAdorner(splitButton, panel, null);

                Assert.That(adorner.KeyTipInformations, Has.Count.EqualTo(0));
            }
        }

        {
            var splitButton = new SplitButton
            {
                KeyTip = "A"
            };
            var panel = new Grid();
            panel.Children.Add(splitButton);

            using (var window = new TestRibbonWindow(panel))
            {
                var adorner = new KeyTipAdorner(splitButton, panel, null);

                Assert.That(adorner.KeyTipInformations, Has.Count.EqualTo(2));
                Assert.That(adorner.KeyTipInformations[0].Keys, Is.EqualTo("AA"));
                Assert.That(adorner.KeyTipInformations[1].Keys, Is.EqualTo("AB"));
            }
        }

        {
            var splitButton = new SplitButton
            {
                SecondaryKeyTip = "B"
            };
            var panel = new Grid();
            panel.Children.Add(splitButton);

            using (var window = new TestRibbonWindow(panel))
            {
                var adorner = new KeyTipAdorner(splitButton, panel, null);

                Assert.That(adorner.KeyTipInformations, Has.Count.EqualTo(1));
                Assert.That(adorner.KeyTipInformations[0].Keys, Is.EqualTo("B"));
            }
        }
    }

    /// <summary>
    /// KeyTips of controls inside a ribbon group are meant to snap to the group's rows
    /// (added for https://github.com/fluentribbon/Fluent.Ribbon/issues/572).
    /// The snapping was moved into SnapToRowsIfPresent, which took the position as a Point
    /// (a struct) by value, so the snapped value was computed and then thrown away.
    /// </summary>
    [Test]
    public void KeyTips_in_a_group_snap_to_the_group_rows()
    {
        // SizeDefinition="Small" keeps the button small whatever the group's state,
        // so its KeyTip goes through the "small control" placement.
        var button = new Fluent.Button
        {
            Header = "Small",
            KeyTip = "S",
            SizeDefinition = "Small"
        };

        var groupBox = new RibbonGroupBox { Header = "Group" };
        groupBox.Items.Add(button);

        using (new TestRibbonWindow(groupBox))
        {
            groupBox.ApplyTemplate();
            UIHelper.DoEvents();

            var adorner = new KeyTipAdorner(groupBox, groupBox, null);

            // Measuring the adorner is what computes KeyTip positions.
            adorner.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var keyTipInformation = adorner.KeyTipInformations.Single(x => ReferenceEquals(x.AssociatedElement, button));

            // The same row lines the adorner uses: top, middle and bottom of the group's panel, and below it.
            var layoutRoot = groupBox.GetLayoutRoot();
            var panel = groupBox.GetPanel();
            Assert.That(layoutRoot, Is.Not.Null);
            Assert.That(panel, Is.Not.Null);

            var rows = new[]
            {
                layoutRoot.TranslatePoint(new Point(0, 0), groupBox).Y,
                layoutRoot.TranslatePoint(new Point(0, panel.DesiredSize.Height / 2.0), groupBox).Y,
                layoutRoot.TranslatePoint(new Point(0, panel.DesiredSize.Height), groupBox).Y,
                layoutRoot.TranslatePoint(new Point(0, layoutRoot.DesiredSize.Height + 1), groupBox).Y
            };

            // Snapping places the KeyTip's vertical center exactly on a row line.
            var keyTipCenterY = keyTipInformation.Position.Y + (keyTipInformation.KeyTip.DesiredSize.Height / 2.0);

            Assert.That(rows.Any(row => Math.Abs(row - keyTipCenterY) < 0.01), Is.True, $"KeyTip center {keyTipCenterY} should be on one of the rows {string.Join(", ", rows)}");
        }
    }

    /// <summary>
    /// https://github.com/fluentribbon/Fluent.Ribbon/issues/357
    /// Standard WPF controls that implement <see cref="IKeyTipedControl"/> themselves (like a slider)
    /// used to get the centered placement meant for large ribbon buttons.
    /// They should be placed like the other small, text box shaped controls instead.
    /// </summary>
    [Test]
    public void Custom_IKeyTipedControl_is_placed_like_a_text_box_shaped_control()
    {
        Assert.That(IsTextBoxShapedControl(new KeyTipedSlider()), Is.True, "A standard control implementing IKeyTipedControl");
        Assert.That(IsTextBoxShapedControl(new TextBox()), Is.True, "TextBox was already handled");
        Assert.That(IsTextBoxShapedControl(new Slider()), Is.False, "Controls without KeyTip support are unchanged");
        Assert.That(IsTextBoxShapedControl(new Fluent.Button()), Is.False, "Ribbon controls keep their size based placement");
    }

    /// <summary>
    /// The #357 rule ("implements <see cref="IKeyTipedControl"/> but not <see cref="IRibbonControl"/>") is meant for
    /// controls from outside the library. Fluent's own <see cref="RibbonGroupBox"/>, <see cref="GalleryItem"/> and
    /// <see cref="BackstageTabItem"/> match it too, so they lost the placement they had before #357.
    /// </summary>
    [Test]
    public void Fluent_controls_with_KeyTips_are_not_text_box_shaped()
    {
        Assert.That(IsTextBoxShapedControl(new RibbonGroupBox()), Is.False, nameof(RibbonGroupBox));
        Assert.That(IsTextBoxShapedControl(new GalleryItem()), Is.False, nameof(GalleryItem));
        Assert.That(IsTextBoxShapedControl(new BackstageTabItem()), Is.False, nameof(BackstageTabItem));
    }

    /// <summary>
    /// A collapsed group is shown as one large button, and its KeyTip is meant to sit at the bottom center like
    /// the KeyTips of other large buttons. Since #357 it was put in the top left corner instead.
    /// </summary>
    [Test]
    public void Collapsed_group_KeyTip_is_centered_horizontally()
    {
        // A fixed width, aligned left, so the group's size doesn't depend on its header or the window.
        var groupBox = new RibbonGroupBox
        {
            Header = "Group",
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            Items =
            {
                new Fluent.Button { Header = "Button" }
            }
        };

        KeyTip.SetKeys(groupBox, "ZC");

        var panel = new StackPanel { Children = { groupBox } };

        using (new TestRibbonWindow(panel))
        {
            groupBox.ApplyTemplate();
            groupBox.State = RibbonGroupBoxState.Collapsed;
            UIHelper.DoEvents();

            Assert.That(groupBox.State, Is.EqualTo(RibbonGroupBoxState.Collapsed), "Precondition: the group is collapsed");
            Assert.That(RibbonProperties.GetSize(groupBox), Is.EqualTo(RibbonControlSize.Large), "Precondition: the group has the default (large) size");

            var adorner = new KeyTipAdorner(panel, panel, null);

            // Measuring the adorner is what computes KeyTip positions.
            adorner.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var keyTipInformation = adorner.KeyTipInformations.Single(x => ReferenceEquals(x.AssociatedElement, groupBox));
            Assert.That(keyTipInformation.Visibility, Is.EqualTo(Visibility.Visible), "Precondition: a collapsed group's own KeyTip is shown");

            var groupCenterX = groupBox.TranslatePoint(new Point(groupBox.ActualWidth / 2.0, 0), panel).X;
            var keyTipCenterX = keyTipInformation.Position.X + (keyTipInformation.KeyTip.DesiredSize.Width / 2.0);

            Assert.That(keyTipCenterX, Is.EqualTo(groupCenterX).Within(0.5), "The KeyTip of a collapsed group should be centered horizontally on the group");
        }
    }

    /// <summary>
    /// https://github.com/nidarian/Fluent.Ribbon/issues/5
    /// The rows used for snapping were taken from the first KeyTip's element. On a tab that is often a group box
    /// itself, and for a collapsed group these rows come from its drop down content (not shown), not from the button.
    /// Since snapping took effect, the KeyTip of a collapsed group was moved to the top of the window.
    /// </summary>
    [Test]
    public void Collapsed_group_KeyTip_is_placed_on_the_collapsed_group()
    {
        var groupBox = CreateCollapsibleGroupBox("ZC", new Fluent.Button { Header = "Button" });

        // Something above the group, so "the top of the window" and "the group" are different places.
        var panel = new StackPanel { Children = { new Border { Height = 100 }, groupBox } };

        using (new TestRibbonWindow(panel))
        {
            groupBox.ApplyTemplate();
            groupBox.State = RibbonGroupBoxState.Collapsed;
            UIHelper.DoEvents();

            Assert.That(groupBox.State, Is.EqualTo(RibbonGroupBoxState.Collapsed), "Precondition: the group is collapsed");

            var adorner = new KeyTipAdorner(panel, panel, null);

            // Measuring the adorner is what computes KeyTip positions.
            adorner.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var keyTipInformation = adorner.KeyTipInformations.Single(x => ReferenceEquals(x.AssociatedElement, groupBox));
            Assert.That(keyTipInformation.Visibility, Is.EqualTo(Visibility.Visible), "Precondition: a collapsed group's own KeyTip is shown");

            AssertKeyTipTopIsWithinElement(keyTipInformation, groupBox, panel);
        }
    }

    /// <summary>
    /// https://github.com/nidarian/Fluent.Ribbon/issues/5
    /// A tab with a collapsed group and an expanded group: the controls in the expanded group still snap to its rows,
    /// and the collapsed group's KeyTip stays on the collapsed group.
    /// </summary>
    [Test]
    public void KeyTips_snap_to_rows_of_expanded_groups_next_to_a_collapsed_group()
    {
        var collapsedGroupBox = CreateCollapsibleGroupBox("ZC", new Fluent.Button { Header = "Button" });

        // SizeDefinition="Small" keeps the button small, so its KeyTip goes through the "small control" placement.
        var button = new Fluent.Button
        {
            Header = "Small",
            KeyTip = "S",
            SizeDefinition = "Small"
        };
        var expandedGroupBox = CreateCollapsibleGroupBox("ZE", button);

        // The collapsed group comes first, like groupLL on the Showcase's Insert tab.
        var groups = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { collapsedGroupBox, expandedGroupBox }
        };

        var panel = new StackPanel { Children = { new Border { Height = 100 }, groups } };

        using (new TestRibbonWindow(panel))
        {
            collapsedGroupBox.ApplyTemplate();
            expandedGroupBox.ApplyTemplate();
            collapsedGroupBox.State = RibbonGroupBoxState.Collapsed;
            UIHelper.DoEvents();

            Assert.That(collapsedGroupBox.State, Is.EqualTo(RibbonGroupBoxState.Collapsed), "Precondition: the first group is collapsed");
            Assert.That(expandedGroupBox.State, Is.Not.EqualTo(RibbonGroupBoxState.Collapsed), "Precondition: the second group is expanded");

            var adorner = new KeyTipAdorner(panel, panel, null);

            // Measuring the adorner is what computes KeyTip positions.
            adorner.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var groupKeyTip = adorner.KeyTipInformations.Single(x => ReferenceEquals(x.AssociatedElement, collapsedGroupBox));
            Assert.That(groupKeyTip.Visibility, Is.EqualTo(Visibility.Visible), "Precondition: a collapsed group's own KeyTip is shown");
            AssertKeyTipTopIsWithinElement(groupKeyTip, collapsedGroupBox, panel);

            var buttonKeyTip = adorner.KeyTipInformations.Single(x => ReferenceEquals(x.AssociatedElement, button));
            Assert.That(buttonKeyTip.Visibility, Is.EqualTo(Visibility.Visible), "Precondition: the button's KeyTip is shown");

            // The rows of the button's own group: top, middle and bottom of the group's panel, and below it.
            var layoutRoot = expandedGroupBox.GetLayoutRoot();
            var groupPanel = expandedGroupBox.GetPanel();
            Assert.That(layoutRoot, Is.Not.Null);
            Assert.That(groupPanel, Is.Not.Null);

            var rows = new[]
            {
                layoutRoot.TranslatePoint(new Point(0, 0), panel).Y,
                layoutRoot.TranslatePoint(new Point(0, groupPanel.DesiredSize.Height / 2.0), panel).Y,
                layoutRoot.TranslatePoint(new Point(0, groupPanel.DesiredSize.Height), panel).Y,
                layoutRoot.TranslatePoint(new Point(0, layoutRoot.DesiredSize.Height + 1), panel).Y
            };

            var keyTipCenterY = buttonKeyTip.Position.Y + (buttonKeyTip.KeyTip.DesiredSize.Height / 2.0);

            Assert.That(rows.Any(row => Math.Abs(row - keyTipCenterY) < 0.01), Is.True, $"KeyTip center {keyTipCenterY} should be on one of the rows {string.Join(", ", rows)} of its own group");
        }
    }

    /// <summary>
    /// https://github.com/nidarian/Fluent.Ribbon/issues/5
    /// Inside the open drop down of a collapsed group the rows start at the drop down's top edge.
    /// Snapping centered the KeyTips of the top controls on that edge, so half of each KeyTip was outside the drop down.
    /// </summary>
    [Test]
    public void KeyTips_in_the_drop_down_of_a_collapsed_group_stay_inside_the_drop_down()
    {
        // Small buttons, stacked like in a ribbon group, so the top button is far from the middle row.
        var button = new Fluent.Button { Header = "Small 1", KeyTip = "S", SizeDefinition = "Small" };
        var groupBox = CreateCollapsibleGroupBox(
            "ZC",
            button,
            new Fluent.Button { Header = "Small 2", KeyTip = "T", SizeDefinition = "Small" },
            new Fluent.Button { Header = "Small 3", KeyTip = "U", SizeDefinition = "Small" });

        var panel = new StackPanel { Children = { groupBox } };

        using (new TestRibbonWindow(panel))
        {
            groupBox.ApplyTemplate();
            groupBox.State = RibbonGroupBoxState.Collapsed;
            UIHelper.DoEvents();

            groupBox.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(groupBox.IsDropDownOpen, Is.True, "Precondition: the drop down is open");
            Assert.That(PresentationSource.FromVisual(button), Is.Not.Null, "Precondition: the button is shown in the drop down");

            // The adorner KeyTipAdorner.Forward creates after the collapsed group's KeyTip was pressed:
            // it adorns the content of the drop down and looks for KeyTips in the group.
            var adorner = new KeyTipAdorner(button, groupBox, null);

            // Measuring the adorner is what computes KeyTip positions.
            adorner.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var keyTipInformation = adorner.KeyTipInformations.Single(x => ReferenceEquals(x.AssociatedElement, button));
            Assert.That(keyTipInformation.Visibility, Is.EqualTo(Visibility.Visible), "Precondition: the button's KeyTip is shown");

            // The drop down's content (the group's layout root moves into the drop down while the group is collapsed).
            var layoutRoot = groupBox.GetLayoutRoot();
            Assert.That(layoutRoot, Is.Not.Null);
            var dropDownContentTop = layoutRoot.TranslatePoint(new Point(0, 0), button).Y;

            Assert.That(keyTipInformation.Position.Y, Is.GreaterThanOrEqualTo(dropDownContentTop - 0.5), "The KeyTip's top edge should not be above the drop down's content");
        }
    }

    /// <summary>
    /// In the drop down of a collapsed group there are no rows to snap to (#5), so small and text box shaped KeyTips
    /// kept their top edge half a KeyTip below the control's top edge: for the top row that looked flush against the
    /// top of the drop down (FD and KET in the Showcase, hand check 2026-10-07).
    /// They are centered on the control's bottom edge instead, like small controls sit on a row line in the ribbon.
    /// </summary>
    [Test]
    public void KeyTips_in_the_drop_down_of_a_collapsed_group_are_centered_on_the_bottom_edge_of_their_control()
    {
        // A combo box (text box shaped, like FD) and a small button (like the "small control" placement).
        var comboBox = new Fluent.ComboBox { Header = "Fonts", KeyTip = "FD", IsEditable = false };
        var button = new Fluent.Button { Header = "Small 1", KeyTip = "S", SizeDefinition = "Small" };
        var groupBox = CreateCollapsibleGroupBox(
            "ZC",
            comboBox,
            button,
            new Fluent.Button { Header = "Small 2", KeyTip = "T", SizeDefinition = "Small" });

        var panel = new StackPanel { Children = { groupBox } };

        using (new TestRibbonWindow(panel))
        {
            groupBox.ApplyTemplate();
            groupBox.State = RibbonGroupBoxState.Collapsed;
            UIHelper.DoEvents();

            groupBox.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(groupBox.IsDropDownOpen, Is.True, "Precondition: the drop down is open");
            Assert.That(PresentationSource.FromVisual(comboBox), Is.Not.Null, "Precondition: the combo box is shown in the drop down");
            Assert.That(PresentationSource.FromVisual(button), Is.Not.Null, "Precondition: the button is shown in the drop down");

            // The adorner KeyTipAdorner.Forward creates after the collapsed group's KeyTip was pressed.
            var adorner = new KeyTipAdorner(button, groupBox, null);

            // Measuring the adorner is what computes KeyTip positions.
            adorner.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            foreach (var element in new FrameworkElement[] { comboBox, button })
            {
                var keyTipInformation = adorner.KeyTipInformations.Single(x => ReferenceEquals(x.AssociatedElement, element));
                Assert.That(keyTipInformation.Visibility, Is.EqualTo(Visibility.Visible), $"Precondition: the KeyTip of the {element.GetType().Name} is shown");

                // Positions are relative to the adorned element (the button).
                var bottomEdgeY = keyTipInformation.VisualTarget.TranslatePoint(new Point(0, keyTipInformation.VisualTarget.RenderSize.Height), button).Y;
                var keyTipCenterY = keyTipInformation.Position.Y + (keyTipInformation.KeyTip.DesiredSize.Height / 2.0);

                Assert.That(keyTipCenterY, Is.EqualTo(bottomEdgeY).Within(0.5), $"The KeyTip of the {element.GetType().Name} should be centered on its bottom edge");
            }
        }
    }

    // A group with the height it has in a ribbon, aligned top left, so its size doesn't depend on the window.
    private static RibbonGroupBox CreateCollapsibleGroupBox(string keys, params UIElement[] items)
    {
        var groupBox = new RibbonGroupBox
        {
            Header = "Group",
            Width = 200,
            Height = 94,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        foreach (var item in items)
        {
            groupBox.Items.Add(item);
        }

        KeyTip.SetKeys(groupBox, keys);

        return groupBox;
    }

    // The KeyTip's top edge has to be on the element (the KeyTip itself may reach below it).
    private static void AssertKeyTipTopIsWithinElement(KeyTipInformation keyTipInformation, FrameworkElement element, UIElement relativeTo)
    {
        var elementTop = element.TranslatePoint(new Point(0, 0), relativeTo).Y;
        var elementBottom = elementTop + element.ActualHeight;

        Assert.That(keyTipInformation.Position.Y, Is.InRange(elementTop - 0.5, elementBottom + 0.5), $"The KeyTip's top edge should be on {element} (from {elementTop} to {elementBottom})");
    }

    // The placement decision is private, so it's called through reflection to test it directly.
    private static bool IsTextBoxShapedControl(FrameworkElement element)
    {
        var method = typeof(KeyTipAdorner).GetMethod("IsTextBoxShapedControl", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null, "KeyTipAdorner.IsTextBoxShapedControl was renamed or removed");

        return (bool)method.Invoke(null, new object[] { element });
    }

    // A standard WPF control with KeyTip support, like the one asked about in #357.
    private sealed class KeyTipedSlider : Slider, IKeyTipedControl
    {
        public string KeyTip { get; set; }

        public KeyTipPressedResult OnKeyTipPressed()
        {
            this.Focus();
            return KeyTipPressedResult.Empty;
        }

        public void OnKeyTipBack()
        {
        }
    }

    /// <summary>
    /// A group's own KeyTip (like "ZC") is only shown while the group is collapsed. While it's
    /// expanded, the KeyTip is hidden and can't be pressed: activation only considers visible KeyTips.
    /// The prefix check didn't look at visibility, so typing "Z" counted as the start of the hidden "ZC".
    /// The service then kept waiting for more keys while every KeyTip was hidden, and the key was swallowed.
    /// </summary>
    [Test]
    public void Hidden_KeyTips_do_not_count_as_a_prefix()
    {
        var button = new Fluent.Button { Header = "Button", KeyTip = "B" };

        // Expanded group (not Collapsed): its own KeyTip is hidden, its button's KeyTip is shown.
        var groupBox = new RibbonGroupBox { Header = "Group" };
        KeyTip.SetKeys(groupBox, "ZC");
        groupBox.Items.Add(button);

        var panel = new StackPanel { Children = { groupBox } };

        using (new TestRibbonWindow(panel))
        {
            groupBox.ApplyTemplate();
            UIHelper.DoEvents();

            Assert.That(groupBox.State, Is.Not.EqualTo(RibbonGroupBoxState.Collapsed), "Precondition: the group is expanded");

            var adorner = new KeyTipAdorner(panel, panel, null);

            var groupKeyTip = adorner.KeyTipInformations.Single(x => ReferenceEquals(x.AssociatedElement, groupBox));
            Assert.That(groupKeyTip.Visibility, Is.Not.EqualTo(Visibility.Visible), "Precondition: an expanded group's own KeyTip is hidden");

            // Visible KeyTips still match.
            Assert.That(adorner.ContainsKeyTipStartingWith("B"), Is.True);

            // The hidden one can't be pressed, so it must not keep the input going either.
            Assert.That(adorner.ContainsKeyTipStartingWith("Z"), Is.False, "Hidden KeyTip \"ZC\" must not match the prefix \"Z\"");
        }
    }

    [Test(Description = "Terminating an adorner which still waits for its element to be loaded must cancel the pending attach")]
    public void Terminate_should_cancel_an_attach_waiting_for_Loaded()
    {
        // Fully qualified, because "Button" would resolve to System.Windows.Controls.Button here.
        var button = new Fluent.Button
        {
            KeyTip = "A"
        };
        var panel = new Grid();
        panel.Children.Add(button);

        var adorner = new KeyTipAdorner(button, panel, null);

        Assert.That(button.IsLoaded, Is.False, "Precondition: the element must not be loaded yet.");

        // Attach has to wait for Loaded because the element is not loaded yet.
        adorner.Attach();

        Assert.That(adorner.IsAdornerChainAlive, Is.True, "Precondition: the adorner must be waiting for Loaded.");

        adorner.Terminate(KeyTipPressedResult.Empty);

        Assert.That(adorner.IsAdornerChainAlive, Is.False, "A terminated adorner must no longer wait for Loaded.");

        using (new TestRibbonWindow(panel))
        {
            Assert.That(button.IsLoaded, Is.True, "Precondition: the element must be loaded now.");

            // Loaded must not attach the terminated adorner, otherwise key tips of a dead chain would show up.
            Assert.That(adorner.IsAdornerChainAlive, Is.False, "A terminated adorner must not attach once its element gets loaded.");
        }
    }
}
