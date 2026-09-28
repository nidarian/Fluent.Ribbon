#pragma warning disable SA1402 // File may only contain a single class
// ReSharper disable once CheckNamespace
namespace Fluent;

using System;
using System.Collections;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Fluent.Internal.KnownBoxes;

/// <summary>
/// This interface must be implemented for controls
/// which are intended to insert to quick access toolbar
/// </summary>
public interface IQuickAccessItemProvider
{
    /// <summary>
    /// Gets control which represents shortcut item.
    /// This item MUST be syncronized with the original
    /// and send command to original one control.
    /// </summary>
    /// <returns>Control which represents shortcut item</returns>
    FrameworkElement CreateQuickAccessItem();

    /// <summary>
    /// Gets or sets a value indicating whether control can be added to quick access toolbar
    /// </summary>
    bool CanAddToQuickAccessToolBar { get; set; }
}

/// <summary>
/// Peresents quick access shortcut to another control
/// </summary>
[ContentProperty(nameof(Target))]
public class QuickAccessMenuItem : MenuItem
{
    #region Fields

    private Ribbon? ribbon;

    /// <summary>
    /// Gets or sets the ribbon owning this item.
    /// Set by <see cref="Fluent.Ribbon.QuickAccessItems"/> when the item is added or removed.
    /// </summary>
    internal Ribbon? Ribbon
    {
        get => this.ribbon;
        set
        {
            this.ribbon = value;

            // Items declared in XAML get added to the toolbar through OnFirstLoaded, because they
            // receive their Loaded event together with the ribbon.
            // Items added from code after the ribbon is live only get Loaded once the quick access
            // menu is opened, so a checked item would stay hidden until then (#1251).
            // Only do this once the toolbar exists. Registering the element earlier would make the
            // Loaded logic believe it's already shown, and it would never be added to the toolbar.
            if (value?.QuickAccessToolBar is not null
                && this.IsChecked)
            {
                value.AddToQuickAccessToolBar(this.Target);
            }
        }
    }

    #endregion

    #region Initialization

    static QuickAccessMenuItem()
    {
        IsCheckableProperty.AddOwner(typeof(QuickAccessMenuItem), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public QuickAccessMenuItem()
    {
        this.Checked += this.OnChecked;
        this.Unchecked += this.OnUnchecked;
        this.Loaded += this.OnFirstLoaded;
        this.Loaded += this.OnItemLoaded;
    }

    #endregion

    #region Target Property

    /// <summary>
    /// Gets or sets shortcut to the target control
    /// </summary>
    public UIElement? Target
    {
        get => (UIElement?)this.GetValue(TargetProperty);
        set => this.SetValue(TargetProperty, value);
    }

    /// <summary>Identifies the <see cref="Target"/> dependency property.</summary>
    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(UIElement), typeof(QuickAccessMenuItem), new PropertyMetadata(OnTargetChanged));

    private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var quickAccessMenuItem = (QuickAccessMenuItem)d;
        var ribbonControl = e.NewValue as IRibbonControl;

        if (quickAccessMenuItem.Header is null
            && ribbonControl is not null)
        {
            // Set Default Text Value
            RibbonControl.Bind(ribbonControl, quickAccessMenuItem, nameof(IRibbonControl.Header), HeaderProperty, BindingMode.OneWay);
        }

        if (ribbonControl is not null)
        {
            var parent = LogicalTreeHelper.GetParent((DependencyObject)ribbonControl);
            if (parent is null)
            {
                quickAccessMenuItem.AddLogicalChild(ribbonControl);
            }
        }

        if (e.OldValue is IRibbonControl oldRibbonControl)
        {
            var parent = LogicalTreeHelper.GetParent((DependencyObject)oldRibbonControl);
            if (ReferenceEquals(parent, quickAccessMenuItem))
            {
                quickAccessMenuItem.RemoveLogicalChild(oldRibbonControl);
            }
        }
    }

    #endregion

