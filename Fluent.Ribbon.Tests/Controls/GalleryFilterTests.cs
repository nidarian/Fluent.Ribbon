namespace Fluent.Tests.Controls;

using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class GalleryFilterTests
{
    [Test]
    public void Gallery_Clearing_Filters_Removes_Filter_Menu_Items()
    {
        var gallery = new Gallery();
        gallery.Filters.Add(new GalleryGroupFilter { Title = "F1" });
        gallery.Filters.Add(new GalleryGroupFilter { Title = "F2" });

        using (new TestRibbonWindow(gallery))
        {
            UIHelper.DoEvents();

            var filterButton = (DropDownButton)gallery.Template.FindName("PART_DropDownButton", gallery);

            Assert.That(filterButton, Is.Not.Null, "Precondition: filter drop down button must exist in the template.");
            Assert.That(filterButton.Items.Count, Is.EqualTo(2), "Precondition: one menu item per filter.");

            // ObservableCollection.Clear raises a Reset notification.
            gallery.Filters.Clear();
            gallery.Filters.Add(new GalleryGroupFilter { Title = "G" });

            Assert.That(filterButton.Items.Count, Is.EqualTo(1));
        }
    }

    [Test]
    public void Gallery_Clearing_Filters_Resets_SelectedFilter()
    {
        var f1 = new GalleryGroupFilter { Title = "F1" };

        var gallery = new Gallery();
        gallery.Filters.Add(f1);
        gallery.SelectedFilter = f1;

        Assert.That(gallery.SelectedFilter, Is.SameAs(f1), "Precondition: F1 is selected.");

        gallery.Filters.Clear();

        Assert.That(gallery.SelectedFilter, Is.Null);
    }

    [Test]
    public void Gallery_Removing_Selected_Filter_Changes_SelectedFilter()
    {
        var f1 = new GalleryGroupFilter { Title = "F1" };
        var f2 = new GalleryGroupFilter { Title = "F2" };

        var gallery = new Gallery();
        gallery.Filters.Add(f1);
        gallery.Filters.Add(f2);
        gallery.SelectedFilter = f2;

        Assert.That(gallery.SelectedFilter, Is.SameAs(f2), "Precondition: F2 is selected.");

        gallery.Filters.Remove(f2);

        Assert.That(gallery.SelectedFilter, Is.SameAs(f1));
    }

    [Test]
    public void InRibbonGallery_Removing_Selected_Filter_Changes_SelectedFilter()
    {
        var f1 = new GalleryGroupFilter { Title = "F1" };
        var f2 = new GalleryGroupFilter { Title = "F2" };

        var gallery = new InRibbonGallery();
        gallery.Filters.Add(f1);
        gallery.Filters.Add(f2);
        gallery.SelectedFilter = f2;

        Assert.That(gallery.SelectedFilter, Is.SameAs(f2), "Precondition: F2 is selected.");

        gallery.Filters.Remove(f2);

        Assert.That(gallery.SelectedFilter, Is.SameAs(f1));
    }
}
