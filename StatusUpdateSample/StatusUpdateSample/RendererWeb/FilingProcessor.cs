using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using System.Configuration;
using System.Net.Cache;
using System.Diagnostics;
using XRB = XBRLReportBuilder;
using XBRLReportBuilder;
using XBRLReportBuilder.Utilities;
using Aucent.FilingServices.Data;


namespace Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilderRenderer
{
	public class FilingProcessor
	{
		public const string INSTANCE_COMMAND = "Instance";
		public const string REPORTS_FOLDER_COMMAND = "ReportsFolder";
		public const string REPORT_FORMAT_COMMAND = "ReportFormat";
		public const string HTML_REPORT_FORMAT_COMMAND = "HtmlReportFormat";
		public const string REMOTE_CACHE_POLICY_COMMAND = "RemoteFileCachePolicy";
		public const string QUIET_COMMAND = "Quiet";
		public const string SAVEAS_COMMAND = "SaveAs";
        public const string XSLT_STYLESHEET_COMMAND = "XSLT";

		public static readonly string bin = null;
		public static readonly string cd = null;
		public List<Filing> Filings = new List<Filing>();

		#region private members

        private const string separator = "->";

        // plamen
        private UpdateStatusExecutor executor;

		private string _currencyMappingFile = null;
		public string CurrencyMappingFile
		{
			get
			{
				if( this._currencyMappingFile == null )
				{
					string tmp = ConfigurationManager.AppSettings[ "CurrencyMappingFile" ] as string;
					if( !string.IsNullOrEmpty( tmp ) )
					{
						tmp = Path.Combine( bin, tmp );
						if( File.Exists( tmp ) )
						{
							this._currencyMappingFile = tmp;
							executor.Invoke( "Configuration: Setting CurrencyMappingFile to " + tmp + "." );
						}
					}
				}

				return this._currencyMappingFile;
			}
		}

		private HtmlReportFormat _htmlReportFormat = HtmlReportFormat.None;
		private HtmlReportFormat HtmlReportFormat
		{
			get
			{
				if( this._htmlReportFormat == HtmlReportFormat.None )
				{
					string format = ConfigurationManager.AppSettings[ HTML_REPORT_FORMAT_COMMAND ] as string;
					this.SetHtmlReportFormat( format, "Configuration" );
				}

				if( this._htmlReportFormat == HtmlReportFormat.None )
					this._htmlReportFormat = HtmlReportFormat.Complete;

				return this._htmlReportFormat;
			}
			set { this._htmlReportFormat = value; }
		}

		private bool _reportsFolderChecked = false;
		protected string _reportsFolder = "Reports";
		private string ReportsFolder
		{
			get
			{
				if( !this._reportsFolderChecked )
				{
					this._reportsFolderChecked = true;
					string folder = ConfigurationManager.AppSettings[ REPORTS_FOLDER_COMMAND ] as string;
					this.SetReportsFolder( folder, "Configuration" );
				}

				return this._reportsFolder;
			}
			set
			{
				this._reportsFolderChecked = true;

				value = value ?? string.Empty;
				if( string.IsNullOrEmpty( value.Trim() ) )
					this._reportsFolder = "Reports";
				else
					this._reportsFolder = value;
			}
		}

		private ReportFormat _reportFormat = ReportFormat.None;
		private ReportFormat ReportFormat
		{
			get
			{
				if( this._reportFormat == ReportFormat.None )
				{
					string format = ConfigurationManager.AppSettings[ REPORT_FORMAT_COMMAND ] as string;
					this.SetReportFormat( format, "Configuration" );
				}

				if( this._reportFormat == ReportFormat.None )
					this._reportFormat = ReportFormat.Xml;

				return this._reportFormat;
			}
			set { this._reportFormat = value; }
		}

