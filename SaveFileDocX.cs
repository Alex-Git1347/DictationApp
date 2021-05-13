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
    class SaveFileDocX
    {
        public static StorageFile openFile;
        public static async void SaveWord(MemoryStream streams, string filename)
        {
            streams.Position = 0;
            StorageFile stFile;
            if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")))
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.DefaultFileExtension = ".docx";
                savePicker.SuggestedFileName = filename;
                savePicker.FileTypeChoices.Add("Word Documents", new List<string>() { ".docx" });
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
                string mruToken = mru.Add(stFile, "Docx file");
            }
            await Windows.System.Launcher.LaunchFileAsync(stFile);
        }

        public static async Task<string> OpenFileWord(StorageFile openFile)
        {
            //if (openFile == null)
            //{
            //    FileOpenPicker openPicker = new FileOpenPicker();
            //    openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            //    openPicker.FileTypeFilter.Add(".docx");
            //    StorageFile inputStorageFile = await openPicker.PickSingleFileAsync();
            //    openFile = inputStorageFile;
            //}
            SaveFileDocX.openFile = openFile;
            WordDocument document = new WordDocument();
            await document.OpenAsync(openFile).ConfigureAwait(true);
            string docText = document.GetText();


            //Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

            var mru = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList;
            string mruToken = mru.Add(openFile, "profile pic");
            document.Dispose();
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
