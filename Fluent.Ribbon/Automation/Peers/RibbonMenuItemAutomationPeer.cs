namespace Fluent.Automation.Peers;

using System.Windows.Automation.Peers;

/// <summary>
/// Automation peer for <see cref="MenuItem"/>: exposes its KeyTip as AccessKey and its ScreenTip text as HelpText,
/// like the other ribbon controls, so screen readers can announce them.
/// </summary>
public class RibbonMenuItemAutomationPeer : MenuItemAutomationPeer
{
    /// <summary>Initializes a new instance of the <see cref="RibbonMenuItemAutomationPeer" /> class.</summary>
    /// <param name="owner">The element associated with this automation peer.</param>
    public RibbonMenuItemAutomationPeer(MenuItem owner)
        : base(owner)
    {
    }

    /// <inheritdoc />
    protected override string? GetAccessKeyCore()
    {
        return AutomationPeerHelper.GetAccessKey(this.Owner, base.GetAccessKeyCore());
    }

    /// <inheritdoc />
    protected override string GetHelpTextCore()
    {
        return AutomationPeerHelper.GetHelpText(this.Owner, base.GetHelpTextCore());
    }
}
