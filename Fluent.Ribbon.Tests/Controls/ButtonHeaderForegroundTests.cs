namespace Fluent.Tests.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// On the hover and pressed fills the button templates switch the header text to the theme colors
/// Button.MouseOver.Foreground and Button.Pressed.Foreground, so the text stays readable.
/// That replaced every text color, also one the application gave the button itself
/// (for example a red "Delete" button lost its red while hovered or pressed).
/// The theme colors should only be used for buttons whose text color is the theme's default.
/// </summary>
[TestFixture]
public class ButtonHeaderForegroundTests
{
    private const string HeaderPartName = "PART_HeaderContentHost";
    private const string MouseOverForegroundKey = "Fluent.Ribbon.Brushes.Button.MouseOver.Foreground";
    private const string PressedForegroundKey = "Fluent.Ribbon.Brushes.Button.Pressed.Foreground";

    private static readonly object[] ControlCases =
    {
        new object[] { nameof(Fluent.Button), false },
        new object[] { nameof(Fluent.Button), true },
        new object[] { nameof(Fluent.ToggleButton), false },
        new object[] { nameof(Fluent.ToggleButton), true },
    };

    [TestCaseSource(nameof(ControlCases))]
    public void Pressed_header_keeps_foreground_set_by_application(string controlType, bool isSimplified)
    {
        var control = CreateControl(controlType);
        control.Foreground = Brushes.Red;

        using (new TestRibbonWindow(control))
        {
            Prepare(control, isSimplified);

            SetIsPressed(control);

            Assert.That(GetHeader(control).Foreground, Is.SameAs(Brushes.Red), "The pressed header should keep the text color the application set on the button");
        }
    }

    [TestCaseSource(nameof(ControlCases))]
    public void Pressed_header_keeps_foreground_set_by_application_style(string controlType, bool isSimplified)
    {
        var control = CreateControl(controlType);

        // An application style based on the Fluent style, which only changes the text color.
        var fluentStyle = (Style)Application.Current.FindResource(control.GetType());
        control.Style = new Style(control.GetType(), fluentStyle)
        {
            Setters =
            {
                new Setter(Control.ForegroundProperty, Brushes.Red)
            }
        };

        using (new TestRibbonWindow(control))
        {
            Prepare(control, isSimplified);

            Assert.That(control.Foreground, Is.SameAs(Brushes.Red), "Precondition: the application style sets the text color");

            SetIsPressed(control);

            Assert.That(GetHeader(control).Foreground, Is.SameAs(Brushes.Red), "The pressed header should keep the text color set by the application's style");
        }
    }

    [TestCaseSource(nameof(ControlCases))]
    public void Pressed_header_of_default_button_uses_theme_pressed_foreground(string controlType, bool isSimplified)
    {
        var control = CreateControl(controlType);

        using (new TestRibbonWindow(control))
        {
            Prepare(control, isSimplified);

            SetIsPressed(control);

            var expected = control.TryFindResource(PressedForegroundKey);
            Assert.That(expected, Is.InstanceOf<Brush>(), "Precondition: the theme defines " + PressedForegroundKey);
            Assert.That(expected, Is.Not.SameAs(control.Foreground), "Precondition: the theme's pressed text color is a different brush than the normal text color");

            Assert.That(GetHeader(control).Foreground, Is.SameAs(expected), "A button without its own text color should use the theme's pressed text color");
        }
    }

    // IsMouseOver is maintained by WPF's mouse input and can't be set from a test without moving the real mouse,
    // which the hidden test window can't rely on. These tests therefore evaluate the template's real triggers
    // against the live control, with only IsMouseOver taken as true, and report the header text color the
    // last matching trigger applies (later template triggers win), exactly like WPF picks it.
    [TestCaseSource(nameof(ControlCases))]
    public void MouseOver_header_keeps_foreground_set_by_application(string controlType, bool isSimplified)
    {
        var control = CreateControl(controlType);
        control.Foreground = Brushes.Red;

        using (new TestRibbonWindow(control))
        {
            Prepare(control, isSimplified);

            Assert.That(GetHeaderForegroundWhileMouseOver(control), Is.SameAs(Brushes.Red), "The hovered header should keep the text color the application set on the button");
        }
    }

    [TestCaseSource(nameof(ControlCases))]
    public void MouseOver_header_of_default_button_uses_theme_mouse_over_foreground(string controlType, bool isSimplified)
    {
        var control = CreateControl(controlType);

        using (new TestRibbonWindow(control))
        {
            Prepare(control, isSimplified);

            var expected = control.TryFindResource(MouseOverForegroundKey);
            Assert.That(expected, Is.InstanceOf<Brush>(), "Precondition: the theme defines " + MouseOverForegroundKey);
            Assert.That(expected, Is.Not.SameAs(control.Foreground), "Precondition: the theme's hover text color is a different brush than the normal text color");

            Assert.That(GetHeaderForegroundWhileMouseOver(control), Is.SameAs(expected), "A button without its own text color should use the theme's hover text color");
        }
    }

