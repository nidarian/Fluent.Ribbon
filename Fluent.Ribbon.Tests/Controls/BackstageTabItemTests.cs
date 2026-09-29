namespace Fluent.Tests.Controls;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// https://github.com/fluentribbon/Fluent.Ribbon/issues/1265
/// Apps asked to be able to recolor the keyboard focus frame of backstage tab items.
/// It used the general Fluent.Ribbon.Brushes.Black, so it couldn't be changed on its own.
/// </summary>
[TestFixture]
public class BackstageTabItemTests
{
    private const string FocusBorderKey = "Fluent.Ribbon.Brushes.BackstageTabItem.Focus.Border";

    [TestCase("Light.Blue")]
    [TestCase("Dark.Blue")]
    public void Focus_border_brush_exists_and_looks_the_same_as_before(string themeName)
    {
        var theme = (ResourceDictionary)Application.LoadComponent(new Uri($"/Fluent;component/Themes/Themes/{themeName}.xaml", UriKind.Relative));

        var focusBorder = theme[FocusBorderKey] as SolidColorBrush;
        var previousBrush = theme["Fluent.Ribbon.Brushes.Black"] as SolidColorBrush;

        Assert.That(focusBorder, Is.Not.Null, $"{themeName} should define {FocusBorderKey}");
        Assert.That(previousBrush, Is.Not.Null);

        // Adding the key must not change how anything looks: it defaults to the color used before.
        Assert.That(focusBorder.Color, Is.EqualTo(previousBrush.Color));
    }

    [Test]
    public void Focus_frame_uses_the_focus_border_brush()
    {
        var tabItem = new BackstageTabItem { Header = "Info" };

        using (new TestRibbonWindow(tabItem))
        {
            tabItem.ApplyTemplate();

            // The focus frame is drawn by a trigger in the control template that sets rootBorder's border brush.
            var focusTrigger = tabItem.Template.Triggers
                .OfType<Trigger>()
                .Single(x => x.Property == UIElement.IsKeyboardFocusedProperty);

            var setter = focusTrigger.Setters
                .OfType<Setter>()
                .Single(x => x.TargetName == "rootBorder" && x.Property.Name == "BorderBrush");

            // DynamicResource, so apps can override the key in their own resources.
            Assert.That(setter.Value, Is.InstanceOf<DynamicResourceExtension>());
            Assert.That(((DynamicResourceExtension)setter.Value).ResourceKey, Is.EqualTo(FocusBorderKey));
        }
    }
}
