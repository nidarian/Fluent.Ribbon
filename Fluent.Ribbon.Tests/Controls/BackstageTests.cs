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
}
