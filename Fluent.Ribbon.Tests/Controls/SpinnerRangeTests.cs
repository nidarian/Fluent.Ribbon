namespace Fluent.Tests.Controls;

using NUnit.Framework;

[TestFixture]
public class SpinnerRangeTests
{
    [Test]
    public void Minimum_Is_Recoerced_When_Maximum_Grows()
    {
        var spinner = new Spinner { Maximum = 5 };

        spinner.Minimum = 10;

        Assert.That(spinner.Minimum, Is.EqualTo(5), "Precondition: Minimum is coerced to Maximum.");

        spinner.Maximum = 20;

        // The requested Minimum (10) fits into the new range, so it must be restored.
        Assert.That(spinner.Minimum, Is.EqualTo(10));
    }

    [Test]
    public void Maximum_Is_Recoerced_When_Minimum_Shrinks()
    {
        var spinner = new Spinner { Minimum = 10 };

        spinner.Maximum = 5;

        Assert.That(spinner.Maximum, Is.EqualTo(10), "Precondition: Maximum is coerced to Minimum.");

        spinner.Minimum = 0;

        // The requested Maximum (5) fits into the new range, so it must be restored.
        Assert.That(spinner.Maximum, Is.EqualTo(5));
    }
}
