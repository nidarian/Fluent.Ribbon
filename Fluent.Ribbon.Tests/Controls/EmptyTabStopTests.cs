namespace Fluent.Tests.Controls;

using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Wrappers that do nothing on their own should not be tab stops, otherwise Tab lands on an
/// invisible stop (no focus visual, no action) before reaching the real controls inside.
/// </summary>
[TestFixture]
public class EmptyTabStopTests
{
    [Test]
    public void ColorGallery_itself_is_not_focusable()
    {
        var gallery = new ColorGallery();

        using (new TestRibbonWindow(gallery))
        {
            UIHelper.DoEvents();

            Assert.That(gallery.Style, Is.Not.Null, "precondition: the theme style should be applied");

            // The color items, menu items and list boxes inside are the real stops.
            Assert.That(gallery.Focusable, Is.False, "the color gallery wrapper should not be focusable");
        }
    }

    [Test]
    public void QuickAccessToolBar_holder_below_ribbon_is_not_a_tab_stop()
    {
        // AutomaticStateManagement is off so this test doesn't save "toolbar below the ribbon" to isolated storage.
        // Otherwise later tests that create a Ribbon would load that state and see the toolbar in the wrong place.
        var ribbon = new Ribbon
        {
            AutomaticStateManagement = false,
            ShowQuickAccessToolBarAboveRibbon = false
        };

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            var holder = (ContentControl)ribbon.Template.FindName("quickAccessToolBarHolder", ribbon);
            Assert.That(holder, Is.Not.Null, "precondition: the ribbon template should contain quickAccessToolBarHolder");
            Assert.That(holder.Content, Is.InstanceOf<QuickAccessToolBar>(), "precondition: the quick access toolbar should be hosted below the ribbon");

            Assert.That(holder.Focusable, Is.False, "the holder of the quick access toolbar should not be focusable");
            Assert.That(holder.IsTabStop, Is.False, "the holder of the quick access toolbar should not be a tab stop");
        }
    }
}
