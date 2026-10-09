namespace Fluent.Tests.Controls;

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// The ribbon always draws contextual tabs after the normal tabs (RibbonTabsContainer.ArrangeOverride),
/// even when a normal tab comes after a contextual one in <see cref="Ribbon.Tabs"/>
/// (for example a plugin that calls Tabs.Add at runtime while contextual tabs exist).
/// The mouse wheel follows that on-screen order, but Ctrl+Tab, Ctrl+Shift+Tab, Home and End
/// used the order of the Tabs collection. So Ctrl+Tab jumped back and forth on screen,
/// and End didn't select the rightmost tab.
/// </summary>
[TestFixture]
public class RibbonTabControlKeyboardOrderTests
{
    [Test]
    public void Keyboard_navigation_follows_the_on_screen_order_when_a_normal_tab_comes_after_a_contextual_tab()
    {
        var normal1 = new RibbonTabItem { Header = "Normal 1" };
        var contextual1 = new RibbonTabItem { Header = "Contextual 1" };
        var normal2 = new RibbonTabItem { Header = "Normal 2" };

        var group = new RibbonContextualTabGroup { Header = "Group", Visibility = Visibility.Visible };
        contextual1.Group = group;

        var ribbon = new Ribbon
        {
            ContextualGroups = { group },
            Tabs = { normal1, contextual1, normal2 }
        };

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            var onScreen = GetOnScreenOrder(ribbon);

            // The ribbon draws the contextual tab last. This is the order the user sees.
            Assert.That(onScreen.Select(GetHeader), Is.EqualTo(new[] { "Normal 1", "Normal 2", "Contextual 1" }), "Precondition: contextual tabs are drawn after the normal tabs");

            AssertKeyboardNavigationFollows(ribbon, onScreen);
        }
    }

    [Test]
    public void Keyboard_navigation_follows_the_on_screen_order_without_contextual_tabs()
    {
        var ribbon = new Ribbon
        {
            Tabs =
            {
                new RibbonTabItem { Header = "Tab 1" },
                new RibbonTabItem { Header = "Tab 2" },
                new RibbonTabItem { Header = "Tab 3" }
            }
        };

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            var onScreen = GetOnScreenOrder(ribbon);

            Assert.That(onScreen.Select(GetHeader), Is.EqualTo(new[] { "Tab 1", "Tab 2", "Tab 3" }), "Precondition: tabs are drawn in collection order");

            AssertKeyboardNavigationFollows(ribbon, onScreen);
        }
    }

    private static void AssertKeyboardNavigationFollows(Ribbon ribbon, IReadOnlyList<RibbonTabItem> onScreen)
    {
        var tabControl = ribbon.TabControl;
        Assert.That(tabControl, Is.Not.Null, "Precondition: the ribbon template must create its RibbonTabControl");

        var expectedForward = new List<string>();
        var actualForward = new List<string>();
        var expectedBackward = new List<string>();
        var actualBackward = new List<string>();

        for (var i = 0; i < onScreen.Count; i++)
        {
            var start = onScreen[i];

            Select(tabControl, start);
            PressKey(tabControl, Key.Tab, ModifierKeys.Control);
            expectedForward.Add($"{GetHeader(start)} -> {GetHeader(onScreen[(i + 1) % onScreen.Count])}");
            actualForward.Add($"{GetHeader(start)} -> {GetHeader(tabControl.SelectedTabItem)}");

            Select(tabControl, start);
            PressKey(tabControl, Key.Tab, ModifierKeys.Control | ModifierKeys.Shift);
            expectedBackward.Add($"{GetHeader(start)} -> {GetHeader(onScreen[(i + onScreen.Count - 1) % onScreen.Count])}");
            actualBackward.Add($"{GetHeader(start)} -> {GetHeader(tabControl.SelectedTabItem)}");
        }

        Assert.That(actualForward, Is.EqualTo(expectedForward), "Ctrl+Tab should select the next tab on screen (wrapping around)");
        Assert.That(actualBackward, Is.EqualTo(expectedBackward), "Ctrl+Shift+Tab should select the previous tab on screen (wrapping around)");

        Select(tabControl, onScreen[onScreen.Count / 2]);
        PressKey(tabControl, Key.End, ModifierKeys.None);
        Assert.That(GetHeader(tabControl.SelectedTabItem), Is.EqualTo(GetHeader(onScreen[onScreen.Count - 1])), "End should select the rightmost tab");

        Select(tabControl, onScreen[onScreen.Count / 2]);
        PressKey(tabControl, Key.Home, ModifierKeys.None);
        Assert.That(GetHeader(tabControl.SelectedTabItem), Is.EqualTo(GetHeader(onScreen[0])), "Home should select the leftmost tab");
    }

    // The tabs as the user sees them: visible ones, from left to right.
    private static List<RibbonTabItem> GetOnScreenOrder(Ribbon ribbon)
    {
        var tabControl = ribbon.TabControl;
        Assert.That(tabControl, Is.Not.Null, "Precondition: the ribbon template must create its RibbonTabControl");

        var tabs = ribbon.Tabs
            .Where(x => x.IsVisible && x.ActualWidth > 0)
            .OrderBy(x => x.TranslatePoint(default, tabControl).X)
            .ToList();

        Assert.That(tabs, Has.Count.EqualTo(ribbon.Tabs.Count), "Precondition: all tabs should be shown");

        return tabs;
    }

    private static void Select(RibbonTabControl tabControl, RibbonTabItem tab)
    {
        tab.IsSelected = true;
        UIHelper.DoEvents();

        Assert.That(tabControl.SelectedTabItem, Is.SameAs(tab), "Precondition: the start tab should be selected");
    }

    private static string GetHeader(RibbonTabItem tab)
    {
        return tab?.Header as string;
    }

    // Simulates a key press the way WPF input delivers it: PreviewKeyDown (tunnel) and then KeyDown (bubble)
    // sharing one args object.
    private static void PressKey(UIElement target, Key key, ModifierKeys modifiers)
    {
        // The real keyboard can't be made to hold Ctrl or Shift in a headless test, so a fake device reports the modifiers.
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
