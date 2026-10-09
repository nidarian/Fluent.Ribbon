namespace Fluent.Tests.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Apps commonly fill a drop down from a view model, for example ItemsSource="{Binding Fonts}".
/// While the drop down of a Quick Access Toolbar copy is open, the items are moved from the original control to the copy and back.
/// Moving them must not replace the binding on the original with a fixed value.
/// Otherwise, after the copy was opened once, the original (and every later copy) keeps showing the old list
/// when the view model assigns a new collection or the DataContext changes.
/// </summary>
[TestFixture]
public class QuickAccessItemsSourceBindingTests
{
    [TestCase("DropDownButton")]
    [TestCase("ComboBox")]
    [TestCase("MenuItem")]
    [TestCase("InRibbonGallery")]
    public void Opening_The_Copy_Keeps_The_ItemsSource_Binding_Of_The_Original(string kind)
    {
        var viewModel = new ItemsViewModel
        {
            Items = new[] { "old" }
        };

        var source = CreateSource(kind);
        source.DataContext = viewModel;
        BindingOperations.SetBinding(source, System.Windows.Controls.ItemsControl.ItemsSourceProperty, new Binding(nameof(ItemsViewModel.Items)) { Mode = BindingMode.OneWay });

        Assert.That(source.Items, Is.EqualTo(new[] { "old" }));

        var copy = ((IQuickAccessItemProvider)source).CreateQuickAccessItem();

        using (CreateWindow(source, copy))
        {
            SetIsDropDownOpen(copy, true);
            UIHelper.DoEvents();

            // While the copy is open it shows the items, and the original does not (each item can only be shown once).
            Assert.That(((System.Windows.Controls.ItemsControl)copy).Items, Is.EqualTo(new[] { "old" }), "The open copy does not show the items.");
            Assert.That(source.Items, Is.Empty);

            SetIsDropDownOpen(copy, false);
            UIHelper.DoEvents();

            Assert.That(source.Items, Is.EqualTo(new[] { "old" }), "The items did not come back to the original.");

            // The view model assigns a new collection, like a font list being refreshed.
            viewModel.Items = new[] { "new" };
            UIHelper.DoEvents();

            Assert.That(BindingOperations.GetBindingExpressionBase(source, System.Windows.Controls.ItemsControl.ItemsSourceProperty), Is.Not.Null, "The ItemsSource binding of the original was removed.");
            Assert.That(source.Items, Is.EqualTo(new[] { "new" }), "The original no longer follows its ItemsSource binding.");

            // The DataContext changes, like switching to another document.
            source.DataContext = new ItemsViewModel
            {
                Items = new[] { "other document" }
            };
            UIHelper.DoEvents();

            Assert.That(source.Items, Is.EqualTo(new[] { "other document" }), "The original does not follow a DataContext change.");

            // Opening the copy again shows the current items.
            SetIsDropDownOpen(copy, true);
            UIHelper.DoEvents();

            Assert.That(((System.Windows.Controls.ItemsControl)copy).Items, Is.EqualTo(new[] { "other document" }));

            SetIsDropDownOpen(copy, false);
            UIHelper.DoEvents();

            Assert.That(source.Items, Is.EqualTo(new[] { "other document" }));
        }
    }

    private static System.Windows.Controls.ItemsControl CreateSource(string kind)
    {
        switch (kind)
        {
            case "DropDownButton":
                return new DropDownButton();
            case "ComboBox":
                return new ComboBox();
            case "MenuItem":
                return new MenuItem { Header = "Header" };
            case "InRibbonGallery":
                return new InRibbonGallery
                {
                    Width = 100,
                    Height = 30
                };
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }

    private static void SetIsDropDownOpen(FrameworkElement copy, bool value)
    {
        switch (copy)
        {
            case DropDownButton dropDownButton:
                dropDownButton.IsDropDownOpen = value;
                break;
            case ComboBox comboBox:
                comboBox.IsDropDownOpen = value;
                break;
            case InRibbonGallery inRibbonGallery:
                inRibbonGallery.IsDropDownOpen = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(copy), copy, null);
        }
    }

    private static TestRibbonWindow CreateWindow(params UIElement[] elements)
    {
        var panel = new System.Windows.Controls.StackPanel();

        foreach (var element in elements)
        {
            panel.Children.Add(element);
        }

        var window = new TestRibbonWindow(panel);

        UIHelper.DoEvents();

        return window;
    }

    private sealed class ItemsViewModel : INotifyPropertyChanged
    {
        private IEnumerable<string> items;

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<string> Items
        {
            get => this.items;
            set
            {
                this.items = value;
                this.OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
