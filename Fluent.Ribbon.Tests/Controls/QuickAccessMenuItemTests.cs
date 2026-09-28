namespace Fluent.Tests.Controls;

using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for <see cref="QuickAccessMenuItem"/> being added from code-behind.
/// </summary>
/// <remarks>
/// Regression tests for https://github.com/fluentribbon/Fluent.Ribbon/issues/1251.
/// Items declared in XAML receive their Loaded event together with the ribbon, which is
/// what puts checked items onto the toolbar. Items added from code after the ribbon is live
/// only get Loaded once the quick access menu is opened, so checked items did not show up
/// until then.
/// </remarks>
[TestFixture]
public class QuickAccessMenuItemTests
{
    [Test]
    public void Checked_item_added_from_code_behind_is_shown_in_toolbar()
    {
        var ribbon = new Ribbon();

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();
            Assert.That(ribbon.QuickAccessToolBar, Is.Not.Null);

            var target = new Button { Header = "Target" };

            // This is the exact order used in the issue: IsChecked is set before the item is added.
            var item = new QuickAccessMenuItem
            {
                Target = target,
                IsChecked = true
            };

            ribbon.QuickAccessItems.Add(item);

            Assert.That(ribbon.IsInQuickAccessToolBar(target), Is.True, "Target should be registered as a quick access element.");
            Assert.That(ribbon.QuickAccessToolBar.Items, Has.Count.EqualTo(1), "Toolbar should show the item without opening the quick access menu.");
        }
    }

    [Test]
    public void Item_checked_after_being_added_from_code_behind_is_shown_in_toolbar()
    {
        // The workaround mentioned in the issue. Guards against the fix changing this path.
        var ribbon = new Ribbon();

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();

            var target = new Button { Header = "Target" };
            var item = new QuickAccessMenuItem { Target = target };

            ribbon.QuickAccessItems.Add(item);
            Assert.That(ribbon.QuickAccessToolBar.Items, Is.Empty, "Unchecked items must not be added.");

            item.IsChecked = true;

            Assert.That(ribbon.IsInQuickAccessToolBar(target), Is.True);
            Assert.That(ribbon.QuickAccessToolBar.Items, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void Unchecked_item_added_from_code_behind_is_not_shown_in_toolbar()
    {
        var ribbon = new Ribbon();

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();

            var target = new Button { Header = "Target" };

            ribbon.QuickAccessItems.Add(new QuickAccessMenuItem { Target = target, IsChecked = false });

            Assert.That(ribbon.IsInQuickAccessToolBar(target), Is.False);
            Assert.That(ribbon.QuickAccessToolBar.Items, Is.Empty);
        }
    }

    [Test]
    public void Checked_item_added_before_template_is_applied_does_not_throw_and_is_shown_after_load()
    {
        // Covers the XAML-like order: items are added before the ribbon has its template.
        // The fix must not register the element early (the toolbar doesn't exist yet),
        // otherwise the later Loaded logic would think it's already on the toolbar.
        var ribbon = new Ribbon();
        var target = new Button { Header = "Target" };

        ribbon.QuickAccessItems.Add(new QuickAccessMenuItem { Target = target, IsChecked = true });

        Assert.That(ribbon.IsInQuickAccessToolBar(target), Is.False, "Nothing can be shown before the toolbar exists.");

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();

            // Let queued Loaded events run, which is how XAML declared items get added.
            UIHelper.DoEvents();

            Assert.That(ribbon.IsInQuickAccessToolBar(target), Is.True);
            Assert.That(ribbon.QuickAccessToolBar.Items, Has.Count.EqualTo(1));
        }
    }
}
