namespace Fluent.Automation.Peers;

using System.Windows.Automation.Peers;

/// <summary>
/// Automation peer for <see cref="RibbonControl" />.
/// </summary>
public class RibbonControlAutomationPeer : FrameworkElementAutomationPeer
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public RibbonControlAutomationPeer(RibbonControl owner)
        : base(owner)
    {
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return this.Owner.GetType().Name;
    }

    /// <inheritdoc />
    protected override string GetNameCore()
    {
        // AutomationProperties.Name (and other explicit names) come first.
        var name = base.GetNameCore();

        // A RibbonControl is a plain Control, so WPF finds no text of its own to use.
        // Fall back to the header it shows (for example "File" on the backstage button).
        if (string.IsNullOrEmpty(name)
            && this.Owner is RibbonControl { Header: string header })
        {
            return header;
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