		private bool _remoteFileCachePolicyChecked = false;
		private RequestCacheLevel _remoteFileCachePolicy = RequestCacheLevel.Default;
		private RequestCacheLevel RemoteFileCachePolicy
		{
			get
			{
				if( !this._remoteFileCachePolicyChecked )
				{
					this._remoteFileCachePolicyChecked = true;
					string format = ConfigurationManager.AppSettings[ REMOTE_CACHE_POLICY_COMMAND ] as string;
					this.SetRemoteFileCachePolicy( format, "Configuration" );
				}

				return this._remoteFileCachePolicy;
			}
			set
			{
				this._remoteFileCachePolicyChecked = true;
				this._remoteFileCachePolicy = value;
			}
		}

		private Filing.SaveAs SaveAs = Filing.SaveAs.Auto;

        private String XsltStylesheetPath = String.Empty;

		private bool Quiet = false;

		#endregion

		public FilingProcessor(UpdateStatusExecutor executor)
		{
            // plamen
            this.executor = executor;
			string tmp = this.ReportsFolder;
			tmp = this.ReportFormat.ToString();
			tmp = this.HtmlReportFormat.ToString();
			tmp = this.CurrencyMappingFile;
			tmp = this.RemoteFileCachePolicy.ToString();
		}

		public static FilingProcessor Load(UpdateStatusExecutor executor, params string[] args)
		{
			FilingProcessor fp = new FilingProcessor(executor);
            //return fp;

			foreach( string arg in args )
			{
				if( !arg.StartsWith( "/" ) )
				{
					executor.Invoke( "Ignoring parameter.  Reason: incorrect format." + separator +
						"\t" + arg +"" );
					continue;
				}

				int commandEnd = arg.IndexOf( '=' );
				bool isCommandOnly = commandEnd == -1;
				string command = isCommandOnly ? arg.Substring( 1 ) : arg.Substring( 1, commandEnd - 1 );
				if( isCommandOnly && !string.Equals( command, QUIET_COMMAND ) )
				{
					executor.Invoke( "Ignoring parameter.  Reason: incorrect format." + separator + "\t" + arg + "" );
					continue;
				}

				string value = arg.Substring( commandEnd + 1 );
				switch( command )
				{
					case INSTANCE_COMMAND:
						Filing filing = new Filing(executor, value );
						fp.Filings.Add( filing );
						break;
					case REPORTS_FOLDER_COMMAND:
						fp.SetReportsFolder( value, "Arguments" );
						break;
					case REPORT_FORMAT_COMMAND:
						fp.SetReportFormat( value, "Arguments" );
						break;
					case HTML_REPORT_FORMAT_COMMAND:
						fp.SetHtmlReportFormat( value, "Arguments" );
						break;
					case REMOTE_CACHE_POLICY_COMMAND:
						fp.SetRemoteFileCachePolicy( value, "Arguments" );
						break;
					case QUIET_COMMAND:
						fp.Quiet = true;
						break;
					case SAVEAS_COMMAND:
						fp.SetSaveAs( value, "Arguments" );
						break;
                    case XSLT_STYLESHEET_COMMAND:
                        fp.SetXsltStylesheetPath( value, "Arguments" );
                        break;
					default:
						executor.Invoke( "Arguments: Ignoring parameter " + command + ".  Reason: unrecognized command " + command + "" + separator +
							"\t" + arg + "" );
						break;
				}
			}

			return fp;
		}

		static FilingProcessor()
		{
			bin = AppDomain.CurrentDomain.BaseDirectory;
			cd = Environment.CurrentDirectory;

			RulesEngineUtils.SetBaseResourcePath( bin );
			XRB.ReportBuilder.SetSynchronizedResources( true );
		}

        public void TestProcess(params string[] args)
        {
            executor.Invoke("File " + args[0] + " processing started");
            System.Threading.Thread.Sleep(2000);

            for (int i = 0; i < 20; i++)
            {
                executor.Invoke(string.Format("Step\t {0} complete", i));
                System.Threading.Thread.Sleep(200);
            }
        }

		public void ProcessFilings()
		{
			if( this.Filings == null || this.Filings.Count == 0 )
			{
				executor.Invoke( "Error: No filings were loaded.  Exiting." );
				return;
			}

			foreach( Filing f in this.Filings )
			{
				this.ProcessFiling( f, this.Filings.Count > 1 );
			}
		}

