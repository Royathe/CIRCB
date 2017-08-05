using System;
using System.Threading;
using System.Windows.Forms;

namespace CIRCBot.Apis
{
    /// <summary>
    /// Loads a website and allows it to process javacript until a certain section in the inner html has data.
    /// </summary>
    class WebParser : WebProcessorBase
    {
        private string GeneratedSource { get; set; }
        private string ElementStart { get; set; }
        private string ElementEnd { get; set; }

        public string GetGeneratedHTML(string url, string elementStart = "", string elementEnd = "")
        {
            URL = url;
            ElementStart = elementStart;
            ElementEnd = elementEnd;

            InitThread();
            
            return GeneratedSource;
        }

        protected override void Process(WebBrowser wb)
        {
            int count = 0;
            GeneratedSource = string.Empty;
            while (count < 100)
            {
                count++;
                Thread.Sleep(10);
                if (wb.Document != null && wb.Document.Body != null && wb.Document.Body.InnerHtml != null)
                {
                    if (ElementStart == "")
                    {
                        GeneratedSource = wb.Document.Body.InnerHtml;
                    }
                    else
                    {
                        if (ElementEnd != "")
                        {
                            GeneratedSource = wb.Document.Body.InnerHtml.Between(ElementStart, ElementEnd);
                        }
                        else
                        {
                            GeneratedSource = wb.Document.Body.InnerHtml.GetElement(ElementStart);
                        }
                    }
                }
                if (GeneratedSource == string.Empty)
                {
                    Application.DoEvents();
                }
                else
                {
                    break;
                }
            }
        }
    }
}
