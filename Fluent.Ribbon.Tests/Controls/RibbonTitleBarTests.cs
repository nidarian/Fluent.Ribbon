namespace Fluent.Tests.Controls;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Fluent.Tests.Helper;
using Fluent.Tests.TestClasses;
using NUnit.Framework;

[TestFixture]
public sealed class RibbonTitleBarTests
{
    [TestFixture]
    public class MeasureTests
    {
        private static readonly Size zeroSize = default;
        private const double ReferenceWidth = 1024;
        private static readonly double ReferenceHeight = SystemParameters.WindowCaptionHeight;
        private static readonly Size referenceSize = new(ReferenceWidth, SystemParameters.WindowCaptionHeight);
        private const string QuickaccessToolbarRect = "quickAccessToolbarRect";
        private const string HeaderRect = "headerRect";
        private const string ItemsRect = "itemsRect";
        private static readonly double DefaultTitleBarHeight = SystemParameters.WindowCaptionHeight;

        [Test]
        public void Without_Parts()
        {
            var titlebar = new RibbonTitleBar();

            titlebar.Measure(referenceSize);

            Assert.That(titlebar.DesiredSize, Is.EqualTo(zeroSize));
        }

        [Test]
        public void Empty()
        {
            var titlebar = new RibbonTitleBar();

            using (new TestRibbonWindow(titlebar))
            {
                titlebar.Measure(referenceSize);

                Assert.That(titlebar.DesiredSize, Is.EqualTo(new Size(2, DefaultTitleBarHeight)));
            }
        }

        [Test]
        public void Collapsed()
        {
            var titlebar = new RibbonTitleBar
            {
                IsCollapsed = true
            };

            using (new TestRibbonWindow(titlebar))
            {
                titlebar.Measure(referenceSize);

                Assert.That(titlebar.DesiredSize, Is.EqualTo(new Size(2, DefaultTitleBarHeight)));
            }
        }

        [Test]
        [TestCaseSource(nameof(With_Header_TestData))]
        public void With_Header(RibbonTitleBarSizeData testdata)
        {
            var titlebar = CreateNewTitlebar();

            using (new TestRibbonWindow(titlebar))
            {
                titlebar.Measure(new Size(testdata.ConstraintWidth, ReferenceHeight));

                var resultData = new RibbonTitleBarSizeData(testdata.ConstraintWidth, titlebar);
                Assert.That(resultData, Is.EqualTo(testdata));
            }
        }

        private static IEnumerable<RibbonTitleBarSizeData> With_Header_TestData()
        {
            yield return new RibbonTitleBarSizeData(ReferenceWidth, new Size(89, DefaultTitleBarHeight), zeroSize, new Size(89, DefaultTitleBarHeight), zeroSize);
            yield return new RibbonTitleBarSizeData(100, new Size(89, DefaultTitleBarHeight), zeroSize, new Size(89, DefaultTitleBarHeight), zeroSize);
            yield return new RibbonTitleBarSizeData(52, new Size(52, DefaultTitleBarHeight), zeroSize, new Size(54, DefaultTitleBarHeight), zeroSize);
            yield return new RibbonTitleBarSizeData(50, new Size(50, DefaultTitleBarHeight), zeroSize, new Size(52, DefaultTitleBarHeight), zeroSize);
            yield return new RibbonTitleBarSizeData(10, new Size(10, DefaultTitleBarHeight), zeroSize, new Size(52, DefaultTitleBarHeight), zeroSize);
        }

        [Test]
        [TestCaseSource(nameof(With_Parts_And_Wide_Header_TestData))]
        public void With_Wide_Header(RibbonTitleBarSizeData testdata)
        {
            var titlebar = CreateNewTitlebar();
            titlebar.Header = "This is a really wide header which needs some more space";

            using (new TestRibbonWindow(titlebar))
            {
                titlebar.Measure(new Size(testdata.ConstraintWidth, ReferenceHeight));

                var resultData = new RibbonTitleBarSizeData(testdata.ConstraintWidth, titlebar);
                Assert.That(resultData, Is.EqualTo(testdata));
            }
        }

