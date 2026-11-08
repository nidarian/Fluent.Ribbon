namespace Fluent.Automation.Peers;

/// <inheritdoc />
public class RibbonToggleButtonAutomationPeer : System.Windows.Automation.Peers.ToggleButtonAutomationPeer
{
    /// <summary>Initializes a new instance of the <see cref="T:ToggleButtonAutomationPeer" /> class.</summary>
    /// <param name="owner">The element associated with this automation peer.</param>
    public RibbonToggleButtonAutomationPeer(ToggleButton owner)
        : base(owner)
    {
    }

    /// <inheritdoc />
    /// <remarks>
    /// The Toggle pattern is wrapped so that toggling the checked button of a group keeps it checked, like a click (issue #481).
    /// </remarks>
    public override object? GetPattern(System.Windows.Automation.Peers.PatternInterface patternInterface)
    {
        var pattern = base.GetPattern(patternInterface);

        if (patternInterface == System.Windows.Automation.Peers.PatternInterface.Toggle
            && pattern is System.Windows.Automation.Provider.IToggleProvider toggleProvider
            && this.Owner is ToggleButton toggleButton)
        {
            return new GroupedToggleProvider(toggleButton, toggleProvider, () => string.IsNullOrEmpty(toggleButton.GroupName) == false && toggleButton.IsChecked == true); // like ToggleButton.OnClick
        }

        return pattern;
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return "RibbonToggleButton";
    }

    /// <inheritdoc />
    protected override string? GetNameCore()
    {
        var name = base.GetNameCore();

        if (string.IsNullOrEmpty(name))
        {
            name = (this.Owner as IHeaderedControl)?.Header as string;
        }

        return name;
    }

    /// <inheritdoc />
    protected override string? GetAccessKeyCore()
    {
        // Expose the KeyTip so screen readers can announce the keyboard shortcut.
        return AutomationPeerHelper.GetAccessKey(this.Owner, base.GetAccessKeyCore());
    }

    /// <inheritdoc />
    protected override string GetHelpTextCore()
    {
        // Expose the ScreenTip text, which the default implementation can't read.
        return AutomationPeerHelper.GetHelpText(this.Owner, base.GetHelpTextCore());
    }
}