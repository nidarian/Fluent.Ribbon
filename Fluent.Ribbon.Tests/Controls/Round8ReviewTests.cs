namespace Fluent.Tests.Controls;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Findings of the two independent reviews of round 8 (pessimistic and adversarial), each checked here before it was fixed.
/// </summary>
[TestFixture]
public class Round8ReviewTests
{
    /// <summary>
    /// UI Automation Toggle on a checkable SplitButton must do what a click on its button does.
    /// It flipped IsChecked even while the button part was disabled (IsButtonEnabled = false or a command that can't execute),
    /// which a mouse user can't do, and the TwoWay binding passed that on to the app.
    /// </summary>
    [Test]
    public void Toggle_through_UI_Automation_respects_a_disabled_button_part()
    {
        var splitButton = new SplitButton { Header = "Bold", IsCheckable = true, IsButtonEnabled = false };

        using (new TestRibbonWindow(splitButton))
        {
            UIHelper.DoEvents();

            var toggle = UIElementAutomationPeer.CreatePeerForElement(splitButton).GetPattern(PatternInterface.Toggle) as IToggleProvider;
            Assert.That(toggle, Is.Not.Null, "Precondition: a checkable split button exposes the Toggle pattern");

            Assert.That(() => toggle!.Toggle(), Throws.TypeOf<ElementNotEnabledException>(), "Toggle must refuse like Invoke does");
            UIHelper.DoEvents();

            Assert.That(splitButton.IsChecked, Is.False, "A disabled button part must not be toggled");
        }
    }

    /// <summary>
    /// The Spinner's text box was named by binding to Header, which is an object: a header that isn't text was announced
    /// with its type name ("System.Windows.Controls.TextBlock"), and an AutomationProperties.Name the app set was ignored.
    /// </summary>
    [Test]
    public void Spinner_text_box_takes_the_spinners_automation_name()
    {
        var named = new Spinner { Header = "Zoom" };
        AutomationProperties.SetName(named, "Page zoom");
        var elementHeader = new Spinner { Header = new TextBlock { Text = "Width" } };

        using (new TestRibbonWindow(new StackPanel { Children = { named, elementHeader } }))
        {
            UIHelper.DoEvents();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(TextBoxName(named), Is.EqualTo("Page zoom"), "The app's name for the spinner must be used for its text box");
                Assert.That(TextBoxName(elementHeader), Does.Not.Contain("System."), "A header that isn't text must not be announced by its type name");
            }
        }
    }

    /// <summary>
    /// Replacing all tabs while the ribbon is not loaded (in a hidden page or a view swapped out) and loading it again
    /// showed no tab: the first-tab selection only ran while the ribbon was loaded.
    /// </summary>
    [Test]
    public void Tabs_replaced_while_the_ribbon_is_unloaded_select_a_tab_when_it_comes_back()
    {
        var ribbon = new Ribbon { AutomaticStateManagement = false };
        ribbon.Tabs.Add(new RibbonTabItem { Header = "A" });

        using (var window = new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();
            Assert.That(ribbon.SelectedTabItem, Is.Not.Null, "Precondition: a tab is selected");

            window.Content = null;
            UIHelper.DoEvents();
            Assert.That(ribbon.IsLoaded, Is.False, "Precondition: the ribbon is unloaded");

            ribbon.Tabs.Clear();
            var c = new RibbonTabItem { Header = "C" };
            ribbon.Tabs.Add(c);

            window.Content = ribbon;
            UIHelper.DoEvents();

            Assert.That(ribbon.SelectedTabItem, Is.SameAs(c), "The first new tab must be selected when the ribbon is shown again");
        }
    }

    /// <summary>
    /// Round 8 made toolbar children use SimplifiedSizeDefinition in the simplified ribbon. Its default (Large, Middle, Small)
    /// changed the look of existing apps whose toolbar buttons only set SizeDefinition (for example icon-only "Small"):
    /// they grew in the simplified ribbon. Only a SimplifiedSizeDefinition the app set must be used.
    /// </summary>
    [Test]
    public void Toolbar_children_without_a_SimplifiedSizeDefinition_keep_their_size_in_the_simplified_ribbon()
    {
        var button = new Button
        {
            Header = "Cut",
            SizeDefinition = new RibbonControlSizeDefinition(RibbonControlSize.Small, RibbonControlSize.Small, RibbonControlSize.Small)
        };

        var toolBar = new RibbonToolBar();
        toolBar.Children.Add(button);
        var groupBox = new RibbonGroupBox { Header = "Group", Items = { toolBar } };
        var tabItem = new RibbonTabItem { Header = "Home", Groups = { groupBox } };
        var ribbon = new Ribbon { CanUseSimplified = true, AutomaticStateManagement = false, Tabs = { tabItem } };

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.SelectedTabItem = tabItem;
            ribbon.IsSimplified = true;
            UIHelper.DoEvents();

            Assert.That(toolBar.IsSimplified, Is.True, "Precondition: the toolbar is simplified");
            Assert.That(button.Size, Is.EqualTo(RibbonControlSize.Small), "Without its own SimplifiedSizeDefinition the button keeps the size it had before round 8");
        }
    }

    private static string TextBoxName(Spinner spinner)
    {
        var textBox = spinner.Template.FindName("PART_TextBox", spinner) as System.Windows.Controls.TextBox;
        Assert.That(textBox, Is.Not.Null, "Precondition: the template has PART_TextBox");
        return UIElementAutomationPeer.CreatePeerForElement(textBox!).GetName();
    }
}
