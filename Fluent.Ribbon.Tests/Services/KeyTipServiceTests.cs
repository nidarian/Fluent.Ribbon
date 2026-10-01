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
}