namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// <see cref="RibbonTitleBar"/> overrides HitTestCore so that it reports a hit even where it has no background
/// (so empty parts of the title bar can be used to drag the window).
/// The override returns a hit for any point it is asked about, so these tests check that the title bar
/// does not take mouse input that belongs to other elements: the window icon, the content below it,
/// or the empty caption area next to it. A point over a quick access button must still reach that button.
/// </summary>
[TestFixture]
public class RibbonTitleBarHitTestTests
{
    [Test]
    public void Window_icon_is_hit_and_not_the_title_bar()
    {
        using (var window = CreateWindow(out _, out _))
        {
            var titleBar = GetTitleBar(window);
            var icon = (Image)window.Template.FindName("PART_Icon", window);

            Assert.That(icon, Is.Not.Null, "precondition: the window template should contain the icon");
            Assert.That(icon.Source, Is.Not.Null, "precondition: the icon should have an image");
            Assert.That(icon.ActualWidth, Is.GreaterThan(0), "precondition: the icon should be laid out");

            var point = icon.TranslatePoint(new Point(icon.ActualWidth / 2, icon.ActualHeight / 2), window);
            var hit = window.InputHitTest(point) as DependencyObject;

            Assert.That(hit, Is.SameAs(icon), $"the window icon should be hit at {point}, but the hit was {Describe(hit)}");
            Assert.That(IsSelfOrDescendantOf(hit, titleBar), Is.False, "the title bar should not take a point over the window icon");
        }
    }

    [Test]
    public void Quick_access_button_in_title_bar_is_hit()
    {
        using (var window = CreateWindow(out var ribbon, out _))
        {
            var titleBar = GetTitleBar(window);

            Assert.That(titleBar.QuickAccessToolBar, Is.SameAs(ribbon.QuickAccessToolBar), "precondition: the quick access toolbar should be shown in the title bar");
            Assert.That(ribbon.QuickAccessToolBar.Items.Count, Is.EqualTo(1), "precondition: the quick access toolbar should show the pinned button");

            var quickAccessButton = (FrameworkElement)ribbon.QuickAccessToolBar.Items[0];

            Assert.That(quickAccessButton.ActualWidth, Is.GreaterThan(0), "precondition: the quick access button should be laid out");

            var point = quickAccessButton.TranslatePoint(new Point(quickAccessButton.ActualWidth / 2, quickAccessButton.ActualHeight / 2), window);
            var hit = window.InputHitTest(point) as DependencyObject;

            Assert.That(IsSelfOrDescendantOf(hit, quickAccessButton), Is.True, $"the quick access button should be hit at {point}, but the hit was {Describe(hit)}");
        }
    }

    [Test]
    public void Empty_content_below_the_title_bar_is_not_taken_by_the_title_bar()
    {
        using (var window = CreateWindow(out _, out var emptyArea))
        {
            var titleBar = GetTitleBar(window);

            Assert.That(emptyArea.ActualHeight, Is.GreaterThan(100), "precondition: the empty area should be laid out");

            // The empty area has no background, so it is not hit itself. The hit must go to whatever is behind it, not to the title bar.
            var point = emptyArea.TranslatePoint(new Point(emptyArea.ActualWidth / 2, emptyArea.ActualHeight / 2), window);
            var hit = window.InputHitTest(point) as DependencyObject;

            Assert.That(IsSelfOrDescendantOf(hit, titleBar), Is.False, $"the title bar should not take a point in the window content at {point}, but the hit was {Describe(hit)}");
        }
    }

    [Test]
    public void Points_just_outside_the_title_bar_are_not_taken_by_the_title_bar()
    {
        using (var window = CreateWindow(out _, out _))
        {
            var titleBar = GetTitleBar(window);

            Assert.That(titleBar.ActualWidth, Is.GreaterThan(0), "precondition: the title bar should be laid out");

            var below = titleBar.TranslatePoint(new Point(titleBar.ActualWidth / 2, titleBar.ActualHeight + 2), window);
            var hitBelow = window.InputHitTest(below) as DependencyObject;

            Assert.That(IsSelfOrDescendantOf(hitBelow, titleBar), Is.False, $"the title bar should not take a point just below it at {below}, but the hit was {Describe(hitBelow)}");

            // Left of the title bar is the margin of the window icon, which belongs to the caption area of the window.
            var left = titleBar.TranslatePoint(new Point(-2, titleBar.ActualHeight / 2), window);
            var hitLeft = window.InputHitTest(left) as DependencyObject;

            Assert.That(left.X, Is.GreaterThanOrEqualTo(0), "precondition: the point left of the title bar should be inside the window");
            Assert.That(IsSelfOrDescendantOf(hitLeft, titleBar), Is.False, $"the title bar should not take a point just left of it at {left}, but the hit was {Describe(hitLeft)}");
        }
    }

    private static TestRibbonWindow CreateWindow(out Ribbon ribbon, out FrameworkElement emptyArea)
    {
        // AutomaticStateManagement is off so this test doesn't load or save ribbon state in isolated storage.
        ribbon = new Ribbon
        {
            AutomaticStateManagement = false,
            Tabs =
            {
                new RibbonTabItem { Header = "Tab" }
            }
        };

        // No background, so this area itself is not hit-testable.
        emptyArea = new Canvas();

        var content = new DockPanel();
        DockPanel.SetDock(ribbon, Dock.Top);
        content.Children.Add(ribbon);
        content.Children.Add(emptyArea);

        var window = new TestRibbonWindow(content)
        {
            Icon = CreateIcon()
        };

        UIHelper.DoEvents();

        ribbon.AddToQuickAccessToolBar(new Fluent.Button { Header = "Quick" });

        window.UpdateLayout();
        UIHelper.DoEvents();

        return window;
    }

    private static RibbonTitleBar GetTitleBar(RibbonWindow window)
    {
        var titleBar = window.TitleBar;

        Assert.That(titleBar, Is.Not.Null, "precondition: the window should have a title bar");

        return titleBar;
    }

    private static BitmapSource CreateIcon()
    {
        var pixels = new byte[16 * 16 * 4];

        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = 0xFF;
        }

        var bitmap = BitmapSource.Create(16, 16, 96, 96, PixelFormats.Bgra32, null, pixels, 16 * 4);
        bitmap.Freeze();
        return bitmap;
    }

    private static bool IsSelfOrDescendantOf(DependencyObject element, DependencyObject ancestor)
    {
        while (element is not null)
        {
            if (ReferenceEquals(element, ancestor))
            {
                return true;
            }

            element = element is Visual
                ? VisualTreeHelper.GetParent(element)
                : LogicalTreeHelper.GetParent(element);
        }

        return false;
    }

    private static string Describe(DependencyObject element)
    {
        if (element is null)
        {
            return "nothing";
        }

        var name = (element as FrameworkElement)?.Name;
        return string.IsNullOrEmpty(name)
            ? element.GetType().Name
            : $"{element.GetType().Name} '{name}'";
    }
}
