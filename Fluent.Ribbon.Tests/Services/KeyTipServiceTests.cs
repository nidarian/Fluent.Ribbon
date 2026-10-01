namespace Fluent.Tests.Services;

using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
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
}