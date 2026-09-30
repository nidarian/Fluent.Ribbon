namespace Fluent.Tests.Controls;

using System.Linq;
using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using FluentTest.Commanding;
using NUnit.Framework;

[TestFixture]
public class SplitButtonTests
{
    [Test]
    public void Command_Should_Not_Disable_Control()
    {
        var splitButton = new SplitButton
        {
            Command = new RelayCommand(null, () => false)
        };

        using (new TestRibbonWindow(splitButton))
        {
            splitButton.ApplyTemplate();

            Assert.That(splitButton.IsEnabled, Is.True);

            var partButton = splitButton.Template.FindName("PART_Button", splitButton) as ToggleButton;

            Assert.That(partButton, Is.Not.Null);
            Assert.That(partButton.IsEnabled, Is.False);

            splitButton.Command = new RelayCommand(null, () => true);

            Assert.That(splitButton.IsEnabled, Is.True);
            Assert.That(partButton.IsEnabled, Is.True);
        }
    }

    [Test]
    public void Disabling_Control_Should_Disable_Popup()
    {
        var splitButton = new SplitButton
        {
            Command = new RelayCommand(null, () => false)
        };

        using (new TestRibbonWindow(splitButton))
        {
            splitButton.ApplyTemplate();

            Assert.That(splitButton.IsEnabled, Is.True);

            var dummyButton = new Button();

            splitButton.Items.Add(dummyButton);

            Assert.That(dummyButton.IsEnabled, Is.True);

            splitButton.IsDropDownOpen = true;

            splitButton.IsEnabled = false;

            Assert.That(splitButton.IsEnabled, Is.False);
            Assert.That(dummyButton.IsEnabled, Is.False);

            splitButton.IsDropDownOpen = false;

            Assert.That(splitButton.IsEnabled, Is.False);
            Assert.That(dummyButton.IsEnabled, Is.False);
        }
    }

    [Test]
    public void KeyTips_Should_Have_Postfix()
    {
        {
            var splitButton = new SplitButton
            {
                KeyTip = "Z"
            };

            using (new TestRibbonWindow(splitButton))
            {
                var keyTipInformations = splitButton.GetKeyTipInformations(false).ToList();
                Assert.That(keyTipInformations, Has.Count.EqualTo(2));
                Assert.That(keyTipInformations[0].Keys, Is.EqualTo("ZA"));
                Assert.That(keyTipInformations[1].Keys, Is.EqualTo("ZB"));
            }
        }

        {
            var splitButton = new SplitButton
            {
                KeyTip = "Z",
                PrimaryActionKeyTipPostfix = "X",
                SecondaryActionKeyTipPostfix = "Y"
            };

            using (new TestRibbonWindow(splitButton))
            {
                var keyTipInformations = splitButton.GetKeyTipInformations(false).ToList();
                Assert.That(keyTipInformations, Has.Count.EqualTo(2));
                Assert.That(keyTipInformations[0].Keys, Is.EqualTo("ZX"));
                Assert.That(keyTipInformations[1].Keys, Is.EqualTo("ZY"));
            }
        }
    }

    [Test]
    public void KeyTips_Should_Work_With_Secondary_KeyTip()
    {
        {
            var splitButton = new SplitButton
            {
                SecondaryKeyTip = "Z"
            };

            using (new TestRibbonWindow(splitButton))
            {
                var keyTipInformations = splitButton.GetKeyTipInformations(false).ToList();
                Assert.That(keyTipInformations, Has.Count.EqualTo(1));
                Assert.That(keyTipInformations[0].Keys, Is.EqualTo("Z"));
            }
        }

        {
            var splitButton = new SplitButton
            {
                KeyTip = "X",
                SecondaryKeyTip = "Z"
            };

            using (new TestRibbonWindow(splitButton))
            {
                var keyTipInformations = splitButton.GetKeyTipInformations(false).ToList();
                Assert.That(keyTipInformations, Has.Count.EqualTo(2));
                Assert.That(keyTipInformations[0].Keys, Is.EqualTo("X"));
                Assert.That(keyTipInformations[1].Keys, Is.EqualTo("Z"));
            }
        }
    }

    /// <summary>
    /// Pressing Enter on a focused SplitButton clicks its button part. It did that even when the
    /// button part was disabled (IsButtonEnabled = false, which is how an app greys out the main
    /// action while keeping the drop down usable): Click was raised and the app's handler ran.
    /// </summary>
    [Test]
    public void Enter_does_not_click_a_disabled_button_part()
    {
        var clicks = 0;
        var splitButton = new SplitButton
        {
            Header = "Split",
            Items = { new MenuItem { Header = "Item" } }
        };
        splitButton.Click += (_, _) => clicks++;

        using (new TestRibbonWindow(splitButton))
        {
            splitButton.ApplyTemplate();
            UIHelper.DoEvents();

            // Baseline: with the button part enabled, Enter clicks it. Proves the key reaches the control.
            PressKey(splitButton, Key.Enter);
            Assert.That(clicks, Is.EqualTo(1), "Precondition: Enter clicks an enabled button part");

            splitButton.IsDropDownOpen = false;
            splitButton.IsButtonEnabled = false;
            UIHelper.DoEvents();

            Assert.That(splitButton.Button?.IsEnabled, Is.False, "Precondition: the button part is disabled");

            PressKey(splitButton, Key.Enter);

            Assert.That(clicks, Is.EqualTo(1), "Enter must not click a disabled button part");
        }
    }

    // Simulates a key press the way WPF delivers it: a KeyDown event on the focused element.
    private static void PressKey(UIElement target, Key key)
    {
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(target), 0, key)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };

        target.RaiseEvent(args);
        UIHelper.DoEvents();
    }
}
