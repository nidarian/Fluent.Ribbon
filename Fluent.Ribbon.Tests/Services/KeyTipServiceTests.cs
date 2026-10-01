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