namespace Fluent.Tests.Localization;

using System.ComponentModel;
using Fluent.Localization;
using Fluent.Localization.Languages;
using NUnit.Framework;

/// <summary>
/// Apps change the ribbon texts by assigning their own localization object to <see cref="RibbonLocalization.Localization"/>.
/// That assignment must take effect even when the new object reports the same culture as the current one,
/// e.g. a class derived from <see cref="English"/> (it inherits the [RibbonLocalization("English", "en")] attribute)
/// or two custom localizations without the attribute (both have no culture name).
/// </summary>
[TestFixture]
public class CustomLocalizationReplaceTests
{
    [Test]
    public void Localization_Derived_From_English_Should_Replace_Current_English_Localization()
    {
        var originalLocalization = RibbonLocalization.Current.Localization;

        try
        {
            RibbonLocalization.Current.Localization = new English();
            Assert.That(RibbonLocalization.Current.Localization.CultureName, Is.EqualTo("en"), "Precondition: an english localization must be current.");

            var myTexts = new MyEnglishTexts();
            Assert.That(myTexts.CultureName, Is.EqualTo("en"), "Precondition: the derived class inherits the culture of English.");

            RibbonLocalization.Current.Localization = myTexts;

            Assert.That(RibbonLocalization.Current.Localization, Is.SameAs(myTexts));
            Assert.That(RibbonLocalization.Current.Localization.CustomizeStatusBar, Is.EqualTo(MyEnglishTexts.MyCustomizeStatusBar));
        }
        finally
        {
            RibbonLocalization.Current.Localization = originalLocalization;
        }
    }

    [Test]
    public void Localization_Without_Attribute_Should_Replace_Other_Localization_Without_Attribute()
    {
        var originalLocalization = RibbonLocalization.Current.Localization;

        try
        {
            var first = new TextsWithoutAttribute("First");
            var second = new TextsWithoutAttribute("Second");
            Assert.That(first.CultureName, Is.Null, "Precondition: no attribute means no culture name.");

            RibbonLocalization.Current.Localization = first;
            Assert.That(RibbonLocalization.Current.Localization, Is.SameAs(first), "Precondition: the first custom localization must be current.");

            RibbonLocalization.Current.Localization = second;

            Assert.That(RibbonLocalization.Current.Localization, Is.SameAs(second));
            Assert.That(RibbonLocalization.Current.Localization.CustomizeStatusBar, Is.EqualTo("Second"));
        }
        finally
        {
            RibbonLocalization.Current.Localization = originalLocalization;
        }
    }

    [Test]
    public void Setting_The_Same_Localization_Instance_Again_Should_Not_Raise_PropertyChanged_Again()
    {
        var originalLocalization = RibbonLocalization.Current.Localization;
        var raisedCount = 0;

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RibbonLocalization.Localization))
            {
                raisedCount++;
            }
        }

        RibbonLocalization.Current.PropertyChanged += OnPropertyChanged;

        try
        {
            var myTexts = new TextsWithoutAttribute("Mine");

            RibbonLocalization.Current.Localization = myTexts;
            RibbonLocalization.Current.Localization = myTexts;

            Assert.That(raisedCount, Is.EqualTo(1));
        }
        finally
        {
            RibbonLocalization.Current.PropertyChanged -= OnPropertyChanged;
            RibbonLocalization.Current.Localization = originalLocalization;
        }
    }

    private sealed class MyEnglishTexts : English
    {
        public const string MyCustomizeStatusBar = "My customized status bar text";

        public override string CustomizeStatusBar => MyCustomizeStatusBar;
    }

    private sealed class TextsWithoutAttribute : RibbonLocalizationBase
    {
        private static readonly English Defaults = new();

        private readonly string customizeStatusBar;

        public TextsWithoutAttribute(string customizeStatusBar)
        {
            this.customizeStatusBar = customizeStatusBar;
        }

        public override string CustomizeStatusBar => this.customizeStatusBar;

        public override string Automatic => Defaults.Automatic;

        public override string BackstageBackButtonUid => Defaults.BackstageBackButtonUid;

        public override string BackstageButtonKeyTip => Defaults.BackstageButtonKeyTip;

        public override string BackstageButtonText => Defaults.BackstageButtonText;

        public override string MoreColors => Defaults.MoreColors;

        public override string NoColor => Defaults.NoColor;

        public override string QuickAccessToolBarDropDownButtonTooltip => Defaults.QuickAccessToolBarDropDownButtonTooltip;

        public override string QuickAccessToolBarMenuHeader => Defaults.QuickAccessToolBarMenuHeader;

        public override string QuickAccessToolBarMenuShowAbove => Defaults.QuickAccessToolBarMenuShowAbove;

        public override string QuickAccessToolBarMenuShowBelow => Defaults.QuickAccessToolBarMenuShowBelow;

        public override string QuickAccessToolBarMoreControlsButtonTooltip => Defaults.QuickAccessToolBarMoreControlsButtonTooltip;

        public override string RibbonContextMenuAddGallery => Defaults.RibbonContextMenuAddGallery;

        public override string RibbonContextMenuAddGroup => Defaults.RibbonContextMenuAddGroup;

        public override string RibbonContextMenuAddItem => Defaults.RibbonContextMenuAddItem;

        public override string RibbonContextMenuAddMenu => Defaults.RibbonContextMenuAddMenu;

        public override string RibbonContextMenuCustomizeQuickAccessToolBar => Defaults.RibbonContextMenuCustomizeQuickAccessToolBar;

        public override string RibbonContextMenuCustomizeRibbon => Defaults.RibbonContextMenuCustomizeRibbon;

        public override string RibbonContextMenuMinimizeRibbon => Defaults.RibbonContextMenuMinimizeRibbon;

        public override string RibbonContextMenuRemoveItem => Defaults.RibbonContextMenuRemoveItem;

        public override string RibbonContextMenuShowAbove => Defaults.RibbonContextMenuShowAbove;

        public override string RibbonContextMenuShowBelow => Defaults.RibbonContextMenuShowBelow;

        public override string UseClassicRibbon => Defaults.UseClassicRibbon;

        public override string UseSimplifiedRibbon => Defaults.UseSimplifiedRibbon;

        public override string ScreenTipDisableReasonHeader => Defaults.ScreenTipDisableReasonHeader;

        public override string ScreenTipF1LabelHeader => Defaults.ScreenTipF1LabelHeader;
    }
}
