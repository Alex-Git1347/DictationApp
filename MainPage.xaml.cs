using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Syncfusion.Pdf.Graphics;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Background;
using System.Linq;
using Windows.Storage.Streams;

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

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            RecognizerViewModel = new RecognizerSpeechViewModel(dispatcher);
            PopulateLanguageDropdown();
            PopulateLanguageInterfaceDropdown();
            HaveTempFile();
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
                ContentDialog noWifiDialog = new ContentDialog
                {
                    //Title = "No wifi connection",
                    Content = "Хотите востановить несохраненный документ?",
                    CloseButtonText = "No",
                    PrimaryButtonText = "Yes"
                };

                ContentDialogResult result = await noWifiDialog.ShowAsync();

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
                //var messageDialog = new MessageDialog("Хотите востановить файл?");
                //await messageDialog.ShowAsync();
            }
        }


        private void Toggle_Click()
        {
            //Splitter.IsPaneOpen = !Splitter.IsPaneOpen;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string textFile="";
            if (e != null)
            {
                try
                {
                    this.shareOperation = (ShareOperation)e.Parameter;
                }
                catch (InvalidCastException)
                {
                    //MessageDialog message = new MessageDialog(arg.ToString());
                    //await message.ShowAsync();
                }
            }
            
            if (this.shareOperation != null)
            {
                IEnumerable<IStorageItem> file = await shareOperation.Data.GetStorageItemsAsync();
                
                foreach(var item in file)
                {
                    StorageFile f = (StorageFile)item;
                    foreach (ComboBoxItem itemBox in FormatSelection.Items)
                    {
                        if (itemBox.Tag.ToString() == f.FileType)
                        {
                            FormatSelection.SelectedItem = itemBox;
                        }
                    }

                    switch (f.FileType)
                    {
                        case ".pdf": 
                            SaveFilePdf.openFile = f;
                            textFile = await SaveFilePdf.Read(openFile).ConfigureAwait(true);
                            CurrentFormatFile ="PDF";
                            break;
                        case ".doc": 
                            SaveFileDoc.openFile = f;
                            textFile = await SaveFileDoc.OpenFileWord(openFile).ConfigureAwait(true);
                            CurrentFormatFile = "DOC";
                            break;
                        case ".docx": 
                            SaveFileDocX.openFile = f;
                            textFile = await SaveFileDocX.OpenFileWord(openFile).ConfigureAwait(true);
                            CurrentFormatFile = "DOCX";
                            break;
                    }
                }
                dictationTextBox.Text = textFile;
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
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(saveAs),dictationTextBox.Text);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Exit();
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
                    catch (ArgumentException exception)
                    {
                        var messageDialog = new MessageDialog(exception.Message, "Exception");
                        await messageDialog.ShowAsync();
                    }
                }
            }
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            if(dictationTextBox.Text.Length != 0)
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
                    catch (ArgumentException exception)
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
                
        private async void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem comboBoxItem = ((ComboBoxItem)FormatSelection.SelectedItem);
            switch(comboBoxItem.Content.ToString())
            {
                case "PDF":
                    using (Syncfusion.Pdf.PdfDocument PDFdocument = new Syncfusion.Pdf.PdfDocument())
                    {
                        Syncfusion.Pdf.PdfPage page = PDFdocument.Pages.Add();
                        PdfGraphics graphics = page.Graphics;
                        Syncfusion.Pdf.Graphics.PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);
                        graphics.DrawString(dictationTextBox.Text, font, PdfBrushes.Black, new System.Drawing.PointF(0, 0));
                        MemoryStream ms = new MemoryStream();
                        PDFdocument.Save(ms);
                        PDFdocument.Close(true);
                        SaveFilePdf.Save(ms, "New PDF file.pdf");
                        ms.Dispose();
                    }
                    break;

                case "DOC":
                    WordDocument doc = new WordDocument();
                    doc.EnsureMinimal();
                    doc.LastParagraph.AppendText(dictationTextBox.Text);
                    MemoryStream stream = new MemoryStream();
                    await doc.SaveAsync(stream, FormatType.Doc).ConfigureAwait(true);
                    doc.Close();
                    SaveFileDoc.SaveWord(stream, "Result.doc");
                    stream.Dispose();
                    break;

                case "DOCX":
                    WordDocument docx = new WordDocument();
                    docx.EnsureMinimal();
                    docx.LastParagraph.AppendText(dictationTextBox.Text);
                    MemoryStream streamDocx = new MemoryStream();
                    await docx.SaveAsync(streamDocx, FormatType.Docx).ConfigureAwait(true);
                    docx.Close();
                    SaveFileDocX.SaveWord(streamDocx, "Result.docx");
                    streamDocx.Dispose();
                    break;
            }
        }
               
        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".doc");
            openPicker.FileTypeFilter.Add(".docx");
            openPicker.FileTypeFilter.Add(".pdf");
            StorageFile inputStorageFile = await openPicker.PickSingleFileAsync();
            openFile = inputStorageFile;
            try
            {
                openFile = inputStorageFile;
                dictationTextBox.SelectionChanging += dictationTextBox_SelectionChanging;
                switch (openFile.FileType)
                {
                    case ".pdf":
                        dictationTextBox.Text += await SaveFilePdf.Read(openFile).ConfigureAwait(true);
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
            catch(NullReferenceException)
            {
                var message = new MessageDialog("No file selected or file is damaged");
                await message.ShowAsync();
            }
        }

        private async void SaveChangesCurrentFile_Click(object sender, RoutedEventArgs e)
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
            //dictationTextBox.Text = "";
            StorageFile stFile;
            StorageFolder localFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            stFile = await localFolder.GetFileAsync("temp");
            await stFile.DeleteAsync();
            //stFile = await localFolder.CreateFileAsync("temp.txt", CreationCollisionOption.ReplaceExisting);
        }
        public string hello = "hello";
        private async void Recent_Click(object sender, RoutedEventArgs e)
        {
            List<StorageFile> files = new List<StorageFile>();
            var mru = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList;
            
            //StorageFile retrievedFile = await mru.GetFileAsync(token);

            foreach (Windows.Storage.AccessCache.AccessListEntry entry in mru.Entries)
            {
                string mruToken = entry.Token;
                string mruMetadata = entry.Metadata;
                Windows.Storage.IStorageItem item = await mru.GetItemAsync(mruToken);
                if (item is StorageFile)
                {
                    //StorageFile item = await mru.GetFileAsync(mruToken);
                    files.Add((StorageFile)item);
                }
            }

            this.Frame.Navigate(typeof(recent), files);
        }

        string taskName = "AutoSaveFile";

        
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
                taskBuilder.TaskEntryPoint = typeof(BackgroundTasks.AutoSaveFile).ToString();

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

                    //stopButton.IsEnabled = false;
                    //startButton.IsEnabled = true;
                }
            });
        }


        private void dictationTextBox_SelectionChanging(TextBox sender, TextBoxSelectionChangingEventArgs args)
        {
            Start_Click();
        }
    }
}
