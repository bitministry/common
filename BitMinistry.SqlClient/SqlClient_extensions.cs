using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace BitMinistry
{
    public static class SqlClient_extensions
    {


        /// <summary>
        /// SqlClient.ExecuteReader with a selector from IDataRecord 
        /// </summary>   
        public static T[] SqlRead<T>(this string query, Func<IDataRecord, T> selector) => new SqlClient().Read(query, selector);

        /// <summary>
        /// SqlDataAdapter applied to DataSet with selector from DataRow
        /// </summary>   
        public static IEnumerable<T> SqlData<T>(this string query, Func<DataRow, T> selector) => new SqlClient().Data(query).Select( selector );

        /// <summary>
        /// SqlClient.ExecuteNonQuery
        /// </summary>   
        public static int SqlNonQuery(this string query ) => new SqlClient().NonQuery(query);

        /// <summary>
        /// SqlClient.ExecuteScalar, ps. convert DBNull.Value return to null 
        /// </summary>   
        public static object SqlScalar(this string query) => new SqlClient().Scalar(query);

        /// <summary>
        /// execute a function using disposable SqlConnection and SqlCommand 
        /// </summary>   
        public static T SqlCommandFunk<T>(this string query, Func<SqlCommand,T> sqlFunc) => new SqlClient().CommandFunk( sqlFunc, query);



        // datarecord 

        /// <summary>
        /// value or null 
        /// </summary>   
        public static string SafeString(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : record.GetString(index);
        }

        /// <summary>
        /// value or null 
        /// </summary>   
        public static DateTime? SafeDateTime(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : record.GetDateTime(index) as DateTime?;
        }

        /// <summary>
        /// value or null 
        /// </summary>   

        public static decimal SafeDecimal(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? 0 : record.GetDecimal(index);
        }

        /// <summary>
        /// value or null 
        /// </summary>   

        public static double SafeDouble(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? 0 : record.GetDouble(index);
        }

        /// <summary>
        /// value or null 
        /// </summary>   
        public static short? SafeInt16(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : (short?)record.GetInt16(index);
        }

        /// <summary>
        /// value or null 
        /// </summary>   
        public static int? SafeInt32(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : (int?)record.GetInt32(index);
        }

        /// <summary>
        /// value or null 
        /// </summary>   
        public static long? SafeInt64(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : (long?)record.GetInt64(index);
        }

        /// <summary>
        /// value or null 
        /// </summary>   
        public static Guid? SafeGuid(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : record.GetGuid(index) as Guid?;
        }


        /// <summary>
        /// value or null 
        /// </summary>   
        public static bool SafeBool(this IDataRecord record, int index)
        {
            return !record.IsDBNull(index) && record.GetBoolean(index);
        }

        /// <summary>
        /// value or null 
        /// </summary>   
        public static bool? SafeNBool(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : record.GetBoolean(index) as bool?;
        }

        /// <summary>
        /// value or null 
        /// </summary>   
        public static object Safe(this IDataRecord record, int index)
        {
            return record.IsDBNull(index) ? null : record.GetValue(index);
        }


    }


}