		public void ProcessFiling( Filing f, bool isMultipleFilings )
		{
			executor.Invoke( "Information: Preparing " + f.InstancePath );

			if( !File.Exists( f.InstancePath ) )
			{
				executor.Invoke( "Error: Instance document not found at " + f.InstancePath + "." );
				executor.Invoke( "\tSkipping filing: " + f.InstancePath );
				return;
			}

			Filing.SaveAs saveAs = Filing.SaveAs.Auto;
			bool isZip = string.Equals( Path.GetExtension( f.InstancePath ), ".zip", StringComparison.CurrentCultureIgnoreCase );
			if( isZip )
			{
				saveAs = Filing.SaveAs.Zip;
				if( this.SaveAs == Filing.SaveAs.Xml )
					saveAs = Filing.SaveAs.Xml;
			}
			else
			{
				saveAs = Filing.SaveAs.Xml;
				if( this.SaveAs == Filing.SaveAs.Zip )
					saveAs = Filing.SaveAs.Zip;
			}

			string outputPath;
			string basePath = Path.GetDirectoryName( f.InstancePath );
			if( !this.GetOutputPath( basePath, saveAs, isMultipleFilings, f, out outputPath ) )
				return;

			string tmpPath = Path.Combine( Path.GetTempPath(), Path.GetFileNameWithoutExtension( f.InstancePath ) );
			if( !Directory.Exists( tmpPath ) )
				Directory.CreateDirectory( tmpPath );
			else
				CleanPath( tmpPath );

			try
			{
				if( isZip )
				{
					string[] packageFiles;
					if( !UnzipPackage(executor, f, tmpPath, out packageFiles ) )
						return;

					string taxonomy;
					bool instanceFound = false;
					foreach( string file in packageFiles )
					{
						if( IsInstance( file, out taxonomy ) )
						{
							instanceFound = true;
							f.InstancePath = file;
							f.TaxonomyPath = taxonomy;
							break;
						}
					}

					if( !instanceFound )
					{
						executor.Invoke( "Error: The ZIP file does not contain an instance document." );
						executor.Invoke( "\tSkipping filing: " + f.InstancePath );
						return;
					}
				}
				else
				{
					string taxonomyPath;
					if( !this.IsInstance( f.InstancePath, out taxonomyPath ) )
					{
						executor.Invoke( "Error: Instance document not found at " + f.InstancePath + "." );
						executor.Invoke( "\tSkipping filing: " + f.InstancePath );
						return;
					}

					f.TaxonomyPath = taxonomyPath;
				}
				string error;
				FilingSummary fs;
				string reportsPath = isZip ? Path.Combine( tmpPath, "Reports" ) : tmpPath;
				string fsPath = Path.Combine( reportsPath, FilingSummary.FilingSummaryXmlName );

				XRB.ReportBuilder rb = new XRB.ReportBuilder(executor, RulesEngineUtils.DefaultRulesFile, this.ReportFormat, this.HtmlReportFormat );
				rb.CurrencyMappingFile = this.CurrencyMappingFile;
				rb.RemoteFileCachePolicy = this.RemoteFileCachePolicy;
                rb.XsltStylesheetPath = this.XsltStylesheetPath;

				if( !rb.BuildReports( f.InstancePath, f.TaxonomyPath, fsPath, reportsPath, out fs, out error ) )
				{
					executor.Invoke( "Unexpected error: " + error );
					executor.Invoke( "\tFailed filing: " + f.InstancePath );
				}

				if( saveAs == Filing.SaveAs.Xml )
				{
					foreach( string copyFrom in Directory.GetFiles( reportsPath ) )
					{
						string copyTo = Path.Combine( outputPath, Path.GetFileName( copyFrom ) );
						File.Copy( copyFrom, copyTo );
					}
				}
				else
				{
					string[] reports = Directory.GetFiles( reportsPath );
					this.ZipReports( outputPath, reports );
				}

				executor.Invoke( "Information: Reports successfully created." );
				executor.Invoke( "\t" + ( fs.MyReports.Count - 1 ) + " reports created at " + outputPath + "." );
                if (string.IsNullOrEmpty(error))
                {
                    executor.Invoke("reload");
                }
			}
			catch( Exception ex )
			{
				executor.Invoke( "Unexpected error: " + ex.Message );
			}
			finally
			{
				CleanPath( tmpPath );
			}
		}

