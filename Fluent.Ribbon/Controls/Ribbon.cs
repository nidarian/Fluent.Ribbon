// ReSharper disable once CheckNamespace
namespace Fluent;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using Fluent.Collections;
using Fluent.Extensions;
using Fluent.Helpers;
using Fluent.Internal.KnownBoxes;
using Fluent.Localization;

// TODO: improve style parts naming & using

/// <summary>
/// Represents the main Ribbon control which consists of multiple tabs, each of which
/// containing groups of controls.  The Ribbon also provides improved context
/// menus, enhanced screen tips, and keyboard shortcuts.
/// </summary>
[ContentProperty(nameof(Tabs))]
[DefaultProperty(nameof(Tabs))]
[TemplatePart(Name = "PART_LayoutRoot", Type = typeof(Panel))]
[TemplatePart(Name = "PART_RibbonTabControl", Type = typeof(RibbonTabControl))]
[TemplatePart(Name = "PART_QuickAccessToolBar", Type = typeof(QuickAccessToolBar))]
public class Ribbon : Control, ILogicalChildSupport
{
    private IRibbonStateStorage? ribbonStateStorage;

    /// <summary>
    /// Gets the current instance for storing the state of this control.
    /// </summary>
    public IRibbonStateStorage RibbonStateStorage => this.ribbonStateStorage ??= this.CreateRibbonStateStorage();

    /// <summary>
    /// Create a new instance for storing the state of this control.
    /// </summary>
    /// <returns>Instance of a state storage class.</returns>
    protected virtual IRibbonStateStorage CreateRibbonStateStorage()
    {
        return new RibbonStateStorage(this);
    }

    #region Constants

    /// <summary>
    /// Minimal width of ribbon parent window
    /// </summary>
    public const double MinimalVisibleWidth = 300;

    /// <summary>
    /// Minimal height of ribbon parent window
    /// </summary>
    public const double MinimalVisibleHeight = 250;

    #endregion

    #region ContextMenu

    /// <summary>Identifies the <see cref="IsDefaultContextMenuEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty IsDefaultContextMenuEnabledProperty = DependencyProperty.Register(nameof(IsDefaultContextMenuEnabled), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets whether the default context menu should be enabled/used.
    /// </summary>
    public bool IsDefaultContextMenuEnabled
    {
        get => (bool)this.GetValue(IsDefaultContextMenuEnabledProperty);
        set => this.SetValue(IsDefaultContextMenuEnabledProperty, BooleanBoxes.Box(value));
    }

    private static readonly Dictionary<int, System.Windows.Controls.ContextMenu> contextMenus = new();

    /// <summary>
    /// Context menu for ribbon in current thread
    /// </summary>
    public static System.Windows.Controls.ContextMenu RibbonContextMenu
    {
        get
        {
            if (!contextMenus.ContainsKey(Thread.CurrentThread.ManagedThreadId))
            {
                InitRibbonContextMenu();
            }

            return contextMenus[Thread.CurrentThread.ManagedThreadId];
        }
    }

    // Context menu owner ribbon
    private static Ribbon? contextMenuOwner;

    // Context menu items
    private static readonly Dictionary<int, System.Windows.Controls.MenuItem> addToQuickAccessMenuItemDictionary = new();

    private static System.Windows.Controls.MenuItem AddToQuickAccessMenuItem => addToQuickAccessMenuItemDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, System.Windows.Controls.MenuItem> addGroupToQuickAccessMenuItemDictionary = new();

    private static System.Windows.Controls.MenuItem AddGroupToQuickAccessMenuItem => addGroupToQuickAccessMenuItemDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, System.Windows.Controls.MenuItem> addMenuToQuickAccessMenuItemDictionary = new();

    private static System.Windows.Controls.MenuItem AddMenuToQuickAccessMenuItem => addMenuToQuickAccessMenuItemDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, System.Windows.Controls.MenuItem> addGalleryToQuickAccessMenuItemDictionary = new();

    private static System.Windows.Controls.MenuItem AddGalleryToQuickAccessMenuItem => addGalleryToQuickAccessMenuItemDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, System.Windows.Controls.MenuItem> removeFromQuickAccessMenuItemDictionary = new();

    private static System.Windows.Controls.MenuItem RemoveFromQuickAccessMenuItem => removeFromQuickAccessMenuItemDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, System.Windows.Controls.MenuItem> showQuickAccessToolbarBelowTheRibbonMenuItemDictionary = new();

    private static System.Windows.Controls.MenuItem ShowQuickAccessToolbarBelowTheRibbonMenuItem => showQuickAccessToolbarBelowTheRibbonMenuItemDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, System.Windows.Controls.MenuItem> showQuickAccessToolbarAboveTheRibbonMenuItemDictionary = new();

    private static System.Windows.Controls.MenuItem ShowQuickAccessToolbarAboveTheRibbonMenuItem => showQuickAccessToolbarAboveTheRibbonMenuItemDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, System.Windows.Controls.MenuItem> minimizeTheRibbonMenuItemDictionary = new();

    private static System.Windows.Controls.MenuItem MinimizeTheRibbonMenuItem => minimizeTheRibbonMenuItemDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, System.Windows.Controls.MenuItem> useTheClassicRibbonMenuItemDictionary = new();

    private static System.Windows.Controls.MenuItem UseTheClassicRibbonMenuItem => useTheClassicRibbonMenuItemDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, System.Windows.Controls.MenuItem> useTheSimplifiedRibbonMenuItemDictionary = new();

    private static System.Windows.Controls.MenuItem UseTheSimplifiedRibbonMenuItem => useTheSimplifiedRibbonMenuItemDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, System.Windows.Controls.MenuItem> customizeQuickAccessToolbarMenuItemDictionary = new();

    private static System.Windows.Controls.MenuItem CustomizeQuickAccessToolbarMenuItem => customizeQuickAccessToolbarMenuItemDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, System.Windows.Controls.MenuItem> customizeTheRibbonMenuItemDictionary = new();

    private static System.Windows.Controls.MenuItem CustomizeTheRibbonMenuItem => customizeTheRibbonMenuItemDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, Separator> firstSeparatorDictionary = new();

    private static Separator FirstSeparator => firstSeparatorDictionary[Thread.CurrentThread.ManagedThreadId];

    private static readonly Dictionary<int, Separator> secondSeparatorDictionary = new();

    private static Separator SecondSeparator => secondSeparatorDictionary[Thread.CurrentThread.ManagedThreadId];

    // Initialize ribbon context menu
    private static void InitRibbonContextMenu()
    {
        contextMenus.Add(Thread.CurrentThread.ManagedThreadId, new ContextMenu());
        RibbonContextMenu.Opened += OnContextMenuOpened;
        RibbonContextMenu.Closed += OnContextMenuClosed;
    }

    private static void InitRibbonContextMenuItems()
    {
        // Headers are bound to RibbonLocalization.Current with the path "Localization.<Text>" (not to the current
        // Localization object), so they update when Culture/Localization is switched at runtime.
        // Add to quick access toolbar
        addToQuickAccessMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, CreateMenuItemForContextMenu(AddToQuickAccessCommand));
        RibbonContextMenu.Items.Add(AddToQuickAccessMenuItem);
        RibbonControl.Bind(RibbonLocalization.Current, AddToQuickAccessMenuItem, $"{nameof(RibbonLocalization.Localization)}.{nameof(RibbonLocalizationBase.RibbonContextMenuAddItem)}", HeaderedItemsControl.HeaderProperty, BindingMode.OneWay);
        RibbonControl.Bind(RibbonContextMenu, AddToQuickAccessMenuItem, nameof(System.Windows.Controls.ContextMenu.PlacementTarget), System.Windows.Controls.MenuItem.CommandParameterProperty, BindingMode.OneWay);

