using System;
using System.Collections.Generic;

using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Configuration;
using Aucent.FilingServices.Data;
using XBRLReportBuilder.Utilities;
using System.Xml.Xsl;
using System.Text.RegularExpressions;

namespace XBRLReportBuilder
{
    public class CompareReports
    {
        #region members

        private DirectoryInfo baseDirs { get; set; }
        private DirectoryInfo newDirs { get; set; }
        private string reportName;
        private ReportHeader workingHeader;

        private const string mismatchRows = "Row property {0} mismatched counts: <br>Expected Results: '{1}' Actual Results: '{2}'";
        private const string mismatchCol = "Column# {0}: '{1}' has mismatched property: {2}. <br>Expected Results: '{3}' Actual Results: '{4}'";
        private const string mismatchCell = "Row# {0}:'{1}' has unmatched data.<br>Expected Results: '{2}' Actual Results: '{3}'";
        private const string mismatchColLab = "Column# {0}: '{1}' has mismatched label. <br>Expected Results: '{2}' Actual Results: '{3}'";
        private const string expectedResults = "<br>Expected Results: '{0}' Actual Results: '{1}'";

        private const string debugParameters = "Passing Parameters: BaseRep: {0}, NewRep: {1}, ErrorNum:{2}, Line Number {3}";
        private const string debugRows = "Passing Parameters: BaseRow: {0}, NewRow: {1}, ErrorNum: {2}, Line Number {3}";

        private string rFileLoc1;
        private string rFileLoc2;
                
        public StringBuilder report = new StringBuilder();

        public htmlLog html = new htmlLog(string.Format("AutoTester Results - {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm")));

        private Stopwatch swCompare = new Stopwatch();
        
        private List<InstanceReport> baseList;
        private List<InstanceReport> newList;

        private int reportID;
        private int fileID;
        private int errorID;

        private delegate void output(string message);

        #endregion
        
        public CompareReports() { }

        public CompareReports(string BasePath, string NewPath)
        {
            if (!string.IsNullOrEmpty(BasePath) 
                && !string.IsNullOrEmpty(NewPath))
            {
                //assign the variables: 
                baseDirs = new DirectoryInfo(BasePath);
                newDirs = new DirectoryInfo(NewPath);
            }
            else
                throw new ArgumentNullException("File paths must be set!");
        }

