using System;
using System.IO;
using RendererWeb;

namespace StatusUpdateSample
{
    public partial class XBRLSECviewer : System.Web.UI.Page
    {
        RenderStartupParameters rsp;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["file"] != null)
                {
                    string file = Request.QueryString["file"].Trim();
                    rsp = new RenderStartupParameters(file);
                    this.HiddenFilePath.Value = rsp.XBRLInstanceFullPath;
                    this.LabelXbrlInstanceDoc.Text = rsp.XBRLFileName;
                    if (File.Exists(rsp.XmlFilingSummary))
                    {
                        Xml1.DocumentSource = rsp.XmlFilingSummary;
                        Xml1.TransformSource = rsp.XmlFilingSummaryXslt;
                    }
                }
            }
        }
    }
}