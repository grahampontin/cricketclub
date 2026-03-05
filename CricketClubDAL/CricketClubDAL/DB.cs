using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
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


        private static SqlConnection OpenConnection()
        {
            var scorebookConnectionString = GetScorebookConnectionString();
            var conn = new SqlConnection(scorebookConnectionString);
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
            if (connectionString != null)
            {
                return connectionString;
            }

            var envConnectionString = Environment.GetEnvironmentVariable($"Database_ConnectionString");
            if (!string.IsNullOrEmpty(envConnectionString))
            {
                connectionString = envConnectionString;
                Log.Info($"Using connection string from environment variable for key 'Database_ConnectionString'");
                Log.Info("Connection string: " + SanitizeConnectionString(connectionString));
                return connectionString;
            }
            else
            {
                Log.Error("Environment variable 'Database_ConnectionString' not found or empty");
                throw new ConfigurationErrorsException("Database connection string not found in environment variable 'Database_ConnectionString'");
            }

        }

        public T ExecuteSQLAndReturnFirstRow<T>(string sql, Func<Row, T> rowExtractorFunc, T defaultIfNone) where T : class
        {
            var allRows = ExecuteSqlAndReturnAllRows(sql, rowExtractorFunc).ToList();
            var first = allRows.FirstOrDefault();
            return first ?? defaultIfNone;
        }

        public T ExecuteSQLAndReturnFirstRow<T>(string sql, Func<Row, T> rowExtractorFunc, T defaultIfNone, params SqlParameter[] parameters) where T : class
        {
            var allRows = ExecuteSqlAndReturnAllRows(sql, rowExtractorFunc, parameters).ToList();
            var first = allRows.FirstOrDefault();
            return first ?? defaultIfNone;
        }

        public DataRow ExecuteSQLAndReturnFirstRow(string sql)
        {
            try
            {
                using (var connection = OpenConnection())
                {
                    Log.Info("Executing SQL: " + sql);
                    var data = new DataSet();
                    var adaptor = new SqlDataAdapter(sql, connection);
                    adaptor.Fill(data);
                    if (data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0)
                    {
                        Log.Info("Found " + data.Tables[0].Rows.Count + " rows.");
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

        public DataRow ExecuteSQLAndReturnFirstRow(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (var connection = OpenConnection())
                {
                    Log.Info("Executing SQL with parameters: " + sql);
                    using (var command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        var data = new DataSet();
                        var adaptor = new SqlDataAdapter(command);
                        adaptor.Fill(data);
                        if (data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0)
                        {
                            Log.Info("Found " + data.Tables[0].Rows.Count + " rows.");
                            var firstRow = data.Tables[0].Rows[0];
                            Log.Debug("Result: " + firstRow.ItemArray.Aggregate("", (current, item) => current + (item + ", ")));
                            return firstRow;
                        }
                        Log.Debug("Result: null");
                        return null;
                    }
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
                    using (var command = new SqlCommand(sql, conn))
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

        public object ExecuteSqlAndReturnSingleResult(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (var conn = OpenConnection())
                {
                    using (var command = new SqlCommand(sql, conn))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        Log.Debug("Executing SQL with parameters: " + sql);
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
                    using (var command = new SqlCommand(sql, conn))
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

        public int ExecuteInsertOrUpdate(string sql, params SqlParameter[] parameters)
        {
            try
            {
                Log.Debug("Executing SQL with parameters: " + sql);
                using (var conn = OpenConnection())
                {
                    using (var command = new SqlCommand(sql, conn))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
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
            var dataSet = ExecuteSqlAndReturnAllRows(sql);
            if (dataSet.Tables.Count == 0) return Enumerable.Empty<T>();
            return dataSet.Tables[0].Rows.Cast<DataRow>().Select(r=>new Row(r)).Select(rowConverter);
        }

        public IEnumerable<T> ExecuteSqlAndReturnAllRows<T>(string sql, Func<Row, T> rowConverter, params SqlParameter[] parameters)
        {
            var dataSet = ExecuteSqlAndReturnAllRows(sql, parameters);
            if (dataSet.Tables.Count == 0) return Enumerable.Empty<T>();
            return dataSet.Tables[0].Rows.Cast<DataRow>().Select(r=>new Row(r)).Select(rowConverter);
        }


        public DataSet ExecuteSqlAndReturnAllRows(string sql)
        {
            try
            {
                Log.Info("Executing SQL: " + sql);
                using (var conn = OpenConnection())
                {
                    var dataSet = new DataSet();
                    var adaptor = new SqlDataAdapter(sql, conn);
                    adaptor.Fill(dataSet);
                    if (dataSet.Tables.Count > 0)
                    {
                        Log.Info("Found " + dataSet.Tables[0].Rows.Count + " rows.");
                        dataSet.Tables[0].Rows.Cast<DataRow>().ToList().ForEach(r =>
                        {
                            var rowData = r.ItemArray.Aggregate("", (current, item) => current + (item + ", "));
                            Log.Debug("Row: " + rowData);
                        });
                    }
                    else
                    {
                        Log.Info("Query returned no result set.");
                    }
                    return dataSet;
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error executing SQL: " + sql, exception);
                throw new Exception("Error executing: " + sql, exception);
            }
        }

        public DataSet ExecuteSqlAndReturnAllRows(string sql, params SqlParameter[] parameters)
        {
            try
            {
                Log.Info("Executing SQL with parameters: " + sql);
                using (var conn = OpenConnection())
                {
                    using (var command = new SqlCommand(sql, conn))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        var dataSet = new DataSet();
                        var adaptor = new SqlDataAdapter(command);
                        adaptor.Fill(dataSet);
                        if (dataSet.Tables.Count > 0)
                        {
                            Log.Info("Found " + dataSet.Tables[0].Rows.Count + " rows.");
                            dataSet.Tables[0].Rows.Cast<DataRow>().ToList().ForEach(r =>
                            {
                                var rowData = r.ItemArray.Aggregate("", (current, item) => current + (item + ", "));
                                Log.Debug("Row: " + rowData);
                            });
                        }
                        else
                        {
                            Log.Info("Query returned no result set.");
                        }
                        return dataSet;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error executing SQL: " + sql, exception);
                throw new Exception("Error executing: " + sql, exception);
            }
        }

        public Row QueryOne(string sql)
        {
            return new Row(ExecuteSQLAndReturnFirstRow(sql));
        }

        public IEnumerable<Row> QueryMany(string sql)
        {
            var dataSet = ExecuteSqlAndReturnAllRows(sql);
            if (dataSet.Tables.Count == 0) return Enumerable.Empty<Row>();
            return dataSet.Tables[0].Rows.Cast<DataRow>().Select(row => new Row(row));
        }

        public IEnumerable<T> QueryMany<T>(string sql, Func<Row, T> extractor)
        {
            var dataSet = ExecuteSqlAndReturnAllRows(sql);
            if (dataSet.Tables.Count == 0) return Enumerable.Empty<T>();
            return dataSet.Tables[0].Rows.Cast<DataRow>().Select(row => new Row(row)).Select(extractor);
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

        public decimal GetDecimal(string columnName, decimal defaultValue)
        {
            var value = dataRow[columnName];
            return value is DBNull ? defaultValue : Convert.ToDecimal(value);
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
        
        public double GetDouble(string columnName, double defaultValue=0.0)
        {
            var value = dataRow[columnName];
            if (value is DBNull)
            {
                return defaultValue;
            }
            return Convert.ToDouble(value);
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

        public DateTime GetDateTime(string columnName)
        {
            return Convert.ToDateTime(dataRow[columnName]);
        }

        public T GetEnum<T>(string columnName)
        {
            return (T) Enum.Parse(typeof (T), GetString(columnName));
        }
    }
}