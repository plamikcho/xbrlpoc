
namespace RendererWeb
{
    using System.IO;
    using System.Web;

    /// <summary>
    /// Utilities class
    /// </summary>
    public class Utils
    {
        // Create user directory for xbrl reports - used in POC
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

        // Be sure that the path ends with backslash
        public static string AddBackslash(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path : path + Path.DirectorySeparatorChar;
        }

        // transform relative path for xslt
        public static string PreparePathForXslt(string path)
        {
            string ret = path;

            if (path.Contains("~"))
            {
                ret = ret.Replace("~", string.Empty);
            }

            ret = ret.Replace(Path.DirectorySeparatorChar, '/');
            //ret = ret.EndsWith("/") ? ret.Substring(0, ret.Length - 1) : ret;
            return ret;
        }

        // insert templated variable in text file
        public static void ReplaceStringInFile(string file, string search, string replace, string replacedFile)
        {
            string fullpath = HttpContext.Current.Server.MapPath(file);
            if (File.Exists(fullpath))
            {
                string content = File.ReadAllText(fullpath, System.Text.Encoding.UTF8);
                string replacedDir = Path.GetDirectoryName(replacedFile);
                if (!Directory.Exists(replacedDir))
                {
                    Directory.CreateDirectory(replacedDir);
                }

                File.WriteAllText(replacedFile, content.Replace(search, replace), System.Text.Encoding.UTF8);
            }
        }
    }
}