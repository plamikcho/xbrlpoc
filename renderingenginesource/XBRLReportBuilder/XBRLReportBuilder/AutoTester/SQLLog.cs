using System;
using System.Collections.Generic;
using System.Configuration;

using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace XBRLReportBuilder
{
    public static class SQLLog
    {
        //TODO: validate the connectionString, try to connect to the DB before the write, THEN write to the DB. Need to handle a connection string that's not there or is blank.
        
    //Fields
        private const string InsertNewError =   "InsertNewError";
        private const string InsertNewFile =    "InsertNewFile";
        private const string InsertNewReport =  "InsertNewReport";
        private const string UpdateFileBaseine = "UpdateFileBaseline";

    //Constructors
        static SQLLog() { }

    //Methods

        /// <summary>
        /// Adds a new Report to the sql database
        /// </summary>
        /// <param name="reportName">The name of the report (ReportLongName)</param>
        /// <param name="error">Output of any error messages</param>
        /// <returns>The inserted ID of the SQL record</returns>
        public static int AddReport(string reportName, DateTime reportDate, out string error)
        {
            //String connectionString = String.Empty;
            String connectionString = (!String.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["SaveResults"].ConnectionString)) ? ConfigurationManager.ConnectionStrings["SaveResults"].ConnectionString : String.Empty;

            error = string.Empty;
            SqlParameter _reportID = new SqlParameter("@reportID", 0);
            _reportID.Direction = ParameterDirection.Output;

            try
            {
                if (!String.IsNullOrEmpty(connectionString))
                {
                    using (DataAccess data = new DataAccess(connectionString))
                    {
                        if (data.ExecuteTestConnection())
                        {
                            data.ExecuteNonQuery(CommandType.StoredProcedure, InsertNewReport,
                                new SqlParameter("@ReportName", reportName),
                                new SqlParameter("@ReportDateTime", reportDate),
                                _reportID);
                        }
                    } 
                }
                
            }
            catch (SqlException ex)
            { 
                error = ex.Message;
				RLogger.Error( "An SQL exception occured", ex );

            }
            catch (Exception ex)
            { 
                error = ex.Message;
				RLogger.Error( "An exception occured", ex );
            }

            return Convert.ToInt32(_reportID.Value);

        }

        /// <summary>
        /// Inserts a new RFile ID into the SQL database
        /// </summary>
        /// <param name="reportID">The ReportID from the Report table</param>
        /// <param name="rFile">The r file name (R1.xml)</param>
        /// <param name="baseURL">Location of the base HTML</param>
        /// <param name="newURL">Location of the new HTML</param>
        /// <param name="updateBaseline">Whether or not to update the baseline</param>
        /// <param name="error">Output of any error messages</param>
        /// <returns>Returns the R File ID of the inserted record</returns>
        public static int AddFile(int reportID, string rFile, 
            string baseURL, string newURL, out string error)
        {
			error = string.Empty;
			if( ConfigurationManager.ConnectionStrings[ "SaveResults" ] == null )
				return -1;

			string connectionString = string.IsNullOrEmpty( ConfigurationManager.ConnectionStrings[ "SaveResults" ].ConnectionString ) ?
				string.Empty : ConfigurationManager.ConnectionStrings[ "SaveResults" ].ConnectionString;

			if( string.IsNullOrEmpty( connectionString ) )
				return -1;

            SqlParameter fileID = new SqlParameter("@FileID", 0);
			fileID.Direction = ParameterDirection.Output;

            try
            {
                using (DataAccess data = new DataAccess(connectionString))
                {
                    if (data.ExecuteTestConnection())
                    {
                        data.ExecuteNonQuery(CommandType.StoredProcedure, InsertNewFile,
                            new SqlParameter("@ReportID", reportID), new SqlParameter("@RFile", rFile),
                            new SqlParameter("@BaseURL", baseURL), new SqlParameter("@NewURL", newURL),
                            new SqlParameter("@UpdateBaseline", true), fileID);
                    }
                }
            }

            catch (SqlException ex)
            { 
                error = ex.Message;
                RLogger.Error("An SQL exception occured", ex);
            }
            catch (Exception ex)
            { 
                error = ex.Message;
				RLogger.Error( "An exception occured", ex );
            }
            
            return Convert.ToInt32(fileID.Value);

        }

        /// <summary>
        /// Add an error to the SQL table
        /// </summary>
        /// <param name="fileID">The corresponding R file</param>
        /// <param name="errorMessage">The error type</param>
        /// <param name="errorDesc">The full error description</param>
        /// <param name="error">Output of any error messages</param>
        /// <returns>Whether the method was successful</returns>
        public static int AddError(int fileID, string errorMessage, string errorDesc, out string error)
        {
			error = string.Empty;
			if( ConfigurationManager.ConnectionStrings[ "SaveResults" ] == null )
				return -1;

			string connectionString = string.IsNullOrEmpty( ConfigurationManager.ConnectionStrings[ "SaveResults" ].ConnectionString ) ?
				string.Empty : ConfigurationManager.ConnectionStrings[ "SaveResults" ].ConnectionString;

			if( string.IsNullOrEmpty( connectionString ) )
				return -1;


            int errorID = 0;

            try
            {
                SqlParameter _errorID = new SqlParameter("@ErrorID", errorID);
                _errorID.Direction = ParameterDirection.Output;

                if (!String.IsNullOrEmpty(connectionString))
                {
                    using (DataAccess data = new DataAccess(connectionString))
                    {
                        if (data.ExecuteTestConnection())
                        {
                            data.ExecuteNonQuery(CommandType.StoredProcedure, InsertNewError,
                            new SqlParameter("@FileID", fileID), new SqlParameter("@Error", errorMessage),
                            new SqlParameter("@ErrorDesc", errorDesc), new SqlParameter("@UpdateBaseline", true),
                            _errorID);    
                        }
                    }    
                }

                errorID = Convert.ToInt32(_errorID.Value);
            }

            catch (SqlException ex)
            {
                error = ex.Message;
				RLogger.Error( "An SQL exception occured", ex );
            }
            catch (Exception ex)
            {
                error = ex.Message;
				RLogger.Error( "An exception occured", ex );
                
            }

            return errorID;

        }

        /// <summary>
        /// Updates an existing File record in the db
        /// </summary>
        /// <param name="errorID">The id of the file to modify</param>
        /// <param name="updateBaseline">bool value to set</param>
        /// <returns>Whether the method worked</returns>
        public static bool ModifyBaseLine(int errorID, bool updateBaseline, out string error)
        {
            String connectionString = (!String.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["SaveResults"].ConnectionString)) ? ConfigurationManager.ConnectionStrings["SaveResults"].ConnectionString : String.Empty;
            bool retVal = true;
            error = string.Empty;
            try
            {
                if (!String.IsNullOrEmpty(connectionString))
                {
                    using (DataAccess data = new DataAccess(connectionString))
                    {
                        if (data.ExecuteTestConnection())
                        {
                            data.ExecuteNonQuery(CommandType.StoredProcedure, UpdateFileBaseine,
                                new SqlParameter("@ErrorID", errorID), new SqlParameter("@UpdateBaseline", updateBaseline));
                        }
                    }    
                }
                
                retVal = true;
            }
            catch (SqlException ex)
            {
                retVal = false;
                error = ex.Message;
            }
            catch (Exception ex)
            {
                retVal = false;
                error = ex.Message;
            }

            return retVal;
        }

    }
}
