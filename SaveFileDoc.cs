using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace Dictation
{
    class SaveFileDoc
    {
        public static StorageFile openFile;
        public static async void SaveWord(MemoryStream streams, string fileName, string fileFolder)
        {
            streams.Position = 0;
            StorageFile stFile=null;
            if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")))
            {
                try
                {
                    FileSavePicker savePicker = new FileSavePicker();
                    savePicker.DefaultFileExtension = ".doc";
                    savePicker.SuggestedFileName = fileName;
                    savePicker.FileTypeChoices.Add("Word Documents", new List<string>() { ".doc" });
                    //stFile = await savePicker.PickSaveFileAsync();
                    //StorageFolder f = await StorageFolder.GetFolderFromPathAsync("");
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(fileFolder);
                    stFile = await folder.CreateFileAsync(fileName + ".doc");
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
                var mru = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList;
                string mruToken = mru.Add(stFile, "Doc file");
                //await Windows.System.Launcher.LaunchFileAsync(stFile);
            }
        }

        public static async Task<string> OpenFileWord(StorageFile openFile)
        {
            SaveFileDoc.openFile = openFile;
            WordDocument document = new WordDocument();
            await document.OpenAsync(openFile).ConfigureAwait(true);
            string docText = document.GetText();
            document.Dispose();
            var mru = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList;
            string mruToken = mru.Add(openFile, "Doc file");
            return docText;

        }

        public static async void SaveChangesFile(string text)
        {
            WordDocument document = new WordDocument();
            await document.OpenAsync(openFile).ConfigureAwait(true);
            document.TextBoxes.Clear();
            document.EnsureMinimal();
            document.LastParagraph.AppendText(text);
            await document.SaveAsync(openFile, FormatType.Docx).ConfigureAwait(true);
            document.Dispose();
        }
    }
}
