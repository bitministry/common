using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace BitMinistry
{
    public static class ReflectionExtensions 
    {
        public static string GetVariableName<T>(this Expression<Func<T>> memberExpression)
        {
            var expressionBody = (MemberExpression)memberExpression.Body;
            return expressionBody.Member.Name;
        }

        public static void SetPropValue(this PropertyInfo prop, object entity, object value)
        {
            if (prop == null || !prop.CanWrite) return;

            value = (value == DBNull.Value ? null : value);

            if (value == null || prop.PropertyType == typeof(string))
            {
                prop.SetValue(entity, value?.ToString());
                return;
            }

            var pt = (prop.PropertyType.IsGenericType &&
                      prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                ? Nullable.GetUnderlyingType(prop.PropertyType)
                : prop.PropertyType;

            if (pt.IsEnum)
            {
                if (Cnv.IsInt(value.ToString()))
                    prop.SetValue(entity, Cnv.CInt(value));
                else
                    try
                    {
                        prop.SetValue(entity, Enum.Parse(pt, value.ToString()));
                    }
                    catch { }
            }

            if (pt == typeof(bool))
                prop.SetValue(entity, Convert.ToBoolean(value));
            if (pt == typeof(char))
                prop.SetValue(entity, Convert.ToChar(value));
            if (pt == typeof(int))
                prop.SetValue(entity, Cnv.CInt(value));
            if (pt == typeof(short))
                prop.SetValue(entity, Convert.ToInt16(Cnv.CNDec(value)));
            if (pt == typeof(long))
                prop.SetValue(entity, Convert.ToInt64(Cnv.CNDec(value)));
            if (pt == typeof(decimal))
                prop.SetValue(entity, Cnv.CNDec(value));
            if (pt == typeof(double))
                prop.SetValue(entity, Convert.ToDouble(Cnv.CNDec(value)));
            if (pt == typeof(float))
                prop.SetValue(entity, Convert.ToSingle(Cnv.CNDec(value)));
            if (pt == typeof(DateTime))
                prop.SetValue(entity, Cnv.CDate(value));
            if (pt == typeof(bool))
                prop.SetValue(entity, Cnv.CBool(value));

        }

        public static KeyValuePair<string, T> GetNameAndValue<T>( this PropertyInfo prop, object src ) where T : class 
        {
            return new KeyValuePair<string, T>( prop.Name, prop.GetValue(src) as T);
        }


        public static object GetValue(this MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

    }

}
