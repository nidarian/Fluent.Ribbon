namespace Fluent.Tests.Data;

using System;
using System.IO;
using System.IO.IsolatedStorage;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class RibbonStateStorageTests
{
    /// <summary>
    /// The temporary state is kept in a MemoryStream that is reused for every save.
    /// SaveTemporary rewinds it (Position = 0) and writes the new state, but it never cut the
    /// stream to the new length. A shorter state then kept the tail of the longer one:
    /// "True,True,True" written over "False,False,False" leaves "True,True,Truelse",
    /// and the last value no longer parses, so LoadTemporary silently skips it.
    /// </summary>
    [Test]
    public void LoadTemporary_restores_a_state_that_is_shorter_than_the_previous_one()
    {
        // CanUseSimplified must be on, otherwise loading ignores IsSimplified entirely.
        var ribbon = new Ribbon { CanUseSimplified = true };
        var storage = ribbon.RibbonStateStorage;

        // Longest possible state: "False,False,False" (17 characters).
        SetState(ribbon, false);
        storage.SaveTemporary();

        // Shorter state: "True,True,True" (14 characters), written over the longer one.
        SetState(ribbon, true);
        storage.SaveTemporary();

        // Change the state, then restore the saved one.
        // ShowQuickAccessToolBarAboveRibbon is left alone on purpose: changing it calls SaveTemporary
        // by itself (Ribbon.OnShowQuickAccessToolBarAboveRibbonChanged), which would overwrite the
        // state saved above before it's loaded.
        ribbon.IsMinimized = false;
        ribbon.IsSimplified = false;
        storage.LoadTemporary();

        Assert.That(ribbon.IsMinimized, Is.True, nameof(Ribbon.IsMinimized));
        Assert.That(ribbon.ShowQuickAccessToolBarAboveRibbon, Is.True, nameof(Ribbon.ShowQuickAccessToolBarAboveRibbon));
        Assert.That(ribbon.IsSimplified, Is.True, nameof(Ribbon.IsSimplified) + " (the last value, where the old text is left over)");
    }

    // The three values RibbonStateStorage saves, all set to the same value.
    // Note: changing ShowQuickAccessToolBarAboveRibbon also saves the temporary state by itself.
    private static void SetState(Ribbon ribbon, bool value)
    {
        ribbon.IsMinimized = value;
        ribbon.ShowQuickAccessToolBarAboveRibbon = value;
        ribbon.IsSimplified = value;
    }

    [Test(Description = "Enabling AutomaticStateManagement after the ribbon was loaded must load the persisted state")]
    public void State_is_loaded_when_AutomaticStateManagement_is_enabled_after_the_ribbon_was_loaded()
    {
        var ribbon = new TestRibbon
        {
            // A unique name gives a unique isolated storage file, so we never read or clobber state of other tests or apps.
            Name = "RibbonStateStorageTests_" + Guid.NewGuid().ToString("N"),
            AutomaticStateManagement = false,
            CanMinimize = true
        };

        string fileName = null;

        try
        {
            using (new TestRibbonWindow(ribbon))
            {
                Assert.That(ribbon.IsLoaded, Is.True, "Precondition: the ribbon must be loaded, so Ribbon.OnLoaded already tried to load the state.");
                Assert.That(ribbon.IsMinimized, Is.False, "Precondition: the ribbon starts not minimized.");

                // The file name depends on the window, so it must be read after the ribbon is inside the window.
                fileName = ((TestRibbonStateStorage)ribbon.RibbonStateStorage).FileName;

                // Simulate state persisted by an earlier session: IsMinimized, ShowQuickAccessToolBarAboveRibbon, IsSimplified.
                TestRibbonStateStorage.WriteStateFile(fileName, "True,False,False");

                ribbon.AutomaticStateManagement = true;

                Assert.That(ribbon.IsMinimized, Is.True, "The persisted state should be loaded once AutomaticStateManagement gets enabled.");
            }
        }
        finally
        {
            // Closing the window saves the state again, so clean up only after the window is gone.
            if (fileName is not null)
            {
                TestRibbonStateStorage.DeleteStateFile(fileName);
            }
        }
    }

    /// <summary>
    /// With AutomaticStateManagement off, RibbonStateStorage.Load doesn't mark the state as loaded
    /// (so enabling AutomaticStateManagement later still reads the saved state).
    /// Ribbon.LoadInitialState used IsLoaded to run only once, so it ran again on every Loaded
    /// and selected the first tab again whenever no tab was selected,
    /// undoing an app's choice to have no tab selected after the ribbon was moved to another parent.
    /// The first tab must only be selected on the first Loaded, as before.
    /// </summary>
    [Test]
    public void First_tab_is_not_selected_again_when_the_ribbon_is_loaded_again_with_AutomaticStateManagement_disabled()
    {
        var firstTab = new RibbonTabItem { Header = "First" };
        var ribbon = new Ribbon
        {
            AutomaticStateManagement = false,
            Tabs =
            {
                firstTab,
                new RibbonTabItem { Header = "Second" }
            }
        };

        using (var window = new TestRibbonWindow(ribbon))
        {
            Assert.That(ribbon.IsLoaded, Is.True, "Precondition: the ribbon must be loaded.");
            Assert.That(ribbon.SelectedTabItem, Is.SameAs(firstTab), "Precondition: the first Loaded selects the first tab.");

            // The app chooses to have no tab selected.
            ribbon.SelectedTabItem = null;
            UIHelper.DoEvents();

            Assert.That(ribbon.SelectedTabItem, Is.Null, "Precondition: the app cleared the selected tab.");

            // Move the ribbon out of the window and back in (unloaded, then loaded again).
            window.Content = null;
            UIHelper.DoEvents();
            Assert.That(ribbon.IsLoaded, Is.False, "Precondition: the ribbon must be unloaded.");

            window.Content = ribbon;
            UIHelper.DoEvents();
            Assert.That(ribbon.IsLoaded, Is.True, "Precondition: the ribbon must be loaded again.");

            Assert.That(ribbon.SelectedTabItem, Is.Null, "Loading the ribbon again must not select the first tab again.");
        }
    }

    private sealed class TestRibbon : Ribbon
    {
        protected override IRibbonStateStorage CreateRibbonStateStorage()
        {
            return new TestRibbonStateStorage(this);
        }
    }

    private sealed class TestRibbonStateStorage : RibbonStateStorage
    {
        public TestRibbonStateStorage(Ribbon ribbon)
            : base(ribbon)
        {
        }

        public string FileName => this.IsolatedStorageFileName;

        public static void WriteStateFile(string fileName, string content)
        {
            var storage = GetIsolatedStorageFile();

            using (var stream = new IsolatedStorageFileStream(fileName, FileMode.Create, FileAccess.Write, storage))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(content);
            }
        }

        public static void DeleteStateFile(string fileName)
        {
            var storage = GetIsolatedStorageFile();

            if (IsolatedStorageFileExists(storage, fileName))
            {
                storage.DeleteFile(fileName);
            }
        }
    }
}
