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

        // Percent formats (e.g. "P0" or "0 %") show the value multiplied by 100 ("50 %" for 0.5),
        // and exponent formats (e.g. "E2") show it as "1.50E+003". Without knowing that, "60 %" would become 60
        // and "1.50E+003" would become 1.50003.
        AnalyzeFormat(format, out var isPercentFormat, out var isExponentFormat);

        // Remove all except digits, signs and commas
        var stringBuilder = new StringBuilder();
        var isNegative = false;
        var hasExponent = false;

        foreach (var symbol in text)
        {
            if (char.IsDigit(symbol)
                || symbol == ','
                || symbol == '.')
            {
                stringBuilder.Append(symbol);
            }
            else if (isExponentFormat
                     && hasExponent == false
                     && stringBuilder.Length > 0
                     && (symbol == 'E' || symbol == 'e'))
            {
                // The exponent marker is only kept for exponent formats, so letters typed into other spinners are still ignored.
                stringBuilder.Append('E');
                hasExponent = true;
            }
            else if (hasExponent
                     && stringBuilder[stringBuilder.Length - 1] == 'E'
                     && IsNegativeSign(symbol, negativeSign))
            {
                // Negative exponent, e.g. "1.50E-003". The culture's sign is used because that is what double.TryParse expects.
                stringBuilder.Append(negativeSign);
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

        if (isPercentFormat)
        {
            doubleValue /= 100;
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

    /// <summary>
    /// Finds out if <paramref name="format"/> multiplies the value by 100 (standard "P" or a custom format with '%')
    /// or shows it in exponent notation (standard "E" or a custom format like "0.0E+0").
    /// Quoted text and escaped characters in custom formats are literals and are ignored.
    /// </summary>
    private static void AnalyzeFormat(string? format, out bool isPercentFormat, out bool isExponentFormat)
    {
        isPercentFormat = false;
        isExponentFormat = false;

        if (string.IsNullOrEmpty(format))
        {
            return;
        }

        if (IsStandardFormat(format!))
        {
            var specifier = char.ToUpperInvariant(format![0]);
            isPercentFormat = specifier == 'P';
            isExponentFormat = specifier == 'E';
            return;
        }

        var quote = '\0';

        for (var i = 0; i < format!.Length; i++)
        {
            var symbol = format[i];

            if (quote != '\0')
            {
                if (symbol == quote)
                {
                    quote = '\0';
                }
            }
            else if (symbol == '\\')
            {
                i++;
            }
            else if (symbol == '\'' || symbol == '"')
            {
                quote = symbol;
            }
            else if (symbol == '%')
            {
                isPercentFormat = true;
            }
            else if ((symbol == 'E' || symbol == 'e')
                     && i + 1 < format.Length)
            {
                var next = format[i + 1] == '+' || format[i + 1] == '-'
                    ? i + 2
                    : i + 1;

                if (next < format.Length
                    && format[next] == '0')
                {
                    isExponentFormat = true;
                }
            }
        }
    }

    /// <summary>
    /// A standard numeric format is one letter, optionally followed by a precision (e.g. "P1", "E2").
    /// </summary>
    private static bool IsStandardFormat(string format)
    {
        if (char.IsLetter(format[0]) == false)
        {
            return false;
        }

        for (var i = 1; i < format.Length; i++)
        {
            if (char.IsDigit(format[i]) == false)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsNegativeSign(char symbol, string negativeSign)
    {
        return symbol == '-'
               || symbol == '\u2212' // MINUS SIGN
               || (negativeSign.Length == 1 && symbol == negativeSign[0]);
    }
}