
namespace RendererWeb
{
    using System.IO;
    using System.Web;

    /// <summary>
    /// Build and control various input file and folder params
    /// </summary>
    public class RenderStartupParameters
    {
        public const string RENDERED_HTML_FOLDER_VAR = "__RENDERED_HTML_FOLDER__";

        // ctor
        public RenderStartupParameters(string xbrlInstancePath)
        {
            this.XBRLInstancePath = xbrlInstancePath;
            this.ConfigureParams();
            this.BuildXmlSource();
        }

        private RenderStartupParameters()
        {
        }

        public bool IsPathRelative { get; private set; }

        public string XBRLInstancePath { get; private set; }

        public string XBRLInstanceFullPath { get; private set; }

        public string FolderPath { get; private set; }

        public string FolderRelativePath { get; private set; }

        public string ReportsFolder { get; private set; }

        public string XBRLFileName { get; private set; }

        public string XmlFilingSummary { get; private set; }

        public string XmlFilingSummaryXslt { get; private set; }

        private void ConfigureParams()
        {
            if (Path.IsPathRooted(this.XBRLInstancePath))
            {
                this.IsPathRelative = false;
                this.FolderPath = Path.GetDirectoryName(this.XBRLInstancePath);
                this.XBRLInstanceFullPath = this.XBRLInstancePath;
            }
            else
            {
                this.IsPathRelative = true;
                this.FolderRelativePath = Path.GetDirectoryName(this.XBRLInstancePath);
                this.XBRLInstanceFullPath = HttpContext.Current.Server.MapPath(this.XBRLInstancePath);
                this.FolderPath = Path.GetDirectoryName(this.XBRLInstanceFullPath);
            }

            this.XBRLFileName = Path.GetFileName(this.XBRLInstancePath);
            this.ReportsFolder = Path.GetFileNameWithoutExtension(this.XBRLInstancePath);
        }

        private void BuildXmlSource()
        {
            string reportsFolderPath = Utils.AddBackslash(this.FolderPath) +
                    Utils.AddBackslash(this.ReportsFolder);
            this.XmlFilingSummary = reportsFolderPath + "FilingSummary.xml";
            string inputXslt = "~/Resources/InstanceReportSummary.xslt";
            string outputXslt = reportsFolderPath + "InstanceReportSummary.xslt";
            string replace = Utils.PreparePathForXslt(Utils.AddBackslash(this.IsPathRelative ? this.FolderRelativePath : this.FolderPath) +
                    Utils.AddBackslash(this.ReportsFolder));

            Utils.ReplaceStringInFile(inputXslt, RENDERED_HTML_FOLDER_VAR,
                 replace, outputXslt);
            this.XmlFilingSummaryXslt = Utils.AddBackslash(this.IsPathRelative ? this.FolderRelativePath : this.FolderPath) + 
                Utils.AddBackslash(this.ReportsFolder) + "InstanceReportSummary.xslt";
        }
    }
}