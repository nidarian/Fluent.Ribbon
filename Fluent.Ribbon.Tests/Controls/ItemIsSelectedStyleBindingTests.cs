namespace Fluent.Tests.Controls;

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// MVVM apps create backstage pages and gallery items from their data and bind each item's IsSelected in the ItemContainerStyle.
/// A click (or KeyTip, or focus) set IsSelected with the CLR setter. That puts a local value on top of the style's binding,
/// so even a TwoWay binding no longer worked: the view model was never told which page or item the user picked.
/// </summary>
[TestFixture]
public class ItemIsSelectedStyleBindingTests
{
    [Test]
    public void KeyTip_on_a_backstage_page_reaches_the_view_model()
    {
        var models = CreateModels();
        var tabControl = new BackstageTabControl
        {
            ItemsSource = models,
            ItemContainerStyle = CreateIsSelectedStyle(typeof(BackstageTabItem), BackstageTabItem.IsSelectedProperty)
        };

        using (new TestRibbonWindow(tabControl))
        {
            UIHelper.DoEvents();

            var page = (BackstageTabItem)tabControl.ItemContainerGenerator.ContainerFromIndex(1);
            Assert.That(page, Is.Not.Null, "Precondition: the second page must be generated.");

            ((IKeyTipedControl)page).OnKeyTipPressed();
            UIHelper.DoEvents();

            AssertSelectionReachedModel(page, BackstageTabItem.IsSelectedProperty, models, 1);
        }
    }

    [Test]
    public void Click_on_a_backstage_page_reaches_the_view_model()
    {
        var models = CreateModels();
        var tabControl = new BackstageTabControl
        {
            ItemsSource = models,
            ItemContainerStyle = CreateIsSelectedStyle(typeof(BackstageTabItem), BackstageTabItem.IsSelectedProperty)
        };

        using (new TestRibbonWindow(tabControl))
        {
            UIHelper.DoEvents();

            var page = (BackstageTabItem)tabControl.ItemContainerGenerator.ContainerFromIndex(1);
            Assert.That(page, Is.Not.Null, "Precondition: the second page must be generated.");

            page.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = page
            });
            UIHelper.DoEvents();

            AssertSelectionReachedModel(page, BackstageTabItem.IsSelectedProperty, models, 1);
        }
    }

    [Test]
    public void Click_on_a_gallery_item_reaches_the_view_model()
    {
        var models = CreateModels();
        var gallery = new Gallery
        {
            ItemsSource = models,
            ItemContainerStyle = CreateIsSelectedStyle(typeof(GalleryItem), GalleryItem.IsSelectedProperty)
        };

        using (new TestRibbonWindow(gallery))
        {
            UIHelper.DoEvents();

            var item = (GalleryItem)gallery.ItemContainerGenerator.ContainerFromIndex(1);
            Assert.That(item, Is.Not.Null, "Precondition: the second item must be generated.");

            item.RaiseClick();
            UIHelper.DoEvents();

            AssertSelectionReachedModel(item, GalleryItem.IsSelectedProperty, models, 1);
        }
    }

    private static List<ItemModel> CreateModels()
    {
        return new List<ItemModel>
        {
            new() { Name = "First" },
            new() { Name = "Second" },
            new() { Name = "Third" }
        };
    }

    // The usual MVVM style: IsSelected bound TwoWay to the item's view model (the container's DataContext).
    private static Style CreateIsSelectedStyle(System.Type targetType, DependencyProperty isSelectedProperty)
    {
        return new Style(targetType)
        {
            Setters =
            {
                new Setter(isSelectedProperty, new Binding(nameof(ItemModel.IsSelected)) { Mode = BindingMode.TwoWay })
            }
        };
    }

    private static void AssertSelectionReachedModel(FrameworkElement container, DependencyProperty isSelectedProperty, List<ItemModel> models, int index)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That((bool)container.GetValue(isSelectedProperty), Is.True, "Precondition: the item must be selected.");
            Assert.That(DependencyPropertyHelper.GetValueSource(container, isSelectedProperty).BaseValueSource, Is.Not.EqualTo(BaseValueSource.Local), "IsSelected must still come from the style's binding, not from a local value.");
            Assert.That(models[index].IsSelected, Is.True, "The view model must be told that the item was selected.");
            Assert.That(models.Count(x => x.IsSelected), Is.EqualTo(1), "Only the selected item's view model may be selected.");
        }
    }

    private sealed class ItemModel : INotifyPropertyChanged
    {
        private bool isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsSelected)));
            }
        }

        public override string ToString() => this.Name;
    }
}