        private static IEnumerable<RibbonTitleBarSizeData> With_Parts_And_Wide_Header_TestData()
        {
            yield return new RibbonTitleBarSizeData(ReferenceWidth, new Size(309, DefaultTitleBarHeight), zeroSize, new Size(309, DefaultTitleBarHeight), zeroSize);
            yield return new RibbonTitleBarSizeData(100, new Size(100, DefaultTitleBarHeight), zeroSize, new Size(102, DefaultTitleBarHeight), zeroSize);
            yield return new RibbonTitleBarSizeData(52, new Size(52, DefaultTitleBarHeight), zeroSize, new Size(54, DefaultTitleBarHeight), zeroSize);
            yield return new RibbonTitleBarSizeData(50, new Size(50, DefaultTitleBarHeight), zeroSize, new Size(52, DefaultTitleBarHeight), zeroSize);
            yield return new RibbonTitleBarSizeData(10, new Size(10, DefaultTitleBarHeight), zeroSize, new Size(52, DefaultTitleBarHeight), zeroSize);
        }

        [Test]
        [TestCaseSource(nameof(With_Header_And_QuickAccessItems_TestData))]
        public void With_Header_And_QuickAccessItems(RibbonTitleBarSizeData testdata)
        {
            var titlebar = CreateNewTitlebar();
            var quickAccessToolBar = (QuickAccessToolBar)(titlebar.QuickAccessToolBar = new QuickAccessToolBar());

            using (new TestRibbonWindow(titlebar))
            {
                quickAccessToolBar.Items.Add(new TextBlock { Text = "ABC" });
                quickAccessToolBar.Items.Add(new TextBlock { Text = "ABC" });
                quickAccessToolBar.Items.Add(new TextBlock { Text = "ABC" });

                titlebar.Measure(new Size(testdata.ConstraintWidth, ReferenceHeight));

                var resultData = new RibbonTitleBarSizeData(testdata.ConstraintWidth, titlebar);
                Assert.That(resultData, Is.EqualTo(testdata));
            }
        }

        private static IEnumerable<RibbonTitleBarSizeData> With_Header_And_QuickAccessItems_TestData()
        {
            yield return new RibbonTitleBarSizeData(ReferenceWidth, new Size(177, DefaultTitleBarHeight), new Size(88, DefaultTitleBarHeight - 1), new Size(89, DefaultTitleBarHeight), zeroSize);
            yield return new RibbonTitleBarSizeData(100, new Size(100, DefaultTitleBarHeight), new Size(40, DefaultTitleBarHeight - 1), new Size(62, DefaultTitleBarHeight), zeroSize);
            yield return new RibbonTitleBarSizeData(52, new Size(52, DefaultTitleBarHeight), new Size(2, DefaultTitleBarHeight - 1), new Size(52, DefaultTitleBarHeight), zeroSize);
            yield return new RibbonTitleBarSizeData(50, new Size(50, DefaultTitleBarHeight), new Size(0, DefaultTitleBarHeight - 1), new Size(52, DefaultTitleBarHeight), zeroSize);
            yield return new RibbonTitleBarSizeData(10, new Size(10, DefaultTitleBarHeight), new Size(0, DefaultTitleBarHeight - 1), new Size(52, DefaultTitleBarHeight), zeroSize);
        }

        [Test]
        public void Without_QuickAccessToolbarHolder_Header_Should_Get_Its_Desired_Width()
        {
            var titlebar = CreateTitlebarWithOnlyHeaderHolder();
            titlebar.HeaderAlignment = HorizontalAlignment.Left;

            using (new TestRibbonWindow(titlebar))
            {
                titlebar.InvalidateMeasure();
                titlebar.Measure(new Size(800, 30));
                titlebar.Arrange(new Rect(0, 0, 800, 30));

                Assert.That(titlebar.Template.FindName("PART_HeaderHolder", titlebar), Is.Not.Null, "Precondition: template should contain the header holder.");
                Assert.That(titlebar.Template.FindName("PART_QuickAccessToolbarHolder", titlebar), Is.Null, "Precondition: template should not contain a quick access toolbar holder.");

                var headerRect = titlebar.GetFieldValue<Rect>(HeaderRect);

                // Header holder is 300 wide, plus the 2 pixels Update always adds.
                Assert.That(headerRect.Width, Is.EqualTo(302));
                Assert.That(headerRect.X, Is.EqualTo(0));
            }
        }