		#region private methods

		private static bool CleanPath( string path )
		{
			try
			{
				foreach( string file in Directory.GetFiles( path, "*", SearchOption.AllDirectories ) )
				{
					File.Delete( file );
				}

				return true;
			}
			catch { }

			return false;
		}

        // plamen
		private bool CleanOrCreateReportsPath( string reportsPath, Filing f )
		{
			bool deleted = true;
			if( Directory.Exists( reportsPath ) )
			{
				string[] files = Directory.GetFiles( reportsPath );
				if( files.Length == 0 )
					return true;

                //if( this.Quiet )
                //{
                //    Console.WriteLine( "Information: The destination reports folder contains other files.  Prompt suppressed." );
                //    Console.WriteLine( "\tSkipping filing: " + f.InstancePath );
                //    return false;
                //}

				deleted = false;
				bool canDeleteOrReplace = true; // plamen
                //if( !canDeleteOrReplace )
                //{
                //    bool validSelection = false;
                //    while( !validSelection )
                //    {
                //        Console.WriteLine();
                //        Console.Write( "The destination reports folder contains other files.  Overwrite these files? [Y/N] " );
                //        ConsoleKeyInfo keyInfo = Console.ReadKey( false );

                //        switch( keyInfo.Key )
                //        {
                //            case ConsoleKey.Y:
                //                canDeleteOrReplace = true;
                //                goto case ConsoleKey.N;
                //            case ConsoleKey.N:
                //                validSelection = true;
                //                break;
                //        }
                //    }
                //    Console.WriteLine();
                //}

				if( !canDeleteOrReplace )
					return false;

				deleted = CleanPath( reportsPath );
			}

			if( !deleted )
			{
				executor.Invoke( "Error: Reports folder already exists and cannot delete or replace files." );
				executor.Invoke( "\tReports folder: " + reportsPath );
				executor.Invoke( "\tSkipping filing: " + f.InstancePath );
				return false;
			}

			if( !Directory.Exists( reportsPath ) )
			{
				try
				{
					Directory.CreateDirectory( reportsPath );
				}
				catch
				{
					executor.Invoke( "Error: Reports folder does not exist and cannot be created." );
					executor.Invoke( "\tReports folder: " + reportsPath );
					executor.Invoke( "\tSkipping filing: " + f.InstancePath );
					return false;
				}
			}

			return true;
		}

		private bool CleanOrOverwriteFile( string filePath, Filing f )
		{
			if( !File.Exists( filePath ) )
				return true;

            //if( this.Quiet )
            //{
            //    executor.Invoke( "Information: The destination reports zip already exists.  Prompt suppressed." );
            //    executor.Invoke( "\tSkipping filing: " + f.InstancePath );
            //    return false;
            //}

            //bool validSelection = false;
            //while( !validSelection )
            //{
            //    Console.WriteLine();
            //    Console.Write( "The destination zip already exists.  Overwrite this file? [Y/N] " );

            //    ConsoleKeyInfo keyInfo = Console.ReadKey( false );
            //    switch( keyInfo.Key )
            //    {
            //        case ConsoleKey.Y:
            //            validSelection = true;
            //            File.Delete( filePath );
            //            Console.WriteLine();
            //            break;
            //        case ConsoleKey.N:
            //            Console.WriteLine();
            //            Console.Write( "Information:  The user selected not to overwrite destination zip." );
            //            Console.WriteLine( "\tSkipping filing: " + f.InstancePath );
            //            return false;
            //    }
            //}

            File.Delete(filePath);
			return true;
		}

		private static string CopyFromWeb( string instancePath )
		{
			string tmpPath = Path.GetTempFileName();

			WebClient client = new WebClient();
			client.DownloadFile( instancePath, tmpPath );

			return tmpPath;
		}

