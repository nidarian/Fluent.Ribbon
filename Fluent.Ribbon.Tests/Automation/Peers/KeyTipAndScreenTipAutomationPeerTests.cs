namespace Fluent.Tests.Automation.Peers;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using NUnit.Framework;

/// <summary>
/// KeyTips are the ribbon's keyboard shortcuts and ScreenTips its rich tooltips.
/// Screen readers announce AccessKey and HelpText, so every ribbon control should expose its KeyTip and ScreenTip text there,
/// not just <see cref="Button"/>.
/// </summary>
[TestFixture]
public class KeyTipAndScreenTipAutomationPeerTests
{
    // Types (not instances) because WPF controls must be created on the STA test thread, not during test discovery.
    private static IEnumerable<Type> ControlTypes()
    {
        yield return typeof(Button);
        yield return typeof(ToggleButton);
        yield return typeof(CheckBox);
        yield return typeof(RadioButton);
        yield return typeof(DropDownButton);
        yield return typeof(SplitButton);
        yield return typeof(Spinner);
        yield return typeof(ComboBox);
        yield return typeof(TextBox);
        yield return typeof(InRibbonGallery);
        yield return typeof(BackstageTabItem);
        yield return typeof(RibbonTabItem);
    }

    [Test]
    [TestCaseSource(nameof(ControlTypes))]
    public void KeyTip_and_ScreenTip_are_exposed_as_AccessKey_and_HelpText(Type controlType)
    {
        var control = (FrameworkElement)Activator.CreateInstance(controlType)!;
        KeyTip.SetKeys(control, "B");
        control.ToolTip = new ScreenTip { Title = "T", Text = "Help" };

        var peer = UIElementAutomationPeer.CreatePeerForElement(control);
        Assert.That(peer, Is.Not.Null, "Precondition: the control creates an automation peer");
        Assert.That(peer.GetType().Namespace, Is.EqualTo("Fluent.Automation.Peers"), "Precondition: the control creates one of our automation peers");

        Assert.That(peer.GetAccessKey(), Is.EqualTo("B"), "The KeyTip must be exposed as AccessKey");
        Assert.That(peer.GetHelpText(), Is.EqualTo("Help"), "The ScreenTip text must be exposed as HelpText");
    }

    [Test]
    [TestCaseSource(nameof(ControlTypes))]
    public void Explicit_AutomationProperties_win_over_KeyTip_and_ScreenTip(Type controlType)
    {
        var control = (FrameworkElement)Activator.CreateInstance(controlType)!;
        KeyTip.SetKeys(control, "B");
        control.ToolTip = new ScreenTip { Title = "T", Text = "Help" };
        AutomationProperties.SetHelpText(control, "Explicit help");

        var peer = UIElementAutomationPeer.CreatePeerForElement(control);
        Assert.That(peer, Is.Not.Null, "Precondition: the control creates an automation peer");

        Assert.That(peer.GetHelpText(), Is.EqualTo("Explicit help"), "An explicit AutomationProperties.HelpText must not be replaced by the ScreenTip text");
    }
}
