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

    // Simulates a key press the way WPF input delivers it: PreviewKeyDown (tunnel) and then KeyDown (bubble)
    // sharing one args object.
    private static void PressKey(UIElement target, Key key)
    {
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(target), 0, key)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        };

        target.RaiseEvent(args);

        args.RoutedEvent = Keyboard.KeyDownEvent;
        target.RaiseEvent(args);

        UIHelper.DoEvents();
    }
}
