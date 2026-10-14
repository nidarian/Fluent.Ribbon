namespace Fluent.Tests.Data;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using NUnit.Framework;

/// <summary>
/// Apps commonly drive <see cref="Ribbon.IsMinimized"/>, <see cref="Ribbon.ShowQuickAccessToolBarAboveRibbon"/> and
/// <see cref="Ribbon.IsSimplified"/> from their settings with a OneWay binding (none of them binds TwoWay by default).
/// Loading the saved ribbon state (on every start once a state file exists) and the ribbon's own commands wrote these
/// properties with their CLR setters (SetValue). That replaces the app's binding with a local value,
/// so the app's settings can no longer change the ribbon afterwards.
/// </summary>
[TestFixture]
public class RibbonStateOneWayBindingTests
{
    // Saved state: IsMinimized, ShowQuickAccessToolBarAboveRibbon, IsSimplified.
    // Every value is the opposite of the default, so loading changes each property.
    private const string SavedState = "True,False,True";

    [Test]
    public void Loading_the_state_keeps_a_OneWay_binding_on_IsMinimized()
    {
        AssertOneWayBindingSurvives(Ribbon.IsMinimizedProperty, nameof(RibbonSettings.IsMinimized), expectedValue: true, ribbon => LoadState(ribbon, SavedState));
    }

    [Test]
    public void Loading_the_state_keeps_a_OneWay_binding_on_ShowQuickAccessToolBarAboveRibbon()
    {
        AssertOneWayBindingSurvives(Ribbon.ShowQuickAccessToolBarAboveRibbonProperty, nameof(RibbonSettings.ShowQuickAccessToolBarAboveRibbon), expectedValue: false, ribbon => LoadState(ribbon, SavedState));
    }

    [Test]
    public void Loading_the_state_keeps_a_OneWay_binding_on_IsSimplified()
    {
        AssertOneWayBindingSurvives(Ribbon.IsSimplifiedProperty, nameof(RibbonSettings.IsSimplified), expectedValue: true, ribbon => LoadState(ribbon, SavedState));
    }

    [Test]
    public void ToggleMinimizeTheRibbonCommand_keeps_a_OneWay_binding_on_IsMinimized()
    {
        AssertOneWayBindingSurvives(Ribbon.IsMinimizedProperty, nameof(RibbonSettings.IsMinimized), expectedValue: true, ribbon => Ribbon.ToggleMinimizeTheRibbonCommand.Execute(null, ribbon));
    }

    [Test]
    public void ShowQuickAccessBelowCommand_keeps_a_OneWay_binding_on_ShowQuickAccessToolBarAboveRibbon()
    {
        AssertOneWayBindingSurvives(Ribbon.ShowQuickAccessToolBarAboveRibbonProperty, nameof(RibbonSettings.ShowQuickAccessToolBarAboveRibbon), expectedValue: false, ribbon => Ribbon.ShowQuickAccessBelowCommand.Execute(null, ribbon));
    }

    [Test]
    public void SwitchToTheSimplifiedRibbonCommand_keeps_a_OneWay_binding_on_IsSimplified()
    {
        AssertOneWayBindingSurvives(Ribbon.IsSimplifiedProperty, nameof(RibbonSettings.IsSimplified), expectedValue: true, ribbon => Ribbon.SwitchToTheSimplifiedRibbonCommand.Execute(null, ribbon));
    }

    /// <summary>
    /// Guard for apps with TwoWay bindings: they must still get the loaded and toggled values written back to their settings.
    /// </summary>
    [Test]
    public void TwoWay_bindings_receive_the_loaded_and_toggled_values()
    {
        var settings = new RibbonSettings();
        var ribbon = CreateRibbon(settings, BindingMode.TwoWay);

        LoadState(ribbon, SavedState);

        Assert.That(settings.IsMinimized, Is.True, "Loaded IsMinimized must reach the TwoWay source.");
        Assert.That(settings.ShowQuickAccessToolBarAboveRibbon, Is.False, "Loaded ShowQuickAccessToolBarAboveRibbon must reach the TwoWay source.");
        Assert.That(settings.IsSimplified, Is.True, "Loaded IsSimplified must reach the TwoWay source.");

        Ribbon.ToggleMinimizeTheRibbonCommand.Execute(null, ribbon);
        Ribbon.ShowQuickAccessAboveCommand.Execute(null, ribbon);
        Ribbon.SwitchToTheClassicRibbonCommand.Execute(null, ribbon);

        Assert.That(settings.IsMinimized, Is.False, "Toggled IsMinimized must reach the TwoWay source.");
        Assert.That(settings.ShowQuickAccessToolBarAboveRibbon, Is.True, "ShowQuickAccessToolBarAboveRibbon set by the command must reach the TwoWay source.");
        Assert.That(settings.IsSimplified, Is.False, "IsSimplified set by the command must reach the TwoWay source.");

        Assert.That(BindingOperations.GetBindingExpression(ribbon, Ribbon.IsMinimizedProperty), Is.Not.Null, "TwoWay binding on IsMinimized must still exist.");
        Assert.That(BindingOperations.GetBindingExpression(ribbon, Ribbon.ShowQuickAccessToolBarAboveRibbonProperty), Is.Not.Null, "TwoWay binding on ShowQuickAccessToolBarAboveRibbon must still exist.");
        Assert.That(BindingOperations.GetBindingExpression(ribbon, Ribbon.IsSimplifiedProperty), Is.Not.Null, "TwoWay binding on IsSimplified must still exist.");
    }

