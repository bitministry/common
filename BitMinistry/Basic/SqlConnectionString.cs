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
            foreach (var pair in connectionString.Split(';').Where(x => x.Contains('=')))
            {
                var pp = pair.Split('=').Select(x => x.Trim()).ToArray();

                var pkey = pp[0]?.ToLower();

                if (" data dource | address | addr | network address ".IndexOf(pkey) > -1)
                    pkey = "server";
                if (" uid | user ".IndexOf(pkey) > -1)
                    pkey = "user id";
                if (pkey == "pwd")
                    pkey = "password";
                if (pkey == "initial catalog")
                    pkey = "database";
                if (pkey == "trusted_connection")
                    pkey = "integrated security";

                _tbl[pkey] = pp[1];
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
