using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

namespace ArelleRenderingSample
{
    public partial class MenuItems : System.Web.UI.Page
    {
        protected ArelleFactTableHtmlRenderer arelleRenderer;

        protected void Page_Load(object sender, EventArgs e)
        {
            HiddenFile.Value = "";
            if (Request.QueryString["file"] != null)
            {
                HiddenFile.Value = Request.QueryString["file"].Trim();
            }

            if (!IsPostBack)
            {
                using (arelleRenderer = new ArelleFactTableHtmlRenderer(Utils.PutSlashes(HiddenFile.Value)))
                {
                    arelleRenderer.LoadMenuItems();
                    ListBox1.DataSource = arelleRenderer.MenuItems;
                    ListBox1.DataBind();
                    int nItem = Convert.ToInt32(ListBox1.Items.Count * 17);
                    ListBox1.Height = nItem;
                    ListBox1.Width = 800;
                }
            }
        }

        protected void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            frame1.Attributes["src"] = string.Format("ViewXbrl.aspx?file={0}&id={1}", Server.UrlEncode(HiddenFile.Value), Server.UrlEncode(ListBox1.SelectedValue));
        }
    }
}