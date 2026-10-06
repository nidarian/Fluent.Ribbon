namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// A collapsed <see cref="RibbonGroupBox"/> acts as a drop down button, and buttons are activated with Space and Enter.
/// RibbonGroupBox.OnKeyDown only opened the drop down on Space.
/// </summary>
[TestFixture]
public class RibbonGroupBoxKeyboardTests
{
    [Test]
    public void Enter_opens_collapsed_group()
    {
        var groupBox = new RibbonGroupBox
        {
            Header = "Group",
            Items =
            {
                new Button { Header = "Button" }
            }
        };

        using (new TestRibbonWindow(groupBox))
        {
            groupBox.State = RibbonGroupBoxState.Collapsed;
            UIHelper.DoEvents();

            Assert.That(groupBox.IsInButtonState, Is.True, "precondition: a collapsed group is in button state");

            // Baseline: Space opens the drop down, so opening works in this environment.
            PressKey(groupBox, Key.Space);
            Assert.That(groupBox.IsDropDownOpen, Is.True, "precondition: Space should open the collapsed group");

            groupBox.IsDropDownOpen = false;
            UIHelper.DoEvents();
            Assert.That(groupBox.IsDropDownOpen, Is.False, "precondition: drop down should be closed again");

            PressKey(groupBox, Key.Enter);

            Assert.That(groupBox.IsDropDownOpen, Is.True, "Enter should open the collapsed group like Space does");
        }
    }

    // Keys pressed inside the open drop down of a collapsed group bubble up through the popup to the group box.
    // The group box handled Enter from anywhere, so Enter typed into a text box in the drop down was swallowed:
    // window-level Enter key bindings, KeyDown handlers and default buttons never saw it.
    [Test]
    public void Enter_typed_into_a_text_box_inside_the_drop_down_is_not_swallowed_by_the_group()
    {
        var textBox = new System.Windows.Controls.TextBox();
        var groupBox = CreateGroupBoxWith(textBox);

        using (var window = new TestRibbonWindow(groupBox))
        {
            var entersSeenByWindow = 0;
            window.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    entersSeenByWindow++;
                }
            };

            OpenCollapsedGroup(groupBox, textBox);

            var args = PressKey(textBox, Key.Enter);

            Assert.That(groupBox.IsDropDownOpen, Is.True, "Enter typed into the text box must not close the drop down");
            Assert.That(args.Handled, Is.False, "Enter typed into the text box must not be handled by the group");
            Assert.That(entersSeenByWindow, Is.EqualTo(1), "Enter typed into the text box should reach the window");
        }
    }

    // Space is checked too, because the group box also handles Space. A text box may handle Space itself,
    // so only check that the drop down stays open.
    [Test]
    public void Space_typed_into_a_text_box_inside_the_drop_down_keeps_it_open()
    {
        var textBox = new System.Windows.Controls.TextBox();
        var groupBox = CreateGroupBoxWith(textBox);

        using (new TestRibbonWindow(groupBox))
        {
            OpenCollapsedGroup(groupBox, textBox);

            PressKey(textBox, Key.Space);

            Assert.That(groupBox.IsDropDownOpen, Is.True, "Space typed into the text box must not close the drop down");
        }
    }

    private static RibbonGroupBox CreateGroupBoxWith(UIElement content)
    {
        return new RibbonGroupBox
        {
            Header = "Group",
            Items =
            {
                content
            }
        };
    }

    private static void OpenCollapsedGroup(RibbonGroupBox groupBox, UIElement content)
    {
        groupBox.State = RibbonGroupBoxState.Collapsed;
        UIHelper.DoEvents();

        groupBox.IsDropDownOpen = true;
        UIHelper.DoEvents();

        Assert.That(groupBox.IsDropDownOpen, Is.True, "precondition: the drop down should be open");
        Assert.That(PresentationSource.FromVisual(content), Is.Not.Null, "precondition: the content should be shown in the drop down");
    }

    // Simulates a key press the way WPF input delivers it: PreviewKeyDown (tunnel) and then KeyDown (bubble)
    // sharing one args object.
    private static KeyEventArgs PressKey(UIElement target, Key key)
    {
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(target), 0, key)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        };

        target.RaiseEvent(args);

        args.RoutedEvent = Keyboard.KeyDownEvent;
        target.RaiseEvent(args);

        UIHelper.DoEvents();

        return args;
    }
}
