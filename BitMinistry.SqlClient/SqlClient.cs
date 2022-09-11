using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Configuration;
using Newtonsoft.Json.Linq;


namespace BitMinistry
{
    public class SqlClient
    {

        // #if NETFRAMEWORK

        public static string GlobConnectStr => ConfigurationManager.ConnectionStrings["main"]?.ConnectionString ??
                (File.Exists("appsettings.json") ?
                    JObject.Parse(File.ReadAllText("appsettings.json"))["ConnectionStrings"]?["main"]?.ToString()
                    : null);


        public SqlClient(string connectionString = null )
        {
            ConnectionProps = new ConnectionStringComponents(connectionString ?? GlobConnectStr);

            if (ConnectionProps["server"] == "")
                throw new ArgumentNullException("Connection string required");
        }

        public T[] Read<T>(string query, Func<IDataRecord, T> selector)
        {

            return CommandFunk(x => x.ExecuteReader().Cast<IDataRecord>().Select(selector).ToArray(), query);
        }

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

        public virtual int NonQuery(string query )
        {
            return CommandFunk(com => com.ExecuteNonQuery(), query);
        }
        public virtual object Scalar(string query )
        {
            return CommandFunk(com =>
            {
                var ret = com.ExecuteScalar();
                return ret == DBNull.Value ? null : ret;
            }, query);
        }

        public T CommandFunk<T>(Func<SqlCommand, T> sqlFunc, string query)
        {

            using (var connection = new SqlConnection(ConnectionProps.ToString()))
            using (var sqlCom = new SqlCommand(query)
            {
                Connection = connection
            })
            {
                connection.Open();
                return sqlFunc(sqlCom);
            }

        }

        public ConnectionStringComponents ConnectionProps { get; private set; }

        public class ConnectionStringComponents {
            readonly Dictionary<string, string> _tbl = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            public ConnectionStringComponents(string xs ) {
                
                xs = xs.ToLower();

                foreach (var pair in xs.Split(';').Where( x => x.Contains('=') ))
                {
                    var pp = pair.Split('=').Select( x=>x.Trim() ).ToArray();

                    if (" data dource | address | addr | network address ".IndexOf(pp[0]) > -1)
                        pp[0] = "server";
                    if (" uid | user ".IndexOf(pp[0]) > -1)
                        pp[0] = "user id";
                    if (pp[0] == "pwd")
                        pp[0] = "password";
                    if (pp[0] == "initial catalog" )
                        pp[0] = "database";
                    if (pp[0] == "trusted_connection")
                        pp[0] = "integrated security";

                    _tbl[pp[0]] = pp[1];
                }


            }

            public string this[string key]
            {
                get => _tbl.ContainsKey(key) ? _tbl[key] : "";
                set => _tbl[key] = value;            
            }

            public override string ToString() => string.Join(";", _tbl.Select( x => $"{x.Key}={x.Value}" ));

        }

    }


}
