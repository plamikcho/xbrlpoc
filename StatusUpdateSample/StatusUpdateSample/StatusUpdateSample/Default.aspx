<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="StatusUpdateSample._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <style type="text/css">
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
            font-size: 0.9em;
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

        function BeginProcess(filename) {
            // Create an iframe.
            var iframe = document.createElement("iframe");
            // Point the iframe to the location of
            //  the long running process.
            iframe.src = "LongRunningProcess.aspx?file=" + filename;
            // Make the iframe invisible.
            iframe.style.display = "none";
            // Add the iframe to the DOM.  The process
            //  will begin execution at this point.
            document.body.appendChild(iframe);
        }

        function UpdateProgress(PercentComplete, Message) {
            document.getElementById('status').innerHTML =
            PercentComplete + '%: ' + Message;
        }

        function UpdateStatus(message) {
            InitBox();
            document.getElementById('status').innerHTML += "<pre>" + message + "</pre>";
        }

        function toBottom() {
            window.scrollTo(0, document.body.scrollHeight);            
        }

        function uploadError(sender, args) {
            document.getElementById('labelStatus').innerText = args.get_fileName() + //,
	        "<span style='color:red;'>" + args.get_errorMessage() + "</span>";
        }

        function StartUpload(sender, args) {
            document.getElementById('labelStatus').innerText = 'Uploading Started.';
        }

        function UploadComplete(sender, args) {
            var filename = args.get_fileName();
            var contentType = args.get_contentType();
            var text = "Size of " + filename + " is " + args.get_length() + " bytes";
            if (contentType.length > 0) {
                text += " and content type is '" + contentType + "'.";
            }
            document.getElementById('labelStatus').innerText = text;
            BeginProcess(filename);
        }

    </script>
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h3>
        Upload zipped package with the xbrl files (instance document and taxonomy files)
    </h3>    
    <div>
        <asp:Image ID="myThrobber" runat="server" ImageUrl="~/Styles/ajax-loader.gif"  />
        
        <asp:Label ID="labelStatus" ClientIDMode="Static" runat="server" Text="Label">-</asp:Label>
        <cc1:AsyncFileUpload OnClientUploadError="uploadError" OnClientUploadStarted="StartUpload" 
            OnClientUploadComplete="UploadComplete"
            OnUploadedComplete="AsyncFileUpload1_UploadedComplete" 
            OnUploadedFileError="AsyncFileUpload1_UploadedFileError" 
          runat="server"
         ID="AsyncFileUpload1" UploaderStyle="Traditional"
         UploadingBackColor="#CCFFFF" ThrobberID="myThrobber" />
        </div>
    <div>
        <input type="submit" value="Start Long Running Process" 
            id="trigger" onclick="BeginProcess(); return false;" style="display: none" />
        <div id="box">           
            <span id="status"></span>                        
        </div>        
    </div>
    <br />
    <div id="preview">
        <h3>
            Current rendered xbrl instance document
        </h3>
        <br />
        <asp:Xml ID="Xml1" ClientIDMode="Static" runat="server"></asp:Xml>  
    </div>
</asp:Content>
