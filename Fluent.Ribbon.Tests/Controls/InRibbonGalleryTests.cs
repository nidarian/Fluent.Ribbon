namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class InRibbonGalleryTests
{
    [Test]
    public void Opening_DropDown_Should_Not_Throw_When_GalleryPanel_Has_No_Width()
    {
        var control = new InRibbonGallery
        {
            Width = 10,
            Height = 30
        };

        using (new TestRibbonWindow(control))
        {
            Assert.That(() => control.IsDropDownOpen = true, Throws.Nothing);
        }
    }

    // When the gallery is unloaded (tab switch, content removed) while its drop down is open,
    // OnUnloaded closes the drop down. At that moment the gallery is already invisible.
    // The snapshot of the in-ribbon part (taken on opening) must still be discarded,
    // otherwise the next opening shows the old picture with its old size.
    [Test]
    public void Closing_DropDown_Through_Unload_Should_Discard_Snapshot()
    {
        var gallery = CreateGalleryWithFixedItemWidth();
        var host = new ContentControl { Content = gallery };

        using (new TestRibbonWindow(host))
        {
            UIHelper.DoEvents();

            gallery.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(gallery.IsSnapped, Is.True);

            host.Content = null;
            UIHelper.DoEvents();

            Assert.That(gallery.IsDropDownOpen, Is.False);
            Assert.That(gallery.IsSnapped, Is.False);
        }
    }

    [Test]
    public void Opening_DropDown_After_Unload_Should_Not_Show_Old_Snapshot_With_Old_Size()
    {
        var gallery = CreateGalleryWithFixedItemWidth();
        var host = new ContentControl { Content = gallery };

        using (new TestRibbonWindow(host))
        {
            UIHelper.DoEvents();

            var widthWithFiveItems = gallery.ActualWidth;

            gallery.IsDropDownOpen = true;
            UIHelper.DoEvents();

            // Unloading closes the drop down (e.g. the app switches to another tab)
            host.Content = null;
            UIHelper.DoEvents();

            Assert.That(gallery.IsDropDownOpen, Is.False);

            // The gallery gets smaller while it is not shown
            gallery.MaxItemsInRow = 2;

            host.Content = gallery;
            UIHelper.DoEvents();

            var widthWithTwoItems = gallery.ActualWidth;

            Assert.That(widthWithTwoItems, Is.LessThan(widthWithFiveItems));

            gallery.IsDropDownOpen = true;
            UIHelper.DoEvents();

            // While the drop down is open the in-ribbon part shows a snapshot of the current state, so the size does not change
            Assert.That(gallery.ActualWidth, Is.EqualTo(widthWithTwoItems));

            gallery.IsDropDownOpen = false;
            UIHelper.DoEvents();
        }
    }

    private static InRibbonGallery CreateGalleryWithFixedItemWidth()
    {
        var gallery = new InRibbonGallery
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Height = RibbonTabControl.DefaultContentHeight - 10,
            MinItemsInRow = 1,
            MaxItemsInRow = 5,
            ItemWidth = 50,
            ItemHeight = 18
        };

        for (var i = 0; i < 8; i++)
        {
            gallery.Items.Add("Item " + i);
        }

        return gallery;
    }
}
