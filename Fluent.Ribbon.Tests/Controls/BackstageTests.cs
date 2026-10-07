namespace Fluent.Tests.Controls;

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class BackstageTests
{
    /// <summary>
    /// This test ensures that the <see cref="BackstageAdorner"/> is destroyed as soon as the <see cref="Backstage"/> is unloaded.
    /// </summary>
    [Test]
    public void Adorner_should_be_destroyed_on_unload()
    {
        var backstage = new Backstage
        {
            Content = new Button()
        };

        using (var window = new TestRibbonWindow(backstage))
        {
            Assert.That(backstage.IsLoaded, Is.True);

            Assert.That(backstage.GetFieldValue<object>("adorner"), Is.Null);

            backstage.IsOpen = true;

            Assert.That(backstage.GetFieldValue<object>("adorner"), Is.Not.Null);

            backstage.IsOpen = false;

            Assert.That(backstage.GetFieldValue<object>("adorner"), Is.Not.Null);

            window.Content = null;

            UIHelper.DoEvents();

            Assert.That(backstage.GetFieldValue<object>("adorner"), Is.Null);
        }
    }

    // https://github.com/fluentribbon/Fluent.Ribbon/issues/1247
    // The tests close the backstage through OnKeyTipBack, which is one of the real user paths
    // (pressing Escape while KeyTips are shown). All user paths go through the same internal method.

    [Test]
    public void Closing_can_be_cancelled_to_keep_the_backstage_open()
    {
        var backstage = new Backstage { Content = new Button() };

        using (new TestRibbonWindow(backstage))
        {
            backstage.IsOpen = true;

            var raised = 0;
            backstage.Closing += (_, e) =>
            {
                raised++;
                e.Cancel = true;
            };

            backstage.OnKeyTipBack();

            Assert.That(raised, Is.EqualTo(1));
            Assert.That(backstage.IsOpen, Is.True, "Cancelled close must keep the backstage open");
        }
    }

    [Test]
    public void Closing_without_cancel_closes_the_backstage()
    {
        var backstage = new Backstage { Content = new Button() };

        using (new TestRibbonWindow(backstage))
        {
            backstage.IsOpen = true;

            var raised = 0;
            backstage.Closing += (_, _) => raised++;

            backstage.OnKeyTipBack();

            Assert.That(raised, Is.EqualTo(1));
            Assert.That(backstage.IsOpen, Is.False);
        }
    }

    [Test]
    public void Closing_is_not_raised_when_IsOpen_is_set_from_code_or_backstage_is_already_closed()
    {
        var backstage = new Backstage { Content = new Button() };

        using (new TestRibbonWindow(backstage))
        {
            var raised = 0;
            backstage.Closing += (_, e) =>
            {
                raised++;
                e.Cancel = true;
            };

            // Already closed: nothing to cancel.
            backstage.OnKeyTipBack();
            Assert.That(raised, Is.EqualTo(0));

            // Set from code: the app decided, so it's not asked.
            backstage.IsOpen = true;
            backstage.IsOpen = false;

            Assert.That(raised, Is.EqualTo(0));
            Assert.That(backstage.IsOpen, Is.False);
        }
    }

    /// <summary>
    /// The backstage binds its content's Visibility to its own. When the content is replaced,
    /// the old content must be released: before the fix, the binding was cleared on the NEW content
    /// (a copy/paste slip in OnContentChanged), so the old element kept following the backstage's
    /// visibility even after it was removed. An app that reuses that element elsewhere would see it
    /// appear and disappear with the backstage.
    /// </summary>
    [Test]
    public void Replacing_Content_releases_the_old_content()
    {
        var oldContent = new Button();
        var newContent = new Button();
        var backstage = new Backstage { Content = oldContent };

        Assert.That(BindingOperations.IsDataBound(oldContent, UIElement.VisibilityProperty), Is.True, "Precondition: content follows the backstage's visibility");

        backstage.Content = newContent;

        Assert.That(BindingOperations.IsDataBound(oldContent, UIElement.VisibilityProperty), Is.False, "Old content must no longer be bound to the backstage");
        Assert.That(BindingOperations.IsDataBound(newContent, UIElement.VisibilityProperty), Is.True, "New content follows the backstage's visibility");
        Assert.That(LogicalTreeHelper.GetParent(oldContent), Is.Null, "Old content is no longer a logical child");

        // The visible consequence: hiding the backstage must not hide content it no longer owns.
        backstage.Visibility = Visibility.Collapsed;

        Assert.That(oldContent.Visibility, Is.EqualTo(Visibility.Visible));
        Assert.That(newContent.Visibility, Is.EqualTo(Visibility.Collapsed));
    }

    /// <summary>
    /// Opening the backstage puts the ribbon into "backstage open" mode (IsBackstageOrStartScreenOpen,
    /// which hides the Quick Access Toolbar, and more). Closing it restores that, but only through its adorner.
    /// When the backstage was unloaded while open (the app moves the ribbon to another panel, swaps the
    /// window content, ...), the adorner was destroyed and the ribbon was never restored: not on unload,
    /// and not on a later close either, because closing without an adorner returned early.
    /// </summary>
    [Test]
    public void Unloading_an_open_backstage_restores_the_ribbon()
    {
        var backstage = new Backstage { Content = new Button(), AreAnimationsEnabled = false };
        var ribbon = new Ribbon { Menu = backstage };

        using (var window = new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            backstage.IsOpen = true;
            UIHelper.DoEvents();
            Assert.That(ribbon.IsBackstageOrStartScreenOpen, Is.True, "Precondition: the ribbon is in backstage mode");

            // The ribbon (and with it the backstage) leaves the window while the backstage is open.
            window.Content = null;
            UIHelper.DoEvents();

            Assert.That(ribbon.IsBackstageOrStartScreenOpen, Is.False, "Unloading must restore the ribbon");

            // Back in the window with IsOpen still true: the backstage is shown again, like any
            // backstage opened before it was loaded.
            window.Content = ribbon;
            UIHelper.DoEvents();
            UIHelper.DoEvents();
            Assert.That(ribbon.IsBackstageOrStartScreenOpen, Is.True, "Shown again after re-loading, because IsOpen is still true");

            // And a normal close works again.
            backstage.IsOpen = false;
            UIHelper.DoEvents();
            Assert.That(ribbon.IsBackstageOrStartScreenOpen, Is.False, "Closing restores the ribbon");
        }
    }

    /// <summary>
    /// Closing the backstage gives keyboard focus back to the backstage button, so a keyboard user
    /// continues where they were. Only the animated close did that. With animations off (for example
    /// for users who turn off animations), focus stayed on the hidden backstage content: nothing
    /// visible had focus anymore.
    /// </summary>
    [Test]
    public void Closing_without_animation_returns_focus_to_the_backstage_button()
    {
        var content = new System.Windows.Controls.Button { Content = "Inside" };
        var backstage = new Backstage { Content = content, AreAnimationsEnabled = false };
        var ribbon = new Ribbon { Menu = backstage };

        using (var window = new TestRibbonWindow(ribbon))
        {
            window.Activate();
            UIHelper.DoEvents();

            backstage.IsOpen = true;
            UIHelper.DoEvents();

            // Baseline: proves keyboard focus works here and that it's inside the open backstage.
            content.Focus();
            UIHelper.DoEvents();
            if (content.IsKeyboardFocused == false)
            {
                Assert.Inconclusive("Keyboard focus is not available in this test environment.");
            }

            backstage.IsOpen = false;
            UIHelper.DoEvents();

            UIHelper.InconclusiveIfKeyboardFocusLost("after closing the backstage");
            Assert.That(Keyboard.FocusedElement, Is.SameAs(backstage), "Focus must return to the backstage button");
        }
    }

    /// <summary>
    /// Backstages (and start screens) in one window share the same <see cref="System.Windows.Documents.AdornerLayer"/>.
    /// Destroying the adorner of one of them must only remove its own command binding, not the ones of the others.
    /// </summary>
    [Test]
    public void Destroying_adorner_keeps_command_bindings_of_other_backstages()
    {
        var firstBackstage = new Backstage
        {
            Content = new Button()
        };

        var secondBackstage = new Backstage
        {
            Content = new Button()
        };

        var panel = new System.Windows.Controls.StackPanel
        {
            Children =
            {
                firstBackstage,
                secondBackstage
            }
        };

        using (new TestRibbonWindow(panel))
        {
            firstBackstage.IsOpen = true;
            firstBackstage.IsOpen = false;

            secondBackstage.IsOpen = true;
            secondBackstage.IsOpen = false;

            Assert.That(firstBackstage.AdornerLayer, Is.Not.Null.And.SameAs(secondBackstage.AdornerLayer), "Precondition: both backstages must share the adorner layer.");

            var secondAdorner = secondBackstage.GetFieldValue<UIElement>("adorner");
            Assert.That(secondAdorner, Is.Not.Null, "Precondition: the second backstage must have an adorner.");
            Assert.That(RibbonCommands.OpenBackstage.CanExecute(null, secondAdorner), Is.True, "Precondition: the back command of the second backstage must be executable.");

            // Unloading the first backstage destroys its adorner.
            panel.Children.Remove(firstBackstage);

            UIHelper.DoEvents();

            Assert.That(firstBackstage.GetFieldValue<object>("adorner"), Is.Null, "Precondition: the adorner of the removed backstage must be destroyed.");
            Assert.That(secondBackstage.GetFieldValue<object>("adorner"), Is.SameAs(secondAdorner), "Precondition: the second backstage must keep its adorner.");

            Assert.That(RibbonCommands.OpenBackstage.CanExecute(null, secondAdorner), Is.True, "The back command of the remaining backstage must still be executable.");
        }
    }

    // Escape while KeyTips are shown inside the open backstage goes "back" one KeyTip level:
    // the backstage closes and the KeyTips of the ribbon (its tabs) are shown.
    // Since #1247 the app can cancel that close (e.g. unsaved changes). The backstage then stays open,
    // so the KeyTips of the tabs must not be shown on top of it: pressing one would select a tab hidden behind the open backstage.

    [Test]
    public void Escape_in_KeyTips_of_open_backstage_closes_it_and_shows_the_KeyTips_of_the_tabs()
    {
        var (ribbon, backstage, ribbonTab) = CreateRibbonWithBackstage();

        using (var window = new TestRibbonWindow(ribbon))
        {
            var keyTipService = ShowKeyTipsInOpenBackstage(window, ribbon, backstage, out var wasDeactivated);

            PressKey(keyTipService, window, Key.Escape);
            InconclusiveIfWindowDeactivated(window, wasDeactivated(), "after Escape");

            Assert.That(backstage.IsOpen, Is.False, "Escape should close the backstage");
            Assert.That(IsKeyTipShown(keyTipService, ribbonTab), Is.True, "Escape should go back to the KeyTips of the tabs");
        }
    }

    [Test]
    public void Escape_in_KeyTips_of_open_backstage_does_not_show_the_KeyTips_of_the_tabs_when_Closing_is_cancelled()
    {
        var (ribbon, backstage, ribbonTab) = CreateRibbonWithBackstage();

        var raised = 0;
        backstage.Closing += (_, e) =>
        {
            raised++;
            e.Cancel = true;
        };

        using (var window = new TestRibbonWindow(ribbon))
        {
            var keyTipService = ShowKeyTipsInOpenBackstage(window, ribbon, backstage, out var wasDeactivated);

            PressKey(keyTipService, window, Key.Escape);
            InconclusiveIfWindowDeactivated(window, wasDeactivated(), "after Escape");

            Assert.That(raised, Is.EqualTo(1), "Precondition: Escape should try to close the backstage");
            Assert.That(backstage.IsOpen, Is.True, "Precondition: the app cancelled closing, so the backstage stays open");
            Assert.That(IsKeyTipShown(keyTipService, ribbonTab), Is.False, "The KeyTips of the tabs must not be shown on top of the backstage that stayed open");
        }
    }

    private static (Ribbon Ribbon, Backstage Backstage, RibbonTabItem RibbonTab) CreateRibbonWithBackstage()
    {
        var backstage = new Backstage
        {
            AreAnimationsEnabled = false,
            Content = new BackstageTabControl
            {
                Items =
                {
                    new BackstageTabItem { Header = "Info", KeyTip = "I" }
                }
            }
        };

        var ribbonTab = new RibbonTabItem { Header = "Home", KeyTip = "H" };

        var ribbon = new Ribbon
        {
            Menu = backstage,

            // Don't load a persisted (maybe minimized) state.
            AutomaticStateManagement = false
        };
        ribbon.Tabs.Add(ribbonTab);

        return (ribbon, backstage, ribbonTab);
    }

    // Opens the backstage and shows the KeyTips (like pressing Alt), which go directly to the open backstage.
    private static KeyTipService ShowKeyTipsInOpenBackstage(Window window, Ribbon ribbon, Backstage backstage, out Func<bool> wasDeactivated)
    {
        // KeyTipService ignores all keys while the window is not active.
        window.Activate();
        UIHelper.DoEvents();

        if (window.IsActive == false)
        {
            Assert.Inconclusive("The test window could not be activated, so KeyTipService would ignore all keys.");
        }

        // The tests of the three target frameworks run at the same time, so another test window can take activation for a moment.
        // KeyTipService then ends the KeyTips, and the window can be active again by the time IsActive is checked.
        var deactivated = false;
        window.Deactivated += (_, _) => deactivated = true;
        wasDeactivated = () => deactivated;

        backstage.IsOpen = true;
        UIHelper.DoEvents();

        Assert.That(backstage.IsOpenAndDisplayed, Is.True, "Precondition: the backstage should be open and displayed");

        var keyTipService = ribbon.GetFieldValue<KeyTipService>("keyTipService");

        typeof(KeyTipService).GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(keyTipService, null);
        InconclusiveIfWindowDeactivated(window, deactivated, "after showing the KeyTips");

        var activeAdornerChain = keyTipService.GetFieldValue<KeyTipAdorner>("activeAdornerChain");
        Assert.That(activeAdornerChain, Is.Not.Null, "Precondition: the KeyTips should be shown");
        Assert.That(activeAdornerChain.ActiveKeyTipAdorner.KeyTipInformations.Any(x => x.AssociatedElement is BackstageTabItem && x.IsVisible), Is.True, "Precondition: the KeyTips of the backstage should be shown");
        Assert.That(IsKeyTipShown(keyTipService, ribbon.Tabs[0]), Is.False, "Precondition: the KeyTips of the tabs should not be shown while the backstage is open");

        return keyTipService;
    }

    // Whether the given element's KeyTip is shown in the KeyTip level the user is currently in.
    private static bool IsKeyTipShown(KeyTipService keyTipService, FrameworkElement element)
    {
        var activeAdornerChain = keyTipService.GetFieldValue<KeyTipAdorner>("activeAdornerChain");

        return activeAdornerChain is not null
               && activeAdornerChain.IsAdornerChainAlive
               && activeAdornerChain.ActiveKeyTipAdorner.KeyTipInformations.Any(x => ReferenceEquals(x.AssociatedElement, element) && x.IsVisible);
    }

    private static void PressKey(KeyTipService keyTipService, Window window, Key key)
    {
        typeof(KeyTipService).GetMethod("OnWindowPreviewKeyDown", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(keyTipService, new object[]
            {
                null,
                new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, key) { RoutedEvent = Keyboard.PreviewKeyDownEvent }
            });

        UIHelper.DoEvents();
    }

    // Another test window taking activation ends the KeyTips. That is an environment problem, not the bug.
    private static void InconclusiveIfWindowDeactivated(Window window, bool wasDeactivated, string step)
    {
        if (wasDeactivated
            || window.IsActive == false)
        {
            Assert.Inconclusive($"The test window was deactivated ({step}).");
        }
    }
}
