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
        yield return typeof(ToggleButton);
        yield return typeof(CheckBox);
        yield return typeof(RadioButton);
        yield return typeof(DropDownButton);
        yield return typeof(SplitButton);
        yield return typeof(Spinner);
        yield return typeof(ComboBox);
        yield return typeof(TextBox);
        yield return typeof(InRibbonGallery);
    }

    [Test]
    [TestCaseSource(nameof(ControlTypes))]
    public void Control_without_Header_is_named_by_its_ScreenTip_title(Type controlType)
    {
        var control = (FrameworkElement)Activator.CreateInstance(controlType)!;
        control.ToolTip = new ScreenTip { Title = "Bold", Text = "Makes the text bold" };

        var peer = UIElementAutomationPeer.CreatePeerForElement(control);
        Assert.That(peer, Is.Not.Null, "Precondition: the control creates an automation peer");

        Assert.That(peer.GetName(), Is.EqualTo("Bold"), "Without a header the ScreenTip title must be the name");
        Assert.That(peer.GetHelpText(), Is.EqualTo("Makes the text bold"), "The ScreenTip text stays the help text");
    }

    [Test]
    [TestCaseSource(nameof(ControlTypes))]
    public void Control_with_a_Header_that_is_not_text_is_named_by_its_ScreenTip_title(Type controlType)
    {
        var control = (FrameworkElement)Activator.CreateInstance(controlType)!;
        ((IHeaderedControl)control).Header = new System.Windows.Shapes.Rectangle();
        control.ToolTip = new ScreenTip { Title = "Bold" };

        var peer = UIElementAutomationPeer.CreatePeerForElement(control);

        Assert.That(peer.GetName(), Is.EqualTo("Bold"), "A header that is not text can't be announced, so the ScreenTip title must be the name");
    }

    // Guard: a text header stays the name.
    [Test]
    [TestCaseSource(nameof(ControlTypes))]
    public void Header_text_wins_over_the_ScreenTip_title(Type controlType)
    {
        var control = (FrameworkElement)Activator.CreateInstance(controlType)!;
        ((IHeaderedControl)control).Header = "Header";
        control.ToolTip = new ScreenTip { Title = "Title" };

        var peer = UIElementAutomationPeer.CreatePeerForElement(control);

        Assert.That(peer.GetName(), Is.EqualTo("Header"));
    }

    // Guard: an explicit name from the app stays the name.
    [Test]
    [TestCaseSource(nameof(ControlTypes))]
    public void Explicit_AutomationProperties_Name_wins_over_the_ScreenTip_title(Type controlType)
    {
        var control = (FrameworkElement)Activator.CreateInstance(controlType)!;
        AutomationProperties.SetName(control, "Explicit");
        control.ToolTip = new ScreenTip { Title = "Title" };

        var peer = UIElementAutomationPeer.CreatePeerForElement(control);

        Assert.That(peer.GetName(), Is.EqualTo("Explicit"));
    }

    // Guard: a ScreenTip without a title, or a plain text ToolTip (which already is the help text), adds no name.
    [Test]
    [TestCaseSource(nameof(ControlTypes))]
    public void ScreenTip_without_title_and_plain_ToolTip_add_no_name(Type controlType)
    {
        var withEmptyTitle = (FrameworkElement)Activator.CreateInstance(controlType)!;
        withEmptyTitle.ToolTip = new ScreenTip { Text = "Only text" };

        var withPlainToolTip = (FrameworkElement)Activator.CreateInstance(controlType)!;
        withPlainToolTip.ToolTip = "Plain";

        Assert.That(UIElementAutomationPeer.CreatePeerForElement(withEmptyTitle).GetName(), Is.Null.Or.Empty);
        Assert.That(UIElementAutomationPeer.CreatePeerForElement(withPlainToolTip).GetName(), Is.Null.Or.Empty);
    }

    [Test]
    public void Name_follows_a_changed_ScreenTip_title()
    {
        var screenTip = new ScreenTip { Title = "Bold" };
        var button = new Button { ToolTip = screenTip };
        var peer = UIElementAutomationPeer.CreatePeerForElement(button);

        Assert.That(peer.GetName(), Is.EqualTo("Bold"));

        screenTip.Title = "Fett";

        Assert.That(peer.GetName(), Is.EqualTo("Fett"), "The name must follow the title (for example after a language change)");
    }

    [Test]
    public void Group_launcher_button_is_named_by_the_title_of_its_ScreenTip()
    {
        var groupBox = new RibbonGroupBox
        {
            Header = "Font",
            IsLauncherVisible = true,
            LauncherToolTip = new ScreenTip { Title = "Font settings", Text = "Shows all font options" }
        };

        using (new TestRibbonWindow(groupBox))
        {
            UIHelper.DoEvents();

            Assert.That(groupBox.LauncherButton, Is.Not.Null, "Precondition: the group has its launcher button");
            Assert.That(groupBox.LauncherButton.ToolTip, Is.InstanceOf<ScreenTip>(), "Precondition: the launcher button shows the ScreenTip");

            var peer = UIElementAutomationPeer.CreatePeerForElement(groupBox.LauncherButton);

            Assert.That(peer.GetName(), Is.EqualTo("Font settings"), "Without LauncherText the ScreenTip title must be the launcher's name");
        }
    }

    // Guard: LauncherText stays the launcher's name.
    [Test]
    public void Group_launcher_button_keeps_LauncherText_as_its_name()
    {
        var groupBox = new RibbonGroupBox
        {
            Header = "Font",
            IsLauncherVisible = true,
            LauncherText = "Font dialog",
            LauncherToolTip = new ScreenTip { Title = "Font settings" }
        };

        using (new TestRibbonWindow(groupBox))
        {
            UIHelper.DoEvents();

            Assert.That(groupBox.LauncherButton, Is.Not.Null, "Precondition: the group has its launcher button");

            var peer = UIElementAutomationPeer.CreatePeerForElement(groupBox.LauncherButton);

            Assert.That(peer.GetName(), Is.EqualTo("Font dialog"));
        }
    }
}
