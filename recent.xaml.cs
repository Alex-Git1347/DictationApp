using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Dictation
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class recent : Page
    {
        private List<StorageFile> files;
        public recent()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                files = (List<StorageFile>)e.Parameter;
                foreach (var file in files)
                {
                    Button button = new Button();
                    button.Click += OpenFile_Click;
                    button.Content = file.Path;
                    recentList.Children.Add(button);
                }
            }
            catch (ArgumentNullException) { }
            base.OnNavigatedTo(e);
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StorageFile file = files.FirstOrDefault(f => f.Path.ToString() == button.Content.ToString());
            this.Frame.Navigate(typeof(MainPage),file);
        }

        private void BackToMain_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
