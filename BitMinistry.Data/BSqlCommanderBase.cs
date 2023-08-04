using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace BitMinistry.Data
{
    public abstract class BSqlCommanderBase <TDbCommand, TDbConnection, TDbParameter> : IDisposable 
        where TDbCommand : IDbCommand
        where TDbConnection : IDbConnection 
        where TDbParameter : IDbDataParameter 
    {
    
        protected abstract T ExecuteSql<T>(Func<TDbCommand, T> sqlFunc);
        public abstract IDataRecord[] GetData(string query = null, bool reset = false);
        public abstract object GetParameterValue(string key);



        public string ConnectionString { get; set; }

        public TDbCommand Com;

        public int CommandTimeout;

        public BSqlCommanderBase(string connectionName = null,  CommandType comType = CommandType.Text, int commandTimeout = 30)
        {
            this.CommandTimeout = commandTimeout;

            ConnectionString = Config.ConnectionStrings[connectionName] 
                ?? connectionName; // if no named entry exists, use the connectionName as the connectionString itself

            if (string.IsNullOrEmpty(ConnectionString))
                ConnectionString = Config.DefaultSqlConnectionString;

            Reset(comType);
        }



        string _dbSchema;
        protected string inSchema(string entityName) => $"{ (_dbSchema != null ? _dbSchema + "." : "") }{entityName}";
        public void SetDbSchema(string schema) => _dbSchema = schema;




        public Action<string> Log = s => { };

        public CommandType DefaultCommandType = CommandType.Text;

        public virtual void Reset(CommandType? comType = null)
        {
            Com = Activator.CreateInstance<TDbCommand>();
            Com.CommandTimeout = CommandTimeout;
            Com.CommandType = comType ?? DefaultCommandType;

            if (comType == CommandType.StoredProcedure)
                AddWithValue("returnInt", null, ParameterDirection.ReturnValue);
        }

        public int? StoredProcedureReturnValue => GetParameterValue("returnInt") as int?;


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

  


        protected T Execute<T>(Func<TDbCommand, T> sqlFunc, string query = null, bool reset = false)
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

            var p = Activator.CreateInstance<TDbParameter>();
            p.ParameterName = parameterName;
            p.Value = CheckCovertUnsignedIfRequired(value);
            p.Direction = direction;

//            var par = new SqlParameter() { ParameterName = parameterName, Value = value, Direction = direction };
            Com.Parameters.Add( p );

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
                query = $"select count(*) from {inSchema(tableName)} WHERE " + string.Join(" AND ", parameters.Keys.Select(x => $"{x} = @{x}"));

                if (ExecuteScalar(query) as int? > 0)
                    return false;
            }

            query = $"INSERT INTO {inSchema(tableName)} ([{string.Join("],[", parameters.Keys)}]) VALUES (@{string.Join(",@", parameters.Keys)})";

            ExecuteNonQuery(query);
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

            var query = $"UPDATE {inSchema(tableName)  } "+ 
                        ( (Com is SqlCommand) ? "WITH ( ROWLOCK ) ": "" ) +
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



        private static object CheckCovertUnsignedIfRequired(object numIn)
        {
            var tt = numIn.GetType();

            var umtch = rexUInt.Match(tt.FullName);

            if (!umtch.Success)
                return numIn;

            var nuTypName = tt.FullName.Replace(umtch.Value, rexUInt.Match(tt.FullName).Value.Replace("UInt", "Int"));
            var nutyp = Type.GetType(nuTypName);

            var nuob = Convert.ChangeType(numIn, nutyp);

            return nuob;
        }
        static Regex rexUInt = new Regex(@"UInt\d{2}");


    }


}