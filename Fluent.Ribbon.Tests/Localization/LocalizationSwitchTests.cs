namespace Fluent.Tests.Localization;

using System.Globalization;
using System.Windows.Controls;
using Fluent.Tests.TestClasses;
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

    // The ApplicationMenu takes its default KeyTip (no KeyTip set by the app) from the localization.
    // It must follow a runtime language switch, like the Backstage button and the menu headers do.
    [Test]
    public void ApplicationMenu_Default_KeyTip_Should_Follow_Runtime_Culture_Switch()
    {
        var originalCulture = RibbonLocalization.Current.Culture;

        try
        {
            RibbonLocalization.Current.Culture = new CultureInfo("en-US");
            var englishKeyTip = RibbonLocalization.Current.Localization.BackstageButtonKeyTip;

            var applicationMenu = new ApplicationMenu();

            using (new TestRibbonWindow(applicationMenu))
            {
                Assert.That(applicationMenu.KeyTip, Is.EqualTo(englishKeyTip), "Precondition: KeyTip should be the english one initially.");

                RibbonLocalization.Current.Culture = new CultureInfo("de-DE");
                var germanKeyTip = RibbonLocalization.Current.Localization.BackstageButtonKeyTip;

                Assert.That(germanKeyTip, Is.Not.EqualTo(englishKeyTip), "Precondition: german and english KeyTips should differ.");

                Assert.That(applicationMenu.KeyTip, Is.EqualTo(germanKeyTip));
            }
        }
        finally
        {
            RibbonLocalization.Current.Culture = originalCulture;
        }
    }

    // Guard: a KeyTip set by the app must never be replaced by the localized default.
    [Test]
    public void ApplicationMenu_KeyTip_Set_By_App_Should_Be_Kept_On_Runtime_Culture_Switch()
    {
        var originalCulture = RibbonLocalization.Current.Culture;

        try
        {
            RibbonLocalization.Current.Culture = new CultureInfo("en-US");

            var applicationMenu = new ApplicationMenu { KeyTip = "Q" };

            using (new TestRibbonWindow(applicationMenu))
            {
                Assert.That(applicationMenu.KeyTip, Is.EqualTo("Q"), "Precondition: KeyTip set by the app should be used.");

                RibbonLocalization.Current.Culture = new CultureInfo("de-DE");

                Assert.That(applicationMenu.KeyTip, Is.EqualTo("Q"));
            }
        }
        finally
        {
            RibbonLocalization.Current.Culture = originalCulture;
        }
    }

    // The Backstage button gets its default KeyTip through a binding in its style, so it already follows a switch.
    // This test documents that and protects it.
    [Test]
    public void Backstage_Default_KeyTip_Should_Follow_Runtime_Culture_Switch()
    {
        var originalCulture = RibbonLocalization.Current.Culture;

        try
        {
            RibbonLocalization.Current.Culture = new CultureInfo("en-US");
            var englishKeyTip = RibbonLocalization.Current.Localization.BackstageButtonKeyTip;

            var backstage = new Backstage();

            using (new TestRibbonWindow(backstage))
            {
                Assert.That(backstage.KeyTip, Is.EqualTo(englishKeyTip), "Precondition: KeyTip should be the english one initially.");

                RibbonLocalization.Current.Culture = new CultureInfo("de-DE");
                var germanKeyTip = RibbonLocalization.Current.Localization.BackstageButtonKeyTip;

                Assert.That(germanKeyTip, Is.Not.EqualTo(englishKeyTip), "Precondition: german and english KeyTips should differ.");

                Assert.That(backstage.KeyTip, Is.EqualTo(germanKeyTip));
            }
        }
        finally
        {
            RibbonLocalization.Current.Culture = originalCulture;
        }
    }
}
