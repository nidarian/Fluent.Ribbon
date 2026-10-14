namespace Fluent.Tests.Controls;

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class ScreenTipPlacementTests
{
    // WPF passes popupSize to the custom placement callback in device pixels,
    // so the right-to-left offset must not be scaled by the DPI a second time
    // (the same reason the popup height is no longer scaled, see upstream #1165).
    [Test]
    [TestCase(FlowDirection.RightToLeft, 1.5, -300)]
    [TestCase(FlowDirection.RightToLeft, 1.0, -300)]
    [TestCase(FlowDirection.LeftToRight, 1.5, 0)]
    [TestCase(FlowDirection.LeftToRight, 1.0, 0)]
    public void Horizontal_Offset_Should_Be_Popup_Width_In_Device_Pixels(FlowDirection flowDirection, double dpiScale, double expectedX)
    {
        var target = new Border
        {
            Width = 100,
            Height = 20
        };

        using (var window = new TestRibbonWindow(target))
        {
            UIHelper.DoEvents();

            VisualTreeHelper.SetRootDpi(window, new DpiScale(dpiScale, dpiScale));

            Assert.That(VisualTreeHelper.GetDpi(target).DpiScaleX, Is.EqualTo(dpiScale), "Precondition: simulated DPI should be applied to the target.");

            var screenTip = new ScreenTip
            {
                PlacementTarget = target,
                FlowDirection = flowDirection
            };

            // popupSize is in device pixels: a 200 x 66.67 DIP screen tip at 150 % is 300 x 100 pixels.
            var placements = screenTip.CustomPopupPlacementCallback(new Size(300, 100), new Size(150, 30), default);

            Assert.That(placements, Is.Not.Empty);
            Assert.That(placements.Select(x => x.Point.X), Is.All.EqualTo(expectedX));
        }
    }
}
