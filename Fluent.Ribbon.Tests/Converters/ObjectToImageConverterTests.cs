namespace Fluent.Tests.Converters;

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

    /// <summary>
    /// Icon and LargeIcon of every Fluent control go through this converter (IconPresenter).
    /// A bound path that is empty, points to a missing file or to a file that is not an image
    /// made the converter throw. WPF does not catch exceptions thrown by converters, so an app
    /// with Icon="{Binding IconPath}" crashed. The control should show no icon instead.
    /// </summary>
    [TestCase("")]
    [TestCase(@"C:\does-not-exist\x.png")]
    [TestCase(@"Images\does-not-exist.png")]
    public void Convert_Does_Not_Throw_For_Invalid_Image_Path(string imagePath)
    {
        AssertConvertsToNoIcon(imagePath);
    }

    [Test]
    public void Convert_Does_Not_Throw_For_File_That_Is_Not_An_Image()
    {
        var filePath = Path.Combine(Path.GetTempPath(), "Fluent.Tests." + Guid.NewGuid().ToString("N") + ".png");

        try
        {
            File.WriteAllText(filePath, "This is not an image.");

            AssertConvertsToNoIcon(filePath);
        }
        finally
        {
            TryDeleteFile(filePath);
        }
    }

    [Test]
    public void Convert_Still_Converts_Valid_Image_Path()
    {
        var filePath = Path.Combine(Path.GetTempPath(), "Fluent.Tests." + Guid.NewGuid().ToString("N") + ".png");

        try
        {
            var bitmap = BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgra32, null, new byte[2 * 2 * 4], 2 * 4);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = File.Create(filePath))
            {
                encoder.Save(stream);
            }

            var convertedValue = new ObjectToImageConverter().Convert(filePath, typeof(object), null, CultureInfo.InvariantCulture);

            Assert.That(convertedValue, Is.InstanceOf<Image>());
            Assert.That(((Image)convertedValue).Source, Is.InstanceOf<BitmapSource>());
        }
        finally
        {
            TryDeleteFile(filePath);
        }
    }

    private static void AssertConvertsToNoIcon(string imagePath)
    {
        // A null value converts to null today, which IconPresenter shows as "no icon".
        Assert.That(new ObjectToImageConverter().Convert((object)null, typeof(object), null, CultureInfo.InvariantCulture), Is.Null, "Precondition: null converts to null.");

        object convertedValue = "not converted";

        Assert.That(() => convertedValue = new ObjectToImageConverter().Convert(imagePath, typeof(object), null, CultureInfo.InvariantCulture), Throws.Nothing);

        // Returning the path itself would make IconPresenter show the path as text.
        Assert.That(convertedValue, Is.Null);
    }

    private static void TryDeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (IOException)
        {
            // The decoder may still hold the file open (BitmapCacheOption.Default), so it stays in the temp folder.
        }
    }
}
