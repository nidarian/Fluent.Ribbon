// ReSharper disable once CheckNamespace
namespace Fluent;

using System.Windows;
using System.Windows.Automation;
using System.Windows.Data;

/// <summary>
/// Represents menu item in ribbon status bar menu
/// </summary>
public class StatusBarMenuItem : MenuItem
{
    #region Properties

    /// <summary>
    /// Gets or sets Ribbon Status Bar menu item
    /// </summary>
    public StatusBarItem? StatusBarItem
    {
        get => (StatusBarItem?)this.GetValue(StatusBarItemProperty);
        set => this.SetValue(StatusBarItemProperty, value);
    }

    /// <summary>Identifies the <see cref="StatusBarItem"/> dependency property.</summary>
    public static readonly DependencyProperty StatusBarItemProperty =
        DependencyProperty.Register(nameof(StatusBarItem), typeof(StatusBarItem), typeof(StatusBarMenuItem), new PropertyMetadata());

    #endregion

    #region Constructors

    /// <summary>
    /// Static constructor
    /// </summary>
    static StatusBarMenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusBarMenuItem), new FrameworkPropertyMetadata(typeof(StatusBarMenuItem)));
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="item">Ribbon Status Bar menu item</param>
    public StatusBarMenuItem(StatusBarItem item)
    {
        this.StatusBarItem = item;

        // The template draws the item's title, but the menu item has no Header of its own: without this a screen reader
        // announces every entry as "menu item, checked" with no label.
        this.SetBinding(AutomationProperties.NameProperty, new Binding(nameof(Fluent.StatusBarItem.Title)) { Source = item, Mode = BindingMode.OneWay });
    }

    internal StatusBarMenuItem()
    {
    }

    #endregion
}