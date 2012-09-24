using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using RendererWeb;
using System.Xml.Xsl;

namespace StatusUpdateSample
{
    public partial class _Default : System.Web.UI.Page
    {
        string fiName = "plamen";

        protected void Page_Load(object sender, EventArgs e)
        {
            string xmlsource = "~/Uploaded/" + fiName+ "/test/" + "FilingSummary.xml";
            if (File.Exists(Server.MapPath(xmlsource)))
            {
                 Xml1.DocumentSource = xmlsource;
                 Xml1.TransformSource = "~/Resources/InstanceReportSummary.xslt";                 
            }           
            
        }

        protected void AsyncFileUpload1_UploadedFileError(object sender, AjaxControlToolkit.AsyncFileUploadEventArgs e)
        {
            // log error            
        }

        protected void AsyncFileUpload1_UploadedComplete(object sender, AjaxControlToolkit.AsyncFileUploadEventArgs e)
        {
            if (AsyncFileUpload1.HasFile && e.State == AjaxControlToolkit.AsyncFileUploadState.Success)
            {
                AsyncFileUpload1.SaveAs(Utils.PrepareUserDir(fiName) + e.FileName);
            }       
        }
    }
}
