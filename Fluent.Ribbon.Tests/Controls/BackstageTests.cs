namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Data;
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
}