		private static string FixTaxonomyReference( string referenceFile, string localFile )
		{
			if( IsWebFile( localFile ) )
				return localFile;

			string baseHRef = GetBaseHRef( referenceFile );
			string newReference = baseHRef + localFile;
			return newReference;
		}

		private static string GetBaseHRef( string webFile )
		{
			int len = webFile.Length - Path.GetFileName( webFile ).Length;
			string baseHRef = webFile.Substring( 0, len );
			return baseHRef;
		}

		private bool GetOutputPath( string basePath, Filing.SaveAs saveAs, bool isMultiple, Filing f, out string outputPath )
		{
			outputPath = null;

			if( Path.IsPathRooted( this.ReportsFolder ) )
			{
				if( !Directory.Exists( this.ReportsFolder ) )
				{
					executor.Invoke( "Error: The ReportsFolder does not exist at " + this.ReportsFolder + "." );
					executor.Invoke( "\tSkipping filing: " + f.InstancePath );
					return false;
				}

				string tmpPath = this.ReportsFolder.TrimEnd( '\\' );
				if( string.Equals( basePath, tmpPath, StringComparison.CurrentCultureIgnoreCase ) )
				{
					executor.Invoke( "Error: The ReportsFolder matches the Filing folder:" );
					executor.Invoke( "\tReportsFolder: " + this.ReportsFolder );
					executor.Invoke( "\tFiling folder: " + basePath );
					executor.Invoke( "\tSkipping filing: " + f.InstancePath );
					return false;
				}

				if( saveAs == Filing.SaveAs.Xml )
				{
					string xmlPath = Path.Combine( this.ReportsFolder, Path.GetFileNameWithoutExtension( f.InstancePath ) );
					if( !this.CleanOrCreateReportsPath( xmlPath, f ) )
						return false;

					outputPath = xmlPath;
					return true;
				}
				else
				{
					string zipPath = Path.Combine( this.ReportsFolder, Path.GetFileNameWithoutExtension( f.InstancePath ) + ".zip" );
					if( !this.CleanOrOverwriteFile( zipPath, f ) )
						return false;

					outputPath = zipPath;
					return true;
				}
			}
			else
			{
				if( saveAs == Filing.SaveAs.Xml )
				{
					string xmlPath = Path.Combine( basePath, this.ReportsFolder );
					if( !Directory.Exists( xmlPath ) )
						Directory.CreateDirectory( xmlPath );

					if( isMultiple )
						xmlPath = Path.Combine( xmlPath, Path.GetFileNameWithoutExtension( f.InstancePath ) );

					if( !this.CleanOrCreateReportsPath( xmlPath, f ) )
						return false;

					outputPath = xmlPath;
					return true;
				}
				else
				{
					string zipPath = Path.Combine( basePath, this.ReportsFolder );
					if( !Directory.Exists( zipPath ) )
						Directory.CreateDirectory( zipPath );

					zipPath = Path.Combine( zipPath, Path.GetFileNameWithoutExtension( f.InstancePath ) + ".zip" );
					if( !this.CleanOrOverwriteFile( zipPath, f ) )
						return false;

					outputPath = zipPath;
					return true;
				}
			}
		}

