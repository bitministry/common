using System;

namespace BitMinistry.Exceptions
{
    public class DenialOfServiceException : Exception
    {
        public DenialOfServiceException( string msg ) :base ( msg )
        {
        }
    }
}
