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
            }
            await Windows.System.Launcher.LaunchFileAsync(stFile);
        }

        public static async Task<string> OpenFileWord()
        {
            if (openFile == null)
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
                openPicker.FileTypeFilter.Add(".doc");
                StorageFile inputStorageFile = await openPicker.PickSingleFileAsync();
                openFile = inputStorageFile;
            }
            WordDocument document = new WordDocument();
            await document.OpenAsync(openFile).ConfigureAwait(true);
            string docText = document.GetText();
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
