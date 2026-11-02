namespace Fluent.Automation.Peers;

using System.Windows.Automation.Peers;

/// <summary>
/// Automation peer for <see cref="RibbonContextualTabGroup"/>, so screen readers announce the context of its tabs ("Table Tools").
/// The header is drawn by a TextBlock in the template, which UI Automation hides from the control view.
/// </summary>
public class RibbonContextualTabGroupAutomationPeer : FrameworkElementAutomationPeer
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public RibbonContextualTabGroupAutomationPeer(RibbonContextualTabGroup owner)
        : base(owner)
    {
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Group;
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return this.Owner.GetType().Name;
    }

    /// <inheritdoc />
    protected override string? GetNameCore()
    {
        // AutomationProperties.Name first; then the header, but only if it is text (never the type name of a header that isn't).
        var name = base.GetNameCore();

        if (string.IsNullOrEmpty(name)
            && this.Owner is RibbonContextualTabGroup { Header: string header })
        {
            name = header;
        }

        return name;
    }
}
