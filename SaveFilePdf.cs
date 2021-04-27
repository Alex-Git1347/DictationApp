﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Pdf.Graphics;
using System.Drawing;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.Foundation.Metadata;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf;

namespace Dictation
{
    class SaveFilePdf
    {

        static StorageFile openFile;
        
        public static async void Save(Stream stream, string filename)
        {
            stream.Position = 0;

            StorageFile stFile;
            if (!(ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")))
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.DefaultFileExtension = ".pdf";
                savePicker.SuggestedFileName = "New PDF file";
                savePicker.FileTypeChoices.Add("Adobe PDF Document", new List<string>() { ".pdf" });
                stFile = await savePicker.PickSaveFileAsync();
            }
            else
            {
                StorageFolder local = ApplicationData.Current.LocalFolder;
                stFile = await local.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            }
            if (stFile != null)
            {
                Windows.Storage.Streams.IRandomAccessStream fileStream = await stFile.OpenAsync(FileAccessMode.ReadWrite);
                Stream st = fileStream.AsStreamForWrite();
                st.SetLength(0);
                st.Write((stream as MemoryStream).ToArray(), 0, (int)stream.Length);
                st.Flush();
                st.Dispose();
                fileStream.Dispose();
                MessageDialog msgDialog = new MessageDialog("Do you want to view the Document?", "File created.");
                UICommand yesCmd = new UICommand("Yes");
                msgDialog.Commands.Add(yesCmd);
                UICommand noCmd = new UICommand("No");
                msgDialog.Commands.Add(noCmd);
                IUICommand cmd = await msgDialog.ShowAsync();
                if (cmd == yesCmd)
                {
                    bool success = await Windows.System.Launcher.LaunchFileAsync(stFile);
                }
            }
        }

        public static async void SaveChangesFile(string text)
        {
            PdfLoadedDocument loadedDocument = new PdfLoadedDocument();
            //Loads or opens an existing PDF document through Open method of PdfLoadedDocument class
            await loadedDocument.OpenAsync(openFile);
            loadedDocument.Pages.RemoveAt(0);
            Syncfusion.Pdf.PdfPage page = (Syncfusion.Pdf.PdfPage)loadedDocument.Pages.Add();
            PdfGraphics graphics = page.Graphics;

            Syncfusion.Pdf.Graphics.PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);

            graphics.DrawString(text, font, PdfBrushes.Black, new PointF(0, 0));

            //To-Do some manipulation
            //To-Do some manipulation  
            //Resave the document to the same file
            await loadedDocument.Save();
        }

        public static async Task<string> Read()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".pdf");
            StorageFile file = await picker.PickSingleFileAsync();
            openFile = file;
            PdfLoadedDocument loadedDocument = new PdfLoadedDocument();
            await loadedDocument.OpenAsync(file);
            PdfPageBase page = loadedDocument.Pages[0];
            string extractedText = page.ExtractText();
            loadedDocument.Close(true);
            return extractedText;
        }

        public static string ReadPdfFile(string fileFullName)
        {
            StringBuilder text = new StringBuilder();

            if (File.Exists(fileFullName))
            {
                PdfReader pdfReader = new PdfReader(fileFullName);

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
    }
}
