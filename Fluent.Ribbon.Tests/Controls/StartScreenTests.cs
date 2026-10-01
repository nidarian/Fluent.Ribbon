namespace Fluent.Tests.Controls;

using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class StartScreenTests
{
    [Test(Description = "Closing a StartScreen that was not displayed (because it was already shown once) must not overwrite RibbonTitleBar.IsCollapsed")]
    public void TitleBar_IsCollapsed_is_only_restored_once_after_the_StartScreen_was_displayed()
    {
        var titleBar = new RibbonTitleBar();
        var startScreen = new StartScreen
        {
            Content = new StartScreenTabControl()
        };
        var ribbon = new Ribbon
        {
            TitleBar = titleBar,
            StartScreen = startScreen
        };

        using (new TestRibbonWindow(ribbon))
        {
            Assert.That(titleBar.IsCollapsed, Is.False, "Precondition: the title bar starts expanded.");

            // First open/close: the start screen is really displayed, collapses the title bar and restores it on close.
            startScreen.IsOpen = true;

            Assert.That(startScreen.Shown, Is.True, "Precondition: the first open must display the start screen.");
            Assert.That(titleBar.IsCollapsed, Is.True, "Precondition: displaying the start screen collapses the title bar.");

            startScreen.IsOpen = false;

            Assert.That(titleBar.IsCollapsed, Is.False, "Precondition: closing the start screen restores the title bar.");

            // Later the application collapses the title bar on its own.
            titleBar.IsCollapsed = true;

            // Second open/close: StartScreen.Show returns early because Shown is already true, so nothing is displayed
            // and the title bar was never touched. Closing must therefore not write an old saved value back.
            startScreen.IsOpen = true;
            startScreen.IsOpen = false;

            Assert.That(titleBar.IsCollapsed, Is.True, "Closing a start screen that was not displayed must not restore an outdated RibbonTitleBar.IsCollapsed value.");
        }
    }
}