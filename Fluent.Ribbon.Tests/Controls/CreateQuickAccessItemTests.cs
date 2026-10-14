namespace Fluent.Tests.Controls;

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Media;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;
using ButtonBase = System.Windows.Controls.Primitives.ButtonBase;

/// <summary>
/// Tests for all implementations of <see cref="IQuickAccessItemProvider.CreateQuickAccessItem"/>.
/// </summary>
[TestFixture]
public class CreateQuickAccessItemTests
{
    #region Button, ToggleButton, CheckBox, RadioButton

    [TestCase(typeof(Button))]
    [TestCase(typeof(ToggleButton))]
    [TestCase(typeof(CheckBox))]
    [TestCase(typeof(RadioButton))]
    public void ButtonLike_Creates_Bound_Item_Of_Same_Type(Type type)
    {
        var command = new RoutedCommand();

        var source = (System.Windows.Controls.Control)Activator.CreateInstance(type);
        source.SetValue(RibbonControl.HeaderProperty, "Header");
        source.SetValue(RibbonControl.IconProperty, "Icon");
        source.SetValue(LargeIconProviderProperties.LargeIconProperty, "LargeIcon");
        source.SetValue(ButtonBase.CommandProperty, command);
        source.SetValue(RibbonProperties.SizeProperty, RibbonControlSize.Large);

        var item = ((IQuickAccessItemProvider)source).CreateQuickAccessItem();

        Assert.That(item, Is.TypeOf(type));
        Assert.That(item, Is.Not.SameAs(source));

        Assert.That(item.GetValue(RibbonControl.HeaderProperty), Is.EqualTo("Header"));
        Assert.That(item.GetValue(RibbonControl.IconProperty), Is.EqualTo("Icon"));
        Assert.That(item.GetValue(LargeIconProviderProperties.LargeIconProperty), Is.EqualTo("LargeIcon"));
        Assert.That(item.GetValue(ButtonBase.CommandProperty), Is.SameAs(command));
        Assert.That(item.ToolTip, Is.EqualTo("Header"));
        Assert.That(RibbonProperties.GetSize(item), Is.EqualTo(RibbonControlSize.Small));

        source.SetValue(RibbonControl.HeaderProperty, "Changed");

        Assert.That(item.GetValue(RibbonControl.HeaderProperty), Is.EqualTo("Changed"));
    }

    // The Quick Access Toolbar copy is a separate element with its own automation peer.
    // Screen reader users must hear the same accessible name and help text for it as for the original control.
    [TestCase(typeof(Button))]
    [TestCase(typeof(ToggleButton))]
    [TestCase(typeof(CheckBox))]
    [TestCase(typeof(RadioButton))]
    public void ButtonLike_Copies_AutomationProperties_Name_And_HelpText(Type type)
    {
        var source = (System.Windows.Controls.Control)Activator.CreateInstance(type);
        AutomationProperties.SetName(source, "Save");
        AutomationProperties.SetHelpText(source, "Saves");

        var item = ((IQuickAccessItemProvider)source).CreateQuickAccessItem();

        Assert.That(item, Is.Not.SameAs(source), "Precondition: the quick access item must be a separate copy.");

        Assert.That(AutomationProperties.GetName(item), Is.EqualTo("Save"), "The quick access item must have the same AutomationProperties.Name as its source.");
        Assert.That(AutomationProperties.GetHelpText(item), Is.EqualTo("Saves"), "The quick access item must have the same AutomationProperties.HelpText as its source.");

        AutomationProperties.SetName(source, "Save all");

        Assert.That(AutomationProperties.GetName(item), Is.EqualTo("Save all"), "Changes to the source's AutomationProperties.Name must flow to the quick access item.");
    }

