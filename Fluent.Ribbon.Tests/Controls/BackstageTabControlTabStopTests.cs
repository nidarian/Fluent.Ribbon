namespace Fluent.Tests.Controls;

using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// The backstage tab control is only a container: its tab items and the selected content are the real tab stops.
/// As a focusable Selector with an empty focus visual it used to be an extra, invisible stop when tabbing.
/// </summary>
[TestFixture]
public class BackstageTabControlTabStopTests
{
    [Test]
    public void BackstageTabControl_is_not_a_tab_stop()
    {
        var tabControl = new BackstageTabControl
        {
            Items =
            {
                new BackstageTabItem { Header = "Tab" }
            }
        };

        using (new TestRibbonWindow(tabControl))
        {
            UIHelper.DoEvents();

            Assert.That(tabControl.Style, Is.Not.Null, "precondition: the theme style should be applied");
            Assert.That(tabControl.IsTabStop, Is.False, "the tab control itself should not be a tab stop");
        }
    }

    [Test]
    public void StartScreenTabControl_is_not_a_tab_stop()
    {
        // StartScreenTabControl's style is based on the BackstageTabControl style.
        var tabControl = new StartScreenTabControl
        {
            Items =
            {
                new BackstageTabItem { Header = "Tab" }
            }
        };

        using (new TestRibbonWindow(tabControl))
        {
            UIHelper.DoEvents();

            Assert.That(tabControl.Style, Is.Not.Null, "precondition: the theme style should be applied");
            Assert.That(tabControl.IsTabStop, Is.False, "the tab control itself should not be a tab stop");
        }
    }
}
