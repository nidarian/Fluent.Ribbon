namespace Fluent.Tests.Controls;

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// While the Quick Access Toolbar copy of an <see cref="InRibbonGallery"/> has its drop down open, the original lends its items to the copy.
/// To do that it set its own <see cref="System.Windows.Controls.Primitives.Selector.SelectedItem"/> to null (and back when the copy closed).
/// <see cref="System.Windows.Controls.Primitives.Selector.SelectedItem"/> binds TwoWay by default, so the app's view model received null
/// for a moment each time the copy opened (an app applying the selection, a font or a style, would apply "nothing").
/// A OneWay binding was replaced by a local value, so the gallery stopped following the view model.
/// </summary>
[TestFixture]
public class InRibbonGalleryQuickAccessCopySelectionTests
{
    private static readonly string[] Items = { "A", "B", "C" };

    [Test]
    public void Opening_the_copy_does_not_push_null_into_a_TwoWay_SelectedItem_binding()
    {
        var model = new SelectionModel { Selected = "B" };
        var original = CreateGallery();
        original.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedItemProperty, new Binding(nameof(SelectionModel.Selected)) { Source = model, Mode = BindingMode.TwoWay });

        Assert.That(original.SelectedItem, Is.EqualTo("B"), "Precondition: the gallery shows the view model's selection.");
        model.History.Clear();

        var copy = (InRibbonGallery)original.CreateQuickAccessItem();

