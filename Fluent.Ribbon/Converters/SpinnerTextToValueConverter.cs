namespace Fluent.Converters;

using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

/// <summary>
/// Converter class which converts from <see cref="string"/> to <see cref="double"/> and back.
/// </summary>
public class SpinnerTextToValueConverter : IValueConverter
{
    /// <summary>
    /// Gets a default instance of <see cref="SpinnerTextToValueConverter"/>.
    /// </summary>
    public static readonly SpinnerTextToValueConverter DefaultInstance = new();

    /// <inheritdoc />
    public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var converterParam = (Tuple<string, double>)parameter;
        var format = converterParam.Item1;
        var previousValue = converterParam.Item2;

        return this.TextToDouble((string)value, format, previousValue, culture);
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return this.DoubleToText((double)value, (string)parameter, culture);
    }

    /// <summary>
    /// Converts the given <paramref name="text"/> to a <see cref="double"/>.
    /// </summary>
    /// <returns>The <see cref="double"/> value converted from <paramref name="text"/> or <paramref name="previousValue"/> if the conversion fails.</returns>
    public virtual double TextToDouble(string text, string format, double previousValue, CultureInfo culture)
    {
        // Some cultures (e.g. sv-SE, nb-NO, fi-FI with ICU data) use U+2212 MINUS SIGN as negative sign.
        // A plain '-' check would drop that sign and turn "−5,0" into 5.
        var negativeSign = NumberFormatInfo.GetInstance(culture).NegativeSign;

        // Remove all except digits, signs and commas
        var stringBuilder = new StringBuilder();
        var isNegative = false;

        foreach (var symbol in text)
        {
            if (char.IsDigit(symbol)
                || symbol == ','
                || symbol == '.')
            {
                stringBuilder.Append(symbol);
            }
            else if (stringBuilder.Length == 0
                     && isNegative == false
                     && IsNegativeSign(symbol, negativeSign))
            {
                // The sign is remembered instead of appended, because double.TryParse on .NET Framework
                // does not accept ASCII '-' for a culture whose NegativeSign is U+2212 (and vice versa).
                isNegative = true;
            }
        }

        text = stringBuilder.ToString();

        if (double.TryParse(text, NumberStyles.Any, culture, out var doubleValue) == false)
        {
            return previousValue;
        }

        return isNegative
            ? -doubleValue
            : doubleValue;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a formatted text using <paramref name="format"/>.
    /// </summary>
    /// <returns><paramref name="value"/> converted to a <see cref="string"/>.</returns>
    public virtual string DoubleToText(double value, string format, CultureInfo culture)
    {
        return value.ToString(format, culture);
    }

    private static bool IsNegativeSign(char symbol, string negativeSign)
    {
        return symbol == '-'
               || symbol == '\u2212' // MINUS SIGN
               || (negativeSign.Length == 1 && symbol == negativeSign[0]);
    }
}