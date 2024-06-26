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
        /// <param name="values">"{text}" for the CSV cells; packed into quotations</param>
        public static void AppendToCsv<TType>(params string[] values ) {

            for (int i = 0; i < values.Length; i++)
                values[i] = $"\"{ values[i].Replace("\"", "\"\"") }\"";

            AppendToFile( message: string.Join("; ", values), filename: $@"c:\SimpleLogger\{ typeof(TType).Name }.csv" );
        }

        /// <summary>
        /// Append to textfile according to filename extension: CSV, or plaintext. 
        /// </summary> 
        /// <param name="filename">default is 'log.csv'; c:\SimpleLogger\ is prepended to the names without paths</param>
        public static void AppendToFile( string message, string filename = null , bool rethrow = true )
        {
            filename = filename  ?? @"c:\SimpleLogger\log.csv" ;

            if (!Regex.IsMatch(filename, @"\\|\/"))
                filename = @"c:\SimpleLogger\" + filename;

            try
            {
                var fi = new FileInfo( filename );
                if (!fi.Directory.Exists)
                    fi.Directory.Create();

                object lck = new object();
                lock (lck)
                    if (filename.EndsWith(".csv"))
                    {
                        if (!fi.Exists)
                            File.AppendAllLines(filename, new[] { $"sep =;" }); // start CSV

                        File.AppendAllLines(filename, new[] { $"\"{ DateTime.UtcNow }\";{ message }" }); // single line 
                    }
                    else
                        File.AppendAllLines(filename, new[] { $"{ DateTime.UtcNow }", "", message, "", }); // multiple lines 
            }
            catch (Exception ex)
            {
                if (rethrow)
                    throw new Exception( $"{filename}: {ex.Message}");

            }
        }

    }
}
