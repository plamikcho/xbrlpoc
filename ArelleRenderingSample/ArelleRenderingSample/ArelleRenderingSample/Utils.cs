
namespace ArelleRenderingSample
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Web;

    public class Utils
    {
        public static string CreateWebRequestParameters(Dictionary<string, string> parameters)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var item in parameters)
            {
                if (first)
                {
                    sb.Append("?");
                    first = false;
                }
                else
                {
                    sb.Append("&");
                }

                sb.Append(item.Key + "=" + item.Value);
            }

            return sb.ToString();
        }

        public static string PutSlashes(string path)
        {
            return path.Replace(@"\", "/");
        }

        public static string PutBackSlashes(string path)
        {
            return path.Replace("/", @"\");
        }

        public static string AddBackSlash(string path)
        {
            string separator = @"\";
            return path.EndsWith(separator) ? path : path + separator;
        }

        public static string PrepareUserDir(string username)
        {
            string separator = @"\";
            string targetdir = HttpContext.Current.Server.MapPath("~/Uploaded/");
            string savedir = targetdir + separator + username;
            if (!Directory.Exists(savedir))
            {
                Directory.CreateDirectory(savedir);
            }

            return savedir.EndsWith(separator) ? savedir : savedir + separator;
        }

        public static void CleanHtml(string htmlFilePath)
        {
            string htmlContent = File.ReadAllText(htmlFilePath);
            string toRemove = "\n";
            string newContent = htmlContent.Replace(toRemove, string.Empty);            
            //string newContent = Regex.Replace(htmlContent, @"(?<=\s)\s+(?![^<>]*</pre>)", string.Empty, RegexOptions.None);
            //File.WriteAllText(htmlFilePath, Regex.Replace(htmlContent, @"\s*(<[^>]+>)\s*", "$1", RegexOptions.Singleline), Encoding.UTF8);
            File.WriteAllText(htmlFilePath, newContent, Encoding.UTF8);
        }
    }
}