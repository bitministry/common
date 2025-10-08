using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using static System.String;

namespace BitMinistry
{
    public static class StringExtensions
    {

        // this object 
        public static string CStr(this object obj)
        {
            return obj?.ToString() ?? "";
        }

        // this string  

        public static Guid ToGuid(this string id)
        {
            Guid guidId;
            if (!IsNullOrEmpty(id) && Guid.TryParse(id, out guidId))
            {
                return guidId;
            }
            return Guid.Empty;
        }

        public static int ToBit(this bool bb ) => bb ? 1 : 0;

        public static int? SwipeToNInt(this string exp)
        {
            return exp == null ? null : Cnv.CNInt(Regex.Replace(exp, @"[^-+\d]", ""));
        }

        public static long ToLong(this string number)
        {
            long i;
            Int64.TryParse(number, out i);
            return i;
        }

        public static int ToInt(this string number )
        {
            int i;
            Int32.TryParse(number, out i);
            return i;
        }
        public static int? SafeInt(this string number)
        {
            int i;
            Int32.TryParse(number, out i);
            return i == default(int) ? null : (int?) i;
        }


        public static decimal ToDecimal(this string dec)
        {
            if (IsNullOrEmpty(dec)) return 0;

            return Cnv.CDec(dec);
        }

        public static decimal? FromShorthandDec(this string xs)
        {
            var dec = Cnv.CNDec(xs);

            if (dec == null ) return null;

            xs = xs.Trim();

            switch (xs.Last().ToString().ToLower())
            {
                case "k":
                    dec = dec * 1000;
                    break;
                case "m":
                    dec = dec * 1000000;
                    break;
                case "b":
                    dec = dec * 1000000000;
                    break;
                case "t":
                    dec = dec * 1000000000000;
                    break;
            }
            return dec;
        }

        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }



        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        

        public static bool ThrowIfUnvalidEmail(this string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                throw new Exception( email +" not valid email");
            }
        }

        public static string MaxLength(this string str, int maxLength, bool useDots = true)
        {
            return IsNullOrEmpty(str) || str.Length <= maxLength
                ? str
                : (maxLength < 11 || ! useDots ? str.Substring(0, maxLength) : str.Substring(0, maxLength - 2) + "..");
        }

        public static string CutEnd(this string str, int chars)
        {
            return IsNullOrEmpty(str) || str.Length <= chars
                ? str
                : str.Substring(0, str.Length - chars );
        }


        public static string NewLineToBR(this string str )
        {
            return (str ?? "").Replace( "\n", "<BR>");
        }

        public static string FontAwesomeIcon(this string str)
        {
            switch (Path.GetExtension(str))
            {
                case ".pdf":
                    return "fa-file-pdf-o";
                case ".xls":
                case ".xlsx":
                case ".ods":
                case ".ots":
                    return "fa-file-excel-o";
                case ".doc":
                case ".docx":
                case ".rtf":
                case ".odt":
                case ".ott":
                case ".oth":
                case ".odm":
                case ".wpd":
                case ".wps":
                case ".txt":
                    return "fa-file-word-o";
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".bmp":
                case ".png":
                case ".tiff":
                case ".tif":
                case ".cpt":
                case ".psd":
                case ".psp":
                case ".cdr":
                case ".ai":
                case ".xps":
                case ".svg":
                    return "fa-file-image-o";
                case ".zip":
                case ".rar":
                case ".7z":
                    return "fa-file-zip-o";
                case ".ppt":
                case ".pps":
                case ".pptx":
                case ".ppsx":
                    return "fa-file-powerpoint-o";
                default:
                    return "fa-file-o";
            }
        }




        public static bool IsNumeric(this string value)
        {
            decimal nr;
            return decimal.TryParse(value, NumberStyles.Any, new NumberFormatInfo(), out nr);
        }
        public static bool IsInteger(this string value)
        {
            int nr;
            return Int32.TryParse(value, out nr);
        }

        // private static readonly Regex _isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

        public static bool IsGuid(this string candidate) => Cnv.IsGuid(candidate);

        public static bool IsAbsoluteUrl(this string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
            {
                return uriResult.IsAbsoluteUri;
            }

            return false;
        }

        public static string HttpChk( this string str )
        {
            if (IsNullOrEmpty(str)) return str;

            str = str.StartsWith("http")
                ? str
                : ("http://" + str);

            while (str.Contains("///"))
                str = str.Replace("///", "//");

            return str;
        }

        public static string CleanUrl(this string str)
        {
            

            if (Uri.IsWellFormedUriString(str, UriKind.Absolute))
            {
                var uri = new Uri(str, UriKind.RelativeOrAbsolute);
                return uri.Authority + uri.PathAndQuery.TrimEnd('/');
            }
            if (Uri.IsWellFormedUriString(str, UriKind.Relative))
                return str.TrimEnd('/');

            return null;
        }


        public static bool IsDate(this string value)
        {
            DateTime dt;
            return DateTime.TryParse(value, out dt);
        }


  

        private static Regex reDangerous = new Regex("<.*runat.*>|<script.*>|<embed.*>|<object.*>|<applet.*>", RegexOptions.IgnoreCase);

        public static string RemDangrHtml(this string xsIn)
        {
            return reDangerous.Replace(xsIn ?? "", "");
        }

        public static string RemHtml(this string xsIn)
        {
            return Regex.Replace(xsIn, "<.*?>", String.Empty);
        }

        public static string SqlSanitize(this string xsIn)
        {
            return xsIn.RemDangrHtml().Replace("'", "''");
        }


 


        public static T ToEnum<T>(this string str) where T : struct, IConvertible
        {
            try
            {
                var res = (T)Enum.Parse(typeof(T), str);
                return !Enum.IsDefined(typeof(T), res) ? default(T) : res;
            }
            catch
            {
                return default(T);
            }
        }

        public static DateTime CDate(this string str) => ToDateTime(str).Value;
        public static DateTime? ToDateTime(this string str )
        {
            return Cnv.CDate(str);
        }

        public static bool ToBool(this string str)
        {
            if (str== "Yes") return true;
            if (str == "No") return false;
            bool.TryParse(str, out var result );
            return result;
        }



        public static string NumberToWords(this int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }

        public static string ToShortUrl(this string link, int maxArgs = 1)
        {

            if ( string.IsNullOrEmpty(link) || !Uri.IsWellFormedUriString(link, UriKind.RelativeOrAbsolute)) return null;

            var uri = new Uri(link);

            var localPath = uri.LocalPath.Replace("//", "/")
                            + (uri.Query.Split('&').Length < maxArgs ? "" : uri.Query); // query only if single variable 

            var url = uri.Scheme + "://" + uri.Host + (localPath.Length > 1 ? localPath : "");

            return url;
        }

        public static string StringJoin(this string[] strArr, string separator) => strArr?.Length > 0 ? string.Join(",", strArr) : null;


        public static string ToBootstrapColor(this Severity severity, bool infoToSuccess = false ) {
            switch (severity) {
                case Severity.Error:
                    return "danger";
                case Severity.Warn:
                    return "warning";
                case Severity.Info :
                    return infoToSuccess ? "success" : "";
                default:
                    return "";

            }

        }

    }
}
