<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InstanceList.aspx.cs" Inherits="ArelleRenderingSample.InstanceList" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    
    <form id="form1" runat="server">
    <div>
        <h2>
            Arelle viewer sample. Choose instance to view the rendered output.
        </h2>

        <asp:Label ID="Label1" BackColor="Aquamarine" runat="server" Text="-"></asp:Label>
        &nbsp;&nbsp;
        <asp:Button ID="Button1" runat="server" Text="View" onclick="Button1_Click" />
        <br />
        <asp:TreeView ID="TreeView1" runat="server" ImageSet="XPFileExplorer" 
            NodeIndent="15" onselectednodechanged="TreeView1_SelectedNodeChanged" 
            ShowLines="True">
            <HoverNodeStyle Font-Underline="True" ForeColor="#6666AA" />
            <NodeStyle Font-Names="Tahoma" Font-Size="8pt" ForeColor="Black" 
                HorizontalPadding="2px" NodeSpacing="0px" VerticalPadding="2px" />
            <ParentNodeStyle Font-Bold="False" />
            <SelectedNodeStyle BackColor="#B5B5B5" Font-Underline="False" 
                HorizontalPadding="0px" VerticalPadding="0px" />
        </asp:TreeView>
       
    </div>
    </form>
</body>
</html>
