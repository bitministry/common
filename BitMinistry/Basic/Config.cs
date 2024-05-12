using BitMinistry;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace BitMinistry 
{


    /// <summary>
    /// abstraction over web.config, app.config and appsettings.json
    /// including (default) sqlConnectionString manager 
    /// </summary> 
    public class Config
    {
        static SafeDictionary<string, string> _appSettings;

        /// <summary>
        /// across ConfigurationManager.AppSettings and JsonAppSettings
        /// </summary> 
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

        /// <summary>
        /// across ConfigurationManager.ConnectionStrings and JsonAppSettings["ConnectionStrings"]
        /// </summary> 
        public static SafeDictionary<string, string> ConnectionStrings
        {
            get
            {
                if (_connectionStrings == null)
                {
                    _connectionStrings = ConfigurationManager.ConnectionStrings
                        .Cast<ConnectionStringSettings>()
                        .ToDictionary(x => x.Name.ToLower(), x => x.ConnectionString)
                        .ToSafe();

                    foreach (var jp in JsonAppSettings?["ConnectionStrings"]?.Cast<JProperty>() ?? new JProperty[0])
                        _connectionStrings[jp.Name.ToLower()] = jp.Value.ToString() ;

                }

                return _connectionStrings;
            }
        }


        public static string HardDefaultSqlConnectionString;

        public static string DefaultSqlConnectionString {
            get => HardDefaultSqlConnectionString ?? ConnectionStrings[DefaultSqlConnectionName];
            set {
                _defaultSqlConnectionName = DefaultSqlConnectionName ?? "main";
                ConnectionStrings[DefaultSqlConnectionName] = value;  
            }
        }

        static string _defaultSqlConnectionName;
        /// <summary>
        /// an existing key from ConnectionStrings: 'main' ?? 'default' ?? 'defaultconnection' ?? connectionstrings.first( not 'localsqlserver')
        /// </summary> 
        public static string DefaultSqlConnectionName
        {
            get
            {
                if (_defaultSqlConnectionName != null) return _defaultSqlConnectionName;

                foreach (var key in new[] { "main", "default", "defaultconnection" })
                    if (ConnectionStrings.ContainsKey(key)) _defaultSqlConnectionName = key;

                _defaultSqlConnectionName = _defaultSqlConnectionName ??
                        ConnectionStrings.FirstOrDefault(x => x.Key != "localsqlserver").Key;

                // _defaultSqlConnectionName.ThrowIfNull("No connection strings");
                
                return _defaultSqlConnectionName;
            }
            set => _defaultSqlConnectionName = value;
        }

        /// <summary>
        /// get a (modified) connectionstring by key; 
        /// modified connection is placed in the dictionary under: [key:property:newvalue], eg [defaultconnection:database:whatnot]
        /// </summary> 
        /// <param name="connectionName">the key in ConnectionStrings</param>
        /// <param name="nuConf">property name and value, you want to change</param>
        public static string SqlConnectionString( string connectionName = null, KeyValuePair<string,string>? nuConf = null ) {

            connectionName = connectionName ?? DefaultSqlConnectionName;

            if (!ConnectionStrings.ContainsKey(connectionName)) return null;

            if (nuConf != null )
            {
                var confIn = nuConf.Value;
                string confKey() => $"{connectionName}:{confIn.Key}:{confIn.Value}";

                // if already built, return it 
                if ( ConnectionStrings.ContainsKey( confKey() ) )
                    return ConnectionStrings[ confKey() ];

                // build new connection string, and return it 
                var cs = new SqlConnectionStringBuilder(ConnectionStrings[connectionName]);
                cs[confIn.Key] = confIn.Value;
                ConnectionStrings[confKey()] = cs.ToString();

                return ConnectionStrings[confKey()];
            }

            return ConnectionStrings[connectionName];

        }

        static JObject _jsonAppSettings = null;

        /// <summary>
        /// build settings from File.ReadAllText("appsettings.json")
        /// </summary> 
        public static JObject JsonAppSettings
        {
            get {

                if (_jsonAppSettings != null) return _jsonAppSettings;
                
                if (File.Exists("appsettings.json") )
                    _jsonAppSettings = JObject.Parse(File.ReadAllText("appsettings.json"));

                return _jsonAppSettings;
            }
        
        }


        /// <summary>
        /// JsonAppSettings?[key]
        /// </summary> 
        public virtual JToken this[string key] => JsonAppSettings?[key];

        static void BuildAppSettings(JToken oo, string name)
        {

            switch (oo?.GetType().Name)
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
