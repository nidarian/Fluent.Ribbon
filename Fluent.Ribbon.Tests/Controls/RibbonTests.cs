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
    /// Right-clicking a <see cref="Gallery"/> shows "Add Gallery to Quick Access Toolbar".
    /// A Gallery can't be put on the toolbar itself, so AddToQuickAccessToolBar adds the control
    /// hosting it instead (here the drop down). The command's can-execute check didn't do that
    /// redirect: it required the Gallery itself to be addable, so the menu entry was always disabled.
    /// </summary>
    [Test]
    public void AddToQuickAccessCommand_is_enabled_for_a_gallery_and_adds_its_host()
    {
        var gallery = new Gallery();
        var dropDownButton = new DropDownButton { Header = "Host", Items = { gallery } };
        var ribbon = new Ribbon
        {
            Tabs =
            {
                new RibbonTabItem
                {
                    Header = "Tab",
                    Groups = { new RibbonGroupBox { Header = "Group", Items = { dropDownButton } } }
                }
            }
        };

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            Assert.That(ribbon.QuickAccessToolBar, Is.Not.Null, "Precondition: the ribbon has its toolbar");
            Assert.That(LogicalTreeHelper.GetParent(gallery), Is.SameAs(dropDownButton), "Precondition: the drop down hosts the gallery");

            Assert.That(Ribbon.AddToQuickAccessCommand.CanExecute(gallery, ribbon), Is.True, "Adding a gallery must be offered");

            Ribbon.AddToQuickAccessCommand.Execute(gallery, ribbon);

            Assert.That(ribbon.IsInQuickAccessToolBar(dropDownButton), Is.True, "Executing adds the host control");
            Assert.That(Ribbon.AddToQuickAccessCommand.CanExecute(gallery, ribbon), Is.False, "Once the host is on the toolbar, adding it again isn't offered");
        }
    }

    /// <summary>
    /// Apps often restore their Quick Access Toolbar in the window's constructor, right after
    /// InitializeComponent. The Ribbon has no template yet at that point, so it has no toolbar:
    /// AddToQuickAccessToolBar recorded the element but had nowhere to put its toolbar copy, and
    /// nothing added the recorded copies once the template (and so the toolbar) was created.
    /// The element never appeared, and because it counted as added, it couldn't be added again.
    /// </summary>
    [Test]
    public void Elements_added_before_the_template_is_applied_appear_on_the_toolbar()
    {
        var button = new Fluent.Button { Header = "Early" };
        var ribbon = new Ribbon
        {
            Tabs =
            {
                new RibbonTabItem
                {
                    Header = "Tab",
                    Groups = { new RibbonGroupBox { Header = "Group", Items = { button } } }
                }
            }
        };

        // Like in a window constructor: the ribbon isn't shown, so it has no template yet.
        Assert.That(ribbon.QuickAccessToolBar, Is.Null, "Precondition: no template applied yet");
        ribbon.AddToQuickAccessToolBar(button);

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            Assert.That(ribbon.QuickAccessToolBar, Is.Not.Null, "Precondition: the template created the toolbar");
            Assert.That(ribbon.IsInQuickAccessToolBar(button), Is.True);

            var copy = ribbon.GetQuickAccessElements()[button];
            Assert.That(ribbon.QuickAccessToolBar.Items, Does.Contain(copy), "The toolbar copy must be on the toolbar");
        }
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

    /// <summary>
    /// <see cref="Ribbon.QuickAccessItems"/>.Clear() raises a Reset without the removed items.
    /// The removed menu items must still forget the ribbon, otherwise checking them later pins their target to a ribbon they no longer belong to.
    /// </summary>
    [Test]
    public void Clearing_QuickAccessItems_detaches_them_from_the_ribbon()
    {
        var ribbon = new Ribbon();
        var target = new Button();
        var item = new QuickAccessMenuItem
        {
            Target = target
        };

        ribbon.QuickAccessItems.Add(item);

        Assert.That(item.Ribbon, Is.SameAs(ribbon), "Precondition: adding the item must attach it to the ribbon.");
        Assert.That(item.IsChecked, Is.False, "Precondition: the item must start unchecked.");

        ribbon.QuickAccessItems.Clear();

        item.IsChecked = true;

        Assert.That(ribbon.IsInQuickAccessToolBar(target), Is.False, "Checking a removed item must not pin its target.");
        Assert.That(item.Ribbon, Is.Null, "A removed item must not reference the ribbon.");
    }

    /// <summary>
    /// Re-templating a ribbon at runtime (for example after a theme switch) creates a new tab control and a new quick access toolbar.
    /// Pinned elements must survive that, and the old collection sync helpers must stop forwarding changes to the old parts.
    /// </summary>
    [Test]
    public void Retemplating_keeps_quick_access_elements_and_stops_syncing_old_parts()
    {
        var ribbon = new Ribbon
        {
            Tabs =
            {
                new RibbonTabItem()
            }
        };

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();

            var button = new Button();
            ribbon.AddToQuickAccessToolBar(button);

            var oldToolBar = ribbon.QuickAccessToolBar;
            Assert.That(oldToolBar, Is.Not.Null, "Precondition: the ribbon must have a quick access toolbar.");
            Assert.That(ribbon.IsInQuickAccessToolBar(button), Is.True, "Precondition: the button must be pinned.");
            Assert.That(oldToolBar.Items.Count, Is.EqualTo(1), "Precondition: the toolbar must show the pinned button.");

            var template = ribbon.Template;
            ribbon.Template = null;
            ribbon.ApplyTemplate();
            ribbon.Template = template;
            ribbon.ApplyTemplate();

            Assert.That(ribbon.QuickAccessToolBar, Is.Not.Null.And.Not.SameAs(oldToolBar), "Precondition: re-templating must create a new quick access toolbar.");

            Assert.That(ribbon.IsInQuickAccessToolBar(button), Is.True, "The button must still be pinned after re-templating.");
            Assert.That(ribbon.QuickAccessToolBar.Items.Count, Is.EqualTo(1), "The new toolbar must show the pinned button.");

            // The old sync helpers would still forward these changes to the cleared old parts and throw.
            var newTab = new RibbonTabItem();
            Assert.DoesNotThrow(() => ribbon.Tabs.Add(newTab));
            Assert.That(ribbon.TabControl.Items.Contains(newTab), Is.True, "The new tab must reach the new tab control.");

            Assert.DoesNotThrow(() => ribbon.QuickAccessItems.Add(new QuickAccessMenuItem()));
            Assert.That(ribbon.QuickAccessToolBar.QuickAccessItems.Count, Is.EqualTo(1), "The new menu item must reach the new toolbar.");

            Assert.DoesNotThrow(() => ribbon.ToolBarItems.Add(new Button()));
            Assert.That(ribbon.TabControl.ToolBarItems.Count, Is.EqualTo(1), "The new toolbar item must reach the new tab control.");
        }
    }

    /// <summary>
    /// Re-templating must keep the toolbar order. The pinned elements are kept in a dictionary,
    /// and after a removal a new entry reuses the removed entry's slot, so the dictionary order is D, B, C here.
    /// </summary>
    [Test]
    public void Retemplating_keeps_quick_access_toolbar_order_after_removal()
    {
        var ribbon = new Ribbon
        {
            Tabs =
            {
                new RibbonTabItem()
            }
        };

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();

            var a = new Button { Header = "A" };
            var b = new Button { Header = "B" };
            var c = new Button { Header = "C" };
            var d = new Button { Header = "D" };

            ribbon.AddToQuickAccessToolBar(a);
            ribbon.AddToQuickAccessToolBar(b);
            ribbon.AddToQuickAccessToolBar(c);
            ribbon.RemoveFromQuickAccessToolBar(a);
            ribbon.AddToQuickAccessToolBar(d);

            Assert.That(GetQuickAccessToolBarOrder(ribbon), Is.EqualTo(new[] { "B", "C", "D" }), "Precondition: the toolbar must show B, C, D.");

            var template = ribbon.Template;
            ribbon.Template = null;
            ribbon.ApplyTemplate();
            ribbon.Template = template;
            ribbon.ApplyTemplate();

            Assert.That(GetQuickAccessToolBarOrder(ribbon), Is.EqualTo(new[] { "B", "C", "D" }), "Re-templating must keep the toolbar order.");
        }
    }

    /// <summary>
    /// <see cref="Button.CanAddToQuickAccessToolBar"/> only prevents new adds. An element pinned before it was set to false stays pinned,
    /// and re-templating must not drop it.
    /// </summary>
    [Test]
    public void Retemplating_keeps_element_pinned_before_CanAddToQuickAccessToolBar_was_turned_off()
    {
        var ribbon = new Ribbon
        {
            Tabs =
            {
                new RibbonTabItem()
            }
        };

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();

            var button = new Button { Header = "X" };
            ribbon.AddToQuickAccessToolBar(button);
            button.CanAddToQuickAccessToolBar = false;

            Assert.That(ribbon.IsInQuickAccessToolBar(button), Is.True, "Precondition: turning the flag off must not unpin the button.");
            Assert.That(ribbon.QuickAccessToolBar.Items.Count, Is.EqualTo(1), "Precondition: the toolbar must show the button.");

            var template = ribbon.Template;
            ribbon.Template = null;
            ribbon.ApplyTemplate();
            ribbon.Template = template;
            ribbon.ApplyTemplate();

            Assert.That(ribbon.IsInQuickAccessToolBar(button), Is.True, "The button must still be pinned after re-templating.");
            Assert.That(ribbon.QuickAccessToolBar.Items.Count, Is.EqualTo(1), "The new toolbar must show the button.");
        }
    }

    private static List<string> GetQuickAccessToolBarOrder(Ribbon ribbon)
    {
        var copies = ribbon.GetQuickAccessElements();
        var result = new List<string>();

        foreach (var item in ribbon.QuickAccessToolBar.Items)
        {
            foreach (var pair in copies)
            {
                if (ReferenceEquals(pair.Value, item))
                {
                    result.Add((string)((Button)pair.Key).Header);
                }
            }
        }

        return result;
    }
}
