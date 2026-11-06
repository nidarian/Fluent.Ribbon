namespace Fluent.Tests.Controls;

using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// An app can open a backstage page or pick a gallery item from its view model with an explicit OneWay binding on IsSelected.
/// Selecting the page or item with a click or its KeyTip must not replace that binding with a local value,
/// otherwise the view model can no longer select it (round 8 checked only TwoWay style bindings, which the CLR setter keeps).
/// </summary>
[TestFixture]
public class ItemIsSelectedOneWayBindingTests
{
    [Test]
    public void KeyTip_on_a_backstage_page_keeps_its_OneWay_IsSelected_binding()
    {
        var model = new SelectionModel();
        var first = new BackstageTabItem { Header = "First" };
        var second = new BackstageTabItem { Header = "Second" };
        second.SetBinding(BackstageTabItem.IsSelectedProperty, new Binding(nameof(SelectionModel.IsSelected)) { Source = model, Mode = BindingMode.OneWay });
        var tabControl = new BackstageTabControl { Items = { first, second } };

        using (new TestRibbonWindow(tabControl))
        {
            UIHelper.DoEvents();

            ((IKeyTipedControl)second).OnKeyTipPressed();
            UIHelper.DoEvents();

            Assert.That(second.IsSelected, Is.True, "Precondition: the KeyTip selects the page");
            Assert.That(BindingOperations.GetBindingExpression(second, BackstageTabItem.IsSelectedProperty), Is.Not.Null, "The app's OneWay binding on IsSelected was lost");

            // The view model still says false (OneWay never wrote back); select the first page, then let the app select the second.
            ((IKeyTipedControl)first).OnKeyTipPressed();
            UIHelper.DoEvents();
            model.IsSelected = true;
            UIHelper.DoEvents();

            Assert.That(second.IsSelected, Is.True, "The app must still be able to select the page");
        }
    }

    [Test]
    public void Click_on_a_backstage_page_keeps_its_OneWay_IsSelected_binding()
    {
        var model = new SelectionModel();
        var first = new BackstageTabItem { Header = "First" };
        var second = new BackstageTabItem { Header = "Second" };
        second.SetBinding(BackstageTabItem.IsSelectedProperty, new Binding(nameof(SelectionModel.IsSelected)) { Source = model, Mode = BindingMode.OneWay });
        var tabControl = new BackstageTabControl { Items = { first, second } };

        using (new TestRibbonWindow(tabControl))
        {
            UIHelper.DoEvents();

            second.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = second
            });
            UIHelper.DoEvents();

            Assert.That(second.IsSelected, Is.True, "Precondition: the click selects the page");
            Assert.That(BindingOperations.GetBindingExpression(second, BackstageTabItem.IsSelectedProperty), Is.Not.Null, "The app's OneWay binding on IsSelected was lost");
        }
    }

    [Test]
    public void Click_on_a_gallery_item_keeps_its_OneWay_IsSelected_binding()
    {
        var model = new SelectionModel();
        var first = new GalleryItem { Content = "First" };
        var second = new GalleryItem { Content = "Second" };
        second.SetBinding(GalleryItem.IsSelectedProperty, new Binding(nameof(SelectionModel.IsSelected)) { Source = model, Mode = BindingMode.OneWay });
        var gallery = new Gallery { Items = { first, second } };

        using (new TestRibbonWindow(gallery))
        {
            UIHelper.DoEvents();

            second.RaiseClick();
            UIHelper.DoEvents();

            Assert.That(second.IsSelected, Is.True, "Precondition: the click selects the item");
            Assert.That(BindingOperations.GetBindingExpression(second, GalleryItem.IsSelectedProperty), Is.Not.Null, "The app's OneWay binding on IsSelected was lost");

            first.RaiseClick();
            UIHelper.DoEvents();
            model.IsSelected = true;
            UIHelper.DoEvents();

            Assert.That(second.IsSelected, Is.True, "The app must still be able to select the item");
        }
    }

    private sealed class SelectionModel : INotifyPropertyChanged
    {
        private bool isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

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
    }
}
