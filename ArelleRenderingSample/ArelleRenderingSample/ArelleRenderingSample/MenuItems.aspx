<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MenuItems.aspx.cs" ValidateRequest="false" Inherits="ArelleRenderingSample.MenuItems" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" EnablePartialRendering="true" runat="server">
    </asp:ScriptManager>
    
    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
        <ProgressTemplate>
            <div style="font-family: Arial, Verdana; font-weight: bold; background-color: #1ea; width: 200px">
                Loading instance...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="UpdatePanel1"  UpdateMode="Conditional" runat="server">
        <ContentTemplate>
            <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/InstanceList.aspx">Home</asp:HyperLink>
            <asp:HiddenField ID="HiddenFile" runat="server" />
            <table border="0">
                <tr>
                    <td style="width: 20%; vertical-align: top;">
                        <div id="hello" style="Z-INDEX: 102; OVERFLOW: auto; 
                            WIDTH: 250px; POSITION: relative; HEIGHT: 500px" >
                            <asp:ListBox ID="ListBox1" runat="server" AutoPostBack="true"
                                Font-Size="0.7em" 
                                onselectedindexchanged="ListBox1_SelectedIndexChanged"></asp:ListBox>    
                        </div>
                    </td>
                    <td style="width: 80%; height: 100%; padding: 5px; vertical-align: top;">
                        <iframe id="frame1" runat="server" clientidmode="Static" width="100%" height="600px" ></iframe>
                    </td>
                </tr>
            </table>   
         </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="ListBox1" EventName="SelectedIndexChanged" />
        </Triggers>
    </asp:UpdatePanel>
    </form>
</body>
</html>
