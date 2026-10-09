namespace Fluent.Tests.Controls;

using System.Linq;
using System.Windows.Automation;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for the localized captions of the caption buttons (minimize, maximize, restore, close) in <see cref="WindowCommands"/>.
/// The captions are loaded from user32 and are used as Uid, <see cref="AutomationProperties.NameProperty"/> and tooltip of the buttons.
/// </summary>
[TestFixture]
public class WindowCommandsTests
{
    [TestCase("Minimize")]
    [TestCase("Maximize")]
    [TestCase("Restore")]
    [TestCase("Close")]
    public void Caption_Has_No_Trailing_Nul_Characters(string caption)
    {
        using var windowCommands = new WindowCommands();

        var text = caption switch
        {
            "Minimize" => windowCommands.Minimize,
            "Maximize" => windowCommands.Maximize,
            "Restore" => windowCommands.Restore,
            _ => windowCommands.Close,
        };

        Assert.That(text, Is.Not.Null.And.Not.Empty);

        // Count the characters on purpose: a culture sensitive substring search ignores '\0'.
        Assert.That(text!.Count(c => c == '\0'), Is.Zero, $"The caption '{caption}' must not contain NUL characters, but its length was {text.Length}.");
    }

    [TestCase("PART_Min")]
    [TestCase("PART_Max")]
    [TestCase("PART_Restore")]
    [TestCase("PART_Close")]
    public void Caption_Button_Automation_Name_Has_No_Nul_Characters(string partName)
    {
        using (var window = new TestRibbonWindow())
        {
            UIHelper.DoEvents();

            var windowCommands = window.WindowCommands;
            Assert.That(windowCommands, Is.Not.Null);

            windowCommands!.ApplyTemplate();

            var button = windowCommands.Template.FindName(partName, windowCommands) as Button;
            Assert.That(button, Is.Not.Null);

            var name = AutomationProperties.GetName(button);

            Assert.That(name, Is.Not.Null.And.Not.Empty);
            Assert.That(name.Count(c => c == '\0'), Is.Zero, $"The automation name of '{partName}' must not contain NUL characters, but its length was {name.Length}.");
        }
    }
}
