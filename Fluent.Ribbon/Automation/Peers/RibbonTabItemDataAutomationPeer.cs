namespace Fluent.Automation.Peers;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Fluent.Extensions;
using Fluent.Internal.KnownBoxes;

/// <summary>
/// Automation peer for <see cref="RibbonTabItem"/>.
/// </summary>
public class RibbonTabItemDataAutomationPeer : SelectorItemAutomationPeer, IScrollItemProvider, IExpandCollapseProvider
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public RibbonTabItemDataAutomationPeer(object item, RibbonTabControlAutomationPeer tabControlAutomationPeer)
        : base(item, tabControlAutomationPeer)
    {
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return "RibbonTabItem";
    }

    /// <inheritdoc />
    protected override string GetNameCore()
    {
        var wrapper = this.GetWrapper() as RibbonTabItem;

        if (wrapper is not null)
        {
            // An explicit AutomationProperties.Name is the app author's deliberate choice
            // (e.g. "Home tab" or a localized name), so it must win over the visible Header text.
            var automationName = AutomationProperties.GetName(wrapper);
            if (string.IsNullOrEmpty(automationName) == false)
            {
                return automationName;
            }

            // Otherwise a string Header is the best name: base can fall back to item.ToString(),
            // which for a RibbonTabItem is not meaningful to a user.
            if (wrapper.Header is string headerString
                && string.IsNullOrEmpty(headerString) == false)
            {
                return headerString;
            }
        }

        return base.GetNameCore();
    }

    /// <inheritdoc />
    protected override string? GetAccessKeyCore()
    {
        var text = (this.GetWrapper() as RibbonTabItem)?.KeyTip;
        if (string.IsNullOrEmpty(text))
        {
            text = base.GetAccessKeyCore();
        }

        return text;
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.TabItem;
    }

    #region IExpandCollapseProvider Members

    /// <summary>
    /// If Ribbon.IsMinimized then set Ribbon.IsDropDownOpen to false
    /// </summary>
    void IExpandCollapseProvider.Collapse()
    {
        var wrapperTab = this.GetWrapper() as RibbonTabItem;
        if (wrapperTab is not null)
        {
            var tabControl = wrapperTab.TabControlParent;
            if (tabControl is not null &&
                tabControl.IsMinimized)
            {
                tabControl.SetCurrentValue(RibbonTabControl.IsDropDownOpenProperty, BooleanBoxes.FalseBox);
            }
        }
    }

    /// <summary>
    /// If Ribbon.IsMinimized then set Ribbon.IsDropDownOpen to true
    /// </summary>
    void IExpandCollapseProvider.Expand()
    {
        var wrapperTab = this.GetWrapper() as RibbonTabItem;

        // Select the tab and display popup
        if (wrapperTab is not null)
        {
            var tabControl = wrapperTab.TabControlParent;
            if (tabControl is not null &&
                tabControl.IsMinimized)
            {
                wrapperTab.IsSelected = true;
                tabControl.SetCurrentValue(RibbonTabControl.IsDropDownOpenProperty, BooleanBoxes.TrueBox);
            }
        }
    }

    /// <summary>
    /// Return Ribbon.IsDropDownOpen
    /// </summary>
    ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
    {
        get
        {
            var wrapperTab = this.GetWrapper() as RibbonTabItem;
            if (wrapperTab is not null)
            {
                var tabControl = wrapperTab.TabControlParent;
                if (tabControl is not null &&
                    tabControl.IsMinimized)
                {
                    if (wrapperTab.IsSelected && tabControl.IsDropDownOpen)
                    {
                        return ExpandCollapseState.Expanded;
                    }
                    else
                    {
                        return ExpandCollapseState.Collapsed;
                    }
                }
            }

            // When not minimized
            return ExpandCollapseState.Expanded;
        }
    }

    #endregion

    #region IScrollItemProvider Members

    void IScrollItemProvider.ScrollIntoView()
    {
        var wrapperTab = this.GetWrapper() as RibbonTabItem;
        if (wrapperTab is not null)
        {
            wrapperTab.BringIntoView();
        }
    }

    #endregion
}