namespace Fluent.Tests.Controls;

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

            Assert.That(Keyboard.FocusedElement, Is.SameAs(backstage), "Focus must return to the backstage button");
        }
    }
}
