namespace Fluent.Automation.Peers;

using System.Windows.Automation.Peers;

/// <inheritdoc />
public class GalleryItemWrapperAutomationPeer : FrameworkElementAutomationPeer
{
    /// <inheritdoc cref="FrameworkElementAutomationPeer" />
    public GalleryItemWrapperAutomationPeer(GalleryItem owner)
        : base(owner)
    {
    }

    /// <inheritdoc />
    protected override string GetClassNameCore() => "ListBoxItem";

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ListItem;

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