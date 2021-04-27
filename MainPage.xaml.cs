using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Syncfusion.Pdf;
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
using System.Drawing;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Windows.Storage.Streams;

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
        string CurrentFormatFile="";
        
        
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
                    }
                    break;

                case "DOC":
                    WordDocument doc = new WordDocument();
                    doc.EnsureMinimal();
                    doc.LastParagraph.AppendText(dictationTextBox.Text);
                    MemoryStream stream = new MemoryStream();
                    await doc.SaveAsync(stream, FormatType.Doc);
                    doc.Close();
                    SaveFileDoc.SaveWord(stream, "Result.doc");
                    break;

                case "DOCX":
                    WordDocument docx = new WordDocument();
                    docx.EnsureMinimal();
                    docx.LastParagraph.AppendText(dictationTextBox.Text);
                    MemoryStream streamDocx = new MemoryStream();
                    await docx.SaveAsync(streamDocx, FormatType.Docx);
                    docx.Close();
                    SaveFileDocX.SaveWord(streamDocx, "Result.docx");
                    break;
            }
        }
               
        public string ReadPdfFile(string fileName)
        {
            StringBuilder text = new StringBuilder();

            if (File.Exists(fileName))
            {
                PdfReader pdfReader = new PdfReader(fileName);

                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                }
                pdfReader.Close();
            }
            return text.ToString();
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem comboBoxItem = ((ComboBoxItem)FormatSelection.SelectedItem);
            switch (comboBoxItem.Content.ToString())
            {
                case "PDF":
                    dictationTextBox.Text += await SaveFilePdf.Read();
                    CurrentFormatFile = "PDF";
                    break;

                case "DOCX":
                    dictationTextBox.Text = await SaveFileDocX.OpenFileWord().ConfigureAwait(true);
                    CurrentFormatFile = "DOCX";
                    break;

                case "DOC":
                    dictationTextBox.Text = await SaveFileDoc.OpenFileWord().ConfigureAwait(true);
                    CurrentFormatFile = "DOC";
                    break;
            }
        }     

        private void SaveChangesCurrentFile_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentFormatFile)
            {
                case "PDF": SaveFilePdf.SaveChangesFile(dictationTextBox.Text);
                    break;

                case "DOCX": SaveFileDocX.SaveChangesFile(dictationTextBox.Text);
                    break;

                case "DOC": SaveFileDoc.SaveChangesFile(dictationTextBox.Text);
                    break;
            }
        }
    }
}
