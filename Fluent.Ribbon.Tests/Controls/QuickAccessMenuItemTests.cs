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

    /// <summary>
    /// Follow-up to #1251: unchecking from code (or through a binding) must remove the item from the toolbar,
    /// even when the quick access menu was never opened.
    /// OnUnchecked returned early while the item wasn't loaded, and items added from code only get loaded
    /// once the quick access menu is opened, so the item stayed on the toolbar.
    /// </summary>
    [Test]
    public void Checked_item_added_from_code_behind_is_removed_from_toolbar_when_unchecked_from_code()
    {
        var ribbon = new Ribbon();

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();

            var target = new Button { Header = "Target" };
            var item = new QuickAccessMenuItem
            {
                Target = target,
                IsChecked = true
            };

            ribbon.QuickAccessItems.Add(item);
            UIHelper.DoEvents();

            Assert.That(ribbon.IsInQuickAccessToolBar(target), Is.True, "Precondition: the checked item is on the toolbar.");
            Assert.That(item.IsLoaded, Is.False, "Precondition: the quick access menu was never opened, so the item isn't loaded.");

            item.IsChecked = false;

            Assert.That(ribbon.IsInQuickAccessToolBar(target), Is.False, "Unchecking from code should remove the target from the toolbar.");
            Assert.That(ribbon.QuickAccessToolBar.Items, Is.Empty, "The toolbar should no longer show the item.");
        }
    }

    [Test]
    public void Item_checked_and_unchecked_from_code_behind_is_removed_from_toolbar()
    {
        var ribbon = new Ribbon();

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();

            var target = new Button { Header = "Target" };
            var item = new QuickAccessMenuItem { Target = target };

            ribbon.QuickAccessItems.Add(item);

            item.IsChecked = true;
            Assert.That(ribbon.QuickAccessToolBar.Items, Has.Count.EqualTo(1), "Precondition: checking from code shows the item.");
            Assert.That(item.IsLoaded, Is.False, "Precondition: the quick access menu was never opened, so the item isn't loaded.");

            item.IsChecked = false;

            Assert.That(ribbon.IsInQuickAccessToolBar(target), Is.False);
            Assert.That(ribbon.QuickAccessToolBar.Items, Is.Empty);
        }
    }

    [Test]
    public void Unchecking_item_before_template_is_applied_does_not_remove_anything()
    {
        // XAML/startup order: there is no toolbar yet. The element was pinned from code (for example in the window constructor).
        // Unchecking the menu item this early must not remove it, as before: the item's IsChecked is synced
        // from the toolbar once the item gets loaded (OnItemLoaded).
        var ribbon = new Ribbon();
        var target = new Button { Header = "Target" };

        ribbon.AddToQuickAccessToolBar(target);

        var item = new QuickAccessMenuItem { Target = target, IsChecked = true };
        ribbon.QuickAccessItems.Add(item);

        item.IsChecked = false;

        Assert.That(ribbon.IsInQuickAccessToolBar(target), Is.True, "Nothing may be removed before the toolbar exists.");

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();
            UIHelper.DoEvents();

            Assert.That(ribbon.IsInQuickAccessToolBar(target), Is.True);
            Assert.That(ribbon.QuickAccessToolBar.Items, Has.Count.EqualTo(1));
            Assert.That(item.IsChecked, Is.True, "The loaded item should reflect the toolbar.");
        }
    }

    [Test]
    public void Unchecking_item_whose_target_is_not_in_toolbar_does_not_remove_other_items()
    {
        var ribbon = new Ribbon();

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();

            var firstTarget = new Button { Header = "First" };
            var secondTarget = new Button { Header = "Second" };
            var firstItem = new QuickAccessMenuItem { Target = firstTarget, IsChecked = true };
            var secondItem = new QuickAccessMenuItem { Target = secondTarget, IsChecked = true };

            ribbon.QuickAccessItems.Add(firstItem);
            ribbon.QuickAccessItems.Add(secondItem);
            Assert.That(ribbon.QuickAccessToolBar.Items, Has.Count.EqualTo(2), "Precondition: both items are on the toolbar.");

            // Removed some other way (for example the toolbar's context menu), the menu item isn't synced until it gets loaded.
            ribbon.RemoveFromQuickAccessToolBar(firstTarget);
            Assert.That(firstItem.IsChecked, Is.True, "Precondition: the unloaded menu item is still checked.");

            Assert.That(() => firstItem.IsChecked = false, Throws.Nothing);

            Assert.That(ribbon.IsInQuickAccessToolBar(firstTarget), Is.False);
            Assert.That(ribbon.IsInQuickAccessToolBar(secondTarget), Is.True);
            Assert.That(ribbon.QuickAccessToolBar.Items, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void Loading_temporary_state_keeps_checked_items_added_from_code_behind()
    {
        var ribbon = new Ribbon();

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.ApplyTemplate();

            var target = new Button { Header = "Target" };
            var item = new QuickAccessMenuItem { Target = target, IsChecked = true };

            ribbon.QuickAccessItems.Add(item);

            ribbon.RibbonStateStorage.SaveTemporary();
            ribbon.RibbonStateStorage.LoadTemporary();
            UIHelper.DoEvents();

            Assert.That(ribbon.IsInQuickAccessToolBar(target), Is.True);
            Assert.That(ribbon.QuickAccessToolBar.Items, Has.Count.EqualTo(1));
            Assert.That(item.IsChecked, Is.True);
        }
    }
}
