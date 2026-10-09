namespace Fluent.Tests.Controls;

using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class ColorGalleryTests
{
    /// <summary>
    /// When a <see cref="ColorGallery"/> gets a new template, <c>OnApplyTemplate</c> must
    /// detach its click handler from the "More Colors" menu item of the OLD template.
    /// Otherwise the discarded menu item keeps the gallery alive and still triggers
    /// <see cref="ColorGallery.MoreColorsExecuting"/> when it is clicked.
    /// </summary>
    [Test]
    public void ReTemplating_Should_Detach_Handler_From_Old_MoreColors_Button()
    {
        var colorGallery = new ColorGallery();

        // Count how often the gallery reacts to a "More Colors" click.
        // Canceled = true keeps the handler from touching the static RecentColors list,
        // so this test doesn't leak state into other tests.
        var moreColorsExecutingCount = 0;
        colorGallery.MoreColorsExecuting += (_, args) =>
        {
            moreColorsExecutingCount++;
            args.Canceled = true;
        };

        using (new TestRibbonWindow(colorGallery))
        {
            colorGallery.ApplyTemplate();

            var oldMoreColorsButton = (MenuItem)colorGallery.Template.FindName("PART_MoreColors", colorGallery);
            Assert.That(oldMoreColorsButton, Is.Not.Null);

            // Force a fresh visual tree from the same template:
            // removing the template tears down the old parts, restoring it builds new ones
            // and calls OnApplyTemplate again (while the gallery still remembers the old button).
            var template = colorGallery.Template;
            colorGallery.Template = null;
            colorGallery.ApplyTemplate();
            colorGallery.Template = template;
            colorGallery.ApplyTemplate();

            var newMoreColorsButton = (MenuItem)colorGallery.Template.FindName("PART_MoreColors", colorGallery);
            Assert.That(newMoreColorsButton, Is.Not.Null);
            Assert.That(newMoreColorsButton, Is.Not.SameAs(oldMoreColorsButton), "Precondition: re-templating must create a new More Colors button.");

            // Clicking the discarded button must not reach the gallery anymore.
            oldMoreColorsButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent, oldMoreColorsButton));
            Assert.That(moreColorsExecutingCount, Is.EqualTo(0), "The old More Colors button is still attached to the gallery.");

            // The new button must still work, exactly once per click.
            newMoreColorsButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent, newMoreColorsButton));
            Assert.That(moreColorsExecutingCount, Is.EqualTo(1));
        }
    }

    /// <summary>
    /// <see cref="ColorGallery.RecentColors"/> is one static list shared by every <see cref="ColorGallery"/> of the process,
    /// and the template shows it in each gallery's "Recent Colors" list box.
    /// Apps can have windows on more than one UI thread (one window per thread).
    /// When "More Colors" adds a color on one thread, the list box of a gallery on another thread must not break.
    /// Before the fix WPF threw a <see cref="NotSupportedException"/>
    /// ("This type of CollectionView does not support changes to its SourceCollection from a thread different from the Dispatcher thread"),
    /// which ends the whole app.
    /// </summary>
    [Test]
    public void MoreColors_On_Second_UI_Thread_Should_Not_Break_Gallery_On_First_Thread()
    {
        // A color that no other test uses, so we can remove it again afterwards.
        var color = Color.FromRgb(0x12, 0x34, 0x56);
        Assume.That(ColorGallery.RecentColors.Contains(color), Is.False);
        var originalRecentColors = ColorGallery.RecentColors.ToList();

        Exception exceptionOnFirstThread = null;
        DispatcherUnhandledExceptionEventHandler unhandledOnFirstThread = (_, e) =>
        {
            exceptionOnFirstThread ??= e.Exception;
            e.Handled = true;
        };

        var firstGallery = new ColorGallery();

        Dispatcher.CurrentDispatcher.UnhandledException += unhandledOnFirstThread;

        try
        {
            using (new TestRibbonWindow(firstGallery))
            {
                firstGallery.ApplyTemplate();

                var firstRecentColorsListBox = (System.Windows.Controls.ListBox)firstGallery.Template.FindName("PART_RecentColorsListBox", firstGallery);
                Assert.That(firstRecentColorsListBox, Is.Not.Null);

                Exception exceptionOnSecondThread = null;
                Dispatcher secondDispatcher = null;

                var secondThread = new Thread(() =>
                {
                    secondDispatcher = Dispatcher.CurrentDispatcher;
                    secondDispatcher.UnhandledException += (_, e) =>
                    {
                        exceptionOnSecondThread ??= e.Exception;
                        e.Handled = true;
                    };

                    Window secondWindow = null;
                    try
                    {
                        // Like a second app window that runs on its own UI thread.
                        // It gets its own copy of the Fluent styles, created on this thread.
                        secondWindow = new Window
                        {
                            Width = 800,
                            Height = 600,
                            ShowActivated = false,
                            ShowInTaskbar = false,
                            Left = int.MinValue,
                            Top = int.MinValue,
                        };
                        secondWindow.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri("/Fluent;component/Themes/Generic.xaml", UriKind.Relative)));

                        var secondGallery = new ColorGallery();
                        secondGallery.MoreColorsExecuting += (_, args) => args.Color = color;
                        secondWindow.Content = secondGallery;
                        secondWindow.Show();
                        secondGallery.ApplyTemplate();

                        var moreColorsButton = (MenuItem)secondGallery.Template.FindName("PART_MoreColors", secondGallery)
                                               ?? throw new InvalidOperationException("PART_MoreColors not found.");

                        // The user picks a color through "More Colors" in the second window.
                        moreColorsButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent, moreColorsButton));

                        ProcessPendingWork();
                    }
                    catch (Exception exception)
                    {
                        exceptionOnSecondThread ??= exception;
                    }
                    finally
                    {
                        try
                        {
                            secondWindow?.Close();

                            // The views WPF created for RecentColors on this thread stay attached to the static list.
                            // Detach them, so later changes from the test thread don't reach a view of a finished thread.
                            (CollectionViewSource.GetDefaultView(ColorGallery.RecentColors) as CollectionView)?.DetachFromSourceCollection();
                        }
                        catch (Exception exception)
                        {
                            exceptionOnSecondThread ??= exception;
                        }

                        Dispatcher.CurrentDispatcher.InvokeShutdown();
                    }
                });

                secondThread.SetApartmentState(ApartmentState.STA);
                secondThread.IsBackground = true;
                secondThread.Start();

                var finished = secondThread.Join(TimeSpan.FromSeconds(60));
                if (finished == false)
                {
                    secondDispatcher?.InvokeShutdown();
                    secondThread.Join(TimeSpan.FromSeconds(10));
                }

                Assert.That(finished, Is.True, "The second UI thread did not finish in time.");

                // Let the first thread process what the second thread changed.
                ProcessPendingWork();

                Assert.That(exceptionOnSecondThread, Is.Null, "Adding a color through More Colors on the second UI thread failed.");
                Assert.That(exceptionOnFirstThread, Is.Null, "The gallery on the first UI thread failed after the second thread changed RecentColors.");
                Assert.That(ColorGallery.RecentColors, Does.Contain(color));
                Assert.That(firstRecentColorsListBox.Items.Contains(color), Is.True, "The gallery on the first UI thread does not show the new recent color.");
            }
        }
        finally
        {
            Dispatcher.CurrentDispatcher.UnhandledException -= unhandledOnFirstThread;

            // RecentColors is static: restore it for other tests.
            // Clear (a reset) instead of Remove, because without the fix the first thread's list box never saw the color being added.
            ColorGallery.RecentColors.Clear();
            foreach (var originalRecentColor in originalRecentColors)
            {
                ColorGallery.RecentColors.Add(originalRecentColor);
            }

            ProcessPendingWork();
        }
    }

    /// <summary>
    /// Runs everything that is queued on the current thread's dispatcher, including the changes WPF passes over
    /// from other threads (those run at <see cref="DispatcherPriority.ContextIdle"/>, below the usual DoEvents priority).
    /// </summary>
    private static void ProcessPendingWork()
    {
        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle, CancellationToken.None, TimeSpan.FromSeconds(10));
    }
}