    #region Overrides

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren
    {
        get
        {
            var baseEnumerator = base.LogicalChildren;
            while (baseEnumerator?.MoveNext() == true)
            {
                yield return baseEnumerator.Current;
            }

            if (this.Target is not null)
            {
                var parent = LogicalTreeHelper.GetParent(this.Target);
                if (ReferenceEquals(parent, this))
                {
                    yield return this.Target;
                }
            }
        }
    }

    #endregion

    #region Event Handlers

    private void OnChecked(object sender, RoutedEventArgs e)
    {
        this.Ribbon?.AddToQuickAccessToolBar(this.Target);
    }

    private void OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (this.IsLoaded == false)
        {
            return;
        }

        this.Ribbon?.RemoveFromQuickAccessToolBar(this.Target);
    }

    private void OnItemLoaded(object sender, RoutedEventArgs e)
    {
        if (this.IsLoaded == false)
        {
            return;
        }

        if (this.Ribbon is not null)
        {
            this.IsChecked = this.Ribbon.IsInQuickAccessToolBar(this.Target);
        }
    }

    private void OnFirstLoaded(object sender, RoutedEventArgs e)
    {
        this.Loaded -= this.OnFirstLoaded;

        if (this.IsChecked)
        {
            this.Ribbon?.AddToQuickAccessToolBar(this.Target);
        }
    }

    #endregion
}

/// <summary>
/// The class responds to mine controls for QuickAccessToolBar
/// </summary>
internal static class QuickAccessItemsProvider
{
    #region Public Methods

    /// <summary>
    /// Determines whether the given control can provide a quick access toolbar item
    /// </summary>
    /// <param name="element">Control</param>
    /// <returns>True if this control is able to provide
    /// a quick access toolbar item, false otherwise</returns>
    public static bool IsSupported(UIElement? element)
    {
        if (element is IQuickAccessItemProvider provider
            && provider.CanAddToQuickAccessToolBar)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets control which represents quick access toolbar item
    /// </summary>
    /// <param name="element">Host control</param>
    /// <returns>Control which represents quick access toolbar item</returns>
    public static FrameworkElement? GetQuickAccessItem(UIElement element)
    {
        FrameworkElement? result = null;

        // If control supports the interface just return what it provides
        if (element is IQuickAccessItemProvider provider
            && provider.CanAddToQuickAccessToolBar)
        {
            result = provider.CreateQuickAccessItem();
        }

        // The control isn't supported
        if (result is null)
        {
            throw new ArgumentException("The contol " + element.GetType().Name + " is not able to provide a quick access toolbar item");
        }

        RibbonProperties.SetIsElementInQuickAccessToolBar(result, true);

        if (BindingOperations.IsDataBound(result, UIElement.VisibilityProperty) == false)
        {
            RibbonControl.Bind(element, result, nameof(UIElement.Visibility), UIElement.VisibilityProperty, BindingMode.OneWay);
        }

        if (BindingOperations.IsDataBound(result, UIElement.IsEnabledProperty) == false)
        {
            RibbonControl.Bind(element, result, nameof(UIElement.IsEnabled), UIElement.IsEnabledProperty, BindingMode.OneWay);
        }

        if (result.TryFindResource("Fluent.Ribbon.Styles.FocusVisual") is Style tightFocusVisual)
        {
            result.FocusVisualStyle = tightFocusVisual;
        }

        return result;
    }

    /// <summary>
    /// Finds the top supported control
    /// </summary>
    public static FrameworkElement? FindSupportedControl(Visual visual, Point point)
    {
        var result = VisualTreeHelper.HitTest(visual, point);
        if (result is null)
        {
            return null;
        }

        // Try to find in visual (or logical) tree
        var element = result.VisualHit as FrameworkElement;
        while (element is not null)
        {
            if (IsSupported(element))
            {
                return element;
            }

            var visualParent = VisualTreeHelper.GetParent(element) as FrameworkElement;
            var logicalParent = LogicalTreeHelper.GetParent(element) as FrameworkElement;
            element = visualParent ?? logicalParent;
        }

        return null;
    }

    #endregion
}