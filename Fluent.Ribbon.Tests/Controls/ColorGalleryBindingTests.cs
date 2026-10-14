namespace Fluent.Tests.Controls;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Apps commonly drive <see cref="ColorGallery.SelectedColor"/> from a view model, e.g. <c>SelectedColor="{Binding Color}"</c>,
/// which is OneWay because the property does not bind two way by default.
/// Writing SelectedColor with SetValue (or the CLR setter) when the user picks a color replaces that binding with a local value,
/// so after the first pick the view model can no longer update the gallery (e.g. the caret moved to differently colored text).
/// These tests pick colors through the gallery's own paths (color list, Automatic, No Color) and check the binding survives.
/// </summary>
[TestFixture]
public class ColorGalleryBindingTests
{
    [Test]
    public void Picking_a_color_keeps_OneWay_binding()
    {
        // Selecting an item in the list box is what a mouse pick does (SelectionChanged commits the color).
        AssertOneWayBindingSurvivesPick(
            ColorGallery.StandardColors[0],
            (_, listBox) => listBox.SelectedItem = ColorGallery.StandardColors[1],
            ColorGallery.StandardColors[1]);
    }

    [Test]
    public void Automatic_click_keeps_OneWay_binding()
    {
        AssertOneWayBindingSurvivesPick(
            ColorGallery.StandardColors[1],
            (gallery, _) => ClickMenuItem(gallery, "PART_AutomaticColor"),
            null);
    }

    [Test]
    public void NoColor_click_keeps_OneWay_binding()
    {
        AssertOneWayBindingSurvivesPick(
            ColorGallery.StandardColors[1],
            (gallery, _) => ClickMenuItem(gallery, "PART_NoColor"),
            Colors.Transparent);
    }

    [Test]
    public void TwoWay_binding_receives_picked_colors()
    {
        var gallery = new ColorGallery();
        var state = new ColorState { Color = ColorGallery.StandardColors[0] };
        gallery.SetBinding(ColorGallery.SelectedColorProperty, new Binding(nameof(ColorState.Color)) { Source = state, Mode = BindingMode.TwoWay });

        using (new TestRibbonWindow(gallery))
        {
            var listBox = GetStandardColorsListBox(gallery);

            listBox.SelectedItem = ColorGallery.StandardColors[1];
            UIHelper.DoEvents();
            Assert.That(state.Color, Is.EqualTo(ColorGallery.StandardColors[1]), "The view model should receive the picked color.");

            ClickMenuItem(gallery, "PART_NoColor");
            Assert.That(state.Color, Is.EqualTo(Colors.Transparent), "The view model should receive No Color.");

            ClickMenuItem(gallery, "PART_AutomaticColor");
            Assert.That(state.Color, Is.Null, "The view model should receive Automatic.");

            Assert.That(BindingOperations.GetBindingExpression(gallery, ColorGallery.SelectedColorProperty), Is.Not.Null, "TwoWay binding on SelectedColor was lost.");

            state.Color = ColorGallery.StandardColors[6];
            Assert.That(gallery.SelectedColor, Is.EqualTo(ColorGallery.StandardColors[6]), "The view model should still update the gallery.");
        }
    }

    private static void AssertOneWayBindingSurvivesPick(Color? initialColor, Action<ColorGallery, ListBox> pick, Color? expectedAfterPick)
    {
        var gallery = new ColorGallery();
        var state = new ColorState { Color = initialColor };
        gallery.SetBinding(ColorGallery.SelectedColorProperty, new Binding(nameof(ColorState.Color)) { Source = state, Mode = BindingMode.OneWay });

        using (new TestRibbonWindow(gallery))
        {
            var listBox = GetStandardColorsListBox(gallery);

            Assert.That(gallery.SelectedColor, Is.EqualTo(initialColor), "Precondition: the gallery should show the bound color.");

            // The user picks something in the gallery.
            pick(gallery, listBox);
            UIHelper.DoEvents();

            Assert.That(gallery.SelectedColor, Is.EqualTo(expectedAfterPick), "Precondition: the pick should change SelectedColor.");

            // SetValue would have replaced the binding with a local value; SetCurrentValue keeps it.
            Assert.That(BindingOperations.GetBindingExpression(gallery, ColorGallery.SelectedColorProperty), Is.Not.Null, "OneWay binding on SelectedColor was lost after the user picked a color.");

            // The view model changes (e.g. the caret moved to text with another color): the gallery has to follow.
            state.Color = ColorGallery.StandardColors[6];
            Assert.That(gallery.SelectedColor, Is.EqualTo(ColorGallery.StandardColors[6]), "The view model should still update the gallery after the user picked a color.");
            Assert.That(listBox.SelectedItem, Is.EqualTo(ColorGallery.StandardColors[6]), "The gallery should highlight the color from the view model.");
        }
    }

    // In the default StandardColors mode the standard colors are shown in the standard gradient list box.
    private static ListBox GetStandardColorsListBox(ColorGallery gallery)
    {
        gallery.ApplyTemplate();
        UIHelper.DoEvents();

        Assert.That(gallery.Mode, Is.EqualTo(ColorGalleryMode.StandardColors), "Precondition: default mode.");

        var listBox = (ListBox)gallery.Template.FindName("PART_StandardGradientColorsListBox", gallery);
        Assert.That(listBox, Is.Not.Null, "PART_StandardGradientColorsListBox should exist");
        Assert.That(listBox.Items.Contains(ColorGallery.StandardColors[1]), Is.True, "Precondition: the list box should show the standard colors.");

        return listBox;
    }

    private static void ClickMenuItem(ColorGallery gallery, string partName)
    {
        var menuItem = (UIElement)gallery.Template.FindName(partName, gallery);
        Assert.That(menuItem, Is.Not.Null, $"{partName} should exist");

        menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, menuItem));
        UIHelper.DoEvents();
    }

    private sealed class ColorState : INotifyPropertyChanged
    {
        private Color? color;

        public event PropertyChangedEventHandler PropertyChanged;

        public Color? Color
        {
            get => this.color;
            set
            {
                if (this.color == value)
                {
                    return;
                }

                this.color = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Color)));
            }
        }
    }
}