    private static void AssertOneWayBindingSurvives(DependencyProperty property, string settingsPropertyName, bool expectedValue, Action<Ribbon> change)
    {
        var settings = new RibbonSettings();
        var ribbon = CreateRibbon(settings, BindingMode.OneWay);

        Assert.That((bool)ribbon.GetValue(property), Is.EqualTo(!expectedValue), $"Precondition: {property.Name} starts with the value from the settings.");

        change(ribbon);

        Assert.That((bool)ribbon.GetValue(property), Is.EqualTo(expectedValue), $"{property.Name} must be changed by the ribbon.");
        Assert.That(settings.Get(settingsPropertyName), Is.EqualTo(!expectedValue), "Precondition: a OneWay binding never writes back to the settings.");

        // SetValue would have replaced the binding with a local value; SetCurrentValue keeps it.
        Assert.That(BindingOperations.GetBindingExpression(ribbon, property), Is.Not.Null, $"The app's OneWay binding on {property.Name} was lost.");

        // The settings still hold the old value (OneWay never wrote back), so set the changed value first to get a change notification through.
        settings.Set(settingsPropertyName, expectedValue);
        settings.Set(settingsPropertyName, !expectedValue);
        Assert.That((bool)ribbon.GetValue(property), Is.EqualTo(!expectedValue), $"The app's settings must still change {property.Name}.");

        settings.Set(settingsPropertyName, expectedValue);
        Assert.That((bool)ribbon.GetValue(property), Is.EqualTo(expectedValue), $"The app's settings must still change {property.Name} back.");
    }

    private static Ribbon CreateRibbon(RibbonSettings settings, BindingMode mode)
    {
        // CanUseSimplified must be on, otherwise loading and the switch commands ignore IsSimplified.
        var ribbon = new Ribbon
        {
            CanUseSimplified = true,
            CanMinimize = true
        };

        ribbon.SetBinding(Ribbon.IsMinimizedProperty, new Binding(nameof(RibbonSettings.IsMinimized)) { Source = settings, Mode = mode });
        ribbon.SetBinding(Ribbon.ShowQuickAccessToolBarAboveRibbonProperty, new Binding(nameof(RibbonSettings.ShowQuickAccessToolBarAboveRibbon)) { Source = settings, Mode = mode });
        ribbon.SetBinding(Ribbon.IsSimplifiedProperty, new Binding(nameof(RibbonSettings.IsSimplified)) { Source = settings, Mode = mode });

        return ribbon;
    }

    private static void LoadState(Ribbon ribbon, string data)
    {
        using (var storage = new LoadableRibbonStateStorage(ribbon))
        {
            storage.LoadFrom(data);
        }
    }

    // Gives the tests access to the parsing that runs when a saved state is loaded (from the state file or the temporary state).
    private sealed class LoadableRibbonStateStorage : RibbonStateStorage
    {
        public LoadableRibbonStateStorage(Ribbon ribbon)
            : base(ribbon)
        {
        }

        public void LoadFrom(string data)
        {
            this.LoadState(data);
        }
    }

    private sealed class RibbonSettings : INotifyPropertyChanged
    {
        private bool isMinimized;
        private bool showQuickAccessToolBarAboveRibbon = true;
        private bool isSimplified;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsMinimized
        {
            get => this.isMinimized;
            set => this.SetField(ref this.isMinimized, value, nameof(this.IsMinimized));
        }

        public bool ShowQuickAccessToolBarAboveRibbon
        {
            get => this.showQuickAccessToolBarAboveRibbon;
            set => this.SetField(ref this.showQuickAccessToolBarAboveRibbon, value, nameof(this.ShowQuickAccessToolBarAboveRibbon));
        }

        public bool IsSimplified
        {
            get => this.isSimplified;
            set => this.SetField(ref this.isSimplified, value, nameof(this.IsSimplified));
        }

        public bool Get(string propertyName)
        {
            return (bool)typeof(RibbonSettings).GetProperty(propertyName).GetValue(this);
        }

        public void Set(string propertyName, bool value)
        {
            typeof(RibbonSettings).GetProperty(propertyName).SetValue(this, value);
        }

        private void SetField(ref bool field, bool value, string propertyName)
        {
            if (field == value)
            {
                return;
            }

            field = value;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
