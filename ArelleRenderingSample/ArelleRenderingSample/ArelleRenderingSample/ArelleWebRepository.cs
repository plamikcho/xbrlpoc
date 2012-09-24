
namespace ArelleRenderingSample
{
    using System.IO;
    using System.Net;

    // simple web request wrapper
    public class ArelleWebRepository
    {
        private string tempFolderPath;

        public ArelleWebRepository(string tempFolderPath)
        {
            this.tempFolderPath = tempFolderPath;            
        }

        public string InstanceSavePath { get; private set; }        

        public void GetRestResult(ArelleWebRequest request)
        {
            HttpWebRequest req = WebRequest.Create(Utils.PutSlashes(request.GetString())) as HttpWebRequest;
            
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                string savepath = Utils.AddBackSlash(this.tempFolderPath) + 
                    Path.GetFileNameWithoutExtension(request.ViewParameters["file"]) + 
                    "." + request.ViewParameters["media"];
                using (var fileStream = File.Create(savepath))
                {
                    resp.GetResponseStream().CopyTo(fileStream);
                }

                this.InstanceSavePath = savepath;
            }
        }
    }
}