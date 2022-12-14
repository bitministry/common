using BitMinistry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BitMinistry.Utility
{
    public static class Mirror
    {

        public enum InjectRegulation { None, ExcludingNulls, ExcludingDefaults };

        /// <summary>
        /// Inject source properties into the target object
        /// </summary>
        public static void InjectInto<TType>(this object source, ref TType target, InjectRegulation injectRegulation = InjectRegulation.None )
        {
            CopyProperties(source, ref target, source.GetType().GetProperties(), injectRegulation);
        }


        /// <summary>
        /// Inject source properties into the target object, refined by property names
        /// </summary>

        public static void InjectInto<TType>(this object source, ref TType target, string[] props, InjectRegulation injectRegulation = InjectRegulation.None)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var sourceProps = source.GetType().GetProperties();

            if (props != null)
                sourceProps = sourceProps.AsEnumerable().Where(x => props.Contains(x.Name)).ToArray();

            CopyProperties(source, ref target, sourceProps, injectRegulation );
        }

        private static void CopyProperties<TType>(object source, ref TType target, IEnumerable<PropertyInfo> sourceProps, InjectRegulation injectRegulation )
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var targetType = typeof( TType ) ;
            if (target == null) target = Activator.CreateInstance<TType>();
            var clone = (object) target; 

            foreach (var sp in sourceProps)
            {
                if (sp.CanRead && sp.GetGetMethod() != null)
                {
                    var tp = targetType.GetProperty(sp.Name);

                    if (tp != null && tp.CanWrite && sp.PropertyType == tp.PropertyType && tp.GetSetMethod() != null)
                    {
                        var srcValue = sp.GetValue(source, null);
                        if (injectRegulation == InjectRegulation.ExcludingNulls && srcValue == null) continue;
                        if (injectRegulation == InjectRegulation.ExcludingDefaults )
                            if (srcValue == null || tp.PropertyType.IsValueType && Equals(srcValue, Activator.CreateInstance(tp.PropertyType) ) ) continue; 

                        tp.SetValue( clone, srcValue, null);
                    }
                }
            }
            target = (TType)clone; 

        }

        /// <summary>
        /// Create TType object and inject source properties into the target object; refine by property names
        /// </summary>

        public static TTarget GetClone<TTarget>(this object source, string[] props = null, InjectRegulation injectRegulation = InjectRegulation.None ) where TTarget : new()
        {
            var target = new TTarget();
            source.InjectInto(ref target, props, injectRegulation );
            return target;
        }

        /// <summary>
        /// deep copy 
        /// </summary>
        public static TType GetClone<TType>(this TType source, string[] props = null, InjectRegulation injectRegulation = InjectRegulation.None) where TType : new()
        {
            var target = new TType();
            source.InjectInto(ref target, props, injectRegulation);
            return target;
        }


        public static MemberInfo GetMember<T>(Expression<Func<T, object>> expression)
        {
            if (expression == null) return null;
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                var body = (UnaryExpression)expression.Body;
                memberExpression = body.Operand as MemberExpression;
                if (memberExpression == null)
                {
                    memberExpression = (body.Operand as BinaryExpression)?.Left as MemberExpression;
                }
            }
            memberExpression.ThrowIfNull("cant get member name from Lambda expression!");

            return memberExpression.Member;
        }

        public static string GetMemberName<T>(Expression<Func<T, object>> expression)
            => GetMember(expression)?.Name;

        public static IEnumerable<PropertyInfo> GetPropsWithAttribute<TAttr>(Type type) where TAttr : Attribute
        {
            return GetProps(type).Where(prop => prop.GetCustomAttribute<TAttr>() != null);
        }

        public static PropertyInfo[] GetProps(Type type)
        {
            return type.IsValueType ?
                    type.GetFields().Select(x => new FieldToPropInfo(x)).ToArray()
                    : type.GetProperties();
        }

    }



    public class FieldToPropInfo : PropertyInfo
    {
        public FieldToPropInfo(FieldInfo ff) { f = ff; }
        FieldInfo f;
        public override Type PropertyType => f.FieldType;

        public override PropertyAttributes Attributes => PropertyAttributes.None;
        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override string Name => f.Name;

        public override Type DeclaringType => f.DeclaringType;

        public override Type ReflectedType => f.ReflectedType;

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return f.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return f.GetCustomAttributes(attributeType, inherit);
        }

        public override MethodInfo GetGetMethod(bool nonPublic) => (GetType().GetMethod(nameof(Equals)));

        public override ParameterInfo[] GetIndexParameters()
        {
            return new ParameterInfo[0];
        }

        public override MethodInfo GetSetMethod(bool nonPublic) => (GetType().GetMethod(nameof(Equals)));

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            return f.GetValue(obj);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return f.IsDefined(attributeType, inherit);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            f.SetValue(obj, value, invokeAttr, binder, culture);
        }


    }


}
