namespace Fluent.Tests.Controls;

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NUnit.Framework;

/// <summary>
/// The spinner shows where keyboard focus is with an IsKeyboardFocusWithin trigger (focus border/background).
/// The simplified template (used in the simplified ribbon) lacked that trigger, so a focused simplified spinner looked unfocused.
/// </summary>
[TestFixture]
public class SpinnerTemplateTests
{
    [Test]
    public void Simplified_template_highlights_keyboard_focus()
    {
        // Baseline: the normal template has the trigger, which also proves the lookup below finds it.
        Assert.That(HasKeyboardFocusWithinTrigger("Fluent.Ribbon.Templates.Spinner"), Is.True, "precondition: the normal spinner template should have an IsKeyboardFocusWithin trigger");

        Assert.That(HasKeyboardFocusWithinTrigger("Fluent.Ribbon.Templates.Spinner.Simplified"), Is.True, "the simplified spinner template should highlight keyboard focus like the normal one");
    }

    private static bool HasKeyboardFocusWithinTrigger(string templateKey)
    {
        // The theme resources are merged into the application resources in AssemblySetup.
        var template = Application.Current.FindResource(templateKey) as ControlTemplate;
        Assert.That(template, Is.Not.Null, $"precondition: theme resource {templateKey} should be a ControlTemplate");

        return template.Triggers
            .OfType<Trigger>()
            .Any(x => x.Property == UIElement.IsKeyboardFocusWithinProperty && Equals(x.Value, true));
    }
}
