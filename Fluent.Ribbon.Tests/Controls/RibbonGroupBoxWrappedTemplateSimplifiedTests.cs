namespace Fluent.Tests.Controls;

using System.Windows;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// For items bound through ItemsSource the group passed the simplified state and size only to the first element of the
/// item's DataTemplate. When that is a wrapper (here a Border around the button), the button stayed in the classic look
/// in the simplified ribbon. Found by a bug hunt after round 7.
/// </summary>
[TestFixture]
public class RibbonGroupBoxWrappedTemplateSimplifiedTests
{
    [Test]
    public void Button_wrapped_in_a_DataTemplate_follows_the_simplified_ribbon()
    {
        var buttonFactory = new FrameworkElementFactory(typeof(Fluent.Button));
        buttonFactory.SetBinding(Fluent.Button.HeaderProperty, new System.Windows.Data.Binding());
        var borderFactory = new FrameworkElementFactory(typeof(Border));
        borderFactory.AppendChild(buttonFactory);

        var groupBox = new RibbonGroupBox
        {
            Header = "Group",
            ItemsSource = new[] { "Paste" },
            ItemTemplate = new DataTemplate { VisualTree = borderFactory }
        };

        var tabItem = new RibbonTabItem { Header = "Home" };
        tabItem.Groups.Add(groupBox);
        var ribbon = new Ribbon { CanUseSimplified = true, AutomaticStateManagement = false };
        ribbon.Tabs.Add(tabItem);

        using (new TestRibbonWindow(ribbon))
        {
            ribbon.SelectedTabItem = tabItem;
            UIHelper.DoEvents();

            var button = Fluent.Internal.UIHelper.FindVisualChild<Fluent.Button>(groupBox);
            Assert.That(button, Is.Not.Null, "Precondition: the DataTemplate's button is shown");
            Assert.That(button!.IsSimplified, Is.False, "Precondition: the classic ribbon");

            ribbon.IsSimplified = true;
            UIHelper.DoEvents();

            Assert.That(groupBox.IsSimplified, Is.True, "Precondition: the group is simplified");
            Assert.That(button.IsSimplified, Is.True, "The button inside the wrapper must follow the simplified ribbon");

            ribbon.IsSimplified = false;
            UIHelper.DoEvents();

            Assert.That(button.IsSimplified, Is.False, "The button must follow the ribbon back to the classic look");
        }
    }
}
