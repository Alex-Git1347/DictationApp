using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Popups;

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

    public class RecentFilesViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<RecentFile> recentFiles = new ObservableCollection<RecentFile>();
        public ObservableCollection<RecentFile> ResentFiles { get { return this.recentFiles; } }
        RecognizerSpeechViewModel recognizerSpeechView;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

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
        
        public ICommand Test
        {
            get
            {
                return new RelayCommand(async (obj) =>
                {
                    var messageDialog = new MessageDialog( "hello yes");
                    await messageDialog.ShowAsync();
                });
            }
        }
        private void method()
        {
            int a = 5;
            //recognizerSpeechView = selected///
        }
    }
}
