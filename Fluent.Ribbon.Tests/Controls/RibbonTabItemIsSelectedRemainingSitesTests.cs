namespace Fluent.Tests.Controls;

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Round 11 kept an app's explicit OneWay binding on a tab's IsSelected for the keyboard, the mouse wheel and the contextual
/// header (each proven by a failing test). Three more places select or deselect a tab with the CLR setter; these tests check
/// whether they also replace such a binding, so they are only fixed if they do.
/// </summary>
[TestFixture]
public class RibbonTabItemIsSelectedRemainingSitesTests
{
    [Test]
    public void Removing_the_selected_tab_keeps_the_binding_on_the_tab_selected_instead()
    {
        var (ribbon, tabs, models) = CreateRibbon(3);

        using (new TestRibbonWindow(ribbon))
        {
            Prepare(ribbon);
            ribbon.SelectedTabItem = tabs[1];
            UIHelper.DoEvents();
            Assert.That(ribbon.SelectedTabItem, Is.SameAs(tabs[1]), "Precondition: the middle tab is selected");

            ribbon.Tabs.Remove(tabs[1]);
            UIHelper.DoEvents();

            var selected = ribbon.SelectedTabItem;
            Assert.That(selected, Is.Not.Null.And.Not.SameAs(tabs[1]), "Precondition: another tab is selected after the removal");
            Assert.That(BindingOperations.GetBindingExpression(selected!, RibbonTabItem.IsSelectedProperty), Is.Not.Null, "The app's OneWay binding on the newly selected tab was lost");
        }
    }

    [Test]
    public void Restoring_a_minimized_ribbon_without_a_selection_keeps_the_binding_on_the_tab_it_selects()
    {
        var (ribbon, tabs, models) = CreateRibbon(2);

        using (new TestRibbonWindow(ribbon))
        {
            Prepare(ribbon);

            ribbon.IsMinimized = true;
            UIHelper.DoEvents();
            ribbon.TabControl!.SelectedItem = null;
            UIHelper.DoEvents();
            Assert.That(ribbon.TabControl.SelectedIndex, Is.EqualTo(-1), "Precondition: no tab is selected while minimized");

            ribbon.IsMinimized = false;
            UIHelper.DoEvents();

            Assert.That(ribbon.SelectedTabItem, Is.SameAs(tabs[0]), "Precondition: restoring selects the first tab");
            Assert.That(BindingOperations.GetBindingExpression(tabs[0], RibbonTabItem.IsSelectedProperty), Is.Not.Null, "The app's OneWay binding on the first tab was lost");
        }
    }

    [Test]
    public void Hiding_the_selected_tab_while_minimized_keeps_its_binding()
    {
        var (ribbon, tabs, models) = CreateRibbon(2);

        using (new TestRibbonWindow(ribbon))
        {
            Prepare(ribbon);
            ribbon.SelectedTabItem = tabs[1];
            UIHelper.DoEvents();

            ribbon.IsMinimized = true;
            UIHelper.DoEvents();
            Assert.That(tabs[1].IsSelected, Is.True, "Precondition: the second tab is selected while minimized");

            tabs[1].Visibility = Visibility.Collapsed;
            UIHelper.DoEvents();

            Assert.That(tabs[1].IsSelected, Is.False, "Precondition: hiding the selected tab deselects it");
            Assert.That(BindingOperations.GetBindingExpression(tabs[1], RibbonTabItem.IsSelectedProperty), Is.Not.Null, "The app's OneWay binding on the hidden tab was lost");
        }
    }

    // Every tab's IsSelected is bound OneWay to its own view model (all start "not selected").
    private static (Ribbon Ribbon, RibbonTabItem[] Tabs, TabModel[] Models) CreateRibbon(int count)
    {
        var ribbon = new Ribbon { CanMinimize = true, AutomaticStateManagement = false };
        var tabs = Enumerable.Range(0, count).Select(i => new RibbonTabItem { Header = "Tab " + i }).ToArray();
        var models = tabs.Select(_ => new TabModel()).ToArray();

        for (var i = 0; i < count; i++)
        {
            tabs[i].SetBinding(RibbonTabItem.IsSelectedProperty, new Binding(nameof(TabModel.IsSelected)) { Source = models[i], Mode = BindingMode.OneWay });
            ribbon.Tabs.Add(tabs[i]);
        }

        return (ribbon, tabs, models);
    }

    private static void Prepare(Ribbon ribbon)
    {
        ribbon.ApplyTemplate();
        UIHelper.DoEvents();
        ribbon.IsMinimized = false;
        UIHelper.DoEvents();
        Assert.That(ribbon.TabControl, Is.Not.Null, "Precondition: the ribbon has its tab control");
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
