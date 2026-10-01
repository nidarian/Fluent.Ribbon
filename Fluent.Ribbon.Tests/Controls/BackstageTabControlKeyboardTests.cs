namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tab on a <see cref="BackstageTabItem"/> jumps into the selected content (that's the intended shortcut),
/// but Shift+Tab is the reverse direction and must be left to normal keyboard navigation.
/// BackstageTabControl.OnKeyDown checked only for <see cref="Key.Tab"/> and ignored Shift.
/// </summary>
[TestFixture]
public class BackstageTabControlKeyboardTests
{
    [Test]
    public void Shift_Tab_on_tab_item_does_not_move_focus_into_content()
    {
        var contentButton = new System.Windows.Controls.Button { Content = "In content" };
        var tabItem = new BackstageTabItem
        {
            Header = "Tab",
            Content = contentButton
        };

        var tabControl = new BackstageTabControl
        {
            Items =
            {
                tabItem
            }
        };

        using (var window = new TestRibbonWindow(tabControl))
        {
            UIHelper.DoEvents();

            Assert.That(tabItem.IsSelected, Is.True, "precondition: the only tab should be selected");
            Assert.That(tabControl.SelectedContentHost, Is.Not.Null, "precondition: template should provide PART_SelectedContentHost");

            // Keyboard focus only works in the active window.
            window.Activate();
            FocusTabItem(tabItem);

            // Baseline: plain Tab is handled and moves focus into the content. This proves focus works here,
            // so the Shift+Tab check below can't pass just because MoveFocus did nothing.
            var tabHandled = PressKey(tabItem, Key.Tab, ModifierKeys.None);
            if (tabHandled == false
                || contentButton.IsKeyboardFocused == false)
            {
                Assert.Inconclusive("Keyboard focus is not available in this test environment.");
            }

            FocusTabItem(tabItem);
            Assert.That(tabControl.SelectedContentHost.IsKeyboardFocusWithin, Is.False, "precondition: focus should be back on the tab item");

            var shiftTabHandled = PressKey(tabItem, Key.Tab, ModifierKeys.Shift);

            Assert.That(shiftTabHandled, Is.False, "Shift+Tab should be left to normal keyboard navigation");
            Assert.That(contentButton.IsKeyboardFocused, Is.False, "Shift+Tab should not move focus forward into the content");
        }
    }

    private static void FocusTabItem(BackstageTabItem tabItem)
    {
        tabItem.Focus();
        UIHelper.DoEvents();

        if (tabItem.IsKeyboardFocused == false)
        {
            Assert.Inconclusive("Keyboard focus is not available in this test environment.");
        }
    }

    // Simulates a key press the way WPF input delivers it: PreviewKeyDown (tunnel) and then KeyDown (bubble)
    // sharing one args object. Returns whether the key was handled.
    private static bool PressKey(UIElement target, Key key, ModifierKeys modifiers)
    {
        // The real keyboard can't be made to hold Shift in a headless test, so a fake device reports the modifiers.
        var keyboardDevice = modifiers == ModifierKeys.None
            ? Keyboard.PrimaryDevice
            : FakeModifiersKeyboardDevice.Create(modifiers);

        var args = new KeyEventArgs(keyboardDevice, PresentationSource.FromVisual(target), 0, key)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        };

        target.RaiseEvent(args);

        args.RoutedEvent = Keyboard.KeyDownEvent;
        target.RaiseEvent(args);

        UIHelper.DoEvents();

        return args.Handled;
    }

    /// <summary>
    /// A <see cref="KeyboardDevice"/> that only answers key state queries (which is what <see cref="KeyboardDevice.Modifiers"/> uses).
    /// It is created without running the constructor, because the constructor would register the device
    /// with the process wide <see cref="InputManager"/> for the rest of the test run.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Created via GetUninitializedObject, see Create.")]
    private sealed class FakeModifiersKeyboardDevice : KeyboardDevice
    {
        private ModifierKeys modifiers;

        // Never called, see Create. It only exists because KeyboardDevice has no parameterless constructor.
        private FakeModifiersKeyboardDevice()
            : base(InputManager.Current)
        {
        }

        public static FakeModifiersKeyboardDevice Create(ModifierKeys modifiers)
        {
#if NETFRAMEWORK
            var device = (FakeModifiersKeyboardDevice)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(FakeModifiersKeyboardDevice));
#else
            var device = (FakeModifiersKeyboardDevice)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(FakeModifiersKeyboardDevice));
#endif
            device.modifiers = modifiers;
            return device;
        }

        protected override KeyStates GetKeyStatesFromSystem(Key key)
        {
            var isDown = key switch
            {
                Key.LeftShift or Key.RightShift => (this.modifiers & ModifierKeys.Shift) == ModifierKeys.Shift,
                Key.LeftCtrl or Key.RightCtrl => (this.modifiers & ModifierKeys.Control) == ModifierKeys.Control,
                Key.LeftAlt or Key.RightAlt => (this.modifiers & ModifierKeys.Alt) == ModifierKeys.Alt,
                Key.LWin or Key.RWin => (this.modifiers & ModifierKeys.Windows) == ModifierKeys.Windows,
                _ => false
            };

            return isDown ? KeyStates.Down : KeyStates.None;
        }
    }
}
