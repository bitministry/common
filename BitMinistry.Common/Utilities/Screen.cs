using System;
using System.Diagnostics;

namespace BitMinistry.Utility
{
    public class Screen
    {

        public static Action<string> Print = (s) => Debug.WriteLine(s);
    }
}
