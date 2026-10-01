namespace Fluent.Automation.Peers;

using System.Collections.Generic;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Fluent.Extensions;

/// <summary>
///     Automation peer for <see cref="BackstageTabControl" />.
/// </summary>
public class RibbonBackstageTabControlAutomationPeer : SelectorAutomationPeer, ISelectionProvider
{
    /// <summary>
    ///     Creates a new instance.
    /// </summary>
    public RibbonBackstageTabControlAutomationPeer(BackstageTabControl owner)
        : base(owner)
    {
        this.OwningBackstageTabControl = owner;
    }

    private BackstageTabControl OwningBackstageTabControl { get; }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Tab;
    }

    /// <inheritdoc />
    protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
    {
        // Tabs (and data items, which get a generated BackstageTabItem as container) must be selectable tab items for UI Automation.
        // Other items that are their own container (Button, Separator, SeparatorTabItem) are not tabs,
        // so they keep the generic peer that forwards to the container's own peer.
        if (item is BackstageTabItem
            || this.OwningBackstageTabControl.IsItemItsOwnContainer(item) == false)
        {
            return new RibbonBackstageTabItemDataAutomationPeer(item, this);
        }

        return new RibbonControlDataAutomationPeer(item, this);
    }

    bool ISelectionProvider.IsSelectionRequired => true;

    bool ISelectionProvider.CanSelectMultiple => false;

    /// <inheritdoc />
    protected override List<AutomationPeer> GetChildrenCore()
    {
        var baseResult = base.GetChildrenCore() ?? new List<AutomationPeer>();

        if (this.OwningBackstageTabControl.BackButton is { } backButton)
        {
            var backButtonAutomationPeer = backButton.GetOrCreateAutomationPeer();

            if (backButtonAutomationPeer is not null)
            {
                baseResult.Insert(0, backButtonAutomationPeer);
            }
        }

        return baseResult;
    }
}