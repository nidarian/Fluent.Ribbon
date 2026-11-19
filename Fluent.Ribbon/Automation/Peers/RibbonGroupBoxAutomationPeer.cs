namespace Fluent.Automation.Peers;

using System.Collections.Generic;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Fluent.Internal.KnownBoxes;

/// <summary>
/// Automation peer for <see cref="RibbonGroupBox"/>.
/// </summary>
public class RibbonGroupBoxAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider, IScrollItemProvider
{
    private RibbonGroupHeaderAutomationPeer? headerPeer;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public RibbonGroupBoxAutomationPeer(RibbonGroupBox owner)
        : base(owner)
    {
        this.OwningGroup = owner;
    }

    private RibbonGroupBox OwningGroup { get; }

    private RibbonGroupHeaderAutomationPeer? HeaderPeer
    {
        get
        {
            if (this.headerPeer is null
                || !this.headerPeer.Owner.IsDescendantOf(this.OwningGroup))
            {
                if (this.OwningGroup.State == RibbonGroupBoxState.Collapsed)
                {
                    if (this.OwningGroup.CollapsedHeaderContentControl is not null)
                    {
                        this.headerPeer = new RibbonGroupHeaderAutomationPeer(this.OwningGroup.CollapsedHeaderContentControl);
                    }
                }
                else if (this.OwningGroup.Header is not null
                         && this.OwningGroup.HeaderContentControl is not null)
                {
                    this.headerPeer = new RibbonGroupHeaderAutomationPeer(this.OwningGroup.HeaderContentControl);
                }
            }

            return this.headerPeer;
        }
    }

    /// <inheritdoc />
    protected override List<AutomationPeer> GetChildrenCore()
    {
        var list = base.GetChildrenCore();

        var header = this.HeaderPeer;
        if (header is not null)
        {
            if (list is null)
            {
                list = new List<AutomationPeer>(1);
            }

            // The header ContentControl has no peer of its own, so the base walk goes through it and lists its content
            // (the header TextBlock) as a direct child of the group. The header peer lists that same peer as its child.
            // A peer has one parent, and the last walk wins: from the second walk on, the TextBlock's parent is the header
            // peer while the group still lists it, so UI Automation's next-sibling navigation ends there and everything after
            // it (the dialog launcher, the header) drops out of the tree. Leave the header's content to the header peer.
            var headerOwner = header.Owner;
            list.RemoveAll(peer => peer is UIElementAutomationPeer { Owner: { } owner }
                                   && owner != headerOwner
                                   && owner.IsDescendantOf(headerOwner));

            list.Add(header);
        }

        return list;
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return this.Owner.GetType().Name;
    }

    /// <inheritdoc />
    protected override string GetNameCore()
    {
        var name = base.GetNameCore();

        if (string.IsNullOrEmpty(name))
        {
            name = (this.Owner as IHeaderedControl)?.Header as string;
        }

        return name ?? string.Empty;
    }

    /// <inheritdoc />
    public override object GetPattern(PatternInterface patternInterface)
    {
        switch (patternInterface)
        {
            case PatternInterface.ExpandCollapse:
                return this.IsCollapseOrExpandValid ? this : base.GetPattern(patternInterface);

            case PatternInterface.Scroll:
                return base.GetPattern(patternInterface);

            default:
                return base.GetPattern(patternInterface);
        }
    }

    /// <inheritdoc />
    protected override void SetFocusCore()
    {
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return this.IsCollapseOrExpandValid
            ? AutomationControlType.Button
            : AutomationControlType.Group;
    }

    #region IExpandCollapseProvider Members

    /// <inheritdoc />
    void IExpandCollapseProvider.Expand()
    {
        // UI Automation: a disabled element must refuse actions.
        if (this.IsEnabled() == false)
        {
            throw new ElementNotEnabledException();
        }

        if (this.IsCollapseOrExpandValid == false)
        {
            return;
        }

        this.OwningGroup.SetCurrentValue(RibbonGroupBox.IsDropDownOpenProperty, BooleanBoxes.TrueBox);
    }

    /// <inheritdoc />
    void IExpandCollapseProvider.Collapse()
    {
        // UI Automation: a disabled element must refuse actions.
        if (this.IsEnabled() == false)
        {
            throw new ElementNotEnabledException();
        }

        if (this.IsCollapseOrExpandValid == false)
        {
            return;
        }

        this.OwningGroup.SetCurrentValue(RibbonGroupBox.IsDropDownOpenProperty, BooleanBoxes.FalseBox);
    }

    /// <inheritdoc />
    // A group that isn't collapsed shows its content, so it counts as expanded.
    // A collapsed group is expanded while its drop down is open.
    ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState => this.IsCollapseOrExpandValid == false || this.OwningGroup.IsDropDownOpen
        ? ExpandCollapseState.Expanded
        : ExpandCollapseState.Collapsed;

    private bool IsCollapseOrExpandValid => this.OwningGroup.State is RibbonGroupBoxState.Collapsed or RibbonGroupBoxState.QuickAccess;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
    {
        this.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
            oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
            newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
    }

    #endregion

    #region IScrollItemProvider Members

    /// <inheritdoc />
    void IScrollItemProvider.ScrollIntoView()
    {
        this.OwningGroup.BringIntoView();
    }

    /// <inheritdoc />
    protected override string? GetAccessKeyCore()
    {
        // Expose the KeyTip so screen readers can announce the keyboard shortcut (a collapsed group is a button with a KeyTip).
        return AutomationPeerHelper.GetAccessKey(this.Owner, base.GetAccessKeyCore());
    }

    /// <inheritdoc />
    protected override string GetHelpTextCore()
    {
        // Expose the ScreenTip text, which the default implementation can't read.
        return AutomationPeerHelper.GetHelpText(this.Owner, base.GetHelpTextCore());
    }

    #endregion
}