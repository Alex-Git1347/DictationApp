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
        public static async void SaveWord(MemoryStream streams, string fileName, string fileFolder)
        {
            streams.Position = 0;
            StorageFile stFile = null;
            if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")))
            {
                try
                {
                    FileSavePicker savePicker = new FileSavePicker();
                    savePicker.DefaultFileExtension = ".docx";
                    savePicker.SuggestedFileName = fileName;
                    savePicker.FileTypeChoices.Add("Word Documents", new List<string>() { ".docx" });
                    //stFile = await savePicker.PickSaveFileAsync();
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(fileFolder);
                    stFile = await folder.CreateFileAsync(fileName + ".docx");
                }
                catch (NullReferenceException)
                { }
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
                string mruToken = mru.Add(stFile, "Docx file");
                //await Windows.System.Launcher.LaunchFileAsync(stFile);
            }
        }

        public static async Task<string> OpenFileWord(StorageFile openFile)
        {
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
