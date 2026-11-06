namespace Fluent.Automation.Peers;

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Provider;

/// <summary>
/// Wraps WPF's own Toggle pattern of a toggle button or checkable menu item that can be in a group (GroupName).
/// A click on the checked item of a group keeps its state (issue #481; for a three-state toggle button also the
/// indeterminate state), so the group keeps its checked item. WPF's Toggle doesn't know about groups and would change it;
/// this wrapper leaves such an item as it is and passes everything else to WPF's Toggle unchanged.
/// </summary>
internal sealed class GroupedToggleProvider : IToggleProvider
{
    private readonly UIElement owner;
    private readonly IToggleProvider inner;
    private readonly Func<bool> isCheckedInGroup;

    public GroupedToggleProvider(UIElement owner, IToggleProvider inner, Func<bool> isCheckedInGroup)
    {
        this.owner = owner;
        this.inner = inner;
        this.isCheckedInGroup = isCheckedInGroup;
    }

    public ToggleState ToggleState => this.inner.ToggleState;

    public void Toggle()
    {
        if (this.isCheckedInGroup())
        {
            // UI Automation: a disabled element must refuse actions.
            if (this.owner.IsEnabled == false)
            {
                throw new ElementNotEnabledException();
            }

            // Like a click: the checked item of a group stays checked.
            return;
        }

        this.inner.Toggle();
    }
}
