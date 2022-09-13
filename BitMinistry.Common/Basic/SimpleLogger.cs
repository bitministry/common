using System;
using System.IO;
using System.Text.RegularExpressions;

namespace BitMinistry
{
    public partial class SimpleLogger
    {


        /// <summary>
        /// append to c:\SimpleLogger\{ typeof(TType) }.csv
        /// </summary> 
        public static void AppendToCsv<TType>(params string[] values ) {

            AppendToFile( message: string.Join("; ", values), filename: $@"c:\SimpleLogger\{ typeof(TType).Name }.csv");
        }

        /// <summary>
        /// append to c:\SimpleLogger\log.csv
        /// </summary> 
        public static void AppendToFile( string message, string filename = null)
        {
            filename = filename  ?? @"c:\SimpleLogger\log.csv" ;

            var fi = new FileInfo( filename );
            if (!fi.Directory.Exists)
                fi.Directory.Create();
            if (! fi.Exists)
                File.AppendAllLines(filename, new string[] { $"sep=;" });

            message = Regex.Replace(message, "\r|\n", String.Empty); 

            object lck = new object();
            lock (lck)
                File.AppendAllLines(filename, new string[] { $"{DateTime.UtcNow};{message}"});
            
        }

    }
}
