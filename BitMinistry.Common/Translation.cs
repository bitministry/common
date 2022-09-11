using System;
using System.Collections;
using System.Globalization;
using System.Resources;

namespace BitMinistry.Common
{
    public class Translation
    {
        public static ResourceManager Dictionary;
        public static string Translate(string term, CultureInfo cult = null)
        {
            var translation = cult == null ? Dictionary.GetString(term) : Dictionary.GetString(term, cult);
            return string.IsNullOrEmpty(translation) ? term : translation;
        }

        readonly ResourceManager _dictionary;

        public Translation(Type dictionary)
        {
            _dictionary = new ResourceManager(dictionary);
        }

        public string Trans(string term, CultureInfo cult = null )
        {
            var translation = cult == null ? _dictionary.GetString(term) : _dictionary.GetString(term, cult) ;
            return string.IsNullOrEmpty(translation) ? term : translation;
        }

        public static Hashtable Hashtable(Type resources, string cultureInfo = null)
        {
            var cult = cultureInfo != null ? new CultureInfo(cultureInfo) : CultureInfo.CurrentUICulture;

            return Hashtable(resources, cult);
        }

        public static Hashtable Hashtable(Type resources, CultureInfo cult )
        {

            var resourceSet = new ResourceManager(resources).GetResourceSet(cult, true, true);

            var ht = new Hashtable();

            foreach (DictionaryEntry entry in resourceSet)
                ht[entry.Key.ToString()] = entry.Value;

            return ht;
        }
    }
}
