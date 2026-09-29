namespace Fluent.Tests.Automation.Peers;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Data;
using Fluent.Automation.Peers;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for <see cref="RibbonSpinnerAutomationPeer"/>.
/// https://github.com/fluentribbon/Fluent.Ribbon/issues/647 lists Spinner as missing a custom automation peer.
/// </summary>
[TestFixture]
public class RibbonSpinnerAutomationPeerTests
{
    private static Spinner CreateSpinner() => new()
    {
        Header = "Zoom",
        Minimum = 0,
        Maximum = 10,
        Increment = 0.5,
        Value = 3
    };

    [Test]
    public void Spinner_creates_spinner_peer_with_name_from_header()
    {
        var spinner = CreateSpinner();

        var peer = UIElementAutomationPeer.CreatePeerForElement(spinner);

        Assert.That(peer, Is.InstanceOf<RibbonSpinnerAutomationPeer>());
        Assert.That(peer.GetAutomationControlType(), Is.EqualTo(AutomationControlType.Spinner));
        Assert.That(peer.GetName(), Is.EqualTo("Zoom"));
        Assert.That(peer.GetClassName(), Is.EqualTo(nameof(Spinner)));
    }

    [Test]
    public void RangeValue_pattern_reports_value_and_range()
    {
        var spinner = CreateSpinner();
        var provider = GetRangeValueProvider(spinner);

        Assert.That(provider.Value, Is.EqualTo(3));
        Assert.That(provider.Minimum, Is.EqualTo(0));
        Assert.That(provider.Maximum, Is.EqualTo(10));
        Assert.That(provider.SmallChange, Is.EqualTo(0.5));
        Assert.That(provider.LargeChange, Is.EqualTo(0.5));
        Assert.That(provider.IsReadOnly, Is.False);

        // The pattern must follow the control, not a copy taken when the peer was created.
        spinner.Value = 4.5;
        Assert.That(provider.Value, Is.EqualTo(4.5));
    }

    [Test]
    public void SetValue_changes_the_spinner_value()
    {
        var spinner = CreateSpinner();
        var provider = GetRangeValueProvider(spinner);

        provider.SetValue(7);

        Assert.That(spinner.Value, Is.EqualTo(7));
    }

    [Test]
    public void SetValue_rejects_values_outside_the_range()
    {
        var spinner = CreateSpinner();
        var provider = GetRangeValueProvider(spinner);

        Assert.Throws<ArgumentOutOfRangeException>(() => provider.SetValue(11));
        Assert.Throws<ArgumentOutOfRangeException>(() => provider.SetValue(-1));
        Assert.That(spinner.Value, Is.EqualTo(3), "Rejected values must not change the spinner");
    }

    [Test]
    public void Disabled_spinner_is_read_only_for_automation()
    {
        var spinner = CreateSpinner();
        spinner.IsEnabled = false;

        using (new TestRibbonWindow(spinner))
        {
            var provider = GetRangeValueProvider(spinner);

            Assert.That(provider.IsReadOnly, Is.True);
            Assert.Throws<ElementNotEnabledException>(() => provider.SetValue(5));
            Assert.That(spinner.Value, Is.EqualTo(3));
        }
    }

    [Test]
    public void SetValue_keeps_a_binding_on_Value()
    {
        // Apps usually bind Value to a view model. Setting it through automation must update
        // the view model, not silently replace the binding.
        var viewModel = new ViewModel { Zoom = 3 };
        var spinner = CreateSpinner();
        spinner.SetBinding(Spinner.ValueProperty, new Binding(nameof(ViewModel.Zoom)) { Source = viewModel });

        var provider = GetRangeValueProvider(spinner);
        provider.SetValue(8);

        Assert.That(viewModel.Zoom, Is.EqualTo(8), "View model should receive the new value");
        Assert.That(BindingOperations.IsDataBound(spinner, Spinner.ValueProperty), Is.True, "Binding must survive");

        viewModel.Zoom = 2;
        Assert.That(spinner.Value, Is.EqualTo(2), "Binding must still flow from the view model");
    }

    private static IRangeValueProvider GetRangeValueProvider(Spinner spinner)
    {
        var peer = UIElementAutomationPeer.CreatePeerForElement(spinner);
        var provider = peer.GetPattern(PatternInterface.RangeValue) as IRangeValueProvider;

        Assert.That(provider, Is.Not.Null, "Spinner should support the RangeValue pattern");

        return provider;
    }

    private sealed class ViewModel : INotifyPropertyChanged
    {
        private double zoom;

        public event PropertyChangedEventHandler PropertyChanged;

        public double Zoom
        {
            get => this.zoom;
            set
            {
                this.zoom = value;
                this.OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
