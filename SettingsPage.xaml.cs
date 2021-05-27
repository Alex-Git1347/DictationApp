using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Dictation
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
            PopulateLanguageInterfaceDropdown();
            PopulateLanguageDropdown();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //radio.Items.Add("item");
            //radio.Items.Add("item1");
            //radio.Items.Add("item2");
        }

        private void RecordingLanguageSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            {
                ComboBoxItem item = (ComboBoxItem)(cbLanguageSelection.SelectedItem);
                Language newLanguage = (Language)item.Tag;
                MainPage.tmp = newLanguage;
            }
        }

        private void PopulateLanguageDropdown()
        {
            Language defaultLanguage = SpeechRecognizer.SystemSpeechLanguage;
            IEnumerable<Language> supportedLanguages = SpeechRecognizer.SupportedTopicLanguages;
            foreach (Language lang in supportedLanguages)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = lang;
                item.Content = lang.NativeName;

                cbLanguageSelection.Items.Add(item);
                if (lang.LanguageTag == defaultLanguage.LanguageTag)
                {
                    item.IsSelected = true;
                    cbLanguageSelection.SelectedItem = item;
                }
            }
        }

        private void PopulateLanguageInterfaceDropdown()
        {
            IEnumerable<string> supportedLanguages = ApplicationLanguages.ManifestLanguages;
            Language defaultLanguage = new Language(ApplicationLanguages.PrimaryLanguageOverride);

            foreach (string lang in supportedLanguages)
            {
                Language temp = new Language(lang);
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = temp.LanguageTag;
                item.Content = temp.NativeName;

                LanguageInterface.Items.Add(item);
                if (temp.LanguageTag == defaultLanguage.LanguageTag)
                {
                    item.IsSelected = true;
                    LanguageInterface.SelectedItem = item;
                }
            }
        }

        static public Language newLanguage;
        private async void LanguageInterface_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Frame != null)
            {
                ComboBoxItem item = (ComboBoxItem)(LanguageInterface.SelectedItem);
                newLanguage = new Language(item.Tag.ToString());

                if (ApplicationLanguages.PrimaryLanguageOverride != newLanguage.LanguageTag)
                {
                    try
                    {
                        Frame.CacheSize = 0;
                        ApplicationLanguages.PrimaryLanguageOverride = newLanguage.LanguageTag;
                        //ResourceContext.GetForCurrentView().Reset();
                        //ResourceContext.GetForViewIndependentUse().Reset();
                        LanguageInterface.UpdateLayout();

                        //Frame.Navigate(this.GetType());
                        //this.Frame.Navigate(typeof(settings));
                    }
                    catch (ArgumentException exception)
                    {
                        var messageDialog = new MessageDialog(exception.Message, "Exception");
                        await messageDialog.ShowAsync();
                    }
                }
            }
        }

        private void SelectedThemeApp(object sender, RoutedEventArgs e)
        {
            //ComboBoxItem item = (ComboBoxItem)(ColorTheme.SelectedItem);
            //if (item.Content.ToString() == "Light")
            //{
            //    //App.Current.RequestedTheme = ApplicationTheme.Light;
            //    //Application.Current.RequestedTheme = ApplicationTheme.Light;
            //    if (Window.Current.Content is FrameworkElement frameworkElement)
            //    {
            //        frameworkElement.RequestedTheme = ElementTheme.Light;

            //    }
            //}
            //else if (item.Content.ToString() == "Dark")
            //{
            //    //Application.Current.RequestedTheme = ApplicationTheme.Light;
            //    //App.Current.RequestedTheme = ApplicationTheme.Dark;
            //    if (Window.Current.Content is FrameworkElement frameworkElement)
            //    {
            //        frameworkElement.RequestedTheme = ElementTheme.Dark;
            //    }
            //}
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void BackgroundColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }
    }
}
