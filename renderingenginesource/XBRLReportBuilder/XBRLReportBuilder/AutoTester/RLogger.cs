using System;
using System.Collections.Generic;

using System.Text;
using log4net;

namespace XBRLReportBuilder
{
	class RLogger
	{
		protected static string LoggerConfigFile = @"C:\Logs\ConfigFiles\log4NetConfig.xml";
		protected static readonly ILog Logger = log4net.LogManager.GetLogger( typeof( RLogger ) );
		protected static bool ConfiguredAlready = false;

		#region IRLogger Members

		public RLogger()
		{
			log4net.Config.XmlConfigurator.Configure();
		}

		public static void Configure( string filepath )
		{
			LoggerConfigFile = filepath;
			InitConfig();
		}

		public static void Configure()
		{
			if( ConfiguredAlready == false )
			{
				InitConfig();
			}
		}
		protected static void InitConfig()
		{
			log4net.Config.XmlConfigurator.Configure();
		}

		public static void Info( string msg )
		{
			Configure();
			Logger.Info( msg );
		}

		public static void Info( string msg, Exception ex )
		{
			Configure();
			Logger.Info( msg, ex );
		}


		public static void Warn( string msg )
		{
			Configure();
			Logger.Warn( msg );
		}

		public static void Warn( string msg, Exception ex )
		{
			Configure();
			Logger.Warn( msg, ex );
		}
		public static void Error( string msg )
		{
			Configure();
			Logger.Error( msg );
		}
		public static void Error( string msg, Exception ex )
		{
			Configure();
			Logger.Error( msg, ex );
		}

		public static void Debug( string msg )
		{
			Configure();
			Logger.Debug( msg );
		}

		public static void Debug( string msg, Exception ex )
		{
			Configure();
			Logger.Debug( msg, ex );
		}

		public static void Fatal( string msg )
		{
			Configure();
			Logger.Fatal( msg );
		}

		public static void Fatal( string msg, Exception ex )
		{
			Configure();
			Logger.Fatal( msg, ex );

		}
		#endregion
	}
}
