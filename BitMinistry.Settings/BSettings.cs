using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using BitMinistry;
using BitMinistry.Data;


namespace BitMinistry.Settings
{

    ///<summary>
    /// manage your settings in sql database
    ///</summary>    
    public class BSettings
    {

        public static string sBasePath => HttpContext.Current?.Server?.MapPath(HttpContext.Current.Request?.ApplicationPath) ?? Directory.GetCurrentDirectory();

        public static string sBaseURL
        {
            get
            {
                var request = HttpContext.Current.Request;
                return request.Url.Scheme + Uri.SchemeDelimiter + request.Url.Host + request.ApplicationPath + '/';

            }
        }

        ///<summary>
        /// use BSettings only as a proxy to BitMinistry.Congig.AppSettings 
        ///</summary>
        public static bool IgnoreDatabase { get; set; }

        public static SafeDictionary<string, Setting> All { get; private set; }

        static string _connectionName;

        ///<summary>
        /// BitMinistry.Config.ConnectionStrings["infra"] for default; 
        /// excludeModules - settings where name not like '{moduleName}:%'
        ///</summary>
        public static void Init ( string connectionName = null, string[] excludeModules = null )
        {

            if (All == null && ! IgnoreDatabase )
            {
                _excludeModules = excludeModules;

                _connectionName = connectionName ?? _connectionName ??
                    (Config.ConnectionStrings.ContainsKey("infra") ? "infra" : "main") ;


                using (var sql = new BSqlRawCommander(_connectionName ))
                    if (sql.ExecuteScalar("select OBJECT_ID('bm.Setting')") != DBNull.Value)
                        InitHashSet();

            }
            InitIsCalled = true;
        }


        ///<summary>
        /// ExecuteSqlFileWithGoStatements("create_tables.sql");
        ///</summary>
        public static void CreateTables()
        {

            using (var sql = new BSqlRawCommander(_connectionName))
                sql.ExecuteSqlFileWithGoStatements("create_tables.sql");
        }

        public static void Reset()
        {
            All = null;
            Init();
        }

        static string[] _excludeModules;

        public static bool InitIsCalled { get; private set; }
        
        public static IList<VendorSource> Source(string type, string target = "", string titleWhereCondition = "")
        {
            using (var sql = new BSqlRawCommander(_connectionName))
                return sql.GetData(
                    $"SELECT Type, Title, Url, Target, Lang FROM bm.Source WHERE Type = '{type}' and Target like '%{target}%' {titleWhereCondition} ORDER BY Ranking DESC")
                    .Select(x => new VendorSource
                    {
                        Type = x.SafeString(0),
                        Title = x.SafeString(1),
                        Url = x.SafeString(2),
                        Target = x.SafeString(3),
                        Lang = x.SafeString(4)
                    }).ToList();
        }

        private static void InitHashSet()
        {
            var t = new Setting();

            var where = $"({nameof(t.NTextValue)} IS NOT NULL OR {nameof(t.NumericValue)} IS NOT NULL OR {nameof(t.DateTimeValue)} IS NOT NULL) ";

            where += _excludeModules?.Length > 0 ? $" AND {nameof(t.Name)} not like '{string.Join($":%' and {nameof(t.Name)} not like '", _excludeModules)}:%'" : "";

            using (var sql = new BSqlRawCommander(_connectionName ))
            {
                All = sql.GetDataRows($"select * from bm.{typeof(Setting).Name} where {where}")
                    .Select( DataRowToSetting )
                .ToDictionary(x => x.Name, x => x).ToSafe();
            }                
        }

        static Setting DataRowToSetting(DataRow row) => new Setting
        {
            Name = row["Name"] as string,
            NTextValue = row["NTextValue"] as string,
            NumericValue = row["NumericValue"] as decimal?,
            DateTimeValue = row["DateTimeValue"] as DateTime?,
        };

        public static Setting GetSetting(string id)
        {
            if (!InitIsCalled) Init();
            var setting = All?[id];

            if (setting == null)
            {
                var value = Config.AppSettings[id];
                setting = new Setting() { Name = id, NTextValue = value };
                if (value.IsNumeric())
                {
                    decimal temp;
                    setting.NumericValue = decimal.TryParse(value, out temp) ? temp : (decimal?)null;
                }

                if (All != null && ! string.IsNullOrEmpty( setting.NTextValue ) )
                    Save(setting);
            }
            return setting;
        }
        public static Setting GetFreshSetting(string name) {

            if (!InitIsCalled)
                return GetSetting(name);
            ReFreshSetting(name);
            return All[name];
        }


        public static void ReFreshSetting(string name)
        {
            using (var sql = new BSqlRawCommander( _connectionName ))
                All[name] = sql.GetDataRows($"select * from bm.{typeof(Setting).Name} where Name ='{name}'")
                    .Select(DataRowToSetting).FirstOrDefault();
                

        }



        ///<summary>
        /// Fetches settings from database or config file 
        /// Writes the setting into the database if its not there yet 
        /// May return null 
        /// save the setting, with default value, if not exists 
        ///</summary>
        public static string Get(string id, string defaultValue = "")
        {
            var ret = Config.AppSettings[id] ?? GetSetting(id).NTextValue;
            if ( ret == null && ! string.IsNullOrEmpty( defaultValue ) )
                Save(new Setting()
                {
                    Name = id,
                    NTextValue = defaultValue
                });

            return ret ?? defaultValue;
        }

        ///<summary>
        /// get int 
        /// save the setting, with default value, if not exists 
        ///</summary>
        public static int GetInt(string id, int defaultValue )
        {
            var val = GetSetting(id).NumericValue;
            if (val == null )
                Save( new Setting()
                {
                    Name = id, 
                    NumericValue = defaultValue 
                });
            return (int) Math.Round( val ?? defaultValue, 0 );
        }


        public static void Save(Setting setting)
        {
            if ( IgnoreDatabase ) return;

            if (setting.Name == null) throw new NullReferenceException("setting.Name");

            var pars = new Dictionary<string, object>() {
                { nameof(setting.Name), setting.Name},
                { nameof(setting.NTextValue), setting.NTextValue },
                { nameof(setting.DateTimeValue), setting.DateTimeValue },
                { nameof(setting.NumericValue), setting.NumericValue },
            };

            using (var sql = new BSqlRawCommander( _connectionName, dbSchema: "bm"))
                if (sql.UpdateAtTable(pars, tableName: "Setting", idColumn: nameof(setting.Name), idValue: setting.Name) == 0)
                    sql.InsertToTable(pars, tableName: "Setting");


            InitHashSet();
        }


        public static void Save(string settingName, string ntextValue = null, DateTime? dateTimValue = null, decimal? numericValue = null )
        {
            Save( new Setting()
            {
                Name = settingName,
                NTextValue = ntextValue ,
                DateTimeValue =  dateTimValue,
                NumericValue =  numericValue 
            });
        }





    }
}
