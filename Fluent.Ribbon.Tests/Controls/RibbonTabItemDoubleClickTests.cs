namespace Fluent.Tests.Controls;

using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Double-clicking a tab minimizes or restores the ribbon. The ribbon's <see cref="RibbonTabControl"/> gets its
/// <see cref="RibbonTabControl.IsMinimized"/> from <see cref="Ribbon.IsMinimized"/> through a TwoWay binding set in its style.
/// The double-click wrote <see cref="RibbonTabControl.IsMinimized"/> with its CLR setter (SetValue). A local value replaces
/// a binding that comes from a style, so after one double-click the tabs no longer follow <see cref="Ribbon.IsMinimized"/>:
/// minimizing or restoring the ribbon from code (or from an app setting bound to it) no longer changes what the user sees.
/// </summary>
[TestFixture]
public class RibbonTabItemDoubleClickTests
{
    [Test]
    public void Double_click_on_a_tab_keeps_the_tabs_following_Ribbon_IsMinimized()
    {
        var tab = new RibbonTabItem { Header = "Home" };
        var ribbon = new Ribbon { CanMinimize = true };
        ribbon.Tabs.Add(tab);

        using (new TestRibbonWindow(ribbon))
        {
            var tabControl = Prepare(ribbon, tab);

            DoubleClick(tab);

            Assert.That(tabControl.IsMinimized, Is.True, "Precondition: the double-click must minimize the tabs.");
            Assert.That(ribbon.IsMinimized, Is.True, "The double-click must reach Ribbon.IsMinimized.");

            ribbon.IsMinimized = false;
            Assert.That(tabControl.IsMinimized, Is.False, "After the double-click the tabs must still follow Ribbon.IsMinimized (restore from code).");

            ribbon.IsMinimized = true;
            Assert.That(tabControl.IsMinimized, Is.True, "After the double-click the tabs must still follow Ribbon.IsMinimized (minimize from code).");

            DoubleClick(tab);

            Assert.That(tabControl.IsMinimized, Is.False, "Precondition: the second double-click must restore the tabs.");
            Assert.That(ribbon.IsMinimized, Is.False, "The second double-click must reach Ribbon.IsMinimized.");
        }
    }

    [Test]
    public void Double_click_on_a_tab_keeps_a_OneWay_binding_on_Ribbon_IsMinimized()
    {
        var settings = new RibbonSettings();
        var tab = new RibbonTabItem { Header = "Home" };
        var ribbon = new Ribbon { CanMinimize = true };
        ribbon.Tabs.Add(tab);
        ribbon.SetBinding(Ribbon.IsMinimizedProperty, new Binding(nameof(RibbonSettings.IsMinimized)) { Source = settings, Mode = BindingMode.OneWay });

        using (new TestRibbonWindow(ribbon))
        {
            var tabControl = Prepare(ribbon, tab);

            DoubleClick(tab);

            Assert.That(tabControl.IsMinimized, Is.True, "Precondition: the double-click must minimize the tabs.");
            Assert.That(ribbon.IsMinimized, Is.True, "The double-click must reach Ribbon.IsMinimized.");
            Assert.That(settings.IsMinimized, Is.False, "Precondition: a OneWay binding never writes back to the settings.");
            Assert.That(BindingOperations.GetBindingExpression(ribbon, Ribbon.IsMinimizedProperty), Is.Not.Null, "The app's OneWay binding on Ribbon.IsMinimized was lost.");

            // The settings still hold the old value (OneWay never wrote back), so set the changed value first to get a change notification through.
            settings.IsMinimized = true;
            settings.IsMinimized = false;
            Assert.That(ribbon.IsMinimized, Is.False, "The app's settings must still restore the ribbon.");
            Assert.That(tabControl.IsMinimized, Is.False, "The app's settings must still restore the tabs.");

            settings.IsMinimized = true;
            Assert.That(ribbon.IsMinimized, Is.True, "The app's settings must still minimize the ribbon.");
            Assert.That(tabControl.IsMinimized, Is.True, "The app's settings must still minimize the tabs.");
        }
    }

