using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace Dictation
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        private RecognizerSpeechViewModel RecognizerViewModel { get; set; }
        string languageTag = "";
        
        
        public MainPage()
        {
            this.InitializeComponent();
            RecognizerViewModel = new RecognizerSpeechViewModel(dispatcher);
            PopulateLanguageDropdown();
            PopulateLanguageInterfaceDropdown();
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

        private async void cbLanguageSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecognizerViewModel.RecognizerSpeech.SpeechRecognizer != null)
            {
                ComboBoxItem item = (ComboBoxItem)(cbLanguageSelection.SelectedItem);
                Language newLanguage = (Language)item.Tag;
                if (RecognizerViewModel.RecognizerSpeech.SpeechRecognizer.CurrentLanguage != newLanguage)
                {
                    try
                    {
                        RecognizerViewModel.RecognizerSpeech = null;
                        RecognizerViewModel.RecognizerSpeech = new RecognizerSpeech(dispatcher, newLanguage);
                        languageTag = newLanguage.LanguageTag;
                    }
                    catch (Exception exception)
                    {
                        var messageDialog = new MessageDialog(exception.Message, "Exception");
                        await messageDialog.ShowAsync();
                    }
                }
            }
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {

            if (RecognizerViewModel.RecognizerSpeech.dictatedTextBuilder.Length != 0)
            {
                RecognizerViewModel.RecognizerSpeech.dictatedTextBuilder.Clear();
                Bindings.Update();
            }
            RecognizerViewModel.RecognizerSpeech.StartRecording();
            
        }

        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            RecognizerViewModel.RecognizerSpeech.StopRecording();
            Bindings.Update();
        }

        private void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            RecognizerViewModel.RecognizerSpeech.ClearRecordingText();
            Bindings.Update();
        }

        private async void LanguageInterface_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Frame != null)
            {
                ComboBoxItem item = (ComboBoxItem)(LanguageInterface.SelectedItem);
                Language newLanguage =new Language(item.Tag.ToString());

                if (ApplicationLanguages.PrimaryLanguageOverride != newLanguage.LanguageTag)
                {
                    try
                    {
                        Frame.CacheSize = 0;
                        ApplicationLanguages.PrimaryLanguageOverride = newLanguage.LanguageTag;
                        ResourceContext.GetForCurrentView().Reset();
                        ResourceContext.GetForViewIndependentUse().Reset();
                        LanguageInterface.UpdateLayout();
                        
                        Frame.Navigate(this.GetType());
                    }
                    catch (Exception exception)
                    {
                        var messageDialog = new MessageDialog(exception.Message, "Exception");
                        await messageDialog.ShowAsync();
                    }
                }
            }
        }

        private void SelectedThemePage(object sender, RoutedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)(ColorTheme.SelectedItem);
            if (item.Content.ToString() == "Light")
            {
                mainPage.RequestedTheme = ElementTheme.Light;
            }
            else if (item.Content.ToString() == "Dark")
            {
                mainPage.RequestedTheme = ElementTheme.Dark;
            }

        }
    }
}
