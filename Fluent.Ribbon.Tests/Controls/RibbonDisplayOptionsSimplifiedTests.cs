namespace Fluent.Tests.Controls;

using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// "Use classic ribbon" and "Use simplified ribbon" in the display options menu bound IsChecked TwoWay (the default for MenuItem.IsChecked)
/// to <see cref="RibbonTabControl.IsSimplified"/>, which the tab control's style binds TwoWay to <see cref="Ribbon.IsSimplified"/>:
/// a click wrote <see cref="Ribbon.IsSimplified"/> with SetValue and replaced an app's OneWay binding.
/// Same bug as "Minimize ribbon" (<see cref="RibbonDisplayOptionsMinimizeTests"/>); found by the plan review of round 11.
/// </summary>
[TestFixture]
public class RibbonDisplayOptionsSimplifiedTests
{
    [Test]
    public void Use_simplified_ribbon_menu_item_keeps_a_OneWay_binding_on_IsSimplified()
    {
        var settings = new RibbonSettings();
        var ribbon = CreateRibbon(settings, BindingMode.OneWay);

        using (new TestRibbonWindow(ribbon))
        {
            Prepare(ribbon);

            Click(GetMenuItem(ribbon, RibbonLocalization.Current.Localization.UseSimplifiedRibbon));

            Assert.That(ribbon.IsSimplified, Is.True, "Precondition: the menu item must switch to the simplified ribbon.");
            Assert.That(settings.IsSimplified, Is.False, "Precondition: a OneWay binding never writes back to the settings.");
            Assert.That(BindingOperations.GetBindingExpression(ribbon, Ribbon.IsSimplifiedProperty), Is.Not.Null, "The app's OneWay binding on Ribbon.IsSimplified was lost.");

            settings.IsSimplified = true;
            settings.IsSimplified = false;
            Assert.That(ribbon.IsSimplified, Is.False, "The app's settings must still switch the ribbon back.");
        }
    }

    [Test]
    public void Use_classic_ribbon_menu_item_keeps_a_OneWay_binding_on_IsSimplified()
    {
        var settings = new RibbonSettings { IsSimplified = true };
        var ribbon = CreateRibbon(settings, BindingMode.OneWay);

        using (new TestRibbonWindow(ribbon))
        {
            Prepare(ribbon);
            Assert.That(ribbon.IsSimplified, Is.True, "Precondition: the app's settings make the ribbon simplified.");

            Click(GetMenuItem(ribbon, RibbonLocalization.Current.Localization.UseClassicRibbon));

            Assert.That(ribbon.IsSimplified, Is.False, "Precondition: the menu item must switch to the classic ribbon.");
            Assert.That(BindingOperations.GetBindingExpression(ribbon, Ribbon.IsSimplifiedProperty), Is.Not.Null, "The app's OneWay binding on Ribbon.IsSimplified was lost.");

            settings.IsSimplified = false;
            settings.IsSimplified = true;
            Assert.That(ribbon.IsSimplified, Is.True, "The app's settings must still switch the ribbon.");
        }
    }

    /// <summary>
    /// Guard: with a TwoWay binding the menu items still reach the app's settings, and their check marks follow the ribbon.
    /// </summary>
    [Test]
    public void Menu_items_reach_a_TwoWay_binding_and_show_the_state()
    {
        var settings = new RibbonSettings();
        var ribbon = CreateRibbon(settings, BindingMode.TwoWay);

        using (new TestRibbonWindow(ribbon))
        {
            Prepare(ribbon);

            var simplified = GetMenuItem(ribbon, RibbonLocalization.Current.Localization.UseSimplifiedRibbon);
            var classic = GetMenuItem(ribbon, RibbonLocalization.Current.Localization.UseClassicRibbon);

            Click(simplified);
            Assert.That(settings.IsSimplified, Is.True, "Use simplified must reach the TwoWay source.");
            Assert.That(simplified.IsChecked, Is.True, "Use simplified must be checked while the ribbon is simplified.");
            Assert.That(classic.IsChecked, Is.False, "Use classic must not be checked while the ribbon is simplified.");

            Click(classic);
            Assert.That(settings.IsSimplified, Is.False, "Use classic must reach the TwoWay source.");
            Assert.That(simplified.IsChecked, Is.False, "Use simplified must not be checked while the ribbon is classic.");
            Assert.That(classic.IsChecked, Is.True, "Use classic must be checked while the ribbon is classic.");
        }
    }

    private static Ribbon CreateRibbon(RibbonSettings settings, BindingMode mode)
    {
        var ribbon = new Ribbon { CanUseSimplified = true, AutomaticStateManagement = false };
        ribbon.Tabs.Add(new RibbonTabItem { Header = "Home" });
        ribbon.SetBinding(Ribbon.IsSimplifiedProperty, new Binding(nameof(RibbonSettings.IsSimplified)) { Source = settings, Mode = mode });
        return ribbon;
    }

    private static void Prepare(Ribbon ribbon)
    {
        ribbon.ApplyTemplate();
        UIHelper.DoEvents();

        Assert.That(ribbon.TabControl, Is.Not.Null, "Precondition: the ribbon template must create its RibbonTabControl.");
        ribbon.TabControl!.ApplyTemplate();
    }

    private static System.Windows.Controls.MenuItem GetMenuItem(Ribbon ribbon, string header)
    {
        var button = ribbon.TabControl!.Template.FindName("PART_DisplayOptionsButton", ribbon.TabControl) as DropDownButton;
        Assert.That(button, Is.Not.Null, "Precondition: the tab control template must have the display options button.");

        var item = button!.Items.OfType<System.Windows.Controls.MenuItem>().FirstOrDefault(x => Equals(x.Header, header));
        Assert.That(item, Is.Not.Null, $"Precondition: the display options menu must have the item \"{header}\".");

        return item!;
    }

    // What a mouse click on a menu item does: a checkable item toggles IsChecked, then raises Click (MenuItem.OnClickCore, internal).
    private static void Click(System.Windows.Controls.MenuItem item)
    {
        var onClickCore = typeof(System.Windows.Controls.MenuItem).GetMethod("OnClickCore", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(onClickCore, Is.Not.Null, "Precondition: MenuItem.OnClickCore must exist.");

        onClickCore!.Invoke(item, new object[] { true });
        UIHelper.DoEvents();
    }

    private sealed class RibbonSettings : INotifyPropertyChanged
    {
        private bool isSimplified;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSimplified
        {
            get => this.isSimplified;
            set
            {
                if (this.isSimplified == value)
                {
                    return;
                }

                this.isSimplified = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsSimplified)));
            }
        }
    }
}
