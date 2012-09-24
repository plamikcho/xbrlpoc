<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="XBRLSECviewer.aspx.cs" Inherits="StatusUpdateSample.XBRLSECviewer" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        body 
        {
            font-family: Arial, Verdana;
            font-size: 10pt;    
        }
        
        #box 
        {
            width: auto;
            background-color: #ffe;
            margin: 10px auto 0px 0;
            border: 1px solid #496077;
            display: none;
        }
        
        #preview 
        {
            display: block;
        }
        
        table 
        {
            border: 1px;
            border-collapse: collapse;
            padding: 1px;
        }
        
        #status 
        {            
            font-size: 9pt;
        }
        
    </style>
    <script type="text/javascript">
        var initBox = false;

        function InitBox() {
            if (!initBox) {
                document.getElementById('preview').style.display = "none";
                document.getElementById('box').style.display = "block";
                initBox = true;
            }
        }

        function BeginProcess() {
            var filename = document.getElementById("HiddenFilePath").value;
            var iframe = document.createElement("iframe");
            iframe.src = "XBRLSECrenderer.aspx?file=" + filename;
            iframe.style.display = "none";
            document.body.appendChild(iframe);
        }
        
        function UpdateStatus(message) {
            InitBox();
            document.getElementById('status').innerHTML += "<pre>" + message + "</pre>";
        }

        function toBottom() {
            window.scrollTo(0, document.body.scrollHeight);
        }
        
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:HiddenField ID="HiddenFilePath" runat="server" ClientIDMode="Static" />
        <div>
            <h3>
                Selected xbrl instance document: &nbsp;
                <asp:Label ID="LabelXbrlInstanceDoc" Font-Bold="true" ForeColor="Blue" runat="server" Text="-"></asp:Label>
                &nbsp;
                <input type="submit" value="Render xbrl" id="trigger" onclick="BeginProcess(); return false;" />
            </h3>            
            <div id="box">           
                <span id="status"></span>                        
            </div>        
        </div>
        <br />
        <div id="preview">
            <asp:Xml ID="Xml1" ClientIDMode="Static" runat="server"></asp:Xml>  
        </div>
    </form>
</body>
</html>
