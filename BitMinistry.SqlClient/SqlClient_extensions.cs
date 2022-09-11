using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace BitMinistry
{
    public static class SqlClient_extensions
    {


        public static T[] SqlRead<T>(this string query, Func<IDataRecord, T> selector) => new SqlClient().Read(query, selector);

        public static IEnumerable<T> SqlData<T>(this string query, Func<DataRow, T> selector) => new SqlClient().Data(query).Select( selector );

        public static int SqlNonQuery(this string query ) => new SqlClient().NonQuery(query);

        public static object SqlScalar(this string query) => new SqlClient().Scalar(query);
        public static T SqlCommandFunk<T>(this string query, Func<SqlCommand,T> sqlFunc) => new SqlClient().CommandFunk( sqlFunc, query);



        // datarecord 

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


    }


}
