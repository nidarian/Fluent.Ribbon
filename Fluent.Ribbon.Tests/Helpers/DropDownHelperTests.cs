namespace Fluent.Tests.Helpers;

using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Fluent.Helpers;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class DropDownHelperTests
{
    [Test]
    public void GetMaxDropDownHeight_Should_Return_Device_Independent_Units_At_High_Dpi()
    {
        var dropDownButton = new DropDownButton();

        using (var window = new TestRibbonWindow(dropDownButton))
        {
            UIHelper.DoEvents();

            // Test machines usually run at 100%, so simulate a higher DPI on the root visual.
            var originalDpi = VisualTreeHelper.GetDpi(window);
            var simulatedDpi = new DpiScale(originalDpi.DpiScaleX * 2, originalDpi.DpiScaleY * 2);
            VisualTreeHelper.SetRootDpi(window, simulatedDpi);

            Assert.That(VisualTreeHelper.GetDpi(window).DpiScaleY, Is.EqualTo(simulatedDpi.DpiScaleY), "Precondition: simulated DPI should be applied to the window.");

            // WinForms reports the working area in physical pixels.
            var workingAreaHeightInPixels = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(window).Handle).WorkingArea.Height;
            var expected = Math.Floor(workingAreaHeightInPixels / simulatedDpi.DpiScaleY / 3D);

            Assert.That(expected, Is.Not.EqualTo(Math.Floor(workingAreaHeightInPixels / 3D)), "Precondition: pixels and device independent units should differ.");

            Assert.That(DropDownHelper.GetMaxDropDownHeight(dropDownButton, double.NaN), Is.EqualTo(expected));
        }
    }
}
