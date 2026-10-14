namespace Fluent.Tests.Controls;

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// The default style of <see cref="ApplicationMenu"/> sets the header with
/// <c>{converters:ObjectToImageConverter {DynamicResource Fluent.Ribbon.Images.ApplicationMenu}, ...}</c>.
/// That style exists once per process, so the ResourceReferenceExpression created for the DynamicResource
/// is one single object shared by every application menu.
/// <see cref="Converters.ObjectToImageConverter"/> resolved that expression by writing the first menu it saw into the
/// shared expression (its private "_targetObject") and calling its GetValue. WPF then caches that menu as "mentor"
/// and the found image inside the shared expression for the rest of the process.
/// Result: the first menu (and with it its whole window) can never be collected, and every later menu shows
/// the image found for that first menu, ignoring a "Fluent.Ribbon.Images.ApplicationMenu" resource of its own window.
/// </summary>
[TestFixture]
public class ApplicationMenuHeaderTests
{
    private const string ApplicationMenuImageResourceKey = "Fluent.Ribbon.Images.ApplicationMenu";

    [Test]
    public void Header_uses_the_application_menu_image_of_its_own_window()
    {
        // Show (and close) a window with a default application menu first, so the shared style has been used at least once,
        // no matter which tests ran before.
        CreateWindow(out _, null).Dispose();

        var overrideImage = new DrawingImage(new GeometryDrawing(Brushes.Red, null, new RectangleGeometry(new Rect(0, 0, 40, 16))));
        overrideImage.Freeze();

        using (CreateWindow(out var menu, overrideImage))
        {
            Assert.That(menu.Header, Is.InstanceOf<Image>(), "Precondition: the default style shows an image as header");
            Assert.That(((Image)menu.Header).Source, Is.SameAs(overrideImage), "The header must show the application menu image defined in the menu's own window");
        }
    }

    [Test]
    public void Showing_an_application_menu_does_not_store_it_in_the_shared_style()
    {
        var expression = GetSharedHeaderResourceExpression();

        using (CreateWindow(out var menu, null))
        {
            Assert.That(BindingOperations.GetMultiBinding(menu, DropDownButton.HeaderProperty), Is.SameAs(GetSharedHeaderMultiBinding()), "Precondition: the menu uses the header binding of the shared style");
            Assert.That(menu.Header, Is.InstanceOf<Image>(), "Precondition: the default style shows an image as header");
        }

        Assert.That(GetField(expression, "_targetObject"), Is.Null, "Target object of the shared resource expression");
        Assert.That(GetField(expression, "_mentorCache"), Is.Null, "Mentor of the shared resource expression");
    }

    [Test]
    public void Closed_window_with_application_menu_is_collected()
    {
        var expression = GetSharedHeaderResourceExpression();

        // If any earlier test already left its menu in the shared expression, that menu (and its window) is never freed:
        // that is the same bug, so fail here instead of giving a result that depends on the order of the tests.
        Assert.That(GetField(expression, "_targetObject"), Is.Null, "Precondition: no menu shown before is kept by the shared resource expression");

        var window = CreateShowAndCloseWindow();

        Assert.That(IsAliveAfterGarbageCollection(window), Is.False, "A closed window with an application menu should be collected");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateShowAndCloseWindow()
    {
        using (var window = CreateWindow(out var menu, null))
        {
            Assert.That(menu.Header, Is.InstanceOf<Image>(), "Precondition: the default style shows an image as header");

            return new WeakReference(window);
        }
    }

    private static TestRibbonWindow CreateWindow(out ApplicationMenu menu, ImageSource applicationMenuImage)
    {
        var content = new Grid();

        if (applicationMenuImage is not null)
        {
            content.Resources[ApplicationMenuImageResourceKey] = applicationMenuImage;
        }

        var ribbon = new Ribbon
        {
            // Don't load a persisted (maybe minimized) state.
            AutomaticStateManagement = false
        };

        content.Children.Add(ribbon);

        menu = new ApplicationMenu();
        ribbon.Menu = menu;

        var window = new TestRibbonWindow(content);

        UIHelper.DoEvents();

        Assert.That(PresentationSource.FromVisual(menu), Is.Not.Null, "Precondition: the application menu is shown");

        return window;
    }

    private static MultiBinding GetSharedHeaderMultiBinding()
    {
        var style = (Style)Application.Current.FindResource(typeof(ApplicationMenu));

        for (; style is not null; style = style.BasedOn)
        {
            var setter = style.Setters.OfType<Setter>().FirstOrDefault(x => x.Property == DropDownButton.HeaderProperty);

            if (setter is not null)
            {
                Assert.That(setter.Value, Is.InstanceOf<MultiBinding>(), "Precondition: the style sets the header with a MultiBinding");

                return (MultiBinding)setter.Value;
            }
        }

        Assert.Fail("Precondition: the style of ApplicationMenu sets the header");
        return null;
    }

    private static object GetSharedHeaderResourceExpression()
    {
        var multiBinding = GetSharedHeaderMultiBinding();

        Assert.That(multiBinding.Bindings[0], Is.InstanceOf<Binding>(), "Precondition: the first binding is the image");

        var expression = ((Binding)multiBinding.Bindings[0]).Source;

        Assert.That(expression?.GetType().Name, Is.EqualTo("ResourceReferenceExpression"), "Precondition: the image is a DynamicResource");

        return expression;
    }

    private static object GetField(object instance, string name)
    {
        var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null, $"Precondition: field {name} exists");

        return field.GetValue(instance);
    }

    private static bool IsAliveAfterGarbageCollection(WeakReference reference)
    {
        for (var i = 0; i < 5 && reference.IsAlive; i++)
        {
            // Let pending dispatcher work (layout, deferred cleanup of the closed window) finish first.
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
            Thread.Sleep(50);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        return reference.IsAlive;
    }
}
