namespace Fluent.Tests.Automation.Peers;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// A ribbon control without a text Header (an icon-only button, common in small groups and on the Quick Access Toolbar)
/// had no name for screen readers, even when its <see cref="ScreenTip"/> carries a title like "Bold".
/// The title is the best name the control has, so the automation peers fall back to it.
/// </summary>
[TestFixture]
public class NameFromScreenTipTitleTests
{
    // Types (not instances) because WPF controls must be created on the STA test thread, not during test discovery.
    private static IEnumerable<Type> ControlTypes()
    {
        yield return typeof(Button);
        yield return typeof(Spinner);
        yield return typeof(ComboBox);
    }

    [Test]
    [TestCaseSource(nameof(ControlTypes))]
    public void Control_with_an_empty_Header_is_named_by_its_ScreenTip_title(Type controlType)
    {
        var control = (FrameworkElement)Activator.CreateInstance(controlType)!;
        ((IHeaderedControl)control).Header = string.Empty;
        control.ToolTip = new ScreenTip { Title = "Bold" };

        var peer = UIElementAutomationPeer.CreatePeerForElement(control);

        Assert.That(peer.GetName(), Is.EqualTo("Bold"), "An empty header is no name, so the ScreenTip title must be the name");
    }
}
