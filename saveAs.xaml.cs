using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.Pdf.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
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
    public sealed partial class saveAs : Page
    {
        string textFile="";
        string formatFile;
        StorageFolder folder = null;
        AppWindow appWindow;
        public saveAs()
        {
            this.InitializeComponent();
            appWindow = MainPage.appWindow;
            
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            //Window.Current.SetTitleBar(null);
            //ApplicationView view = ApplicationView.GetForCurrentView();
            //view.TryEnterFullScreenMode();
            textFile = e.Parameter.ToString();
            base.OnNavigatedTo(e);
            //var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            //coreTitleBar.ExtendViewIntoTitleBar = true;
        }
        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (formatFile)
                {
                    case "doc":
                        SaveDOC_Click();
                        break;
                    case "docx":
                        SaveDOCX_Click();
                        break;
                    case "pdf":
                        SavePDF_Click();
                        break;
                }
                await appWindow.CloseAsync();
            }
            catch (Exception)
            {

            }

        }
        private async void SaveDOC_Click()
        {
            try
            {
                WordDocument doc = new WordDocument();
                doc.EnsureMinimal();
                doc.LastParagraph.AppendText(textFile);
                MemoryStream stream = new MemoryStream();
                await doc.SaveAsync(stream, FormatType.Doc).ConfigureAwait(true);
                doc.Close();
                SaveFileDoc.SaveWord(stream, nameFile.Text, pathSaveFile.Text);
                stream.Dispose();
            }
            catch (ArgumentNullException)
            {

            }
        }

        private async void SaveDOCX_Click()
        {
            try
            {
                WordDocument docx = new WordDocument();
                docx.EnsureMinimal();
                docx.LastParagraph.AppendText(textFile);
                MemoryStream streamDocx = new MemoryStream();
                await docx.SaveAsync(streamDocx, FormatType.Docx).ConfigureAwait(true);
                docx.Close();
                //SaveFileDocX.SaveWord(streamDocx, "Result.docx");
                SaveFileDocX.SaveWord(streamDocx, nameFile.Text, pathSaveFile.Text);
                streamDocx.Dispose();
            }
            catch (ArgumentNullException)
            {

            }
        }

        private async void SavePDF_Click()
        {
            try
            {
                using (Syncfusion.Pdf.PdfDocument PDFdocument = new Syncfusion.Pdf.PdfDocument())
                {
                    Syncfusion.Pdf.PdfPage page = PDFdocument.Pages.Add();
                    PdfGraphics graphics = page.Graphics;
                    Syncfusion.Pdf.Graphics.PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);
                    graphics.DrawString(textFile, font, PdfBrushes.Black, new System.Drawing.PointF(0, 0));
                    MemoryStream ms = new MemoryStream();
                    await PDFdocument.SaveAsync(ms).ConfigureAwait(true);
                    //SaveFilePdf.Save(ms, "New PDF file.pdf");
                    SaveFilePdf.Save(ms, nameFile.Text, pathSaveFile.Text);
                    PDFdocument.Save(ms);
                    PDFdocument.Close(true);
                    ms.Dispose();
                }
            }
            catch(ArgumentNullException)
            {

            }
        }

        private void cbFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)(cbFormat.SelectedItem);
            formatFile = (item.Tag.ToString());
        }               

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add(".docx");
            folderPicker.FileTypeFilter.Add(".xlsx");
            folderPicker.FileTypeFilter.Add(".pptx");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                pathSaveFile.Text = folder.Path.ToString();
            }
        }
    }
}
