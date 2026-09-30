namespace Fluent.Tests.Data;

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
}
