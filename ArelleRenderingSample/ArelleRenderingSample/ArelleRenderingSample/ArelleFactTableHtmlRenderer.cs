
namespace ArelleRenderingSample
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Web;
    using HtmlAgilityPack;

    // render the arelle fact table html output
    public class ArelleFactTableHtmlRenderer : IDisposable
    {
        // ctor
        // loads the html document from arelle web service
        public ArelleFactTableHtmlRenderer(string instancePath)
        {
            this.MainHtmlDocument = new HtmlDocument();
            this.InstancePath = instancePath;
            this.LoadDoc();
        }

        private ArelleFactTableHtmlRenderer()
        {
        }

        public string InstancePath { get; private set; }

        // html fact table document
        public HtmlDocument MainHtmlDocument { get; private set; }

        // menu items list
        public IList<string> MenuItems { get; private set; }

        // gets report by content id
        public HtmlDocument GetViewItemById(string id)
        {
            var reportRows = this.ExtractReportRowsById(id);
            this.LoadRowSegment(reportRows);
            var arelleStructure = new List<ArelleColumnSection>();
            this.DistributeColumnsStructure(arelleStructure);
            this.WipeEmptyColumns(arelleStructure);
            this.FinalizeHtmlDocument(arelleStructure);
            arelleStructure.Clear();
            arelleStructure = null;
            return this.MainHtmlDocument;
        }

        // fill menu items list from main html document
        public void LoadMenuItems()
        {
            this.MenuItems = new List<string>();
            foreach (var menuItem in this.MainRowsWithAttributedChildren)
            {
                var tdrows = this.GetTds(menuItem, "td");
                var cn = tdrows.Count;
                if (cn == 1)
                {
                    this.MenuItems.Add(tdrows.First().InnerText.Trim());
                }
            }
        }

        // gets table node in Arelle html
        private HtmlNode mainTableNode
        {
            get
            {
                var docnode = this.MainHtmlDocument.DocumentNode;
                if (docnode != null)
                {
                    var tableNode = (from node in docnode.Descendants()
                                     where node.Name == "table"
                                     select node).FirstOrDefault();
                    return tableNode;
                }

                return null;
            }
        }

        // gets table rows
        private IEnumerable<HtmlNode> MainTableRows
        {
            get
            {
                var tableNode = this.mainTableNode;
                if (tableNode != null)
                {
                    var rows = tableNode.ChildNodes.Where(cn => cn.Name == "tr");
                    return rows;
                }

                return new List<HtmlNode>();
            }
        }

        // gets table rows with attributed <td>
        private IEnumerable<HtmlNode> MainRowsWithAttributedChildren
        {
            get
            {
                return this.MainTableRows.Where(dn => dn.ChildNodes.Count(c => c.HasAttributes == true) > 0);
            }
        }

        // loads html document into the object model
        private void LoadDoc()
        {
            Utils.CleanHtml(this.InstancePath);
            this.MainHtmlDocument.Load(this.InstancePath, true);       
        }

        // get <tr> child elements by tagname
        private IList<HtmlNode> GetTds(HtmlNode trNode, string tdType)
        {
            return trNode.Descendants().Where(d => d.Name == tdType).ToList();
        }

        // extract the rows between 2 content rows and associate them to the first content row
        private IList<HtmlNode> ExtractReportRowsById(string id)
        {
            var outlist = new List<HtmlNode>();
            bool found = false;
            int menucount = 0;
            int foundindex = 0;

            foreach (var menuItem in this.MainRowsWithAttributedChildren)
            {
                bool todelete = true;
                bool isMenuRow = false;
                
                var tds = this.GetTds(menuItem, "td");
                if (tds.Count == 1)
                {
                    isMenuRow = true;
                }

                foreach (var ditem in menuItem.Descendants())
                {
                    if (ditem.HasAttributes)
                    {
                        if (ditem.Name == "th")
                        {
                            todelete = false;
                            break;
                        }

                        if (isMenuRow)
                        {
                            menucount++;
                            if (ditem.InnerText.StartsWith(id))
                            {
                                if (found)
                                {
                                    break;
                                }
                                
                                todelete = false;
                                found = true;
                                foundindex = menucount;
                            }
                        }

                        if (found && menucount < foundindex + 1)
                        {
                            if (ditem.Attributes["class"].Value == "tableCell"
                                && !string.IsNullOrEmpty(ditem.InnerText.Trim())) // 
                            {
                                todelete = false;
                            }
                        }
                    }
                }

                if (!todelete)
                {
                    outlist.Add(menuItem);
                }
            }

            return outlist;
        }

        private void LoadRowSegment(IList<HtmlNode> rowList)
        {
            this.mainTableNode.RemoveAllChildren();
            rowList.ToList().ForEach(l => this.mainTableNode.ChildNodes.Add(l));
        }

        // extract the variable lead columns and static grid ones into collections
        private void DistributeColumnsStructure(IList<ArelleColumnSection> outlist)
        {
            foreach (var item in this.MainRowsWithAttributedChildren)
            {
                var colStructure = new ArelleColumnSection();
                bool borderfound = false;
                foreach (var ditem in item.Descendants().Where(td => td.HasAttributes == true))
                {
                    if ((ditem.Name == "th" || ditem.Attributes["class"].Value == "tableCell") &&
                        !ditem.Attributes.Contains("colSpan"))
                    {
                        colStructure.StaticColumns.Add(ditem);
                        borderfound = true;
                    }
                    else
                    {
                        if (!borderfound)
                        {
                            colStructure.DynamicColumns.Add(ditem);
                        }
                    }
                }

                outlist.Add(colStructure);
            }
        }

        // remove column in which all cells are empty
        private void WipeEmptyColumns(IList<ArelleColumnSection> outlist)
        {
            // get static columns count
            var staCount = outlist.First().StaticColumns.Count;
            // initialize global column flag
            var globalTake = new List<bool>(staCount);
            for (int i = 0; i < staCount; i++)
            {
                globalTake.Add(false);
            }

            // take the concept column
            globalTake[0] = true;
            for (int i = 1; i < globalTake.Count; i++)
            {
                bool gtakeflag = false;
                for (int j = 1; j < outlist.Count; j++)
                {
                    if (outlist[j].StaticColumns.Count > 0)
                    {
                        var checkCol = outlist[j].StaticColumns[i];
                        if (!string.IsNullOrEmpty(checkCol.InnerText.Trim()))
                        {
                            gtakeflag = true;
                            break;
                        }
                    }
                }

                globalTake[i] = gtakeflag;
            }

            for (int i = 1; i < globalTake.Count; i++)
            {
                if (globalTake[i])
                {
                    for (int j = 0; j < outlist.Count; j++)
                    {
                        if (outlist[j].StaticColumns.Count > 0)
                        {
                            // copy marked columns
                            outlist[j].NewStaticColumns.Add(outlist[j].StaticColumns[i]);
                        }
                    }
                }
            }
            
            this.mainTableNode.ChildNodes.ToList().ForEach(cn => cn.RemoveAllChildren());
        }

        private void FinalizeHtmlDocument(IList<ArelleColumnSection> outlist)
        {
            var mtn = this.mainTableNode;
            for (int i = 0; i < mtn.ChildNodes.Count; i++)
            {
                mtn.ChildNodes[i].RemoveAllChildren();
                var row = new HtmlNodeCollection(mtn.ChildNodes[i]);
                foreach (var item in outlist[i].DynamicColumns)
                {
                    row.Add(item);
                }

                foreach (var item in outlist[i].NewStaticColumns)
                {
                    row.Add(item);
                }

                mtn.ChildNodes[i].AppendChildren(row);
            }
        }
        
        #region Disposing
       
        // Track whether Dispose has been called.
        private bool disposed = false;

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.
                    this.MainHtmlDocument = null;
                    GC.Collect();
                }
            }

            disposed = true;         
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~ArelleFactTableHtmlRenderer()
	    {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
	    }

       
       // Allow your Dispose method to be called multiple times,
       // but throw an exception if the object has been disposed.
       // Whenever you do something with this class, 
       // check to see if it has been disposed.
       public void DoSomething()
       {
          if(this.disposed)
          {
             throw new ObjectDisposedException("html");
          }
       }

        #endregion
    }
}