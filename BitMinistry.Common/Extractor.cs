using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BitMinistry.Common
{
    public class Extractor
    {

        public static string EmailPattern = @"\b[A-Z0-9._-]+@[A-Z0-9][A-Z0-9.-]{0,61}[A-Z0-9]\.[A-Z.]{2,6}\b";

        public static string[] ExtractEmails(string str)
        {

            // Find matches
            var matches= Regex.Matches(str, EmailPattern, RegexOptions.IgnoreCase);

            string[] MatchList = new string[matches.Count];

            // add each match
            int c = 0;
            foreach ( Match match in matches)
            {
                MatchList[c] = match.ToString();
                c++;
            }

            return MatchList;
        }


        public static string DomainPattern = @"\b(?:https?://|www\.)\S+\b";
        public static List<string> ExtractDomains(string text)
        {

            var linkParser = new Regex(DomainPattern, RegexOptions.IgnoreCase);

            var domains = linkParser.Matches(text).Cast<Match>()
                .Select(x => new Uri(httpChk(x.Value)).Host).Distinct().ToList();

            return domains;

            string httpChk(string str)
            {
                if (string.IsNullOrEmpty(str)) return str;

                str = str.StartsWith("http")
                    ? str
                    : ("http://" + str);

                while (str.Contains("///"))
                    str = str.Replace("///", "//");

                return str;
            }

        }

    }



}
