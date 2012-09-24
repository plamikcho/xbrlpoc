using System;
using System.Collections.Generic;

using System.Text;
using System.Configuration;

namespace XBRLReportBuilder
{
    public class htmlLog
    {
        public StringBuilder html;

        private string url = ConfigurationManager.AppSettings["urlKey"];

        public bool htmlStarted { get; protected set; }
        public bool reportStarted { get; set; }
        public string ReportName { get; set; }
        public string RFile { get; set; }

        private const string beginReport = @"
<tr>
	<td>Error on Report {0}: <br />
		File {1}: <a href=""{4}{2}"">Development R File</a>, <a href=""{4}{3}"">Base R File</a>
		<ul>";

        private const string endReport = @"
		</ul>
	</td>
</tr>";

        private const string newRFile = @"
		</ul><br/>
		File {0}: <a href=""{3}{1}"">Development R File</a>, <a href=""{3}{2}"">Base R File </a>
		<ul>";

        private const string listItem = @"
			<li>{0}</li>";

        private const string buildSummary = @"
<tr>
    <td>
        -----Summary-----<br />
    <ul>
        <li>Comparison started: {0}</li>
        <li>Comparison finished: {1}</li>
        <li>Filings compared: {2} ({3} completed, {4} failed)</li>
        <li>R Files compared: {5} ({6} completed, {7} failed)</li>
        <li>Time to compare: {8} (Average {9} per report)</li>
        <li>Render started: {10}</li>
        <li>Render ended: {11}</li>
        <li>Total time to render: {12} (Average {13} per filing)</li>
    </ul>
    </td>
</tr>
</table>
</body>
</html>";

        private const string startHTML = ""+
@"<html>
<head> 
	<title>{0}</title>
</head>
<body>
<table border=""1"">
<tr>
	<td>-----Starting New Comparison-----</td>
</tr>";


        public htmlLog(string compareTitle)
        {
            html = new StringBuilder(string.Format(startHTML, compareTitle));
        }
        
        /// <summary>
        /// starts tr td and ul block. Add li AFTER this.
        /// </summary>
        /// <param name="reportTitle">Report Name: eg aci-20090930</param>
        /// <param name="rFile">R file number: eg R15</param>
        /// <param name="baseFilePath">Full path to the base file HTML </param>
        /// <param name="newFilePath">Full path to the new file HTML</param>
        public void StartReport(string reportTitle, string rFile, string baseFilePath, string newFilePath)
        {
            Uri baseP = new Uri(baseFilePath);
            Uri newP = new Uri(newFilePath);

            html.Append(string.Format(beginReport, reportTitle, rFile, baseP.AbsolutePath, newP.AbsolutePath, url));
			this.reportStarted = true;
        }

        public void NewRFile(string rFile, string baseFilePath, string newFilePath)
        {
            Uri baseP = new Uri(baseFilePath);
            Uri newP = new Uri(newFilePath);

            html.Append(string.Format(newRFile, rFile, baseP.AbsolutePath, newP.AbsolutePath, url));
        }

        public void AppendError(string item)
        {
            html.Append(string.Format(listItem, item));
        }

        public void EndReport()
        {
			this.reportStarted = false;
            html.Append(endReport);
        }

        /// <summary>
        /// Appends the summary and closes the HTML string
        /// </summary>
        /// <param name="compareStart">Start time of the comparison</param>
        /// <param name="compareFinish">End time of the comparison</param>
        /// <param name="filingsCompared">Total number of filings compared</param>
        /// <param name="filingsFailed">Number of failed filings</param>
        /// <param name="rFilesCompared">Total number of R files compared</param>
        /// <param name="rFilesFailed">Number of failed R files</param>
        /// <param name="timeToCompare">Total time to complete the comparison</param>
        public void BuildSummary(DateTime compareStart, DateTime compareFinish, int filingsCompared, 
            int filingsFailed ,int rFilesCompared, int rFilesFailed, TimeSpan timeToCompare, 
            DateTime renderStart, DateTime renderEnd, TimeSpan totalRender, int renderCount)
        {
            int filingsComplete = (filingsCompared - filingsFailed);
            int rfilesComplete = (rFilesCompared - rFilesFailed);
            double secondsPer = (filingsCompared / timeToCompare.TotalSeconds);
            double secondsPerRender = (totalRender.TotalSeconds / filingsCompared);

            html.Append(string.Format(buildSummary, 
                compareStart,                   //0
                compareFinish,                  //1
                filingsCompared,                //2
                filingsComplete,                //3
                filingsFailed,                  //4
                rFilesCompared,                 //5
                rfilesComplete,                 //6
                rFilesFailed,                   //7
                timeToCompare.TotalSeconds,     //8
                secondsPer.ToString(),          //9
                renderStart,                    //10
                renderEnd,                      //11
                totalRender.TotalSeconds,       //12
                secondsPerRender));             //13
        }
    }
}

        //<li>Comparison started: {0}</li>
 //       <li>Comparison finished: {1}</li>
 //       <li>Filings compared: {2} ({3} completed, {4} failed)</li>
 //       <li>R Files compared: {5} ({6} completed, {7} failed)</li>
 //       <li>Time to compare: {8} (Average {9} per report)</li>
 //       <li>Render started: {10}</li>
 //       <li>Render ended: {11}</li>
 //       <li>Total time to render: {12} (Average {13} per filing)</li>