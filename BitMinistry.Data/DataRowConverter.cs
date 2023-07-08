using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Data;

namespace BitMinistry.Data
{

    public class DataRowConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DataRow);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DataRow row = (DataRow)value;

            JObject obj = new JObject();
            foreach (DataColumn column in row.Table.Columns)
            {
                object columnValue = row[column];
                if (columnValue != DBNull.Value && columnValue != null)
                {
                    obj[column.ColumnName] = JToken.FromObject(columnValue);
                }
            }

            obj.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;



        public static string GetJson(DataRow[] data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            settings.Converters.Add(new DataRowConverter());
            settings.Formatting = Formatting.Indented;

            string json = JsonConvert.SerializeObject(data, settings);

            return json;
        }

    }



}
