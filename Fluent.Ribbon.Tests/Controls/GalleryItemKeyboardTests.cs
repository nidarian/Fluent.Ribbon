namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// A <see cref="GalleryItem"/> is clicked from the keyboard on key up, like a button.
/// Buttons are activated with Enter and Space, but GalleryItem.OnKeyUp only reacted to Enter.
/// </summary>
[TestFixture]
public class GalleryItemKeyboardTests
{
    [Test]
    public void Space_clicks_the_item()
    {
        var item = new GalleryItem { Content = "Item" };

        var clickCount = 0;

        // handledEventsToo, because GalleryItem's own click handling marks the Click event handled.
        item.AddHandler(GalleryItem.ClickEvent, new RoutedEventHandler((_, _) => clickCount++), true);

        using (new TestRibbonWindow(item))
        {
            UIHelper.DoEvents();

            Assert.That(clickCount, Is.EqualTo(0), "precondition: not clicked yet");

            PressAndReleaseKey(item, Key.Space);

            Assert.That(clickCount, Is.EqualTo(1), "Space should click the item like Enter does");
        }
    }

    // Simulates a full key press the way WPF input delivers it:
    // PreviewKeyDown/KeyDown and then PreviewKeyUp/KeyUp, each pair sharing one args object.
    private static void PressAndReleaseKey(UIElement target, Key key)
    {
        RaiseKeyEvents(target, key, Keyboard.PreviewKeyDownEvent, Keyboard.KeyDownEvent);
        RaiseKeyEvents(target, key, Keyboard.PreviewKeyUpEvent, Keyboard.KeyUpEvent);
    }

    private static void RaiseKeyEvents(UIElement target, Key key, RoutedEvent previewEvent, RoutedEvent bubbleEvent)
    {
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(target), 0, key)
        {
            RoutedEvent = previewEvent
        };

        target.RaiseEvent(args);

        args.RoutedEvent = bubbleEvent;
        target.RaiseEvent(args);

        UIHelper.DoEvents();
    }
}
