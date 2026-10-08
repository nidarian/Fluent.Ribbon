namespace Fluent.Converters;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Fluent.Internal.KnownBoxes;

/// <summary>
/// Checks whether a value is the very object a resource lookup from an element returns.
/// The first value is the element (<see cref="FrameworkElement"/> or <see cref="FrameworkContentElement"/>) the resource is looked up from,
/// the second value is the value to compare and the converter parameter is the resource key.
/// Returns <c>true</c> only if the resource exists and is the same instance as the value.
/// </summary>
/// <remarks>
/// The Fluent button styles use it to find out whether <see cref="System.Windows.Controls.Control.Foreground"/> is still the theme's default text brush
/// (set from the same resource by the style) or was set by the application.
/// </remarks>
public sealed class IsSameAsResourceConverter : IMultiValueConverter
{
    /// <summary>
    /// A singleton instance for <see cref="IsSameAsResourceConverter" />.
    /// </summary>
    public static readonly IsSameAsResourceConverter Instance = new();

    /// <inheritdoc />
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is null
            || values.Length < 2
            || parameter is null)
        {
            return BooleanBoxes.FalseBox;
        }

        var resource = values[0] switch
        {
            FrameworkElement frameworkElement => frameworkElement.TryFindResource(parameter),
            FrameworkContentElement frameworkContentElement => frameworkContentElement.TryFindResource(parameter),
            _ => null
        };

        return BooleanBoxes.Box(resource is not null && ReferenceEquals(resource, values[1]));
    }

    /// <inheritdoc />
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
