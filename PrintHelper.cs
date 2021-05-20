using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Printing;

namespace Dictation
{
    class PrintHelper
    {
        #region Application Content Size Constants given in percents (normalized)
        protected const double ApplicationContentMarginLeft = 0.075;
        protected const double ApplicationContentMarginTop = 0.03;
        #endregion

        protected PrintDocument printDocument;
        protected IPrintDocumentSource printDocumentSource;
        internal List<UIElement> printPreviewPages;
        protected event EventHandler PreviewPagesCreated;
        protected FrameworkElement firstPage;
        protected Page scenarioPage;

        protected Canvas PrintCanvas
        {
            get
            {
                return scenarioPage.FindName("PrintCanvas") as Canvas;
            }
        }

        public PrintHelper(Page scenarioPage)
        {
            this.scenarioPage = scenarioPage;
            printPreviewPages = new List<UIElement>();
        }

        public virtual void RegisterForPrinting()
        {
            printDocument = new PrintDocument();
            printDocumentSource = printDocument.DocumentSource;
            printDocument.Paginate += CreatePrintPreviewPages;
            printDocument.GetPreviewPage += GetPrintPreviewPage;
            printDocument.AddPages += AddPrintPages;

            PrintManager printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested += PrintTaskRequested;
        }

        public virtual void UnregisterForPrinting()
        {
            if (printDocument == null)
            {
                return;
            }

            printDocument.Paginate -= CreatePrintPreviewPages;
            printDocument.GetPreviewPage -= GetPrintPreviewPage;
            printDocument.AddPages -= AddPrintPages;

            PrintManager printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested -= PrintTaskRequested;

            PrintCanvas.Children.Clear();
        }

        public async Task ShowPrintUIAsync()
        {
            try
            {
                await PrintManager.ShowPrintUIAsync();
            }
            catch (Exception e)
            {
                
            }
        }

        public virtual void PreparePrintContent(Page page)
        {

            if (firstPage == null)
            {
                firstPage = page;
                StackPanel header = (StackPanel)firstPage.FindName("Header");
                header.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            PrintCanvas.Children.Add(firstPage);
            PrintCanvas.InvalidateMeasure();
            PrintCanvas.UpdateLayout();
        }

        protected virtual void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs e)
        {
            PrintTask printTask = null;
            printTask = e.Request.CreatePrintTask("Essential Grocer Printing Example", sourceRequested =>
            {
                printTask.Completed += async (s, args) =>
                {
                    if (args.Completion == PrintTaskCompletion.Failed)
                    {
                        await scenarioPage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            
                        });
                    }
                };

                sourceRequested.SetSource(printDocumentSource);
            });
        }

        protected virtual void CreatePrintPreviewPages(object sender, PaginateEventArgs e)
        {
            try
            {
                printPreviewPages.Clear();
                PrintCanvas.Children.Clear();
            }
            catch
            {

            }
            RichTextBlockOverflow lastRTBOOnPage;
            PrintTaskOptions printingOptions = ((PrintTaskOptions)e.PrintTaskOptions);
            PrintPageDescription pageDescription = printingOptions.GetPageDescription(0);
            lastRTBOOnPage = AddOnePrintPreviewPage(null, pageDescription);

            while (lastRTBOOnPage.HasOverflowContent && lastRTBOOnPage.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                lastRTBOOnPage = AddOnePrintPreviewPage(lastRTBOOnPage, pageDescription);
            }

            if (PreviewPagesCreated != null)
            {
                PreviewPagesCreated.Invoke(printPreviewPages, null);
            }

            PrintDocument printDoc = (PrintDocument)sender;
            printDoc.SetPreviewPageCount(printPreviewPages.Count, PreviewPageCountType.Intermediate);
        }

        protected virtual void GetPrintPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            PrintDocument printDoc = (PrintDocument)sender;
            try
            {
                printDoc.SetPreviewPage(e.PageNumber, printPreviewPages[e.PageNumber - 1]);
            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }

        protected virtual void AddPrintPages(object sender, AddPagesEventArgs e)
        {
            for (int i = 0; i < printPreviewPages.Count; i++)
            {
                printDocument.AddPage(printPreviewPages[i]);
            }

            PrintDocument printDoc = (PrintDocument)sender;
            printDoc.AddPagesComplete();
        }

        protected virtual RichTextBlockOverflow AddOnePrintPreviewPage(RichTextBlockOverflow lastRTBOAdded, PrintPageDescription printPageDescription)
        {
            FrameworkElement page;
            RichTextBlockOverflow textLink;
            if (lastRTBOAdded == null)
            {
                page = firstPage;
                StackPanel footer = (StackPanel)page.FindName("Footer");
                footer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                page = new ContinuationPage(lastRTBOAdded);
            }

            page.Width = printPageDescription.PageSize.Width;
            page.Height = printPageDescription.PageSize.Height;

            Grid printableArea = (Grid)page.FindName("PrintableArea");

            double marginWidth = Math.Max(printPageDescription.PageSize.Width - printPageDescription.ImageableRect.Width, printPageDescription.PageSize.Width * ApplicationContentMarginLeft * 2);
            double marginHeight = Math.Max(printPageDescription.PageSize.Height - printPageDescription.ImageableRect.Height, printPageDescription.PageSize.Height * ApplicationContentMarginTop * 2);

            printableArea.Width = firstPage.Width - marginWidth;
            printableArea.Height = firstPage.Height - marginHeight;

            PrintCanvas.Children.Add(page);
            PrintCanvas.InvalidateMeasure();
            PrintCanvas.UpdateLayout();

            textLink = (RichTextBlockOverflow)page.FindName("ContinuationPageLinkedContainer");

            if (!textLink.HasOverflowContent && textLink.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                StackPanel footer = (StackPanel)page.FindName("Footer");
                footer.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            printPreviewPages.Add(page);

            return textLink;
        }
    }
}
