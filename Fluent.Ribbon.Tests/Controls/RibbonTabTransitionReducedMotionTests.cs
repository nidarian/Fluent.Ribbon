namespace Fluent.Tests.Controls;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Switching ribbon tabs slides and fades the new tab's content in. Windows has a setting to turn animations off
/// (Settings, Accessibility, Visual effects, "Animation effects"), which people with motion sensitivity use.
/// The backstage follows it; the tab switch must follow it too.
/// </summary>
[TestFixture]
public class RibbonTabTransitionReducedMotionTests
{
    [Test]
    public void Tab_transition_follows_the_Windows_animation_setting_by_default()
    {
        var ribbon = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            var transition = GetTransitioningControl(ribbon);
            var property = GetAreAnimationsEnabledProperty();

            Assert.That(transition.GetValue(property), Is.EqualTo(SystemParameters.ClientAreaAnimation), "The tab transition must follow the Windows animation setting");
        }
    }

    // The setting is looked up like a resource, so a value further up the tree stands in for Windows changing it while the app runs.
    [Test]
    public void Tab_transition_follows_a_change_of_the_animation_setting_while_the_app_runs()
    {
        var ribbon = CreateRibbon();

        using (var window = new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            var transition = GetTransitioningControl(ribbon);
            var property = GetAreAnimationsEnabledProperty();

            window.Resources[SystemParameters.ClientAreaAnimationKey] = false;
            UIHelper.DoEvents();
            Assert.That(transition.GetValue(property), Is.False, "Animations turned off: the tab transition must stop animating");

            window.Resources[SystemParameters.ClientAreaAnimationKey] = true;
            UIHelper.DoEvents();
            Assert.That(transition.GetValue(property), Is.True, "Animations turned on again: the tab transition must animate again");
        }
    }

    [Test]
    public void With_animations_off_switching_tabs_shows_the_new_content_at_once()
    {
        var ribbon = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            var transition = GetTransitioningControl(ribbon);
            transition.SetValue(GetAreAnimationsEnabledProperty(), false);

            var current = (ContentPresenter)transition.Template.FindName(TransitioningControl.CurrentContentPartName, transition);
            var previous = (ContentPresenter)transition.Template.FindName(TransitioningControl.PreviousContentPartName, transition);
            Assert.That(current.Content, Is.Not.Null, "Precondition: the first tab's content is shown");
            var oldContent = current.Content;

            ribbon.SelectedTabIndex = 1;

            Assert.That(current.Content, Is.Not.Null.And.Not.SameAs(oldContent), "The second tab's content must be shown");
            Assert.That(previous.Content, Is.Null, "The old content must be gone at once, not fade out");
            Assert.That(current.HasAnimatedProperties, Is.False, "The new content must not slide or fade in");
            Assert.That(previous.HasAnimatedProperties, Is.False, "Nothing must be animated");
        }
    }

    // Guard: with animations on, the transition still runs (this also shows the test above can see a transition).
    [Test]
    public void With_animations_on_switching_tabs_still_runs_the_transition()
    {
        var ribbon = CreateRibbon();

        using (new TestRibbonWindow(ribbon))
        {
            UIHelper.DoEvents();

            var transition = GetTransitioningControl(ribbon);

            // The test machine may have animations turned off in Windows; turn them on for this control where the property exists.
            var descriptor = DependencyPropertyDescriptor.FromName("AreAnimationsEnabled", typeof(TransitioningControl), typeof(TransitioningControl));
            if (descriptor is not null)
            {
                transition.SetValue(descriptor.DependencyProperty, true);
            }

            var current = (ContentPresenter)transition.Template.FindName(TransitioningControl.CurrentContentPartName, transition);
            var previous = (ContentPresenter)transition.Template.FindName(TransitioningControl.PreviousContentPartName, transition);
            var oldContent = current.Content;
            Assert.That(oldContent, Is.Not.Null, "Precondition: the first tab's content is shown");

            ribbon.SelectedTabIndex = 1;

            Assert.That(current.Content, Is.Not.Null.And.Not.SameAs(oldContent), "The second tab's content must be shown");
            Assert.That(previous.Content, Is.SameAs(oldContent), "The old content must still be there to fade out");
            Assert.That(current.HasAnimatedProperties, Is.True, "The new content must slide and fade in");
        }
    }

    private static Ribbon CreateRibbon()
    {
        var ribbon = new Ribbon();

        foreach (var header in new[] { "Home", "Insert" })
        {
            var tab = new RibbonTabItem { Header = header };
            var group = new RibbonGroupBox { Header = header + " group" };
            group.Items.Add(new Fluent.Button { Header = header + " button" });
            tab.Groups.Add(group);
            ribbon.Tabs.Add(tab);
        }

        return ribbon;
    }

    private static TransitioningControl GetTransitioningControl(Ribbon ribbon)
    {
        var tabControl = ribbon.TabControl;
        Assert.That(tabControl, Is.Not.Null, "Precondition: the ribbon has its tab control");

        var transition = tabControl.Template.FindName("PART_SelectedContent", tabControl) as TransitioningControl;
        Assert.That(transition, Is.Not.Null, "Precondition: the tab control shows its content in a TransitioningControl");
        Assert.That(transition.TransitionStoryboard, Is.Not.Null, "Precondition: the tab control has a transition");

        return transition;
    }

    // Looked up by name, so the tests also build against a library that doesn't have the property yet (and fail there).
    private static DependencyProperty GetAreAnimationsEnabledProperty()
    {
        var descriptor = DependencyPropertyDescriptor.FromName("AreAnimationsEnabled", typeof(TransitioningControl), typeof(TransitioningControl));
        Assert.That(descriptor, Is.Not.Null, "TransitioningControl needs an AreAnimationsEnabled property, like the backstage has");

        return descriptor.DependencyProperty;
    }
}
