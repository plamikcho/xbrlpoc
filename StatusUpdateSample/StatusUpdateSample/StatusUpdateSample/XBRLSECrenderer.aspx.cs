using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RendererWeb;

namespace StatusUpdateSample
{
    public partial class XBRLSECrenderer : System.Web.UI.Page
    {
        RenderStartupParameters rsp;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (null != Request.QueryString["file"])
                {
                    rsp = new RenderStartupParameters(Request.QueryString["file"].Trim());
                    UpdateProcessor up = new UpdateProcessor(Page.Response, rsp.XBRLInstancePath);                    
                    up.CallExternalProcessor(rsp.ReportsFolder);
                }
            }
        }
    }
}