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
}