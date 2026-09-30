namespace Fluent.Tests.Controls;

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
}
