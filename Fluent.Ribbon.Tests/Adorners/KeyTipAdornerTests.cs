namespace Fluent.Tests.Adorners;

using System.Windows.Controls;
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

    [Test(Description = "Terminating an adorner which still waits for its element to be loaded must cancel the pending attach")]
    public void Terminate_should_cancel_an_attach_waiting_for_Loaded()
    {
        // Fully qualified, because "Button" would resolve to System.Windows.Controls.Button here.
        var button = new Fluent.Button
        {
            KeyTip = "A"
        };
        var panel = new Grid();
        panel.Children.Add(button);

        var adorner = new KeyTipAdorner(button, panel, null);

        Assert.That(button.IsLoaded, Is.False, "Precondition: the element must not be loaded yet.");

        // Attach has to wait for Loaded because the element is not loaded yet.
        adorner.Attach();

        Assert.That(adorner.IsAdornerChainAlive, Is.True, "Precondition: the adorner must be waiting for Loaded.");

        adorner.Terminate(KeyTipPressedResult.Empty);

        Assert.That(adorner.IsAdornerChainAlive, Is.False, "A terminated adorner must no longer wait for Loaded.");

        using (new TestRibbonWindow(panel))
        {
            Assert.That(button.IsLoaded, Is.True, "Precondition: the element must be loaded now.");

            // Loaded must not attach the terminated adorner, otherwise key tips of a dead chain would show up.
            Assert.That(adorner.IsAdornerChainAlive, Is.False, "A terminated adorner must not attach once its element gets loaded.");
        }
    }
}