        [Test]
        public void Without_QuickAccessToolbarHolder_Header_Should_Not_Grow_With_Every_Layout_Pass_When_Contextual_Groups_Are_Visible()
        {
            var titlebar = CreateTitlebarWithOnlyHeaderHolder();
            titlebar.HeaderAlignment = HorizontalAlignment.Right;

            // The tab item has to share a visual root with the titlebar because Update translates its position.
            var tabItem = new RibbonTabItem { Header = "Contextual" };
            var group = new RibbonContextualTabGroup();
            group.Items.Add(tabItem);
            group.Visibility = Visibility.Visible;
            titlebar.Items.Add(group);

            var content = new StackPanel();
            content.Children.Add(titlebar);
            content.Children.Add(tabItem);

            using (new TestRibbonWindow(content))
            {
                Assert.That(group.InnerVisibility, Is.EqualTo(Visibility.Visible), "Precondition: contextual group should be visible.");

                var constraint = new Size(800, 30);

                titlebar.InvalidateMeasure();
                titlebar.Measure(constraint);
                var firstHeaderRect = titlebar.GetFieldValue<Rect>(HeaderRect);

                titlebar.InvalidateMeasure();
                titlebar.Measure(constraint);
                var secondHeaderRect = titlebar.GetFieldValue<Rect>(HeaderRect);

                Assert.That(secondHeaderRect, Is.EqualTo(firstHeaderRect));
            }
        }

        // Custom templates are allowed to omit PART_QuickAccessToolbarHolder and PART_ItemsContainer.
        private static RibbonTitleBar CreateTitlebarWithOnlyHeaderHolder()
        {
            var headerHolder = new FrameworkElementFactory(typeof(Border), "PART_HeaderHolder");
            headerHolder.SetValue(FrameworkElement.WidthProperty, 300D);

            return new RibbonTitleBar
            {
                Template = new ControlTemplate(typeof(RibbonTitleBar))
                {
                    VisualTree = headerHolder
                },
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };
        }

#pragma warning disable CA1815 // Override equals and operator equals on value types
        public struct RibbonTitleBarSizeData
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            public RibbonTitleBarSizeData(double constraintWidth, Size desiredSize, Size quickAccessRectSize, Size headerRectSize, Size itemsRectSize)
            {
                this.ConstraintWidth = constraintWidth;
                this.DesiredSize = desiredSize;
                this.QuickAccessRectSize = quickAccessRectSize;
                this.HeaderRectSize = headerRectSize;
                this.ItemsRectSize = itemsRectSize;
            }

            public RibbonTitleBarSizeData(double constraintWidth, RibbonTitleBar ribbonTitleBar)
                : this(
                    constraintWidth,
                    ribbonTitleBar.DesiredSize,
                    ribbonTitleBar.GetFieldValue<Rect>(QuickaccessToolbarRect).Size,
                    ribbonTitleBar.GetFieldValue<Rect>(HeaderRect).Size,
                    ribbonTitleBar.GetFieldValue<Rect>(ItemsRect).Size)
            {
            }

            public double ConstraintWidth { get; }

            public Size DesiredSize { get; }

            public Size QuickAccessRectSize { get; }

            public Size HeaderRectSize { get; }

            public Size ItemsRectSize { get; }

            public override string ToString()
            {
                return $"[{this.ConstraintWidth}=>{this.DesiredSize}]#{this.QuickAccessRectSize}#{this.HeaderRectSize}#{this.ItemsRectSize}";
            }
        }

        private static RibbonTitleBar CreateNewTitlebar()
        {
            return new RibbonTitleBar
            {
                Header = "This is just a test",
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };
        }
    }
}