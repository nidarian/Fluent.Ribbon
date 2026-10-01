namespace Fluent.Automation.Peers;

using System.Windows.Automation.Peers;

/// <summary>
/// Automation peer for items of <see cref="BackstageTabControl"/> which are shown as <see cref="BackstageTabItem"/>.
/// </summary>
/// <remarks>
/// Deriving from <see cref="SelectorItemAutomationPeer"/> provides the SelectionItem pattern, which lets UI Automation clients select the tab.
/// That works because <see cref="BackstageTabItem.IsSelectedProperty"/> is <see cref="System.Windows.Controls.Primitives.Selector.IsSelectedProperty"/>,
/// so selecting through the <see cref="BackstageTabControl"/> (a Selector) is reflected on the tab.
/// Name, children etc. are still forwarded to the <see cref="RibbonBackstageTabItemAutomationPeer"/> of the container by <see cref="ItemAutomationPeer"/>.
/// </remarks>
public class RibbonBackstageTabItemDataAutomationPeer : SelectorItemAutomationPeer
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public RibbonBackstageTabItemDataAutomationPeer(object item, RibbonBackstageTabControlAutomationPeer tabControlAutomationPeer)
        : base(item, tabControlAutomationPeer)
    {
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return "BackstageTabItem";
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.TabItem;
    }
}