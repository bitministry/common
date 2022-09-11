using System.Collections.Generic;
using System.Linq;


namespace BitMinistry.Common
{
    public class TaxHelper
    {
        private static Dictionary<int, int> _estIncomeTax;

        public static decimal IncomeTax(int year, decimal amount, string country = "EE")
        {
            if (_estIncomeTax == null)
                InitValues();

            var val = TaxTable( country ).OrderByDescending(x => x.Key).FirstOrDefault(x => x.Key <= year);
            val.ThrowIfNull("Income tax not set for year " + year);

            return amount*((decimal) val.Value/100);
        }


        private static Dictionary<int, int> TaxTable(string country)
        {
            switch (country)
            {
                default:
                    return _estIncomeTax;
            }
        }


        private static void InitValues()
        {
            _estIncomeTax = new Dictionary<int, int>
            {
                {1900, 26},
                {2005, 24},
                {2006, 23},
                {2007, 22},
                {2008, 21},
                {2015, 20}
            };
        }
    }
}
