namespace Fluent.Tests.Localization;

using System.Globalization;
using System.Windows.Controls;
using NUnit.Framework;

[TestFixture]
public class LocalizationSwitchTests
{
    [Test]
    public void StatusBar_Customize_Menu_Header_Should_Follow_Runtime_Culture_Switch()
    {
        var originalCulture = RibbonLocalization.Current.Culture;

        try
        {
            RibbonLocalization.Current.Culture = new CultureInfo("en-US");
            var englishHeader = RibbonLocalization.Current.Localization.CustomizeStatusBar;

            var statusBar = new StatusBar();
            var headerItem = (HeaderedItemsControl)statusBar.ContextMenu.Items[0];

            Assert.That(headerItem.Header, Is.EqualTo(englishHeader), "Precondition: header should be in english initially.");

            // Switching the culture replaces RibbonLocalization.Current.Localization with a new object.
            RibbonLocalization.Current.Culture = new CultureInfo("de-DE");
            var germanHeader = RibbonLocalization.Current.Localization.CustomizeStatusBar;

            Assert.That(germanHeader, Is.Not.EqualTo(englishHeader), "Precondition: german and english texts should differ.");

            Assert.That(headerItem.Header, Is.EqualTo(germanHeader));
        }
        finally
        {
            RibbonLocalization.Current.Culture = originalCulture;
        }
    }
}
