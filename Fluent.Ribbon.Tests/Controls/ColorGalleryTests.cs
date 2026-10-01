namespace Fluent.Tests.Controls;

using System.Windows;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class ColorGalleryTests
{
    /// <summary>
    /// When a <see cref="ColorGallery"/> gets a new template, <c>OnApplyTemplate</c> must
    /// detach its click handler from the "More Colors" menu item of the OLD template.
    /// Otherwise the discarded menu item keeps the gallery alive and still triggers
    /// <see cref="ColorGallery.MoreColorsExecuting"/> when it is clicked.
    /// </summary>
    [Test]
    public void ReTemplating_Should_Detach_Handler_From_Old_MoreColors_Button()
    {
        var colorGallery = new ColorGallery();

        // Count how often the gallery reacts to a "More Colors" click.
        // Canceled = true keeps the handler from touching the static RecentColors list,
        // so this test doesn't leak state into other tests.
        var moreColorsExecutingCount = 0;
        colorGallery.MoreColorsExecuting += (_, args) =>
        {
            moreColorsExecutingCount++;
            args.Canceled = true;
        };

        using (new TestRibbonWindow(colorGallery))
        {
            colorGallery.ApplyTemplate();

            var oldMoreColorsButton = (MenuItem)colorGallery.Template.FindName("PART_MoreColors", colorGallery);
            Assert.That(oldMoreColorsButton, Is.Not.Null);

            // Force a fresh visual tree from the same template:
            // removing the template tears down the old parts, restoring it builds new ones
            // and calls OnApplyTemplate again (while the gallery still remembers the old button).
            var template = colorGallery.Template;
            colorGallery.Template = null;
            colorGallery.ApplyTemplate();
            colorGallery.Template = template;
            colorGallery.ApplyTemplate();

            var newMoreColorsButton = (MenuItem)colorGallery.Template.FindName("PART_MoreColors", colorGallery);
            Assert.That(newMoreColorsButton, Is.Not.Null);
            Assert.That(newMoreColorsButton, Is.Not.SameAs(oldMoreColorsButton), "Precondition: re-templating must create a new More Colors button.");

            // Clicking the discarded button must not reach the gallery anymore.
            oldMoreColorsButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent, oldMoreColorsButton));
            Assert.That(moreColorsExecutingCount, Is.EqualTo(0), "The old More Colors button is still attached to the gallery.");

            // The new button must still work, exactly once per click.
            newMoreColorsButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent, newMoreColorsButton));
            Assert.That(moreColorsExecutingCount, Is.EqualTo(1));
        }
    }
}
