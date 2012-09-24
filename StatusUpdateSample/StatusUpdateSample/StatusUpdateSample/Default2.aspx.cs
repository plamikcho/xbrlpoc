using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RendererWeb;

namespace StatusUpdateSample
{
    public partial class Default2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string file = @"~/Uploaded/plamen/brka.zip";
            RenderStartupParameters rsp = new RenderStartupParameters(file);
            if (!IsPostBack)
            {
                frame1.Attributes["src"] = string.Format(
                "XBRLSECviewer.aspx?file={0}", Server.UrlEncode(rsp.XBRLInstancePath));
            }
        }
    }
}