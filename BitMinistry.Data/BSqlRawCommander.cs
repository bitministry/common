using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace BitMinistry.Data
{


    ///<summary>
    /// disposable System.Data.SqlClient.SqlCommand facade; a bit heavier than BitMinistry.SqlClient, for SqlParameter utilization, etc 
    ///</summary>
    public class BSqlRawCommander : BSqlCommanderUtil, IDisposable
    {
        public string ConnectionString { get; set; }
        public SqlCommand Com;

        public int CommandTimeout;

        public CommandType DefaultCommandType = CommandType.Text;

        public string DbSchema => _dbSchema == null ? null : (_dbSchema + "."); string _dbSchema; public string InSchema(string entityName) => DbSchema + entityName;
        public void SetDbSchema(string schema)
        {
            if (schema != null)
                _dbSchema = schema.TrimEnd('.');
        }


        /// <param name="connectionName">if the key dont exist in Config.ConnectionStrings, the connectionName variable is used as a raw Sql connectionString</param>
        public BSqlRawCommander(string connectionName = null, CommandType comType = CommandType.Text, int commandTimeout = 30, string dbSchema = null)  
        {
            this.CommandTimeout = commandTimeout;

            ConnectionString = Config.ConnectionStrings[connectionName]
                ?? connectionName; // if no named entry exists, use the connectionName as the connectionString itself

            if (string.IsNullOrEmpty(ConnectionString))
                ConnectionString = Config.DefaultSqlConnectionString;

            Reset(comType);

            SetDbSchema( dbSchema );
        }

        Action<string> _log;
        public Action<string> Log
        {
            get => _log ?? (_log = (x) => { });
            set => _log = value;
        }
        public virtual void Reset(CommandType? comType = null)
        {
            Com = new SqlCommand();

            Com.CommandTimeout = CommandTimeout;
            Com.CommandType = comType ?? DefaultCommandType;

            if (comType == CommandType.StoredProcedure)
                AddWithValue("returnInt", null, ParameterDirection.ReturnValue);
        }

        public int? StoredProcedureReturnValue => GetParameterValue("returnInt") as int?;

        public virtual IDataRecord[] GetData(string query = null, bool reset = false)
        {
            
            return Execute(x => x.ExecuteReader().Cast<IDataRecord>().ToArray(), query, reset);
        }

        public SqlInfoMessageEventHandler InfoMessage;


        public bool TryCatchTheExecute = false;

        protected T ExecuteSql<T>(Func<SqlCommand, T> sqlFunc)
        {
        ExecuteSql:
            using (Com.Connection = new SqlConnection(ConnectionString))
            {
                Com.Connection.InfoMessage += InfoMessage;
                Com.Connection.Open();
                if (Com.Connection.State == ConnectionState.Open)
                    if (!TryCatchTheExecute)
                        return sqlFunc(Com);
                    else
                        try
                        {
                            Log(Com.CommandText);
                            return sqlFunc(Com);
                        }
                        catch (SqlException ex)
                        {
                            ex.Data["sql"] = Com.CommandAsSql();
                            throw;
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
            }

            goto ExecuteSql;
        }

        public object GetParameterValue(string key)
        {
            return Com.Parameters.Contains(key) ? Com.Parameters[key].Value : null;
        }



        public DataRow[] GetDataRows(string query = null, bool reset = false)
        {
            if (reset) Reset();
            if (query != null)
                Com.CommandText = query;
            Com.CommandTimeout = CommandTimeout;

            using (var dataSet = new DataSet())
            using (var adapter = new SqlDataAdapter { SelectCommand = Com })
                return ExecuteSql(x =>
                {
                    adapter.Fill(dataSet);
                    return dataSet.Tables[0].Rows.Cast<DataRow>().ToArray();
                });
        }



        /// <summary>
        /// Split the text by " go ", and execute parts separately
        /// </summary>
        public void ExecuteSqlFileWithGoStatements(string path)
        {

            var queries = File.ReadAllText(path).ToLower()
                    .Split(new[] { " go " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var q in queries)
                ExecuteNonQuery(q);

        }


        public void AddParameters(IDictionary<string, object> pars)
        {
            foreach (var par in pars)
                AddWithValue(par.Key, par.Value);
        }

        public void AddParameters(object[,] pars)
        {
            if (pars != null)
                for (int i = 0; i < pars.Length / 2; i++)
                    AddWithValue(pars[i, 0].ToString(), pars[i, 1]);
        }

        public virtual int ExecuteNonQuery(string query = null, bool reset = false)
        {
            return Execute(x => x.ExecuteNonQuery(), query, reset);
        }
        public virtual object ExecuteScalar(string query = null, bool reset = false)
        {
            return Execute(x =>
            {
                var ret = x.ExecuteScalar();
                return ret == DBNull.Value ? null : ret;
            }, query, reset);
        }




        protected T Execute<T>(Func<SqlCommand, T> sqlFunc, string query = null, bool reset = false)
        {
            if (reset) Reset();
            if (query != null)
                Com.CommandText = query;
            Com.CommandTimeout = CommandTimeout;

            return ExecuteSql(sqlFunc);
        }


        ///<summary>
        /// do not wrap @arguments to apostrophes in TSQL WHERE part 
        ///</summary>


        public void AddWithValue(string parameterName, object value, ParameterDirection direction = ParameterDirection.Input)
        {

            if (value == null
                || value is float && float.IsNaN((float)value)
                || value is double && double.IsNaN((double)value)
                ) value = DBNull.Value;
            else
                if (value.GetType().IsEnum)
                value = value.ToString();

            var p = new SqlParameter
            {
                ParameterName = parameterName,
                Value = CheckCovertUnsignedIfRequired(value),
                Direction = direction
            };

            Com.Parameters.Add(p);

        }


        public virtual void Dispose()
        {
            Com.Dispose();
        }


        public bool InsertToTable(IDictionary<string, object> parameters, string tableName, bool avoidDuplicate = false)
        {
            Reset();

            parameters = parameters.Where(pair => pair.Value != null) // to preserve MSSQL default values 
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value);

            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));

            if (parameters.Count == 0)
                throw new ArgumentException("Nothing to insert");

            foreach (var key in parameters.Keys)
                AddWithValue(key, parameters[key]);

            string query;

            if (avoidDuplicate)
            {
                query = $"select count(*) from {InSchema(tableName)} WHERE " + string.Join(" AND ", parameters.Keys.Select(x => $"{x} = @{x}"));

                if (ExecuteScalar(query) as int? > 0)
                    return false;
            }

            query = $"INSERT INTO {InSchema(tableName)} ([{string.Join("],[", parameters.Keys)}]) VALUES (@{string.Join(",@", parameters.Keys)})";

            var res = ExecuteNonQuery(query);
            Com.Parameters.Clear();
            return true;
        }


        public int UpdateAtTable(IDictionary<string, object> parameters, string tableName, string idColumn, object idValue)
        {
            if (string.IsNullOrEmpty(idColumn))
                throw new ArgumentNullException(nameof(idColumn));
            if (idValue == null)
                throw new ArgumentNullException(nameof(idValue));

            Reset();

            // parameters[idColumn] = idValue;

            var updateWhere = $"{idColumn}={idValue.Sql()}";

            return UpdateAtTable(parameters, tableName, updateWhere, reset: false);
        }

        public int UpdateAtTable(IDictionary<string, object> parameters, string tableName, string updateWhere, bool reset = true)
        {
            if (reset) Reset();

            if (string.IsNullOrEmpty(updateWhere))
                throw new ArgumentNullException(nameof(updateWhere));

            var query = $"UPDATE {InSchema(tableName)} " +
                        ((Com is SqlCommand) ? "WITH ( ROWLOCK ) " : "") +
                        $"SET {string.Join(", ", parameters.Select(x => $"[{x.Key}]=@{x.Key}"))} " +
                        "WHERE " + updateWhere;


            if (parameters.Count == 0)
                throw new InvalidOperationException("Trying to update without parameters:" + query);

            foreach (var par in parameters)
                AddWithValue(par.Key, par.Value);

            var ret = ExecuteNonQuery(query);
            Com.Parameters.Clear();
            return ret;
        }



    }



}