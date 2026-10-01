namespace Fluent.Tests.Services;

using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class KeyTipServiceTests
{
    [Test]
    public void TestDefaultKeyTipKeys()
    {
        var ribbon = new Ribbon();
        var keytipService = new KeyTipService(ribbon);

        Assert.That(ribbon.KeyTipKeys, Is.Empty);
        Assert.That(keytipService.KeyTipKeys, Is.EquivalentTo(KeyTipService.DefaultKeyTipKeys));

        var defaultKeys = KeyTipService.DefaultKeyTipKeys.ToList();

        keytipService.KeyTipKeys.RemoveAt(0);

        Assert.That(KeyTipService.DefaultKeyTipKeys, Is.EquivalentTo(defaultKeys));
    }

    [Test(Description = "Test for #908 KeyTipService should dismiss keytips if the first key does not match any keytips")]
    public void TestImmediateDismissIfNoMatchesInRootLayer()
    {
        var ribbon = new Ribbon { Menu = new Backstage() };

        using var testWindow = new TestRibbonWindow(ribbon);
        testWindow.Activate();
        var keytipService = new KeyTipService(ribbon);

        Assert.That(keytipService.AreAnyKeyTipsVisible, Is.False);

        keytipService.Attach();

        keytipService.GetType().GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(keytipService, null);

        Assert.That(keytipService.AreAnyKeyTipsVisible, Is.True);

        keytipService.GetType().GetMethod("OnWindowPreviewKeyDown", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(keytipService, new object[]
            {
                null,
                new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(testWindow), 0, Key.A)
            });

        Assert.That(keytipService.AreAnyKeyTipsVisible, Is.False);
    }

    /// <summary>
    /// Detach runs when <see cref="Ribbon.IsKeyTipHandlingEnabled"/> is set to false (and when the ribbon unloads).
    /// It unhooked the keyboard and window handlers but left KeyTips that were showing on screen.
    /// With the handlers gone, nothing (Escape, Alt, clicking elsewhere) could remove them anymore.
    /// Same setup as the #908 test above.
    /// </summary>
    [Test]
    public void Detach_closes_KeyTips_that_are_showing()
    {
        var ribbon = new Ribbon { Menu = new Backstage() };

        using var testWindow = new TestRibbonWindow(ribbon);
        testWindow.Activate();
        var keytipService = new KeyTipService(ribbon);

        keytipService.Attach();

        keytipService.GetType().GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(keytipService, null);

        Assert.That(keytipService.AreAnyKeyTipsVisible, Is.True, "Precondition: KeyTips are showing");

        keytipService.Detach();

        Assert.That(keytipService.AreAnyKeyTipsVisible, Is.False, "Detaching must close the KeyTips that are showing");
    }

    [Test(Description = "Enabling key tip handling before the ribbon is inside a window must not prevent the service from attaching to the window later")]
    public void TestAttachAfterIsKeyTipHandlingEnabledToggledBeforeWindow()
    {
        var ribbon = new Ribbon();

        // Toggling false -> true calls KeyTipService.Detach and then KeyTipService.Attach while Window.GetWindow(ribbon) is still null.
        ribbon.IsKeyTipHandlingEnabled = false;
        ribbon.IsKeyTipHandlingEnabled = true;

        using var testWindow = new TestRibbonWindow(ribbon);

        Assert.That(ribbon.IsLoaded, Is.True, "Precondition: the ribbon must be loaded, so Ribbon.OnLoaded has called KeyTipService.Attach again.");

        var keyTipService = ribbon.GetFieldValue<KeyTipService>("keyTipService");

        // Without a window the service never sees Alt/F10, i.e. key tips would be permanently off.
        Assert.That(keyTipService.GetFieldValue<Window>("window"), Is.SameAs(testWindow), "KeyTipService should be attached to the window hosting the ribbon.");
    }

    [Test(Description = "Showing key tips must terminate a previous adorner chain instead of just dropping it")]
    public void TestShowTerminatesPreviousAdornerChain()
    {
        var ribbon = new Ribbon { Menu = new Backstage() };

        using var testWindow = new TestRibbonWindow(ribbon);
        testWindow.Activate();

        if (testWindow.IsActive == false)
        {
            Assert.Inconclusive("The test window could not be activated, so KeyTipService.Show would return early.");
        }

        var keytipService = new KeyTipService(ribbon);
        keytipService.Attach();

        // A chain which is still alive, but shows no key tips: it waits for its (never loaded) element to be loaded.
        var notLoadedButton = new Button { KeyTip = "A" };
        var previousAdornerChain = new KeyTipAdorner(notLoadedButton, notLoadedButton, null);
        previousAdornerChain.Attach();

        Assert.That(previousAdornerChain.IsAdornerChainAlive, Is.True, "Precondition: the previous chain must be waiting for Loaded.");

        var previousAdornerChainTerminated = false;
        previousAdornerChain.Terminated += (_, _) => previousAdornerChainTerminated = true;

        typeof(KeyTipService).GetField("activeAdornerChain", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(keytipService, previousAdornerChain);

        keytipService.GetType().GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(keytipService, null);

        Assert.That(previousAdornerChainTerminated, Is.True, "Show should terminate the previous adorner chain, otherwise it could still attach later.");
    }

    [Test(Description = "A StartScreen which is open but not displayed must not receive the key tips")]
    public void TestKeyTipsAreShownOnRibbonIfStartScreenIsOpenButNotDisplayed()
    {
        var startScreen = new StartScreen
        {
            // Shown = true makes StartScreen.Show return early, so IsOpen becomes true although nothing gets displayed.
            Shown = true,
            IsOpen = true,
            Content = new StartScreenTabControl()
        };

        var ribbon = new Ribbon
        {
            Menu = new Backstage(),
            StartScreen = startScreen
        };
        ribbon.Tabs.Add(new RibbonTabItem { Header = "Home", KeyTip = "H" });

        using var testWindow = new TestRibbonWindow(ribbon);
        testWindow.Activate();

        if (testWindow.IsActive == false)
        {
            Assert.Inconclusive("The test window could not be activated, so KeyTipService.Show would not show any key tips.");
        }

        Assert.That(startScreen.IsOpen, Is.True, "Precondition: the start screen must be open.");
        Assert.That(startScreen.Shown, Is.True, "Precondition: the start screen must count as already shown.");
        Assert.That(typeof(Backstage).GetField("adorner", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(startScreen), Is.Null, "Precondition: the start screen must not be displayed (it has no BackstageAdorner).");

        var keytipService = new KeyTipService(ribbon);
        keytipService.Attach();

        keytipService.GetType().GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(keytipService, null);

        // Before the fix the key tips were forwarded to the invisible start screen, which has no key tips, so the chain terminated immediately.
        Assert.That(keytipService.AreAnyKeyTipsVisible, Is.True, "The ribbon key tips should be shown because the start screen is not displayed.");

        var activeAdornerChain = keytipService.GetFieldValue<KeyTipAdorner>("activeAdornerChain");
        Assert.That(activeAdornerChain.ActiveKeyTipAdorner.AdornedElement, Is.SameAs(ribbon), "The active key tip level should be the ribbon itself.");
    }

    [Test(Description = "A key which does not match any key tip in a nested layer must keep the key tips open (only the root layer dismisses, see #908)")]
    public void TestNoDismissIfNoMatchesInNestedLayer()
    {
        var groupBox = new RibbonGroupBox { Header = "Group" };
        groupBox.Items.Add(new Button { Header = "Button", KeyTip = "B" });

        var tabItem = new RibbonTabItem { Header = "Home", KeyTip = "H" };
        tabItem.Groups.Add(groupBox);

        var ribbon = new Ribbon
        {
            Menu = new Backstage(),

            // Don't load a persisted (maybe minimized) state, a minimized ribbon would open the tab in a popup instead.
            AutomaticStateManagement = false
        };
        ribbon.Tabs.Add(tabItem);

        using var testWindow = new TestRibbonWindow(ribbon);
        testWindow.Activate();

        if (testWindow.IsActive == false)
        {
            Assert.Inconclusive("The test window could not be activated, so KeyTipService would ignore all keys.");
        }

        var keytipService = new KeyTipService(ribbon);
        keytipService.Attach();

        keytipService.GetType().GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(keytipService, null);

        Assert.That(keytipService.AreAnyKeyTipsVisible, Is.True, "Precondition: the root key tips must be shown.");

        // H opens the next layer (the tab with its button key tip "B").
        PressKey(keytipService, testWindow, Key.H);

        var activeAdornerChain = keytipService.GetFieldValue<KeyTipAdorner>("activeAdornerChain");
        Assert.That(activeAdornerChain, Is.Not.Null, "Precondition: H must not terminate the key tips.");
        Assert.That(activeAdornerChain.ActiveKeyTipAdorner, Is.Not.SameAs(activeAdornerChain), "Precondition: H must navigate to a nested layer.");
        Assert.That(keytipService.AreAnyKeyTipsVisible, Is.True, "Precondition: the nested layer must show key tips.");

        // Z matches nothing in the nested layer: this should beep and keep the key tips, not dismiss them.
        PressKey(keytipService, testWindow, Key.Z);

        Assert.That(keytipService.AreAnyKeyTipsVisible, Is.True, "A non matching key in a nested layer should keep the key tips open.");
    }

    private static void PressKey(KeyTipService keytipService, Window window, Key key)
    {
        keytipService.GetType().GetMethod("OnWindowPreviewKeyDown", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(keytipService, new object[]
            {
                null,
                new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, key) { RoutedEvent = Keyboard.PreviewKeyDownEvent }
            });
    }
}
