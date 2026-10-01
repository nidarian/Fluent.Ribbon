namespace Fluent.Internal;

using Fluent.Internal.KnownBoxes;

/// <summary>
/// Helper for changing <see cref="IDropDownControl.IsDropDownOpen"/> from inside the library.
/// </summary>
/// <remarks>
/// Internal code (mouse, keyboard, key tips, automation, <see cref="PopupService"/>) must not use the CLR setter,
/// because it calls SetValue, which replaces any binding or style setter on the property with a local value.
/// An app that drives the drop down with a OneWay binding would then lose that binding after the first user interaction.
/// SetCurrentValue only changes the effective value and keeps bindings, styles and triggers in place,
/// while property changed callbacks and coercion still run exactly as with SetValue.
/// </remarks>
internal static class DropDownControlHelper
{
    /// <summary>
    /// Opens or closes the drop down of <paramref name="control"/> without replacing a binding on its IsDropDownOpen property.
    /// </summary>
    /// <param name="control">The drop down control.</param>
    /// <param name="value">The new value for IsDropDownOpen.</param>
    internal static void SetIsDropDownOpenCurrentValue(IDropDownControl control, bool value)
    {
        // IDropDownControl only exposes a CLR property, so map each known implementer to its DependencyProperty.
        // DropDownButton also covers SplitButton, ApplicationMenu and every other derived class.
        switch (control)
        {
            case DropDownButton dropDownButton:
                dropDownButton.SetCurrentValue(DropDownButton.IsDropDownOpenProperty, BooleanBoxes.Box(value));
                break;

            case RibbonGroupBox ribbonGroupBox:
                ribbonGroupBox.SetCurrentValue(RibbonGroupBox.IsDropDownOpenProperty, BooleanBoxes.Box(value));
                break;

            case InRibbonGallery inRibbonGallery:
                inRibbonGallery.SetCurrentValue(InRibbonGallery.IsDropDownOpenProperty, BooleanBoxes.Box(value));
                break;

            case RibbonTabControl ribbonTabControl:
                ribbonTabControl.SetCurrentValue(RibbonTabControl.IsDropDownOpenProperty, BooleanBoxes.Box(value));
                break;

            // Fluent.ComboBox inherits IsDropDownOpen from the WPF ComboBox.
            case System.Windows.Controls.ComboBox comboBox:
                comboBox.SetCurrentValue(System.Windows.Controls.ComboBox.IsDropDownOpenProperty, BooleanBoxes.Box(value));
                break;

            // Fluent.MenuItem maps IsDropDownOpen to the WPF MenuItem.IsSubmenuOpen property.
            case System.Windows.Controls.MenuItem menuItem:
                menuItem.SetCurrentValue(System.Windows.Controls.MenuItem.IsSubmenuOpenProperty, BooleanBoxes.Box(value));
                break;

            default:
                // Unknown (e.g. third party) implementer: we don't know its DependencyProperty, so use the interface setter.
                control.IsDropDownOpen = value;
                break;
        }
    }
}
