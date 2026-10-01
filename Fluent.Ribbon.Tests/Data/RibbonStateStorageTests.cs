namespace Fluent.Tests.Data;

using System;
using System.IO;
using System.IO.IsolatedStorage;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class RibbonStateStorageTests
{
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