using System;
using System.Configuration;

namespace BitMinistry.Web
{
    public class IpLocation
    {
        private static string _sqlConnection;
        public static string SqlConnectionString {
            get {
                return _sqlConnection ??
                       (_sqlConnection = ConfigurationManager.ConnectionStrings["infra"]?.ConnectionString);
            }
            set { _sqlConnection = value; }
        }

        public int? IpInt { get; set; }
        public string IpV4 { get; set; }
        public string CountryName { get; set; }
        private string _countryCode;

        public string CountryCode
        {
            get { return _countryCode ?? "EE"; }
            set { _countryCode = value; }
        }

        public string RegionName { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
//        public string TimeZone { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }

        public bool IsInitialized { get; set; }

        public string ToLocation()
        {
            return $"{ZipCode} {City}, {RegionName}, {CountryName}";
        }
    }
}