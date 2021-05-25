using System;
using System.Collections.Generic;
using System.IO;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Background;
using System.Linq;
using Windows.Graphics.Printing;
using Windows.UI.Xaml.Hosting;
using Windows.UI.WindowManagement;
using Windows.Foundation;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using System.Threading.Tasks;


// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

//[assembly: CLSCompliant(true)]
namespace Dictation
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage 
    {
        static public string token;
        private CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        private RecognizerSpeechViewModel RecognizerViewModel { get; set; }
        string languageTag = "";
        string CurrentFormatFile="";
        ShareOperation shareOperation = null;
        public StorageFile openFile;
        public StorageFile recentFile;
        static public Language tmp;
        private PrintHelper printHelper;
        string taskName = "AutoSaveFile";
        public static MainPage page;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            RecognizerViewModel = new RecognizerSpeechViewModel(dispatcher);
            HaveTempFile();
            page = (MainPage)this.mainPage;
            
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            mySplitView.IsPaneOpen = !mySplitView.IsPaneOpen;
        }

        private void MenuBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveAs.IsSelected)
            {
                SaveAs_Click(sender, e);
                //TitleTextBlock.Text = "Главная";
            }
            else if (recentFiles.IsSelected)
            {
                Recent_Click(sender, e);
                //TitleTextBlock.Text = "Поделиться";
            }
            else if (settings.IsSelected)
            {
                Settings_Click(sender, e);
                // myFrame.Navigate(typeof(settings));
                //TitleTextBlock.Text = "Настройки";
            }
            else if (print.IsSelected)
            {
                OnPrintButtonClick(sender, e);
                // myFrame.Navigate(typeof(settings));
                //Tit

            }
        }
            private async void OnPrintButtonClick(object sender, RoutedEventArgs e)
        {
            string tempText = dictationTextBox.Text;
            try
            {
                printHelper.RegisterForPrinting();
                printHelper.PreparePrintContent(new PageToPrint(tempText));
            }
            catch (Exception arg)
            {

            }
            await printHelper.ShowPrintUIAsync().ConfigureAwait(false);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (printHelper != null)
            {
                printHelper.UnregisterForPrinting();
            }
        }

        //private void ClosePopupClicked(object sender, RoutedEventArgs e)
        //{
        //    // if the Popup is open, then close it 
        //    if (StandardPopup.IsOpen) { StandardPopup.IsOpen = false; }
        //}

        //// Handles the Click event on the Button on the page and opens the Popup. 
        //private void ShowPopupOffsetClicked(object sender, RoutedEventArgs e)
        //{
        //    // open the Popup if it isn't open already 
        //    if (!StandardPopup.IsOpen) { StandardPopup.IsOpen = true; }
        //}

        private async void HaveTempFile()
        {            
            StorageFile stFile;
            StorageFolder localFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            try
            {
                stFile = await localFolder.GetFileAsync("temp.txt");
            }
            catch (FileNotFoundException)
            {
                stFile = null;
            }
            if (stFile != null)
            {
                ContentDialog restoreFile = new ContentDialog
                {
                    Content = "Хотите востановить несохраненный документ?",
                    CloseButtonText = "No",
                    PrimaryButtonText = "Yes"
                };

                ContentDialogResult result = await restoreFile.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    string text = await Windows.Storage.FileIO.ReadTextAsync(stFile);
                    dictationTextBox.Text = text;
                }
                else
                {
                    stFile = await localFolder.GetFileAsync("temp.txt");
                    await stFile.DeleteAsync();
                }
            }
        }
                
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.mainPage.IsEnabled == false)
            {
                this.mainPage.IsEnabled = true;
            }
            try
            {
                if (e.Parameter is StorageFile)
                {
                    StorageFile f = (StorageFile)e.Parameter;
                    openFile = f;
                    Open_openFile();
                }
            }
            catch (NullReferenceException)
            {

            }

            try
            {
                //var entry = this.Frame.BackStack.LastOrDefault();
                //if (entry.SourcePageType.Name == "recent")
                //{
                    
                //    recentFile = (StorageFile)e.Parameter;
                //    OpenFileDialog_Click(null, null); 
                //    //await appWindow.CloseAsync();
                //}
            }
            catch (NullReferenceException)
            {

            }

            
            string textFile="";
            if (e != null)
            {
                try
                {
                    this.shareOperation = (ShareOperation)e.Parameter;
                }
                catch (InvalidCastException)
                {

                }
            }
            
            if (this.shareOperation != null)
            {
                IEnumerable<IStorageItem> file = await shareOperation.Data.GetStorageItemsAsync();
                
                foreach(var item in file)
                {
                    StorageFile f = (StorageFile)item;
                    switch (f.FileType)
                    {
                        case ".pdf": 
                            SaveFilePdf.openFile = f;
                            openFile = f;
                            textFile = await SaveFilePdf.Read(openFile).ConfigureAwait(true);
                            CurrentFormatFile ="PDF";
                            break;
                        case ".doc": 
                            SaveFileDoc.openFile = f;
                            openFile = f;
                            textFile = await SaveFileDoc.OpenFileWord(openFile).ConfigureAwait(true);
                            CurrentFormatFile = "DOC";
                            break;
                        case ".docx": 
                            SaveFileDocX.openFile = f;
                            openFile = f;
                            textFile = await SaveFileDocX.OpenFileWord(openFile).ConfigureAwait(true);
                            CurrentFormatFile = "DOCX";
                            break;
                    }
                }
                dictationTextBox.Text = textFile;
            }

            if (PrintManager.IsSupported())
            {
                
            }
            else
            {
                //InvokePrintingButton.Visibility = Visibility.Collapsed;
            }
            printHelper = new PrintHelper(this);
        }
                    
        
        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            appWindow = await AppWindow.TryCreateAsync();
            Frame appWindowContentFrame = new Frame();
            appWindowContentFrame.Navigate(typeof(saveAs), dictationTextBox.Text);
            ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);
            appWindow.RequestSize(new Size(1000, 400));
            await appWindow.TryShowAsync();
            //this.mainPage.IsEnabled = false;

            appWindow.Closed += delegate
            {
                appWindowContentFrame.Content = null;
                appWindow = null;
                this.mainPage.IsEnabled = true;
            };
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Exit();
        }
        
        private async void cbLanguageSelection_SelectionChanged()
        {
            Language newLanguage = SpeechRecognizer.SystemSpeechLanguage;
            if (tmp != null)
            {
                newLanguage = tmp;
            }
            if (RecognizerViewModel.RecognizerSpeech.SpeechRecognizer != null)
            {
                if (newLanguage != null)
                {
                    if (RecognizerViewModel.RecognizerSpeech.SpeechRecognizer.CurrentLanguage != newLanguage)
                    {
                        try
                        {
                            RecognizerViewModel.RecognizerSpeech = null;
                            RecognizerViewModel.RecognizerSpeech = new RecognizerSpeech(dispatcher, newLanguage);
                            languageTag = newLanguage.LanguageTag;
                        }
                        catch (ArgumentException exception)
                        {
                            var messageDialog = new MessageDialog(exception.Message, "Exception");
                            await messageDialog.ShowAsync();
                        }
                    }
                }
            }
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            cbLanguageSelection_SelectionChanged();
            if (dictationTextBox.Text.Length != 0)
            {
                RecognizerViewModel.RecognizerSpeech.dictatedTextBuilder.Append(dictationTextBox.Text);
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
                        
        private async void OpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".doc");
            openPicker.FileTypeFilter.Add(".docx");
            openPicker.FileTypeFilter.Add(".pdf");
            try
            {
                if (recentFile != null) 
                {
                    openFile = recentFile;
                    Open_openFile();
                }
                else
                {
                    //StorageFile inputStorageFile = await openPicker.PickSingleFileAsync();
                    //openFile = inputStorageFile;
                }

            }
            catch (NullReferenceException)
            {
                var message = new MessageDialog("No file selected or file is damaged");
                await message.ShowAsync();
            }
        }

        private async void Open_openFile()
        {
            dictationTextBox.SelectionChanging += dictationTextBox_SelectionChanging;
            switch (openFile.FileType)
            {
                case ".pdf":
                    dictationTextBox.Text = await SaveFilePdf.Read(openFile).ConfigureAwait(true);
                    CurrentFormatFile = "PDF";
                    break;

                case ".docx":
                    dictationTextBox.Text = await SaveFileDocX.OpenFileWord(openFile).ConfigureAwait(true);
                    CurrentFormatFile = "DOCX";
                    break;

                case ".doc":
                    dictationTextBox.Text = await SaveFileDoc.OpenFileWord(openFile).ConfigureAwait(true);
                    CurrentFormatFile = "DOC";
                    break;
            }
        }

        private async void SaveChangesCurrentFile_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFormatFile.Length != 0)
            {
                switch (CurrentFormatFile)
                {
                    case "PDF":
                        SaveFilePdf.SaveChangesFile(dictationTextBox.Text);
                        //SaveFilePdf.openFile = null;
                        break;

                    case "DOCX":
                        SaveFileDocX.SaveChangesFile(dictationTextBox.Text);
                        //SaveFileDocX.openFile = null;
                        break;

                    case "DOC":
                        SaveFileDoc.SaveChangesFile(dictationTextBox.Text);
                        //SaveFileDoc.openFile = null;
                        break;
                }
                RecognizerViewModel.RecognizerSpeech.dictatedTextBuilder.Clear();
                StorageFolder localFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
                StorageFile stFile = await localFolder.GetFileAsync("temp.txt");
                await stFile.DeleteAsync();
            }
            else
            {
                SaveAs_Click(sender, e);
            }
        }

        private  void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void Settings_Click(object sender, RoutedEventArgs e)
        {
            appWindow = await AppWindow.TryCreateAsync();
            Frame appWindowContentFrame = new Frame();
            appWindowContentFrame.Navigate(typeof(settings), RecognizerViewModel);
            ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);
            appWindow.RequestSize(new Size(500, 900));
            await appWindow.TryShowAsync();
            this.mainPage.IsEnabled = false;

            appWindow.Closed += delegate
            {
                appWindowContentFrame.Content = null;
                appWindow = null;
                this.mainPage.IsEnabled = true;
                
            };

            //CoreApplicationView newView = CoreApplication.CreateNewView();
            //int newViewId = 0;
            //await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
            //    Frame frame = new Frame();
            //    frame.Navigate(typeof(settings), RecognizerViewModel);
            //    Window.Current.Content = frame;
            //    Window.Current.Activate();
            //    newViewId = ApplicationView.GetForCurrentView().Id;
            //});
            //bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }
        static public AppWindow appWindow;
        private async void Recent_Click(object sender, RoutedEventArgs e)
        {
            List<StorageFile> files = new List<StorageFile>();
            var mru = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList;
            
            //StorageFile retrievedFile = await mru.GetFileAsync(token);

            foreach (Windows.Storage.AccessCache.AccessListEntry entry in mru.Entries)
            {
                string mruToken = entry.Token;
                string mruMetadata = entry.Metadata;
                try
                {
                    Windows.Storage.IStorageItem item = await mru.GetItemAsync(mruToken);
                    if (item is StorageFile)
                    {
                        //StorageFile item = await mru.GetFileAsync(mruToken);
                        files.Add((StorageFile)item);
                    }
                }
                catch (FileNotFoundException)
                {}
            }

            appWindow = await AppWindow.TryCreateAsync();
            Frame appWindowContentFrame = new Frame();
            appWindowContentFrame.Navigate(typeof(recent),files);
            ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);
            appWindow.RequestSize(new Size(500, 900));
            await appWindow.TryShowAsync();
            this.mainPage.IsEnabled = false;
            
            appWindow.Closed += delegate
            {
                //Task.Delay(1000);
                dictationTextBox.Text = "";
                appWindowContentFrame.Content = null;
                appWindow = null;
                this.mainPage.IsEnabled = true;
                recentFile = recent.openFile;
                recent.openFile = null;
                OpenFileDialog_Click(null, null);
            };
        }
                
        private async void Start_Click()
        {
            string text = dictationTextBox.Text;
            int length = dictationTextBox.Text.Length;
            int position=0;
            if (length >= 2000)
            {
                while (position <= length)
                {
                    string partStr;
                    int delta = length - position;
                    if (delta < 2000)
                    {
                        partStr = text.Substring(position, delta);
                    }
                    else
                    {
                        partStr = text.Substring(position, 2000);
                    }
                    ApplicationData.Current.RoamingSettings.Values["dictationText" + position.ToString()] = partStr;
                    position += 2000;
                }
            }
            else
            {
                ApplicationData.Current.RoamingSettings.Values["dictationText"] = text;
            }
            var taskList = BackgroundTaskRegistration.AllTasks.Values;
            var task = taskList.FirstOrDefault(i => i.Name == taskName);
            if (task == null)
            {
                var taskBuilder = new BackgroundTaskBuilder();
                taskBuilder.Name = taskName;
                taskBuilder.TaskEntryPoint = typeof(BackgroundTask.AutoSaveFile).ToString();

                ApplicationTrigger appTrigger = new ApplicationTrigger();
                taskBuilder.SetTrigger(appTrigger);

                task = taskBuilder.Register();

                //task.Progress += Task_Progress;
                task.Completed += Task_Completed;

                await appTrigger.RequestAsync();

            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            var result = ApplicationData.Current.LocalSettings.Values["factorial"];
            var progress = $"Результат: {result}";
            UpdateUI(progress);
            Stop();
        }

        private void Task_Progress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            var progress = $"Progress: {args.Progress} %";
            UpdateUI(progress);
        }

        private async void UpdateUI(string progress)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                //outputBlock.Text = progress;
            });
        }

        private async void Stop()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                var taskList = BackgroundTaskRegistration.AllTasks.Values;
                var task = taskList.FirstOrDefault(i => i.Name == taskName);
                if (task != null)
                {
                    task.Unregister(true);
                }
            });
        }


        private void dictationTextBox_SelectionChanging(TextBox sender, TextBoxSelectionChangingEventArgs args)
        {
            Start_Click();
        }
    }
}
