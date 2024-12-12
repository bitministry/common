using BitMinistry.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;

namespace BitMinistry.Data
{
    public class BSqlCommanderUtil
    {

        public PropertyInfo[] GetValidProps<TSqlQueryable>() where TSqlQueryable : ISqlQueryable
            => GetValidProps(typeof(TSqlQueryable)) ;

        public PropertyInfo[] GetValidProps(Type entType)
        {
            var props = entType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (entType.IsValueType)
                props = entType.GetFields().Select(x => new FieldToPropInfo(x)).ToArray();

            return
                props
                    .Where(
                        x =>
                            x.CanWrite && x.GetCustomAttribute<BSqlIgnoreAttribute>() == null && IsValidProp(x.PropertyType))
                    .ToArray();
        }

        public bool IsValidProp(Type prop)
        {
            prop = Nullable.GetUnderlyingType(prop) ?? prop;

            return prop == typeof(string) ||
                            prop == typeof(int) ||
                            prop == typeof(Guid) ||
                            prop == typeof(DateTime) ||
                            prop == typeof(long) ||
                            prop == typeof(bool) ||
                            prop == typeof(float) ||
                            prop.IsEnum ||
                            prop == typeof(Int16) ||
                            prop == typeof(decimal) ||
                            prop == typeof(double) ||
                            prop == typeof(char) ||
                            prop == typeof(byte[]);

        }

            


        public object CheckCovertUnsignedIfRequired(object numIn)
        {
            var tt = numIn.GetType();

            var umtch = rexUInt.Match(tt.FullName);

            if (!umtch.Success)
                return numIn;

            var nuTypName = tt.FullName.Replace(umtch.Value, rexUInt.Match(tt.FullName).Value.Replace("UInt", "Int"));
            var nutyp = Type.GetType(nuTypName);

            var nuob = Convert.ChangeType(numIn, nutyp);

            return nuob;
        }

        static Regex rexUInt = new Regex(@"UInt\d{2}");


        public IDictionary<string, object> PropertiesAndValuesExclusively(IEntity obj, IEnumerable<string> propNames, bool ignoreDefaults = false )
        {
            var objType = obj.GetType();
            var hs = new HashSet<string>( propNames );
            var props = Mirror.GetProps(objType)
                .Where(x =>
                {
                    if (! hs.Contains(x.Name) ) return false;

                    if (!(x.CanWrite && x.GetSetMethod(nonPublic: true).IsPublic) || !x.GetGetMethod(nonPublic: true).IsPublic) return false;

                    if (!(x.PropertyType.IsValueType || x.PropertyType.Name == "String" || x.PropertyType.Name == "Byte[]")) return false;

                    // if ignore default, and is default value 
                    var val = x.GetValue(obj);
                    if (ignoreDefaults &&
                        Equals(val, x.PropertyType.IsValueType ? Activator.CreateInstance(x.PropertyType) : null))
                        return false;

                    return true;

                });

            return ValuesOfProps(props, obj);


        }

        public IDictionary<string, object> PropertiesAndValues(IEntity obj, bool ignoreDefaults = false, string[] ignoreColumns = null )
        {
            var objType = obj.GetType();

            HashSet<string> ignore = ignoreColumns != null ? new HashSet<string>(ignoreColumns) : null;

            var props = Mirror.GetProps(objType)
                .Where(x =>
                {
                    if (!(x.CanWrite && x.GetSetMethod(nonPublic: true).IsPublic) || !x.GetGetMethod(nonPublic: true).IsPublic) return false;

                    if (!(x.PropertyType.IsValueType || x.PropertyType.Name == "String" || x.PropertyType.Name == "Byte[]")) return false;

                    if (ignore != null && ignore.Contains(x.Name)) return false;

                    // if ignore default, and is default value 
                    var val = x.GetValue(obj);
                    if (ignoreDefaults &&
                        Equals(val, x.PropertyType.IsValueType ? Activator.CreateInstance(x.PropertyType) : null))
                        return false;

                    // exclusive custom attributes
                    if (x.GetCustomAttributes()
                        .Any(y => y is BSqlIgnoreAttribute
                                || (y is BEntityIdAttribute attribute && attribute.Seed)))
                        return false;

                    return true;

                });

            return ValuesOfProps(props, obj);


        }

        public IDictionary<string, object> ValuesOfProps<TEntity>(IEnumerable<PropertyInfo> props, TEntity obj) where TEntity : ISqlQueryable =>
            props.ToDictionary(
                    x => x.Name,
                    y =>
                    {
                        var val = y.PropertySqlVal(obj);

                        if (val == null) return null;

                        var strLenCondition = y.GetCustomAttribute<StringLengthAttribute>();
                        if (strLenCondition != null && val.ToString().Length > strLenCondition.MaximumLength)
                            throw new InvalidOperationException( $"{ y.Name } max length {strLenCondition.MaximumLength}");

                        return val;
                    });

        public TSqlQueryable LoadEntityWithPropertyValuesFromObject<TSqlQueryable>(IDataRecord dataRec, PropertyInfo[] orderedProperties) where TSqlQueryable : ISqlQueryable
        {
            var entity = Activator.CreateInstance<TSqlQueryable>();
            for (int i = 0; i < orderedProperties.Length; i++)
                if (dataRec.GetValue(i) != DBNull.Value)
                    SetPropValue(entity, orderedProperties[i], dataRec.GetValue(i));
            //                    orderedProperties[i].SetValue(entity, dataRec.GetValue(i));

            return entity;
        }


        public TSqlQueryable LoadEntityWithPropertyValuesFromDataRow<TSqlQueryable>(DataRow row) where TSqlQueryable : ISqlQueryable
        {
            var entity = Activator.CreateInstance<TSqlQueryable>();
            var tt = typeof(TSqlQueryable);

            foreach (DataColumn col in row.Table.Columns)
            {
                var prop = tt.GetProperty(col.ColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (prop != null)
                    SetPropValue(entity, prop, row[col.ColumnName]);
                    

            }


            return entity;
        }


        public void SetPropValue<TEntity>(TEntity entity, PropertyInfo prop, object value)
        {
            if (value == DBNull.Value) return;

            try
            {
                value = getNullableValue(value, prop.PropertyType);

                prop.SetValue(entity, value);
            }
            catch (Exception ex)
            {
                throw new Exception($"{prop.Name}: {ex.Message}");
            }
        }


        object getNullableValue(object value, Type propType)
        {
            var underLying = Nullable.GetUnderlyingType(propType) ?? propType;

            bool isNullable () => (propType != underLying);

            switch (underLying.Name)
            {
                case "Int32":
                    value = Convert.ToInt32(value);
                    return isNullable() ? (int?)value : value; 

                case "Single":
                    value = Convert.ToSingle(value);
                    return isNullable() ? (float?)value : value;                    

                case "Double":
                    value = Convert.ToDouble(value);
                    return isNullable() ? (double?)value : value;

                case "Decimal":
                    value = Convert.ToDecimal(value);
                    return isNullable() ? (decimal?)value : value;

                case "Int64":
                    return isNullable() ? (long?)value : value;

                case "Boolean":
                    return isNullable() ? (bool?)value : value;

                case "DateTime":
                    return isNullable() ? (DateTime?)value : value;

                case "Guid":
                    return isNullable() ? (Guid?)value : value;

                case "Char":
                    return isNullable() ? (char?)value : value;

                case "Byte[]":
                    return (byte[])value;

                default:
                    if (underLying.IsEnum)
                        return Enum.Parse(underLying, value.ToString()); // Handle Enums

                    return value;
            }

        }


    }





}