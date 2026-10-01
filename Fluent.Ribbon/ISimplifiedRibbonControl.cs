namespace Fluent;

/// <summary>
/// Base interface for controls supports simplified state
/// </summary>
public interface ISimplifiedRibbonControl : ISimplifiedStateControl
{
    /// <summary>
    /// Gets or sets SimplifiedSizeDefinition for element on Simplified mode
    /// </summary>
    RibbonControlSizeDefinition SimplifiedSizeDefinition { get; set; }

    /// <summary>
    /// Gets whether the control is in simplified mode (set when the ribbon switches to simplified mode).
    /// </summary>
    bool IsSimplified { get; }
}