		private bool IsInstance( string file, out string taxonomy )
		{
			taxonomy = string.Empty;

			if( !file.StartsWith( "http" ) && !File.Exists( file ) )
				return false;

			if( string.Equals( Path.GetExtension( file ), ".xbrl", StringComparison.CurrentCultureIgnoreCase ) &&
				string.Equals( Path.GetExtension( file ), ".xml", StringComparison.CurrentCultureIgnoreCase ) )
			{
				executor.Invoke( "Error: The file " + file + " does not appear to be an instance document.  Reason: unrecognized extension.  Expected xbrl or xml." );
				return false;
			}

			string readFile = file;
			if( file.StartsWith( "http", StringComparison.CurrentCultureIgnoreCase ) )
			{
				readFile = Path.GetTempFileName();

				WebClient cli = new WebClient();
				cli.CachePolicy = new RequestCachePolicy( this.RemoteFileCachePolicy );
				cli.DownloadFile( file, readFile );
			}

			try
			{
				XmlTextReader doc = new XmlTextReader( readFile );

				doc.MoveToContent();
				if( doc.LocalName != "xbrl" )
				{
					executor.Invoke( "Error: The file " + file + " does not appear to be an instance document.  Reason: the root element is not xbrl." );
					return false;
				}

				List<string> foundTaxonomies = new List<string>();
				int count = 0;
				while( doc.Read() && count++ < 10 )
				{
					if( doc.NodeType != XmlNodeType.Element )
						continue;

					if( doc.LocalName != "schemaRef" )
						continue;

					while( doc.MoveToNextAttribute() )
					{
						if( doc.NodeType != XmlNodeType.Attribute || doc.Name != "xlink:href" )
							continue;

						//this one is prefered because it exists in the same folder
						if( doc.Value == Path.GetFileName( doc.Value ) )
						{
							taxonomy = FixTaxonomyReference( file, doc.Value );
							return true;
						}
						else
						{
							foundTaxonomies.Add( doc.Value );
						}
					}
				}

				if( foundTaxonomies.Count > 0 )
				{
					taxonomy = foundTaxonomies[ 0 ];
					return true;
				}
			}
			catch(Exception ex )
			{
				executor.Invoke( "Unexpected error: The file " + file + " does not appear to be an instance document.  Reason: " + ex.Message );
				return false;
			}

			executor.Invoke( "Error: The file " + file + " does not appear to be an instance document.  Reason: the taxonomys xlink:href could not be found." );
			return false;
		}

		private static bool IsWebFile( string testFile )
		{
			if( testFile.StartsWith( "http:", StringComparison.CurrentCultureIgnoreCase ) )
				return true;

			if( testFile.StartsWith( "https:", StringComparison.CurrentCultureIgnoreCase ) )
				return true;

			return false;
		}

		private bool SetHtmlReportFormat( string value, string setter )
		{
			try
			{
				this.HtmlReportFormat = (HtmlReportFormat)Enum.Parse( typeof( HtmlReportFormat ), value );
				executor.Invoke( setter + ": Setting " + HTML_REPORT_FORMAT_COMMAND + " to " + value + "." );
				return true;
			}
			catch { }

			string[] expected = Enum.GetNames( typeof( HtmlReportFormat ) );
			executor.Invoke( setter + ": Ignoring parameter " + HTML_REPORT_FORMAT_COMMAND + ".  Reason: unrecognized value " + value + "" + separator +
				"\tExpected one of the following: " + string.Join( ", ", expected ) );
			return false;
		}

		private bool SetReportsFolder( string value, string setter )
		{
			if( Path.IsPathRooted( value ) )
			{
				if( Directory.Exists( value ) )
				{
					this.ReportsFolder = value;
					executor.Invoke( setter + ": Setting " + REPORTS_FOLDER_COMMAND + " to absolute path " + this.ReportsFolder + "." );
					return true;
				}
				else
				{
					executor.Invoke( setter + ": Ignoring parameter " + REPORTS_FOLDER_COMMAND + ".  Reason: Absolute path " + value + " does not exist." );
					return false;
				}
			}
			else
			{
				string tmp = Path.GetDirectoryName( value );
				if( string.IsNullOrEmpty( tmp ) )
				{
					this.ReportsFolder = value;
					executor.Invoke( setter + ": Setting " + REPORTS_FOLDER_COMMAND + " to relative path " + this.ReportsFolder + "." );
					return true;
				}
				else
				{
					executor.Invoke( setter + ": Ignoring parameter " + REPORTS_FOLDER_COMMAND + ".  Reason: Relative path " + value + " may not contain multiple directories." );
					return false;
				}
			}
		}

