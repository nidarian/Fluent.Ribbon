namespace Fluent.Automation.Peers;

using System.Windows;

/// <summary>
/// Shared logic for mapping ribbon specific concepts (KeyTips, ScreenTips) to UI Automation properties.
/// </summary>
/// <remarks>
/// Screen readers announce AccessKey and HelpText. Ribbon controls carry that information in <see cref="KeyTip"/> and <see cref="ScreenTip"/>,
/// which UI Automation knows nothing about, so each peer has to translate them.
/// </remarks>
internal static class AutomationPeerHelper
{
    /// <summary>
    /// Gets the access key for <paramref name="owner"/>.
    /// </summary>
    /// <param name="owner">The element owning the automation peer.</param>
    /// <param name="baseValue">The value from the base peer. It contains an explicit AutomationProperties.AccessKey, so it wins if set.</param>
    /// <returns><paramref name="baseValue"/> if it's not empty, otherwise the KeyTip keys if set, otherwise <paramref name="baseValue"/>.</returns>
    public static string? GetAccessKey(UIElement owner, string? baseValue)
    {
        if (string.IsNullOrEmpty(baseValue) == false)
        {
            return baseValue;
        }

        var keys = KeyTip.GetKeys(owner);

        return string.IsNullOrEmpty(keys)
            ? baseValue
            : keys;
    }

    /// <summary>
    /// Gets the help text for <paramref name="owner"/>.
    /// </summary>
    /// <param name="owner">The element owning the automation peer.</param>
    /// <param name="baseValue">The value from the base peer. It contains an explicit AutomationProperties.HelpText (or a plain string ToolTip), so it wins if set.</param>
    /// <returns><paramref name="baseValue"/> if it's not empty, otherwise the text of a <see cref="ScreenTip"/> ToolTip.</returns>
    public static string GetHelpText(UIElement owner, string? baseValue)
    {
        if (string.IsNullOrEmpty(baseValue) == false)
        {
            return baseValue!;
        }

        // ScreenTip.Text is object, but only plain text can be announced
        if ((owner as FrameworkElement)?.ToolTip is ScreenTip screenTip)
        {
            return screenTip.Text as string ?? string.Empty;
        }

        return baseValue ?? string.Empty;
    }
}