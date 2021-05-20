using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Dictation
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class PageToPrint : Page
    {
        string textPrint;
        public PageToPrint(string text)
        {
            this.InitializeComponent();
            this.textPrint = text;
            MakeThePrintOut();
        }

        private void MakeThePrintOut()
        {
            PopulateBlock(TextContent);
        }
        private void PopulateBlock(RichTextBlock Blocker)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            run.Text = textPrint;
            paragraph.Inlines.Add(run);
            Blocker.Blocks.Add(paragraph);
        }
    }
}
