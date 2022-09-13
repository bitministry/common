using System;
using System.Collections.Generic;
using System.Linq;

namespace BitMinistry
{

    /// <summary>
    /// System.Data.SqlClient.SqlConnectionStringBuilder lifted higher, without the rest of the library 
    /// </summary> 
    public class SqlConnectionStringBuilder

    {

        readonly Dictionary<string, string> _tbl = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// MSSQL ConnectionString is required 
        /// </summary> 
        public SqlConnectionStringBuilder(string connectionString  ) 
        {
            connectionString = connectionString?.ToLower();

            foreach (var pair in connectionString.Split(';').Where(x => x.Contains('=')))
            {
                var pp = pair.Split('=').Select(x => x.Trim()).ToArray();

                if (" data dource | address | addr | network address ".IndexOf(pp[0]) > -1)
                    pp[0] = "server";
                if (" uid | user ".IndexOf(pp[0]) > -1)
                    pp[0] = "user id";
                if (pp[0] == "pwd")
                    pp[0] = "password";
                if (pp[0] == "initial catalog")
                    pp[0] = "database";
                if (pp[0] == "trusted_connection")
                    pp[0] = "integrated security";

                _tbl[pp[0]] = pp[1];
            }

            build();

        }

        /// <summary>
        /// change a property 
        /// </summary> 
        public string this[string key]
        {
            get => _tbl.ContainsKey(key) ? _tbl[key] : "";
            set { 
                _tbl[key] = value; 
                build(); 
            }
        }

        string build() => _connectionString = string.Join(";", _tbl.Select(x => $"{x.Key}={x.Value}"));

        string _connectionString;

        /// <summary>
        /// extract ConnectionString 
        /// </summary> 
        public override string ToString() => _connectionString;

    }
}
