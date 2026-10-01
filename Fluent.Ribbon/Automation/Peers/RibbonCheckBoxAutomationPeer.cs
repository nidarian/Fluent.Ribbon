namespace Fluent.Automation.Peers;

/// <inheritdoc />
public class RibbonCheckBoxAutomationPeer : System.Windows.Automation.Peers.CheckBoxAutomationPeer
{
    /// <summary>Initializes a new instance of the <see cref="T:ToggleButtonAutomationPeer" /> class.</summary>
    /// <param name="owner">The element associated with this automation peer.</param>
    public RibbonCheckBoxAutomationPeer(CheckBox owner)
        : base(owner)
    {
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