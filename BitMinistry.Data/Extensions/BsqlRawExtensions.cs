using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;


namespace BitMinistry.Data
{
    public static class BsqlRawExtensions
    {
        public static IEnumerable<T> SelectFromSql<T>(this string query, Func<IDataRecord, T> selector, object[,] pars, CommandType type = CommandType.Text ) {

            
            using (var sql = new BSqlRawCommander(comType: type ))
            {
                for (int i = 0; i < pars.Length / 2; i++)
                    sql.AddWithValue(pars[i, 0].ToString(), pars[i, 1]);

                return sql.GetData(query).Select(selector);
            }


        }

        public static IEnumerable<T> SelectFromSql<T>(this string query, Func<IDataRecord, T> selector, IDictionary<string, object> pars = null, CommandType type = CommandType.Text, string connectionName = null)
        {
            using (var sql = new BSqlRawCommander(comType: type, connectionName: connectionName))
            {
                if (pars != null)
                    foreach (var k in pars.Keys)
                        sql.AddWithValue(k, pars[k]);
                return sql.GetData(query).Select(selector);
            }
        }

        public static DataRow[] GetDataRows(this string query, IDictionary<string, object> pars = null, CommandType type = CommandType.Text)
        {
            using (var sql = new BSqlRawCommander(comType: type))
            {
                if (pars != null)
                    foreach (var k in pars.Keys)
                        sql.AddWithValue(k, pars[k]);
                return sql.GetDataRows(query);
            }
        }

        public static int ExecuteSqlNonQuery(this string query, CommandType type = CommandType.Text, params KeyValuePair<string, object>[] pars)
        {
            using (var sql = new BSqlRawCommander(comType: type))
            {
                if (pars != null)
                    foreach (var k in pars)
                        sql.AddWithValue(k.Key, k.Value);

                return sql.ExecuteNonQuery(query);
            }
        }

        public static int ExecuteSqlNonQuery(this string query, object[,] pars = null, CommandType type = CommandType.Text)
        {
            using (var sql = new BSqlRawCommander(comType: type))
            {
                if (pars != null) sql.AddParameters(pars);

                return sql.ExecuteNonQuery(query);
            }
        }

        public static object SqlScalar(this string query, CommandType type = CommandType.Text)
        {
            using (var sql = new BSqlRawCommander(comType: type))
                return sql.ExecuteScalar(query);
        }



        public static object PropertySqlVal(this PropertyInfo y, object obj)
        {
            var val = y.GetValue(obj);

            if (val == null)
                return null;

            var pt = (y.PropertyType.IsGenericType &&
                      y.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                ? Nullable.GetUnderlyingType(y.PropertyType)
                : y.PropertyType;

            if (pt.IsEnum)
                return val.ToString();

            return val;

        }




    }

}