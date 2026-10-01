namespace Fluent.Tests.Controls;

using System;
using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for clicking a <see cref="GalleryItem"/> with the Enter key.
/// </summary>
[TestFixture]
public class GalleryItemKeyboardTests
{
    [Test]
    public void Enter_released_on_the_item_without_being_pressed_on_it_does_not_click()
    {
        // This is what happens when Enter opens an InRibbonGallery: the key goes down on the toggle button,
        // focus moves to the first item of the drop down, and the key comes up on that item.
        var item = new GalleryItem { Content = "Item" };

        using (new TestRibbonWindow(item))
        {
            var clickCount = CountClicks(item);

            RaiseKey(item, Key.Enter, Keyboard.KeyUpEvent);

            Assert.That(clickCount(), Is.EqualTo(0), "Enter that went down somewhere else must not click the item");
        }
    }

    [Test]
    public void Enter_pressed_and_released_on_the_item_clicks()
    {
        var item = new GalleryItem { Content = "Item" };

        using (new TestRibbonWindow(item))
        {
            var clickCount = CountClicks(item);

            RaiseKey(item, Key.Enter, Keyboard.KeyDownEvent);
            RaiseKey(item, Key.Enter, Keyboard.KeyUpEvent);

            Assert.That(clickCount(), Is.EqualTo(1), "A full Enter press on the item should click it once");
        }
    }

    // Returns a function giving the number of Click events raised so far.
    // handledEventsToo is needed because GalleryItem marks its own Click as handled.
    private static Func<int> CountClicks(GalleryItem item)
    {
        var count = 0;
        item.AddHandler(GalleryItem.ClickEvent, new RoutedEventHandler((_, _) => count++), true);
        return () => count;
    }

    // Simulates a key event the way WPF delivers it to the focused element.
    // KeyEventArgs needs a PresentationSource, which is why the item has to be shown in a window.
    private static void RaiseKey(UIElement target, Key key, RoutedEvent routedEvent)
    {
        var presentationSource = PresentationSource.FromVisual(target);
        Assert.That(presentationSource, Is.Not.Null, "Precondition: the item must be shown in a window");

        var args = new KeyEventArgs(Keyboard.PrimaryDevice, presentationSource, 0, key)
        {
            RoutedEvent = routedEvent
        };

        target.RaiseEvent(args);
        UIHelper.DoEvents();
    }
}
