using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BackgroundTask
{
    public sealed class AutoSaveFile : IBackgroundTask
    {
        volatile bool _cancelRequested = false;
        BackgroundTaskDeferral _deferral;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var cost = BackgroundWorkCost.CurrentBackgroundWorkCost;
            if (cost == BackgroundWorkCostValue.High)
                return;

            var cancel = new CancellationTokenSource();
            taskInstance.Canceled += (s, e) =>
            {
                cancel.Cancel();
                cancel.Dispose();
                _cancelRequested = true;
            };

            _deferral = taskInstance.GetDeferral();

            await DoWork(taskInstance);

            _deferral.Complete();
        }

        private async Task DoWork(IBackgroundTaskInstance taskInstance)
        {
            var settings = ApplicationData.Current.RoamingSettings;
            
            string text="";
            foreach (var item in ApplicationData.Current.RoamingSettings.Values)
            {
                text += item.ToString();

            }
            string typeFile = (string)settings.Values["typeFile"];
            ApplicationData.Current.RoamingSettings.Values.Clear();
            StorageFile stFile;
            StorageFolder localFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            stFile = await localFolder.CreateFileAsync("temp.txt", CreationCollisionOption.ReplaceExisting);
            
            if (stFile != null)
            {
                using (IRandomAccessStream zipStream = await stFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (Stream outstream = zipStream.AsStreamForWrite())
                    {
                        byte[] buffer = System.Text.Encoding.Default.GetBytes(text);
                        outstream.Write(buffer, 0, buffer.Length);
                        outstream.Flush();
                    }
                }
            }
        }
    }
}
