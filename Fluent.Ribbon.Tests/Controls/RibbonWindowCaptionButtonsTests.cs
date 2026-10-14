namespace Fluent.Tests.Controls;

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// <see cref="RibbonWindow"/> inherits ShowMinButton, ShowMaxRestoreButton and UseNativeCaptionButtons from
/// ControlzEx' WindowChromeWindow. ControlzEx documents ShowMinButton/ShowMaxRestoreButton as
/// "whether the button is visible and the system menu entry is enabled", but it only changes the native window style.
/// The minimize, maximize, restore and close buttons a <see cref="RibbonWindow"/> shows are Fluent's own
/// (the template of <see cref="WindowCommands"/>), and they only looked at ResizeMode:
/// - ShowMinButton="False" / ShowMaxRestoreButton="False" still showed the buttons,
///   and a double-click on the title bar still maximized the window.
/// - UseNativeCaptionButtons="True" makes Windows draw its own caption buttons, but Fluent's buttons
///   stayed visible on top of them and no room was kept free for them.
/// </summary>
[TestFixture]
public class RibbonWindowCaptionButtonsTests
{
    private static readonly string[] CaptionButtonNames = { "PART_Min", "PART_Max", "PART_Restore", "PART_Close" };

    /// <summary>
    /// Baseline: with default settings Fluent's own minimize, maximize and close buttons are visible
    /// (restore is only shown while maximized).
    /// </summary>
    [Test]
    public void Baseline_caption_buttons_are_visible_by_default()
    {
        using (var window = new TestRibbonWindow())
        {
            UIHelper.DoEvents();

            Assert.That(GetCaptionButton(window, "PART_Min").Visibility, Is.EqualTo(Visibility.Visible), "minimize button");
            Assert.That(GetCaptionButton(window, "PART_Max").Visibility, Is.EqualTo(Visibility.Visible), "maximize button");
            Assert.That(GetCaptionButton(window, "PART_Close").Visibility, Is.EqualTo(Visibility.Visible), "close button");
        }
    }

    [Test]
    public void ShowMinButton_false_hides_the_minimize_button()
    {
        using (var window = new TestRibbonWindow())
        {
            window.ShowMinButton = false;
            UIHelper.DoEvents();

            Assert.That(GetCaptionButton(window, "PART_Min").Visibility, Is.EqualTo(Visibility.Collapsed), "ShowMinButton=False must hide the minimize button");
            Assert.That(GetCaptionButton(window, "PART_Max").Visibility, Is.EqualTo(Visibility.Visible), "the maximize button must stay visible");
        }
    }

    [Test]
    public void ShowMaxRestoreButton_false_hides_the_maximize_button()
    {
        using (var window = new TestRibbonWindow())
        {
            window.ShowMaxRestoreButton = false;
            UIHelper.DoEvents();

            Assert.That(GetCaptionButton(window, "PART_Max").Visibility, Is.EqualTo(Visibility.Collapsed), "ShowMaxRestoreButton=False must hide the maximize button");
            Assert.That(GetCaptionButton(window, "PART_Min").Visibility, Is.EqualTo(Visibility.Visible), "the minimize button must stay visible");
        }
    }

    [Test]
    public void ShowMaxRestoreButton_false_hides_the_restore_button_of_a_maximized_window()
    {
        using (var window = new TestRibbonWindow())
        {
            window.ShowMaxRestoreButton = false;
            window.WindowState = WindowState.Maximized;
            UIHelper.DoEvents();

            Assert.That(window.WindowState, Is.EqualTo(WindowState.Maximized), "precondition: the window is maximized");
            Assert.That(GetCaptionButton(window, "PART_Restore").Visibility, Is.EqualTo(Visibility.Collapsed), "ShowMaxRestoreButton=False must hide the restore button");
        }
    }