    /// <summary>
    /// Guard for apps with a TwoWay binding: the double-click must still be written back to their settings.
    /// </summary>
    [Test]
    public void Double_click_on_a_tab_reaches_a_TwoWay_binding_on_Ribbon_IsMinimized()
    {
        var settings = new RibbonSettings();
        var tab = new RibbonTabItem { Header = "Home" };
        var ribbon = new Ribbon { CanMinimize = true };
        ribbon.Tabs.Add(tab);
        ribbon.SetBinding(Ribbon.IsMinimizedProperty, new Binding(nameof(RibbonSettings.IsMinimized)) { Source = settings, Mode = BindingMode.TwoWay });

        using (new TestRibbonWindow(ribbon))
        {
            var tabControl = Prepare(ribbon, tab);

            DoubleClick(tab);

            Assert.That(tabControl.IsMinimized, Is.True, "Precondition: the double-click must minimize the tabs.");
            Assert.That(settings.IsMinimized, Is.True, "The double-click must be written back to the TwoWay source.");

            settings.IsMinimized = false;
            Assert.That(ribbon.IsMinimized, Is.False, "The app's settings must still restore the ribbon.");
            Assert.That(tabControl.IsMinimized, Is.False, "The app's settings must still restore the tabs.");

            DoubleClick(tab);

            Assert.That(settings.IsMinimized, Is.True, "The second double-click must be written back to the TwoWay source.");
            Assert.That(BindingOperations.GetBindingExpression(ribbon, Ribbon.IsMinimizedProperty), Is.Not.Null, "The app's TwoWay binding on Ribbon.IsMinimized must still exist.");
        }
    }

    private static RibbonTabControl Prepare(Ribbon ribbon, RibbonTabItem tab)
    {
        ribbon.ApplyTemplate();
        UIHelper.DoEvents();

        var tabControl = ribbon.TabControl;

        Assert.That(tabControl, Is.Not.Null, "Precondition: the ribbon template must create its RibbonTabControl.");
        Assert.That(tab.TabControlParent, Is.SameAs(tabControl), "Precondition: the tab must be hosted in the ribbon's tab control.");
        Assert.That(tabControl.CanMinimize, Is.True, "Precondition: the tab control must allow minimizing.");
        Assert.That(ribbon.IsMinimized, Is.False, "Precondition: the ribbon starts expanded.");
        Assert.That(tabControl.IsMinimized, Is.False, "Precondition: the tabs start expanded.");

        return tabControl;
    }

    // Simulates the second press of a double-click the way WPF input delivers it: MouseDown bubbles from the tab and
    // UIElement re-raises it as MouseLeftButtonDown with the same args (RibbonTabItem handles ClickCount == 2 in OnMouseLeftButtonDown).
    // ClickCount has an internal setter; WPF's input system sets it to 2 for the second press of a double-click.
    private static void DoubleClick(RibbonTabItem tab)
    {
        try
        {
            var down = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseDownEvent
            };

            typeof(MouseButtonEventArgs).GetProperty(nameof(MouseButtonEventArgs.ClickCount)).SetValue(down, 2);
            Assert.That(down.ClickCount, Is.EqualTo(2), "Precondition: the simulated press must be the second click of a double-click.");

            tab.RaiseEvent(down);
            UIHelper.DoEvents();

            Assert.That(down.Handled, Is.True, "Precondition: the tab must handle the double-click.");
        }
        finally
        {
            Mouse.Capture(null);
        }
    }

    private sealed class RibbonSettings : INotifyPropertyChanged
    {
        private bool isMinimized;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsMinimized
        {
            get => this.isMinimized;
            set
            {
                if (this.isMinimized == value)
                {
                    return;
                }

                this.isMinimized = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsMinimized)));
            }
        }
    }
}
