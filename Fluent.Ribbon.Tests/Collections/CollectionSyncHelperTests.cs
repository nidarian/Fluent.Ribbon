namespace Fluent.Tests.Collections;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Fluent.Collections;
using NUnit.Framework;

[TestFixture]
public class CollectionSyncHelperTests
{
    [Test]
    public void NewInstanceShouldCopyItems()
    {
        var source = new ObservableCollection<string> { "One", "Two" };
        var target = new ObservableCollection<string>();

        Assert.That(target, Is.Not.EquivalentTo(source));

        var sync = new CollectionSyncHelper<string>(source, target);

        Assert.That(target, Is.EquivalentTo(source));
    }

    [Test]
    public void CollectionActionShouldSync()
    {
        var source = new ObservableCollection<string>();
        var target = new ObservableCollection<string>();

        Assert.That(target, Is.EquivalentTo(source));

        var sync = new CollectionSyncHelper<string>(source, target);

        Assert.That(target, Is.EquivalentTo(source));

        {
            source.Add("One");

            Assert.That(target, Is.EquivalentTo(source));
        }

        {
            source.RemoveAt(0);

            Assert.That(target, Is.EquivalentTo(source));
        }

        {
            source.Add("One");

            Assert.That(target, Is.EquivalentTo(source));
        }

        {
            source[0] = "Two";

            Assert.That(target, Is.EquivalentTo(source));
        }

        {
            source.Clear();

            Assert.That(target, Is.EquivalentTo(source));
        }
    }

    /// <summary>
    /// <see cref="ObservableCollection{T}.Move"/> raised a Move notification that the helper ignored,
    /// so the target kept the old order. <see cref="Ribbon"/> uses the helper to show its Tabs in the tab control.
    /// </summary>
    [Test]
    public void Move_should_keep_the_order_of_the_target_equal_to_the_source()
    {
        var source = new ObservableCollection<string> { "One", "Two", "Three" };
        var target = new ObservableCollection<string>();

        var sync = new CollectionSyncHelper<string>(source, target);

        source.Move(0, 2);

        Assert.That(target.ToList(), Is.EqualTo(source.ToList()), "after Move(0, 2)");

        source.Move(2, 1);

        Assert.That(target.ToList(), Is.EqualTo(source.ToList()), "after Move(2, 1)");
    }

    /// <summary>
    /// Replacing an item (source[i] = x) removed the old item and added the new one at the end of the target,
    /// instead of at index i.
    /// </summary>
    [Test]
    public void Replace_should_put_the_new_item_at_the_same_index()
    {
        var source = new ObservableCollection<string> { "One", "Two", "Three" };
        var target = new ObservableCollection<string>();

        var sync = new CollectionSyncHelper<string>(source, target);

        source[0] = "Four";

        Assert.That(target.ToList(), Is.EqualTo(source.ToList()), "after source[0] = \"Four\"");

        source[1] = "Five";

        Assert.That(target.ToList(), Is.EqualTo(source.ToList()), "after source[1] = \"Five\"");
    }

    /// <summary>
    /// Same as above, for the helper that listens weakly (used for GroupStyle in <see cref="RibbonControl"/>).
    /// </summary>
    [Test]
    public void Move_and_Replace_should_keep_the_order_when_listening_weakly()
    {
        var source = new ObservableCollection<string> { "One", "Two", "Three" };
        var target = new ObservableCollection<string>();

        var sync = new CollectionSyncHelper<string>(source, target, listenWeakly: true);

        source.Move(0, 2);

        Assert.That(target.ToList(), Is.EqualTo(source.ToList()), "after Move(0, 2)");

        source[0] = "Four";

        Assert.That(target.ToList(), Is.EqualTo(source.ToList()), "after source[0] = \"Four\"");

        GC.KeepAlive(sync);
    }
}