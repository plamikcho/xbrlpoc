
namespace ArelleRenderingSample
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using HtmlAgilityPack;

    // arelle fact table column model
    public class ArelleColumnSection
    {
        // left columns containing the tree nodes - variable size
        public List<HtmlNode> DynamicColumns { get; set; }
        // grid columns with fact values
        public List<HtmlNode> StaticColumns { get; set; }
        // grid columns after wiping the columns not related to the concrete xbrl report item
        public List<HtmlNode> NewStaticColumns { get; set; }

        // ctor
        public ArelleColumnSection()
        {
            this.DynamicColumns = new List<HtmlNode>();
            this.StaticColumns = new List<HtmlNode>();
            this.NewStaticColumns = new List<HtmlNode>();
        }
    }
}