namespace Fluent.Tests.Controls;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class RibbonTests
{
    [Test]
    public void DependencyProperties_and_DataContext_should_be_inherited_from_window()
    {
        var ribbon = new Ribbon
        {
            Menu = new Backstage(),
            StartScreen = new StartScreen()
        };

        var enUs = XmlLanguage.GetLanguage("en-US");
        var deDe = XmlLanguage.GetLanguage("de-DE");

        using (var window = new TestRibbonWindow(ribbon)
               {
                   Language = deDe,
                   DataContext = deDe
               })
        {
            ribbon.ApplyTemplate();

            var elemens = new Dictionary<FrameworkElement, string>
            {
                { ribbon, "Ribbon" },
                { ribbon.Menu, "Menu" },
                { ribbon.StartScreen, "StartScreen" },
                { ribbon.QuickAccessToolBar, "QuickAccessToolBar" },
                { ribbon.TabControl, "TabControl" },
                { (FrameworkElement)ribbon.Template.FindName("PART_LayoutRoot", ribbon), "PART_LayoutRoot" },
            };

            {
                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.DataContextProperty, window);
                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.LanguageProperty, window);
            }

            {
                window.Language = enUs;
                window.DataContext = window.Language;

                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.DataContextProperty, window);
                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.LanguageProperty, window);
            }

            {
                window.Language = deDe;
                window.DataContext = window.Language;

                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.DataContextProperty, window);
                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.LanguageProperty, window);
            }

            {
                window.Language = enUs;
                window.DataContext = window.Language;

                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.DataContextProperty, window);
                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.LanguageProperty, window);
            }
        }
    }

    [Test]
    public void DependencyProperties_and_DataContext_should_be_inherited_from_ribbon()
    {
        var ribbon = new Ribbon
        {
            Menu = new Backstage(),
            StartScreen = new StartScreen()
        };

        var enUs = XmlLanguage.GetLanguage("en-US");
        var deDe = XmlLanguage.GetLanguage("de-DE");

        using (var window = new TestRibbonWindow(ribbon)
               {
                   Language = deDe,
                   DataContext = deDe
               })
        {
            ribbon.ApplyTemplate();

            var elemens = new Dictionary<FrameworkElement, string>
            {
                { ribbon, "Ribbon" },
                { ribbon.Menu, "Menu" },
                { ribbon.StartScreen, "StartScreen" },
                { ribbon.QuickAccessToolBar, "QuickAccessToolBar" },
                { ribbon.TabControl, "TabControl" },
                { (FrameworkElement)ribbon.Template.FindName("PART_LayoutRoot", ribbon), "PART_LayoutRoot" },
            };

            {
                Assert.That(ribbon.Language, Is.EqualTo(window.Language), "Language on Window should match.");
                Assert.That(ribbon.DataContext, Is.EqualTo(window.DataContext), "DataContext on Window should match.");

                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.DataContextProperty, ribbon);
                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.LanguageProperty, ribbon);
            }

            {
                ribbon.Language = enUs;
                ribbon.DataContext = ribbon.Language;

                Assert.That(ribbon.Language, Is.Not.EqualTo(window.Language), "Language on Ribbon should not match Window.");
                Assert.That(ribbon.DataContext, Is.Not.EqualTo(window.DataContext), "DataContext on Ribbon should not match Window.");

                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.DataContextProperty, ribbon);
                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.LanguageProperty, ribbon);
            }

            {
                ribbon.Language = deDe;
                ribbon.DataContext = ribbon.Language;

                Assert.That(ribbon.Language, Is.EqualTo(window.Language), "Language on Ribbon should match Window.");
                Assert.That(ribbon.DataContext, Is.EqualTo(window.DataContext), "DataContext on Ribbon should match Window.");

                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.DataContextProperty, ribbon);
                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.LanguageProperty, ribbon);
            }

            {
                ribbon.Language = enUs;
                ribbon.DataContext = ribbon.Language;

                Assert.That(ribbon.Language, Is.Not.EqualTo(window.Language), "Language on Ribbon should not match Window.");
                Assert.That(ribbon.DataContext, Is.Not.EqualTo(window.DataContext), "DataContext on Ribbon should not match Window.");

                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.DataContextProperty, ribbon);
                CheckIfAllElementsHaveSameValue(elemens, FrameworkElement.LanguageProperty, ribbon);
            }
        }
    }

    private static void CheckIfAllElementsHaveSameValue(Dictionary<FrameworkElement, string> elements, DependencyProperty property, FrameworkElement expectedValueSource)
    {
        var expectedValue = expectedValueSource.GetValue(property);

        foreach (var element in elements)
        {
            Assert.That(element.Key.GetValue(property), Is.EqualTo(expectedValue), $"{property.Name} on {element.Value} should match.");
        }
    }

    [Test]
    public void TitleBar_properties_synchronised_with_ribbon()
    {
        var ribbon = new Ribbon { ContextualGroups = { new RibbonContextualTabGroup() } };
        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();
            Assert.That(ribbon.QuickAccessToolBar, Is.Not.Null);

            var oldTitleBar = ribbon.TitleBar = new RibbonTitleBar();
            Assert.That(oldTitleBar.Items.Count, Is.EqualTo(1));
            Assert.That(ribbon.QuickAccessToolBar, Is.EqualTo(oldTitleBar.QuickAccessToolBar));

            var newTitleBar = new RibbonTitleBar();
            Assert.That(newTitleBar.Items.Count, Is.EqualTo(0));
            Assert.That(newTitleBar.QuickAccessToolBar, Is.Null);

            // assign a new title bar, the contextual groups and quick access are transferred across
            ribbon.TitleBar = newTitleBar;
            Assert.That(oldTitleBar.Items.Count, Is.EqualTo(0));
            Assert.That(oldTitleBar.QuickAccessToolBar, Is.Null);
            Assert.That(newTitleBar.Items.Count, Is.EqualTo(1));
            Assert.That(newTitleBar.QuickAccessToolBar, Is.EqualTo(ribbon.QuickAccessToolBar));

            // remove the title bar
            ribbon.TitleBar = null;
            Assert.That(oldTitleBar.Items.Count, Is.EqualTo(0));
            Assert.That(oldTitleBar.QuickAccessToolBar, Is.Null);
            Assert.That(newTitleBar.Items.Count, Is.EqualTo(0));
            Assert.That(newTitleBar.QuickAccessToolBar, Is.Null);
        }
    }

    [Test]
    public void Test_KeyTipKeys()
    {
        var ribbon = new Ribbon();
        var keyTipService = ribbon.GetFieldValue<KeyTipService>("keyTipService");

        Assert.That(ribbon.KeyTipKeys, Is.Empty);                
        Assert.That(keyTipService.KeyTipKeys, Is.EquivalentTo(KeyTipService.DefaultKeyTipKeys));

        ribbon.KeyTipKeys.Add(Key.A);

        Assert.That(ribbon.KeyTipKeys, Is.EquivalentTo(new[]
        {
            Key.A
        }));

        Assert.That(keyTipService.KeyTipKeys, Is.EquivalentTo(new[]
        {
            Key.A
        }));
    }

    /// <summary>
    /// The ribbon collapses automatically when its window is smaller than
    /// <see cref="Ribbon.MinimalVisibleWidth"/> x <see cref="Ribbon.MinimalVisibleHeight"/>.
    /// That was only checked when the window's size changed after the ribbon had subscribed on Loaded.
    /// A ribbon loaded into a window that was already that small (for example a window restored to a
    /// small saved size) stayed expanded until the user resized the window.
    /// </summary>
    [Test]
    public void Ribbon_loaded_into_a_small_window_is_collapsed()
    {
        var ribbon = new Ribbon();

        using (var window = new TestRibbonWindow())
        {
            window.Width = Ribbon.MinimalVisibleWidth - 50;
            window.Height = Ribbon.MinimalVisibleHeight - 50;
            UIHelper.DoEvents();

            Assert.That(window.ActualWidth, Is.LessThan(Ribbon.MinimalVisibleWidth), "Precondition: the window is already small");

            window.Content = ribbon;
            UIHelper.DoEvents();

            Assert.That(ribbon.IsAutomaticCollapseEnabled, Is.True, "Precondition: automatic collapse is on by default");
            Assert.That(ribbon.IsCollapsed, Is.True, "A ribbon in a window that is too small must be collapsed");
        }
    }
}
