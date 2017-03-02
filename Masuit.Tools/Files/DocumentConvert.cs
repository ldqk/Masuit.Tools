using System.IO;
using System.Threading.Tasks;
using Aspose.Words;
using Masuit.Tools.Models;

namespace Masuit.Tools.Files
{
    /// <summary>
    /// 文档转换操作
    /// </summary>
    public static class DocumentConvert
    {
        static DocumentConvert()
        {
            AsposeLicense.ActivateMemoryPatching();
        }

        #region doc转html

        /// <summary>
        /// doc转html
        /// </summary>
        /// <param name="docPath">doc文件路径</param>
        /// <param name="htmlDir">生成的html所在目录，由于生成html后会将图片都放到同级的目录下，所以用文件夹保存，默认的html文件名为index.html</param>
        /// <param name="index">默认文档名为index.html</param>
        public static void Doc2Html(string docPath, string htmlDir, string index = "index.html")
        {
            Document doc = new Document(docPath);
            doc.Save(Path.Combine(htmlDir, index), SaveFormat.Html);
        }

        /// <summary>
        /// doc转html
        /// </summary>
        /// <param name="docPath">doc文件路径</param>
        /// <param name="htmlDir">生成的html所在目录，由于生成html后会将图片都放到同级的目录下，所以用文件夹保存，默认的html文件名为index.html</param>
        /// <param name="index">默认文档名为index.html</param>
        public static async void Doc2HtmlAsync(string docPath, string htmlDir, string index = "index.html")
        {
            Document doc = new Document(docPath);
            await Task.Run(() => doc.Save(Path.Combine(htmlDir, index), SaveFormat.Html)).ConfigureAwait(true);
        }

        #endregion

        #region doc转pdf

        /// <summary>
        /// doc转pdf
        /// </summary>
        /// <param name="docPath">doc源文件</param>
        /// <param name="pdfPath">目标pdf文件</param>
        /// <param name="paperSize">纸张大小</param>
        /// <param name="leftMargin">左边距</param>
        /// <param name="rightMargin">右边距</param>
        /// <param name="topMargin">上边距</param>
        /// <param name="bottomMargin">下边距</param>
        public static void Doc2Pdf(string docPath, string pdfPath, PaperSize paperSize = PaperSize.A4, double leftMargin = 20, double rightMargin = 20, double topMargin = 20, double bottomMargin = 20)
        {
            Document doc = new Document(docPath);
            PageSetup pageSetup = new DocumentBuilder(doc).PageSetup;
            pageSetup.PaperSize = paperSize;
            pageSetup.LeftMargin = leftMargin;
            pageSetup.RightMargin = rightMargin;
            pageSetup.TopMargin = topMargin;
            pageSetup.BottomMargin = bottomMargin;
            doc.Save(pdfPath, SaveFormat.Pdf);
        }

        /// <summary>
        /// doc转pdf
        /// </summary>
        /// <param name="docPath">doc源文件</param>
        /// <param name="pdfPath">目标pdf文件</param>
        /// <param name="paperSize">纸张大小</param>
        /// <param name="leftMargin">左边距</param>
        /// <param name="rightMargin">右边距</param>
        /// <param name="topMargin">上边距</param>
        /// <param name="bottomMargin">下边距</param>
        public static async void Doc2PdfAsync(string docPath, string pdfPath, PaperSize paperSize = PaperSize.A4, double leftMargin = 20, double rightMargin = 20, double topMargin = 20, double bottomMargin = 20)
        {
            Document doc = new Document(docPath);
            PageSetup pageSetup = new DocumentBuilder(doc).PageSetup;
            pageSetup.PaperSize = paperSize;
            pageSetup.LeftMargin = leftMargin;
            pageSetup.RightMargin = rightMargin;
            pageSetup.TopMargin = topMargin;
            pageSetup.BottomMargin = bottomMargin;
            await Task.Run(() => doc.Save(pdfPath, SaveFormat.Pdf)).ConfigureAwait(false);
        }

        #endregion

        #region html转Word

        /// <summary>
        /// html转Word
        /// </summary>
        /// <param name="htmlPath">html源文件</param>
        /// <param name="docPath">目标doc文件</param>
        /// <param name="paperSize">纸张大小，默认A4纸</param>
        /// <param name="leftMargin">左边距，默认10</param>
        /// <param name="rightMargin">右边距，默认10</param>
        /// <param name="topMargin">上边距，默认10</param>
        /// <param name="bottomMargin">下边距，默认10</param>
        public static void Html2Word(string htmlPath, string docPath, PaperSize paperSize = PaperSize.A4, double leftMargin = 20, double rightMargin = 20, double topMargin = 20, double bottomMargin = 20)
        {
            Document doc = new Document(htmlPath);
            PageSetup pageSetup = new DocumentBuilder(doc).PageSetup;
            pageSetup.PaperSize = paperSize;
            pageSetup.LeftMargin = leftMargin;
            pageSetup.RightMargin = rightMargin;
            pageSetup.TopMargin = topMargin;
            pageSetup.BottomMargin = bottomMargin;
            doc.Save(docPath, SaveFormat.Doc);
        }

        /// <summary>
        /// html转Word
        /// </summary>
        /// <param name="htmlPath">html源文件</param>
        /// <param name="docPath">目标doc文件</param>
        /// <param name="paperSize">纸张大小，默认A4纸</param>
        /// <param name="leftMargin">左边距，默认10</param>
        /// <param name="rightMargin">右边距，默认10</param>
        /// <param name="topMargin">上边距，默认10</param>
        /// <param name="bottomMargin">下边距，默认10</param>
        public static async void Html2WordAsync(string htmlPath, string docPath, PaperSize paperSize = PaperSize.A4, double leftMargin = 20, double rightMargin = 20, double topMargin = 20, double bottomMargin = 20)
        {
            Document doc = new Document(htmlPath);
            PageSetup pageSetup = new DocumentBuilder(doc).PageSetup;
            pageSetup.PaperSize = paperSize;
            pageSetup.LeftMargin = leftMargin;
            pageSetup.RightMargin = rightMargin;
            pageSetup.TopMargin = topMargin;
            pageSetup.BottomMargin = bottomMargin;
            await Task.Run(() => doc.Save(docPath, SaveFormat.Doc)).ConfigureAwait(false);
        }

        #endregion

    }
}