		private bool SetReportFormat( string value, string setter )
		{
			try
			{
				this.ReportFormat = (ReportFormat)Enum.Parse( typeof( ReportFormat ), value );
				executor.Invoke( setter + ": Setting " + REPORT_FORMAT_COMMAND + " to " + value + "." );
				return true;
			}
			catch { }

			string[] expected = Enum.GetNames( typeof( ReportFormat ) );
			executor.Invoke( setter + ": Ignoring parameter `" + REPORT_FORMAT_COMMAND + "`.  Reason: unrecognized value `" + value + "`" + separator +
				"\tExpected one of the following: " + string.Join( ", ", expected ) );
			return false;
		}

		private bool SetRemoteFileCachePolicy( string value, string setter )
		{
			try
			{
				this.RemoteFileCachePolicy = (RequestCacheLevel)Enum.Parse( typeof( RequestCacheLevel ), value );
				executor.Invoke( setter + ": Setting `" + REMOTE_CACHE_POLICY_COMMAND + "` to `" + value + "`." );
				return true;
			}
			catch { }

			string[] expected = Enum.GetNames( typeof( RequestCacheLevel ) );
			executor.Invoke( setter + ": Ignoring parameter `" + REMOTE_CACHE_POLICY_COMMAND + "`.  Reason: unrecognized value `" + value + "`" + separator +
				"\tExpected one of the following: " + string.Join( ", ", expected ) );
			return false;
		}

		private bool SetSaveAs( string value, string setter )
		{
			try
			{
				this.SaveAs = (Filing.SaveAs)Enum.Parse( typeof( Filing.SaveAs ), value );
				executor.Invoke( setter + ": Setting `" + SAVEAS_COMMAND + "` to `" + value + "`." );
				return true;
			}
			catch { }

			string[] expected = Enum.GetNames( typeof( Filing.SaveAs ) );
			executor.Invoke( setter + ": Ignoring parameter `" + SAVEAS_COMMAND + "`.  Reason: unrecognized value `" + value + "`" + separator +
				"\tExpected one of the following: " + string.Join( ", ", expected ) );
			return false;
		}

        private bool SetXsltStylesheetPath( string value, string setter )
		{
            if( File.Exists( value ) ) {
                this.XsltStylesheetPath = value;
                executor.Invoke( setter + ": Setting `" + XSLT_STYLESHEET_COMMAND + "` to `" + value + "`." );
                return true;
            } else {
                executor.Invoke( setter + ": Ignoring parameter `" + XSLT_STYLESHEET_COMMAND + "`.  Reason: file not found `" + value + "`" );
                return false;
            }
        }

		private static bool UnzipPackage(UpdateStatusExecutor executor, Filing f, string zipPath, out string[] zipFiles )
		{
			zipFiles = new string[ 0 ];

			string errorMsg;
			if( !ZipUtilities.TryUnzipAndUncompressFiles( f.InstancePath, zipPath, out zipFiles, out errorMsg ) )
			{
				executor.Invoke( "Error: The ZIP file cannot be opened." );
				executor.Invoke( "\tSkipping filing: " + f.InstancePath );
				return false;
			}

			return true;
		}

		private bool ZipReports( string zipPath, string[] outputFiles )
		{
			executor.Invoke( "Information: Zipping reports for filing package." );

			string rootDirectoryName = string.Empty;
			for( int i = 0; i < outputFiles.Length; i++ )
			{
				if( string.IsNullOrEmpty( rootDirectoryName ) )
				{
					rootDirectoryName = Path.GetDirectoryName( outputFiles[ i ] );
					outputFiles[ i ] = Path.GetFileName( outputFiles[ i ] );
				}
				else if( !outputFiles[ i ].StartsWith( rootDirectoryName ) )
				{
					executor.Invoke( "Error: Zip creation failed." );
					return false;
				}
				else
				{
					outputFiles[ i ] = Path.GetFileName( outputFiles[ i ] );
				}
			}

			if( ZipUtilities.TryZipAndCompressFiles( zipPath, rootDirectoryName, outputFiles ) )
			{
				executor.Invoke( "Information: Zip successfully created."+ separator+ "\tLocation: " + zipPath );
				return true;
			}
			else
			{
				executor.Invoke( "Error: Zip creation failed." );
				return false;
			}
		}

		#endregion
	}
}

