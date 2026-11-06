namespace Fluent.Tests.Automation.Peers;

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Data;
using Fluent.Automation.Peers;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// On a minimized ribbon a screen reader opens a tab through the ExpandCollapse pattern of the tab's
/// <see cref="RibbonTabItemDataAutomationPeer"/>, which implements <see cref="IExpandCollapseProvider"/>.
/// The pattern must be reachable through GetPattern, and expanding must not replace an app's OneWay binding on the tab's IsSelected.
/// </summary>
[TestFixture]
public class RibbonTabItemDataAutomationPeerExpandTests
{
    [Test]
    public void ExpandCollapse_pattern_is_reachable_on_a_minimized_ribbon()
    {
        var (ribbon, tabs, _) = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            var dataPeer = PrepareMinimized(ribbon, tabs[1]);

            Assert.That(dataPeer.GetPattern(PatternInterface.ExpandCollapse), Is.SameAs(dataPeer), "A screen reader must be able to get the ExpandCollapse pattern of a tab on a minimized ribbon");
        }
    }

    // Default: on a normal ribbon a tab isn't expanded or collapsed, so the pattern isn't offered.
    [Test]
    public void ExpandCollapse_pattern_is_not_offered_on_a_normal_ribbon()
    {
        var (ribbon, tabs, _) = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            var dataPeer = PrepareMinimized(ribbon, tabs[1]);
            ribbon.IsMinimized = false;
            UIHelper.DoEvents();
            Assert.That(ribbon.TabControl!.IsMinimized, Is.False, "Precondition: the tab control is no longer minimized");

            Assert.That(dataPeer.GetPattern(PatternInterface.ExpandCollapse), Is.Null, "A tab on a normal ribbon must not offer ExpandCollapse");
        }
    }

    [Test]
    public void Expand_on_a_minimized_ribbon_keeps_the_app_binding_on_IsSelected()
    {
        var (ribbon, tabs, models) = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            var dataPeer = PrepareMinimized(ribbon, tabs[1]);

            ((IExpandCollapseProvider)dataPeer).Expand();
            UIHelper.DoEvents();

            Assert.That(tabs[1].IsSelected, Is.True, "Precondition: expanding selects the tab");
            Assert.That(ribbon.TabControl!.IsDropDownOpen, Is.True, "Precondition: expanding opens the minimized ribbon's popup");
            Assert.That(BindingOperations.GetBindingExpression(tabs[1], RibbonTabItem.IsSelectedProperty), Is.Not.Null, "The app's OneWay binding on the tab's IsSelected was lost");

            // The view model still says false (OneWay never wrote back); select the first tab, then let the app select the second.
            ((IExpandCollapseProvider)dataPeer).Collapse();
            ribbon.SelectedTabItem = tabs[0];
            UIHelper.DoEvents();
            models[1].IsSelected = true;
            UIHelper.DoEvents();

            Assert.That(tabs[1].IsSelected, Is.True, "The app must still be able to select the tab");
        }
    }

    // UI Automation: a disabled element must refuse actions, like the other Fluent.Ribbon peers.
    [Test]
    public void Expand_and_Collapse_on_a_disabled_tab_throw_ElementNotEnabledException()
    {
        var (ribbon, tabs, _) = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            var dataPeer = PrepareMinimized(ribbon, tabs[1]);
            tabs[1].IsEnabled = false;
            UIHelper.DoEvents();

            Assert.That(() => ((IExpandCollapseProvider)dataPeer).Expand(), Throws.TypeOf<ElementNotEnabledException>(), "Expand on a disabled tab must refuse");
            Assert.That(tabs[1].IsSelected, Is.False, "A disabled tab must not be selected by Expand");
            Assert.That(() => ((IExpandCollapseProvider)dataPeer).Collapse(), Throws.TypeOf<ElementNotEnabledException>(), "Collapse on a disabled tab must refuse");
        }
    }

    // A tab the ribbon itself wouldn't select (hidden, or contextual with its group hidden) must not be opened by UI Automation.
    [Test]
    public void Hidden_tab_on_a_minimized_ribbon_neither_offers_nor_acts_on_Expand()
    {
        var (ribbon, tabs, _) = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            var dataPeer = PrepareMinimized(ribbon, tabs[1]);
            tabs[1].Visibility = Visibility.Collapsed;
            UIHelper.DoEvents();

            AssertNotOpenable(ribbon, tabs[1], dataPeer);
        }
    }

    [Test]
    public void Contextual_tab_of_a_hidden_group_on_a_minimized_ribbon_neither_offers_nor_acts_on_Expand()
    {
        var group = new RibbonContextualTabGroup { Header = "Table Tools", Visibility = Visibility.Collapsed };
        var (ribbon, tabs, _) = CreateRibbon(group);

        using (new TestRibbonWindow(ribbon))
        {
            var dataPeer = PrepareMinimized(ribbon, tabs[1]);
            Assert.That(tabs[1].IsContextual, Is.True, "Precondition: the second tab is contextual");

            AssertNotOpenable(ribbon, tabs[1], dataPeer);
        }
    }

    // A collapsed ribbon (narrow window, or set by the app) hides its whole tab control, while each tab's own Visibility stays Visible.
    [Test]
    public void Tab_of_a_collapsed_ribbon_neither_offers_nor_acts_on_Expand()
    {
        var (ribbon, tabs, _) = CreateRibbon();
        ribbon.IsAutomaticCollapseEnabled = false;

        using (new TestRibbonWindow(ribbon))
        {
            var dataPeer = PrepareMinimized(ribbon, tabs[1]);
            ribbon.IsCollapsed = true;
            UIHelper.DoEvents();
            Assert.That(tabs[1].Visibility, Is.EqualTo(Visibility.Visible), "Precondition: the tab's own Visibility stays Visible");
            Assert.That(tabs[1].IsVisible, Is.False, "Precondition: the collapsed ribbon hides the tab");

            AssertNotOpenable(ribbon, tabs[1], dataPeer);
        }
    }

    // Collapse on a tab whose popup isn't the open one must not close the popup of another tab.
    [Test]
    public void Collapse_on_a_tab_that_is_not_open_leaves_the_open_tab_alone()
    {
        var (ribbon, tabs, _) = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            var secondPeer = PrepareMinimized(ribbon, tabs[1]);
            var firstPeer = GetDataPeer(ribbon, tabs[0]);

            ((IExpandCollapseProvider)secondPeer).Expand();
            UIHelper.DoEvents();
            Assert.That(ribbon.TabControl!.IsDropDownOpen, Is.True, "Precondition: the second tab is open");
            Assert.That(((IExpandCollapseProvider)firstPeer).ExpandCollapseState, Is.EqualTo(ExpandCollapseState.Collapsed), "Precondition: the first tab reports collapsed");

            ((IExpandCollapseProvider)firstPeer).Collapse();
            UIHelper.DoEvents();

            Assert.That(ribbon.TabControl.IsDropDownOpen, Is.True, "Collapse on a collapsed tab must not close another tab's popup");
            Assert.That(tabs[1].IsSelected, Is.True, "The open tab must stay selected");
        }
    }

    private static void AssertNotOpenable(Ribbon ribbon, RibbonTabItem tab, RibbonTabItemDataAutomationPeer dataPeer)
    {
        Assert.That(dataPeer.GetPattern(PatternInterface.ExpandCollapse), Is.Null, "A tab the user can't see must not offer ExpandCollapse");

        // A client could still hold the pattern from before the tab was hidden.
        ((IExpandCollapseProvider)dataPeer).Expand();
        UIHelper.DoEvents();

        Assert.That(tab.IsSelected, Is.False, "Expand must not select a tab the user can't see");
        Assert.That(ribbon.TabControl!.IsDropDownOpen, Is.False, "Expand must not open the popup for a tab the user can't see");
    }

    // Both tabs' IsSelected are bound OneWay to their own view model (both start "not selected").
    private static (Ribbon Ribbon, RibbonTabItem[] Tabs, TabModel[] Models) CreateRibbon(RibbonContextualTabGroup groupOfSecondTab = null)
    {
        var ribbon = new Ribbon { CanMinimize = true, AutomaticStateManagement = false };
        var tabs = new[] { new RibbonTabItem { Header = "Home" }, new RibbonTabItem { Header = "Insert", Group = groupOfSecondTab } };

        if (groupOfSecondTab is not null)
        {
            ribbon.ContextualGroups.Add(groupOfSecondTab);
        }

        var models = new[] { new TabModel(), new TabModel() };

        for (var i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetBinding(RibbonTabItem.IsSelectedProperty, new Binding(nameof(TabModel.IsSelected)) { Source = models[i], Mode = BindingMode.OneWay });
            ribbon.Tabs.Add(tabs[i]);
        }

        return (ribbon, tabs, models);
    }

    private static RibbonTabItemDataAutomationPeer PrepareMinimized(Ribbon ribbon, RibbonTabItem tab)
    {
        ribbon.ApplyTemplate();
        UIHelper.DoEvents();
        ribbon.IsMinimized = true;
        UIHelper.DoEvents();

        Assert.That(ribbon.TabControl, Is.Not.Null, "Precondition: the ribbon has its tab control");
        Assert.That(ribbon.TabControl.IsMinimized, Is.True, "Precondition: the tab control is minimized");
        Assert.That(tab.IsSelected, Is.False, "Precondition: the tab is not selected yet");

        return GetDataPeer(ribbon, tab);
    }

    private static RibbonTabItemDataAutomationPeer GetDataPeer(Ribbon ribbon, RibbonTabItem tab)
    {
        var tabControlPeer = UIElementAutomationPeer.CreatePeerForElement(ribbon.TabControl);
        var dataPeer = tabControlPeer.GetChildren().OfType<RibbonTabItemDataAutomationPeer>().FirstOrDefault(x => ReferenceEquals(x.Item, tab));
        Assert.That(dataPeer, Is.Not.Null, "Precondition: the tab control peer exposes a data peer for the tab");

        return dataPeer;
    }

    private sealed class TabModel : INotifyPropertyChanged
    {
        private bool isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsSelected)));
            }
        }
    }
}
