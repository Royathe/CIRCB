using System;
using System.Threading;
using System.Windows.Forms;

namespace CIRCBot.Apis
{
    /// <summary>
    /// Retrieves the current time from https://time.is/
    /// </summary>
    class TimeIsAPI : WebProcessorBase
    {
        private const string url = "https://time.is/";

        private DateTime LastModifiedDate { get; set; }

        public TimeIsAPI()
        {
        }

        public DateTime CurrentTime()
        {
            URL = url;

            InitThread();

            return LastModifiedDate;
        }

        protected override void Process(WebBrowser wb)
        {
            int count = 0;

            while (count < 100)
            {
                count++;
                Thread.Sleep(10);

                if (wb.Document != null && wb.Document.Body != null && wb.Document.Body.InnerHtml != null)
                {
                    var time = wb.Document.GetElementById("twd").InnerText;

                    var date = wb.Document.GetElementById("dd").InnerText;

                    var datestring = date + " " + time;

                    DateTime lastModified;

                    if (DateTime.TryParse(datestring, out lastModified))
                    {
                        LastModifiedDate = lastModified;
                    }

                    break;
                }
                else
                {
                    Application.DoEvents();
                }
            }
        }
    }
}
