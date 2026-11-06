namespace Fluent.Automation.Peers;

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Provider;

/// <summary>
/// Wraps WPF's own Toggle pattern of a toggle button or checkable menu item that can be in a group (GroupName).
/// A click on the checked item of a group keeps it checked (issue #481), so the group always has one checked item.
/// WPF's Toggle doesn't know about groups and would uncheck it; this wrapper leaves a checked grouped item checked
/// and passes everything else to WPF's Toggle unchanged.
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
