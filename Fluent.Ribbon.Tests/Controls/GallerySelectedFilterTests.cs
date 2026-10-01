namespace Fluent.Tests.Controls;

using System.Linq;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class GallerySelectedFilterTests
{
    [Test]
    public void Setting_SelectedFilter_From_Code_Updates_Checked_Menu_Item()
    {
        var f1 = new GalleryGroupFilter { Title = "F1" };
        var f2 = new GalleryGroupFilter { Title = "F2" };

        var gallery = new Gallery();
        gallery.Filters.Add(f1);
        gallery.Filters.Add(f2);

        using (new TestRibbonWindow(gallery))
        {
            UIHelper.DoEvents();

            var filterButton = (DropDownButton)gallery.Template.FindName("PART_DropDownButton", gallery);

            Assert.That(filterButton, Is.Not.Null, "Precondition: filter drop down button must exist in the template.");

            var menuItems = filterButton.Items.Cast<MenuItem>().ToList();
            var f1MenuItem = menuItems.Single(x => ReferenceEquals(x.Tag, f1));
            var f2MenuItem = menuItems.Single(x => ReferenceEquals(x.Tag, f2));

            Assert.That(gallery.SelectedFilter, Is.SameAs(f1), "Precondition: the first filter is selected by default.");
            Assert.That(f1MenuItem.IsChecked, Is.True, "Precondition: the menu item of the first filter is checked.");

            // Not using the menu (click path), but setting the property directly, e.g. from a binding.
            gallery.SelectedFilter = f2;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(f1MenuItem.IsChecked, Is.False);
                Assert.That(f2MenuItem.IsChecked, Is.True);
            }
        }
    }
}
