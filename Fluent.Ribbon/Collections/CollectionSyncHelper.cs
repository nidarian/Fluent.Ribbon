namespace Fluent.Collections;

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

/// <summary>
/// Synchronizes a target collection with a source collection in a one way fashion.
/// </summary>
public class CollectionSyncHelper<TItem>
{
    private readonly bool listenWeakly;

    /// <summary>
    /// Creates a new instance with <paramref name="source"/> as <see cref="Source"/> and <paramref name="target"/> as <see cref="Target"/>.
    /// </summary>
    public CollectionSyncHelper(ObservableCollection<TItem> source, IList target)
        : this(source, target, listenWeakly: false)
    {
    }

    /// <summary>
    /// Creates a new instance with <paramref name="source"/> as <see cref="Source"/> and <paramref name="target"/> as <see cref="Target"/>.
    /// </summary>
    /// <param name="source">The source collection.</param>
    /// <param name="target">The target collection.</param>
    /// <param name="listenWeakly">
    /// <c>true</c> to listen to changes of <paramref name="source"/> through a weak event, so <paramref name="source"/> does not keep this instance
    /// (and with it <paramref name="target"/>) alive. The caller then has to keep this instance alive for as long as <paramref name="target"/> should be synchronized.
    /// </param>
    internal CollectionSyncHelper(ObservableCollection<TItem> source, IList target, bool listenWeakly)
    {
        this.Source = source ?? throw new ArgumentNullException(nameof(source));
        this.Target = target ?? throw new ArgumentNullException(nameof(target));
        this.listenWeakly = listenWeakly;

        this.SyncTarget();

        if (listenWeakly)
        {
            CollectionChangedEventManager.AddHandler(this.Source, this.SourceOnCollectionChanged);
        }
        else
        {
            this.Source.CollectionChanged += this.SourceOnCollectionChanged;
        }
    }

    /// <summary>
    /// Stops synchronizing <see cref="Target"/> with <see cref="Source"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="Source"/> usually lives longer than <see cref="Target"/> (for example when a control is re-templated),
    /// so without this the old helper would keep forwarding changes to a target that's no longer used.
    /// </remarks>
    internal void Detach()
    {
        if (this.listenWeakly)
        {
            CollectionChangedEventManager.RemoveHandler(this.Source, this.SourceOnCollectionChanged);
        }
        else
        {
            this.Source.CollectionChanged -= this.SourceOnCollectionChanged;
        }
    }

    /// <summary>
    /// The source collection.
    /// </summary>
    public ObservableCollection<TItem> Source { get; }

    /// <summary>
    /// The target collection.
    /// </summary>
    public IList Target { get; }

    /// <summary>
    /// Clears <see cref="Target"/> and then copies all items from <see cref="Source"/> to <see cref="Target"/>.
    /// </summary>
    private void SyncTarget()
    {
        this.Target.Clear();

        foreach (var item in this.Source)
        {
            this.Target.Add(item);
        }
    }

    private void SourceOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                for (var i = 0; i < e.NewItems?.Count; i++)
                {
                    this.Target.Insert(e.NewStartingIndex + i, e.NewItems![i]);
                }

                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is not null)
                {
                    foreach (var item in e.OldItems)
                    {
                        this.Target.Remove(item);
                    }
                }

                break;

            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems is not null)
                {
                    foreach (var item in e.OldItems)
                    {
                        this.Target.Remove(item);
                    }
                }

                if (e.NewItems is not null)
                {
                    foreach (var item in e.NewItems)
                    {
                        this.Target.Add(item);
                    }
                }

                break;

            case NotifyCollectionChangedAction.Reset:
                this.SyncTarget();

                break;
        }
    }
}