        using (new TestRibbonWindow(new StackPanel { Children = { original, copy } }))
        {
            UIHelper.DoEvents();

            OpenAndCloseCopy(copy, () =>
            {
                Assert.That(copy.SelectedItem, Is.EqualTo("B"), "Precondition: the open copy shows the original's selection.");
            });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.History, Is.Empty, "Opening and closing the copy must not write anything into the view model.");
                Assert.That(model.Selected, Is.EqualTo("B"), "The view model keeps its selection.");
                Assert.That(original.SelectedItem, Is.EqualTo("B"), "The original shows the selection again after the copy closed.");
                Assert.That(BindingOperations.GetBindingExpression(original, System.Windows.Controls.Primitives.Selector.SelectedItemProperty), Is.Not.Null, "The app's binding on SelectedItem must still exist.");
            }
        }
    }

    [Test]
    public void Opening_the_copy_keeps_a_OneWay_SelectedItem_binding()
    {
        var model = new SelectionModel { Selected = "B" };
        var original = CreateGallery();
        original.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedItemProperty, new Binding(nameof(SelectionModel.Selected)) { Source = model, Mode = BindingMode.OneWay });

        var copy = (InRibbonGallery)original.CreateQuickAccessItem();

        using (new TestRibbonWindow(new StackPanel { Children = { original, copy } }))
        {
            UIHelper.DoEvents();

            OpenAndCloseCopy(copy, null);

            Assert.That(original.SelectedItem, Is.EqualTo("B"), "The original shows the selection again after the copy closed.");
            Assert.That(BindingOperations.GetBindingExpression(original, System.Windows.Controls.Primitives.Selector.SelectedItemProperty), Is.Not.Null, "The app's OneWay binding on SelectedItem was lost.");

            model.Selected = "C";
            Assert.That(original.SelectedItem, Is.EqualTo("C"), "The original must still follow the view model.");
        }
    }

    [Test]
    public void Opening_the_copy_does_not_push_minus_one_into_a_TwoWay_SelectedIndex_binding()
    {
        var model = new SelectionModel { Index = 1 };
        var original = CreateGallery();
        original.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedIndexProperty, new Binding(nameof(SelectionModel.Index)) { Source = model, Mode = BindingMode.TwoWay });

        Assert.That(original.SelectedItem, Is.EqualTo("B"), "Precondition: the gallery shows the view model's selection.");
        model.History.Clear();

        var copy = (InRibbonGallery)original.CreateQuickAccessItem();

        using (new TestRibbonWindow(new StackPanel { Children = { original, copy } }))
        {
            UIHelper.DoEvents();

            OpenAndCloseCopy(copy, null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.History, Is.Empty, "Opening and closing the copy must not write anything into the view model.");
                Assert.That(model.Index, Is.EqualTo(1), "The view model keeps its selection.");
                Assert.That(original.SelectedIndex, Is.EqualTo(1), "The original shows the selection again after the copy closed.");
            }
        }
    }

    /// <summary>
    /// Guard: a selection made in the open copy must still reach the original and a TwoWay view model when the copy closes.
    /// </summary>
    [Test]
    public void A_selection_made_in_the_copy_reaches_the_original_and_a_TwoWay_source()
    {
        var model = new SelectionModel { Selected = "B" };
        var original = CreateGallery();
        original.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedItemProperty, new Binding(nameof(SelectionModel.Selected)) { Source = model, Mode = BindingMode.TwoWay });

        var copy = (InRibbonGallery)original.CreateQuickAccessItem();

        using (new TestRibbonWindow(new StackPanel { Children = { original, copy } }))
        {
            UIHelper.DoEvents();

            OpenAndCloseCopy(copy, () => copy.SelectedItem = "C");

            Assert.That(original.SelectedItem, Is.EqualTo("C"), "The selection made in the copy must reach the original.");
            Assert.That(model.Selected, Is.EqualTo("C"), "The selection made in the copy must reach the TwoWay source.");
            Assert.That(BindingOperations.GetBindingExpression(original, System.Windows.Controls.Primitives.Selector.SelectedItemProperty), Is.Not.Null, "The app's binding on SelectedItem must still exist.");

            model.Selected = "A";
            Assert.That(original.SelectedItem, Is.EqualTo("A"), "The original must still follow the view model.");
        }
    }

    /// <summary>
    /// Guard: without any binding the original keeps its selection, and takes a selection made in the copy.
    /// </summary>
    [Test]
    public void Without_a_binding_the_original_keeps_or_takes_the_selection()
    {
        var original = CreateGallery();
        original.SelectedItem = "B";

        var copy = (InRibbonGallery)original.CreateQuickAccessItem();

        using (new TestRibbonWindow(new StackPanel { Children = { original, copy } }))
        {
            UIHelper.DoEvents();

            OpenAndCloseCopy(copy, null);
            Assert.That(original.SelectedItem, Is.EqualTo("B"), "The original keeps its selection.");

            OpenAndCloseCopy(copy, () => copy.SelectedItem = "A");
            Assert.That(original.SelectedItem, Is.EqualTo("A"), "The original takes the selection made in the copy.");
        }
    }

    private static InRibbonGallery CreateGallery()
    {
        return new InRibbonGallery
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Height = RibbonTabControl.DefaultContentHeight - 10,
            MinItemsInRow = 1,
            MaxItemsInRow = 5,
            ItemWidth = 50,
            ItemHeight = 18,
            ItemsSource = Items
        };
    }

    private static void OpenAndCloseCopy(InRibbonGallery copy, System.Action whileOpen)
    {
        copy.IsDropDownOpen = true;
        UIHelper.DoEvents();

        Assert.That(copy.Items.Count, Is.EqualTo(Items.Length), "Precondition: the open copy shows the original's items.");

        whileOpen?.Invoke();

        copy.IsDropDownOpen = false;
        UIHelper.DoEvents();

        Assert.That(copy.Items.Count, Is.Zero, "Precondition: the items went back to the original.");
    }

    private sealed class SelectionModel : INotifyPropertyChanged
    {
        private object selected;
        private int index = -1;

        public event PropertyChangedEventHandler PropertyChanged;

        // Every value the gallery wrote into the view model.
        public List<object> History { get; } = new();

        public object Selected
        {
            get => this.selected;
            set
            {
                this.History.Add(value);

                if (Equals(this.selected, value))
                {
                    return;
                }

                this.selected = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Selected)));
            }
        }

        public int Index
        {
            get => this.index;
            set
            {
                this.History.Add(value);

                if (this.index == value)
                {
                    return;
                }

                this.index = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Index)));
            }
        }
    }
}
