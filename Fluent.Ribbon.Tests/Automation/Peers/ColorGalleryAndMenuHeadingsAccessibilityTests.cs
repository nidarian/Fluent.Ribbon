namespace Fluent.Tests.Automation.Peers;

using System.Linq;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Found by the accessibility re-audit of round 8:
/// the colour lists of a ColorGallery had no names (a screen reader said only "list"; WCAG 1.3.1), and the display options menu
/// used disabled menu items as headings, which screen readers announced as unavailable menu items.
/// </summary>
[TestFixture]
public class ColorGalleryAndMenuHeadingsAccessibilityTests
{
    [Test]
    [TestCase("PART_ThemeColorsListBox", "themeColorsSeparator")]
    [TestCase("PART_StandardColorsListBox", "standardColorsSeparator")]
    [TestCase("PART_RecentColorsListBox", "recentColorsSeparator")]
    public void ColorGallery_lists_are_named_after_their_heading(string listName, string headingName)
    {
        var gallery = new ColorGallery();

        using (new TestRibbonWindow(gallery))
        {
            gallery.ApplyTemplate();
            UIHelper.DoEvents();

            var list = gallery.Template.FindName(listName, gallery) as ListBox;
            var heading = gallery.Template.FindName(headingName, gallery) as System.Windows.Controls.MenuItem;
            Assert.That(list, Is.Not.Null, $"Precondition: the template has {listName}");
            Assert.That(heading, Is.Not.Null, $"Precondition: the template has {headingName}");

            var peer = UIElementAutomationPeer.CreatePeerForElement(list!);
            Assert.That(peer.GetName(), Is.EqualTo(heading!.Header as string), "The list must be announced with its heading");
        }
    }

    [Test]
    public void Display_options_menu_headings_are_not_announced_as_menu_items()
    {
        var ribbon = new Ribbon { CanMinimize = true, CanUseSimplified = true, AutomaticStateManagement = false };
        ribbon.Tabs.Add(new RibbonTabItem { Header = "Home" });

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();
            UIHelper.DoEvents();
            ribbon.TabControl!.ApplyTemplate();

            var button = ribbon.TabControl.Template.FindName("PART_DisplayOptionsButton", ribbon.TabControl) as DropDownButton;
            Assert.That(button, Is.Not.Null, "Precondition: the display options button exists");

            var headings = button!.Items.OfType<System.Windows.Controls.MenuItem>()
                .Where(x => Equals(x.Header, RibbonLocalization.Current.Localization.ShowRibbon) || Equals(x.Header, RibbonLocalization.Current.Localization.RibbonLayout))
                .ToList();
            Assert.That(headings, Has.Count.EqualTo(2), "Precondition: the menu has its two headings");

            foreach (var heading in headings)
            {
                var peer = UIElementAutomationPeer.CreatePeerForElement(heading);
                Assert.That(peer.IsControlElement(), Is.False, $"The heading \"{heading.Header}\" must not be announced as an (unavailable) menu item");
            }
        }
    }
}
