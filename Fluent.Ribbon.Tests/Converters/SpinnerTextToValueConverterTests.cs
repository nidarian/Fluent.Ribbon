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
}
