using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text.RegularExpressions;
using CricketClubDomain;
using log4net;

namespace CricketClubDAL
{
    public class Db
    {
        
        public Db(string customConnectionString)
        {
            connectionString = customConnectionString;
        }

        public Db()
        {
            
        }
        
        private static string connectionString;
        private static readonly ILog Log = LogManager.GetLogger(typeof(Db));


        private static OleDbConnection OpenConnection()
        {
            var scorebookConnectionString = GetScorebookConnectionString();
            var conn = new OleDbConnection(scorebookConnectionString);
            conn.Open();
            return conn;
        }

        private static string SanitizeConnectionString(string cs)
        {
            if (string.IsNullOrEmpty(cs)) return cs;
            var pattern = @"(password|pwd)\s*=\s*([^;]+)";
            return Regex.Replace(cs, pattern, "$1=****", RegexOptions.IgnoreCase);
        }


        private static string GetScorebookConnectionString()
        {
            if (connectionString == null)
            {
                var key = "ProdDB";
                if (Environment.MachineName.Contains("TABLET"))
                {
                    key = "LocalDB";
                }
                if (Environment.MachineName.Contains("BIG-PC"))
                {
                    key = "BigPC-2019";
                }
                if (Environment.MachineName.Contains("LAPTOP"))
                {
                    key = "Laptop";
                }
                if (Environment.MachineName.Contains("PRO9"))
                {
                    key = "Surface";
                }

                if (Environment.CommandLine.ToUpper().Contains("TEST"))
                {
                    key = "TestDB";
                }
                
                var cnxStr = ConfigurationManager.ConnectionStrings[key];
                if (cnxStr == null)
                    throw new ConfigurationErrorsException("ConnectionString '" + key +
                                                           "' was not found in the configuration file.");

                connectionString = cnxStr.ConnectionString;
                Log.Info("Connection string: " + SanitizeConnectionString(connectionString));
            }
            
            return connectionString;
        }

        public T ExecuteSQLAndReturnFirstRow<T>(string sql, Func<Row, T> rowExtractorFunc, T defaultIfNone) where T : class
        {
            var allRows = ExecuteSqlAndReturnAllRows(sql, rowExtractorFunc).ToList();
            var first = allRows.FirstOrDefault();
            return first ?? defaultIfNone;
        }

        public DataRow ExecuteSQLAndReturnFirstRow(string sql)
        {
            try
            {
                using (var connection = OpenConnection())
                {
                    Log.Debug("Executing SQL: " + sql);
                    var data = new DataSet();
                    var adaptor = new OleDbDataAdapter(sql, connection);
                    adaptor.Fill(data);
                    if (data.Tables[0] != null && data.Tables[0].Rows.Count > 0)
                    {
                        Log.Debug("Found " + data.Tables[0].Rows.Count + " rows.");
                        var firstRow = data.Tables[0].Rows[0];
                        Log.Debug("Result: " + firstRow.ItemArray.Aggregate("", (current, item) => current + (item + ", ")));
                        return firstRow;
                    }
                    Log.Debug("Result: null");
                    return null;
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Error executing: " + sql, exception);
            }
        }

        public object ExecuteSqlAndReturnSingleResult(string sql)
        {
            try
            {
                using (var conn = OpenConnection())
                {
                    using (var command = new OleDbCommand(sql, conn))
                    {
                        Log.Debug("Executing SQL: " + sql);
                        var executeSqlAndReturnSingleResult = command.ExecuteScalar();
                        var returnedValue = executeSqlAndReturnSingleResult is DBNull
                            ? "null"
                            : (executeSqlAndReturnSingleResult?.ToString() ?? "null");
                        Log.Debug("Result: " + returnedValue);
                        return executeSqlAndReturnSingleResult;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Error executing: " + sql, exception);
            }
        }

        public int ExecuteInsertOrUpdate(string sql)
        {
            try
            {
                Log.Debug("Executing SQL: " + sql);
                using (var conn = OpenConnection())
                {
                    using (var command = new OleDbCommand(sql, conn))
                    {
                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error executing SQL: " + sql, exception);
                throw new Exception("Error executing: " + sql, exception);
            }
        }

        public IEnumerable<T> ExecuteSqlAndReturnAllRows<T>(string sql, Func<Row, T> rowConverter)
        {
            Log.Debug("Executing SQL: " + sql);
            var dataSet = ExecuteSqlAndReturnAllRows(sql);
            Log.Debug("Found " + dataSet.Tables[0].Rows.Count + " rows.");
            dataSet.Tables[0].Rows.Cast<DataRow>().ToList().ForEach(r =>
            {
                var rowData = r.ItemArray.Aggregate("", (current, item) => current + (item + ", "));
                Log.Debug("Row: " + rowData);
            });
            return dataSet.Tables[0].Rows.Cast<DataRow>().Select(r=>new Row(r)).Select(rowConverter);
        }


        public DataSet ExecuteSqlAndReturnAllRows(string sql)
        {
            try
            {
                using (var conn = OpenConnection())
                {
                    var data = new DataSet();
                    var adaptor = new OleDbDataAdapter(sql, conn);
                    adaptor.Fill(data);
                    return data;
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Error executing: " + sql, exception);
            }
        }

        public Row QueryOne(string sql)
        {
            return new Row(ExecuteSQLAndReturnFirstRow(sql));
        }

        public IEnumerable<Row> QueryMany(string sql)
        {
            return ExecuteSqlAndReturnAllRows(sql).Tables[0].Rows.Cast<DataRow>().Select(row => new Row(row));
        }

        public IEnumerable<T> QueryMany<T>(string sql, Func<Row, T> extractor)
        {
            return ExecuteSqlAndReturnAllRows(sql).Tables[0].Rows.Cast<DataRow>().Select(row => new Row(row)).Select(extractor);
        }


    }

    public class Row
    {
        private readonly DataRow dataRow;

        public Row(DataRow dataRow)
        {
            this.dataRow = dataRow;
        }

        public int GetInt(int index)
        {
            return GetInt(index, 0);
        }

        public int GetInt(string columnName)
        {
            return Convert.ToInt32(dataRow[columnName]);
        }

        public decimal? GetDecimal(string columnName)
        {
            var value = dataRow[columnName];
            return value is DBNull ? (decimal?)null : Convert.ToDecimal(value);
            
        }

        public string GetString(string columnName)
        {
            return dataRow[columnName].ToString();
        }

        public int GetInt(int index, int valueIfNull)
        {
            var value = dataRow[index];
            if (value is DBNull)
            {
                return valueIfNull;
            }
            return Convert.ToInt32(value);
        }

        public int GetInt(string columnName, int valueIfNull)
        {
            var value = dataRow[columnName];
            if (value is DBNull)
            {
                return valueIfNull;
            }
            return Convert.ToInt32(value);
        }

        public bool GetBool(string columnName, bool defaultValue=false)
        {
            var value = dataRow[columnName];
            if (value is DBNull)
            {
                return defaultValue;
            }
            return Convert.ToBoolean(value);
        }
        
        public DateTime GetDateTime(string columnName, DateTime defaultIfNull)
        {
            var value = dataRow[columnName];
            if (value is DBNull)
            {
                return defaultIfNull;
            }
            return Convert.ToDateTime(value);
        }

        public T GetEnum<T>(string columnName)
        {
            return (T) Enum.Parse(typeof (T), GetString(columnName));
        }
    }
}