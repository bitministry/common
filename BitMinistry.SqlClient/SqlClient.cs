using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace BitMinistry
{
    /// <summary>
    /// Slim MS Sql adapter 
    /// </summary>  
    public class SqlClient
    {

        string _connectionString;

        /// <summary>
        /// provide a local connection string, if required 
        /// </summary>   
        public SqlClient( string connectionName = null )
        {
            _connectionString = Config.ConnectionStrings[connectionName ?? Config.DefaultSqlConnectionName ];

        }

        /// <summary>
        /// ExecuteReader with a selector from IDataRecord 
        /// </summary>   
        public T[] Read<T>(string query, Func<IDataRecord, T> selector)
        {

            return CommandFunk(x => x.ExecuteReader().Cast<IDataRecord>().Select(selector).ToArray(), query);
        }

        /// <summary>
        /// SqlDataAdapter applied to DataSet 
        /// </summary>   
        public IEnumerable<DataRow> Data(string query)
        {
            using (var dataSet = new DataSet())
            using (var adapter = new SqlDataAdapter())
                return CommandFunk( com =>
                {
                    adapter.SelectCommand = com;
                    adapter.Fill(dataSet);
                    return dataSet.Tables[0].Rows.Cast<DataRow>();
                }, query);
        }

        /// <summary>
        /// ExecuteNonQuery 
        /// </summary> 
        public virtual int NonQuery(string query )
        {
            return CommandFunk(com => com.ExecuteNonQuery(), query);
        }

        /// <summary>
        /// ExecuteScalar, ps. convert DBNull.Value return to null 
        /// </summary> 
        public virtual object Scalar(string query )
        {
            return CommandFunk(com =>
            {
                var ret = com.ExecuteScalar();
                return ret == DBNull.Value ? null : ret;
            }, query);
        }

        /// <summary>
        /// execute a function using disposable SqlConnection and SqlCommand 
        /// </summary> 
        public T CommandFunk<T>(Func<SqlCommand, T> sqlFunc, string query)
        {

            using (var connection = new SqlConnection(_connectionString))
            using (var sqlCom = new SqlCommand(query)
            {
                Connection = connection
            })
            {
                connection.Open();
                return sqlFunc(sqlCom);
            }

        }



    }


}