    private static ButtonBase CreateControl(string controlType)
    {
        return controlType switch
        {
            nameof(Fluent.Button) => new Fluent.Button { Header = "Delete" },
            nameof(Fluent.ToggleButton) => new Fluent.ToggleButton { Header = "Delete" },
            _ => throw new ArgumentOutOfRangeException(nameof(controlType), controlType, null)
        };
    }

    private static void Prepare(ButtonBase control, bool isSimplified)
    {
        ((ISimplifiedStateControl)control).UpdateSimplifiedState(isSimplified);
        control.ApplyTemplate();
        UIHelper.DoEvents();

        var expectedTemplateKey = control is Fluent.Button
            ? (isSimplified ? "Fluent.Ribbon.Templates.Button.Simplified" : "Fluent.Ribbon.Templates.Button")
            : (isSimplified ? "Fluent.Ribbon.Templates.ToggleButton.Simplified" : "Fluent.Ribbon.Templates.RibbonToggleButton");

        Assert.That(control.Template, Is.SameAs(Application.Current.FindResource(expectedTemplateKey)), "Precondition: the control uses " + expectedTemplateKey);
        Assert.That(GetHeader(control).Visibility, Is.EqualTo(Visibility.Visible), "Precondition: the header is shown");
    }

    private static ContentControl GetHeader(ButtonBase control)
    {
        var header = control.Template.FindName(HeaderPartName, control) as ContentControl;
        Assert.That(header, Is.Not.Null, "Precondition: the template has " + HeaderPartName);
        return header;
    }

    private static void SetIsPressed(ButtonBase control)
    {
        // IsPressed has a protected setter, the real value comes from mouse or keyboard input.
        typeof(ButtonBase).GetProperty(nameof(ButtonBase.IsPressed))!.SetValue(control, true);
        UIHelper.DoEvents();

        Assert.That(control.IsPressed, Is.True, "Precondition: the control is pressed");
    }

    private static Brush GetHeaderForegroundWhileMouseOver(ButtonBase control)
    {
        var template = control.Template;

        Assert.That(control.IsMouseOver, Is.False, "Precondition: the mouse is not really over the control");
        Assert.That(control.IsPressed, Is.False, "Precondition: the control is not pressed");

        Brush result = null;
        var foundMouseOverTrigger = false;

        foreach (var triggerBase in template.Triggers)
        {
            List<Condition> conditions;
            SetterBaseCollection setters;

            switch (triggerBase)
            {
                case Trigger trigger:
                    conditions = new List<Condition> { new(trigger.Property, trigger.Value, trigger.SourceName) };
                    setters = trigger.Setters;
                    break;

                case MultiTrigger multiTrigger:
                    conditions = multiTrigger.Conditions.ToList();
                    setters = multiTrigger.Setters;
                    break;

                default:
                    Assert.Fail($"This test can't evaluate a {triggerBase.GetType().Name} in the template.");
                    return null;
            }

            if (conditions.Any(x => x.Property == UIElement.IsMouseOverProperty))
            {
                foundMouseOverTrigger = true;
            }

            if (conditions.All(x => ConditionHoldsWhileMouseOver(control, x)) == false)
            {
                continue;
            }

            foreach (var setter in setters.OfType<Setter>().Where(x => x.TargetName == HeaderPartName && x.Property == Control.ForegroundProperty))
            {
                result = ResolveSetterValue(control, setter.Value);
            }
        }

        Assert.That(foundMouseOverTrigger, Is.True, "Precondition: the template has an IsMouseOver trigger");

        // Without a trigger setting it, the header inherits the control's text color.
        return result ?? control.Foreground;
    }

    private static bool ConditionHoldsWhileMouseOver(ButtonBase control, Condition condition)
    {
        if (condition.Binding is not null)
        {
            Assert.Fail("This test can't evaluate binding conditions in the template.");
        }

        if (string.IsNullOrEmpty(condition.SourceName)
            && condition.Property == UIElement.IsMouseOverProperty)
        {
            return Equals(condition.Value, true);
        }

        var source = string.IsNullOrEmpty(condition.SourceName)
            ? control
            : (DependencyObject)control.Template.FindName(condition.SourceName, control);

        return Equals(source.GetValue(condition.Property), condition.Value);
    }

    private static Brush ResolveSetterValue(ButtonBase control, object value)
    {
        switch (value)
        {
            case Brush brush:
                return brush;

            case DynamicResourceExtension dynamicResource:
                var resource = control.TryFindResource(dynamicResource.ResourceKey);
                Assert.That(resource, Is.InstanceOf<Brush>(), $"Precondition: resource {dynamicResource.ResourceKey} is a brush");
                return (Brush)resource;

            default:
                Assert.Fail($"This test can't evaluate the setter value {value}.");
                return null;
        }
    }
}
