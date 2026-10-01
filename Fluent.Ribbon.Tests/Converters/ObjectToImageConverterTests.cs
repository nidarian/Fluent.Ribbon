namespace Fluent.Tests.Converters;

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Fluent.Converters;
using NUnit.Framework;

[TestFixture]
public class ObjectToImageConverterTests
{
    [Test]
    public void TestDynamicResource()
    {
        var fluentRibbonImagesApplicationmenuResourceKey = (object)"Fluent.Ribbon.Images.ApplicationMenu";

        var expressionType = typeof(ResourceReferenceExpressionConverter).Assembly.GetType("System.Windows.ResourceReferenceExpression");

        var expression = Activator.CreateInstance(expressionType, fluentRibbonImagesApplicationmenuResourceKey);

        var convertedValue = StaticConverters.ObjectToImageConverter.Convert(new object[]
        {
            expression, // value to convert
            new ApplicationMenu() // target visual
        }, null, null, null);

        Assert.That(convertedValue, Is.Not.Null);
        Assert.That(convertedValue, Is.InstanceOf<Image>());

        var convertedImageValue = (Image)convertedValue;
        Assert.That(convertedImageValue.Source, Is.InstanceOf<DrawingImage>());

        var drawingImage = (DrawingImage)convertedImageValue.Source;
        Assert.That(drawingImage.Drawing, Is.InstanceOf<DrawingGroup>());

        var drawingGroup = (DrawingGroup)drawingImage.Drawing;

        Assert.That(drawingGroup.Children.Cast<GeometryDrawing>().Select(x => x.Geometry.ToString()),
            Is.EquivalentTo(((DrawingGroup)((DrawingImage)Application.Current.FindResource(fluentRibbonImagesApplicationmenuResourceKey)).Drawing).Children.Cast<GeometryDrawing>().Select(x => x.Geometry.ToString())));
    }

    private class DummyProvider : IServiceProvider
    {
        object IServiceProvider.GetService(Type serviceType)
        {
            return null;
        }
    }

    [Test]
    public void TestStaticResourceSequnece()
    {
        var fluentRibbonImagesApplicationmenuResourceKey = (object)"Fluent.Ribbon.Images.ApplicationMenu";

        var expressionType = typeof(ResourceReferenceExpressionConverter).Assembly.GetType("System.Windows.ResourceReferenceExpression");

        var expression = Activator.CreateInstance(expressionType, fluentRibbonImagesApplicationmenuResourceKey);

        var converter = new ObjectToImageConverter();

        converter.ProvideValue(new DummyProvider());

        var convertedValue = StaticConverters.ObjectToImageConverter.Convert(new object[]
        {
            expression, // value to convert
            new ApplicationMenu() // target visual
        }, null, null, null);

        Assert.That(convertedValue, Is.Not.Null);
        Assert.That(convertedValue, Is.InstanceOf<Image>());
    }

    [Test]
    public void Convert_Parses_String_Size_Parameter_Culture_Invariant()
    {
        var previousCulture = CultureInfo.CurrentCulture;

        try
        {
            // In de-DE '.' is the group separator, so a culture sensitive parse turns "16.5" into 165.
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            Assert.That(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, Is.EqualTo(","), "Precondition: current culture must use ',' as decimal separator.");

            var imageSource = new DrawingImage(new GeometryDrawing(Brushes.Black, null, new RectangleGeometry(new Rect(0, 0, 32, 32))));

            // A converter parameter given in XAML ("16.5") is always written in invariant format.
            var convertedValue = new ObjectToImageConverter().Convert(imageSource, typeof(object), "16.5", CultureInfo.InvariantCulture);

            Assert.That(convertedValue, Is.InstanceOf<Image>(), "Precondition: converting to object must create an Image with the desired size.");

            var image = (Image)convertedValue;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(image.Width, Is.EqualTo(16.5));
                Assert.That(image.Height, Is.EqualTo(16.5));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }
}