    /// <summary>
    /// Baseline: proves that the simulated double-click on the title bar really maximizes the window (on CI too).
    /// </summary>
    [Test]
    public void Baseline_double_click_on_title_bar_maximizes()
    {
        using (var window = new TestRibbonWindow())
        {
            UIHelper.DoEvents();

            DoubleClickTitleBar(window);

            Assert.That(window.WindowState, Is.EqualTo(WindowState.Maximized), "Baseline: a double-click on the title bar must maximize the window");
        }
    }

    [Test]
    public void ShowMaxRestoreButton_false_double_click_on_title_bar_does_not_maximize()
    {
        using (var window = new TestRibbonWindow())
        {
            window.ShowMaxRestoreButton = false;
            UIHelper.DoEvents();

            DoubleClickTitleBar(window);

            Assert.That(window.WindowState, Is.EqualTo(WindowState.Normal), "ShowMaxRestoreButton=False: a double-click on the title bar must not maximize the window");
        }
    }

    [Test]
    public void UseNativeCaptionButtons_hides_the_own_caption_buttons()
    {
        using (var window = new TestRibbonWindow())
        {
            window.UseNativeCaptionButtons = true;
            UIHelper.DoEvents();

            foreach (var name in CaptionButtonNames)
            {
                Assert.That(GetCaptionButton(window, name).Visibility, Is.EqualTo(Visibility.Collapsed), $"UseNativeCaptionButtons=True must hide Fluent's own {name}, Windows draws its own buttons there");
            }
        }
    }

    [Test]
    public void UseNativeCaptionButtons_keeps_room_free_for_the_native_caption_buttons()
    {
        using (var window = new TestRibbonWindow())
        {
            window.UseNativeCaptionButtons = true;
            UIHelper.DoEvents();

            var nativeButtonsWidth = window.CaptionButtonsSize.Width;
            Assert.That(nativeButtonsWidth, Is.GreaterThan(0), "precondition: Windows reports the size of its caption buttons");

            var windowCommands = window.WindowCommands!;
            var ownVisibleButtonsWidth = CaptionButtonNames
                .Select(name => GetCaptionButton(window, name))
                .Where(x => x.Visibility == Visibility.Visible)
                .Sum(x => x.ActualWidth);

            // The width the window commands occupy without Fluent's own buttons must cover the native buttons,
            // otherwise the title and the app's own window commands are drawn below the native buttons.
            Assert.That(windowCommands.ActualWidth - ownVisibleButtonsWidth, Is.GreaterThanOrEqualTo(nativeButtonsWidth - 1), "room kept free for the native caption buttons");
        }
    }

    private static Button GetCaptionButton(RibbonWindow window, string name)
    {
        var windowCommands = window.WindowCommands;
        Assert.That(windowCommands, Is.Not.Null, "the window has window commands");

        windowCommands!.ApplyTemplate();

        var button = windowCommands.Template.FindName(name, windowCommands) as Button;
        Assert.That(button, Is.Not.Null, $"{name} is part of the window commands template");

        return button!;
    }

    // Simulates the second click of a double-click on the title bar, the way RibbonTitleBar receives it.
    private static void DoubleClickTitleBar(RibbonWindow window)
    {
        var titleBar = window.TitleBar;
        Assert.That(titleBar, Is.Not.Null, "the window has a title bar");

        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonDownEvent,
            Source = titleBar
        };

        // ClickCount has no public setter, WPF input sets it for real double-clicks.
        var clickCountSetter = typeof(MouseButtonEventArgs).GetProperty(nameof(MouseButtonEventArgs.ClickCount))!.GetSetMethod(nonPublic: true);
        Assert.That(clickCountSetter, Is.Not.Null, "ClickCount can be set");
        clickCountSetter!.Invoke(args, new object[] { 2 });

        titleBar!.RaiseEvent(args);

        // Maximizing is done by posting a window message, so let it be processed.
        UIHelper.DoEvents();
        UIHelper.DoEvents();
    }
}
