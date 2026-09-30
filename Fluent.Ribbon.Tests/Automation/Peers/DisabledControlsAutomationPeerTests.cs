namespace Fluent.Tests.Automation.Peers;

using System.Linq;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Fluent.Automation.Peers;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// UI Automation says a disabled element must refuse actions with ElementNotEnabledException.
/// InRibbonGallery's peer already does (see RibbonInRibbonGalleryAutomationPeerTests). These peers didn't,
/// so a screen reader or UI test could open a disabled drop down or backstage, or click a
/// SplitButton's disabled button part.
/// </summary>
[TestFixture]
public class DisabledControlsAutomationPeerTests
{
    [Test]
    public void DropDownButton_ExpandCollapse_throws_when_disabled()
    {
        var control = new DropDownButton { Header = "DropDown", IsEnabled = false };

        var expandCollapse = UIElementAutomationPeer.CreatePeerForElement(control).GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
        Assert.That(expandCollapse, Is.Not.Null, "Precondition");

        Assert.That(() => expandCollapse.Expand(), Throws.InstanceOf<ElementNotEnabledException>());
        Assert.That(control.IsDropDownOpen, Is.False, "A disabled drop down must not open");
        Assert.That(() => expandCollapse.Collapse(), Throws.InstanceOf<ElementNotEnabledException>());
    }

    [Test]
    public void Backstage_ExpandCollapse_throws_when_disabled()
    {
        var control = new Backstage { IsEnabled = false };

        var expandCollapse = UIElementAutomationPeer.CreatePeerForElement(control).GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
        Assert.That(expandCollapse, Is.Not.Null, "Precondition");

        Assert.That(() => expandCollapse.Expand(), Throws.InstanceOf<ElementNotEnabledException>());
        Assert.That(control.IsOpen, Is.False, "A disabled backstage must not open");
        Assert.That(() => expandCollapse.Collapse(), Throws.InstanceOf<ElementNotEnabledException>());
    }

    [Test]
    public void SplitButton_Invoke_throws_when_the_button_part_is_disabled()
    {
        var clicks = 0;
        var splitButton = new SplitButton { Header = "Split", IsButtonEnabled = false, Items = { new MenuItem { Header = "Item" } } };
        splitButton.Click += (_, _) => clicks++;

        using (new TestRibbonWindow(splitButton))
        {
            splitButton.ApplyTemplate();
            UIHelper.DoEvents();
            Assert.That(splitButton.Button?.IsEnabled, Is.False, "Precondition: the button part is disabled");

            var invoke = UIElementAutomationPeer.CreatePeerForElement(splitButton).GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            Assert.That(invoke, Is.Not.Null, "Precondition");

            Assert.That(() => invoke.Invoke(), Throws.InstanceOf<ElementNotEnabledException>());
            UIHelper.DoEvents();
            Assert.That(clicks, Is.EqualTo(0), "A disabled button part must not be clicked");
        }
    }
}
