using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Dictation
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class RecentList : Page
    {
        //AppWindow appWindow;
        //static public StorageFile openFile;
        public RecentFilesViewModel RecentView { get; set; }
        RecognizerSpeechViewModel recognizerSpeechViewModel { get; set; }
        public RecentList()
        {
            this.InitializeComponent();
            ////appWindow = MainPage.appWindow;
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            recognizerSpeechViewModel = e.Parameter as RecognizerSpeechViewModel;
            this.RecentView = new RecentFilesViewModel(recognizerSpeechViewModel);
        }
                
        //private async void OpenFile_Click(object sender, RoutedEventArgs e)
        //{
        //    Button button = (Button)sender;
        //    StorageFile file = files.FirstOrDefault(f => f.Path.ToString() == button.Content.ToString());
        //    openFile = file;
        //    await appWindow.CloseAsync();
        //    //MainPage.page.RecognizerViewModel.RecognizerSpeech.dictatedTextBuilder = "";
        //}

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            //HyperlinkButton hyperlink = (HyperlinkButton)sender;
            //StorageFile file = RecentView.ResentFiles.FirstOrDefault(f => f.thisFile.Name == hyperlink.Content.ToString()).thisFile;
            //openFile = file;
            //await appWindow.CloseAsync();
            //MainPage.page.RecognizerViewModel.RecognizerSpeech.dictatedTextBuilder = file.;FirstOrDefault(f => f.Name.ToString() == hyperlink.Content.ToString());
        }
    }
}
