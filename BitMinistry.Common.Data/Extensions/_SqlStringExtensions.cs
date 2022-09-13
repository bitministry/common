using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;


namespace BitMinistry.Common.Data
{
    public static class SqlExtensions
    {

        public static string Sql(this object obj)
        {
            if (obj == null) return "NULL";

            decimal number;
            
            if (decimal.TryParse(Convert.ToString(obj), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out number))
                return obj.ToString().Replace(",", ".");

            return "'" + obj.ToString().Replace("'", "''") + "'";
        }
        public static string Sql(this decimal? dd)
        {
            return dd?.ToString().Replace(",", ".") ?? "NULL";
        }
        public static string Sql(this decimal dd)
        {
            return dd.ToString(CultureInfo.InvariantCulture).Replace(",", ".");
        }

        public static String ParameterValueForSql(this SqlParameter sp)
        {
            String retval = "";

            switch (sp.SqlDbType)
            {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.Time:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                    retval = "'" + sp.Value.ToString().Replace("'", "''") + "'";
                    break;

                case SqlDbType.Bit:
                    retval = (sp.Value as bool? ?? false) ? "1" : "0";
                    break;

                default:
                    retval = sp.Value.ToString().Replace("'", "''");
                    break;
            }

            return retval;
        }

        public static SqlParameter ToStringArrayParameter(this string[] ina, string parName)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("string", typeof(string)));

            foreach (var id in ina)
                dataTable.Rows.Add(id);

            return new SqlParameter()
            {
                ParameterName = parName,
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.stringArray",
                Value = dataTable
            };
        }

        public static SqlParameter ToStringCols2Parameter(this IEnumerable<KeyValuePair<string, string>> ina, string parName)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("string1", typeof(string)));
            dataTable.Columns.Add(new DataColumn("string2", typeof(string)));

            foreach (var id in ina)
                dataTable.Rows.Add(id.Key, id.Value);

            return new SqlParameter()
            {
                ParameterName = parName,
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.stringCols2",
                Value = dataTable
            };
        }

        public static SqlParameter ToStringAndLongParameter(this IEnumerable<KeyValuePair<string, long>> ina, string parName)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("string", typeof(string)));
            dataTable.Columns.Add(new DataColumn("long", typeof(long)));

            foreach (var id in ina)
                dataTable.Rows.Add(id.Key, id.Value);

            return new SqlParameter()
            {
                ParameterName = parName,
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.stringAndLong",
                Value = dataTable
            };
        }


        public static String CommandAsSql(this IDbCommand sc, bool scriptReturnValue = false, bool scriptUseDb = false)
        {
            StringBuilder sql = new StringBuilder();
            Boolean FirstParam = true;

            if (scriptReturnValue)
                sql.Append("use " + sc.Connection.Database + " ");
            switch (sc.CommandType)
            {
                case CommandType.StoredProcedure:
                    if (scriptReturnValue)
                        sql.Append(" declare @return_value int ");

                    foreach (SqlParameter sp in sc.Parameters)
                    {
                        if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
                        {
                            sql.Append(" declare " + sp.ParameterName + " " + sp.DbType + " ");

                            sql.Append(((sp.Direction == ParameterDirection.Output) ? " null" : sp.ParameterValueForSql()) + " ");

                        }
                    }

                    sql.Append("exec " + sc.CommandText + " ");

                    foreach (SqlParameter sp in sc.Parameters)
                    {
                        if (sp.Direction != ParameterDirection.ReturnValue)
                        {
                            sql.Append((FirstParam) ? " " : " , ");

                            if (FirstParam) FirstParam = false;

                            if (sp.Direction == ParameterDirection.Input)
                                sql.Append(sp.ParameterName + " = " + sp.ParameterValueForSql() + " ");
                            else

                                sql.Append(sp.ParameterName + " = " + sp.ParameterName + " output ");
                        }
                    }

                    if (scriptReturnValue)
                        sql.Append(" select 'Return Value' = convert(varchar, @return_value) ");

                    foreach (SqlParameter sp in sc.Parameters)
                    {
                        if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
                            sql.Append(sp.ParameterName + " = " + sp.ParameterValueForSql() + " ");
                    }
                    break;
                case CommandType.Text:
                    sql.Append(sc.CommandText);
                    if (sc.Parameters.Count > 0)
                        sql.Append(Environment.NewLine + string.Join(",", sc.Parameters.Cast<SqlParameter>().Select(x => $"{x.ParameterName}={x.ParameterValueForSql()}")));

                    break;
            }

            return sql.ToString();
        }

        public static string SafeString(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : record.GetString(index);
        }

        public static DateTime? SafeDateTime(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : record.GetDateTime(index) as DateTime?;
        }

        public static decimal SafeDecimal(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? 0 : record.GetDecimal(index);
        }

        public static double SafeDouble(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? 0 : record.GetDouble(index);
        }

        public static short? SafeInt16(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : (short?)record.GetInt16(index);
        }
        public static int? SafeInt32(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : (int?)record.GetInt32(index);
        }
        public static long? SafeInt64(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : (long?)record.GetInt64(index);
        }

        public static Guid? SafeGuid(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : record.GetGuid(index) as Guid?;
        }


        public static bool SafeBool(this IDataRecord record, int index)
        {
            return !record.IsDBNull(index) && record.GetBoolean(index);
        }
        public static bool? SafeNBool(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : record.GetBoolean(index) as bool?;
        }

        public static object Safe(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : record.GetValue(index);
        }


        public static void AddWithNullableValue(this SqlParameterCollection collection, string parameterName, object value)
        {
            if (value == null) value = DBNull.Value;
            collection.AddWithValue(parameterName, value);
        }


    }

}