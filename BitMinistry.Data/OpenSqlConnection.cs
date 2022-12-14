using System;
using System.Data;
using System.Data.SqlClient;

namespace BitMinistry.Data
{
    public class OpenSqlConnection  : IDisposable 
    {
        public SqlConnection Connection {  get; }
        private readonly bool _closeOnDispose;

        public OpenSqlConnection(SqlConnection connection, bool closeOnDispose = false )
        {
            this.Connection = connection;
            this._closeOnDispose = closeOnDispose;
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Close();
                Connection.Open();
            }
            
        }

        public void Dispose()
        {
            if (_closeOnDispose && Connection.State != ConnectionState.Closed )
                Connection.Close();
        }
    }
}
