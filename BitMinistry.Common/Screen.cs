using System;
using System.Diagnostics;

namespace BitMinistry.Common
{
    public class Screen
    {

        public static Action<string> Print = (s) => Debug.WriteLine(s);
    }
}
