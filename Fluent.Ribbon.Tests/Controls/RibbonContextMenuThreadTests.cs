namespace Fluent.Tests.Controls;

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;
using NUnit.Framework;

/// <summary>
/// Every UI thread gets its own ribbon context menu (<see cref="Ribbon.RibbonContextMenu"/>), and every Fluent control
/// created on that thread uses it as its context menu.
/// The menus (and their items) were kept in static dictionaries keyed by the managed thread id, and nothing ever removed
/// an entry. So after a UI thread ended (for example a window that ran on its own thread was closed), the static dictionary
/// still held that thread's menu, through the menu its dispatcher, and through the dispatcher the dead thread itself.
/// None of it could ever be collected: every finished UI thread that used a Fluent control leaked.
/// </summary>
[TestFixture]
public class RibbonContextMenuThreadTests
{
    /// <summary>
    /// Control for the test method: a finished UI thread that only used plain WPF controls is collected.
    /// If this fails, the garbage collection check itself is unreliable here.
    /// </summary>
    [Test]
    public void Finished_ui_thread_that_used_a_wpf_control_is_collected()
    {
        // Run static constructors here, so nothing static is created on the other thread.
        _ = new System.Windows.Controls.Button();

        var references = RunUiThreadAndGetWeakReferences(() => new System.Windows.Controls.Button());

        Assert.Multiple(() =>
        {
            Assert.That(IsAliveAfterGarbageCollection(references.Created), Is.False, "The control of a finished UI thread should be collected");
            Assert.That(IsAliveAfterGarbageCollection(references.Dispatcher), Is.False, "The dispatcher of a finished UI thread should be collected");
            Assert.That(IsAliveAfterGarbageCollection(references.Thread), Is.False, "A finished UI thread should be collected");
        });
    }

    [Test]
    public void Finished_ui_thread_that_used_a_fluent_control_is_collected()
    {
        // Run static constructors here, so nothing static is created on the other thread.
        _ = new Button();

        var references = RunUiThreadAndGetWeakReferences(() =>
        {
            var button = new Button();

            Assert.That(button.ContextMenu, Is.SameAs(Ribbon.RibbonContextMenu), "Precondition: the button uses the ribbon context menu of its thread");

            return button.ContextMenu;
        });

        Assert.Multiple(() =>
        {
            Assert.That(IsAliveAfterGarbageCollection(references.Created), Is.False, "The ribbon context menu of a finished UI thread should be collected");
            Assert.That(IsAliveAfterGarbageCollection(references.Dispatcher), Is.False, "The dispatcher of a finished UI thread should be collected");
            Assert.That(IsAliveAfterGarbageCollection(references.Thread), Is.False, "A finished UI thread should be collected");
        });
    }

    /// <summary>
    /// Each UI thread must get a menu that belongs to its own dispatcher, otherwise opening it would throw
    /// ("The calling thread cannot access this object because a different thread owns it").
    /// </summary>
    [Test]
    public void Fluent_control_on_another_ui_thread_gets_a_context_menu_of_that_thread()
    {
        var menuOfThisThread = new Button().ContextMenu;

        Assert.That(menuOfThisThread, Is.Not.Null, "Precondition: the button on this thread has the ribbon context menu");
        Assert.That(menuOfThisThread!.Dispatcher, Is.SameAs(Dispatcher.CurrentDispatcher), "Precondition: the menu of this thread belongs to this thread");

        System.Windows.Controls.ContextMenu? menuOfOtherThread = null;
        Dispatcher? otherDispatcher = null;

        RunUiThreadAndGetWeakReferences(() =>
        {
            otherDispatcher = Dispatcher.CurrentDispatcher;
            menuOfOtherThread = new Button().ContextMenu;

            return menuOfOtherThread;
        });

        Assert.That(menuOfOtherThread, Is.Not.Null, "The button on the other thread has a context menu");
        Assert.That(menuOfOtherThread, Is.Not.SameAs(menuOfThisThread), "The other thread must not get the menu of this thread");
        Assert.That(menuOfOtherThread!.Dispatcher, Is.SameAs(otherDispatcher), "The menu must belong to the thread of the button");
        Assert.That(new Button().ContextMenu, Is.SameAs(menuOfThisThread), "This thread keeps its own menu");
    }

    private sealed class UiThreadReferences
    {
        public UiThreadReferences(WeakReference thread, WeakReference dispatcher, WeakReference created)
        {
            this.Thread = thread;
            this.Dispatcher = dispatcher;
            this.Created = created;
        }

        public WeakReference Thread { get; }

        public WeakReference Dispatcher { get; }

        public WeakReference Created { get; }
    }

    // Runs work on a new UI thread (STA, with a running dispatcher) the way an application runs a window on its own thread,
    // shuts that dispatcher down, waits until the thread ended and returns weak references to the thread, its dispatcher
    // and the object the work returned.
    // Not inlined, so no local of the caller keeps the thread or its objects alive.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static UiThreadReferences RunUiThreadAndGetWeakReferences(Func<object?> work)
    {
        WeakReference? dispatcherReference = null;
        WeakReference? createdReference = null;
        Exception? exception = null;

        var thread = new Thread(() =>
        {
            try
            {
                var dispatcher = Dispatcher.CurrentDispatcher;
                dispatcherReference = new WeakReference(dispatcher);

                dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    try
                    {
                        createdReference = new WeakReference(work());
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                    finally
                    {
                        dispatcher.InvokeShutdown();
                    }
                }));

                Dispatcher.Run();
            }
            catch (Exception e)
            {
                exception ??= e;
            }
        })
        {
            // A thread that hangs must not keep the test run alive.
            IsBackground = true
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        var ended = thread.Join(TimeSpan.FromSeconds(30));

        Assert.That(ended, Is.True, "Precondition: the UI thread ended");

        if (exception is not null)
        {
            throw new InvalidOperationException("The UI thread failed: " + exception.Message, exception);
        }

        Assert.That(dispatcherReference, Is.Not.Null, "Precondition: the UI thread had a dispatcher");
        Assert.That(createdReference?.IsAlive, Is.True, "Precondition: the UI thread ran the work");

        return new UiThreadReferences(new WeakReference(thread), dispatcherReference!, createdReference!);
    }

    private static bool IsAliveAfterGarbageCollection(WeakReference reference)
    {
        // A thread releases its data shortly after it ended, so try a few times.
        for (var i = 0; i < 20 && reference.IsAlive; i++)
        {
            Thread.Sleep(50);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        return reference.IsAlive;
    }
}
