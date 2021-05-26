using System;
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
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf;
using Windows.ApplicationModel.Activation;
using Windows.Storage.Streams;

namespace Dictation
{
    class SaveFilePdf
    {
        public static StorageFile openFile;

        public static async void Save(MemoryStream streams, string fileName,string fileFolder)
        {
            streams.Position = 0;
            StorageFile stFile = null;
            if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")))
            {
                try
                {
                    FileSavePicker savePicker = new FileSavePicker();
                    savePicker.DefaultFileExtension = ".pdf";
                    savePicker.SuggestedFileName = fileName;
                    savePicker.FileTypeChoices.Add("PDF Documents", new List<string>() { ".pdf" });
                    //stFile = await savePicker.PickSaveFileAsync();
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(fileFolder);
                    stFile = await folder.CreateFileAsync(fileName+".pdf");
                }
                catch (NullReferenceException)
                {}
            }
            else
            {
                StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
                stFile = await local.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            }
            if (stFile != null)
            {
                using (IRandomAccessStream zipStream = await stFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (Stream outstream = zipStream.AsStreamForWrite())
                    {
                        byte[] buffer = streams.ToArray();
                        outstream.Write(buffer, 0, buffer.Length);
                        outstream.Flush();
                    }
                }
                //await Windows.System.Launcher.LaunchFileAsync(stFile);
            }
        }
                
        public static async void SaveChangesFile(string text)
        {
            PdfLoadedDocument loadedDocument = new PdfLoadedDocument();
            await loadedDocument.OpenAsync(openFile).ConfigureAwait(true);
            loadedDocument.Pages.RemoveAt(0);
            Syncfusion.Pdf.PdfPage page = (Syncfusion.Pdf.PdfPage)loadedDocument.Pages.Add();
            PdfGraphics graphics = page.Graphics;

            Syncfusion.Pdf.Graphics.PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);

            graphics.DrawString(text, font, PdfBrushes.Black, new PointF(0, 0));
            await loadedDocument.Save().ConfigureAwait(true);
            loadedDocument.Dispose();
        }

        public static async Task<string> Read(StorageFile openFile)
        {
            SaveFilePdf.openFile = openFile;
            string extractedText = "";
            if (openFile != null)
            {
            PdfLoadedDocument loadedDocument = new PdfLoadedDocument();
            await loadedDocument.OpenAsync(openFile).ConfigureAwait(true);
            PdfPageBase page = loadedDocument.Pages[0];
            extractedText = page.ExtractText();
            loadedDocument.Close(true);
            loadedDocument.Dispose();
            var mru = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList;
            string mruToken = mru.Add(openFile, "Pdf file");
            }
            return extractedText;
        }

    }
}
