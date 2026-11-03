namespace Fluent.Tests.Automation.Peers;

using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Found by the accessibility re-audit of round 8: the colour lists of a ColorGallery had no names
/// (a screen reader said only "list"; WCAG 1.3.1).
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
}
