namespace Fluent.Tests.Automation.Peers;

using System.ComponentModel;
using System.Linq;
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

    // Default (noted in ROUND12.md): on a normal ribbon a tab isn't expanded or collapsed, so the pattern isn't offered.
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

    // Both tabs' IsSelected are bound OneWay to their own view model (both start "not selected").
    private static (Ribbon Ribbon, RibbonTabItem[] Tabs, TabModel[] Models) CreateRibbon()
    {
        var ribbon = new Ribbon { CanMinimize = true, AutomaticStateManagement = false };
        var tabs = new[] { new RibbonTabItem { Header = "Home" }, new RibbonTabItem { Header = "Insert" } };
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