        // Add group to quick access toolbar
        addGroupToQuickAccessMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, CreateMenuItemForContextMenu(AddToQuickAccessCommand));
        RibbonContextMenu.Items.Add(AddGroupToQuickAccessMenuItem);
        RibbonControl.Bind(RibbonLocalization.Current, AddGroupToQuickAccessMenuItem, $"{nameof(RibbonLocalization.Localization)}.{nameof(RibbonLocalizationBase.RibbonContextMenuAddGroup)}", HeaderedItemsControl.HeaderProperty, BindingMode.OneWay);
        RibbonControl.Bind(RibbonContextMenu, AddGroupToQuickAccessMenuItem, nameof(System.Windows.Controls.ContextMenu.PlacementTarget), System.Windows.Controls.MenuItem.CommandParameterProperty, BindingMode.OneWay);

        // Add menu item to quick access toolbar
        addMenuToQuickAccessMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, CreateMenuItemForContextMenu(AddToQuickAccessCommand));
        RibbonContextMenu.Items.Add(AddMenuToQuickAccessMenuItem);
        RibbonControl.Bind(RibbonLocalization.Current, AddMenuToQuickAccessMenuItem, $"{nameof(RibbonLocalization.Localization)}.{nameof(RibbonLocalizationBase.RibbonContextMenuAddMenu)}", HeaderedItemsControl.HeaderProperty, BindingMode.OneWay);
        RibbonControl.Bind(RibbonContextMenu, AddMenuToQuickAccessMenuItem, nameof(System.Windows.Controls.ContextMenu.PlacementTarget), System.Windows.Controls.MenuItem.CommandParameterProperty, BindingMode.OneWay);

        // Add gallery to quick access toolbar
        addGalleryToQuickAccessMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, CreateMenuItemForContextMenu(AddToQuickAccessCommand));
        RibbonContextMenu.Items.Add(AddGalleryToQuickAccessMenuItem);
        RibbonControl.Bind(RibbonLocalization.Current, AddGalleryToQuickAccessMenuItem, $"{nameof(RibbonLocalization.Localization)}.{nameof(RibbonLocalizationBase.RibbonContextMenuAddGallery)}", HeaderedItemsControl.HeaderProperty, BindingMode.OneWay);
        RibbonControl.Bind(RibbonContextMenu, AddGalleryToQuickAccessMenuItem, nameof(System.Windows.Controls.ContextMenu.PlacementTarget), System.Windows.Controls.MenuItem.CommandParameterProperty, BindingMode.OneWay);

        // Remove from quick access toolbar
        removeFromQuickAccessMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, CreateMenuItemForContextMenu(RemoveFromQuickAccessCommand));
        RibbonContextMenu.Items.Add(RemoveFromQuickAccessMenuItem);
        RibbonControl.Bind(RibbonLocalization.Current, RemoveFromQuickAccessMenuItem, $"{nameof(RibbonLocalization.Localization)}.{nameof(RibbonLocalizationBase.RibbonContextMenuRemoveItem)}", HeaderedItemsControl.HeaderProperty, BindingMode.OneWay);
        RibbonControl.Bind(RibbonContextMenu, RemoveFromQuickAccessMenuItem, nameof(System.Windows.Controls.ContextMenu.PlacementTarget), System.Windows.Controls.MenuItem.CommandParameterProperty, BindingMode.OneWay);

        // Separator
        firstSeparatorDictionary.Add(Thread.CurrentThread.ManagedThreadId, new Separator());
        RibbonContextMenu.Items.Add(FirstSeparator);

        // Customize quick access toolbar
        customizeQuickAccessToolbarMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, CreateMenuItemForContextMenu(CustomizeQuickAccessToolbarCommand));
        RibbonContextMenu.Items.Add(CustomizeQuickAccessToolbarMenuItem);
        RibbonControl.Bind(RibbonLocalization.Current, CustomizeQuickAccessToolbarMenuItem, $"{nameof(RibbonLocalization.Localization)}.{nameof(RibbonLocalizationBase.RibbonContextMenuCustomizeQuickAccessToolBar)}", HeaderedItemsControl.HeaderProperty, BindingMode.OneWay);
        RibbonControl.Bind(RibbonContextMenu, CustomizeQuickAccessToolbarMenuItem, nameof(System.Windows.Controls.ContextMenu.PlacementTarget), System.Windows.Controls.MenuItem.CommandParameterProperty, BindingMode.OneWay);

        // Show quick access below the ribbon
        showQuickAccessToolbarBelowTheRibbonMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, CreateMenuItemForContextMenu(ShowQuickAccessBelowCommand));
        RibbonContextMenu.Items.Add(ShowQuickAccessToolbarBelowTheRibbonMenuItem);
        RibbonControl.Bind(RibbonLocalization.Current, ShowQuickAccessToolbarBelowTheRibbonMenuItem, $"{nameof(RibbonLocalization.Localization)}.{nameof(RibbonLocalizationBase.RibbonContextMenuShowBelow)}", HeaderedItemsControl.HeaderProperty, BindingMode.OneWay);
        RibbonControl.Bind(RibbonContextMenu, ShowQuickAccessToolbarBelowTheRibbonMenuItem, nameof(System.Windows.Controls.ContextMenu.PlacementTarget), System.Windows.Controls.MenuItem.CommandParameterProperty, BindingMode.OneWay);

        // Show quick access above the ribbon
        showQuickAccessToolbarAboveTheRibbonMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, CreateMenuItemForContextMenu(ShowQuickAccessAboveCommand));
        RibbonContextMenu.Items.Add(ShowQuickAccessToolbarAboveTheRibbonMenuItem);
        RibbonControl.Bind(RibbonLocalization.Current, ShowQuickAccessToolbarAboveTheRibbonMenuItem, $"{nameof(RibbonLocalization.Localization)}.{nameof(RibbonLocalizationBase.RibbonContextMenuShowAbove)}", HeaderedItemsControl.HeaderProperty, BindingMode.OneWay);
        RibbonControl.Bind(RibbonContextMenu, ShowQuickAccessToolbarAboveTheRibbonMenuItem, nameof(System.Windows.Controls.ContextMenu.PlacementTarget), System.Windows.Controls.MenuItem.CommandParameterProperty, BindingMode.OneWay);

        // Separator
        secondSeparatorDictionary.Add(Thread.CurrentThread.ManagedThreadId, new Separator());
        RibbonContextMenu.Items.Add(SecondSeparator);

        // Customize the ribbon
        customizeTheRibbonMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, CreateMenuItemForContextMenu(CustomizeTheRibbonCommand));
        RibbonContextMenu.Items.Add(CustomizeTheRibbonMenuItem);
        RibbonControl.Bind(RibbonLocalization.Current, CustomizeTheRibbonMenuItem, $"{nameof(RibbonLocalization.Localization)}.{nameof(RibbonLocalizationBase.RibbonContextMenuCustomizeRibbon)}", HeaderedItemsControl.HeaderProperty, BindingMode.OneWay);
        RibbonControl.Bind(RibbonContextMenu, CustomizeTheRibbonMenuItem, nameof(System.Windows.Controls.ContextMenu.PlacementTarget), System.Windows.Controls.MenuItem.CommandParameterProperty, BindingMode.OneWay);

        // Minimize the ribbon
        minimizeTheRibbonMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, CreateMenuItemForContextMenu(ToggleMinimizeTheRibbonCommand));
        RibbonContextMenu.Items.Add(MinimizeTheRibbonMenuItem);
        RibbonControl.Bind(RibbonLocalization.Current, MinimizeTheRibbonMenuItem, $"{nameof(RibbonLocalization.Localization)}.{nameof(RibbonLocalizationBase.RibbonContextMenuMinimizeRibbon)}", HeaderedItemsControl.HeaderProperty, BindingMode.OneWay);
        RibbonControl.Bind(RibbonContextMenu, MinimizeTheRibbonMenuItem, nameof(System.Windows.Controls.ContextMenu.PlacementTarget), System.Windows.Controls.MenuItem.CommandParameterProperty, BindingMode.OneWay);

        // Use the classic ribbon
        useTheClassicRibbonMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, CreateMenuItemForContextMenu(SwitchToTheClassicRibbonCommand));
        RibbonContextMenu.Items.Add(UseTheClassicRibbonMenuItem);
        RibbonControl.Bind(RibbonLocalization.Current, UseTheClassicRibbonMenuItem, $"{nameof(RibbonLocalization.Localization)}.{nameof(RibbonLocalizationBase.UseClassicRibbon)}", HeaderedItemsControl.HeaderProperty, BindingMode.OneWay);
        RibbonControl.Bind(RibbonContextMenu, UseTheClassicRibbonMenuItem, nameof(System.Windows.Controls.ContextMenu.PlacementTarget), System.Windows.Controls.MenuItem.CommandParameterProperty, BindingMode.OneWay);

        // Use the simplifed ribbon
        useTheSimplifiedRibbonMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, CreateMenuItemForContextMenu(SwitchToTheSimplifiedRibbonCommand));
        RibbonContextMenu.Items.Add(UseTheSimplifiedRibbonMenuItem);
        RibbonControl.Bind(RibbonLocalization.Current, UseTheSimplifiedRibbonMenuItem, $"{nameof(RibbonLocalization.Localization)}.{nameof(RibbonLocalizationBase.UseSimplifiedRibbon)}", HeaderedItemsControl.HeaderProperty, BindingMode.OneWay);
        RibbonControl.Bind(RibbonContextMenu, UseTheSimplifiedRibbonMenuItem, nameof(System.Windows.Controls.ContextMenu.PlacementTarget), System.Windows.Controls.MenuItem.CommandParameterProperty, BindingMode.OneWay);
    }

    private static MenuItem CreateMenuItemForContextMenu(ICommand command)
    {
        return new MenuItem
        {
            Command = command,
            CanAddToQuickAccessToolBar = false,
            ContextMenu = null
        };
    }

    /// <inheritdoc />
    protected override void OnContextMenuOpening(ContextMenuEventArgs e)
    {
        contextMenuOwner = this;
        base.OnContextMenuOpening(e);
    }

    /// <inheritdoc />
    protected override void OnContextMenuClosing(ContextMenuEventArgs e)
    {
        contextMenuOwner = null;
        base.OnContextMenuClosing(e);
    }

    private void OnQuickAccessContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        this.OnContextMenuOpening(e);
    }

    private void OnQuickAccessContextMenuClosing(object sender, ContextMenuEventArgs e)
    {
        this.OnContextMenuClosing(e);
    }

    // Occurs when context menu is opening
    private static void OnContextMenuOpened(object sender, RoutedEventArgs e)
    {
        var ribbon = contextMenuOwner;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (RibbonContextMenu is null)
        {
            return;
        }

        if (ribbon is null)
        {
            RibbonContextMenu.IsOpen = false;
            return;
        }

        if (ribbon.IsDefaultContextMenuEnabled
            && RibbonContextMenu.Items.Count == 0)
        {
            InitRibbonContextMenuItems();
        }

        if (ribbon.IsDefaultContextMenuEnabled == false)
        {
            foreach (var item in RibbonContextMenu.Items.OfType<UIElement>())
            {
                item.Visibility = Visibility.Collapsed;
            }

            RibbonContextMenu.IsOpen = false;
            return;
        }

        AddToQuickAccessMenuItem.CommandTarget = ribbon;
        AddGroupToQuickAccessMenuItem.CommandTarget = ribbon;
        AddMenuToQuickAccessMenuItem.CommandTarget = ribbon;
        AddGalleryToQuickAccessMenuItem.CommandTarget = ribbon;
        RemoveFromQuickAccessMenuItem.CommandTarget = ribbon;
        CustomizeQuickAccessToolbarMenuItem.CommandTarget = ribbon;
        CustomizeTheRibbonMenuItem.CommandTarget = ribbon;
        MinimizeTheRibbonMenuItem.CommandTarget = ribbon;
        UseTheClassicRibbonMenuItem.CommandTarget = ribbon;
        UseTheSimplifiedRibbonMenuItem.CommandTarget = ribbon;
        ShowQuickAccessToolbarBelowTheRibbonMenuItem.CommandTarget = ribbon;
        ShowQuickAccessToolbarAboveTheRibbonMenuItem.CommandTarget = ribbon;

        // A menu item doesn't ask its command again whether it can execute when CommandTarget changes, and the target
        // is cleared when the menu closes. So ask now, otherwise the items could show disabled until the next input.
        // Setting Command again is the only public way to make a menu item ask right away (CommandManager.InvalidateRequerySuggested
        // asks every command of the application, and only later on the dispatcher).
        foreach (var menuItem in RibbonContextMenu.Items.OfType<System.Windows.Controls.MenuItem>())
        {
            var command = menuItem.Command;
            menuItem.Command = null;
            menuItem.Command = command;
        }

        // Hide items for ribbon controls
        AddToQuickAccessMenuItem.Visibility = Visibility.Collapsed;
        AddGroupToQuickAccessMenuItem.Visibility = Visibility.Collapsed;
        AddMenuToQuickAccessMenuItem.Visibility = Visibility.Collapsed;
        AddGalleryToQuickAccessMenuItem.Visibility = Visibility.Collapsed;
        RemoveFromQuickAccessMenuItem.Visibility = Visibility.Collapsed;
        FirstSeparator.Visibility = Visibility.Collapsed;

        // Hide customize quick access menu item
        CustomizeQuickAccessToolbarMenuItem.Visibility = Visibility.Collapsed;
        SecondSeparator.Visibility = Visibility.Collapsed;

        // Set use the classic ribbon menu item visibility
        UseTheClassicRibbonMenuItem.Visibility = (ribbon.CanUseSimplified && ribbon.IsSimplified)
            ? Visibility.Visible
            : Visibility.Collapsed;

        // Set use the simplified ribbon menu item visibility
        UseTheSimplifiedRibbonMenuItem.Visibility = (ribbon.CanUseSimplified && !ribbon.IsSimplified)
            ? Visibility.Visible
            : Visibility.Collapsed;

        // Set minimize the ribbon menu item state
        MinimizeTheRibbonMenuItem.IsChecked = ribbon.IsMinimized;

        // Set minimize the ribbon menu item visibility
        MinimizeTheRibbonMenuItem.Visibility = ribbon.CanMinimize
            ? Visibility.Visible
            : Visibility.Collapsed;

        // Set customize the ribbon menu item visibility
        CustomizeTheRibbonMenuItem.Visibility = ribbon.CanCustomizeRibbon
            ? Visibility.Visible
            : Visibility.Collapsed;

        // Hide quick access position menu items
        ShowQuickAccessToolbarBelowTheRibbonMenuItem.Visibility = Visibility.Collapsed;
        ShowQuickAccessToolbarAboveTheRibbonMenuItem.Visibility = Visibility.Collapsed;

        // If quick access toolbar is visible show
        if (ribbon.IsQuickAccessToolBarVisible)
        {
            // Set quick access position menu items visibility
            if (ribbon.CanQuickAccessLocationChanging)
            {
                if (ribbon.ShowQuickAccessToolBarAboveRibbon)
                {
                    ShowQuickAccessToolbarBelowTheRibbonMenuItem.Visibility = Visibility.Visible;
                }
                else
                {
                    ShowQuickAccessToolbarAboveTheRibbonMenuItem.Visibility = Visibility.Visible;
                }
            }

            if (ribbon.CanCustomizeQuickAccessToolBar)
            {
                CustomizeQuickAccessToolbarMenuItem.Visibility = Visibility.Visible;
            }

            if (ribbon.CanQuickAccessLocationChanging
                || ribbon.CanCustomizeQuickAccessToolBar)
            {
                SecondSeparator.Visibility = Visibility.Visible;
            }
            else
            {
                SecondSeparator.Visibility = Visibility.Collapsed;
            }

            if (ribbon.CanCustomizeQuickAccessToolBarItems)
            {
                // Gets control that raise menu opened
                var control = RibbonContextMenu.PlacementTarget;
                AddToQuickAccessCommand.CanExecute(null, control);
                RemoveFromQuickAccessCommand.CanExecute(null, control);

                //Debug.WriteLine("Menu opened on "+control);
                if (control is not null)
                {
                    // Check for value because remove is only possible in the context menu of items in QA which represent the value for QA-items
                    if (ribbon.QuickAccessElements.ContainsValue(control)
                        && (control is not IDropDownControl dropDownControl || dropDownControl.IsDropDownOpen is false))
                    {
                        FirstSeparator.Visibility = Visibility.Visible;

                        // Control is on quick access
                        RemoveFromQuickAccessMenuItem.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        FirstSeparator.Visibility = Visibility.Visible;

                        if (control is System.Windows.Controls.MenuItem)
                        {
                            // Control is menu item
                            AddMenuToQuickAccessMenuItem.Visibility = Visibility.Visible;
                        }
                        else if (control is Gallery or InRibbonGallery)
                        {
                            // Control is gallery
                            AddGalleryToQuickAccessMenuItem.Visibility = Visibility.Visible;
                        }
                        else if (control is RibbonGroupBox)
                        {
                            // Control is group box
                            AddGroupToQuickAccessMenuItem.Visibility = Visibility.Visible;
                        }
                        else if (control is IQuickAccessItemProvider)
                        {
                            // Its other control
                            AddToQuickAccessMenuItem.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            FirstSeparator.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        // We have to close the context menu if no items are visible
        if (RibbonContextMenu.Items.OfType<System.Windows.Controls.MenuItem>().All(x => x.Visibility == Visibility.Collapsed))
        {
            RibbonContextMenu.IsOpen = false;
        }
    }

    // Occurs when context menu is closed
    private static void OnContextMenuClosed(object sender, RoutedEventArgs e)
    {
        // The context menu is static and shared by all ribbons of the thread.
        // Forget the ribbon it was opened on, otherwise the menu items keep that ribbon and its whole window
        // alive after the window was closed (until another ribbon opens the menu).
        // WPF raises Closed after it ran the command of a clicked item, and OnContextMenuOpened sets the target again on the next opening.
        contextMenuOwner = null;

        foreach (var menuItem in RibbonContextMenu.Items.OfType<System.Windows.Controls.MenuItem>())
        {
            menuItem.ClearValue(System.Windows.Controls.MenuItem.CommandTargetProperty);
        }

        // While open, the popup of the menu uses the PlacementTarget as its parent, so the menu and its items take over
        // inherited values from it, like the DataContext and the window (Window.GetWindow).
        // WPF only resets them if the PlacementTarget is still set when it destroys the popup right after this event,
        // but WPF clears the PlacementTarget (by forgetting the menu's owner) during this event already.
        // So keep the PlacementTarget until the popup is destroyed and remove it afterwards.
        var menu = (System.Windows.Controls.ContextMenu)sender;
        var placementTarget = menu.PlacementTarget;

        if (placementTarget is not null
            && menu.ReadLocalValue(System.Windows.Controls.ContextMenu.PlacementTargetProperty) == DependencyProperty.UnsetValue)
        {
            menu.PlacementTarget = placementTarget;

            menu.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
            {
                if (ReferenceEquals(menu.ReadLocalValue(System.Windows.Controls.ContextMenu.PlacementTargetProperty), placementTarget))
                {
                    menu.ClearValue(System.Windows.Controls.ContextMenu.PlacementTargetProperty);
                }
            }));
        }
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when selected tab has been changed (be aware that SelectedTab can be null)
    /// </summary>
    public event SelectionChangedEventHandler? SelectedTabChanged;

    /// <summary>
    /// Occurs when customize the ribbon
    /// </summary>
    public event EventHandler? CustomizeTheRibbon;

    /// <summary>
    /// Occurs when customize quick access toolbar
    /// </summary>
    public event EventHandler? CustomizeQuickAccessToolbar;

    /// <summary>
    /// Occurs when IsMinimized property has changed
    /// </summary>
    public event DependencyPropertyChangedEventHandler? IsMinimizedChanged;

    /// <summary>
    /// Occurs when IsCollapsed property has changed
    /// </summary>
    public event DependencyPropertyChangedEventHandler? IsCollapsedChanged;

    #endregion

    #region Fields

    private ObservableCollection<Key>? keyTipKeys;

    // Collection of contextual tab groups
    private ObservableCollection<RibbonContextualTabGroup>? contextualGroups;

    // Collection of tabs
    private ObservableCollection<RibbonTabItem>? tabs;
    private CollectionSyncHelper<RibbonTabItem>? tabsSync;

    // True while a tab that was moved or replaced in Tabs is moved or replaced in the tab control (see OnTabItemsCollectionChanged).
    private bool isSyncingTabControl;

    // The tab that was selected in the tab control before that.
    private RibbonTabItem? tabSelectedBeforeSync;

    // Collection of toolbar items
    private ObservableCollection<UIElement>? toolBarItems;
    private CollectionSyncHelper<UIElement>? toolBarItemsSync;

    // Ribbon quick access toolbar

    // Ribbon layout root
    private Panel? layoutRoot;

    // Handles F10, Alt and so on
    private readonly KeyTipService keyTipService;

    // Collection of quickaccess menu items
    private ObservableCollection<QuickAccessMenuItem>? quickAccessItems;
    private CollectionSyncHelper<QuickAccessMenuItem>? quickAccessItemsSync;

    // Currently added in QAT items

    private Window? ownerWindow;

    // Whether LoadInitialState already selected the first tab (see LoadInitialState)
    private bool isInitialTabSelectionDone;

    #endregion

    #region Properties

    #region Menu

    /// <summary>
    /// Gets or sets file menu control (can be application menu button, backstage button and so on)
    /// </summary>
    public FrameworkElement? Menu
    {
        get => (FrameworkElement?)this.GetValue(MenuProperty);
        set => this.SetValue(MenuProperty, value);
    }

    /// <summary>Identifies the <see cref="Menu"/> dependency property.</summary>
    public static readonly DependencyProperty MenuProperty =
        DependencyProperty.Register(nameof(Menu), typeof(FrameworkElement), typeof(Ribbon), new FrameworkPropertyMetadata(default(FrameworkElement), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, OnMenuChanged));

    private static void OnMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        AddOrRemoveLogicalChildOnPropertyChanged(d, e);
    }

    /// <summary>Identifies the <see cref="IsBackstageOrStartScreenOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsBackstageOrStartScreenOpenProperty = DependencyProperty.Register(nameof(IsBackstageOrStartScreenOpen), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.FalseBox, OnIsBackstageOrStartScreenOpenChanged));

    /// <summary>
    /// Defines if the backstage or startscreen is currently open or not.
    /// </summary>
    public bool IsBackstageOrStartScreenOpen
    {
        get => (bool)this.GetValue(IsBackstageOrStartScreenOpenProperty);
        set => this.SetValue(IsBackstageOrStartScreenOpenProperty, value);
    }

    private static void OnIsBackstageOrStartScreenOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;
        ribbon.TitleBar?.ScheduleForceMeasureAndArrange();
    }

    #endregion

    #region StartScreen

    /// <summary>
    /// Property for defining the start screen.
    /// </summary>
    public StartScreen? StartScreen
    {
        get => (StartScreen?)this.GetValue(StartScreenProperty);
        set => this.SetValue(StartScreenProperty, value);
    }

    /// <summary>Identifies the <see cref="StartScreen"/> dependency property.</summary>
    public static readonly DependencyProperty StartScreenProperty =
        DependencyProperty.Register(nameof(StartScreen), typeof(StartScreen), typeof(Ribbon), new FrameworkPropertyMetadata(default(StartScreen), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, OnStartScreenChanged));

    private static void OnStartScreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        AddOrRemoveLogicalChildOnPropertyChanged(d, e);
    }

    #endregion

    #region QuickAccessToolBar

    /// <summary>
    /// Property for defining the QuickAccessToolBar.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public QuickAccessToolBar? QuickAccessToolBar
    {
        get => (QuickAccessToolBar?)this.GetValue(QuickAccessToolBarProperty);
        private set => this.SetValue(QuickAccessToolBarPropertyKey, value);
    }

    // ReSharper disable once InconsistentNaming
    private static readonly DependencyPropertyKey QuickAccessToolBarPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(QuickAccessToolBar), typeof(QuickAccessToolBar), typeof(Ribbon), new FrameworkPropertyMetadata(default(QuickAccessToolBar), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, OnQuickAccessToolBarChanged));

    private static void OnQuickAccessToolBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        AddOrRemoveLogicalChildOnPropertyChanged(d, e);
    }

    /// <summary>Identifies the <see cref="QuickAccessToolBar"/> dependency property.</summary>
    public static readonly DependencyProperty QuickAccessToolBarProperty = QuickAccessToolBarPropertyKey.DependencyProperty;

    #endregion

    #region TabControl

    /// <summary>
    /// Property for defining the TabControl.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public RibbonTabControl? TabControl
    {
        get => (RibbonTabControl?)this.GetValue(TabControlProperty);
        private set => this.SetValue(TabControlPropertyKey, value);
    }

    // ReSharper disable once InconsistentNaming
    private static readonly DependencyPropertyKey TabControlPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(TabControl), typeof(RibbonTabControl), typeof(Ribbon), new FrameworkPropertyMetadata(default(RibbonTabControl), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, LogicalChildSupportHelper.OnLogicalChildPropertyChanged));

    /// <summary>Identifies the <see cref="TabControl"/> dependency property.</summary>
    public static readonly DependencyProperty TabControlProperty = TabControlPropertyKey.DependencyProperty;

    #endregion

    #region IsSimplified

    /// <summary>
    /// Gets or sets whether or not the ribbon is in Simplified mode
    /// </summary>
    public bool IsSimplified
    {
        get => (bool)this.GetValue(IsSimplifiedProperty);
        set => this.SetValue(IsSimplifiedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty IsSimplifiedProperty = DependencyProperty.Register(nameof(IsSimplified), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.FalseBox, OnIsSimplifiedChanged));

    private static void OnIsSimplifiedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Ribbon ribbon)
        {
            var isSimplified = ribbon.IsSimplified;
            foreach (var item in ribbon.Tabs.OfType<ISimplifiedStateControl>())
            {
                item.UpdateSimplifiedState(isSimplified);
            }
        }
    }
    #endregion

    /// <summary>
    /// Gets or sets selected tab item
    /// </summary>
    public RibbonTabItem? SelectedTabItem
    {
        get => (RibbonTabItem?)this.GetValue(SelectedTabItemProperty);
        set => this.SetValue(SelectedTabItemProperty, value);
    }

    /// <summary>Identifies the <see cref="SelectedTabItem"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedTabItemProperty =
        DependencyProperty.Register(nameof(SelectedTabItem), typeof(RibbonTabItem), typeof(Ribbon), new PropertyMetadata(OnSelectedTabItemChanged));

    private static void OnSelectedTabItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;
        if (ribbon.TabControl is not null)
        {
            ribbon.TabControl.SelectedItem = e.NewValue;
        }

        if (e.NewValue is RibbonTabItem selectedItem
            && ribbon.Tabs.Contains(selectedItem))
        {
            ribbon.SelectedTabIndex = ribbon.Tabs.IndexOf(selectedItem);
        }
        else
        {
            ribbon.SelectedTabIndex = -1;
        }
    }

    /// <summary>
    /// Gets or sets selected tab index
    /// </summary>
    public int SelectedTabIndex
    {
        get => (int)this.GetValue(SelectedTabIndexProperty);
        set => this.SetValue(SelectedTabIndexProperty, value);
    }

    /// <summary>Identifies the <see cref="SelectedTabIndex"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedTabIndexProperty =
        DependencyProperty.Register(nameof(SelectedTabIndex), typeof(int), typeof(Ribbon), new PropertyMetadata(-1, OnSelectedTabIndexChanged));

    private static void OnSelectedTabIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;
        var selectedIndex = (int)e.NewValue;

        if (ribbon.TabControl is not null)
        {
            ribbon.TabControl.SelectedIndex = selectedIndex;
        }

        if (selectedIndex >= 0
            && selectedIndex < ribbon.Tabs.Count)
        {
            ribbon.SelectedTabItem = ribbon.Tabs[selectedIndex];
        }
        else
        {
            ribbon.SelectedTabItem = null;
        }
    }

    private static void AddOrRemoveLogicalChildOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;
        if (e.OldValue is not null)
        {
            ribbon.RemoveLogicalChild(e.OldValue);
        }

        if (e.NewValue is not null)
        {
            ribbon.AddLogicalChild(e.NewValue);
        }
    }

    /// <summary>
    /// Gets the first visible TabItem
    /// </summary>
    public RibbonTabItem? FirstVisibleItem => this.GetFirstVisibleItem();

    /// <summary>
    /// Gets the last visible TabItem
    /// </summary>
    public RibbonTabItem? LastVisibleItem => this.GetLastVisibleItem();

    /// <summary>
    /// Gets currently active quick access elements.
    /// </summary>
    protected Dictionary<UIElement, UIElement> QuickAccessElements { get; } = new();

    /// <summary>
    /// Gets a copy of currently active quick access elements.
    /// </summary>
    public IDictionary<UIElement, UIElement> GetQuickAccessElements() => this.QuickAccessElements.ToDictionary(x => x.Key, y => y.Value);

    #region TitelBar

    /// <summary>
    /// Gets ribbon titlebar
    /// </summary>
    public RibbonTitleBar? TitleBar
    {
        get => (RibbonTitleBar?)this.GetValue(TitleBarProperty);
        set => this.SetValue(TitleBarProperty, value);
    }

    /// <summary>Identifies the <see cref="TitleBar"/> dependency property.</summary>
    public static readonly DependencyProperty TitleBarProperty = DependencyProperty.Register(nameof(TitleBar), typeof(RibbonTitleBar), typeof(Ribbon), new PropertyMetadata(OnTitleBarChanged));

    private static void OnTitleBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;

        if (e.OldValue is RibbonTitleBar oldValue)
        {
            oldValue.ItemsSource = null;

            ribbon.RemoveQuickAccessToolBarFromTitleBar(oldValue);
        }

        if (e.NewValue is RibbonTitleBar newValue)
        {
            newValue.ItemsSource = ribbon.ContextualGroups;

            if (ribbon.ShowQuickAccessToolBarAboveRibbon)
            {
                ribbon.MoveQuickAccessToolBarToTitleBar(newValue);
            }
        }
    }

    #endregion

    /// <summary>
    /// Gets or sets whether quick access toolbar showes above ribbon
    /// </summary>
    public bool ShowQuickAccessToolBarAboveRibbon
    {
        get => (bool)this.GetValue(ShowQuickAccessToolBarAboveRibbonProperty);
        set => this.SetValue(ShowQuickAccessToolBarAboveRibbonProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="ShowQuickAccessToolBarAboveRibbon"/> dependency property.</summary>
    public static readonly DependencyProperty ShowQuickAccessToolBarAboveRibbonProperty =
        DependencyProperty.Register(nameof(ShowQuickAccessToolBarAboveRibbon), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox, OnShowQuickAccessToolBarAboveRibbonChanged));

    /// <summary>
    /// Handles ShowQuickAccessToolBarAboveRibbon property changed
    /// </summary>
    /// <param name="d">Object</param>
    /// <param name="e">The event data</param>
    private static void OnShowQuickAccessToolBarAboveRibbonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;

        if (ribbon.TitleBar is not null)
        {
            if ((bool)e.NewValue)
            {
                ribbon.MoveQuickAccessToolBarToTitleBar(ribbon.TitleBar);
            }
            else
            {
                ribbon.RemoveQuickAccessToolBarFromTitleBar(ribbon.TitleBar);
            }

            ribbon.TitleBar.InvalidateMeasure();
        }

        ribbon.RibbonStateStorage.SaveTemporary();
    }

    /// <summary>
    /// Gets or sets the height of the Quick Access Toolbar when it is shown below the ribbon.
    /// </summary>
    public double QuickAccessToolBarHeight
    {
        get => (double)this.GetValue(QuickAccessToolBarHeightProperty);
        set => this.SetValue(QuickAccessToolBarHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="QuickAccessToolBarHeight"/> dependency property.</summary>
    public static readonly DependencyProperty QuickAccessToolBarHeightProperty =
        DependencyProperty.Register(nameof(QuickAccessToolBarHeight), typeof(double), typeof(Ribbon), new PropertyMetadata(23D));

    /// <summary>
    /// Gets collection of contextual tab groups
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<RibbonContextualTabGroup> ContextualGroups => this.contextualGroups ??= new ObservableCollection<RibbonContextualTabGroup>();

    #region Tabs

    /// <summary>
    /// gets collection of ribbon tabs
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<RibbonTabItem> Tabs
    {
        get
        {
            if (this.tabs is null)
            {
                this.tabs = new ObservableCollection<RibbonTabItem>();
                this.tabs.CollectionChanged += this.OnTabItemsCollectionChanged;
            }

            return this.tabs;
        }
    }

    /// <summary>
    /// Handles collection of ribbon tab items changes
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">The event data</param>
    private void OnTabItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Replace:
            {
                var isSimplified = this.IsSimplified;
                foreach (var item in e.NewItems.NullSafe().OfType<ISimplifiedStateControl>())
                {
                    item.UpdateSimplifiedState(isSimplified);
                }
            }

                break;

            case NotifyCollectionChangedAction.Reset:
            {
                var isSimplified = this.IsSimplified;
                foreach (var item in this.Tabs.OfType<ISimplifiedStateControl>())
                {
                    item.UpdateSimplifiedState(isSimplified);
                }
            }

                break;
        }

        // This handler runs before tabsSync applies the change to the tab control.
        // The tab control can only move or replace a tab by removing and inserting it. Removing the selected tab selects another one,
        // and while that happens Tabs and the tab control are in a different order, so their indexes must not be mixed
        // (doing that selected tabs back and forth until the stack overflowed).
        // The selection is taken over in OnTabItemsCollectionChangedAfterSync, when both are in the same order again.
        if (this.TabControl is not null
            && (e.Action is NotifyCollectionChangedAction.Move or NotifyCollectionChangedAction.Replace))
        {
            this.isSyncingTabControl = true;
            this.tabSelectedBeforeSync = this.TabControl.SelectedItem as RibbonTabItem;
        }
    }

    /// <summary>
    /// Handles changes of <see cref="Tabs"/> after <see cref="tabsSync"/> applied them to the tab control.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">The event data</param>
    private void OnTabItemsCollectionChangedAfterSync(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (this.isSyncingTabControl == false)
        {
            return;
        }

        var previousSelectedTab = this.SelectedTabItem;

        try
        {
            // A moved tab stays selected. A replaced tab is gone, then the tab control's choice of another tab is kept.
            if (this.tabSelectedBeforeSync is not null
                && this.Tabs.Contains(this.tabSelectedBeforeSync))
            {
                this.TabControl?.SetCurrentValue(System.Windows.Controls.Primitives.Selector.SelectedItemProperty, this.tabSelectedBeforeSync);
            }
        }
        finally
        {
            this.isSyncingTabControl = false;
            this.tabSelectedBeforeSync = null;
        }

        this.SelectedTabItem = this.TabControl?.SelectedItem as RibbonTabItem;

        // Moving tabs can change the index of the selected tab.
        this.SelectedTabIndex = this.TabControl?.SelectedIndex ?? -1;

        if (ReferenceEquals(previousSelectedTab, this.SelectedTabItem) == false)
        {
            this.SelectedTabChanged?.Invoke(this, new SelectionChangedEventArgs(System.Windows.Controls.Primitives.Selector.SelectionChangedEvent, ToList(previousSelectedTab), ToList(this.SelectedTabItem))
            {
                Source = this.TabControl
            });
        }

        static IList ToList(RibbonTabItem? tab) => tab is null ? Array.Empty<object>() : new object[] { tab };
    }
    #endregion

    /// <summary>
    /// Gets collection of toolbar items
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<UIElement> ToolBarItems => this.toolBarItems ??= new ObservableCollection<UIElement>();

    /// <summary>
    /// Gets collection of quick access menu items
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ObservableCollection<QuickAccessMenuItem> QuickAccessItems
    {
        get
        {
            if (this.quickAccessItems is null)
            {
                this.quickAccessItems = new QuickAccessMenuItemCollection();
                this.quickAccessItems.CollectionChanged += this.OnQuickAccessItemsCollectionChanged;
            }

            return this.quickAccessItems;
        }
    }

    /// <summary>
    /// Handles collection of quick access menu items changes
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">The event data</param>
    private void OnQuickAccessItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var item in e.NewItems.NullSafe().OfType<QuickAccessMenuItem>())
                {
                    item.Ribbon = this;
                }

                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (var item in e.OldItems.NullSafe().OfType<QuickAccessMenuItem>())
                {
                    item.Ribbon = null;
                }

                break;

            case NotifyCollectionChangedAction.Replace:
                foreach (var item in e.OldItems.NullSafe().OfType<QuickAccessMenuItem>())
                {
                    item.Ribbon = null;
                }

                foreach (var item in e.NewItems.NullSafe().OfType<QuickAccessMenuItem>())
                {
                    item.Ribbon = this;
                }

                break;

            // Reset (raised by Clear()) carries no OldItems, so QuickAccessMenuItemCollection.ClearItems detaches them instead.
        }
    }

    /// <summary>
    /// Collection used for <see cref="QuickAccessItems"/>.
    /// </summary>
    /// <remarks>
    /// Clear() only raises a Reset, which doesn't contain the removed items.
    /// This is the last moment the items are still known, so detach them from the ribbon here.
    /// </remarks>
    private sealed class QuickAccessMenuItemCollection : ObservableCollection<QuickAccessMenuItem>
    {
        /// <inheritdoc />
        protected override void ClearItems()
        {
            foreach (var item in this.Items)
            {
                if (item is not null)
                {
                    item.Ribbon = null;
                }
            }

            base.ClearItems();
        }
    }

    /// <summary>
    /// Gets or sets whether Customize Quick Access Toolbar menu item is shown
    /// </summary>
    public bool CanCustomizeQuickAccessToolBar
    {
        get => (bool)this.GetValue(CanCustomizeQuickAccessToolBarProperty);
        set => this.SetValue(CanCustomizeQuickAccessToolBarProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="CanCustomizeQuickAccessToolBar"/> dependency property.</summary>
    public static readonly DependencyProperty CanCustomizeQuickAccessToolBarProperty =
        DependencyProperty.Register(nameof(CanCustomizeQuickAccessToolBar), typeof(bool),
            typeof(Ribbon), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets whether items can be added or removed from the quick access toolbar by users.
    /// </summary>
    public bool CanCustomizeQuickAccessToolBarItems
    {
        get => (bool)this.GetValue(CanCustomizeQuickAccessToolBarItemsProperty);
        set => this.SetValue(CanCustomizeQuickAccessToolBarItemsProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="CanCustomizeQuickAccessToolBarItems"/> dependency property.</summary>
    public static readonly DependencyProperty CanCustomizeQuickAccessToolBarItemsProperty =
        DependencyProperty.Register(nameof(CanCustomizeQuickAccessToolBarItems), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets whether the QAT Menu-DropDown is visible or not.
    /// </summary>
    public bool IsQuickAccessToolBarMenuDropDownVisible
    {
        get => (bool)this.GetValue(IsQuickAccessToolBarMenuDropDownVisibleProperty);
        set => this.SetValue(IsQuickAccessToolBarMenuDropDownVisibleProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsQuickAccessToolBarMenuDropDownVisible"/> dependency property.</summary>
    public static readonly DependencyProperty IsQuickAccessToolBarMenuDropDownVisibleProperty =
        DependencyProperty.Register(nameof(IsQuickAccessToolBarMenuDropDownVisible), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets whether Customize Ribbon menu item is shown
    /// </summary>
    public bool CanCustomizeRibbon
    {
        get => (bool)this.GetValue(CanCustomizeRibbonProperty);
        set => this.SetValue(CanCustomizeRibbonProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="CanCustomizeRibbon"/> dependency property.</summary>
    public static readonly DependencyProperty CanCustomizeRibbonProperty =
        DependencyProperty.Register(nameof(CanCustomizeRibbon), typeof(bool),
            typeof(Ribbon), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets whether ribbon can be minimized
    /// </summary>
    public bool CanMinimize
    {
        get => (bool)this.GetValue(CanMinimizeProperty);
        set => this.SetValue(CanMinimizeProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    /// Gets or sets whether ribbon is minimized
    /// </summary>
    public bool IsMinimized
    {
        get => (bool)this.GetValue(IsMinimizedProperty);
        set => this.SetValue(IsMinimizedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsMinimized"/> dependency property.</summary>
    public static readonly DependencyProperty IsMinimizedProperty =
        DependencyProperty.Register(nameof(IsMinimized), typeof(bool),
            typeof(Ribbon), new PropertyMetadata(BooleanBoxes.FalseBox, OnIsMinimizedChanged));

    /// <summary>Identifies the <see cref="CanMinimize"/> dependency property.</summary>
    public static readonly DependencyProperty CanMinimizeProperty =
        DependencyProperty.Register(nameof(CanMinimize), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox));

    private static void OnIsMinimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;

        var oldValue = (bool)e.OldValue;
        var newValue = (bool)e.NewValue;

        ribbon.IsMinimizedChanged?.Invoke(ribbon, e);

        // Invert values of arguments for RaiseExpandCollapseAutomationEvent because IsMinimized means the negative for expand/collapsed
        (UIElementAutomationPeer.FromElement(ribbon) as Fluent.Automation.Peers.RibbonAutomationPeer)?.RaiseExpandCollapseAutomationEvent(!oldValue, !newValue);
    }

    /// <summary>
    /// Gets or sets whether the user can switch between classic and simplified mode.
    /// Setting <see cref="IsSimplified"/> from code still works.
    /// </summary>
    public bool CanUseSimplified
    {
        get => (bool)this.GetValue(CanUseSimplifiedProperty);
        set => this.SetValue(CanUseSimplifiedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="CanUseSimplified"/> dependency property.</summary>
    public static readonly DependencyProperty CanUseSimplifiedProperty =
        DependencyProperty.Register(nameof(CanUseSimplified), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>Identifies the <see cref="IsDisplayOptionsButtonVisible"/> dependency property.</summary>
    public static readonly DependencyProperty IsDisplayOptionsButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsDisplayOptionsButtonVisible), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Defines whether display options button is visible or not.
    /// </summary>
    public bool IsDisplayOptionsButtonVisible
    {
        get => (bool)this.GetValue(IsDisplayOptionsButtonVisibleProperty);
        set => this.SetValue(IsDisplayOptionsButtonVisibleProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    /// Gets or sets the height of the gap between the ribbon and the regular window content
    /// </summary>
    public double ContentGapHeight
    {
        get => (double)this.GetValue(ContentGapHeightProperty);
        set => this.SetValue(ContentGapHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="ContentGapHeight"/> dependency property.</summary>
    public static readonly DependencyProperty ContentGapHeightProperty =
        DependencyProperty.Register(nameof(ContentGapHeight), typeof(double), typeof(Ribbon), new PropertyMetadata(RibbonTabControl.DefaultContentGapHeight));

    /// <summary>
    /// Gets or sets the height of the ribbon content area
    /// </summary>
    public double ContentHeight
    {
        get => (double)this.GetValue(ContentHeightProperty);
        set => this.SetValue(ContentHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="ContentHeight"/> dependency property.</summary>
    public static readonly DependencyProperty ContentHeightProperty =
        DependencyProperty.Register(nameof(ContentHeight), typeof(double), typeof(Ribbon), new PropertyMetadata(RibbonTabControl.DefaultContentHeight));

    // todo check if IsCollapsed and IsAutomaticCollapseEnabled should be reduced to one shared property for RibbonWindow and Ribbon

    /// <summary>
    /// Gets or sets whether ribbon is collapsed.
    /// Set automatically for small windows when <see cref="IsAutomaticCollapseEnabled"/> is true.
    /// </summary>
    public bool IsCollapsed
    {
        get => (bool)this.GetValue(IsCollapsedProperty);
        set => this.SetValue(IsCollapsedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsCollapsed"/> dependency property.</summary>
    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.Register(nameof(IsCollapsed), typeof(bool),
            typeof(Ribbon), new PropertyMetadata(BooleanBoxes.FalseBox, OnIsCollapsedChanged));

    private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;
        ribbon.IsCollapsedChanged?.Invoke(ribbon, e);
    }

    /// <summary>
    /// Defines if the Ribbon should automatically set <see cref="IsCollapsed"/> when the width or height of the owner window drop under <see cref="MinimalVisibleWidth"/> or <see cref="MinimalVisibleHeight"/>
    /// </summary>
    public bool IsAutomaticCollapseEnabled
    {
        get => (bool)this.GetValue(IsAutomaticCollapseEnabledProperty);
        set => this.SetValue(IsAutomaticCollapseEnabledProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsAutomaticCollapseEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty IsAutomaticCollapseEnabledProperty =
        DependencyProperty.Register(nameof(IsAutomaticCollapseEnabled), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox, OnIsAutomaticCollapseEnabledChanged));

    /// <summary>
    /// Gets or sets whether QAT is visible
    /// </summary>
    public bool IsQuickAccessToolBarVisible
    {
        get => (bool)this.GetValue(IsQuickAccessToolBarVisibleProperty);
        set => this.SetValue(IsQuickAccessToolBarVisibleProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsQuickAccessToolBarVisible"/> dependency property.</summary>
    public static readonly DependencyProperty IsQuickAccessToolBarVisibleProperty =
        DependencyProperty.Register(nameof(IsQuickAccessToolBarVisible), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox, OnIsQuickAccessToolBarVisibleChanged));

    private static void OnIsQuickAccessToolBarVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;
        ribbon.TitleBar?.ScheduleForceMeasureAndArrange();
    }

    /// <summary>
    /// Gets or sets whether user can change location of QAT
    /// </summary>
    public bool CanQuickAccessLocationChanging
    {
        get => (bool)this.GetValue(CanQuickAccessLocationChangingProperty);
        set => this.SetValue(CanQuickAccessLocationChangingProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="CanQuickAccessLocationChanging"/> dependency property.</summary>
    public static readonly DependencyProperty CanQuickAccessLocationChangingProperty =
        DependencyProperty.Register(nameof(CanQuickAccessLocationChanging), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>Identifies the <see cref="AreTabHeadersVisible"/> dependency property.</summary>
    public static readonly DependencyProperty AreTabHeadersVisibleProperty = DependencyProperty.Register(nameof(AreTabHeadersVisible), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Defines whether tab headers are visible or not.
    /// </summary>
    public bool AreTabHeadersVisible
    {
        get => (bool)this.GetValue(AreTabHeadersVisibleProperty);
        set => this.SetValue(AreTabHeadersVisibleProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsToolBarVisible"/> dependency property.</summary>
    public static readonly DependencyProperty IsToolBarVisibleProperty = DependencyProperty.Register(nameof(IsToolBarVisible), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Defines whether the toolbar is visible or not.
    /// </summary>
    public bool IsToolBarVisible
    {
        get => (bool)this.GetValue(IsToolBarVisibleProperty);
        set => this.SetValue(IsToolBarVisibleProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsMouseWheelScrollingEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty IsMouseWheelScrollingEnabledProperty = DependencyProperty.Register(nameof(IsMouseWheelScrollingEnabled), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Defines whether scrolling by mouse wheel on the tab container area to cycle tabs is enabled or not.
    /// </summary>
    public bool IsMouseWheelScrollingEnabled
    {
        get => (bool)this.GetValue(IsMouseWheelScrollingEnabledProperty);
        set => this.SetValue(IsMouseWheelScrollingEnabledProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsMouseWheelScrollingEnabledEverywhere"/> dependency property.</summary>
    public static readonly DependencyProperty IsMouseWheelScrollingEnabledEverywhereProperty = DependencyProperty.Register(nameof(IsMouseWheelScrollingEnabledEverywhere), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Defines whether scrolling by mouse wheel always cycles selected tab, also outside the tab container area.
    /// </summary>
    public bool IsMouseWheelScrollingEnabledEverywhere
    {
        get => (bool)this.GetValue(IsMouseWheelScrollingEnabledEverywhereProperty);
        set => this.SetValue(IsMouseWheelScrollingEnabledEverywhereProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    /// Checks if any keytips are visible.
    /// </summary>
    public bool AreAnyKeyTipsVisible => this.keyTipService?.AreAnyKeyTipsVisible == true;

    /// <summary>Identifies the <see cref="IsKeyTipHandlingEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty IsKeyTipHandlingEnabledProperty = DependencyProperty.Register(nameof(IsKeyTipHandlingEnabled), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox, OnIsKeyTipHandlingEnabledChanged));

    /// <summary>
    /// Defines whether handling of key tips is enabled or not.
    /// </summary>
    public bool IsKeyTipHandlingEnabled
    {
        get => (bool)this.GetValue(IsKeyTipHandlingEnabledProperty);
        set => this.SetValue(IsKeyTipHandlingEnabledProperty, BooleanBoxes.Box(value));
    }

    private static void OnIsKeyTipHandlingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;

        if ((bool)e.NewValue)
        {
            ribbon.keyTipService?.Attach();
        }
        else
        {
            ribbon.keyTipService?.Detach();
        }
    }

    /// <summary>
    /// Defines the keys that activate key tips.
    /// Until this collection is changed, the defaults (Alt and F10) are used.
    /// Any change replaces the defaults with exactly the keys in this collection, so clearing it leaves no keys.
    /// </summary>
    public ObservableCollection<Key> KeyTipKeys
    {
        get
        {
            if (this.keyTipKeys is null)
            {
                this.keyTipKeys = new ObservableCollection<Key>();
                this.keyTipKeys.CollectionChanged += this.HandleKeyTipKeys_CollectionChanged;
            }

            return this.keyTipKeys;
        }
    }

    private void HandleKeyTipKeys_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.keyTipService.KeyTipKeys.Clear();

        foreach (var keyTipKey in this.KeyTipKeys)
        {
            this.keyTipService.KeyTipKeys.Add(keyTipKey);
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Gets add to quick access toolbar command
    /// </summary>
    public static readonly RoutedCommand AddToQuickAccessCommand = new(nameof(AddToQuickAccessCommand), typeof(Ribbon));

    /// <summary>
    /// Gets remove from quick access command
    /// </summary>
    public static readonly RoutedCommand RemoveFromQuickAccessCommand = new(nameof(RemoveFromQuickAccessCommand), typeof(Ribbon));

    /// <summary>
    /// Gets show quick access above command
    /// </summary>
    public static readonly RoutedCommand ShowQuickAccessAboveCommand = new(nameof(ShowQuickAccessAboveCommand), typeof(Ribbon));

    /// <summary>
    /// Gets show quick access below command
    /// </summary>
    public static readonly RoutedCommand ShowQuickAccessBelowCommand = new(nameof(ShowQuickAccessBelowCommand), typeof(Ribbon));

    /// <summary>
    /// Gets toggle ribbon minimize command
    /// </summary>
    public static readonly RoutedCommand ToggleMinimizeTheRibbonCommand = new(nameof(ToggleMinimizeTheRibbonCommand), typeof(Ribbon));

    /// <summary>
    /// Gets Switch to classic ribbon command
    /// </summary>
    public static readonly RoutedCommand SwitchToTheClassicRibbonCommand = new(nameof(SwitchToTheClassicRibbonCommand), typeof(Ribbon));

    /// <summary>
    /// Gets Switch to simplified ribbon command
    /// </summary>
    public static readonly RoutedCommand SwitchToTheSimplifiedRibbonCommand = new(nameof(SwitchToTheSimplifiedRibbonCommand), typeof(Ribbon));

    /// <summary>
    /// Gets customize quick access toolbar command
    /// </summary>
    public static readonly RoutedCommand CustomizeQuickAccessToolbarCommand = new(nameof(CustomizeQuickAccessToolbarCommand), typeof(Ribbon));

    /// <summary>
    /// Gets customize the ribbon command
    /// </summary>
    public static readonly RoutedCommand CustomizeTheRibbonCommand = new(nameof(CustomizeTheRibbonCommand), typeof(Ribbon));

    // Occurs when customize toggle minimize command can execute handles
    private static void OnToggleMinimizeTheRibbonCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Ribbon ribbon)
        {
            e.CanExecute = ribbon.CanMinimize;
        }
    }

    // Occurs when toggle minimize command executed
    private static void OnToggleMinimizeTheRibbonCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Ribbon ribbon)
        {
            ribbon.IsMinimized = !ribbon.IsMinimized;
        }
    }

    // Occurs when customize switch ribbon command can execute handles
    private static void OnSwitchTheRibbonCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Ribbon ribbon)
        {
            e.CanExecute = ribbon.CanUseSimplified;
        }
    }

    // Occurs when switch ribbon command executed
    private static void OnSwitchToTheClassicRibbonCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Ribbon ribbon)
        {
            ribbon.IsSimplified = false;
        }
    }

    // Occurs when switch ribbon command executed
    private static void OnSwitchToTheSimplifiedRibbonCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Ribbon ribbon)
        {
            ribbon.IsSimplified = true;
        }
    }

    // Occurs when show quick access below command executed
    private static void OnShowQuickAccessBelowCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var ribbon = sender as Ribbon;

        if (ribbon is null)
        {
            return;
        }

        ribbon.ShowQuickAccessToolBarAboveRibbon = false;
    }

    // Occurs when show quick access above command executed
    private static void OnShowQuickAccessAboveCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var ribbon = sender as Ribbon;

        if (ribbon is null)
        {
            return;
        }

        ribbon.ShowQuickAccessToolBarAboveRibbon = true;
    }

    // Occurs when remove from quick access command executed
    private static void OnRemoveFromQuickAccessCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var ribbon = sender as Ribbon;

        if (ribbon?.QuickAccessToolBar is not null)
        {
            var element = ribbon.QuickAccessElements.First(x => ReferenceEquals(x.Value, e.Parameter)).Key;
            ribbon.RemoveFromQuickAccessToolBar(element);
        }
    }

    // Occurs when add to quick access command executed
    private static void OnAddToQuickAccessCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var ribbon = sender as Ribbon;

        if (ribbon?.QuickAccessToolBar is not null)
        {
            ribbon.AddToQuickAccessToolBar(e.Parameter as UIElement);
        }
    }

    // Occurs when customize quick access command executed
    private static void OnCustomizeQuickAccessToolbarCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var ribbon = sender as Ribbon;

        ribbon?.CustomizeQuickAccessToolbar?.Invoke(sender, EventArgs.Empty);
    }

    // Occurs when customize the ribbon command executed
    private static void OnCustomizeTheRibbonCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var ribbon = sender as Ribbon;

        ribbon?.CustomizeTheRibbon?.Invoke(sender, EventArgs.Empty);
    }

    // Occurs when customize quick access command can execute handles
    private static void OnCustomizeQuickAccessToolbarCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        var ribbon = sender as Ribbon;

        if (ribbon is null)
        {
            return;
        }

        e.CanExecute = ribbon.CanCustomizeQuickAccessToolBar;
    }

    // Occurs when customize the ribbon command can execute handles
    private static void OnCustomizeTheRibbonCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        var ribbon = sender as Ribbon;

        if (ribbon is null)
        {
            return;
        }

        e.CanExecute = ribbon.CanCustomizeRibbon;
    }

    // Occurs when remove from quick access command can execute handles
    private static void OnRemoveFromQuickAccessCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Ribbon ribbon
            && ribbon.IsQuickAccessToolBarVisible
            && e.Parameter is UIElement element)
        {
            e.CanExecute = ribbon.QuickAccessElements.ContainsValue(element);
        }
        else
        {
            e.CanExecute = false;
        }
    }

    // Occurs when add to quick access command can execute handles
    private static void OnAddToQuickAccessCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        // Check the element that AddToQuickAccessToolBar would really add. For a Gallery (or a menu item
        // without an icon) that's the control hosting it, so checking the Gallery itself always said no.
        if (sender is Ribbon ribbon
            && ribbon.IsQuickAccessToolBarVisible
            && GetElementToAddToQuickAccessToolBar(e.Parameter as UIElement) is { } element
            && QuickAccessItemsProvider.IsSupported(element))
        {
            e.CanExecute = ribbon.IsInQuickAccessToolBar(element) == false;
        }
        else
        {
            e.CanExecute = false;
        }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes static members of the <see cref="Ribbon"/> class.
    /// </summary>
    static Ribbon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Ribbon), new FrameworkPropertyMetadata(typeof(Ribbon)));

        // Subscribe to menu commands
        CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(AddToQuickAccessCommand, OnAddToQuickAccessCommandExecuted, OnAddToQuickAccessCommandCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(RemoveFromQuickAccessCommand, OnRemoveFromQuickAccessCommandExecuted, OnRemoveFromQuickAccessCommandCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(ShowQuickAccessAboveCommand, OnShowQuickAccessAboveCommandExecuted));
        CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(ShowQuickAccessBelowCommand, OnShowQuickAccessBelowCommandExecuted));
        CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(ToggleMinimizeTheRibbonCommand, OnToggleMinimizeTheRibbonCommandExecuted, OnToggleMinimizeTheRibbonCommandCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(SwitchToTheClassicRibbonCommand, OnSwitchToTheClassicRibbonCommandExecuted, OnSwitchTheRibbonCommandCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(SwitchToTheSimplifiedRibbonCommand, OnSwitchToTheSimplifiedRibbonCommandExecuted, OnSwitchTheRibbonCommandCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(CustomizeTheRibbonCommand, OnCustomizeTheRibbonCommandExecuted, OnCustomizeTheRibbonCommandCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(CustomizeQuickAccessToolbarCommand, OnCustomizeQuickAccessToolbarCommandExecuted, OnCustomizeQuickAccessToolbarCommandCanExecute));
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public Ribbon()
    {
        this.VerticalAlignment = VerticalAlignment.Top;
        KeyboardNavigation.SetDirectionalNavigation(this, KeyboardNavigationMode.Contained);

        this.keyTipService = new KeyTipService(this);

        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
    }

    #endregion

    #region Overrides

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        this.MaintainIsCollapsed();
    }

    private static void OnIsAutomaticCollapseEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Ribbon)d).MaintainIsCollapsed();
    }

    private void MaintainIsCollapsed()
    {
        if (this.IsAutomaticCollapseEnabled == false
            || this.ownerWindow is null)
        {
            return;
        }

        if (this.ownerWindow.ActualWidth < MinimalVisibleWidth
            || this.ownerWindow.ActualHeight < MinimalVisibleHeight)
        {
            this.SetCurrentValue(IsCollapsedProperty, BooleanBoxes.TrueBox);
        }
        else
        {
            this.SetCurrentValue(IsCollapsedProperty, BooleanBoxes.FalseBox);
        }
    }

    /// <inheritdoc />
    protected override void OnGotFocus(RoutedEventArgs e)
    {
        var ribbonTabItem = (RibbonTabItem?)this.TabControl?.SelectedItem;
        ribbonTabItem?.Focus();
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        this.layoutRoot = this.GetTemplateChild("PART_LayoutRoot") as Panel;

        var selectedTab = this.SelectedTabItem;
        if (this.TabControl is not null)
        {
            this.TabControl.SelectionChanged -= this.OnTabControlSelectionChanged;
            ((INotifyCollectionChanged)this.TabControl.Items).CollectionChanged -= this.OnTabControlItemsCollectionChanged;
            selectedTab = this.TabControl.SelectedItem as RibbonTabItem;

            // Detach the old helpers before clearing their targets, otherwise they keep forwarding
            // changes of Tabs/ToolBarItems to the old (now empty) tab control and throw.
            this.tabsSync?.Detach();
            this.tabsSync?.Target.Clear();

            this.toolBarItemsSync?.Detach();
            this.toolBarItemsSync?.Target.Clear();
        }

        this.TabControl = this.GetTemplateChild("PART_RibbonTabControl") as RibbonTabControl;

        if (this.TabControl is not null)
        {
            this.TabControl.SelectionChanged += this.OnTabControlSelectionChanged;

            this.tabsSync = new CollectionSyncHelper<RibbonTabItem>(this.Tabs, this.TabControl.Items);

            // Has to run after tabsSync, so it's (re)subscribed after creating tabsSync.
            this.Tabs.CollectionChanged -= this.OnTabItemsCollectionChangedAfterSync;
            this.Tabs.CollectionChanged += this.OnTabItemsCollectionChangedAfterSync;

            this.TabControl.SelectedItem = selectedTab;

            // Subscribed after the initial sync, so this only reacts to later changes of Tabs.
            ((INotifyCollectionChanged)this.TabControl.Items).CollectionChanged += this.OnTabControlItemsCollectionChanged;

            this.toolBarItemsSync = new CollectionSyncHelper<UIElement>(this.ToolBarItems, this.TabControl.ToolBarItems);
        }

        // Elements pinned to the old toolbar. They are pinned again below, so re-templating doesn't empty the toolbar.
        // They are taken in the old toolbar's order: after removals the dictionary's order can differ from it.
        var pinnedElements = this.QuickAccessElements
            .OrderBy(x => this.QuickAccessToolBar?.Items.IndexOf(x.Value) ?? 0)
            .Select(x => x.Key)
            .ToList();

        if (this.QuickAccessToolBar is not null)
        {
            this.ClearQuickAccessToolBar();

            this.quickAccessItemsSync?.Detach();
            this.quickAccessItemsSync?.Target.Clear();
        }

        this.QuickAccessToolBar = this.GetTemplateChild("PART_QuickAccessToolBar") as QuickAccessToolBar;

        if (this.QuickAccessToolBar is not null)
        {
            this.quickAccessItemsSync = new CollectionSyncHelper<QuickAccessMenuItem>(this.QuickAccessItems, this.QuickAccessToolBar.QuickAccessItems);

            {
                var binding = new Binding(nameof(this.CanQuickAccessLocationChanging))
                {
                    Source = this,
                    Mode = BindingMode.OneWay
                };
                this.QuickAccessToolBar.SetBinding(QuickAccessToolBar.CanQuickAccessLocationChangingProperty, binding);
            }

            // Elements added before the ribbon had a template (for example in a window constructor)
            // were recorded in QuickAccessElements, but there was no toolbar to show their copies yet.
            foreach (var control in this.QuickAccessElements.Values)
            {
                if (this.QuickAccessToolBar.Items.Contains(control) == false)
                {
                    this.QuickAccessToolBar.Items.Add(control);
                }
            }
        }

        // Pin them again, so each gets a fresh copy.
        // The old copies can't be moved: they are still children of the old toolbar's panel.
        // CanAddToQuickAccessToolBar is not checked here: it only prevents adding, not keeping an element that is already pinned.
        foreach (var element in pinnedElements)
        {
            this.PinToQuickAccessToolBar(element);
        }

        if (this.ShowQuickAccessToolBarAboveRibbon)
        {
            this.MoveQuickAccessToolBarToTitleBar(this.TitleBar);
        }
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new Fluent.Automation.Peers.RibbonAutomationPeer(this);

    private void MoveQuickAccessToolBarToTitleBar(RibbonTitleBar? titleBar)
    {
        if (titleBar is not null)
        {
            titleBar.QuickAccessToolBar = this.QuickAccessToolBar;
        }

        if (this.QuickAccessToolBar is not null)
        {
            // Prevent double add for handler if this method is called multiple times
            this.QuickAccessToolBar.ContextMenuOpening -= this.OnQuickAccessContextMenuOpening;
            this.QuickAccessToolBar.ContextMenuClosing -= this.OnQuickAccessContextMenuClosing;

            this.QuickAccessToolBar.ContextMenuOpening += this.OnQuickAccessContextMenuOpening;
            this.QuickAccessToolBar.ContextMenuClosing += this.OnQuickAccessContextMenuClosing;
        }
    }

    private void RemoveQuickAccessToolBarFromTitleBar(RibbonTitleBar? titleBar)
    {
        // Only remove our own toolbar: the title bar may already show the toolbar of another ribbon
        // that replaced this one in the window.
        if (titleBar is not null
            && ReferenceEquals(titleBar.QuickAccessToolBar, this.QuickAccessToolBar))
        {
            titleBar.QuickAccessToolBar = null;
        }

        if (this.QuickAccessToolBar is not null)
        {
            this.QuickAccessToolBar.ContextMenuOpening -= this.OnQuickAccessContextMenuOpening;
            this.QuickAccessToolBar.ContextMenuClosing -= this.OnQuickAccessContextMenuClosing;
        }
    }

    /// <summary>
    /// Called when the <see cref="ownerWindow"/> is closed, so that we set it to null.
    /// </summary>
    private void OnOwnerWindowClosed(object? sender, EventArgs e)
    {
        this.DetachFromWindow();
    }

    private void AttachToWindow()
    {
        this.DetachFromWindow();

        this.ownerWindow = Window.GetWindow(this);

        if (this.ownerWindow is not null)
        {
            this.ownerWindow.Closed += this.OnOwnerWindowClosed;
            this.ownerWindow.SizeChanged += this.OnSizeChanged;
            this.ownerWindow.KeyDown += this.OnKeyDown;
        }

        // SizeChanged only reports later changes. The window may already be too small
        // (its first SizeChanged happens before the ribbon is loaded), so check it now.
        // Only collapse here: an IsCollapsed set by the app must not be undone on load.
        if (this.IsCollapsed == false)
        {
            this.MaintainIsCollapsed();
        }
    }

    private void DetachFromWindow()
    {
        if (this.ownerWindow is not null)
        {
            this.RibbonStateStorage.Save();
            this.RibbonStateStorage.Dispose();
            this.ribbonStateStorage = null;

            this.ownerWindow.Closed -= this.OnOwnerWindowClosed;
            this.ownerWindow.SizeChanged -= this.OnSizeChanged;
            this.ownerWindow.KeyDown -= this.OnKeyDown;
        }

        this.ownerWindow = null;
    }

    #endregion

    #region Quick Access Items Managment

    /// <summary>
    /// Determines whether the given element is in quick access toolbar
    /// </summary>
    /// <param name="element">Element</param>
    /// <returns>True if element in quick access toolbar</returns>
    public bool IsInQuickAccessToolBar(UIElement? element)
    {
        if (element is null)
        {
            return false;
        }

        return this.QuickAccessElements.ContainsKey(element);
    }

    /// <summary>
    /// Adds the given element to quick access toolbar
    /// </summary>
    /// <param name="element">Element</param>
    public void AddToQuickAccessToolBar(UIElement? element)
    {
        element = GetElementToAddToQuickAccessToolBar(element);

        if (element is null)
        {
            return;
        }

        if (QuickAccessItemsProvider.IsSupported(element) == false)
        {
            return;
        }

        this.PinToQuickAccessToolBar(element);
    }

    /// <summary>
    /// Puts a copy of <paramref name="element"/> on the quick access toolbar, without checking <see cref="IQuickAccessItemProvider.CanAddToQuickAccessToolBar"/>.
    /// </summary>
    private void PinToQuickAccessToolBar(UIElement element)
    {
        if (this.IsInQuickAccessToolBar(element) == false)
        {
            Debug.WriteLine($"Adding \"{element}\" to QuickAccessToolBar.");

            var control = QuickAccessItemsProvider.GetQuickAccessItem(element);

            if (control is not null)
            {
                this.QuickAccessElements.Add(element, control);
                this.QuickAccessToolBar?.Items.Add(control);
            }
        }
    }

    /// <summary>
    /// Gets the element that is put on the quick access toolbar when <paramref name="element"/> is added.
    /// A <see cref="Gallery"/>, or a menu item without an icon, is represented by the ribbon control hosting it.
    /// Used by <see cref="AddToQuickAccessToolBar"/> and by the add command's can-execute check, so both agree.
    /// </summary>
    private static UIElement? GetElementToAddToQuickAccessToolBar(UIElement? element)
    {
        if (element is Gallery)
        {
            element = FindParentRibbonControl(element) as UIElement;
        }

        // Do not add menu items without icon.
        if (element is System.Windows.Controls.MenuItem menuItem && menuItem.Icon is null)
        {
            element = FindParentRibbonControl(element) as UIElement;
        }

        return element;
    }

    private static IRibbonControl? FindParentRibbonControl(DependencyObject element)
    {
        var parent = LogicalTreeHelper.GetParent(element);

        while (parent is not null)
        {
            if (parent is IRibbonControl control)
            {
                return control;
            }

            parent = LogicalTreeHelper.GetParent(parent);
        }

        return null;
    }

    /// <summary>
    /// Removes the given elements from quick access toolbar
    /// </summary>
    /// <param name="element">Element</param>
    public void RemoveFromQuickAccessToolBar(UIElement? element)
    {
        if (element is null)
        {
            return;
        }

        Debug.WriteLine("Removing \"{0}\" from QuickAccessToolBar.", element);

        if (this.IsInQuickAccessToolBar(element))
        {
            var quickAccessItem = this.QuickAccessElements[element];
            this.QuickAccessElements.Remove(element);
            this.QuickAccessToolBar?.Items.Remove(quickAccessItem);
        }
    }

    /// <summary>
    /// Clears quick access toolbar
    /// </summary>
    public void ClearQuickAccessToolBar()
    {
        this.QuickAccessElements.Clear();
        this.QuickAccessToolBar?.Items.Clear();
    }

    #endregion

    #region Event Handling

    // Handles tab control selection changed
    private void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ReferenceEquals(e.OriginalSource, this.TabControl) == false)
        {
            return;
        }

        // While a tab is moved or replaced the tab control removes and inserts it, which can select another tab for a moment.
        // The selection is taken over right after that (OnTabItemsCollectionChangedAfterSync).
        if (this.isSyncingTabControl)
        {
            return;
        }

        this.SelectedTabItem = this.TabControl?.SelectedItem as RibbonTabItem;
        this.SelectedTabIndex = this.TabControl?.SelectedIndex ?? -1;

        this.SelectedTabChanged?.Invoke(this, e);
    }

    // The tab control shifts its SelectedIndex when tabs are inserted or removed before the selected tab,
    // but it doesn't raise SelectionChanged for that because the selected tab stays the same.
    // Without this SelectedTabIndex would keep the old index, pointing to another tab or past the end,
    // and setting it to that old index (for example to select a newly inserted tab) would do nothing.
    // This runs after Tabs has been mirrored into the tab control, so both have the same order here.
    private void OnTabControlItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var selectedTab = this.SelectedTabItem;
        if (selectedTab is null)
        {
            return;
        }

        var index = this.Tabs.IndexOf(selectedTab);
        if (index >= 0
            && index != this.SelectedTabIndex)
        {
            this.SelectedTabIndex = index;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.keyTipService.Attach();

        this.AttachToWindow();

        // Show our toolbar and contextual groups in the title bar again, if they were removed from it in OnUnloaded.
        // OnTitleBarChanged does not run again when the ribbon comes back to the same window.
        // Only for the title bar of our current window: the binding can still hold the title bar of an old window.
        if (this.TitleBar is not null
            && this.ownerWindow is not null
            && ReferenceEquals(Window.GetWindow(this.TitleBar), this.ownerWindow))
        {
            this.TitleBar.ItemsSource = this.ContextualGroups;

            if (this.ShowQuickAccessToolBarAboveRibbon)
            {
                this.MoveQuickAccessToolBarToTitleBar(this.TitleBar);
            }
        }

        this.LoadInitialState();

        this.TitleBar?.ScheduleForceMeasureAndArrange();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            switch (e.Key)
            {
                case Key.F1:
                {
                    if (this.TabControl?.HasItems == true)
                    {
                        if (this.CanMinimize)
                        {
                            this.IsMinimized = !this.IsMinimized;
                        }
                    }

                    break;
                }

                case Key.F2:
                {
                    if (this.TabControl?.HasItems == true)
                    {
                        if (this.CanUseSimplified)
                        {
                            this.IsSimplified = !this.IsSimplified;
                        }
                    }

                    break;
                }
            }
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.RibbonStateStorage.Save();

        this.keyTipService.Detach();

        if (this.ownerWindow is not null)
        {
            // Closed has to be removed too: otherwise a window that stays open (the app removed or replaced the ribbon)
            // keeps this ribbon alive until the window is closed.
            // A ribbon that is loaded again subscribes again in AttachToWindow.
            this.ownerWindow.Closed -= this.OnOwnerWindowClosed;
            this.ownerWindow.SizeChanged -= this.OnSizeChanged;
            this.ownerWindow.KeyDown -= this.OnKeyDown;
        }

        // The TitleBar binding (FindAncestor IRibbonWindow) keeps its value when the ribbon is removed from the window,
        // so OnTitleBarChanged does not run. Take our toolbar and contextual groups out of the title bar here,
        // otherwise the window keeps showing them and keeps this ribbon alive.
        if (this.TitleBar is not null)
        {
            if (ReferenceEquals(this.TitleBar.ItemsSource, this.ContextualGroups))
            {
                this.TitleBar.ItemsSource = null;
            }

            this.RemoveQuickAccessToolBarFromTitleBar(this.TitleBar);
        }
    }

    #endregion

    #region Private methods

    private RibbonTabItem? GetFirstVisibleItem()
    {
        return this.Tabs.FirstOrDefault(item => item.Visibility == Visibility.Visible);
    }

    private RibbonTabItem? GetLastVisibleItem()
    {
        return this.Tabs.LastOrDefault(item => item.Visibility == Visibility.Visible);
    }

    #endregion

    #region State Management

    private void LoadInitialState()
    {
        if (this.RibbonStateStorage.IsLoaded)
        {
            return;
        }

        this.RibbonStateStorage.Load();

        // With AutomaticStateManagement disabled the storage stays "not loaded" (so enabling it later still loads the state),
        // which means we get here on every Loaded.
        // Only select the first tab the first time, so a later Loaded doesn't override the tab selection the app made in the meantime.
        if (this.isInitialTabSelectionDone)
        {
            return;
        }

        this.isInitialTabSelectionDone = true;

        if (this.SelectedTabItem is null)
        {
            this.TabControl?.SelectFirstTab();
        }
    }

    #endregion

    #region AutomaticStateManagement Property

    /// <summary>
    /// Gets or sets whether Quick Access ToolBar can
    /// save and load its state automatically
    /// </summary>
    public bool AutomaticStateManagement
    {
        get => (bool)this.GetValue(AutomaticStateManagementProperty);
        set => this.SetValue(AutomaticStateManagementProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="AutomaticStateManagement"/> dependency property.</summary>
    public static readonly DependencyProperty AutomaticStateManagementProperty =
        DependencyProperty.Register(nameof(AutomaticStateManagement), typeof(bool), typeof(Ribbon), new PropertyMetadata(BooleanBoxes.TrueBox, OnAutomaticStateManagementChanged, CoerceAutomaticStateManagement));

    private static object? CoerceAutomaticStateManagement(DependencyObject d, object? basevalue)
    {
        var ribbon = (Ribbon)d;
        if (ribbon.RibbonStateStorage.IsLoading)
        {
            return BooleanBoxes.FalseBox;
        }

        return basevalue;
    }

    private static void OnAutomaticStateManagementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ribbon = (Ribbon)d;
        if ((bool)e.NewValue)
        {
            ribbon.LoadInitialState();
        }
    }

    #endregion

    /// <inheritdoc />
    void ILogicalChildSupport.AddLogicalChild(object child)
    {
        this.AddLogicalChild(child);
    }

    /// <inheritdoc />
    void ILogicalChildSupport.RemoveLogicalChild(object child)
    {
        this.RemoveLogicalChild(child);
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren
    {
        get
        {
            var baseEnumerator = base.LogicalChildren;
            while (baseEnumerator?.MoveNext() == true)
            {
                yield return baseEnumerator.Current;
            }

            if (this.Menu is not null)
            {
                yield return this.Menu;
            }

            if (this.StartScreen is not null)
            {
                yield return this.StartScreen;
            }

            if (this.QuickAccessToolBar is not null)
            {
                yield return this.QuickAccessToolBar;
            }

            if (this.TabControl?.ToolbarPanel is not null)
            {
                yield return this.TabControl.ToolbarPanel;
            }

            if (this.layoutRoot is not null)
            {
                yield return this.layoutRoot;
            }
        }
    }
}