namespace Fluent.Tests.Converters;

using System.Globalization;
using Fluent.Converters;
using NUnit.Framework;

[TestFixture]
public class SpinnerTextToValueConverterTests
{
    [Test]
    public void TextToDouble_Keeps_Sign_When_Culture_Uses_Unicode_Minus()
    {
        // Several cultures (e.g. sv-SE, nb-NO, fi-FI with ICU culture data) format negative numbers
        // with U+2212 MINUS SIGN instead of ASCII '-'. We force that here so the test does not depend on the OS culture data.
        var culture = (CultureInfo)new CultureInfo("sv-SE").Clone();
        culture.NumberFormat.NegativeSign = "−";

        var text = (-5d).ToString("F1", culture);

        Assert.That(text, Does.StartWith("−"), "Precondition: formatted text must start with U+2212.");

        var value = SpinnerTextToValueConverter.DefaultInstance.TextToDouble(text, "F1", 0, culture);

        Assert.That(value, Is.EqualTo(-5));
    }

    // Format "P0" shows 0.6 as "60 %". Typing "60 %" must give 0.6 back, not 60 (which would then be shown as "6,000 %").
    [TestCase("en-US", "P0", "60 %")]
    [TestCase("en-US", "P0", "60%")]
    [TestCase("en-US", "P1", "60.0 %")]
    [TestCase("en-US", "P0", "60")]
    [TestCase("de-DE", "P0", "60 %")]
    [TestCase("de-DE", "P0", "60\u00A0%")]
    [TestCase("de-DE", "P0", "60%")]
    [TestCase("de-DE", "P1", "60,0\u00A0%")]
    public void TextToDouble_Percent_Format_Divides_By_100(string cultureName, string format, string text)
    {
        var culture = new CultureInfo(cultureName);

        var value = SpinnerTextToValueConverter.DefaultInstance.TextToDouble(text, format, 0, culture);

        Assert.That(value, Is.EqualTo(0.6));
    }

    [TestCase("en-US", "P0", 0.5)]
    [TestCase("en-US", "P1", 0.625)]
    [TestCase("de-DE", "P0", 0.5)]
    [TestCase("de-DE", "P1", 0.625)]
    [TestCase("en-US", "0%", 0.5)]
    [TestCase("en-US", "E2", 1500)]
    [TestCase("de-DE", "E2", 1500)]
    [TestCase("en-US", "e3", 0.0015)]
    [TestCase("en-US", "E2", -1500)]
    [TestCase("en-US", "0.00E+00", 1500)]
    public void TextToDouble_Reads_Back_Text_Formatted_By_The_Spinner(string cultureName, string format, double expectedValue)
    {
        var culture = new CultureInfo(cultureName);

        // This is exactly the text the spinner shows for the value (Spinner uses SpinnerTextToValueConverter.DoubleToText).
        var text = SpinnerTextToValueConverter.DefaultInstance.DoubleToText(expectedValue, format, culture);

        var value = SpinnerTextToValueConverter.DefaultInstance.TextToDouble(text, format, 0, culture);

        // .NET Framework's double parsing is not always correctly rounded, so allow a tiny tolerance.
        Assert.That(value, Is.EqualTo(expectedValue).Within(1e-12), $"Text was \"{text}\".");
    }

    [TestCase("en-US", "E2", "1.50E+003", 1500)]
    [TestCase("en-US", "E2", "1.50E-003", 0.0015)]
    [TestCase("en-US", "E2", "-1.50E+003", -1500)]
    [TestCase("en-US", "e2", "1.50e+003", 1500)]
    [TestCase("de-DE", "E2", "1,50E+003", 1500)]
    public void TextToDouble_Exponent_Format_Keeps_Exponent(string cultureName, string format, string text, double expectedValue)
    {
        var culture = new CultureInfo(cultureName);

        var value = SpinnerTextToValueConverter.DefaultInstance.TextToDouble(text, format, 0, culture);

        // .NET Framework's double parsing is not always correctly rounded, so allow a tiny tolerance.
        Assert.That(value, Is.EqualTo(expectedValue).Within(1e-12));
    }

    // Plain number formats must keep their previous behaviour: unknown characters (including '%' and letters) are ignored.
    [TestCase("en-US", "F1", "1,234.5", 1234.5)]
    [TestCase("de-DE", "F1", "1.234,5", 1234.5)]
    [TestCase("en-US", "F1", "50%", 50)]
    [TestCase("en-US", "F1", "12 e", 12)]
    [TestCase("en-US", "N0", "1e3", 13)]
    [TestCase("en-US", "F1", "-5.0", -5)]
    public void TextToDouble_Plain_Format_Behaviour_Is_Unchanged(string cultureName, string format, string text, double expectedValue)
    {
        var culture = new CultureInfo(cultureName);

        var value = SpinnerTextToValueConverter.DefaultInstance.TextToDouble(text, format, 0, culture);

        Assert.That(value, Is.EqualTo(expectedValue));
    }
}
