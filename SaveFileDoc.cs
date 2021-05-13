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

namespace Dictation
{
    class SaveFileDoc
    {
        public static StorageFile openFile;
        public static async void SaveWord(MemoryStream streams, string filename)
        {
            streams.Position = 0;
            StorageFile stFile;
            if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")))
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.DefaultFileExtension = ".doc";
                savePicker.SuggestedFileName = filename;
                savePicker.FileTypeChoices.Add("Word Documents", new List<string>() { ".doc" });
                stFile = await savePicker.PickSaveFileAsync();
            }
            else
            {
                StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
                stFile = await local.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
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
            }
            await Windows.System.Launcher.LaunchFileAsync(stFile);
        }

        public static async Task<string> OpenFileWord(StorageFile openFile)
        {
            //if (openFile == null)
            //{
            //    FileOpenPicker openPicker = new FileOpenPicker();
            //    openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            //    openPicker.FileTypeFilter.Add(".doc");
            //    StorageFile inputStorageFile = await openPicker.PickSingleFileAsync();
            //    openFile = inputStorageFile;
            //}
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