        public void Compare(bool SkipZip, bool baseHTML, out int totalRFiles, out int failedRFiles)
        {
            //Load list of files: 
            string[] files;
            string error = string.Empty;

            FileInfo[] baseRFiles = null;
            FileInfo[] newRFiles = null;

            bool reportFailed = false;

            output op = new output(Output);
            totalRFiles = 0;
            failedRFiles = 0;

            int totalFilings = 0;
            int failedFilings = 0;

            baseList = new List<InstanceReport>();
            newList = new List<InstanceReport>();
                        
            //Allows the user to skip this proccess if they're simply rerunning the comparison. 
            if (!SkipZip)
            {
                UnzipMove(baseDirs, out files, out error);
                UnzipMove(newDirs, out files, out error);
            }

            baseDirs = new DirectoryInfo(baseDirs.Parent.FullName + "\\Comparison");
            newDirs = new DirectoryInfo(newDirs.Parent.FullName + "\\Comparison");

            DateTime renderStart = new DateTime();
            DateTime renderEnd = new DateTime();

            int renderCount = 0;
            TimeSpan totalRender = GetRenderTime(newDirs, out renderStart, out renderEnd, out renderCount);
                        
            //List of unzipped instance doc directories: 
            List<DirectoryInfo> allB = new List<DirectoryInfo>( baseDirs.GetDirectories() ); 
            List<DirectoryInfo> allN = new List<DirectoryInfo>( newDirs.GetDirectories() );

            allB.Sort(new CompareFileInfo());
            allN.Sort(new CompareFileInfo());

            //Sync the resources: 
			Trace.TraceInformation( "Information: Synchronizing resources. . ." );
            XBRLReportBuilder.Utilities.RulesEngineUtils.SynchronizeResources();

            int minCount = Math.Min(allB.Count, allN.Count);

            DateTime compareStart = DateTime.Now;

            try
            {
				Trace.TraceInformation( "Information: Starting the Comparison loop." );
                for (int i = 0; i <= minCount - 1; i++)
                {
                    reportFailed = false;
                    //Proccess current directory: 
                    DirectoryInfo baseDirectory = allB.Find(ab => ab.Name == allN[i].Name);
					DirectoryInfo newDirectory = allN[ i ];

                    if (baseDirectory != null)
                    {
                        //get the Filing summary for the current filing. 
                        List<ReportHeader> baseHeaders = returnHeaders(baseDirectory);
						List<ReportHeader> newHeaders = returnHeaders( newDirectory );

                        Trace.TraceInformation("Comparing report {0}:\t\t{1} of {2}", baseDirectory.Name, (i + 1).ToString(), minCount);

                        baseRFiles = gFiles(baseDirectory, "R*.xml");
						newRFiles = gFiles( newDirectory, "R*.xml" );

                        reportName = baseDirectory.Name;
                        baseList.Clear();
                        newList.Clear();

                        try
                        {
                            //Trace.TraceInformation("Loading Files into memory.");
                            //Deserialize, load and sort the r files: 
                            foreach (FileInfo file in baseRFiles)
                            {
                                LoadFiles(file, ref baseList);
                            }

                            foreach (FileInfo file in newRFiles)
                            {
                                LoadFiles(file, ref newList);
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceInformation("The following error occured while trying to load the R files for {0}: \n{1}", baseDirectory.Name, ex.Message);
                            RLogger.Error(string.Format("The following error occured while trying to load the R files for {0}: \n{1}", baseDirectory.Name, ex.Message));
                        }

                        //baseList and newList now has the list of the R files: 

                        for (int j = 0; j <= baseList.Count - 1; j++)
                        {
                            //Trace.TraceInformation("Starting Instance Report Comparison Inner Loop.");
                            //Compare the actual R Files: 
                            swCompare.Reset();
                            swCompare.Start();

                            //find the exact R file. 
							InstanceReport currentBase = baseList[ j ];
							string baseLongName = WHITE_SPACE.Replace( currentBase.ReportLongName.Trim(), " " );
							//string baseShortName = WHITE_SPACE.Replace( currentBase.ReportName.Trim(), " " );

							InstanceReport currentNew = newList.Find(
								nl =>
									WHITE_SPACE.Replace( nl.ReportLongName.Trim(), " " ) == baseLongName );

                            //try to match by report long name if it's there. If not, try by report name
                            if (currentNew != null)
                            {
								string newLongName = WHITE_SPACE.Replace( currentNew.ReportLongName.Trim(), " " );
								string newShortName = WHITE_SPACE.Replace( currentNew.ReportName.Trim(), " " );
								if( !string.IsNullOrEmpty( newLongName ) )
                                    workingHeader = baseHeaders.Find(
										bh =>
											WHITE_SPACE.Replace( bh.LongName.Trim(), " " ) == newLongName );

								if( workingHeader == null )
                                    workingHeader = baseHeaders.Find(
										bh =>
											WHITE_SPACE.Replace( bh.ShortName.Trim(), " " ) == newShortName );

                                if (workingHeader != null && baseHTML)
                                {
									//build base html files. Skip if this already exists.
									string xmlFilename = allB[ i ].FullName + "\\" + workingHeader.XmlFileName;
									string htmlFilename = xmlFilename + ".html";
									if( !File.Exists( htmlFilename ) )
										this.HTML( currentBase, xmlFilename, htmlFilename );
                                }

                                if (currentNew != null && workingHeader != null)
                                {
                                    //html.RFile = workingHeader.XmlFileName;

									string newPath = Path.Combine( newDirectory.FullName, workingHeader.XmlFileName );
									this.rFileLoc1 = Path.Combine( newDirectory.FullName, Path.GetFileNameWithoutExtension( newPath ) + ".html" );

									string basePath = Path.Combine( baseDirectory.FullName, workingHeader.XmlFileName );
									this.rFileLoc2 = Path.Combine( baseDirectory.FullName, Path.GetFileNameWithoutExtension( basePath ) + ".html" );

									if( !this.ReportCompare( currentBase, currentNew, basePath, newPath, compareStart, out error ) )
                                    {
                                        failedRFiles++;
                                        reportFailed = true;
                                    }
                                }
                                else
                                {
                                    RLogger.Error("New report does not contain a matching report name.");
                                    htmlStarter("New report does not contain a matching report name<br>New Report does not contain a matching report name.",
                                        compareStart, out error);
                                    failedRFiles++;
                                    reportFailed = true;
                                }

                                totalRFiles++;
                                TimeSpan seconds = TimeSpan.FromMilliseconds(swCompare.ElapsedMilliseconds);
                                swCompare.Stop();
                            }
                            else //if (currentNew == null)
                            {
                                RLogger.Info("The 'Current' report was null. Setting appropriate variables and closing the current R report.");
                                //Set the variables, write the log entry and save the HTML
                                workingHeader = baseHeaders.Find(bh => bh.LongName == baseList[j].ReportLongName);

                                rFileLoc1 = allN[i].FullName + "\\" +
                                    workingHeader.XmlFileName.Remove(workingHeader.XmlFileName.Length - 4) + ".html";

                                rFileLoc2 = baseDirectory.FullName + "\\" +
                                            workingHeader.XmlFileName.Remove(workingHeader.XmlFileName.Length - 4) + ".html";

                                htmlStarter("Could not match report name for this report<br>The 'new' report name could not be matched", compareStart, out error);
                                failedRFiles++;
                                reportFailed = true;
                            }
                        }

                        //finish up the last report: 
                        if (reportFailed)
                            failedFilings++;
                        totalFilings++;

                        if (html.reportStarted)
                            html.EndReport();
                    }
                    else
                    {
                        RLogger.Error("Could not find matching directory.");
						Trace.TraceWarning( "Warning: Could not find matching directory." );
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Error: An error occured trying to compare the following report {0}: \n{1}", reportName, ex.Message);
                RLogger.Error(string.Format("An error occured trying to compare the following report {0}: \n{1}", reportName, ex.Message));
            }
            
            DateTime compareEnd = DateTime.Now;
            TimeSpan timeToCompare = compareEnd.Subtract(compareStart);
            //get render time: 

			if( html.reportStarted )
				html.EndReport();

            html.BuildSummary(compareStart, compareEnd, minCount, failedFilings, totalRFiles, failedRFiles, timeToCompare, renderStart,
                renderEnd, totalRender, renderCount);
            //string path = string.Format("{0}\\Logs\\AutoTesterLog-{1}.html", newDirs.Root.FullName, DateTime.Now.ToString("yyyy-MM-dd_hhmmss"));
            DirectoryInfo logPath = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["logPath"]);

            if (logPath.Exists)
            {
                string path = string.Format("{0}\\AutoTesterLog-{1}.html", logPath.FullName, DateTime.Now.ToString("yyyy-MM-dd_hhmmss"));
                HTML( html.html.ToString(), path);
            }
            else
            {
                Trace.TraceError("Error: Unable to write the HTML log file. Check to make sure the specified log directory exists.");
                RLogger.Error("Unable to write the HTML log file. Check to make sure the specified log directory exists.");
            }
            
        }

        private TimeSpan GetRenderTime(DirectoryInfo newDirs, out DateTime renderStart, out DateTime renderEnd, out int renderCount)
        {
			renderCount = 0;
			renderEnd = DateTime.MinValue;
			renderStart = DateTime.MinValue;

            newDirs = new DirectoryInfo(newDirs.Parent.FullName + "\\Reports");
            List<FileInfo> zipFiles = new List<FileInfo>( newDirs.GetFiles("*.zip") );
            if (zipFiles.Count > 0)
            {
				renderStart = DateTime.MaxValue;

				foreach( FileInfo z in zipFiles )
				{
					if( z.CreationTime < renderStart )
						renderStart = z.CreationTime;

					if( z.CreationTime > renderEnd )
						renderEnd = z.CreationTime;
				}

                renderCount = zipFiles.Count;
            }

			return renderEnd.Subtract(renderStart);
        }

        private FileInfo[] gFiles(DirectoryInfo directory, string filter)
        {
            FileInfo[] rFiles = directory.GetFiles(filter);
            return rFiles;
        }

        private List<ReportHeader> returnHeaders(DirectoryInfo path)
        {
            FilingSummary sum = FilingSummary.Load(path.FullName);
            List<ReportHeader> HeaderList = new List<ReportHeader>();

            if (sum != null)
            {
                ReportHeader[] header = (ReportHeader[])sum.MyReports.ToArray();
                HeaderList = new List<ReportHeader>(header);   
            }
                        
            return HeaderList;
        }

        private bool UnzipMove(DirectoryInfo directory, out string[] files, out string error)
        {
            //Directory passed should be \Reports
            bool retval = true;
            FileInfo[] zipped = directory.GetFiles("*.zip");
            files = null;
            error = string.Empty;
            string fileName = string.Empty;
            try
            {
                foreach (FileInfo zip in zipped)
                {
                    fileName = zip.Name;
                    fileName = fileName.Remove(fileName.Length - 4);
                    ZipUtilities.TryUnzipAndUncompressFiles(zip.FullName, 
                        zip.Directory.Parent.FullName + "\\Comparison\\" + fileName,
                        out files, out error);
                }
            }
            catch (System.Security.SecurityException ex)
            {
                error = "Failed proccess report with the following exception: " + ex.Message;
                RLogger.Error(error, ex);
                retval = false;
            }
            catch (UnauthorizedAccessException ex)
            {
                error = "Failed proccess report with the following exception: " + ex.Message;
                RLogger.Error(error, ex);
                retval = false;
            }
            catch (PathTooLongException ex)
            {
                error = "Failed proccess report with the following exception: " + ex.Message;
                RLogger.Error(error, ex);
                retval = false;
            }
            catch (Exception ex)
            {
                error = "Failed proccess report with the following exception: " + ex.Message;
                RLogger.Error(error, ex);
                retval = false;
            }
                return retval;
        }

        private void LoadFiles(FileInfo rFile, ref List<InstanceReport> list)
        {
            try
            {
                InstanceReport ir = InstanceReport.LoadXml(rFile.FullName);
                list.Add(ir);
            }
            catch (InvalidOperationException ex)
            {
                RLogger.Error(string.Format("Error on file {0}, file {1}.", reportName, rFile.Name));
                RLogger.Error(string.Format("Unable to deserialize the file: {0}. Error: {1}", rFile.Name, ex.Message));
            }
            catch (Exception ex)
            {
                RLogger.Error(string.Format("An error on file {0}:\n{1}", rFile.Name, ex.Message));
            }
        }

		private bool ReportCompare( InstanceReport baseRep, InstanceReport newRep,
			string basePath, string newPath,
			DateTime reportDate, out string error )
		{
			error = string.Empty;

			bool retval = true;
			if( WHITE_SPACE.Replace( baseRep.ReportName.Trim(), " " ) != WHITE_SPACE.Replace( newRep.ReportName.Trim(), " " ) )
			{
				htmlStarter( string.Format( "Report Names do not match. " + expectedResults, baseRep.ReportName, newRep.ReportName ), reportDate, out error );
				retval = false;
			}

			//If counts don't match, we can't do the cell by cell compare, skip
			if( baseRep.Rows.Count != newRep.Rows.Count )
			{
				htmlStarter( string.Format( "Row counts do not match " + expectedResults, baseRep.Rows.Count, newRep.Rows.Count ), reportDate, out error );
				retval = false;
			}

			if( baseRep.Columns.Count != newRep.Columns.Count )
			{
				htmlStarter( string.Format( "Column counts do not match " + expectedResults, baseRep.Columns.Count, newRep.Columns.Count ), reportDate, out error );
				retval = false;
			}

			if( !this.CompareRows( baseRep, newRep, reportDate, out error ) )
			{
				retval = false;
			}

			if( retval )
			{
				//Only run this if row/column counts match, otherwise, this will error out 
				for( int i = 0; i < baseRep.Rows.Count; i++ )
				{
					if( !CompareRowCells( baseRep.Rows[ i ], newRep.Rows[ i ], baseRep, newRep, basePath, newPath, i + 1, reportDate, out error ) )
						retval = false;
				}

				if( !CompareColumns( baseRep, newRep, reportDate, out error ) )
				{
					retval = false;
				}
			}

			//Compare this last. A false here won't effect the outcome of the row/column check
			if( !baseRep.RoundingOption.Equals( newRep.RoundingOption ) )
			{
				htmlStarter( string.Format( "Rounding options do not match." + expectedResults, baseRep.RoundingOption, newRep.RoundingOption ), reportDate, out error );
				//RLogger.Info(string.Format(debugParameters, baseRep.ReportLongName, newRep.ReportLongName, errorID, 397));
				using( BaselineRules baseL = new BaselineRules( baseRep, newRep, errorID ) )
				{
					baseL.EvaluateRoundingOptionError();
				}
				retval = false;
			}

			if( !retval )
			{
				//Save the baseRep and newRep HTML
				this.HTML( newRep, newPath, rFileLoc1 );
				this.HTML( baseRep, basePath, rFileLoc2 );
			}

			return retval;
		}

        private bool CompareRows(InstanceReport baseRep, InstanceReport newRep, DateTime reportDate, out string error)
        {
            error = string.Empty;
            output op = new output(Output);
            bool retval = true;
            RowCounter baseCount = BuildRowCount(baseRep);
            RowCounter newCount = BuildRowCount(newRep);

            //"Row property {0} mismatched counts: <br>Expected Results: '{1}' Actual Results: '{2}'"
            //Compare results of each dictionary: 
            foreach (string key in baseCount.Keys)
            {
                if (baseCount[key] != newCount[key])
                {
                    retval = false;
                    this.htmlStarter(string.Format(mismatchRows,
                        key, baseCount[key], newCount[key]), reportDate, out error);

                    //RLogger.Info(string.Format(debugParameters, baseRep.ReportLongName, newRep.ReportLongName, errorID, 433));
                    using (BaselineRules baseL = new BaselineRules(baseRep, newRep, errorID))
                    {
                        baseL.EvaluateIsAbstractGroupRule();
                    }

                }
            }

			if( baseRep.Rows.Count != newRep.Rows.Count )
			{
				retval = false;
				this.htmlStarter( string.Format( "Row counts do not match " + expectedResults, baseRep.Rows.Count, newRep.Rows.Count ), reportDate, out error );
			}

			int min = Math.Min( baseRep.Rows.Count, newRep.Rows.Count );
			for( int r = 0; r < min; r++ )
			{
				InstanceReportRow baseRow = baseRep.Rows[ r ];
				InstanceReportRow newRow = newRep.Rows[ r ];

				string baseLabel = WHITE_SPACE.Replace( baseRow.Label, " " );
				string newLabel = WHITE_SPACE.Replace( newRow.Label, " " );
				if( !string.Equals( baseLabel, newLabel ) )
				{
					retval = false;
					this.htmlStarter( string.Format( "Row '" + r + "' labels do not match " + expectedResults, baseLabel, newLabel ), reportDate, out error );
				}

				if( !string.Equals( baseRow.FootnoteIndexer, newRow.FootnoteIndexer ) )
				{
					retval = false;
					htmlStarter( string.Format( mismatchCell,
						r.ToString(), baseLabel, "Footnotes: " + baseRow.FootnoteIndexer, "Footnotes: " + newRow.FootnoteIndexer ), reportDate, out error );
				}
			}

			return retval;
        }

        private bool CompareRowCells(
			InstanceReportRow baseRow, InstanceReportRow newRow,
			InstanceReport baseReport, InstanceReport newReport,
			string basePath, string newPath,
            int rowCount, DateTime reportDate, out string error)
        {
            error = string.Empty;
            //Row# {0}:'{1}' has unmatched data.<br>Expected Results: '{2}' Actual Results: '{3}'
            output op = new output(Output);
            bool retval = true;
			string labelText = baseRow.Label.Replace( "\r\n", " " );


			//if (!string.Equals( baseRow.ToString(), newRow.ToString(), StringComparison.InvariantCultureIgnoreCase ) )
			//{
                for (int i = 0; i < baseRow.Cells.Count; i++)
			    {
					Cell baseCell = baseRow.Cells[i];
					Cell newCell = newRow.Cells[i];

					if( baseCell.HasEmbeddedReport == newCell.HasEmbeddedReport )
					{
						if( newCell.HasEmbeddedReport )
						{
							this.ReportCompare( baseCell.EmbeddedReport.InstanceReport, newCell.EmbeddedReport.InstanceReport,
								basePath, newPath, reportDate, out error );
							continue;
						}
					}
					else
					{
						if( baseCell.HasEmbeddedReport )
							this.htmlStarter( "Unmatched embedded reports<br>BaseRep cell has an embedded report, but NewRep cell does not.", reportDate, out error );
						else
							this.htmlStarter( "Unmatched embedded reports<br>NewRep cell has an embedded report, but BaseRep cell does not.", reportDate, out error );
					}

					string baseText = WHITE_SPACE.Replace( baseCell.ToString().Trim(), " " );
					string newText = WHITE_SPACE.Replace( newCell.ToString().Trim(), " " );
					if( !string.Equals( baseText, newText, StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        retval = false;
                        

                        htmlStarter(string.Format(mismatchCell,
                            i.ToString(), labelText, baseRow.Cells[i].ToString(), newRow.Cells[i].ToString()), reportDate, out error);

                        if (errorID > 0)
                        {
                            //RLogger.Info(string.Format(debugParameters, baseReport.ReportName, newReport.ReportName, errorID, 464));
                            using (BaselineRules baseL = new BaselineRules(baseReport, newReport, errorID))
                            {
                                baseL.EvaluateUnmatchedDataError(rowCount);
                            }    
                        }
                        else if (errorID == 0)
                        {
                            RLogger.Error("No error ID on " + baseReport.ReportName + " " + workingHeader.XmlFileName + " SQL DB returned 0");
                        }
                    }

					if( !string.Equals( baseCell.FootnoteIndexer, newCell.FootnoteIndexer ) )
					{
						retval = false;
						htmlStarter( string.Format( mismatchCell,
							i.ToString(), labelText, "Footnotes: "+ baseCell.FootnoteIndexer, "Footnotes: "+ newCell.FootnoteIndexer ), reportDate, out error );
					}
			    }
	        //}
            return retval;
        }
        
        private bool CompareColumns(InstanceReport baseRep, InstanceReport newRep, DateTime reportDate, out string error)
        {
            error = string.Empty;
            //Column# {0}: '{1}' has mismatched property: {2}. <br>Expected Results: '{3}' Actual Results: '{4}'
            output op = new output(Output);
            bool retval = true;
            int x = 0;
            string labelText1;
            string labelText2;
            for (int i = 0; i < baseRep.NumberOfCols; i++)
            {
                x = i + 1;
                if (baseRep.Columns[i].CurrencySymbol != newRep.Columns[i].CurrencySymbol)
                {
                    labelText1 = WHITE_SPACE.Replace( baseRep.Columns[i].Label.Trim(), " " );

                    htmlStarter(string.Format(mismatchCol,
                        x.ToString(), labelText1, "CurrencySymbol", baseRep.Columns[i].CurrencySymbol, newRep.Columns[i].CurrencySymbol), 
                        reportDate, out error);
                    
                    retval = false;
                }

                if (!baseRep.Columns[i].Label.Equals(newRep.Columns[i].Label))
                {
					labelText1 = WHITE_SPACE.Replace( baseRep.Columns[ i ].Label.Trim(), " " );
					labelText2 = WHITE_SPACE.Replace( newRep.Columns[ i ].Label.Trim(), " " );
                   
                    htmlStarter(string.Format(mismatchCol,
                        x.ToString(), labelText1, "Label", labelText1, labelText2), reportDate, out error);
                    retval = false;
                }
            }
            return retval;
        }

        public bool HTML(string Data, string saveAs)
        {
			File.WriteAllText( saveAs, Data );
            return true;
        }

		public bool HTML( InstanceReport report, string readFrom, string saveAs )
		{
			bool retval = true;
			try
			{
				string transformFile = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, RulesEngineUtils.TransformFile );
				if( !File.Exists( transformFile ) )
				{
					Trace.TraceError( "Error: Transform File '" + RulesEngineUtils.TransformFile + "' not found at:\n\t" + transformFile + "\nHtml Conversion aborted." );
					return false;
				}

				XslCompiledTransform transform = new XslCompiledTransform();
				transform.Load( transformFile );

				XsltArgumentList argList = new XsltArgumentList();
				argList.AddParam( "asPage", string.Empty, "true" );

				using( FileStream fs = new FileStream( saveAs, FileMode.Create, FileAccess.Write ) )
				{
					transform.Transform( readFrom, argList, fs );
				}

				string reportDirectory = Path.GetDirectoryName( saveAs );
				string styleSheetTo = Path.Combine( reportDirectory, RulesEngineUtils.StylesheetFile );
				string stylesheetFrom = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, RulesEngineUtils.StylesheetFile );
				FileUtilities.Copy( stylesheetFrom, styleSheetTo );

				string javascriptTo = Path.Combine( reportDirectory, RulesEngineUtils.JavascriptFile );
				string javascriptFrom = RulesEngineUtils.GetResourcePath( RulesEngineUtils.ReportBuilderFolder.Resources, RulesEngineUtils.JavascriptFile );
				FileUtilities.Copy( javascriptFrom, javascriptTo );
				return true;
			}
			catch( IOException ex )
			{
				RLogger.Error( string.Format( "An error occured writing the HTML file {0}. Error: {1}", report.ReportName, ex.Message ) );
				retval = false;
			}
			catch( System.Security.SecurityException ex )
			{
				RLogger.Error( string.Format( "An error occured writing the HTML file {0}. Error: {1}", report.ReportName, ex.Message ) );
				retval = false;
			}
			catch( ArgumentNullException ex )
			{
				RLogger.Error( string.Format( "An error occured writing the HTML file {0}. Error: {1}", report.ReportName, ex.Message ) );
				retval = false;
			}
			return retval;
		}

		protected static readonly Regex WHITE_SPACE = new Regex( @"(&#160;|\s)+", RegexOptions.Compiled | RegexOptions.ExplicitCapture );
        public static RowCounter BuildRowCount(InstanceReport report)
        {
            RowCounter rows = RowCounter();

            for (int i = 0; i < report.Rows.Count; i++)
            {
                if (report.Rows[i].IsAbstractGroupTitle)
                    rows["IsAbstractGroupTitle"]++;

                if (report.Rows[i].IsBaseElement)
                    rows["IsBaseElement"]++;

                if (report.Rows[i].IsBeginningBalance)
                    rows["IsBeginningBalance"]++;

                if (report.Rows[i].IsCalendarTitle)
                    rows["IsCalendarTitle"]++;

                if (report.Rows[i].IsEndingBalance)
                    rows["IsEndingBalance"]++;

                //if (report.Rows[i].IsEPS)
                //    rows["IsEPS"]++;

                if (report.Rows[i].IsEquityAdjustmentRow)
                    rows["IsEquityAdjustmentRow"]++;

                if (report.Rows[i].IsEquityPrevioslyReportedAsRow)
                    rows["IsEquityPrevioslyReportedAsRow"]++;

                //if (report.Rows[i].IsNumericDataType)
                //    rows["IsNumericDataType"]++;

                if (report.Rows[i].IsReportTitle)
                    rows["IsReportTitle"]++;

                if (report.Rows[i].IsReverseSign)
                    rows["IsReverseSign"]++;

                if (report.Rows[i].IsSegmentTitle)
                    rows["IsSegmentTitle"]++;

                if (report.Rows[i].IsSubReportEnd)
                    rows["IsSubReportEnd"]++;

                if (report.Rows[i].IsTotalLabel)
                    rows["IsTotalLabel"]++;

                if (report.Rows[i].IsTuple)
                    rows["IsTuple"]++;
            }
            return rows;
        }

        private static RowCounter RowCounter()
        {
            string[] isPropertyNames = new string[13] { 
                "IsAbstractGroupTitle", 
                "IsBaseElement", 
                "IsBeginningBalance", 
                "IsCalendarTitle",
                "IsEndingBalance", 
                "IsEquityAdjustmentRow", 
                "IsEquityPrevioslyReportedAsRow", 
                "IsReportTitle", 
                "IsReverseSign", 
                "IsSegmentTitle", 
                "IsSubReportEnd", 
                "IsTotalLabel", 
                "IsTuple" };

            RowCounter row = new RowCounter();
            foreach (string prop in isPropertyNames)
            {
                row[prop] = 0;
            }
            return row;
        }

        public void Output(string message)
        {
            html.AppendError(message);
            Trace.TraceInformation(message);
        }

		private static readonly string[] breakToken = new string[ 1 ] { "<br>" };
        private void htmlStarter(string message, DateTime reportDate, out string error)
        {
            error = string.Empty;

            string rName;
            if (this.workingHeader != null)
                rName = this.workingHeader.XmlFileName;
            else
                rName = "R9999.xml";

            string[] errorMessages = new string[2];

            try
            {
				errorMessages = message.Split( breakToken, StringSplitOptions.None );
            }
            catch (Exception ex)
            {
				error = ex.Message;
			}


			if( this.reportName != this.html.ReportName )
			{
				if( this.html.htmlStarted )
				{
					// close old, start new, update html instance
					this.html.EndReport();
				}

				this.html.ReportName = this.reportName;
				this.html.RFile = rName;
			}

            if (this.reportName == html.ReportName)
            {
				//Run the existing report
                if (!html.reportStarted)
                {
                    //Report not started yet: 
                    html.StartReport(reportName, rName, rFileLoc1, rFileLoc2);
                    html.AppendError(message);
                    html.reportStarted = true;

                    //Log a new report: 
                    reportID = SQLLog.AddReport(reportName, reportDate, out error);
                    fileID = SQLLog.AddFile(reportID, rName, rFileLoc1, rFileLoc2, out error);

					if( errorMessages.Length > 1 )
						errorID = SQLLog.AddError(fileID, errorMessages[0], errorMessages[1], out error);
                }
                else
                {
					//update an error list item
                    //check for new r file: 
                    if (rName == html.RFile)
                    {
                        //r file exists: 
                        html.AppendError(message);
                        errorID = SQLLog.AddError(fileID, errorMessages[0], errorMessages[1], out error);
                    }
                    else
                    {
                        // r file doesn't exist: 
                        html.NewRFile(rName, rFileLoc1, rFileLoc2);
                        html.AppendError(message);
                        html.RFile = rName;
                        fileID = SQLLog.AddFile(reportID, rName, rFileLoc1, rFileLoc2, out error);
                        errorID = SQLLog.AddError(fileID, errorMessages[0], errorMessages[1], out error);
                    }
                }
            }
        }
    }

    public class RowCounter : Dictionary<string, int>
    {//summary: Keep track of the row property and its count
      
        public RowCounter() 
        {
            
        }
    }

    public class CompareFileInfo : IComparer<DirectoryInfo>
    {
        public int Compare(DirectoryInfo d1, DirectoryInfo d2)
        {
            return (string.Compare(d1.Name, d2.Name));
        }
    }

}
