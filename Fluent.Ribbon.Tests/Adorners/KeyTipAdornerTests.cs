namespace Fluent.Tests.Adorners;

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public class KeyTipAdornerTests
{
    [Test]
    public void Adorner_Should_Properly_Grab_Keys_From_KeyTipInformationProvider()
    {
        {
            var splitButton = new SplitButton();
            var panel = new Grid();
            panel.Children.Add(splitButton);
            using (var window = new TestRibbonWindow(panel))
            {
                var adorner = new KeyTipAdorner(splitButton, panel, null);

                Assert.That(adorner.KeyTipInformations, Has.Count.EqualTo(0));
            }
        }

        {
            var splitButton = new SplitButton
            {
                KeyTip = "A"
            };
            var panel = new Grid();
            panel.Children.Add(splitButton);

            using (var window = new TestRibbonWindow(panel))
            {
                var adorner = new KeyTipAdorner(splitButton, panel, null);

                Assert.That(adorner.KeyTipInformations, Has.Count.EqualTo(2));
                Assert.That(adorner.KeyTipInformations[0].Keys, Is.EqualTo("AA"));
                Assert.That(adorner.KeyTipInformations[1].Keys, Is.EqualTo("AB"));
            }
        }

        {
            var splitButton = new SplitButton
            {
                SecondaryKeyTip = "B"
            };
            var panel = new Grid();
            panel.Children.Add(splitButton);

            using (var window = new TestRibbonWindow(panel))
            {
                var adorner = new KeyTipAdorner(splitButton, panel, null);

                Assert.That(adorner.KeyTipInformations, Has.Count.EqualTo(1));
                Assert.That(adorner.KeyTipInformations[0].Keys, Is.EqualTo("B"));
            }
        }
    }

    /// <summary>
    /// A group's own KeyTip (like "ZC") is only shown while the group is collapsed. While it's
    /// expanded, the KeyTip is hidden and can't be pressed: activation only considers visible KeyTips.
    /// The prefix check didn't look at visibility, so typing "Z" counted as the start of the hidden "ZC".
    /// The service then kept waiting for more keys while every KeyTip was hidden, and the key was swallowed.
    /// </summary>
    [Test]
    public void Hidden_KeyTips_do_not_count_as_a_prefix()
    {
        var button = new Fluent.Button { Header = "Button", KeyTip = "B" };

        // Expanded group (not Collapsed): its own KeyTip is hidden, its button's KeyTip is shown.
        var groupBox = new RibbonGroupBox { Header = "Group" };
        KeyTip.SetKeys(groupBox, "ZC");
        groupBox.Items.Add(button);

        var panel = new StackPanel { Children = { groupBox } };

        using (new TestRibbonWindow(panel))
        {
            groupBox.ApplyTemplate();
            UIHelper.DoEvents();

            Assert.That(groupBox.State, Is.Not.EqualTo(RibbonGroupBoxState.Collapsed), "Precondition: the group is expanded");

            var adorner = new KeyTipAdorner(panel, panel, null);

            var groupKeyTip = adorner.KeyTipInformations.Single(x => ReferenceEquals(x.AssociatedElement, groupBox));
            Assert.That(groupKeyTip.Visibility, Is.Not.EqualTo(Visibility.Visible), "Precondition: an expanded group's own KeyTip is hidden");

            // Visible KeyTips still match.
            Assert.That(adorner.ContainsKeyTipStartingWith("B"), Is.True);

            // The hidden one can't be pressed, so it must not keep the input going either.
            Assert.That(adorner.ContainsKeyTipStartingWith("Z"), Is.False, "Hidden KeyTip \"ZC\" must not match the prefix \"Z\"");
        }
    }
}
