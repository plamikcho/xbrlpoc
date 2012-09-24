using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RendererWeb;

namespace StatusUpdateSample
{
    public partial class LongRunningProcess : System.Web.UI.Page
    {
        string fiName = "plamen";
        string reportsFolder = "test";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string filename = string.Empty;

                try
                {
                    filename = Request.QueryString["file"];
                }
                catch
                {
                }
                
                UpdateProcessor up = new UpdateProcessor(Page.Response, Utils.PrepareUserDir(fiName) + filename);
                //up.TestProcess();
                up.CallExternalProcessor(reportsFolder);
            }
        }
    }
}