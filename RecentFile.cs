using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Dictation
{
    public class RecentFile
    {
        public RecentFile()
        {
            
        }
        public string Name { get; set; }
        public StorageFile thisFile { get; set; }
    }

    public class RecentFilesViewModel
    {
        private ObservableCollection<RecentFile> recentFiles = new ObservableCollection<RecentFile>();
        public ObservableCollection<RecentFile> ResentFiles { get { return this.recentFiles; } }
        RecognizerSpeechViewModel recognizerSpeechView;

        public RecentFilesViewModel(RecognizerSpeechViewModel recognizerSpeechViewModel)
        {
            recognizerSpeechView = recognizerSpeechViewModel;
            PopulateRecentFiles();
        }

        private async void PopulateRecentFiles()
        {
            var mru = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList;
            foreach (Windows.Storage.AccessCache.AccessListEntry entry in mru.Entries)
            {
                string mruToken = entry.Token;
                try
                {
                    Windows.Storage.IStorageItem item = await mru.GetItemAsync(mruToken);
                    if (item is StorageFile)
                    {
                        this.recentFiles.Add(new RecentFile() { thisFile = (StorageFile)item, Name = item.Name });
                    }
                }
                catch (FileNotFoundException)
                { }
            }
        }
        
        private void method()
        {
            int a = 5;
            //recognizerSpeechView = selected///
        }
    }
}
