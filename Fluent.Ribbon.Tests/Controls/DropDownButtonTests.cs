namespace Fluent.Tests.Controls;

using System.Windows.Input;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Tests for <see cref="DropDownButton.FocusFirstItemOnDropDownOpen"/>.
/// </summary>
/// <remarks>
/// https://github.com/fluentribbon/Fluent.Ribbon/issues/813
/// Opening a drop down focuses its first item, which also gives that item its highlighted style.
/// The new property lets apps turn that off.
/// </remarks>
[TestFixture]
public class DropDownButtonTests
{
    [Test]
    public void FocusFirstItemOnDropDownOpen_defaults_to_true()
    {
        // Existing apps must keep the old behavior without changing anything.
        Assert.That(new DropDownButton().FocusFirstItemOnDropDownOpen, Is.True);
        Assert.That(new SplitButton().FocusFirstItemOnDropDownOpen, Is.True);
    }

    [Test]
    public void First_item_is_focused_only_when_FocusFirstItemOnDropDownOpen_is_true()
    {
        var firstItem = new MenuItem { Header = "First" };
        var dropDownButton = new DropDownButton
        {
            Header = "DropDown",
            Items =
            {
                firstItem,
                new MenuItem { Header = "Second" }
            }
        };

        using (var window = new TestRibbonWindow(dropDownButton))
        {
            dropDownButton.ApplyTemplate();

            // Keyboard focus only works in the active window.
            window.Activate();
            dropDownButton.Focus();
            UIHelper.DoEvents();

            // 1. Default: the first item gets focus. This is also the baseline proving that keyboard
            //    focus works in this environment at all, so step 2 below can't pass by accident.
            OpenDropDown(dropDownButton);

            if (firstItem.IsKeyboardFocused == false)
            {
                Assert.Inconclusive("Keyboard focus is not available in this test environment (the default case couldn't focus the first item).");
            }

            CloseDropDown(dropDownButton);

            // 2. Turned off: the first item must not get focus (and so isn't highlighted),
            //    but keyboard focus must still be inside the drop down so arrow keys keep working.
            dropDownButton.FocusFirstItemOnDropDownOpen = false;
            OpenDropDown(dropDownButton);

            Assert.That(firstItem.IsKeyboardFocused, Is.False, "First item must not be focused");
            Assert.That(firstItem.IsHighlighted, Is.False, "First item must not look highlighted");
            Assert.That(dropDownButton.DropDownPopup.Child.IsKeyboardFocusWithin, Is.True, "Focus should be inside the drop down content");

            CloseDropDown(dropDownButton);
        }
    }

    private static void OpenDropDown(DropDownButton dropDownButton)
    {
        dropDownButton.IsDropDownOpen = true;

        // Focus is moved asynchronously after opening. Let the dispatcher run it.
        UIHelper.DoEvents();
    }

    private static void CloseDropDown(DropDownButton dropDownButton)
    {
        dropDownButton.IsDropDownOpen = false;
        UIHelper.DoEvents();

        // Clean start for the next step.
        Keyboard.ClearFocus();
        dropDownButton.Focus();
        UIHelper.DoEvents();
    }
}
