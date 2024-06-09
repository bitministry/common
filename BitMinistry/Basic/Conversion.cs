using System;
using System.Globalization;
using System.Text.RegularExpressions;



// namespace on purpose, for historical reasons 
namespace BitMinistry
{
    public class Cnv
    {

        public static char? CChar(object xoin)
        {
            if (CStr(xoin).Length > 0) return xoin.ToString()[0];
            else return null;
        }

        public static string[] RemoveFromArray(ref string[] xas, string xs)
        {
            Array xa = (Array)xas;
            RemoveFromArray(ref xa, xs);
            xas = (string[])xa;
            return xas;
        }

        public static Array RemoveFromArray(ref Array xarr, object xobj)
        {
            Array xarrReturn = Array.CreateInstance(xobj.GetType(), xarr.Length - 1); int xiReturnLooper = 0;
            foreach (object xxo in xarr)
            {
                if (!xxo.Equals(xobj))
                {
                    if (xiReturnLooper == xarr.Length - 1) return xarr;
                    xarrReturn.SetValue(xxo, xiReturnLooper);
                    xiReturnLooper++;
                }
            }
            xarr = xarrReturn;
            return xarr;
        }

        public static Guid? ToGuid(object v)
        {
            Guid g;
            if (Guid.TryParse(CStr(v), out g))
                return g;
            else
                return null;            
        }

        public static bool IsEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsNumeric(object expression )
        {
            decimal retNum;
            return decimal.TryParse(Convert.ToString(expression ), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum);
        }

        public static bool IsInt(string xsin)
        {
            int retNum;
            return int.TryParse(xsin, out retNum);
        }

        public static bool IsGuid(string candidate)
        {
            if (string.IsNullOrEmpty(candidate)) return false;
            Guid guidOutput;
            bool isValid = Guid.TryParse(candidate, out guidOutput);
            return isValid;
        }


        public static int CInt(object expression )
        {
            //var nr = CDec(expression);
            var nr = Math.Max(Math.Min(CDec(expression), int.MaxValue), int.MinValue);
            return (int) Math.Round( nr, 0 ) ;
        }
        public static Int64 CBigInt(object expression)
        {
            //var nr = CDec(expression);
            var nr = Math.Max(Math.Min(CDec(expression), Int64.MaxValue), Int64.MinValue);
            return (Int64)Math.Round(nr, 0);
        }

        public static int? CNInt(object exp)
        {
            if (exp == null || exp.ToString() == "" ) return null;
            return CInt( exp );
        }


        /// <summary>
        /// return format with dot  -123.45
        /// </summary> 
        public static string SweepDecimal( string exp)
        {
            return (exp[0] == '-' ? "-" : "")  // negative / postitive 
                + (Regex.IsMatch(exp, @"\.")
                    ? Regex.Replace(exp, @"[^0-9\.]", "")
                    : (Regex.Matches(exp, @",").Count > 1 
                        ? Regex.Replace(exp, "[^0-9]", "")
                        : Regex.Replace(exp, "[^0-9,]", "").Replace(',', '.') ));
        }

        public static decimal CDec(object expression, string cleanExp = null  )
        {
            decimal retNum;
            var exp = Convert.ToString(expression );
            if (string.IsNullOrEmpty(exp)) return 0;

            cleanExp = cleanExp ?? SweepDecimal( exp );

            return decimal.TryParse(cleanExp, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum) ? retNum : 0;
        }

        public static decimal? CNDec(object expression)
        {
            if (expression == null || expression.ToString() == "") return null;
            var cleanExp = SweepDecimal(Convert.ToString(expression));
            if (cleanExp == "") return null;

            return CDec(expression);
        }

        public static double CDbl(object expression )
        {
            double retNum;
            Double.TryParse(Convert.ToString(expression ), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum);
            return retNum;
        }

        public static double? CNDbl(object expression)
        {
            if (expression == null || expression.ToString() == "") return null;
            var cleanExp = SweepDecimal(Convert.ToString(expression));
            if (cleanExp == "") return null;

            return CDbl(expression);
        }


        public static string CStr(object xIn)
        {
            return xIn?.ToString() ?? "";
        }

        public static string NullIfEmpty(string xs)
        {
            if (CStr(xs).Length == 0)
                return null;
            else
                return xs;
        }
        public static string CNull(object xIn)
        {
            if (xIn == null) return null;
            else
                if (xIn.ToString() == "") return null;
                else return xIn.ToString();
        }
        public static string BasicDateStr(object xoDt)
        { return BasicDateStr(CStr(xoDt)); }
        public static string BasicDateStr(string xsDt)
        {
            if (String.IsNullOrWhiteSpace(xsDt)) return "";
            try { return BasicDateStr(DateTime.Parse(xsDt)); }
            catch { return ""; }
        }
        public static string BasicDateStr(DateTime xdt)
        {
            return xdt.ToString("yyyy-MM-dd");
        }

        public static DateTime? CDate(object xoDt, string format = null )
        {
            return CDate(Cnv.CStr(xoDt), format );
        }

        public static DateTime? CDate(string xsDt, params string[] formats)
        {
            foreach (var format in formats)
            {
                var ddt = CDate(xsDt, format);
                if (ddt != null)
                    return ddt;
            }
            return null;
        }

        public static DateTime? CDate(string xsDt, string format = null)
        {
            if (string.IsNullOrEmpty(xsDt)) return null;
            xsDt = xsDt.Trim();
            try
            {
                return format == null
                    ? DateTime.Parse(xsDt)
                    : DateTime.ParseExact(xsDt, format, CultureInfo.InvariantCulture);
            }
            catch (Exception ex )
            {
                return null;
            }
        }

        public static string EmptyBit(bool xBoolIn)
        {
            if (xBoolIn) return "1";
            else return "";
        }

        public static long IpNum(string xsIP)
        {
            string[] aiIp = xsIP.Split('.');
            if (aiIp.Length != 4) return 0;
            return 16777216 * Convert.ToInt64(aiIp[0])
                + 65536 * Convert.ToInt64(aiIp[1])
                + 256 * Convert.ToInt64(aiIp[2])
                + Convert.ToInt64(aiIp[3]);
        }


        public static bool CBool(object str)
        {
            bool ob = false;
            Boolean.TryParse( CStr( str ), out ob);
            return ob;
        }




    }
}
