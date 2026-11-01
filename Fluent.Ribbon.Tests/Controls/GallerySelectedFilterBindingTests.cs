namespace Fluent.Tests.Controls;

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// <see cref="Gallery.SelectedFilter"/> and <see cref="InRibbonGallery.SelectedFilter"/> don't bind TwoWay by default,
/// so an app's plain binding on them is OneWay. Picking a filter in the gallery's filter menu set SelectedFilter with
/// the CLR setter, which replaced that binding: the app could no longer choose the filter. Found by a bug hunt after round 7.
/// </summary>
[TestFixture]
public class GallerySelectedFilterBindingTests
{
    [Test]
    public void Gallery_filter_menu_keeps_a_OneWay_binding_on_SelectedFilter()
    {
        var model = new FilterModel();
        var gallery = new Gallery();
        AssertFilterMenuKeepsBinding(gallery, Gallery.SelectedFilterProperty, "PART_DropDownButton", model);
    }

    [Test]
    public void InRibbonGallery_filter_menu_keeps_a_OneWay_binding_on_SelectedFilter()
    {
        var model = new FilterModel();
        var gallery = new InRibbonGallery();
        AssertFilterMenuKeepsBinding(gallery, InRibbonGallery.SelectedFilterProperty, "PART_FilterDropDownButton", model);
    }

    private static void AssertFilterMenuKeepsBinding(System.Windows.Controls.Control gallery, DependencyProperty selectedFilterProperty, string menuButtonName, FilterModel model)
    {
        var first = new GalleryGroupFilter { Title = "First", Groups = "A" };
        var second = new GalleryGroupFilter { Title = "Second", Groups = "B" };

        var filters = (System.Collections.ObjectModel.ObservableCollection<GalleryGroupFilter>)gallery.GetType().GetProperty("Filters")!.GetValue(gallery)!;
        filters.Add(first);
        filters.Add(second);

        model.Filter = first;

        // The usual app binding, with the default mode (OneWay for this property).
        BindingOperations.SetBinding(gallery, selectedFilterProperty, new Binding(nameof(FilterModel.Filter)) { Source = model });

        using (new TestRibbonWindow(gallery))
        {
            UIHelper.DoEvents();
            gallery.ApplyTemplate();
            UIHelper.DoEvents();

            Assert.That(gallery.GetValue(selectedFilterProperty), Is.SameAs(first), "Precondition: the gallery shows the app's filter");

            var menuButton = gallery.Template.FindName(menuButtonName, gallery) as DropDownButton;
            Assert.That(menuButton, Is.Not.Null, $"Precondition: the template has {menuButtonName}");

            var secondItem = menuButton!.Items.OfType<System.Windows.Controls.MenuItem>().FirstOrDefault(x => ReferenceEquals(x.Tag, second));
            Assert.That(secondItem, Is.Not.Null, "Precondition: the filter menu has an item for the second filter");

            // What a click on the filter menu item raises.
            secondItem!.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent));
            UIHelper.DoEvents();

            Assert.That(gallery.GetValue(selectedFilterProperty), Is.SameAs(second), "Precondition: the menu must select the filter");
            Assert.That(BindingOperations.GetBindingExpression(gallery, selectedFilterProperty), Is.Not.Null, "The app's OneWay binding on SelectedFilter was lost");

            model.Filter = null;
            model.Filter = first;
            Assert.That(gallery.GetValue(selectedFilterProperty), Is.SameAs(first), "The app must still choose the filter");
        }
    }

    private sealed class FilterModel : INotifyPropertyChanged
    {
        private GalleryGroupFilter filter;

        public event PropertyChangedEventHandler PropertyChanged;

        public GalleryGroupFilter Filter
        {
            get => this.filter;
            set
            {
                if (ReferenceEquals(this.filter, value))
                {
                    return;
                }

                this.filter = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Filter)));
            }
        }
    }
}
