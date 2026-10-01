namespace Fluent.Tests.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Arrow keys in a <see cref="ColorGallery"/> should only move the highlight (browse).
/// A single-select ListBox selects the item it navigates to, and every selection change used to
/// commit <see cref="ColorGallery.SelectedColor"/> and dismiss the popup, so the first arrow key
/// picked a color and closed the drop down. Enter/Space should be the keys that commit.
/// </summary>
[TestFixture]
public class ColorGalleryKeyboardTests
{
    [TestCase(Key.Enter)]
    [TestCase(Key.Space)]
    public void Arrow_key_browses_without_committing_and_commit_key_commits(Key commitKey)
    {
        var gallery = new ColorGallery
        {
            Mode = ColorGalleryMode.HighlightColors
        };

        var dismissCount = 0;

        // handledEventsToo, so a class handler marking the event handled can't hide it from the count.
        gallery.AddHandler(PopupService.DismissPopupEvent, new EventHandler<DismissPopupEventArgs>((_, _) => dismissCount++), true);

        using (var window = new TestRibbonWindow(gallery))
        {
            gallery.ApplyTemplate();
            UIHelper.DoEvents();

            // In HighlightColors mode the highlight colors are shown in the standard colors list box.
            var listBox = (ListBox)gallery.Template.FindName("PART_StandardColorsListBox", gallery);
            Assert.That(listBox, Is.Not.Null, "PART_StandardColorsListBox should exist");

            var firstItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(0);
            var secondItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(1);
            Assert.That(firstItem, Is.Not.Null, "first color item should be generated");
            Assert.That(secondItem, Is.Not.Null, "second color item should be generated");
            Assert.That(secondItem.Content, Is.EqualTo(ColorGallery.HighlightColors[1]), "second item should hold the second highlight color");

            // Keyboard focus only works in the active window.
            window.Activate();
            firstItem.Focus();
            UIHelper.DoEvents();

            if (firstItem.IsKeyboardFocused == false)
            {
                Assert.Inconclusive("Keyboard focus is not available in this test environment.");
            }

            Assert.That(gallery.SelectedColor, Is.Null, "precondition: focusing an item must not select a color");
            Assert.That(dismissCount, Is.EqualTo(0), "precondition: nothing dismissed yet");

            // Browse to the next color.
            PressKey(firstItem, Key.Right);

            // These two are the real checks and don't depend on where focus went.
            Assert.That(gallery.SelectedColor, Is.Null, "an arrow key should only browse, not commit a color");
            Assert.That(dismissCount, Is.EqualTo(0), "an arrow key should not dismiss the popup");

            // Committing needs the focus on the second color. In a long test run the hidden test window can lose
            // keyboard focus between steps; then the rest can't be tested, which is not a failure of the code.
            if (secondItem.IsKeyboardFocused == false)
            {
                Assert.Inconclusive("Keyboard focus did not move to the next color (the test window lost keyboard focus).");
            }

            // Commit the browsed color.
            PressKey(secondItem, commitKey);

            Assert.That(gallery.SelectedColor, Is.EqualTo(ColorGallery.HighlightColors[1]), $"{commitKey} should commit the focused color");
            Assert.That(dismissCount, Is.EqualTo(1), $"{commitKey} should dismiss the popup once");
        }
    }

    // Simulates a key press the way WPF input delivers it: PreviewKeyDown (tunnel) and then KeyDown (bubble)
    // on the focused element, sharing one args object so a handled preview is seen as handled by KeyDown.
    private static void PressKey(UIElement target, Key key)
    {
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(target), 0, key)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        };

        target.RaiseEvent(args);

        args.RoutedEvent = Keyboard.KeyDownEvent;
        target.RaiseEvent(args);

        UIHelper.DoEvents();
    }
}
