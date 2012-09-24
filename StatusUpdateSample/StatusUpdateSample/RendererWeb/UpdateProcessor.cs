
namespace RendererWeb
{
    using System.Text;
    using System.Web;

    /// <summary>
    /// Class to simulate console status output
    /// </summary>
    public class UpdateProcessor
    {
        // injected page response
        private HttpResponse response;

        // target filename
        private string fileName;

        // ctor with page's response object and target filename
        public UpdateProcessor(HttpResponse response, string fileName)
        {
            this.response = response;
            this.fileName = fileName;
            CircumventIE();
        }

        // execute UpdateStatus in parent iframe
        public void UpdateStatus(string message)
        {
            StringBuilder messageBuilder = new StringBuilder(message);
            string format = "<script>parent.UpdateStatus('{0}'); parent.toBottom();</script>";

            // clean message to prevent from javascript errors
            messageBuilder.Replace("\n", "<br/>");
            messageBuilder.Replace(@"\", @"\\");
            messageBuilder.Replace("\"", "`");
            messageBuilder.Replace("'", "`");
            
            // use custom message to end the process and reload the page
            if (messageBuilder.ToString() == "reload")
            {
                format = @"<script>
parent.UpdateStatus('Processing complete. You will be redirected to results after 5 seconds'); parent.toBottom();
                            </script>
                        <script>
                            setTimeout('parent.window.location.reload(true);', 5000);
                        </script>";
            }

            // Write out the parent script callback.
            response.Write(string.Format(format, messageBuilder.ToString()));
            // To be sure the response isn't buffered on the server.
            response.Flush();
        }
        
        // here is the entry point for the SEC rendering engine filing processor
        public void CallExternalProcessor(string reportsFolder)
        {
            RendererStarter fp = new RendererStarter(this.UpdateStatus);
            fp.Run(this.fileName, reportsFolder);
        }        

        // Padding to circumvent IE's buffer*
        private void CircumventIE()
        {
            response.Write(new string('*', 256));
            response.Flush();
        }
    }
}