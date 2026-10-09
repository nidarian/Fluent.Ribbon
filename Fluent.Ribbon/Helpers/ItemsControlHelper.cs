namespace Fluent.Helpers;

using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Fluent.Internal.KnownBoxes;

internal static class ItemsControlHelper
{
    public static readonly DependencyProperty IsMovingItemsToDifferentControlProperty = DependencyProperty.RegisterAttached(
        "IsMovingItemsToDifferentControl", typeof(bool), typeof(ItemsControlHelper), new PropertyMetadata(BooleanBoxes.FalseBox));

    // Holds the ItemsSource binding of a control while its items are shown by a different control (for example a quick access copy),
    // so the binding can be restored when the items are moved back.
    // The ConditionalWeakTable does not keep the control alive.
    private static readonly ConditionalWeakTable<ItemsControl, BindingBase> MovedItemsSourceBindings = new();

    public static void SetIsMovingItemsToDifferentControl(DependencyObject element, bool value)
    {
        element.SetValue(IsMovingItemsToDifferentControlProperty, BooleanBoxes.Box(value));
    }

    public static bool GetIsMovingItemsToDifferentControl(DependencyObject element)
    {
        return (bool)element.GetValue(IsMovingItemsToDifferentControlProperty);
    }

    public static ItemsControl? ItemsControlFromItemContainer(DependencyObject? container)
    {
        if (container is null)
        {
            return null;
        }

        var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);

        if (itemsControl is not null
            && itemsControl != DependencyProperty.UnsetValue)
        {
            return itemsControl;
        }

        var visualParent = VisualTreeHelper.GetParent(container);
        if (visualParent is not null)
        {
            itemsControl = ItemsControl.ItemsControlFromItemContainer(visualParent);
        }

        if (itemsControl is not null
            && itemsControl != DependencyProperty.UnsetValue)
        {
            return itemsControl;
        }

        if (container is FrameworkElement { Parent: { } } frameworkElement)
        {
            itemsControl = ItemsControl.ItemsControlFromItemContainer(frameworkElement.Parent);
        }

        return itemsControl;
    }

    public static void MoveItemsToDifferentControl(ItemsControl source, ItemsControl target)
    {
        try
        {
            SetIsMovingItemsToDifferentControl(source, true);
            SetIsMovingItemsToDifferentControl(target, true);

            var itemsSource = source.ItemsSource;
            if (itemsSource is not null)
            {
                // Setting ItemsSource replaces a binding (for example ItemsSource="{Binding Fonts}") with a fixed value.
                // Remember the binding of the source, so it still follows its view model once the items are moved back.
                var itemsSourceBinding = BindingOperations.GetBindingBase(source, ItemsControl.ItemsSourceProperty);

                source.ItemsSource = null;

                if (itemsSourceBinding is not null)
                {
                    MovedItemsSourceBindings.Remove(source);
                    MovedItemsSourceBindings.Add(source, itemsSourceBinding);
                }

                if (MovedItemsSourceBindings.TryGetValue(target, out var targetItemsSourceBinding))
                {
                    // The items come back to the control they were bound on: restore its binding instead of a fixed value.
                    MovedItemsSourceBindings.Remove(target);
                    BindingOperations.SetBinding(target, ItemsControl.ItemsSourceProperty, targetItemsSourceBinding);
                }
                else
                {
                    target.ItemsSource = itemsSource;
                }
            }
            else
            {
                while (source.Items.Count > 0)
                {
                    var item = source.Items[0];
                    source.Items.Remove(item);
                    target.Items.Add(item);
                }
            }
        }
        finally
        {
            SetIsMovingItemsToDifferentControl(source, false);
            SetIsMovingItemsToDifferentControl(target, false);
        }
    }
}