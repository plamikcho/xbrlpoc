
namespace ArelleRenderingSample
{
    using System.Collections.Generic;
    using System.Configuration;

    // simple web request model
    public class ArelleWebRequest
    {
        private string configUri = string.Empty;

        public string Rest { get; private set; }
        public string ViewFormat { get; private set; }

        public Dictionary<string, string> ViewParameters { get; private set; }

        public ArelleWebRequest()
        {
            this.GetConfigUri();
            this.Rest = "/rest/xbrl/view";
            this.ViewParameters = new Dictionary<string, string>();
            this.ViewParameters.Add("file", "");
            this.ViewParameters.Add("media", "html");
            this.ViewParameters.Add("view", "factTable");
        }
        
        public string GetString()
        {
            return this.configUri + this.Rest + this.ViewFormat + Utils.CreateWebRequestParameters(this.ViewParameters);
        }

        private void GetConfigUri()
        {
            this.configUri = ConfigurationManager.AppSettings.Get("ArelleHost");
        }
    }
}