using System;
using System.Collections.Generic;

using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace XBRLReportBuilder
{
    public sealed class DataAccess : IDisposable
    {
        #region members

        private static string connectionString;
        private const string con = "Data Source={0}; Initial Catalog={1}; User ID={2}; Password={3};";
        private const string con2 = "Data Source={0}; Initial Catalog={1}; Integrated Security=SSPI;";
        private string[] connectionInfo = new string[4];
        public string[] ConnectionInfo
        {
            get { return connectionInfo; }
            set
            {
                if (value != null)
                {
                    connectionString = string.Format(con, value[0], value[1], value[2], value[3]);
                }
            }
        }

        #endregion

        #region constructors

        public DataAccess()
        {
            //This constructor needs the ConnectionInfo property to generate the connection string. 
        }

        public DataAccess(string Server, string DataBase, string UserID, string Password)
        {
            //Use this constructor to connect via login/password
            connectionString = string.Format(con, Server, DataBase, UserID, Password);
        }

        public DataAccess(string Server, string DataBase)
        {
            //Use this constructor to connect via Windows Auth:
            connectionString = string.Format(con2, Server, DataBase);
        }

        public DataAccess(string CustomConnectionString)
        {
            //Allow user to use custom connection string 
            connectionString = CustomConnectionString;
        }

        #endregion

        #region ExecuteReader Block

        public DataTable ExecuteReader(CommandType commandType, string sqlString)
        {
            SqlParameter parameter = null;
            return ExecuteReader(commandType, sqlString, 60, parameter);
        }

        public DataTable ExecuteReader(CommandType commandType, string sqlString, int commandTimeout)
        {
            SqlParameter parameter = null;
            return ExecuteReader(commandType, sqlString, commandTimeout, parameter);
        }

        public DataTable ExecuteReader(CommandType commandType, string sqlString, params SqlParameter[] parameters)
        {
            return ExecuteReader(commandType, sqlString, 60, parameters);
        }

        public DataTable ExecuteReader(CommandType commandType, string sqlString, int commandTimeout, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();

            if (connectionString == null)
            {
                ThrowConnectionException();
                dt = null;
                return dt;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sqlString, connection);
                cmd.CommandType = commandType;

                if (parameters[0] != null)
                {
                    foreach (SqlParameter parameter in parameters)
                    {
                        cmd.Parameters.Add(parameter);
                    }
                }

                try
                {
                    cmd.CommandTimeout = commandTimeout;
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                }
                catch (SqlException)
                {
                    throw;
                }
                finally
                {
                    cmd.Parameters.Clear();
                }
                dt.TableName = "SQLData";
                return dt;
            }
        }

        #endregion

        #region ExecuteNonQueryBlock

        public int ExecuteNonQuery(CommandType commandType, string sqlString)
        {
            SqlParameter parameter = null;
            return ExecuteNonQuery(commandType, sqlString, 60, parameter);
        }

        public int ExecuteNonQuery(CommandType commandType, string sqlString, int commandTimeout)
        {
            SqlParameter parameter = null;
            return ExecuteNonQuery(commandType, sqlString, commandTimeout, parameter);
        }

        public int ExecuteNonQuery(CommandType commandType, string sqlString, params SqlParameter[] parameters)
        {
            return ExecuteNonQuery(commandType, sqlString, 60, parameters);
        }

        public int ExecuteNonQuery(CommandType commandType, string sqlString, int commandTimeout, params SqlParameter[] parameters)
        {
            if (connectionString == null)
            {
                ThrowConnectionException();
                return 0;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                int rows = 0;
                SqlCommand cmd = new SqlCommand(sqlString, connection);
                if (parameters[0] != null)
                {
                    foreach (SqlParameter parameter in parameters)
                    {
                        cmd.Parameters.Add(parameter);
                    }
                }
                try
                {
                    cmd.CommandType = commandType;
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Connection.Open();
                    rows = cmd.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    throw;
                }
                finally
                {
                    cmd.Parameters.Clear();
                    cmd.Connection.Close();
                }

                return rows;
            }
        }

        #endregion

        #region ExecuteScalar Block

        public object ExecuteScalar(CommandType commandType, string sqlString)
        {
            SqlParameter parameter = null;
            return ExecuteScalar(commandType, sqlString, 60, parameter);
        }

        public object ExecuteScalar(CommandType commandType, string sqlString, int commandTimeout)
        {
            SqlParameter parameter = null;
            return ExecuteScalar(commandType, sqlString, commandTimeout, parameter);
        }

        public object ExecuteScalar(CommandType commandType, string sqlString, params SqlParameter[] parameters)
        {
            return ExecuteScalar(commandType, sqlString, 60, parameters);
        }

        public object ExecuteScalar(CommandType commandType, string sqlString, int commandTimeout, params SqlParameter[] parameters)
        {
            if (connectionString == null)
            {
                ThrowConnectionException();
                return string.Empty;
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(sqlString, connection);
                var value = new object();
                if (parameters[0] != null)
                {
                    foreach (SqlParameter parameter in parameters)
                    {
                        cmd.Parameters.Add(parameter);
                    }
                }

                try
                {
                    cmd.CommandType = commandType;
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Connection.Open();
                    value = cmd.ExecuteScalar();
                }
                catch (SqlException)
                {
                    throw;
                }
                finally
                {
                    cmd.Parameters.Clear();
                    cmd.Connection.Close();
                }
                return value;
            }
        }

        #endregion

        #region ExecuteBulkInsert Block

        public void ExecuteBulkInsert(DataTable insertDT, string insertTableName)
        {
            ExecuteBulkInsert(insertDT, insertTableName, 60, false);
        }

        public void ExecuteBulkInsert(DataTable insertDT, string insertTableName, int commandTimeOut)
        {
            ExecuteBulkInsert(insertDT, insertTableName, commandTimeOut, false);
        }

        public void ExecuteBulkInsert(DataTable insertDT, string insertTableName, bool hasIDColumn)
        {
            ExecuteBulkInsert(insertDT, insertTableName, 60, hasIDColumn);
        }

        public void ExecuteBulkInsert(DataTable insertDT, string insertTableName, int commandTimeout, bool hasIDColumn)
        {
            if (connectionString == null)
            {
                ThrowConnectionException();
                return;
            }

            if (!hasIDColumn)
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    try
                    {
                        connection.ConnectionString = connectionString;
                        connection.Open();

                        using (SqlBulkCopy bulk = new SqlBulkCopy(connection))
                        {
                            bulk.DestinationTableName = insertTableName;
                            bulk.BulkCopyTimeout = commandTimeout;
                            bulk.WriteToServer(insertDT);
                        }
                    }
                    catch (SqlException)
                    {
                        throw;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            else
            {//Keep the identity column
                try
                {
                    using (SqlBulkCopy bulk = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.KeepIdentity))
                    {
                        bulk.DestinationTableName = insertTableName;
                        bulk.BulkCopyTimeout = commandTimeout;
                        bulk.WriteToServer(insertDT);
                    }
                }
                catch (SqlException)
                {
                    throw;
                }
            }
        }

        #endregion

        #region ExecuteTestConnection Block

        public bool ExecuteTestConnection()
        {
            if (connectionString == null)
            {
                ThrowConnectionException();
                return false;
            }

            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;
                try
                {
                    connection.Open();
                    return true;
                }
                catch (SqlException)
                {
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        #endregion

        private void ThrowConnectionException()
        {
            throw new ArgumentNullException("ConnectionInfo[]",
                "ConnectionInfo[] property or Connection String must be set before using this class.");
        }

        public void Dispose()
        {
            ConnectionInfo = null;
        }
    }
}
