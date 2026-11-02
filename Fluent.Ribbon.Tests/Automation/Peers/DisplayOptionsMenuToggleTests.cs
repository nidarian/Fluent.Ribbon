namespace Fluent.Tests.Automation.Peers;

using System.Linq;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Rounds 8 and 11 bound the display options menu items OneWay and handled their Click, to keep an app's binding on
/// Ribbon.IsMinimized and Ribbon.IsSimplified. UI Automation's Toggle pattern (UI tests, some assistive technology) changes
/// IsChecked without a Click, so it then only changed the check mark, not the ribbon. Found by a review of round 11.
/// </summary>
[TestFixture]
public class DisplayOptionsMenuToggleTests
{
    [Test]
    public void Toggling_Minimize_ribbon_through_UI_Automation_minimizes_the_ribbon()
    {
        var ribbon = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            Prepare(ribbon);

            Toggle(GetMenuItem(ribbon, RibbonLocalization.Current.Localization.MinimizeRibbon));

            Assert.That(ribbon.IsMinimized, Is.True, "Toggle on \"Minimize ribbon\" must minimize the ribbon");
        }
    }

    [Test]
    public void Toggling_Use_simplified_ribbon_through_UI_Automation_switches_the_ribbon()
    {
        var ribbon = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            Prepare(ribbon);

            Toggle(GetMenuItem(ribbon, RibbonLocalization.Current.Localization.UseSimplifiedRibbon));

            Assert.That(ribbon.IsSimplified, Is.True, "Toggle on \"Use simplified ribbon\" must switch to the simplified ribbon");
        }
    }

    private static Ribbon CreateRibbon()
    {
        var ribbon = new Ribbon { CanMinimize = true, CanUseSimplified = true, AutomaticStateManagement = false };
        ribbon.Tabs.Add(new RibbonTabItem { Header = "Home" });
        return ribbon;
    }

    private static void Prepare(Ribbon ribbon)
    {
        ribbon.ApplyTemplate();
        UIHelper.DoEvents();
        ribbon.IsMinimized = false;
        ribbon.IsSimplified = false;
        UIHelper.DoEvents();
        ribbon.TabControl!.ApplyTemplate();
    }

    private static System.Windows.Controls.MenuItem GetMenuItem(Ribbon ribbon, string header)
    {
        var button = ribbon.TabControl!.Template.FindName("PART_DisplayOptionsButton", ribbon.TabControl) as DropDownButton;
        Assert.That(button, Is.Not.Null, "Precondition: the display options button exists");

        var item = button!.Items.OfType<System.Windows.Controls.MenuItem>().FirstOrDefault(x => Equals(x.Header, header));
        Assert.That(item, Is.Not.Null, $"Precondition: the menu has \"{header}\"");
        return item!;
    }

    private static void Toggle(System.Windows.Controls.MenuItem item)
    {
        var toggle = UIElementAutomationPeer.CreatePeerForElement(item).GetPattern(PatternInterface.Toggle) as IToggleProvider;
        Assert.That(toggle, Is.Not.Null, "Precondition: the checkable menu item supports the Toggle pattern");

        toggle!.Toggle();
        UIHelper.DoEvents();
    }
}
