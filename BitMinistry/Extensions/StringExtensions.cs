﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
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

        public static Guid StringToGuid(this string id)
        {
            Guid guidId;
            if (!IsNullOrEmpty(id) && Guid.TryParse(id, out guidId))
            {
                return guidId;
            }
            return Guid.Empty;
        }

        public static int? SwipeToNInt(this string exp)
        {
            return exp == null ? null : Cnv.CNInt(Regex.Replace(exp, @"[^-+\d]", ""));
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

        public static string NewLineToBR(this string str )
        {
            return (str ?? "").Replace( Environment.NewLine, "<BR>");
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

        private static readonly Regex _isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

        public static bool IsGuid(this string candidate )
        {
            return candidate != null && _isGuid.IsMatch(candidate);
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

   

    }
}