using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;

namespace BitMinistry
{
    public static class Extensions
    {
        public static string Str(this JToken token, string path)
        {
            return token.SelectToken(path).CStr();
        }

        public static int StrLength(this object obj)
        {
            return (obj?.ToString() ?? "").Length;
        }

        public static object GetDefaultValue(this Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }

        public static int Fibonacci( this int i)
        {
            if (i == 0) return 0;
            if (i == 1) return 1;

            return (i - 1).Fibonacci() + (i - 2).Fibonacci();
        }
        public static string String(this DateTime? date, string format)
        {
            return date?.ToString(format);
        }

        public static int ToEpoch(this DateTime dateTime) => ToUnixTimestamp( dateTime );

        public static int ToUnixTimestamp(this DateTime dateTime)
        {
            return (int)
                ((dateTime.Ticks - 621355968000000000) / 10000000 );

            //return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
            //        new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }


        public static DateTime ToDateTimeFromMs(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static int GetQuarter(this DateTime date)
        {
            if (date.Month >= 4 && date.Month <= 6)
                return 1;
            else if (date.Month >= 7 && date.Month <= 9)
                return 2;
            else if (date.Month >= 10 && date.Month <= 12)
                return 3;
            else
                return 4;
        }

        public static DateTime ToUtc(this int epoch)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dtDateTime.AddSeconds(epoch);
        }

        public static DateTime ToLocalTime(this int epoch)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(epoch).ToLocalTime();
            return dtDateTime;
        }

        



        public static string ToBasicDateStr(this DateTime? date)
        {
            return date .HasValue ? Cnv.BasicDateStr(date.Value) : "";
        }
        public static string ToBasicDateStr(this DateTime value)
        {
            return Cnv.BasicDateStr(value);
        }

        public static DateTime AddBusinessDays(this DateTime current, int days)
        {
            var sign = Math.Sign(days);
            var unsignedDays = Math.Abs(days);
            for (var i = 0; i < unsignedDays; i++)
            {
                do
                {
                    current = current.AddDays(sign);
                }
                while (current.DayOfWeek == DayOfWeek.Saturday ||
                    current.DayOfWeek == DayOfWeek.Sunday);
            }
            return current;
        }



        public static string ToCurrency(this decimal? sum)
        {
            return sum?.ToString("C");
        }

        public static IEnumerable<Enum> GetFlags(this Enum input)
        {
            return Enum.GetValues(input.GetType()).Cast<Enum>().Where(input.HasFlag);
        }

        public static int NextAround(this Random rnd, int smthn )
        {
            return (rnd.Next(  smthn / 2, smthn + smthn / 2));
        }


        public static IEnumerable<T> DequeueChunk<T>(this Queue<T> queue, int chunkSize)
        {
            for (int i = 0; i < chunkSize && queue.Count > 0; i++)
                yield return queue.Dequeue();
        }


        public static void RemoveAll<TSource>(this ICollection<TSource> list, Func<TSource, bool> predicate)
        {
            foreach (var x in list.Where(predicate).ToArray())
                list.Remove(x);

        }


        public static Dictionary<string, string> ToDictionary(this NameValueCollection nvc)
        {
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }

        public static string ToImgDataUrl(this byte[] binimg, string ext = "jpeg") => binimg?.Length > 0 ? $"data:image/{ ext };base64,{ Convert.ToBase64String(binimg) }" : "";

        public static byte[] ToArray(this Stream str) {
            using (var memoryStream = new MemoryStream())
            {
                str.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

    }
}
