using System;
using Newtonsoft.Json;

namespace BitMinistry
{

    public class NewtonNumberFormatConverter : JsonConverter
    {
        private readonly string _format;

        public NewtonNumberFormatConverter(string format = "{0:N2}")
        {
            this._format = format;
        }

        public override bool CanConvert(Type objectType)
        {
            var tt = (objectType.IsGenericType &&
                      objectType.GetGenericTypeDefinition() == typeof (Nullable<>))
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

            return (
                tt == typeof(Int16) ||
                tt == typeof(Int32) ||
                tt == typeof(Int64) ||
                tt == typeof(decimal) ||
                tt == typeof(float) ||
                tt == typeof(double) );
        }

        public override void WriteJson(JsonWriter writer, object value,
                                       JsonSerializer serializer)
        {
            writer.WriteValue(string.Format(_format, value));

        }

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType,
                                     object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
