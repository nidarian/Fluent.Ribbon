namespace Fluent.Automation.Peers;

using System.Windows;
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
    /// <remarks>
    /// The base class doesn't return <see cref="PatternInterface.ExpandCollapse"/>, so screen readers could not open a tab on a
    /// minimized ribbon. It is offered only while the ribbon is minimized (on a normal ribbon a tab isn't expanded or collapsed),
    /// and only for a tab the user can see.
    /// </remarks>
    public override object? GetPattern(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.ExpandCollapse
            && this.GetWrapper() is RibbonTabItem { TabControlParent.IsMinimized: true } tab
            && IsShown(tab))
        {
            return this;
        }

        return base.GetPattern(patternInterface);
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
    /// On a minimized ribbon, closes the popup if this tab is the open one.
    /// Throws <see cref="ElementNotEnabledException"/> for a disabled tab.
    /// </summary>
    void IExpandCollapseProvider.Collapse()
    {
        // UI Automation: a disabled element must refuse actions.
        if (this.IsEnabled() == false)
        {
            throw new ElementNotEnabledException();
        }

        var wrapperTab = this.GetWrapper() as RibbonTabItem;
        if (wrapperTab is not null)
        {
            var tabControl = wrapperTab.TabControlParent;
            if (tabControl is not null &&
                tabControl.IsMinimized &&
                wrapperTab.IsSelected)
            {
                tabControl.SetCurrentValue(RibbonTabControl.IsDropDownOpenProperty, BooleanBoxes.FalseBox);
            }
        }
    }

    /// <summary>
    /// On a minimized ribbon, selects this tab and opens its popup, if the user can see the tab.
    /// Throws <see cref="ElementNotEnabledException"/> for a disabled tab.
    /// </summary>
    void IExpandCollapseProvider.Expand()
    {
        // UI Automation: a disabled element must refuse actions.
        if (this.IsEnabled() == false)
        {
            throw new ElementNotEnabledException();
        }

        var wrapperTab = this.GetWrapper() as RibbonTabItem;

        // Select the tab and display popup
        if (wrapperTab is not null)
        {
            var tabControl = wrapperTab.TabControlParent;
            if (tabControl is not null &&
                tabControl.IsMinimized &&
                IsShown(wrapperTab))
            {
                wrapperTab.SetCurrentValue(RibbonTabItem.IsSelectedProperty, BooleanBoxes.TrueBox); // keeps an app's binding on IsSelected
                tabControl.SetCurrentValue(RibbonTabControl.IsDropDownOpenProperty, BooleanBoxes.TrueBox);
            }
        }
    }

    /// <summary>
    /// On a minimized ribbon, Expanded while this tab is selected and its popup is open.
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

    // Same rule the tab control uses to pick tabs (mouse wheel, keyboard): visible, and for a contextual tab its group visible too.
    private static bool IsShown(RibbonTabItem tab)
    {
        return tab.Visibility == Visibility.Visible
               && (tab.IsContextual == false || tab.Group?.Visibility == Visibility.Visible);
    }
}
