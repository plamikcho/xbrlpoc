using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ArelleRenderingSample
{
    public partial class ViewXbrl : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
           // Page.Trace.IsEnabled = true;
            if (!IsPostBack)
            {
                if (Request.QueryString["id"] != null && Request.QueryString["file"] != null)
                {
                    string file = Server.UrlDecode(Request.QueryString["file"].Trim());
                    string id = Server.UrlDecode(Request.QueryString["id"].Trim());
                    
                    using (ArelleFactTableHtmlRenderer arelleRenderer = new ArelleFactTableHtmlRenderer(file))
                    {
                        var renderedDoc = arelleRenderer.GetViewItemById(id);
                        Response.Write(Server.HtmlDecode( renderedDoc.DocumentNode.InnerHtml));
                    }
                }
            }      
        }
    }
}