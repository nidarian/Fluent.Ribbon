namespace Fluent.Automation.Peers;

using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

/// <summary>
/// Automation peer for <see cref="Spinner"/>.
/// </summary>
/// <remarks>
/// Exposes the spinner as a UI Automation "Spinner" with the RangeValue pattern, the same pattern
/// WPF uses for sliders and scroll bars. That lets screen readers announce the value and its range,
/// and lets UI test tools read and set it.
/// See https://github.com/fluentribbon/Fluent.Ribbon/issues/647.
/// </remarks>
public class RibbonSpinnerAutomationPeer : RibbonHeaderedControlAutomationPeer, IRangeValueProvider
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public RibbonSpinnerAutomationPeer(Spinner owner)
        : base(owner)
    {
        this.OwnerSpinner = owner;
    }

    private Spinner OwnerSpinner { get; }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Spinner;
    }

    /// <inheritdoc />
    public override object GetPattern(PatternInterface patternInterface)
    {
        switch (patternInterface)
        {
            case PatternInterface.RangeValue:
                return this;
        }

        return base.GetPattern(patternInterface);
    }

    #region IRangeValueProvider Members

    /// <inheritdoc />
    double IRangeValueProvider.Value => this.OwnerSpinner.Value;

    /// <inheritdoc />
    double IRangeValueProvider.Minimum => this.OwnerSpinner.Minimum;

    /// <inheritdoc />
    double IRangeValueProvider.Maximum => this.OwnerSpinner.Maximum;

    /// <inheritdoc />
    /// <remarks>One click on the up or down button.</remarks>
    double IRangeValueProvider.SmallChange => this.OwnerSpinner.Increment;

    /// <inheritdoc />
    /// <remarks>The spinner has no separate "page" step, so this is the same as <see cref="IRangeValueProvider.SmallChange"/>.</remarks>
    double IRangeValueProvider.LargeChange => this.OwnerSpinner.Increment;

    /// <inheritdoc />
    bool IRangeValueProvider.IsReadOnly => this.OwnerSpinner.IsEnabled == false;

    /// <inheritdoc />
    void IRangeValueProvider.SetValue(double value)
    {
        // Same rules as WPF's RangeBaseAutomationPeer: disabled controls and out of range values are rejected
        // instead of being silently clamped, so automation clients learn about the mistake.
        if (this.IsEnabled() == false)
        {
            throw new ElementNotEnabledException();
        }

        if (value < this.OwnerSpinner.Minimum
            || value > this.OwnerSpinner.Maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Value must be between {this.OwnerSpinner.Minimum} and {this.OwnerSpinner.Maximum}.");
        }

        // SetCurrentValue keeps any binding on Value intact (a plain assignment could replace it).
        this.OwnerSpinner.SetCurrentValue(Spinner.ValueProperty, value);
    }

    #endregion

    /// <summary>
    /// Tells automation clients (for example screen readers) that the value changed.
    /// </summary>
    internal void RaiseValuePropertyChangedEvent(double oldValue, double newValue)
    {
        this.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, oldValue, newValue);
    }
}
