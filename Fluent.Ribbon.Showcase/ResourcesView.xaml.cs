namespace FluentTest
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using ControlzEx.Theming;

    public partial class ResourcesView
    {
        public ResourcesView()
        {
            this.InitializeComponent();

            this.ThemeResources = new ObservableCollection<ThemeResource>();

            var view = CollectionViewSource.GetDefaultView(this.ThemeResources);

            view.SortDescriptions.Add(new SortDescription(nameof(ThemeResource.Key), ListSortDirection.Ascending));

            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ThemeResource.Theme)));
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ThemeResource.LibraryTheme)));

            // ThemeManager.Current.ThemeChanged is static and raised on the thread that changed the theme.
            // Subscribing in the constructor (and never unsubscribing) kept every ResourcesView ever created alive and subscribed,
            // including the one of a window opened on its own UI thread ("Open Ribbon-Window (new Thread)") after that window was closed.
            // The next theme change on the main thread then ran this view's handler on the wrong thread: VerifyAccess threw,
            // which was swallowed by the binding for BaseColors/Theme (so the change looked like it did nothing)
            // and terminated the process when it came from "Sync now".
            // So only listen while loaded, and refresh on Loaded because changes made while unloaded (e.g. on another tab) were missed.
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ThemeChanged -= this.ThemeManager_ThemeChanged;
            ThemeManager.Current.ThemeChanged += this.ThemeManager_ThemeChanged;

            this.UpdateThemeAnalyzers(ThemeManager.Current.DetectTheme());
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ThemeChanged -= this.ThemeManager_ThemeChanged;
        }

        public static readonly DependencyProperty ThemeResourcesProperty = DependencyProperty.Register(
            nameof(ThemeResources), typeof(ObservableCollection<ThemeResource>), typeof(ResourcesView), new PropertyMetadata(default(ObservableCollection<ThemeResource>)));

        public ObservableCollection<ThemeResource>? ThemeResources
        {
            get => (ObservableCollection<ThemeResource>?)this.GetValue(ThemeResourcesProperty);
            set => this.SetValue(ThemeResourcesProperty, value);
        }

        public class ThemeResource
        {
            public ThemeResource(Theme theme, LibraryTheme libraryTheme, ResourceDictionary resourceDictionary, DictionaryEntry dictionaryEntry)
                : this(theme, libraryTheme, resourceDictionary, dictionaryEntry.Key.ToString()!, dictionaryEntry.Value!)
            {
            }

            public ThemeResource(Theme theme, LibraryTheme libraryTheme, ResourceDictionary resourceDictionary, string key, object value)
            {
                this.Theme = theme;
                this.LibraryTheme = libraryTheme;

                this.Source = resourceDictionary.Source?.ToString() ?? "Runtime";
                this.Key = key;

                this.Value = value switch
                {
                    Color color => new Rectangle { Fill = new SolidColorBrush(color) },
                    Brush brush => new Rectangle { Fill = brush },
                    _ => null
                };

                this.StringValue = value.ToString()!;
            }

            public Theme Theme { get; }

            public LibraryTheme LibraryTheme { get; }

            public string Source { get; }

            public string Key { get; }

            public object? Value { get; }

            public string StringValue { get; }
        }

        private void ThemeManager_ThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            // A view on another UI thread (a window opened on its own thread) is still loaded while the main thread changes the theme.
            if (this.Dispatcher.CheckAccess() == false)
            {
                if (this.Dispatcher.HasShutdownStarted)
                {
                    // That thread is gone (its window was closed without Unloaded reaching us), stop listening.
                    ThemeManager.Current.ThemeChanged -= this.ThemeManager_ThemeChanged;
                    return;
                }

                var newTheme = e.NewTheme;
                this.Dispatcher.BeginInvoke(new Action(() => this.UpdateThemeAnalyzers(newTheme)));
                return;
            }

            this.UpdateThemeAnalyzers(e.NewTheme);
        }

        private void UpdateThemeAnalyzers(Theme? theme)
        {
            if (this.ThemeResources is null)
            {
                return;
            }

            this.ThemeResources.Clear();

            if (theme is null)
            {
                return;
            }

            foreach (var libraryTheme in theme.LibraryThemes)
            {
                foreach (var resourceDictionary in libraryTheme.Resources.MergedDictionaries)
                {
                    foreach (DictionaryEntry dictionaryEntry in resourceDictionary)
                    {
                        this.ThemeResources.Add(new ThemeResource(theme, libraryTheme, resourceDictionary, dictionaryEntry));
                    }
                }
            }
        }
    }
}