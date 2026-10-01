namespace Fluent.Tests.Controls;

using System.Windows;
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
}