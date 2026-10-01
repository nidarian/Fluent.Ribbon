namespace Fluent;

using System;

/// <summary>
/// Interface for handling loading and saving the state of a <see cref="Ribbon"/>.
/// </summary>
public interface IRibbonStateStorage : IDisposable
{
    /// <summary>
    /// Gets whether state is currently loading.
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Gets whether state is loaded.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Save current state to a temporary storage.
    /// </summary>
    void SaveTemporary();

    /// <summary>
    /// Save current state to a persistent storage.
    /// </summary>
    void Save();

    /// <summary>
    /// Load state from a temporary storage.
    /// Fluent.Ribbon never calls this itself.
    /// </summary>
    void LoadTemporary();

    /// <summary>
    /// Loads the state from a persistent storage.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="IsLoaded" /> when finished, also when loading fails or is disabled,
    /// so that <see cref="Save" /> does not overwrite stored state before it was loaded.
    /// </remarks>
    void Load();

    /// <summary>
    /// Resets saved state.
    /// </summary>
    void Reset();
}