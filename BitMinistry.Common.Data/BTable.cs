using System.Collections.Generic;
using System.Linq;

namespace BitMinistry.Common.Data
{

    public class BTable
    {
        public BTable(string name)
        {
            Name = name;

            Columns = "SELECT COLUMN_NAME, DATA_TYPE , CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE".SelectFromSql(
                x => new Column()
                {
                    Name = x.GetString(0),
                    DataType = x.GetString(1),
                    MaxLength = x.SafeInt32(2),
                    Precision = x.SafeInt32(3),
                    Scale = x.SafeInt32(4),
                }).ToDictionary(x => x.Name, x => x);

        }

        public Dictionary<string, Column> Columns { get; }

        public string Name { get; }
    }

}
