using System;
using System.Threading;
using System.Windows.Forms;

namespace CIRCBot.Apis
{
    /// <summary>
    /// Connects to a website and allows it to run its javascript
    /// </summary>
    public class WebProcessorBase
    {
        protected string URL { get; set; }

        protected void InitThread()
        {
            Thread t = new Thread(new ThreadStart(WebBrowserThread));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        private void WebBrowserThread()
        {
            WebBrowser wb = new WebBrowser();
            wb.ScriptErrorsSuppressed = true;
            wb.Navigate(URL);

            wb.DocumentCompleted +=
                new WebBrowserDocumentCompletedEventHandler(
                    wb_DocumentCompleted);

            int count = 0;
            while (wb.ReadyState != WebBrowserReadyState.Complete && wb.ReadyState != WebBrowserReadyState.Interactive && count < 99000)
            {
                Application.DoEvents();
                count++;
            }
            
            Thread.Sleep(10);

            try
            {
                Process(wb);
            }
            catch (NotImplementedException ex)
            {
                Console.WriteLine(ex);
            }

            wb.Dispose();
        }

        protected virtual void Process(WebBrowser wb)
        {
            throw new NotImplementedException();
        }

        private void wb_DocumentCompleted(object sender,
            WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser wb = (WebBrowser)sender;
            string html = wb.Document.Body.InnerHtml;
        }
    }
}
