using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Aucent.MAX.AXE.XBRLReportBuilder;
using Aucent.FilingServices.Data;

namespace XBRLReportBuilder
{
    public class Filings : IDisposable
    {
        ///<summary>
        ///1. Read all of the filings in the 'unprocessed dir' (DirectoryInfo)
        ///2. Delete the extraneous files from each directory
        ///3. Zip the contents of each directory, save the ZIP to the proper location
        ///</summary>

        #region members

        private DirectoryInfo inputPath { get; set; }
        private DirectoryInfo outputPath { get; set; }

        private delegate void output(string message);

        private const string zipSave = @"{0}{1}.zip";
        private const string defnref = "defnref.xml";
        private const string FilingSummary = "FilingSummary.xml";
        private static string rFileSearch = @"^R\d+\.xml$";
        private static string filingSearch = @"\w+-\d+\.xml$";

        private CompareReports compare = new CompareReports();

        #endregion

        /// <summary>
        /// Deletes extraneous files from the filing directories, 
        /// zips then copies the archives to the output path. 
        /// </summary>
        /// <param name="InputPath">InputPath: Specify the top level containing the filings that need to be fixed.</param>
        /// <param name="OutputPath">OutputPath: Specify where the finished ZIP files should be placed. </param>
        
        public Filings(string InputPath, string OutputPath)
        {
            if (!string.IsNullOrEmpty(InputPath))
                inputPath = new DirectoryInfo(InputPath);
            else
                throw new ArgumentNullException("Must specify an input path.");

            if (!string.IsNullOrEmpty(OutputPath))
                outputPath = new DirectoryInfo(OutputPath);
            else
                throw new ArgumentNullException("Must specify an output path.");
        }

        public bool ProcessNewFilings()
        {
            StringBuilder sb = new StringBuilder("Building ZIP files:");
            bool retval = true;
            string filingName = string.Empty;
            //files should start in 1_IncomingFilings, each directory will be scanned, cleaned and zipped.
            //Scan dirs:
            DirectoryInfo[] filings = inputPath.GetDirectories();
            foreach (DirectoryInfo directory in filings)
            {
                sb.Append(directory.Name + Environment.NewLine);
                //delete extraneous files
                DeleteFiles(directory, out filingName);

                //zip files and move the zip:
                ZipFiles(directory, string.Format(zipSave, outputPath, filingName));
                
            }
            //writeTxt(sb.ToString());
            return retval;
        }

        public void Dispose()
        { }

        private void ZipFiles(DirectoryInfo directory, string zipName)
        {
            output op = new output(compare.Output);
            string[] fileNames = BuildString(directory.GetFiles());

            try
            {
                ZipUtilities.TryZipAndCompressFiles(zipName, directory.FullName, fileNames);
            }
            catch (Exception ex)
            {
                 op(string.Format(Program.zipCopyErr, zipName, ex.Message)); 
            }
        }

        private void DeleteFiles(DirectoryInfo directory, out string filingName)
        {
            FileInfo[] allFiles = directory.GetFiles();
            string[] temp = new string[2];
            filingName = string.Empty;
            output op = new output(compare.Output);

            foreach (FileInfo file in allFiles)
            {//Delete the files
                if (Regex.IsMatch(file.Name, defnref)
                    || Regex.IsMatch(file.Name, rFileSearch))
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (IOException ex)
                    {
                        op(string.Format(Program.fileDelErr, file.Name, ex.Message));      
                    }
                    catch (System.Security.SecurityException ex)
                    {
                        op(string.Format(Program.fileDelErr, file.Name, ex.Message));
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        op(string.Format(Program.fileDelErr, file.Name, ex.Message));
                    }
                }
            }

            foreach (FileInfo file in allFiles)
            {//Grab the name of the instance doc
                if (Regex.IsMatch(file.Name, filingSearch))
                {
                    Match match = Regex.Match(file.Name, filingSearch);
                    filingName = match.Value.Remove(match.Value.Length - 4) + "_" + file.Directory.Name;
                    break;
                }
            }
        }

        //Helper Methods
        private string[] BuildString(FileInfo[] files)
        {
            string[] fileNames = new string[files.Length];

            for (int i = 0; i <= files.Length - 1; i++)
            {
                fileNames[i] = files[i].Name;
            }

            return fileNames;
        }

        private void writeTxt(string file)
        {
            using (FileStream fs = new FileStream(@"\\srv_dev_db.aucent.local\TestAutomation\Written.txt", FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(file);
                }
            }
        }
    }
}
