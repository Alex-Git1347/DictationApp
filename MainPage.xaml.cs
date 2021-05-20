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

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            RecognizerViewModel = new RecognizerSpeechViewModel(dispatcher);
            HaveTempFile();
        }

        private async void OnPrintButtonClick(object sender, RoutedEventArgs e)
        {
            string tempText = dictationTextBox.Text;
            try
            {
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
                var entry = this.Frame.BackStack.LastOrDefault();
                if (entry.SourcePageType.Name == "recent")
                {
                    recentFile = (StorageFile)e.Parameter;
                    OpenFileDialog_Click(null, null);
                }
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
                InvokePrintingButton.Visibility = Visibility.Collapsed;
            }
            printHelper = new PrintHelper(this);
            printHelper.RegisterForPrinting();
        }
                    
        
        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(saveAs),dictationTextBox.Text);
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
                }
                else
                {
                    StorageFile inputStorageFile = await openPicker.PickSingleFileAsync();
                    openFile = inputStorageFile;
                }

                Open_openFile();
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

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(settings), RecognizerViewModel);
        }
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

            this.Frame.Navigate(typeof(recent), files);
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
