namespace Fluent.Tests.Controls;

using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;

/// <summary>
/// Tests that a quick access copy does not stay alive just because its original control is alive.
/// </summary>
/// <remarks>
/// The Quick Access Toolbar throws away its copies and creates new ones when items are added or removed,
/// when its state is loaded and when the ribbon is re-templated.
/// The original controls live as long as the window, so anything that lets an original keep its old copies alive
/// makes memory grow with every one of those actions.
/// </remarks>
[TestFixture]
public class QuickAccessItemLeakTests
{
    // Button, ToggleButton and MenuItem (without items) get a copy that is NOT an ItemsControl.
    // They prove that this way of checking for leaks works at all.
    // All others get a copy that is an ItemsControl, so RibbonControl.BindQuickAccessItem syncs its GroupStyle collection.
    [TestCase("Button")]
    [TestCase("ToggleButton")]
    [TestCase("MenuItem")]
    [TestCase("DropDownButton")]
    [TestCase("SplitButton")]
    [TestCase("MenuItem with items")]
    [TestCase("Split MenuItem with items")]
    [TestCase("ComboBox")]
    [TestCase("InRibbonGallery")]
    [TestCase("RibbonGroupBox")]
    public void Discarded_Copy_Is_Collected_While_Original_Is_Alive(string kind)
    {
        var source = CreateSource(kind);

        var discardedCopy = CreateDiscardedCopy(source);

        // Some controls remember their latest copy, so create a newer one, like the toolbar does when it re-creates its items.
        var currentCopy = source.CreateQuickAccessItem();

        CollectGarbage();

        Assert.That(discardedCopy.IsAlive, Is.False, "The original control keeps its discarded quick access copy alive.");

        GC.KeepAlive(source);
        GC.KeepAlive(currentCopy);
    }

    [Test]
    public void GroupStyle_Changes_Still_Reach_A_Live_Copy_After_Garbage_Collection()
    {
        var existingGroupStyle = new System.Windows.Controls.GroupStyle();
        var source = new DropDownButton();
        source.GroupStyle.Add(existingGroupStyle);

        var copy = (DropDownButton)source.CreateQuickAccessItem();

        Assert.That(copy.GroupStyle, Is.EqualTo(new[] { existingGroupStyle }));

        // Whatever keeps the copy in sync must live as long as the copy, not be collected early.
        CollectGarbage();

        var addedGroupStyle = new System.Windows.Controls.GroupStyle();
        source.GroupStyle.Add(addedGroupStyle);

        Assert.That(copy.GroupStyle, Is.EqualTo(new[] { existingGroupStyle, addedGroupStyle }));

        source.GroupStyle.Remove(existingGroupStyle);

        Assert.That(copy.GroupStyle, Is.EqualTo(new[] { addedGroupStyle }));
    }

    private static IQuickAccessItemProvider CreateSource(string kind)
    {
        switch (kind)
        {
            case "Button":
                return new Button();
            case "ToggleButton":
                return new ToggleButton();
            case "MenuItem":
                return new MenuItem();
            case "DropDownButton":
                return new DropDownButton();
            case "SplitButton":
                return new SplitButton();
            case "MenuItem with items":
                return WithItem(new MenuItem());
            case "Split MenuItem with items":
                return WithItem(new MenuItem { IsSplit = true });
            case "ComboBox":
                return new ComboBox();
            case "InRibbonGallery":
                return new InRibbonGallery();
            case "RibbonGroupBox":
                return new RibbonGroupBox();
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }

    private static MenuItem WithItem(MenuItem menuItem)
    {
        menuItem.Items.Add(new MenuItem());
        return menuItem;
    }

    // Not inlined, so no local variable of the test method holds the copy when the garbage collector runs.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateDiscardedCopy(IQuickAccessItemProvider source)
    {
        return new WeakReference(source.CreateQuickAccessItem());
    }

    private static void CollectGarbage()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
