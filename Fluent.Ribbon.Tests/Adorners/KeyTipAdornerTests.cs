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
}
