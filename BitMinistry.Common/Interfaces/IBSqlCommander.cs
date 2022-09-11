using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitMinistry.Common.Interfaces
{
    public interface IBSqlCommander
    {
        void SetDbSchema(string schema);
        object ExecuteScalar(string query = null, bool reset = false);
        bool InsertToTable(IDictionary<string, object> parameters, string tableName, bool avoidDuplicate = false);
        int UpdateAtTable(IDictionary<string, object> parameters, string tableName, string idColumn, object idValue);
        int UpdateAtTable(IDictionary<string, object> parameters, string tableName, string updateWhere, bool reset = true);

    }
}
