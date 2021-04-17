using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace Dictation
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        private RecognizerSpeechViewModel RecognizerViewModel { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            RecognizerViewModel = new RecognizerSpeechViewModel(dispatcher);            
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {

            if (RecognizerViewModel.RecognizerSpeech.dictatedTextBuilder.Length != 0)
            {
                RecognizerViewModel.RecognizerSpeech.dictatedTextBuilder.Clear();
                Bindings.Update();
            }
            RecognizerViewModel.RecognizerSpeech.StartRecording();
            
        }

        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            RecognizerViewModel.RecognizerSpeech.StopRecording();
            Bindings.Update();
        }

        private void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            RecognizerViewModel.RecognizerSpeech.ClearRecordingText();
            Bindings.Update();
        }
    }
}
