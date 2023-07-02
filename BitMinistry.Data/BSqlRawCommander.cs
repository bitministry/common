using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace BitMinistry.Data
{


    ///<summary>
    /// disposable System.Data.SqlClient.SqlCommand facade; a bit heavier than BitMinistry.SqlClient, for SqlParameter utilization, etc 
    ///</summary>
    public class BSqlRawCommander : BSqlCommanderBase<SqlCommand, SqlConnection, SqlParameter> 
    {


        /// <param name="connectionName">if the key dont exist in Config.ConnectionStrings, the connectionName variable is used as a raw Sql connectionString</param>
        public BSqlRawCommander(string connectionName = null, CommandType comType = CommandType.Text, int commandTimeout = 30, string dbSchema = null) :
            base(connectionName: connectionName, comType: comType, commandTimeout: commandTimeout)
        {
            SetDbSchema( dbSchema );
        }


        public override IDataRecord[] GetData(string query = null, bool reset = false)
        {
            
            return Execute(x => x.ExecuteReader().Cast<IDataRecord>().ToArray(), query, reset);
        }

        public SqlInfoMessageEventHandler InfoMessage;

        protected override T ExecuteSql<T>(Func<SqlCommand, T> sqlFunc)
        {
        ExecuteSql:
            using (Com.Connection = new SqlConnection(ConnectionString))
            {
                Com.Connection.InfoMessage += InfoMessage;
                Com.Connection.Open();
                if (Com.Connection.State == ConnectionState.Open)
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
            }

            goto ExecuteSql;
        }

        public override object GetParameterValue(string key)
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







    }



}