    [TestCase(typeof(Button))]
    [TestCase(typeof(ToggleButton))]
    [TestCase(typeof(CheckBox))]
    [TestCase(typeof(RadioButton))]
    public void ButtonLike_Forwards_Click_To_Source(Type type)
    {
        var source = (ButtonBase)Activator.CreateInstance(type);

        var clickCount = 0;
        source.Click += (_, _) => clickCount++;

        var item = (ButtonBase)((IQuickAccessItemProvider)source).CreateQuickAccessItem();

        item.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, item));

        Assert.That(clickCount, Is.EqualTo(1));
    }

    [TestCase(typeof(ToggleButton))]
    [TestCase(typeof(CheckBox))]
    [TestCase(typeof(RadioButton))]
    public void ToggleButtonLike_Syncs_IsChecked_TwoWay(Type type)
    {
        var source = (System.Windows.Controls.Primitives.ToggleButton)Activator.CreateInstance(type);
        source.IsChecked = true;

        var item = (System.Windows.Controls.Primitives.ToggleButton)((IQuickAccessItemProvider)source).CreateQuickAccessItem();

        Assert.That(item.IsChecked, Is.True);

        source.IsChecked = false;
        Assert.That(item.IsChecked, Is.False);

        item.IsChecked = true;
        Assert.That(source.IsChecked, Is.True);
    }

    [TestCase(null, null)]
    [TestCase("GroupA", "GroupB")]
    public void RadioButton_Quick_Access_Items_From_Different_Groups_Do_Not_Uncheck_Each_Other(string groupNameA, string groupNameB)
    {
        // Without a GroupName, WPF groups radio buttons by their logical parent.
        // So the sources live in different panels (= different groups), but all quick access items share one panel (like the toolbar).
        var sourceA = new RadioButton { GroupName = groupNameA };
        var sourceB = new RadioButton { GroupName = groupNameB, IsChecked = true };

        var panelA = new System.Windows.Controls.StackPanel();
        panelA.Children.Add(sourceA);

        var panelB = new System.Windows.Controls.StackPanel();
        panelB.Children.Add(sourceB);

        var itemA = (RadioButton)sourceA.CreateQuickAccessItem();
        var itemB = (RadioButton)sourceB.CreateQuickAccessItem();

        var quickAccessPanel = new System.Windows.Controls.StackPanel();
        quickAccessPanel.Children.Add(itemA);
        quickAccessPanel.Children.Add(itemB);

        using (CreateWindow(panelA, panelB, quickAccessPanel))
        {
            Assert.That(sourceB.IsChecked, Is.True, "Precondition: source B is checked.");
            Assert.That(itemB.IsChecked, Is.True, "Precondition: item B mirrors source B.");

            itemA.IsChecked = true;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(sourceA.IsChecked, Is.True);
                Assert.That(itemA.IsChecked, Is.True);
                Assert.That(sourceB.IsChecked, Is.True);
                Assert.That(itemB.IsChecked, Is.True);
            }
        }
    }

    // A checked ToggleButton with a GroupName stays checked when it is clicked again (#481),
    // so a group like "align left/center/right" always has exactly one selected button.
    // The quick access item must behave the same, otherwise clicking it unchecks the original through the IsChecked binding.
    [Test]
    public void ToggleButton_With_GroupName_Stays_Checked_When_Its_Checked_Quick_Access_Item_Is_Clicked()
    {
        var sourceA = new ToggleButton { GroupName = "g", IsChecked = true };
        var sourceB = new ToggleButton { GroupName = "g" };

        var itemA = (ToggleButton)sourceA.CreateQuickAccessItem();
        var itemB = (ToggleButton)sourceB.CreateQuickAccessItem();

        using (CreateWindow(sourceA, sourceB, itemA, itemB))
        {
            Assert.That(itemA.IsChecked, Is.True, "Precondition: item A mirrors source A.");

            itemA.InvokeClick();
            UIHelper.DoEvents();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(sourceA.IsChecked, Is.True, "Clicking the checked quick access item must not uncheck the original.");
                Assert.That(itemA.IsChecked, Is.True);
                Assert.That(sourceB.IsChecked, Is.False);
                Assert.That(itemB.IsChecked, Is.False);
            }
        }
    }

    [Test]
    public void ToggleButton_With_GroupName_Clicking_Unchecked_Quick_Access_Item_Checks_Its_Source_And_Unchecks_Others()
    {
        var sourceA = new ToggleButton { GroupName = "g", IsChecked = true };
        var sourceB = new ToggleButton { GroupName = "g" };

        var itemA = (ToggleButton)sourceA.CreateQuickAccessItem();
        var itemB = (ToggleButton)sourceB.CreateQuickAccessItem();

        using (CreateWindow(sourceA, sourceB, itemA, itemB))
        {
            itemB.InvokeClick();
            UIHelper.DoEvents();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(sourceB.IsChecked, Is.True);
                Assert.That(itemB.IsChecked, Is.True);
                Assert.That(sourceA.IsChecked, Is.False);
                Assert.That(itemA.IsChecked, Is.False);
            }
        }
    }

    #endregion

    #region TextBox

    [Test]
    public void TextBox_Creates_Bound_TextBox()
    {
        var source = new TextBox
        {
            Header = "Header",
            Icon = "Icon",
            Text = "Text"
        };

        var item = source.CreateQuickAccessItem();

        Assert.That(item, Is.TypeOf<TextBox>());
        Assert.That(item, Is.Not.SameAs(source));

        var textBox = (TextBox)item;

        Assert.That(textBox.Header, Is.EqualTo("Header"));
        Assert.That(textBox.Icon, Is.EqualTo("Icon"));
        Assert.That(textBox.Text, Is.EqualTo("Text"));
        Assert.That(textBox.Size, Is.EqualTo(RibbonControlSize.Small));

        source.IsReadOnly = true;
        source.CharacterCasing = System.Windows.Controls.CharacterCasing.Upper;
        source.MaxLength = 42;
        source.TextAlignment = TextAlignment.Right;
        source.TextDecorations = TextDecorations.Underline;
        source.IsUndoEnabled = false;
        source.UndoLimit = 5;
        source.AutoWordSelection = true;
        source.SelectionBrush = Brushes.Red;
        source.SelectionOpacity = 0.3;
        source.CaretBrush = Brushes.Green;

        Assert.That(textBox.IsReadOnly, Is.True);
        Assert.That(textBox.CharacterCasing, Is.EqualTo(System.Windows.Controls.CharacterCasing.Upper));
        Assert.That(textBox.MaxLength, Is.EqualTo(42));
        Assert.That(textBox.TextAlignment, Is.EqualTo(TextAlignment.Right));
        Assert.That(textBox.TextDecorations, Is.SameAs(TextDecorations.Underline));
        Assert.That(textBox.IsUndoEnabled, Is.False);
        Assert.That(textBox.UndoLimit, Is.EqualTo(5));
        Assert.That(textBox.AutoWordSelection, Is.True);
        Assert.That(textBox.SelectionBrush, Is.SameAs(Brushes.Red));
        Assert.That(textBox.SelectionOpacity, Is.EqualTo(0.3));
        Assert.That(textBox.CaretBrush, Is.SameAs(Brushes.Green));
    }

    [Test]
    public void TextBox_Syncs_Text_TwoWay()
    {
        var source = new TextBox();

        var textBox = (TextBox)source.CreateQuickAccessItem();

        source.Text = "FromSource";
        Assert.That(textBox.Text, Is.EqualTo("FromSource"));

        textBox.Text = "FromQuickAccessItem";
        Assert.That(source.Text, Is.EqualTo("FromQuickAccessItem"));
    }

    [Test]
    public void TextBox_Updates_Source_Binding_Of_Original_When_Quick_Access_Item_Loses_Focus()
    {
        var viewModel = new NameViewModel { Name = "Initial" };

        // Default binding for Text: TwoWay with UpdateSourceTrigger.LostFocus.
        // The original never gets/loses focus while the user types into the quick access item.
        var source = new TextBox();
        source.SetBinding(System.Windows.Controls.TextBox.TextProperty, new System.Windows.Data.Binding(nameof(NameViewModel.Name)) { Source = viewModel });

        Assert.That(source.Text, Is.EqualTo("Initial"), "Precondition: original is bound to the view model.");

        var textBox = (TextBox)source.CreateQuickAccessItem();

        textBox.Text = "Typed";

        Assert.That(source.Text, Is.EqualTo("Typed"), "Precondition: quick access item writes into the original.");

        textBox.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, textBox));

        Assert.That(viewModel.Name, Is.EqualTo("Typed"));
    }

    [Test]
    public void TextBox_Does_Not_Update_Explicit_Source_Binding_Of_Original_When_Quick_Access_Item_Loses_Focus()
    {
        var viewModel = new NameViewModel { Name = "Initial" };

        // The application only commits on its own (e.g. a Save action) by calling UpdateSource itself.
        var source = new TextBox();
        source.SetBinding(System.Windows.Controls.TextBox.TextProperty, new System.Windows.Data.Binding(nameof(NameViewModel.Name)) { Source = viewModel, UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.Explicit });

        Assert.That(source.Text, Is.EqualTo("Initial"), "Precondition: original is bound to the view model.");

        var textBox = (TextBox)source.CreateQuickAccessItem();

        textBox.Text = "Typed";

        Assert.That(source.Text, Is.EqualTo("Typed"), "Precondition: quick access item writes into the original.");

        textBox.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, textBox));

        Assert.That(viewModel.Name, Is.EqualTo("Initial"), "An explicit binding must not be committed just because the quick access item lost focus.");

        // The application's own commit still works.
        System.Windows.Data.BindingOperations.GetBindingExpression(source, System.Windows.Controls.TextBox.TextProperty)!.UpdateSource();

        Assert.That(viewModel.Name, Is.EqualTo("Typed"));
    }

    #endregion

    #region Spinner

    [Test]
    public void Spinner_Creates_Bound_Spinner()
    {
        var source = new Spinner
        {
            Header = "Header",
            Icon = "Icon",
            Minimum = 1,
            Maximum = 100,
            Value = 10,
            Increment = 2,
            Format = "F2",
            Delay = 123,
            Interval = 45
        };

        var item = source.CreateQuickAccessItem();

        Assert.That(item, Is.TypeOf<Spinner>());
        Assert.That(item, Is.Not.SameAs(source));

        var spinner = (Spinner)item;

        Assert.That(spinner.Header, Is.EqualTo("Header"));
        Assert.That(spinner.Icon, Is.EqualTo("Icon"));
        Assert.That(spinner.Minimum, Is.EqualTo(1));
        Assert.That(spinner.Maximum, Is.EqualTo(100));
        Assert.That(spinner.Value, Is.EqualTo(10));
        Assert.That(spinner.Increment, Is.EqualTo(2));
        Assert.That(spinner.Format, Is.EqualTo("F2"));
        Assert.That(spinner.Delay, Is.EqualTo(123));
        Assert.That(spinner.Interval, Is.EqualTo(45));
        Assert.That(spinner.Size, Is.EqualTo(RibbonControlSize.Small));
    }

    [Test]
    public void Spinner_Syncs_Value_TwoWay()
    {
        var source = new Spinner
        {
            Minimum = 0,
            Maximum = 100
        };

        var spinner = (Spinner)source.CreateQuickAccessItem();

        source.Value = 20;
        Assert.That(spinner.Value, Is.EqualTo(20));

        spinner.Value = 30;
        Assert.That(source.Value, Is.EqualTo(30));
    }

    [Test]
    public void Spinner_Copy_Keeps_Value_Outside_Default_Range()
    {
        // The range lies completely below the default range (0..double.MaxValue) of a fresh Spinner,
        // so the copy must know Minimum/Maximum before it receives Value, otherwise it clamps Value
        // and pushes the clamped value back to the source through the TwoWay binding.
        var source = new Spinner
        {
            Minimum = -100,
            Maximum = -10,
            Value = -50
        };

        Assert.That(source.Value, Is.EqualTo(-50), "Precondition: source value is inside its own range.");

        var spinner = (Spinner)source.CreateQuickAccessItem();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(source.Value, Is.EqualTo(-50));
            Assert.That(spinner.Value, Is.EqualTo(-50));
            Assert.That(spinner.Minimum, Is.EqualTo(-100));
            Assert.That(spinner.Maximum, Is.EqualTo(-10));
        }
    }

    #endregion

    #region ComboBox

    [Test]
    public void ComboBox_Creates_Bound_ComboBox()
    {
        var source = new ComboBox
        {
            Header = "Header",
            Icon = "Icon",
            IsEditable = true,
            IsReadOnly = true,
            ResizeMode = ContextMenuResizeMode.Both,
            MaxDropDownHeight = 123,
            Text = "Text"
        };

        var item = source.CreateQuickAccessItem();

        Assert.That(item, Is.TypeOf<ComboBox>());
        Assert.That(item, Is.Not.SameAs(source));

        var comboBox = (ComboBox)item;

        Assert.That(comboBox.Header, Is.EqualTo("Header"));
        Assert.That(comboBox.Icon, Is.EqualTo("Icon"));
        Assert.That(comboBox.IsEditable, Is.True);
        Assert.That(comboBox.IsReadOnly, Is.True);
        Assert.That(comboBox.ResizeMode, Is.EqualTo(ContextMenuResizeMode.Both));
        Assert.That(comboBox.MaxDropDownHeight, Is.EqualTo(123));
        Assert.That(comboBox.Text, Is.EqualTo("Text"));
        Assert.That(comboBox.Size, Is.EqualTo(RibbonControlSize.Small));

        comboBox.Text = "FromQuickAccessItem";
        Assert.That(source.Text, Is.EqualTo("FromQuickAccessItem"));
    }

    [Test]
    public void ComboBox_Binds_MaxWidth_To_ActualWidth_Of_Source()
    {
        var source = new ComboBox
        {
            IsEditable = true
        };

        using (new TestRibbonWindow(source))
        {
            UIHelper.DoEvents();

            var comboBox = (ComboBox)source.CreateQuickAccessItem();

            Assert.That(source.ActualWidth, Is.GreaterThan(0));
            Assert.That(comboBox.MaxWidth, Is.EqualTo(source.ActualWidth));
        }
    }

    [Test]
    public void ComboBox_Moves_Items_While_DropDown_Of_Quick_Access_Item_Is_Open()
    {
        var source = new ComboBox();
        source.Items.Add("A");
        source.Items.Add("B");
        source.SelectedItem = "B";

        var comboBox = (ComboBox)source.CreateQuickAccessItem();

        using (CreateWindow(source, comboBox))
        {
            comboBox.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(source.Items, Is.Empty);
            Assert.That(comboBox.Items, Is.EqualTo(new[] { "A", "B" }));
            Assert.That(comboBox.SelectedItem, Is.EqualTo("B"));

            comboBox.IsDropDownOpen = false;
            UIHelper.DoEvents();

            Assert.That(comboBox.Items, Is.Empty);
            Assert.That(source.Items, Is.EqualTo(new[] { "A", "B" }));
            Assert.That(source.SelectedItem, Is.EqualTo("B"));
        }
    }

    #endregion

    #region DropDownButton, SplitButton

    [Test]
    public void DropDownButton_Creates_Bound_DropDownButton()
    {
        var source = new DropDownButton
        {
            Header = "Header",
            Icon = "Icon",
            LargeIcon = "LargeIcon",
            Size = RibbonControlSize.Large
        };

        var item = source.CreateQuickAccessItem();

        Assert.That(item, Is.TypeOf<DropDownButton>());
        Assert.That(item, Is.Not.SameAs(source));

        var button = (DropDownButton)item;

        Assert.That(button.Header, Is.EqualTo("Header"));
        Assert.That(button.Icon, Is.EqualTo("Icon"));
        Assert.That(button.LargeIcon, Is.EqualTo("LargeIcon"));
        Assert.That(button.Size, Is.EqualTo(RibbonControlSize.Small));

        source.ResizeMode = ContextMenuResizeMode.Vertical;
        source.MaxDropDownHeight = 123;
        source.HasTriangle = false;

        Assert.That(button.ResizeMode, Is.EqualTo(ContextMenuResizeMode.Vertical));
        Assert.That(button.MaxDropDownHeight, Is.EqualTo(123));
        Assert.That(button.HasTriangle, Is.False);
    }

    [Test]
    public void SplitButton_Creates_Bound_SplitButton()
    {
        var source = new SplitButton
        {
            Header = "Header",
            Icon = "Icon",
            LargeIcon = "LargeIcon",
            Size = RibbonControlSize.Large
        };

        var item = source.CreateQuickAccessItem();

        Assert.That(item, Is.TypeOf<SplitButton>());
        Assert.That(item, Is.Not.SameAs(source));

        var button = (SplitButton)item;

        Assert.That(button.CanAddButtonToQuickAccessToolBar, Is.False);
        Assert.That(button.Header, Is.EqualTo("Header"));
        Assert.That(button.Icon, Is.EqualTo("Icon"));
        Assert.That(button.LargeIcon, Is.EqualTo("LargeIcon"));
        Assert.That(button.Size, Is.EqualTo(RibbonControlSize.Small));

        source.MaxDropDownHeight = 123;
        source.IsCheckable = true;
        source.DropDownToolTip = "DropDownToolTip";
        source.IsButtonEnabled = false;
        source.ResizeMode = ContextMenuResizeMode.Vertical;
        source.HasTriangle = false;

        Assert.That(button.MaxDropDownHeight, Is.EqualTo(123));
        Assert.That(button.IsCheckable, Is.True);
        Assert.That(button.DropDownToolTip, Is.EqualTo("DropDownToolTip"));
        Assert.That(button.IsButtonEnabled, Is.False);
        Assert.That(button.ResizeMode, Is.EqualTo(ContextMenuResizeMode.Vertical));
        Assert.That(button.HasTriangle, Is.False);
    }

    [Test]
    public void SplitButton_Syncs_IsChecked_TwoWay()
    {
        var source = new SplitButton
        {
            IsCheckable = true
        };

        var button = (SplitButton)source.CreateQuickAccessItem();

        source.IsChecked = true;
        Assert.That(button.IsChecked, Is.True);

        button.IsChecked = false;
        Assert.That(source.IsChecked, Is.False);
    }

    [Test]
    public void SplitButton_Forwards_Click_To_Source()
    {
        var source = new SplitButton();

        var clickCount = 0;
        source.Click += (_, _) => clickCount++;

        var button = (SplitButton)source.CreateQuickAccessItem();

        button.RaiseEvent(new RoutedEventArgs(SplitButton.ClickEvent, button));

        Assert.That(clickCount, Is.EqualTo(1));
    }

    [TestCase(typeof(DropDownButton))]
    [TestCase(typeof(SplitButton))]
    public void DropDownButtonLike_Forwards_DropDown_Events_And_Moves_Items(Type type)
    {
        var source = (DropDownButton)Activator.CreateInstance(type);
        var menuItem = new MenuItem { Header = "Item" };
        source.Items.Add(menuItem);

        object openedSender = null;
        object closedSender = null;
        source.DropDownOpened += (sender, _) => openedSender = sender;
        source.DropDownClosed += (sender, _) => closedSender = sender;

        var button = (DropDownButton)source.CreateQuickAccessItem();

        using (CreateWindow(source, button))
        {
            button.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(openedSender, Is.SameAs(button));
            Assert.That(source.Items, Is.Empty);
            Assert.That(button.Items, Is.EqualTo(new[] { menuItem }));

            button.IsDropDownOpen = false;
            UIHelper.DoEvents();

            Assert.That(closedSender, Is.SameAs(button));
            Assert.That(button.Items, Is.Empty);
            Assert.That(source.Items, Is.EqualTo(new[] { menuItem }));
        }
    }

    #endregion

    #region MenuItem

    [Test]
    public void MenuItem_Without_Items_Creates_Button()
    {
        var command = new RoutedCommand();

        var source = new MenuItem
        {
            Command = command,
            Icon = "Icon"
        };

        var item = source.CreateQuickAccessItem();

        Assert.That(item, Is.TypeOf<Button>());

        var button = (Button)item;

        Assert.That(button.Command, Is.SameAs(command));
        Assert.That(button.Icon, Is.EqualTo("Icon"));
        Assert.That(button.Size, Is.EqualTo(RibbonControlSize.Small));
    }

    [Test]
    public void MenuItem_Checkable_Without_Items_Creates_ToggleButton()
    {
        var source = new MenuItem
        {
            IsCheckable = true,
            Icon = "Icon"
        };

        var item = source.CreateQuickAccessItem();

        Assert.That(item, Is.TypeOf<ToggleButton>());

        var toggleButton = (ToggleButton)item;

        Assert.That(toggleButton.Icon, Is.EqualTo("Icon"));
        Assert.That(toggleButton.Size, Is.EqualTo(RibbonControlSize.Small));
    }

    [Test]
    public void MenuItem_Checkable_Syncs_IsChecked_TwoWay()
    {
        var source = new MenuItem
        {
            IsCheckable = true,
            IsChecked = true
        };

        var toggleButton = (ToggleButton)source.CreateQuickAccessItem();

        Assert.That(toggleButton.IsChecked, Is.True);

        source.IsChecked = false;
        Assert.That(toggleButton.IsChecked, Is.False);

        toggleButton.IsChecked = true;
        Assert.That(source.IsChecked, Is.True);
    }

    [TestCase(false, typeof(DropDownButton))]
    [TestCase(true, typeof(SplitButton))]
    public void MenuItem_With_Items_Creates_DropDownButton(bool isSplit, Type expectedType)
    {
        var source = new MenuItem
        {
            IsSplit = isSplit,
            Icon = "Icon"
        };
        source.Items.Add(new MenuItem());

        var item = source.CreateQuickAccessItem();

        Assert.That(item, Is.TypeOf(expectedType));

        var button = (DropDownButton)item;

        Assert.That(button.Icon, Is.EqualTo("Icon"));
        Assert.That(button.Size, Is.EqualTo(RibbonControlSize.Small));

        if (button is SplitButton splitButton)
        {
            Assert.That(splitButton.CanAddButtonToQuickAccessToolBar, Is.False);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void MenuItem_With_Items_Binds_DropDown_Properties(bool isSplit)
    {
        var source = new MenuItem { IsSplit = isSplit };
        source.Items.Add(new MenuItem());

        var button = (DropDownButton)source.CreateQuickAccessItem();

        source.ResizeMode = ContextMenuResizeMode.Both;
        source.MaxDropDownHeight = 123;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.ResizeMode, Is.EqualTo(ContextMenuResizeMode.Both));
            Assert.That(button.MaxDropDownHeight, Is.EqualTo(123));
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void MenuItem_With_Items_Moves_Items_While_DropDown_Of_Quick_Access_Item_Is_Open(bool isSplit)
    {
        var source = new MenuItem
        {
            IsSplit = isSplit
        };
        var childMenuItem = new MenuItem { Header = "Child" };
        source.Items.Add(childMenuItem);

        var button = (DropDownButton)source.CreateQuickAccessItem();

        using (CreateWindow(button))
        {
            button.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(source.Items, Is.Empty);
            Assert.That(button.Items, Is.EqualTo(new[] { childMenuItem }));

            button.IsDropDownOpen = false;
            UIHelper.DoEvents();

            Assert.That(button.Items, Is.Empty);
            Assert.That(source.Items, Is.EqualTo(new[] { childMenuItem }));
        }
    }

    [TestCase(false, false, false, TestName = "MenuItem_Transfers_Header_To_Button")]
    [TestCase(false, false, true, TestName = "MenuItem_Transfers_Header_To_ToggleButton")]
    [TestCase(true, false, false, TestName = "MenuItem_Transfers_Header_To_DropDownButton")]
    [TestCase(true, true, false, TestName = "MenuItem_Transfers_Header_To_SplitButton")]
    public void MenuItem_Transfers_Header_To_Created_Item(bool hasItems, bool isSplit, bool isCheckable)
    {
        var source = new MenuItem
        {
            Header = "Header",
            IsSplit = isSplit,
            IsCheckable = isCheckable
        };

        if (hasItems)
        {
            source.Items.Add(new MenuItem());
        }

        var item = source.CreateQuickAccessItem();

        Assert.That(((IHeaderedControl)item).Header, Is.EqualTo("Header"));
        Assert.That(item.ToolTip, Is.EqualTo("Header"));
    }

    // Same as for ToggleButton: a checkable MenuItem with a GroupName stays checked when it is clicked again,
    // so its quick access item must not uncheck it either.
    [Test]
    public void MenuItem_Checkable_With_GroupName_Stays_Checked_When_Its_Checked_Quick_Access_Item_Is_Clicked()
    {
        var sourceA = new MenuItem { IsCheckable = true, GroupName = "g", IsChecked = true };
        var sourceB = new MenuItem { IsCheckable = true, GroupName = "g" };

        var itemA = (ToggleButton)sourceA.CreateQuickAccessItem();
        var itemB = (ToggleButton)sourceB.CreateQuickAccessItem();

        using (CreateWindow(sourceA, sourceB, itemA, itemB))
        {
            Assert.That(itemA.IsChecked, Is.True, "Precondition: item A mirrors source A.");

            itemA.InvokeClick();
            UIHelper.DoEvents();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(sourceA.IsChecked, Is.True, "Clicking the checked quick access item must not uncheck the original.");
                Assert.That(itemA.IsChecked, Is.True);
                Assert.That(sourceB.IsChecked, Is.False);
                Assert.That(itemB.IsChecked, Is.False);
            }
        }
    }

    [Test]
    public void MenuItem_Checkable_With_GroupName_Clicking_Unchecked_Quick_Access_Item_Checks_Its_Source_And_Unchecks_Others()
    {
        var sourceA = new MenuItem { IsCheckable = true, GroupName = "g", IsChecked = true };
        var sourceB = new MenuItem { IsCheckable = true, GroupName = "g" };

        var itemA = (ToggleButton)sourceA.CreateQuickAccessItem();
        var itemB = (ToggleButton)sourceB.CreateQuickAccessItem();

        using (CreateWindow(sourceA, sourceB, itemA, itemB))
        {
            itemB.InvokeClick();
            UIHelper.DoEvents();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(sourceB.IsChecked, Is.True);
                Assert.That(itemB.IsChecked, Is.True);
                Assert.That(sourceA.IsChecked, Is.False);
                Assert.That(itemA.IsChecked, Is.False);
            }
        }
    }

    #endregion

    #region InRibbonGallery

    [Test]
    public void InRibbonGallery_Creates_Bound_InRibbonGallery()
    {
        var source = new InRibbonGallery
        {
            Header = "Header",
            Icon = "Icon",
            Size = RibbonControlSize.Large
        };

        var item = source.CreateQuickAccessItem();

        Assert.That(item, Is.TypeOf<InRibbonGallery>());
        Assert.That(item, Is.Not.SameAs(source));

        var gallery = (InRibbonGallery)item;

        Assert.That(gallery.Header, Is.EqualTo("Header"));
        Assert.That(gallery.Icon, Is.EqualTo("Icon"));
        Assert.That(gallery.Size, Is.EqualTo(RibbonControlSize.Small));

        Func<object, string> groupByAdvanced = _ => "Group";

        source.GroupBy = "GroupBy";
        source.GroupByAdvanced = groupByAdvanced;
        source.ItemHeight = 11;
        source.ItemWidth = 22;
        source.ResizeMode = ContextMenuResizeMode.Both;
        source.MaxItemsInDropDownRow = 5;
        source.MinItemsInDropDownRow = 2;
        source.MaxDropDownWidth = 333;
        source.MaxDropDownHeight = 444;
        source.DisplayMemberPath = "DisplayMember";
        source.SelectedValuePath = "SelectedValue";

        Assert.That(gallery.GroupBy, Is.EqualTo("GroupBy"));
        Assert.That(gallery.GroupByAdvanced, Is.SameAs(groupByAdvanced));
        Assert.That(gallery.ItemHeight, Is.EqualTo(11));
        Assert.That(gallery.ItemWidth, Is.EqualTo(22));
        Assert.That(gallery.ResizeMode, Is.EqualTo(ContextMenuResizeMode.Both));
        Assert.That(gallery.MaxItemsInDropDownRow, Is.EqualTo(5));
        Assert.That(gallery.MinItemsInDropDownRow, Is.EqualTo(2));
        Assert.That(gallery.MaxDropDownWidth, Is.EqualTo(333));
        Assert.That(gallery.MaxDropDownHeight, Is.EqualTo(444));
        Assert.That(gallery.DisplayMemberPath, Is.EqualTo("DisplayMember"));
        Assert.That(gallery.SelectedValuePath, Is.EqualTo("SelectedValue"));
    }

    [Test]
    public void InRibbonGallery_Forwards_DropDown_Events_And_Moves_Items_And_Filters()
    {
        var filter = new GalleryGroupFilter();

        var source = new InRibbonGallery
        {
            Width = 100,
            Height = 30
        };
        source.Items.Add("A");
        source.Items.Add("B");
        source.Filters.Add(filter);
        source.SelectedFilter = filter;

        object openedSender = null;
        object closedSender = null;
        source.DropDownOpened += (sender, _) => openedSender = sender;
        source.DropDownClosed += (sender, _) => closedSender = sender;

        var gallery = (InRibbonGallery)source.CreateQuickAccessItem();

        using (CreateWindow(source, gallery))
        {
            gallery.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(openedSender, Is.SameAs(gallery));
            Assert.That(gallery.Filters, Is.EqualTo(new[] { filter }));
            Assert.That(gallery.SelectedFilter, Is.SameAs(filter));
            Assert.That(source.Items, Is.Empty);
            Assert.That(gallery.Items, Is.EqualTo(new[] { "A", "B" }));

            gallery.IsDropDownOpen = false;
            UIHelper.DoEvents();

            Assert.That(closedSender, Is.SameAs(gallery));
            Assert.That(gallery.Filters, Is.Empty);
            Assert.That(source.SelectedFilter, Is.SameAs(filter));
            Assert.That(gallery.Items, Is.Empty);
            Assert.That(source.Items, Is.EqualTo(new[] { "A", "B" }));
        }
    }

    #endregion

    #region RibbonGroupBox

    [Test]
    public void RibbonGroupBox_Creates_Bound_RibbonGroupBox()
    {
        var source = new RibbonGroupBox
        {
            Header = "Header",
            Icon = "Icon",
            LauncherIcon = "LauncherIcon"
        };

        var item = source.CreateQuickAccessItem();

        Assert.That(item, Is.TypeOf<RibbonGroupBox>());
        Assert.That(item, Is.Not.SameAs(source));

        var groupBox = (RibbonGroupBox)item;

        Assert.That(groupBox.State, Is.EqualTo(RibbonGroupBoxState.QuickAccess));
        Assert.That(groupBox.Header, Is.EqualTo("Header"));
        Assert.That(groupBox.ToolTip, Is.EqualTo("Header"));
        Assert.That(groupBox.Icon, Is.EqualTo("Icon"));
        Assert.That(groupBox.LauncherIcon, Is.EqualTo("LauncherIcon"));

        var itemsSource = new[] { "A" };
        var launcherCommand = new RoutedCommand();
        var launcherCommandParameter = new object();
        var launcherCommandTarget = new Button();

        source.Icon = "ChangedIcon";
        source.LauncherIcon = "ChangedLauncherIcon";
        source.ItemsSource = itemsSource;
        source.LauncherCommandParameter = launcherCommandParameter;
        source.LauncherCommandTarget = launcherCommandTarget;
        source.LauncherCommand = launcherCommand;
        source.LauncherText = "LauncherText";
        source.LauncherToolTip = "LauncherToolTip";
        source.IsLauncherVisible = true;
        source.LauncherKeys = "L";

        Assert.That(groupBox.Icon, Is.EqualTo("ChangedIcon"));
        Assert.That(groupBox.LauncherIcon, Is.EqualTo("ChangedLauncherIcon"));
        Assert.That(groupBox.ItemsSource, Is.SameAs(itemsSource));
        Assert.That(groupBox.LauncherCommandParameter, Is.SameAs(launcherCommandParameter));
        Assert.That(groupBox.LauncherCommandTarget, Is.SameAs(launcherCommandTarget));
        Assert.That(groupBox.LauncherCommand, Is.SameAs(launcherCommand));
        Assert.That(groupBox.LauncherText, Is.EqualTo("LauncherText"));
        Assert.That(groupBox.LauncherToolTip, Is.EqualTo("LauncherToolTip"));
        Assert.That(groupBox.IsLauncherVisible, Is.True);
        Assert.That(groupBox.LauncherKeys, Is.EqualTo("L"));

        source.IsLauncherEnabled = false;

        Assert.That(groupBox.IsLauncherEnabled, Is.False);
    }

    [Test]
    public void RibbonGroupBox_Creates_Rectangles_When_Icons_Are_Visuals()
    {
        var icon = new System.Windows.Controls.Border();
        var launcherIcon = new System.Windows.Controls.Border();

        var source = new RibbonGroupBox
        {
            Icon = icon,
            LauncherIcon = launcherIcon
        };

        var groupBox = (RibbonGroupBox)source.CreateQuickAccessItem();

        RibbonControlBindQuickAccessItemTests.AssertIsRectangleWithVisualBrush(groupBox.Icon, icon, 16);
        RibbonControlBindQuickAccessItemTests.AssertIsRectangleWithVisualBrush(groupBox.LauncherIcon, launcherIcon, 16);
    }

    [Test]
    public void RibbonGroupBox_Forwards_LauncherClick_Handlers()
    {
        var source = new RibbonGroupBox();

        RoutedEventHandler handler = (_, _) => { };
        source.LauncherClick += handler;

        var groupBox = (RibbonGroupBox)source.CreateQuickAccessItem();

        var launcherClick = groupBox.GetFieldValue<RoutedEventHandler>(nameof(RibbonGroupBox.LauncherClick));

        Assert.That(launcherClick, Is.Not.Null);
        Assert.That(launcherClick.GetInvocationList(), Does.Contain(handler));
    }

    [Test]
    public void RibbonGroupBox_Moves_Items_While_DropDown_Of_Quick_Access_Item_Is_Open()
    {
        var button = new Button { Header = "Button" };

        var source = new RibbonGroupBox();
        source.Items.Add(button);

        var groupBox = (RibbonGroupBox)source.CreateQuickAccessItem();

        using (CreateWindow(source, groupBox))
        {
            groupBox.IsDropDownOpen = true;
            UIHelper.DoEvents();

            Assert.That(source.IsSnapped, Is.True);
            Assert.That(source.Items, Is.Empty);
            Assert.That(groupBox.Items.Cast<object>().ToList(), Is.EqualTo(new[] { button }));

            groupBox.IsDropDownOpen = false;
            UIHelper.DoEvents();

            Assert.That(source.IsSnapped, Is.False);
            Assert.That(groupBox.Items, Is.Empty);
            Assert.That(source.Items.Cast<object>().ToList(), Is.EqualTo(new[] { button }));
        }
    }

    #endregion

    #region Unsupported controls

    [TestCase(typeof(Backstage))]
    [TestCase(typeof(ApplicationMenu))]
    [TestCase(typeof(RibbonToolBar))]
    public void Unsupported_Controls_Throw_NotImplementedException(Type type)
    {
        var control = (IQuickAccessItemProvider)Activator.CreateInstance(type);

        Assert.That(control.CanAddToQuickAccessToolBar, Is.False);
        Assert.That(() => control.CreateQuickAccessItem(), Throws.TypeOf<NotImplementedException>());
    }

    #endregion

    private static TestRibbonWindow CreateWindow(params UIElement[] elements)
    {
        var panel = new System.Windows.Controls.StackPanel();

        foreach (var element in elements)
        {
            panel.Children.Add(element);
        }

        var window = new TestRibbonWindow(panel);

        UIHelper.DoEvents();

        return window;
    }

    private sealed class NameViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Name)));
            }
        }
    }

    [Test]
    public void CheckBox_Binds_IsThreeState()
    {
        var source = new CheckBox { IsThreeState = true };

        var item = (CheckBox)source.CreateQuickAccessItem();

        // Without IsThreeState the copy would never cycle through the indeterminate state when clicked.
        Assert.That(item.IsThreeState, Is.True);

        source.IsThreeState = false;

        Assert.That(item.IsThreeState, Is.False);
    }
}
