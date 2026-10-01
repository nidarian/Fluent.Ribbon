namespace Fluent.Automation.Peers;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;

/// <summary>
/// Automation peer for <see cref="QuickAccessToolBar"/>.
/// </summary>
public class RibbonQuickAccessToolBarAutomationPeer : FrameworkElementAutomationPeer
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public RibbonQuickAccessToolBarAutomationPeer(QuickAccessToolBar owner)
        : base(owner)
    {
        this.OwningQuickAccessToolBar = owner;
    }

    private QuickAccessToolBar OwningQuickAccessToolBar { get; }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.ToolBar;
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return this.Owner.GetType().Name;
    }

    /// <inheritdoc />
    protected override List<AutomationPeer> GetChildrenCore()
    {
        var children = new List<AutomationPeer>();

        foreach (var quickAccessMenuItem in this.OwningQuickAccessToolBar.Items)
        {
            //if (quickAccessMenuItem.IsChecked == false)
            //{
            //    continue;
            //}

            var automationPeer = CreatePeerForElement(quickAccessMenuItem);

            if (automationPeer is not null)
            {
                children.Add(automationPeer);
            }
        }

        // The overflow button is only shown when not all items fit. Listing it while collapsed
        // would expose an invisible element, so it's only added when the template shows it.
        var overflowButton = this.OwningQuickAccessToolBar.ToolBarDownButton;
        if (overflowButton is not null
            && overflowButton.Visibility == Visibility.Visible)
        {
            var automationPeer = CreatePeerForElement(overflowButton);

            if (automationPeer is not null)
            {
                children.Add(automationPeer);
            }
        }

        var customizeMenuButton = this.OwningQuickAccessToolBar.MenuDownButton;
        if (customizeMenuButton is not null)
        {
            var automationPeer = CreatePeerForElement(customizeMenuButton);

            if (automationPeer is not null)
            {
                children.Add(automationPeer);
            }
        }

        return children;
    }
}