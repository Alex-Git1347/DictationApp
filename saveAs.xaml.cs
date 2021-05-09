using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.Pdf.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
        public saveAs()
        {
            this.InitializeComponent();
        }
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            textFile = e.Parameter.ToString();
            //if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            //{
            //    greeting.Text = $"Hi, {e.Parameter.ToString()}";
            //}
            //else
            //{
            //    greeting.Text = "Hi!";
            //}
            base.OnNavigatedTo(e);
        }

        private async void SaveDOC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WordDocument doc = new WordDocument();
                doc.EnsureMinimal();
                doc.LastParagraph.AppendText(textFile);
                MemoryStream stream = new MemoryStream();
                await doc.SaveAsync(stream, FormatType.Doc).ConfigureAwait(true);
                doc.Close();
                SaveFileDoc.SaveWord(stream, "Result.doc");
                stream.Dispose();
            }
            catch (ArgumentNullException)
            {

            }
        }

        private async void SaveDOCX_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WordDocument docx = new WordDocument();
                docx.EnsureMinimal();
                docx.LastParagraph.AppendText(textFile);
                MemoryStream streamDocx = new MemoryStream();
                await docx.SaveAsync(streamDocx, FormatType.Docx).ConfigureAwait(true);
                docx.Close();
                SaveFileDocX.SaveWord(streamDocx, "Result.docx");
                streamDocx.Dispose();
            }
            catch (ArgumentNullException)
            {

            }
        }

        private void SavePDF_Click(object sender, RoutedEventArgs e)
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
                    PDFdocument.Save(ms);
                    PDFdocument.Close(true);
                    SaveFilePdf.Save(ms, "New PDF file.pdf");
                    ms.Dispose();
                }
            }
            catch(ArgumentNullException)
            {

            }
        }

    }
}
