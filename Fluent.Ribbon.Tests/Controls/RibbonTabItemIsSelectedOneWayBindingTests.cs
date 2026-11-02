namespace Fluent.Tests.Controls;

using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// An app can bind a tab's IsSelected OneWay (for example to show a tab while a document type is open).
/// The ribbon selected tabs through the CLR setter (SetValue) on several paths, which replaced that binding with a local value,
/// so the app's view model no longer controlled the tab. Found by a bug hunt after round 7; widened after a plan review in round 11.
/// A OneWay source doesn't change when the user selects the tab, so these tests check that the binding survives.
/// </summary>
[TestFixture]
public class RibbonTabItemIsSelectedOneWayBindingTests
{
    [Test]
    public void End_key_keeps_a_OneWay_binding_on_the_tab_it_selects()
    {
        AssertBindingSurvives((ribbon, tabs) =>
        {
            var source = PresentationSource.FromVisual(ribbon.TabControl!);
            Assert.That(source, Is.Not.Null, "Precondition: the tab control is shown");
            ribbon.TabControl!.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source!, 0, Key.End) { RoutedEvent = Keyboard.KeyDownEvent });
        });
    }

    [Test]
    public void Mouse_wheel_keeps_a_OneWay_binding_on_the_tab_it_selects()
    {
        AssertBindingSurvives((ribbon, tabs) =>
        {
            // Two notches down: from the first tab past the second to the third.
            ribbon.TabControl!.ProcessMouseWheel(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, -120) { RoutedEvent = Mouse.MouseWheelEvent });
            UIHelper.DoEvents();
            ribbon.TabControl!.ProcessMouseWheel(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, -120) { RoutedEvent = Mouse.MouseWheelEvent });
        });
    }

    [Test]
    public void Click_on_a_tab_keeps_a_OneWay_binding_on_it()
    {
        AssertBindingSurvives((ribbon, tabs) =>
        {
            tabs[2].RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = UIElement.MouseLeftButtonDownEvent });
        });
    }

    [Test]
    public void Click_on_a_contextual_group_header_keeps_a_OneWay_binding_on_the_tab_it_selects()
    {
        var group = new RibbonContextualTabGroup { Header = "Tools", Visibility = Visibility.Visible };

        AssertBindingSurvives(
            (ribbon, tabs) =>
            {
                var up = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.MouseUpEvent };
                typeof(MouseButtonEventArgs).GetProperty(nameof(MouseButtonEventArgs.ClickCount))!.SetValue(up, 1);
                group.RaiseEvent(up);
                Mouse.Capture(null);
            },
            group);
    }

    // Three tabs; the third one's IsSelected is bound OneWay to a view model that says "not selected".
    // The action selects the third tab; afterwards the binding must still exist and still control the tab.
    private static void AssertBindingSurvives(System.Action<Ribbon, RibbonTabItem[]> selectThirdTab, RibbonContextualTabGroup group = null)
    {
        var tabs = new[]
        {
            new RibbonTabItem { Header = "First" },
            new RibbonTabItem { Header = "Second" },
            new RibbonTabItem { Header = "Third", Group = group }
        };

        var ribbon = new Ribbon { AutomaticStateManagement = false };

        if (group is not null)
        {
            ribbon.ContextualGroups.Add(group);
        }

        foreach (var tab in tabs)
        {
            ribbon.Tabs.Add(tab);
        }

        var model = new TabModel();
        tabs[2].SetBinding(RibbonTabItem.IsSelectedProperty, new Binding(nameof(TabModel.IsSelected)) { Source = model, Mode = BindingMode.OneWay });

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();
            UIHelper.DoEvents();

            ribbon.SelectedTabItem = tabs[0];
            ribbon.IsMinimized = false;
            UIHelper.DoEvents();
            Assert.That(ribbon.SelectedTabItem, Is.SameAs(tabs[0]), "Precondition: the first tab is selected");

            selectThirdTab(ribbon, tabs);
            UIHelper.DoEvents();

            Assert.That(ribbon.SelectedTabItem, Is.SameAs(tabs[2]), "Precondition: the action selects the third tab");
            Assert.That(BindingOperations.GetBindingExpression(tabs[2], RibbonTabItem.IsSelectedProperty), Is.Not.Null, "The app's OneWay binding on the tab's IsSelected was lost");

            // The view model still says false (OneWay never wrote back); select the first tab, then let the app select the third.
            ribbon.SelectedTabItem = tabs[0];
            UIHelper.DoEvents();
            model.IsSelected = true;
            UIHelper.DoEvents();

            Assert.That(ribbon.SelectedTabItem, Is.SameAs(tabs[2]), "The app must still be able to select the tab");
        }
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
