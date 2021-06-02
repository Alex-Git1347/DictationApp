using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.Foundation;
using Windows.UI.Core;

namespace Dictation
{
    public class MainViewModel
    {
        public AppWindow appWindow;
        private RecognizerSpeechViewModel recognizerViewModel;
        private CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        public RecognizerSpeechViewModel RecognizerViewModel { get; set; }

        public MainViewModel()
        {
            RecognizerViewModel = new RecognizerSpeechViewModel(dispatcher);
        }

        public async void ToPage()
        {
            appWindow = await AppWindow.TryCreateAsync();
            Frame appWindowContentFrame = new Frame();
            appWindowContentFrame.Navigate(typeof(RecentList), RecognizerViewModel);
            ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);
            appWindow.RequestSize(new Size(500, 900));
            await appWindow.TryShowAsync();
            //this.mainPage.IsEnabled = false;

            appWindow.Closed += delegate
            {
                //    //Task.Delay(1000);
                //    dictationTextBox.Text = "";
                //    appWindowContentFrame.Content = null;
                //    appWindow = null;
                //this.mainPage.IsEnabled = true;
                //    recentFile = RecentList.openFile;
                //    RecentList.openFile = null;
                //    OpenFileDialog_Click(null, null);
                //
            };
        }
    }
}
