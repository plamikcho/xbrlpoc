using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ArelleRenderingSample
{
    public partial class InstanceList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                System.IO.DirectoryInfo RootDir = new System.IO.DirectoryInfo(Server.MapPath("~/Uploaded"));
                // output the directory into a node
                TreeNode RootNode = OutputDirectory(RootDir, null);
                // add the output to the tree
                TreeView1.Nodes.Add(RootNode);
                //TreeView1.CollapseAll();                
            }
        }

        TreeNode OutputDirectory(System.IO.DirectoryInfo directory, TreeNode parentNode)
        {
            // validate param
            if (directory == null) return null;

            // create a node for this directory
            TreeNode DirNode = new TreeNode(directory.Name);
            
            // get subdirectories of the current directory
            System.IO.DirectoryInfo[] SubDirectories = directory.GetDirectories();

            // output each subdirectory
            for (int DirectoryCount = 0; DirectoryCount < SubDirectories.Length; DirectoryCount++)
            {
                OutputDirectory(SubDirectories[DirectoryCount], DirNode);
            }

            // output the current directories files
            System.IO.FileInfo[] Files = directory.GetFiles();

            for (int FileCount = 0; FileCount < Files.Length; FileCount++)
            {
                DirNode.ChildNodes.Add(new TreeNode(Files[FileCount].Name));
            }

            // if the parent node is null, return this node
            // otherwise add this node to the parent and return the parent
            if (parentNode == null)
            {
                return DirNode;
            }
            else
            {
                parentNode.ChildNodes.Add(DirNode);
                return parentNode;
            }
        }

        protected void TreeView1_SelectedNodeChanged(object sender, EventArgs e)
        {
            Label1.Text = TreeView1.SelectedNode.ValuePath;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            ArelleWebRepository arelleRepo = new ArelleWebRepository(Server.MapPath(TreeView1.SelectedNode.Parent.ValuePath));
            ArelleWebRequest arr = new ArelleWebRequest();
            arr.ViewParameters["file"] = Server.MapPath(TreeView1.SelectedNode.ValuePath);
            arelleRepo.GetRestResult(arr);            
            Response.Redirect(string.Format("~/MenuItems.aspx?file={0}", arelleRepo.InstanceSavePath));
        }
    }
}