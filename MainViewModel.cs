using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Controls;

namespace Dictation
{
    public class MainViewModel
    {
        public AppWindow appWindow;
        private async void ToPage()
        {
            appWindow = await AppWindow.TryCreateAsync();
            Frame appWindowContentFrame = new Frame();
            appWindowContentFrame.Navigate(typeof(RecentList), RecognizerViewModel);
            ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);
            appWindow.RequestSize(new Size(500, 900));
            await appWindow.TryShowAsync();
            this.mainPage.IsEnabled = false;

            appWindow.Closed += delegate
            {
                //    //Task.Delay(1000);
                //    dictationTextBox.Text = "";
                //    appWindowContentFrame.Content = null;
                //    appWindow = null;
                this.mainPage.IsEnabled = true;
                //    recentFile = RecentList.openFile;
                //    RecentList.openFile = null;
                //    OpenFileDialog_Click(null, null);
                //
            };
        }
    }
}
