using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Drawing;
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



        public static byte[] ResizeImage(this byte[] imageData, int maxWidth, int maxHeight)
        {
            using (MemoryStream originalStream = new MemoryStream(imageData))
            using (Image originalImage = Image.FromStream(originalStream))
            {
                int newWidth;
                int newHeight;

                // Calculate new dimensions to maintain aspect ratio
                double aspectRatio = (double)originalImage.Width / originalImage.Height;
                if (aspectRatio <= 1 && originalImage.Height > maxHeight)
                {
                    newHeight = maxHeight;
                    newWidth = (int)(newHeight * aspectRatio);
                }
                else if (aspectRatio > 1 && originalImage.Width > maxWidth)
                {
                    newWidth = maxWidth;
                    newHeight = (int)(newWidth / aspectRatio);
                }
                else
                {
                    newWidth = originalImage.Width;
                    newHeight = originalImage.Height;
                }

                // Create a new Bitmap with the resized dimensions
                using (Bitmap resizedImage = new Bitmap(newWidth, newHeight))
                {
                    using (Graphics graphics = Graphics.FromImage(resizedImage))
                    {
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                        // Draw the original image onto the resized image
                        graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                    }

                    // Save the resized image to a new byte array
                    using (MemoryStream resizedStream = new MemoryStream())
                    {
                        resizedImage.Save(resizedStream, ImageFormat.Jpeg);
                        return resizedStream.ToArray();
                    }
                }
            }
        }


        public static byte[] ResizeAndCrop(this byte[] imageData, int maxWidth, int maxHeight)
        {
            using (MemoryStream originalStream = new MemoryStream(imageData))
            using (Image originalImage = Image.FromStream(originalStream))
            {
                if (maxWidth >= originalImage.Width && maxHeight >= originalImage.Height) 
                    return imageData;

                if (maxWidth > originalImage.Width) maxWidth = originalImage.Width;
                if (maxHeight > originalImage.Height) maxHeight = originalImage.Height;

                // Step 1: Resize the image to ensure it fits within the bounding box (maxWidth x maxHeight)
                double aspectRatio = (double)originalImage.Width / originalImage.Height;
                int resizeWidth, resizeHeight;

                // Resize while maintaining aspect ratio
                if (originalImage.Width > originalImage.Height) // Landscape or square
                {
                    resizeWidth = maxWidth;
                    resizeHeight = (int)(resizeWidth / aspectRatio);
                    if (resizeHeight < maxHeight)
                    {
                        resizeHeight = maxHeight;
                        resizeWidth = (int)(resizeHeight * aspectRatio);
                    }
                }
                else // Portrait
                {
                    resizeHeight = maxHeight;
                    resizeWidth = (int)(resizeHeight * aspectRatio);
                    if (resizeWidth < maxWidth)
                    {
                        resizeWidth = maxWidth;
                        resizeHeight = (int)(resizeWidth / aspectRatio);
                    }
                }

                // Create a resized image
                using (Bitmap resizedImage = new Bitmap(resizeWidth, resizeHeight))
                {
                    using (Graphics graphics = Graphics.FromImage(resizedImage))
                    {
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                        graphics.DrawImage(originalImage, 0, 0, resizeWidth, resizeHeight);
                    }

                    // Step 2: Center crop the resized image
                    int cropX = (resizeWidth - maxWidth) / 2;
                    int cropY = (resizeHeight - maxHeight) / 2;

                    using (Bitmap finalImage = new Bitmap(maxWidth, maxHeight))
                    {
                        using (Graphics graphics = Graphics.FromImage(finalImage))
                        {
                            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                            // Draw the cropped area from the resized image onto the final image
                            graphics.DrawImage(resizedImage, new Rectangle(0, 0, maxWidth, maxHeight),
                                               new Rectangle(cropX, cropY, maxWidth, maxHeight),
                                               GraphicsUnit.Pixel);
                        }

                        // Step 3: Save the final image to a memory stream and return the byte array
                        using (MemoryStream resizedStream = new MemoryStream())
                        {
                            finalImage.Save(resizedStream, ImageFormat.Jpeg); // Adjust format as needed
                            return resizedStream.ToArray();
                        }
                    }
                }
            }
        }



    }
}
