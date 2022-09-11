using Newtonsoft.Json.Linq;
using System.Configuration;
using System.IO;
using System.Linq;

namespace BitMinistry.Common
{
    public class Config
    {
        static SafeDictionary<string, string> _appSettings;

        public static SafeDictionary<string, string> AppSettings {
            get {
                if (_appSettings == null)
                {
                    _appSettings = ConfigurationManager.AppSettings
                        .ToDictionary()
                        .ToSafe();

                    BuildAppSettings( JsonAppSettings, "");

                }

                return _appSettings;
            }
        }


        static SafeDictionary<string, string> _connectionStrings;

        public static SafeDictionary<string, string> ConnectionStrings
        {
            get
            {
                if (_connectionStrings == null)
                {
                    _connectionStrings = ConfigurationManager.ConnectionStrings
                        .Cast<ConnectionStringSettings>()
                        .ToDictionary(x => x.Name, x => x.ConnectionString)
                        .ToSafe();

                    foreach (var jp in JsonAppSettings["ConnectionStrings"].Cast<JProperty>())
                        _connectionStrings.Add(jp.Name, jp.Value.ToString() );

                }

                return _connectionStrings;
            }
        }

        static JObject _jsonAppSettings = null;
        public static JObject JsonAppSettings
        {
            get {

                if (_jsonAppSettings != null) return _jsonAppSettings;
                
                if (File.Exists("appsettings.json") )
                    _jsonAppSettings = JObject.Parse(File.ReadAllText("appsettings.json"));

                return _jsonAppSettings;
            }
        
        }

        public virtual JToken this[string key] => JsonAppSettings?[key];

        static void BuildAppSettings(JToken oo, string name)
        {

            switch (oo.GetType().Name)
            {
                case nameof(JObject):

                    foreach (var jt in (oo as JObject))
                        BuildAppSettings(jt.Value, name + "." + jt.Key);

                    break;

                case nameof(JArray):

                    _appSettings[name.Substring(1)] = string.Join(",", ((JArray)oo).Children());

                    break;

                case nameof(JValue):
                    
                    _appSettings[name.Substring(1)] = oo.ToString();

                    break;


            }



        }

    }
}
