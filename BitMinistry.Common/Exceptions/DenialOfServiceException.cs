using System;

namespace BitMinistry.Common.Exceptions
{
    public class DenialOfServiceException : Exception
    {
        public DenialOfServiceException( string msg ) :base ( msg )
        {
        }
    }
}
